using Microsoft.Data.Sqlite;
using OSDC.DotnetLibraries.General.DataManagement;
using OSDC.Drilling.WellBoreArchitecture.Model;
using OSDC.Drilling.WellBoreArchitecture.Service;
using Architecture = OSDC.Drilling.WellBoreArchitecture.Model.WellBoreArchitecture;

namespace ServiceTest;

[TestFixture]
public sealed class WellBoreArchitectureBatchBackupRestoreTests
{
    [Test]
    public void Export_contains_only_referenced_catalog_dependencies()
    {
        Guid identityId = Guid.NewGuid(), unusedIdentityId = Guid.NewGuid();
        Guid categoryId = Guid.NewGuid(), optionId = Guid.NewGuid(), unusedOptionId = Guid.NewGuid();
        Architecture architecture = ArchitectureWithAssignments(identityId, categoryId, optionId);
        WellBoreArchitectureIdentity[] identities =
        [
            Identity(identityId, "Planning name"), Identity(unusedIdentityId, "Unused identity")
        ];
        WellBoreArchitectureFeatureCategory[] categories =
        [
            Category(categoryId, false, true, (optionId, "Planned"), (unusedOptionId, "Unused")),
            Category(Guid.NewGuid(), false, false, (Guid.NewGuid(), "Unreferenced category"))
        ];

        WellBoreArchitectureBatchExportOutcome outcome = WellBoreArchitectureBatchExporter.Create(
            new WellBoreArchitectureBatchExportRequest { Scope = WellBoreArchitectureBatchExportScope.All },
            [architecture], DateTimeOffset.UtcNow, identities, categories);

        Assert.That(outcome.IsSuccess, Is.True);
        Assert.That(outcome.Document!.CatalogDependencies.Identities.Select(item => item.MetaInfo!.ID), Is.EqualTo(new[] { identityId }));
        Assert.That(outcome.Document.CatalogDependencies.FeatureCategories, Has.Count.EqualTo(1));
        Assert.That(outcome.Document.CatalogDependencies.FeatureCategories[0].Options.Select(item => item.ID), Is.EqualTo(new[] { optionId }));
    }

    [Test]
    public void Restore_creates_missing_catalogs_and_architecture_atomically()
    {
        using SqliteConnection connection = CreateDatabase();
        Guid identityId = Guid.NewGuid(), categoryId = Guid.NewGuid(), optionId = Guid.NewGuid();
        WellBoreArchitectureBatchRestoreOutcome outcome = WellBoreArchitectureBatchRestorer.Restore(connection,
            RestoreRequest(ArchitectureWithAssignments(identityId, categoryId, optionId),
                Identity(identityId, "Planning name"), Category(categoryId, false, true, (optionId, "Planned"))),
            DateTimeOffset.UtcNow);

        Assert.That(outcome.IsSuccess, Is.True);
        Assert.Multiple(() =>
        {
            Assert.That(Count(connection, "WellBoreArchitectureTable"), Is.EqualTo(1));
            Assert.That(Count(connection, "WellBoreArchitectureIdentityTable"), Is.EqualTo(1));
            Assert.That(Count(connection, "WellBoreArchitectureFeatureCategoryTable"), Is.EqualTo(1));
            Assert.That(outcome.Response!.CreatedCatalogDefinitionCount, Is.EqualTo(2));
            Assert.That(outcome.Response.CatalogMappings, Has.Count.EqualTo(3));
        });
    }

    [Test]
    public void Invalid_assignment_rolls_back_catalog_and_architecture_creation()
    {
        using SqliteConnection connection = CreateDatabase();
        Guid identityId = Guid.NewGuid(), categoryId = Guid.NewGuid(), optionId = Guid.NewGuid();
        Architecture architecture = ArchitectureWithAssignments(identityId, categoryId, optionId);
        architecture.WellBoreArchitectureFeatureAssignments![0].FromDate = DateTimeOffset.UtcNow;

        WellBoreArchitectureBatchRestoreOutcome outcome = WellBoreArchitectureBatchRestorer.Restore(connection,
            RestoreRequest(architecture, Identity(identityId, "Planning name"),
                Category(categoryId, false, false, (optionId, "Operational"))), DateTimeOffset.UtcNow);

        Assert.That(outcome.IsSuccess, Is.False);
        Assert.That(outcome.FailureKind, Is.EqualTo(WellBoreArchitectureBatchRestoreFailureKind.InvalidRequest));
        Assert.Multiple(() =>
        {
            Assert.That(Count(connection, "WellBoreArchitectureTable"), Is.Zero);
            Assert.That(Count(connection, "WellBoreArchitectureIdentityTable"), Is.Zero);
            Assert.That(Count(connection, "WellBoreArchitectureFeatureCategoryTable"), Is.Zero);
        });
    }

