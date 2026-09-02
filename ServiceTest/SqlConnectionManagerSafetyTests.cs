using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Logging;
using OSDC.Drilling.WellBoreArchitecture.Service.Managers;

namespace ServiceTest;

[TestFixture]
public sealed class SqlConnectionManagerSafetyTests
{
    private ILogger<SqlConnectionManager> _logger = null!;

    [OneTimeSetUp]
    public void OneTimeSetUp()
    {
        ILoggerFactory factory = LoggerFactory.Create(builder => builder.ClearProviders());
        _logger = factory.CreateLogger<SqlConnectionManager>();
    }

    [Test]
    public void Fresh_database_is_created_with_the_current_schema_version()
    {
        WithDatabase(path =>
        {
            _ = Manager(path);

            using SqliteConnection connection = Open(path);
            Assert.Multiple(() =>
            {
                Assert.That(ScalarLong(connection,
                    "SELECT COUNT(*) FROM sqlite_master WHERE type='table' AND name='WellBoreArchitectureTable'"), Is.EqualTo(1));
                Assert.That(ScalarLong(connection,
                    "SELECT COUNT(*) FROM sqlite_master WHERE type='table' AND name='WellBoreArchitectureIdentityTable'"), Is.EqualTo(1));
                Assert.That(ScalarLong(connection,
                    "SELECT COUNT(*) FROM sqlite_master WHERE type='table' AND name='WellBoreArchitectureFeatureCategoryTable'"), Is.EqualTo(1));
                Assert.That(ScalarLong(connection, "PRAGMA user_version"),
                    Is.EqualTo(SqlConnectionManager.CURRENT_SCHEMA_VERSION));
            });
        });
    }

    [Test]
    public void Valid_legacy_database_is_adopted_without_changing_existing_rows()
    {
        WithDatabase(path =>
        {
            using (SqliteConnection connection = Open(path))
            {
                CreateExpectedTable(connection);
                Execute(connection, "INSERT INTO WellBoreArchitectureTable (ID,MetaInfo,Name,Description,CreationDate,LastModificationDate,WellBoreArchitecture) " +
                                    "VALUES ('marker','{\"ID\":\"marker\"}','preserve-name','preserve-description','created','modified','{\"payload\":\"preserve-me\"}')");
            }

            _ = Manager(path);

            using SqliteConnection verification = Open(path);
            Assert.Multiple(() =>
            {
                Assert.That(ScalarString(verification,
                    "SELECT WellBoreArchitecture FROM WellBoreArchitectureTable WHERE ID='marker'"),
                    Is.EqualTo("{\"payload\":\"preserve-me\"}"));
                Assert.That(ScalarString(verification,
                    "SELECT Name FROM WellBoreArchitectureTable WHERE ID='marker'"), Is.EqualTo("preserve-name"));
                Assert.That(ScalarLong(verification, "PRAGMA user_version"),
                    Is.EqualTo(SqlConnectionManager.CURRENT_SCHEMA_VERSION));
                Assert.That(ScalarLong(verification,
                    "SELECT COUNT(*) FROM sqlite_master WHERE type='index' AND name='WellBoreArchitectureTableIndex'"), Is.EqualTo(1));
                Assert.That(ScalarLong(verification,
                    "SELECT COUNT(*) FROM sqlite_master WHERE type='table' AND name='WellBoreArchitectureIdentityTable'"), Is.EqualTo(1));
                Assert.That(ScalarLong(verification,
                    "SELECT COUNT(*) FROM sqlite_master WHERE type='table' AND name='WellBoreArchitectureFeatureCategoryTable'"), Is.EqualTo(1));
            });
        });
    }

    [Test]
    public void Version_one_database_is_upgraded_additively_without_rewriting_architectures()
    {
        WithDatabase(path =>
        {
            using (SqliteConnection connection = Open(path))
            {
                CreateExpectedTable(connection);
                Execute(connection, "INSERT INTO WellBoreArchitectureTable (ID,Name,WellBoreArchitecture) VALUES ('marker','preserve-me','{\"Name\":\"preserve-me\"}')");
                Execute(connection, "PRAGMA user_version = 1");
            }
            _ = Manager(path);
            using SqliteConnection verification = Open(path);
            Assert.Multiple(() =>
            {
                Assert.That(ScalarString(verification, "SELECT WellBoreArchitecture FROM WellBoreArchitectureTable WHERE ID='marker'"), Is.EqualTo("{\"Name\":\"preserve-me\"}"));
                Assert.That(ScalarLong(verification, "PRAGMA user_version"), Is.EqualTo(2));
                Assert.That(ScalarLong(verification, "SELECT COUNT(*) FROM sqlite_master WHERE type='table' AND name='WellBoreArchitectureIdentityTable'"), Is.EqualTo(1));
                Assert.That(ScalarLong(verification, "SELECT COUNT(*) FROM sqlite_master WHERE type='table' AND name='WellBoreArchitectureFeatureCategoryTable'"), Is.EqualTo(1));
            });
        });
    }

