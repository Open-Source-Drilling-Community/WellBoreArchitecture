using System;
using System.IO;
using Microsoft.Extensions.Logging;
using System.Data;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Data.Sqlite;

namespace OSDC.Drilling.WellBoreArchitecture.Service.Managers
{
    /// <summary>
    /// A manager for the sql database connection, registered as a singleton through dependency injection (see Program.cs)
    /// Existing databases are validated before use. Unknown or malformed schemas abort startup without changing user data.
    /// </summary>
    /// <remarks>
    /// SQLite database connection strategy:
    /// - single connection for every access (chosen strategy in the general case)
    ///     each access to the database is performed through isolated connections stored in a List of connections
    ///     > isolation, reliability, fail-safe, thread-safe, but overhead due to opening connections
    /// - shared connection between access
    ///     one connection is opened for the lifetime of the application and used to access database through various web requests and commands 
    ///     > no overhead, but issues with concurrency, single-point of failure, state management
    /// - scoped connection (registering service with AddScoped rather than AddSingleton)
    ///     one connection is opened per web request
    ///     > same problems as with shared connection, but limited to the scope of one webrequest rather than to the whole lifetime of the application
    /// </remarks>
    public class SqlConnectionManager
    {
        private readonly ILogger<SqlConnectionManager> _logger;
        private readonly string _connectionString;
        public static readonly string HOME_DIRECTORY = ".." + Path.DirectorySeparatorChar + "home" + Path.DirectorySeparatorChar;
        public static readonly string DATABASE_FILENAME = "WellBoreArchitecture.db";
        public static readonly string DATE_TIME_FORMAT = "yyyy-MM-dd HH:mm:ss";
        public const int CURRENT_SCHEMA_VERSION = 2;

        // dictionary describing tables format
        // Light weight data fields are enumerated explicitly in the data table implementing the light weight data concept
        // (thus duplicating info in the database) for 2 reasons
        // 1) to avoid loading the complete WellBoreArchitecture (heavy weight data) each time we only need contextual info on the data (light weight data)
        // 2) to keep control of the logic of inserting and selecting a light data in the database
        //    localized at the controller/manager level (storing WellBoreArchitectureLight as a whole could induce database corruption issues)
        // If the light weight data concept is not implemented, the same contextual info can be retrieved directly from the WellBoreArchitecture
        private readonly static Dictionary<string, string[]> _tableStructureDict = new Dictionary<string, string[]>()
            {                
                { "WellBoreArchitectureTable", new string[] {
                    "ID text primary key",
                    "MetaInfo text",
                    // beginning of list of fields used only when light weight concept is implemented
                    "Name text",
                    "Description text",
                    // end of list of fields used only when light weight concept is implemented
                    "CreationDate text",
                    "LastModificationDate text",
                    "WellBoreArchitecture text" }
                },
                { "WellBoreArchitectureIdentityTable", new string[] {
                    "ID text primary key", "MetaInfo text", "Name text", "CreationDate text",
                    "LastModificationDate text", "WellBoreArchitectureIdentity text" }
                },
                { "WellBoreArchitectureFeatureCategoryTable", new string[] {
                    "ID text primary key", "MetaInfo text", "Name text", "IsExclusive integer",
                    "HasValidityPeriod integer", "CreationDate text", "LastModificationDate text",
                    "WellBoreArchitectureFeatureCategory text" }
                }
            };

        public SqlConnectionManager(string connectionString, ILogger<SqlConnectionManager> logger)
        {
            _connectionString = connectionString;
            _logger = logger;
            _logger.LogInformation("SqliteConnectionManager created");
            if (Initialize())
            {
                ManageDataBase();
            }
            else
            {
                _logger.LogInformation("SqliteConnectionManager created");
            }
        }

        public SqliteConnection? GetConnection()
        {
            // a new SQL connection is opened for every transaction, thus ensuring thread-safety and removing unnecessary locks
            var connection = new SqliteConnection(_connectionString);
            if (connection != null)
            {
                connection.Open();
            }
            else
            {
                _logger.LogError("Problem while opening SQLite connection");
            }
            return connection;
        }