    private static WellBoreArchitectureBatchRestoreRequest RestoreRequest(Architecture architecture,
        WellBoreArchitectureIdentity identity, WellBoreArchitectureFeatureCategory category) => new()
    {
        ConflictPolicy = WellBoreArchitectureBatchRestoreConflictPolicy.FailIfExists,
        CatalogPolicy = WellBoreArchitectureBatchCatalogRestorePolicy.MapOrCreateMissing,
        Document = new WellBoreArchitectureBatchExportDocument
        {
            ExportedAtUtc = DateTimeOffset.UtcNow,
            CatalogDependencies = new() { Identities = [identity], FeatureCategories = [category] },
            WellBoreArchitectures = [architecture]
        }
    };

    private static Architecture ArchitectureWithAssignments(Guid identityId, Guid categoryId, Guid optionId) => new()
    {
        MetaInfo = new MetaInfo { ID = Guid.NewGuid() }, Name = "Architecture A",
        CreationDate = DateTimeOffset.UtcNow, LastModificationDate = DateTimeOffset.UtcNow,
        WellBoreArchitectureIdentityAssignments = [new() { ID = Guid.NewGuid(), IdentityID = identityId, Value = "A-01" }],
        WellBoreArchitectureFeatureAssignments = [new() { ID = Guid.NewGuid(), FeatureCategoryID = categoryId, FeatureOptionID = optionId }]
    };

    private static WellBoreArchitectureIdentity Identity(Guid id, string name) => new()
    {
        MetaInfo = new MetaInfo { ID = id }, Name = name,
        CreationDate = DateTimeOffset.UtcNow, LastModificationDate = DateTimeOffset.UtcNow
    };

    private static WellBoreArchitectureFeatureCategory Category(Guid id, bool exclusive, bool validity,
        params (Guid Id, string Name)[] options) => new()
    {
        MetaInfo = new MetaInfo { ID = id }, Name = "Lifecycle", IsExclusive = exclusive, HasValidityPeriod = validity,
        Options = options.Select(item => new WellBoreArchitectureFeatureOption { ID = item.Id, Name = item.Name }).ToList(),
        CreationDate = DateTimeOffset.UtcNow, LastModificationDate = DateTimeOffset.UtcNow
    };

    private static SqliteConnection CreateDatabase()
    {
        SqliteConnection connection = new("Data Source=:memory:"); connection.Open();
        using SqliteCommand command = connection.CreateCommand();
        command.CommandText = """
            CREATE TABLE WellBoreArchitectureTable (ID TEXT PRIMARY KEY, MetaInfo TEXT, Name TEXT, Description TEXT, CreationDate TEXT, LastModificationDate TEXT, WellBoreArchitecture TEXT NOT NULL);
            CREATE TABLE WellBoreArchitectureIdentityTable (ID TEXT PRIMARY KEY, MetaInfo TEXT, Name TEXT, CreationDate TEXT, LastModificationDate TEXT, WellBoreArchitectureIdentity TEXT NOT NULL);
            CREATE TABLE WellBoreArchitectureFeatureCategoryTable (ID TEXT PRIMARY KEY, MetaInfo TEXT, Name TEXT, IsExclusive INTEGER, HasValidityPeriod INTEGER, CreationDate TEXT, LastModificationDate TEXT, WellBoreArchitectureFeatureCategory TEXT NOT NULL);
            """;
        command.ExecuteNonQuery(); return connection;
    }

    private static long Count(SqliteConnection connection, string table)
    {
        using SqliteCommand command = connection.CreateCommand(); command.CommandText = $"SELECT COUNT(*) FROM {table}";
        return Convert.ToInt64(command.ExecuteScalar());
    }
}
