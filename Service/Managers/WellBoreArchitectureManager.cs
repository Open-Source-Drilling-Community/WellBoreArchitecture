using System;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using OSDC.DotnetLibraries.General.DataManagement;
using Microsoft.Data.Sqlite;
using System.Text.Json;
using System.Linq;
using OSDC.Drilling.WellBoreArchitecture.Model;

namespace OSDC.Drilling.WellBoreArchitecture.Service.Managers
{
    /// <summary>
    /// A manager for WellBoreArchitecture. The manager implements the singleton pattern as defined by 
    /// Gamma, Erich, et al. "Design patterns: Abstraction and reuse of object-oriented design." 
    /// European Conference on Object-Oriented Programming. Springer, Berlin, Heidelberg, 1993.
    /// </summary>
    public class WellBoreArchitectureManager
    {
        private static WellBoreArchitectureManager? _instance = null;
        private readonly ILogger<WellBoreArchitectureManager> _logger;
        private readonly SqlConnectionManager _connectionManager;

        internal WellBoreArchitectureManager(ILogger<WellBoreArchitectureManager> logger, SqlConnectionManager connectionManager)
        {
            _logger = logger;
            _connectionManager = connectionManager;
        }

        public static WellBoreArchitectureManager GetInstance(ILogger<WellBoreArchitectureManager> logger, SqlConnectionManager connectionManager)
        {
            _instance ??= new WellBoreArchitectureManager(logger, connectionManager);
            return _instance;
        }

        public int Count
        {
            get
            {
                int count = 0;
                using var connection = _connectionManager.GetConnection();
                if (connection != null)
                {
                    var command = connection.CreateCommand();
                    command.CommandText = "SELECT COUNT(*) FROM WellBoreArchitectureTable";
                    try
                    {
                        using SqliteDataReader reader = command.ExecuteReader();
                        if (reader.Read())
                        {
                            count = (int)reader.GetInt64(0);
                        }
                    }
                    catch (SqliteException ex)
                    {
                        _logger.LogError(ex, "Impossible to count records in the WellBoreArchitectureTable");
                    }
                }
                else
                {
                    _logger.LogWarning("Impossible to access the SQLite database");
                }
                return count;
            }
        }

        public bool Clear()
        {
            using var connection = _connectionManager.GetConnection();
            if (connection != null)
            {
                bool success = false;
                using var transaction = connection.BeginTransaction();
                try
                {
                    //empty WellBoreArchitectureTable
                    var command = connection.CreateCommand();
                    command.CommandText = "DELETE FROM WellBoreArchitectureTable";
                    command.ExecuteNonQuery();

                    transaction.Commit();
                    success = true;
                }
                catch (SqliteException ex)
                {
                    transaction.Rollback();
                    _logger.LogError(ex, "Impossible to clear the WellBoreArchitectureTable");
                }
                return success;
            }
            else
            {
                _logger.LogWarning("Impossible to access the SQLite database");
                return false;
            }
        }

        public bool Contains(Guid guid)
        {
            int count = 0;
            using var connection = _connectionManager.GetConnection();
            if (connection != null)
            {
                var command = connection.CreateCommand();
                command.CommandText = "SELECT COUNT(*) FROM WellBoreArchitectureTable " +
                    "WHERE ID = $idText OR ID = $idValue";
                command.Parameters.AddWithValue("$idText", guid.ToString());
                command.Parameters.AddWithValue("$idValue", guid);
                try
                {
                    using SqliteDataReader reader = command.ExecuteReader();
                    if (reader.Read())
                    {
                        count = (int)reader.GetInt64(0);
                    }
                }
                catch (SqliteException ex)
                {
                    _logger.LogError(ex, "Impossible to count rows from WellBoreArchitectureTable");
                }
            }
            else
            {
                _logger.LogWarning("Impossible to access the SQLite database");
            }
            return count >= 1;
        }