        private bool Initialize()
        {
            if (!Directory.Exists(HOME_DIRECTORY))
            {
                _logger.LogInformation("Creating home directory");
                try
                {
                    Directory.CreateDirectory(HOME_DIRECTORY);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Impossible to create home directory for local storage");
                    return false;
                }
            }
            if (Directory.Exists(HOME_DIRECTORY))
            {
                try
                {
                    string databaseFileName = HOME_DIRECTORY + Path.DirectorySeparatorChar + DATABASE_FILENAME;
                    if (File.Exists(databaseFileName))
                    {
                        _logger.LogInformation("Opening database {_databaseFileName}", DATABASE_FILENAME);
                    }
                    else
                    {
                        _logger.LogInformation("Creating database {_databaseFileName}", DATABASE_FILENAME);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Impossible to create {_databaseFileName}", DATABASE_FILENAME);
                    return false;
                }
            }
            else
            {
                _logger.LogError("Home directory for local storage should have been created, check for access");
                return false;
            }
            return true;
        }

        /// <summary>
        /// Creates an empty database transactionally or adopts the unchanged legacy schema by setting its version marker.
        /// Existing tables and rows are never dropped or rebuilt automatically.
        /// </summary>
        private void ManageDataBase()
        {
            using SqliteConnection connection = GetConnection()
                ?? throw new InvalidOperationException("Unable to open the WellBoreArchitecture database.");

            List<string> tableNames = [];
            using (SqliteCommand tables = connection.CreateCommand())
            {
                tables.CommandText = "SELECT name FROM sqlite_master WHERE type='table' AND name NOT LIKE 'sqlite_%'";
                using SqliteDataReader reader = tables.ExecuteReader();
                while (reader.Read()) tableNames.Add(reader.GetString(0));
            }

            using SqliteCommand versionCommand = connection.CreateCommand();
            versionCommand.CommandText = "PRAGMA user_version";
            int schemaVersion = Convert.ToInt32(versionCommand.ExecuteScalar());
            if (schemaVersion > CURRENT_SCHEMA_VERSION)
                throw new InvalidOperationException($"WellBoreArchitecture database schema version {schemaVersion} is newer than supported version {CURRENT_SCHEMA_VERSION}.");

            if (tableNames.Count == 0)
            {
                if (schemaVersion != 0)
                    throw new InvalidOperationException("The versioned WellBoreArchitecture database has no tables. No data was changed.");
                using SqliteTransaction transaction = connection.BeginTransaction();
                try
                {
                    foreach (KeyValuePair<string, string[]> table in _tableStructureDict)
                    {
                        using SqliteCommand create = connection.CreateCommand();
                        create.Transaction = transaction;
                        create.CommandText = $"CREATE TABLE {table.Key} ({string.Join(',', table.Value)})";
                        create.ExecuteNonQuery();
                        using SqliteCommand index = connection.CreateCommand();
                        index.Transaction = transaction;
                        index.CommandText = $"CREATE UNIQUE INDEX {table.Key}Index ON {table.Key} (ID)";
                        index.ExecuteNonQuery();
                    }
                    SetSchemaVersion(connection, transaction);
                    transaction.Commit();
                }
                catch
                {
                    transaction.Rollback();
                    throw;
                }
                return;
            }

            IEnumerable<string> permittedTables = _tableStructureDict.Keys;
            List<string> unexpected = tableNames.Except(permittedTables, StringComparer.Ordinal).ToList();
            if (unexpected.Count > 0)
                throw new InvalidOperationException($"Unexpected WellBoreArchitecture database tables. No data was changed: [{string.Join(',', unexpected)}].");
            if (!tableNames.Contains("WellBoreArchitectureTable", StringComparer.Ordinal) ||
                !CheckDatabaseStructure(connection, new("WellBoreArchitectureTable", _tableStructureDict["WellBoreArchitectureTable"])))
                throw new InvalidOperationException("The existing WellBoreArchitectureTable is missing or malformed. No data was changed.");
            List<string> malformedExistingCatalogs = _tableStructureDict
                .Where(table => table.Key != "WellBoreArchitectureTable" && tableNames.Contains(table.Key, StringComparer.Ordinal) && !CheckDatabaseStructure(connection, table))
                .Select(table => table.Key).ToList();
            if (malformedExistingCatalogs.Count > 0)
                throw new InvalidOperationException($"Existing WellBoreArchitecture catalog tables are malformed. No data was changed: [{string.Join(',', malformedExistingCatalogs)}].");

            if (schemaVersion < CURRENT_SCHEMA_VERSION)
            {
                using SqliteTransaction transaction = connection.BeginTransaction();
                try
                {
                    foreach (KeyValuePair<string, string[]> table in _tableStructureDict)
                    {
                        if (!tableNames.Contains(table.Key, StringComparer.Ordinal))
                        {
                            using SqliteCommand create = connection.CreateCommand();
                            create.Transaction = transaction;
                            create.CommandText = $"CREATE TABLE {table.Key} ({string.Join(',', table.Value)})";
                            create.ExecuteNonQuery();
                        }
                        using SqliteCommand index = connection.CreateCommand();
                        index.Transaction = transaction;
                        index.CommandText = $"CREATE UNIQUE INDEX IF NOT EXISTS {table.Key}Index ON {table.Key} (ID)";
                        index.ExecuteNonQuery();
                    }
                    SetSchemaVersion(connection, transaction);
                    transaction.Commit();
                }
                catch
                {
                    transaction.Rollback();
                    throw;
                }
                tableNames = _tableStructureDict.Keys.ToList();
            }

            ValidateExpectedSchema(connection, tableNames);
        }

        private static void SetSchemaVersion(SqliteConnection connection, SqliteTransaction transaction)
        {
            using SqliteCommand command = connection.CreateCommand();
            command.Transaction = transaction;
            command.CommandText = $"PRAGMA user_version = {CURRENT_SCHEMA_VERSION}";
            command.ExecuteNonQuery();
        }

        private static void ValidateExpectedSchema(SqliteConnection connection, IReadOnlyCollection<string> tableNames)
        {
            List<string> unexpected = tableNames.Except(_tableStructureDict.Keys, StringComparer.Ordinal).ToList();
            List<string> missing = _tableStructureDict.Keys.Except(tableNames, StringComparer.Ordinal).ToList();
            List<string> malformed = _tableStructureDict
                .Where(table => tableNames.Contains(table.Key) && !CheckDatabaseStructure(connection, table))
                .Select(table => table.Key).ToList();
            if (unexpected.Count > 0 || missing.Count > 0 || malformed.Count > 0)
                throw new InvalidOperationException($"Unexpected WellBoreArchitecture database structure. No data was changed. Missing=[{string.Join(',', missing)}], unexpected=[{string.Join(',', unexpected)}], malformed=[{string.Join(',', malformed)}].");
        }

        /// <summary>
        /// Check that expected fields (in tableStructure.Value) exactly match those of the stored database
        /// </summary>
        /// <param name="tableStructure"></param>
        /// <returns>true if the expected fields exactly match fields of the stored database</returns>
        private static bool CheckDatabaseStructure(SqliteConnection connection, KeyValuePair<string, string[]> tableStructure)
        {
            using SqliteCommand command = connection.CreateCommand();
            command.CommandText = $"SELECT * FROM {tableStructure.Key}";
            using SqliteDataReader reader = command.ExecuteReader(CommandBehavior.SchemaOnly);
            DataTable? schema = reader.GetSchemaTable();
            if (schema == null || tableStructure.Value.Length != schema.Rows.Count) return false;
            foreach (string field in tableStructure.Value)
            {
                string expectedName = field.Split(' ')[0];
                if (!schema.Rows.Cast<DataRow>().Any(column => column.Field<string>("ColumnName") == expectedName))
                    return false;
            }
            return true;
        }

    }
}
