using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Logging;
using OSDC.DotnetLibraries.General.DataManagement;
using OSDC.Drilling.WellBoreArchitecture.Service;
using OSDC.Drilling.WellBoreArchitecture.Service.Managers;
using System.Text.Json;
using ArchitectureModel = OSDC.Drilling.WellBoreArchitecture.Model.WellBoreArchitecture;

namespace ServiceTest;

[TestFixture]
public sealed class WellBoreArchitectureManagerPersistenceTests
{
    private ILoggerFactory _loggerFactory = null!;

    [SetUp]
    public void SetUp() => _loggerFactory = LoggerFactory.Create(builder => builder.ClearProviders());

    [TearDown]
    public void TearDown() => _loggerFactory.Dispose();

    [Test]
    public void Create_and_update_accept_apostrophes_and_create_assigns_concurrency_timestamps()
    {
        WithDatabase(path =>
        {
            var connections = new SqlConnectionManager(
                $"Data Source={path};Pooling=False",
                _loggerFactory.CreateLogger<SqlConnectionManager>());
            var manager = new WellBoreArchitectureManager(
                _loggerFactory.CreateLogger<WellBoreArchitectureManager>(),
                connections);
            var architecture = new ArchitectureModel
            {
                MetaInfo = new MetaInfo { ID = Guid.NewGuid() },
                Name = "U1's architecture",
                Description = "Main features of well U1's downhole architecture"
            };

            DateTimeOffset beforeCreate = DateTimeOffset.UtcNow;
            Assert.That(manager.AddWellBoreArchitecture(architecture), Is.True);
            DateTimeOffset afterCreate = DateTimeOffset.UtcNow;

            ArchitectureModel? stored = manager.GetWellBoreArchitectureById(architecture.MetaInfo.ID);
            Assert.That(stored, Is.Not.Null);
            Assert.Multiple(() =>
            {
                Assert.That(stored!.Name, Is.EqualTo(architecture.Name));
                Assert.That(stored.Description, Is.EqualTo(architecture.Description));
                Assert.That(stored.CreationDate, Is.InRange(beforeCreate, afterCreate));
                Assert.That(stored.LastModificationDate, Is.EqualTo(stored.CreationDate));
            });

            stored!.Description = "It's valid to use another apostrophe";
            Assert.That(manager.UpdateWellBoreArchitectureById(stored.MetaInfo.ID, stored), Is.True);
            ArchitectureModel updated = manager.GetWellBoreArchitectureById(stored.MetaInfo.ID)!;
            Assert.Multiple(() =>
            {
                Assert.That(updated.Description, Is.EqualTo(stored.Description));
                Assert.That(updated.LastModificationDate, Is.GreaterThanOrEqualTo(stored.CreationDate));
            });
        });
    }

    [Test]
    public void Legacy_record_without_timestamps_exposes_its_effective_concurrency_token()
    {
        WithDatabase(path =>
        {
            var connections = new SqlConnectionManager(
                $"Data Source={path};Pooling=False",
                _loggerFactory.CreateLogger<SqlConnectionManager>());
            var manager = new WellBoreArchitectureManager(
                _loggerFactory.CreateLogger<WellBoreArchitectureManager>(),
                connections);
            var architecture = new ArchitectureModel
            {
                MetaInfo = new MetaInfo { ID = Guid.NewGuid() },
                Name = "Legacy architecture"
            };
            using (SqliteConnection connection = connections.GetConnection()!)
            using (SqliteCommand command = connection.CreateCommand())
            {
                command.CommandText = "INSERT INTO WellBoreArchitectureTable " +
                    "(ID, MetaInfo, Name, Description, CreationDate, LastModificationDate, WellBoreArchitecture) " +
                    "VALUES ($id, $meta, $name, '', '', '', $document)";
                command.Parameters.AddWithValue("$id", architecture.MetaInfo.ID);
                command.Parameters.AddWithValue("$meta", JsonSerializer.Serialize(architecture.MetaInfo, JsonSettings.Options));
                command.Parameters.AddWithValue("$name", architecture.Name);
                command.Parameters.AddWithValue("$document", JsonSerializer.Serialize(architecture, JsonSettings.Options));
                Assert.That(command.ExecuteNonQuery(), Is.EqualTo(1));
            }

            ArchitectureModel? stored = manager.GetWellBoreArchitectureById(architecture.MetaInfo.ID);
            Assert.That(stored, Is.Not.Null);
            Assert.That(stored!.LastModificationDate, Is.EqualTo(DateTimeOffset.UnixEpoch));

            stored.Description = "Claude's update can echo the visible epoch token";
            Assert.That(manager.UpdateWellBoreArchitectureById(stored.MetaInfo.ID, stored), Is.True);
            Assert.That(manager.GetWellBoreArchitectureById(stored.MetaInfo.ID)!.LastModificationDate,
                Is.GreaterThan(DateTimeOffset.UnixEpoch));
        });
    }

    private static void WithDatabase(Action<string> test)
    {
        string path = Path.Combine(TestContext.CurrentContext.WorkDirectory,
            $"WellBoreArchitecturePersistence_{Guid.NewGuid():N}.db");
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