        /// <summary>
        /// Returns the list of Guid of all WellBoreArchitecture present in the microservice database 
        /// </summary>
        /// <returns>the list of Guid of all WellBoreArchitecture present in the microservice database</returns>
        public List<Guid>? GetAllWellBoreArchitectureId()
        {
            List<Guid> ids = [];
            using var connection = _connectionManager.GetConnection();
            if (connection != null)
            {
                var command = connection.CreateCommand();
                command.CommandText = "SELECT ID FROM WellBoreArchitectureTable";
                try
                {
                    using var reader = command.ExecuteReader();
                    while (reader.Read() && !reader.IsDBNull(0))
                    {
                        Guid id = reader.GetGuid(0);
                        ids.Add(id);
                    }
                    _logger.LogInformation("Returning the list of ID of existing records from WellBoreArchitectureTable");
                    return ids;
                }
                catch (SqliteException ex)
                {
                    _logger.LogError(ex, "Impossible to get IDs from WellBoreArchitectureTable");
                }
            }
            else
            {
                _logger.LogWarning("Impossible to access the SQLite database");
            }
            return null;
        }

        /// <summary>
        /// Returns the list of MetaInfo of all WellBoreArchitecture present in the microservice database 
        /// </summary>
        /// <returns>the list of MetaInfo of all WellBoreArchitecture present in the microservice database</returns>
        public List<MetaInfo?>? GetAllWellBoreArchitectureMetaInfo()
        {
            List<MetaInfo?> metaInfos = new();
            using var connection = _connectionManager.GetConnection();
            if (connection != null)
            {
                var command = connection.CreateCommand();
                command.CommandText = "SELECT MetaInfo FROM WellBoreArchitectureTable";
                try
                {
                    using var reader = command.ExecuteReader();
                    while (reader.Read() && !reader.IsDBNull(0))
                    {
                        string mInfo = reader.GetString(0);
                        MetaInfo? metaInfo = JsonSerializer.Deserialize<MetaInfo>(mInfo, JsonSettings.Options);
                        metaInfos.Add(metaInfo);
                    }
                    _logger.LogInformation("Returning the list of MetaInfo of existing records from WellBoreArchitectureTable");
                    return metaInfos;
                }
                catch (SqliteException ex)
                {
                    _logger.LogError(ex, "Impossible to get IDs from WellBoreArchitectureTable");
                }
            }
            else
            {
                _logger.LogWarning("Impossible to access the SQLite database");
            }
            return null;
        }

        /// <summary>
        /// Returns the WellBoreArchitecture identified by its Guid from the microservice database 
        /// </summary>
        /// <param name="guid"></param>
        /// <returns>the WellBoreArchitecture identified by its Guid from the microservice database</returns>
        public Model.WellBoreArchitecture? GetWellBoreArchitectureById(Guid guid)
        {
            if (!guid.Equals(Guid.Empty))
            {
                using var connection = _connectionManager.GetConnection();
                if (connection != null)
                {
                    Model.WellBoreArchitecture? wellBoreArchitecture;
                    var command = connection.CreateCommand();
                    command.CommandText = "SELECT WellBoreArchitecture FROM WellBoreArchitectureTable " +
                        "WHERE ID = $idText OR ID = $idValue";
                    command.Parameters.AddWithValue("$idText", guid.ToString());
                    command.Parameters.AddWithValue("$idValue", guid);
                    try
                    {
                        using var reader = command.ExecuteReader();
                        if (reader.Read() && !reader.IsDBNull(0))
                        {
                            string data = reader.GetString(0);
                            wellBoreArchitecture = JsonSerializer.Deserialize<Model.WellBoreArchitecture>(data, JsonSettings.Options);
                            if (wellBoreArchitecture != null && wellBoreArchitecture.MetaInfo != null && !wellBoreArchitecture.MetaInfo.ID.Equals(guid))
                                throw new SqliteException("SQLite database corrupted: returned WellBoreArchitecture is null or has been jsonified with the wrong ID.", 1);
                            if (wellBoreArchitecture != null && !WellBoreArchitectureComponentIdentity.Ensure(wellBoreArchitecture))
                                throw new SqliteException("SQLite database corrupted: nested component IDs are duplicated.", 1);
                            if (wellBoreArchitecture != null)
                                EnsureReadableRevision(wellBoreArchitecture);
                        }
                        else
                        {
                            _logger.LogInformation("No WellBoreArchitecture of given ID in the database");
                            return null;
                        }
                    }
                    catch (SqliteException ex)
                    {
                        _logger.LogError(ex, "Impossible to get the WellBoreArchitecture with the given ID from WellBoreArchitectureTable");
                        return null;
                    }
                    _logger.LogInformation("Returning the WellBoreArchitecture of given ID from WellBoreArchitectureTable");
                    return wellBoreArchitecture;
                }
                else
                {
                    _logger.LogWarning("Impossible to access the SQLite database");
                }
            }
            else
            {
                _logger.LogWarning("The given WellBoreArchitecture ID is null or empty");
            }
            return null;
        }