    [Test]
    public void Default_identity_and_feature_catalogs_are_seeded_without_touching_architecture_rows()
    {
        WithDatabase(path =>
        {
            SqlConnectionManager connections = Manager(path);
            var identities = new WellBoreArchitectureIdentityManager(connections).GetAll();
            var features = new WellBoreArchitectureFeatureCategoryManager(connections).GetAll();
            using SqliteConnection verification = Open(path);

            Assert.Multiple(() =>
            {
                Assert.That(identities.Select(value => value.Name), Is.EquivalentTo(new[]
                {
                    "NameForPlanning", "NameForCompanyReporting", "NameForRegulatoryReporting", "Nickname", "NameForOperationReporting"
                }));
                Assert.That(features.Select(value => value.Name), Is.EquivalentTo(new[] { "Lifecycle", "ApprovalStatus", "SectionRole", "DrillingMethod" }));
                Assert.That(features.Single(value => value.Name == "DrillingMethod").Options!.Select(value => value.Name),
                    Does.Contain("Geosteered"));
                Assert.That(ScalarLong(verification, "SELECT COUNT(*) FROM WellBoreArchitectureTable"), Is.Zero);
            });
        });
    }

    [Test]
    public void Unexpected_schema_aborts_startup_without_dropping_data()
    {
        WithDatabase(path =>
        {
            using (SqliteConnection connection = Open(path))
            {
                Execute(connection, "CREATE TABLE Unexpected (ID TEXT PRIMARY KEY, Payload TEXT)");
                Execute(connection, "INSERT INTO Unexpected (ID,Payload) VALUES ('marker','preserve-me')");
            }

            Assert.Throws<InvalidOperationException>(() => Manager(path));

            using SqliteConnection verification = Open(path);
            Assert.That(ScalarString(verification, "SELECT Payload FROM Unexpected WHERE ID='marker'"),
                Is.EqualTo("preserve-me"));
        });
    }

    [Test]
    public void Malformed_expected_table_aborts_startup_without_changing_data()
    {
        WithDatabase(path =>
        {
            using (SqliteConnection connection = Open(path))
            {
                Execute(connection, "CREATE TABLE WellBoreArchitectureTable (ID TEXT PRIMARY KEY, Payload TEXT)");
                Execute(connection, "INSERT INTO WellBoreArchitectureTable (ID,Payload) VALUES ('marker','preserve-me')");
            }

            Assert.Throws<InvalidOperationException>(() => Manager(path));

            using SqliteConnection verification = Open(path);
            Assert.Multiple(() =>
            {
                Assert.That(ScalarString(verification,
                    "SELECT Payload FROM WellBoreArchitectureTable WHERE ID='marker'"), Is.EqualTo("preserve-me"));
                Assert.That(ScalarLong(verification, "PRAGMA user_version"), Is.Zero);
            });
        });
    }

    [Test]
    public void Newer_schema_version_is_rejected_without_changes()
    {
        WithDatabase(path =>
        {
            using (SqliteConnection connection = Open(path))
            {
                CreateExpectedTable(connection);
                Execute(connection, "INSERT INTO WellBoreArchitectureTable (ID,Name) VALUES ('marker','preserve-me')");
                Execute(connection, $"PRAGMA user_version = {SqlConnectionManager.CURRENT_SCHEMA_VERSION + 1}");
            }

            Assert.Throws<InvalidOperationException>(() => Manager(path));

            using SqliteConnection verification = Open(path);
            Assert.Multiple(() =>
            {
                Assert.That(ScalarString(verification,
                    "SELECT Name FROM WellBoreArchitectureTable WHERE ID='marker'"), Is.EqualTo("preserve-me"));
                Assert.That(ScalarLong(verification, "PRAGMA user_version"),
                    Is.EqualTo(SqlConnectionManager.CURRENT_SCHEMA_VERSION + 1));
            });
        });
    }

    private SqlConnectionManager Manager(string path) => new($"Data Source={path};Pooling=False", _logger);

    private static SqliteConnection Open(string path)
    {
        var connection = new SqliteConnection($"Data Source={path};Pooling=False");
        connection.Open();
        return connection;
    }

    private static void CreateExpectedTable(SqliteConnection connection) => Execute(connection,
        "CREATE TABLE WellBoreArchitectureTable (ID text primary key,MetaInfo text,Name text,Description text," +
        "CreationDate text,LastModificationDate text,WellBoreArchitecture text)");

    private static void Execute(SqliteConnection connection, string sql)
    {
        using SqliteCommand command = connection.CreateCommand();
        command.CommandText = sql;
        command.ExecuteNonQuery();
    }

    private static long ScalarLong(SqliteConnection connection, string sql)
    {
        using SqliteCommand command = connection.CreateCommand();
        command.CommandText = sql;
        return Convert.ToInt64(command.ExecuteScalar());
    }

    private static string ScalarString(SqliteConnection connection, string sql)
    {
        using SqliteCommand command = connection.CreateCommand();
        command.CommandText = sql;
        return Convert.ToString(command.ExecuteScalar())!;
    }

    private static void WithDatabase(Action<string> test)
    {
        string path = Path.Combine(TestContext.CurrentContext.WorkDirectory,
            $"WellBoreArchitectureSafety_{Guid.NewGuid():N}.db");
        try
        {
            test(path);
        }
        finally
        {
            SqliteConnection.ClearAllPools();
            if (File.Exists(path)) File.Delete(path);
        }
    }
}