        /// <summary>
        /// Returns the list of all WellBoreArchitecture present in the microservice database 
        /// </summary>
        /// <returns>the list of all WellBoreArchitecture present in the microservice database</returns>
        public List<Model.WellBoreArchitecture?>? GetAllWellBoreArchitecture()
        {
            List<Model.WellBoreArchitecture?> vals = [];
            using var connection = _connectionManager.GetConnection();
            if (connection != null)
            {
                var command = connection.CreateCommand();
                command.CommandText = "SELECT WellBoreArchitecture FROM WellBoreArchitectureTable";
                try
                {
                    using var reader = command.ExecuteReader();
                    while (reader.Read() && !reader.IsDBNull(0))
                    {
                        string data = reader.GetString(0);
                        Model.WellBoreArchitecture? wellBoreArchitecture = JsonSerializer.Deserialize<Model.WellBoreArchitecture>(data, JsonSettings.Options);
                        if (wellBoreArchitecture != null && !WellBoreArchitectureComponentIdentity.Ensure(wellBoreArchitecture))
                            throw new SqliteException("SQLite database corrupted: nested component IDs are duplicated.", 1);
                        if (wellBoreArchitecture != null)
                            EnsureReadableRevision(wellBoreArchitecture);
                        vals.Add(wellBoreArchitecture);
                    }
                    _logger.LogInformation("Returning the list of existing WellBoreArchitecture from WellBoreArchitectureTable");
                    return vals;
                }
                catch (SqliteException ex)
                {
                    _logger.LogError(ex, "Impossible to get WellBoreArchitecture from WellBoreArchitectureTable");
                }
            }
            else
            {
                _logger.LogWarning("Impossible to access the SQLite database");
            }
            return null;
        }

        /// <summary>
        /// Returns the list of all WellBoreArchitectureLight present in the microservice database 
        /// </summary>
        /// <param name="guid"></param>
        /// <returns>the list of WellBoreArchitectureLight present in the microservice database</returns>
        public List<Model.WellBoreArchitectureLight>? GetAllWellBoreArchitectureLight()
        {
            List<Model.WellBoreArchitectureLight>? wellBoreArchitectureLightList = [];
            using var connection = _connectionManager.GetConnection();
            if (connection != null)
            {
                var command = connection.CreateCommand();
                command.CommandText = "SELECT MetaInfo, Name, Description, CreationDate, LastModificationDate FROM WellBoreArchitectureTable";
                try
                {
                    using var reader = command.ExecuteReader();
                    while (reader.Read() && !reader.IsDBNull(0))
                    {
                        string metaInfoStr = reader.GetString(0);
                        MetaInfo? metaInfo = JsonSerializer.Deserialize<MetaInfo>(metaInfoStr, JsonSettings.Options);
                        string name = reader.GetString(1);
                        string descr = reader.GetString(2);
                        // make sure DateTimeOffset are properly instantiated when stored values are null (and parsed as empty string)
                        DateTimeOffset? creationDate = null;
                        if (DateTimeOffset.TryParse(reader.GetString(3), out DateTimeOffset cDate))
                            creationDate = cDate;
                        DateTimeOffset? lastModificationDate = null;
                        if (DateTimeOffset.TryParse(reader.GetString(4), out DateTimeOffset lDate))
                            lastModificationDate = lDate;
                        lastModificationDate ??= creationDate ?? DateTimeOffset.UnixEpoch;
                        wellBoreArchitectureLightList.Add(new Model.WellBoreArchitectureLight(
                                metaInfo,
                                string.IsNullOrEmpty(name) ? null : name,
                                string.IsNullOrEmpty(descr) ? null : descr,
                                creationDate,
                                lastModificationDate));
                    }
                    _logger.LogInformation("Returning the list of existing WellBoreArchitectureLight from WellBoreArchitectureTable");
                    return wellBoreArchitectureLightList;
                }
                catch (SqliteException ex)
                {
                    _logger.LogError(ex, "Impossible to get light datas from WellBoreArchitectureTable");
                }
            }
            else
            {
                _logger.LogWarning("Impossible to access the SQLite database");
            }
            return null;
        }

        /// <summary>
        /// Performs calculation on the given WellBoreArchitecture and adds it to the microservice database
        /// </summary>
        /// <param name="wellBoreArchitecture"></param>
        /// <returns>true if the given WellBoreArchitecture has been added successfully to the microservice database</returns>
        public bool AddWellBoreArchitecture(Model.WellBoreArchitecture? wellBoreArchitecture)
        {
            if (wellBoreArchitecture != null && !WellBoreArchitectureComponentIdentity.Ensure(wellBoreArchitecture))
            {
                _logger.LogWarning("WellBoreArchitecture contains empty or duplicate nested component IDs");
                return false;
            }
            if (wellBoreArchitecture != null && !ValidateAssignments(wellBoreArchitecture))
            {
                _logger.LogWarning("WellBoreArchitecture contains invalid identity or feature assignments");
                return false;
            }
            if (wellBoreArchitecture != null && wellBoreArchitecture.MetaInfo != null && wellBoreArchitecture.MetaInfo.ID != Guid.Empty)
            {
                //calculate outputs
                if (!wellBoreArchitecture.Calculate())
                {
                    _logger.LogWarning("Impossible to calculate outputs for the given WellBoreArchitecture");
                    return false;
                }

                //if successful, check if another parent data with the same ID was calculated/added during the calculation time
                Model.WellBoreArchitecture? newWellBoreArchitecture = GetWellBoreArchitectureById(wellBoreArchitecture.MetaInfo.ID);
                if (newWellBoreArchitecture == null)
                {
                    //update WellBoreArchitectureTable
                    using var connection = _connectionManager.GetConnection();
                    if (connection != null)
                    {
                        using SqliteTransaction transaction = connection.BeginTransaction();
                        bool success = true;
                        try
                        {
                            //add the WellBoreArchitecture to the WellBoreArchitectureTable
                            DateTimeOffset now = DateTimeOffset.UtcNow;
                            wellBoreArchitecture.CreationDate = now;
                            wellBoreArchitecture.LastModificationDate = now;
                            string metaInfo = JsonSerializer.Serialize(wellBoreArchitecture.MetaInfo, JsonSettings.Options);
                            string cDate = now.ToString(SqlConnectionManager.DATE_TIME_FORMAT);
                            string lDate = cDate;
                            string data = JsonSerializer.Serialize(wellBoreArchitecture, JsonSettings.Options);
                            var command = connection.CreateCommand();
                            command.CommandText = "INSERT INTO WellBoreArchitectureTable " +
                                "(ID, MetaInfo, Name, Description, CreationDate, LastModificationDate, WellBoreArchitecture) " +
                                "VALUES ($id, $metaInfo, $name, $description, $creationDate, $lastModificationDate, $document)";
                            command.Parameters.AddWithValue("$id", wellBoreArchitecture.MetaInfo.ID.ToString());
                            command.Parameters.AddWithValue("$metaInfo", metaInfo);
                            command.Parameters.AddWithValue("$name", wellBoreArchitecture.Name ?? string.Empty);
                            command.Parameters.AddWithValue("$description", wellBoreArchitecture.Description ?? string.Empty);
                            command.Parameters.AddWithValue("$creationDate", cDate);
                            command.Parameters.AddWithValue("$lastModificationDate", lDate);
                            command.Parameters.AddWithValue("$document", data);
                            int count = command.ExecuteNonQuery();
                            if (count != 1)
                            {
                                _logger.LogWarning("Impossible to insert the given WellBoreArchitecture into the WellBoreArchitectureTable");
                                success = false;
                            }
                        }
                        catch (SqliteException ex)
                        {
                            _logger.LogError(ex, "Impossible to add the given WellBoreArchitecture into WellBoreArchitectureTable");
                            success = false;
                        }
                        //finalizing SQL transaction
                        if (success)
                        {
                            transaction.Commit();
                            _logger.LogInformation("Added the given WellBoreArchitecture of given ID into the WellBoreArchitectureTable successfully");
                        }
                        else
                        {
                            transaction.Rollback();
                        }
                        return success;
                    }
                    else
                    {
                        _logger.LogWarning("Impossible to access the SQLite database");
                    }
                }
                else
                {
                    _logger.LogWarning("Impossible to post WellBoreArchitecture. ID already found in database.");
                    return false;
                }

            }
            else
            {
                _logger.LogWarning("The WellBoreArchitecture ID or the ID of its input are null or empty");
            }
            return false;
        }

        /// <summary>
        /// Performs calculation on the given WellBoreArchitecture and updates it in the microservice database
        /// </summary>
        /// <param name="wellBoreArchitecture"></param>
        /// <returns>true if the given WellBoreArchitecture has been updated successfully</returns>
        public bool UpdateWellBoreArchitectureById(Guid guid, Model.WellBoreArchitecture? wellBoreArchitecture)
        {
            if (wellBoreArchitecture != null && !WellBoreArchitectureComponentIdentity.Ensure(wellBoreArchitecture))
            {
                _logger.LogWarning("WellBoreArchitecture contains empty or duplicate nested component IDs");
                return false;
            }
            if (wellBoreArchitecture != null && !ValidateAssignments(wellBoreArchitecture))
            {
                _logger.LogWarning("WellBoreArchitecture contains invalid identity or feature assignments");
                return false;
            }
            bool success = true;
            if (guid != Guid.Empty && wellBoreArchitecture != null && wellBoreArchitecture.MetaInfo != null && wellBoreArchitecture.MetaInfo.ID == guid)
            {
                //calculate outputs
                if (!wellBoreArchitecture.Calculate())
                {
                    _logger.LogWarning("Impossible to calculate outputs of the given WellBoreArchitecture");
                    return false;
                }
                //update WellBoreArchitectureTable
                using var connection = _connectionManager.GetConnection();
                if (connection != null)
                {
                    using SqliteTransaction transaction = connection.BeginTransaction();
                    //update fields in WellBoreArchitectureTable
                    try
                    {
                        string metaInfo = JsonSerializer.Serialize(wellBoreArchitecture.MetaInfo, JsonSettings.Options);
                        string? cDate = null;
                        if (wellBoreArchitecture.CreationDate != null)
                            cDate = ((DateTimeOffset)wellBoreArchitecture.CreationDate).ToString(SqlConnectionManager.DATE_TIME_FORMAT);
                        wellBoreArchitecture.LastModificationDate = DateTimeOffset.UtcNow;
                        string? lDate = ((DateTimeOffset)wellBoreArchitecture.LastModificationDate).ToString(SqlConnectionManager.DATE_TIME_FORMAT);
                        string data = JsonSerializer.Serialize(wellBoreArchitecture, JsonSettings.Options);
                        var command = connection.CreateCommand();
                        command.CommandText = "UPDATE WellBoreArchitectureTable SET " +
                            "MetaInfo = $metaInfo, Name = $name, Description = $description, " +
                            "CreationDate = $creationDate, LastModificationDate = $lastModificationDate, " +
                            "WellBoreArchitecture = $document WHERE (ID = $idText OR ID = $idValue)";
                        command.Parameters.AddWithValue("$metaInfo", metaInfo);
                        command.Parameters.AddWithValue("$name", wellBoreArchitecture.Name ?? string.Empty);
                        command.Parameters.AddWithValue("$description", wellBoreArchitecture.Description ?? string.Empty);
                        command.Parameters.AddWithValue("$creationDate", cDate ?? string.Empty);
                        command.Parameters.AddWithValue("$lastModificationDate", lDate);
                        command.Parameters.AddWithValue("$document", data);
                        command.Parameters.AddWithValue("$idText", guid.ToString());
                        command.Parameters.AddWithValue("$idValue", guid);
                        int count = command.ExecuteNonQuery();
                        if (count != 1)
                        {
                            _logger.LogWarning("Impossible to update the WellBoreArchitecture");
                            success = false;
                        }
                    }
                    catch (SqliteException ex)
                    {
                        _logger.LogError(ex, "Impossible to update the WellBoreArchitecture");
                        success = false;
                    }

                    // Finalizing
                    if (success)
                    {
                        transaction.Commit();
                        _logger.LogInformation("Updated the given WellBoreArchitecture successfully");
                        return true;
                    }
                    else
                    {
                        transaction.Rollback();
                    }
                }
                else
                {
                    _logger.LogWarning("Impossible to access the SQLite database");
                }
            }
            else
            {
                _logger.LogWarning("The WellBoreArchitecture ID or the ID of some of its attributes are null or empty");
            }
            return false;
        }

        /// <summary>Creates a dependency-closed WellBoreArchitecture backup from one SQLite snapshot.</summary>
        public WellBoreArchitectureBatchExportOutcome ExportBatch(WellBoreArchitectureBatchExportRequest? request)
        {
            using SqliteConnection? connection = _connectionManager.GetConnection();
            if (connection == null) return WellBoreArchitectureBatchExporter.StorageFailure("The WellBoreArchitecture database is unavailable.");
            using SqliteTransaction transaction = connection.BeginTransaction();
            try
            {
                List<Model.WellBoreArchitecture?> architectures = ReadDocuments<Model.WellBoreArchitecture>(connection, transaction,
                    "WellBoreArchitectureTable", "WellBoreArchitecture");
                List<Model.WellBoreArchitectureIdentity> identities = ReadDocuments<Model.WellBoreArchitectureIdentity>(connection, transaction,
                    "WellBoreArchitectureIdentityTable", "WellBoreArchitectureIdentity").Where(value => value != null).Cast<Model.WellBoreArchitectureIdentity>().ToList();
                List<Model.WellBoreArchitectureFeatureCategory> categories = ReadDocuments<Model.WellBoreArchitectureFeatureCategory>(connection, transaction,
                    "WellBoreArchitectureFeatureCategoryTable", "WellBoreArchitectureFeatureCategory").Where(value => value != null).Cast<Model.WellBoreArchitectureFeatureCategory>().ToList();
                WellBoreArchitectureBatchExportOutcome outcome = WellBoreArchitectureBatchExporter.Create(request, architectures,
                    DateTimeOffset.UtcNow, identities, categories);
                transaction.Commit();
                return outcome;
            }
            catch (Exception exception) when (exception is SqliteException or JsonException or InvalidOperationException)
            {
                try { transaction.Rollback(); } catch (InvalidOperationException) { }
                _logger.LogError(exception, "Unable to create a dependency-closed WellBoreArchitecture backup");
                return WellBoreArchitectureBatchExporter.StorageFailure("The stored WellBoreArchitectures or catalog dependencies could not be read.");
            }
        }

        /// <summary>Validates and restores a WellBoreArchitecture backup in one transaction.</summary>
        public WellBoreArchitectureBatchRestoreOutcome RestoreBatch(WellBoreArchitectureBatchRestoreRequest? request)
        {
            try
            {
                using SqliteConnection? connection = _connectionManager.GetConnection();
                if (connection == null) return WellBoreArchitectureBatchRestorer.StorageFailure("The WellBoreArchitecture database is unavailable.");
                return WellBoreArchitectureBatchRestorer.Restore(connection, request, DateTimeOffset.UtcNow);
            }
            catch (SqliteException exception)
            {
                _logger.LogError(exception, "Unable to open the WellBoreArchitecture database for batch restore");
                return WellBoreArchitectureBatchRestorer.StorageFailure("The WellBoreArchitecture database is unavailable.");
            }
        }

        private static List<T?> ReadDocuments<T>(SqliteConnection connection, SqliteTransaction transaction,
            string table, string documentColumn)
        {
            using SqliteCommand command = connection.CreateCommand();
            command.Transaction = transaction;
            command.CommandText = $"SELECT {documentColumn} FROM {table} ORDER BY ID";
            using SqliteDataReader reader = command.ExecuteReader();
            List<T?> result = [];
            while (reader.Read())
            {
                if (reader.IsDBNull(0)) throw new JsonException($"{table} contains a null document.");
                T? value = JsonSerializer.Deserialize<T>(reader.GetString(0), JsonSettings.Options);
                if (value == null) throw new JsonException($"{table} contains an invalid document.");
                result.Add(value);
            }
            return result;
        }

        private bool ValidateAssignments(Model.WellBoreArchitecture architecture)
        {
            architecture.WellBoreArchitectureIdentityAssignments ??= [];
            architecture.WellBoreArchitectureFeatureAssignments ??= [];
            if (architecture.WellBoreArchitectureIdentityAssignments.Any(value => value.ID == Guid.Empty) ||
                architecture.WellBoreArchitectureIdentityAssignments.GroupBy(value => value.ID).Any(group => group.Count() > 1) ||
                architecture.WellBoreArchitectureFeatureAssignments.Any(value => value.ID == Guid.Empty) ||
                architecture.WellBoreArchitectureFeatureAssignments.GroupBy(value => value.ID).Any(group => group.Count() > 1))
                return false;

            HashSet<Guid> identities = new WellBoreArchitectureIdentityManager(_connectionManager).GetAll()
                .Select(value => value.MetaInfo!.ID).ToHashSet();
            if (architecture.WellBoreArchitectureIdentityAssignments.Any(value => value.IdentityID is not Guid id || !identities.Contains(id)))
                return false;

            Dictionary<Guid, Model.WellBoreArchitectureFeatureCategory> categories =
                new WellBoreArchitectureFeatureCategoryManager(_connectionManager).GetAll().ToDictionary(value => value.MetaInfo!.ID);
            foreach (Model.WellBoreArchitectureFeatureAssignment assignment in architecture.WellBoreArchitectureFeatureAssignments)
            {
                if (assignment.FeatureCategoryID is not Guid categoryId || !categories.TryGetValue(categoryId, out var category) ||
                    assignment.FeatureOptionID is not Guid optionId || category.Options?.Any(option => option.ID == optionId) != true)
                    return false;
                if (!category.HasValidityPeriod && (assignment.FromDate != null || assignment.ToDate != null)) return false;
                if (assignment.FromDate > assignment.ToDate) return false;
            }
            foreach (var category in categories.Values.Where(value => value.IsExclusive))
            {
                var assignments = architecture.WellBoreArchitectureFeatureAssignments.Where(value => value.FeatureCategoryID == category.MetaInfo!.ID).ToList();
                for (int i = 0; i < assignments.Count; i++)
                    for (int j = i + 1; j < assignments.Count; j++)
                        if (PeriodsOverlap(assignments[i], assignments[j])) return false;
            }
            return true;
        }

        private static bool PeriodsOverlap(Model.WellBoreArchitectureFeatureAssignment left, Model.WellBoreArchitectureFeatureAssignment right) =>
            (left.ToDate == null || right.FromDate == null || left.ToDate >= right.FromDate) &&
            (right.ToDate == null || left.FromDate == null || right.ToDate >= left.FromDate);

        private static void EnsureReadableRevision(Model.WellBoreArchitecture architecture) =>
            architecture.LastModificationDate ??= architecture.CreationDate ?? DateTimeOffset.UnixEpoch;

        /// <summary>
        /// Deletes the WellBoreArchitecture of given ID from the microservice database
        /// </summary>
        /// <param name="guid"></param>
        /// <returns>true if the WellBoreArchitecture was deleted from the microservice database</returns>
        public bool DeleteWellBoreArchitectureById(Guid guid)
        {
            if (!guid.Equals(Guid.Empty))
            {
                using var connection = _connectionManager.GetConnection();
                if (connection != null)
                {
                    using var transaction = connection.BeginTransaction();
                    bool success = true;
                    //delete WellBoreArchitecture from WellBoreArchitectureTable
                    try
                    {
                        var command = connection.CreateCommand();
                        command.CommandText = "DELETE FROM WellBoreArchitectureTable " +
                            "WHERE ID = $idText OR ID = $idValue";
                        command.Parameters.AddWithValue("$idText", guid.ToString());
                        command.Parameters.AddWithValue("$idValue", guid);
                        int count = command.ExecuteNonQuery();
                        if (count < 0)
                        {
                            _logger.LogWarning("Impossible to delete the WellBoreArchitecture of given ID from the WellBoreArchitectureTable");
                            success = false;
                        }
                    }
                    catch (SqliteException ex)
                    {
                        _logger.LogError(ex, "Impossible to delete the WellBoreArchitecture of given ID from WellBoreArchitectureTable");
                        success = false;
                    }
                    if (success)
                    {
                        transaction.Commit();
                        _logger.LogInformation("Removed the WellBoreArchitecture of given ID from the WellBoreArchitectureTable successfully");
                    }
                    else
                    {
                        transaction.Rollback();
                    }
                    return success;
                }
                else
                {
                    _logger.LogWarning("Impossible to access the SQLite database");
                }
            }
            else
            {
                _logger.LogWarning("The WellBoreArchitecture ID is null or empty");
            }
            return false;
        }
    }
}
