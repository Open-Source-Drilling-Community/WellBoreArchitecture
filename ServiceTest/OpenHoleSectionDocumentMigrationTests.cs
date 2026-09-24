using Microsoft.Data.Sqlite;
using OSDC.Drilling.WellBoreArchitecture.Service;
using System.Text.Json.Nodes;

namespace ServiceTest;

[TestFixture]
public sealed class OpenHoleSectionDocumentMigrationTests
{
    [Test]
    public void Apply_moves_final_casing_open_hole_to_root_and_removes_legacy_values()
    {
        using Database database = Database.Create(Document(
            Casing(null),
            Casing(OpenHole(0.20))));

        OpenHoleSectionMigrationReport report = OpenHoleSectionDocumentMigration.Run(
            database.ConnectionString, OpenHoleSectionMigrationMode.Apply,
            new DateTimeOffset(2026, 9, 24, 12, 0, 0, TimeSpan.Zero));

        JsonObject migrated = database.ReadDocument();
        Assert.Multiple(() =>
        {
            Assert.That(report.CanApply, Is.True);
            Assert.That(report.MigratedCount, Is.EqualTo(1));
            Assert.That(migrated["OpenHoleSection"]?["HoleSizes"]?[0]?["HoleSize"]?["GaussianValue"]?["Mean"]?.GetValue<double>(), Is.EqualTo(0.20));
            Assert.That(migrated["CasingSections"]!.AsArray().All(value => value?["OpenHoleSection"] == null), Is.True);
            Assert.That(database.SchemaVersion, Is.EqualTo(OpenHoleSectionDocumentMigration.TargetSchemaVersion));
        });
    }

    [Test]
    public void Audit_rejects_differing_casing_level_values_without_writing()
    {
        string original = Document(Casing(OpenHole(0.30)), Casing(OpenHole(0.20)));
        using Database database = Database.Create(original);

        OpenHoleSectionMigrationReport report = OpenHoleSectionDocumentMigration.Run(
            database.ConnectionString, OpenHoleSectionMigrationMode.Audit, DateTimeOffset.UtcNow);

        Assert.Multiple(() =>
        {
            Assert.That(report.CanApply, Is.False);
            Assert.That(report.Issues.Select(value => value.Code), Contains.Item("conflicting_legacy_open_holes"));
            Assert.That(database.ReadRawDocument(), Is.EqualTo(original));
            Assert.That(database.SchemaVersion, Is.EqualTo(2));
        });
    }

    [Test]
    public void Audit_rejects_a_legacy_source_that_is_not_on_the_final_casing()
    {
        using Database database = Database.Create(Document(Casing(OpenHole(0.30)), Casing(null)));

        OpenHoleSectionMigrationReport report = OpenHoleSectionDocumentMigration.Run(
            database.ConnectionString, OpenHoleSectionMigrationMode.Audit, DateTimeOffset.UtcNow);

        Assert.That(report.Issues.Select(value => value.Code), Contains.Item("legacy_source_not_on_final_casing"));
    }

    [Test]
    public void Audit_accepts_identical_legacy_values_when_the_final_casing_is_populated()
    {
        using Database database = Database.Create(Document(Casing(OpenHole(0.25)), Casing(OpenHole(0.25))));

        OpenHoleSectionMigrationReport report = OpenHoleSectionDocumentMigration.Run(
            database.ConnectionString, OpenHoleSectionMigrationMode.Audit, DateTimeOffset.UtcNow);

        Assert.Multiple(() =>
        {
            Assert.That(report.CanApply, Is.True);
            Assert.That(report.ReadyCount, Is.EqualTo(1));
        });
    }

    private static JsonObject OpenHole(double diameter) => new()
    {
        ["HoleSizes"] = new JsonArray(new JsonObject
        {
            ["HoleSize"] = Gaussian(diameter),
            ["Length"] = Gaussian(10.0)
        })
    };

    private static JsonObject Gaussian(double mean) => new()
    {
        ["GaussianValue"] = new JsonObject { ["Mean"] = mean }
    };

    private static JsonObject Casing(JsonObject? openHole) => new()
    {
        ["CasingSectionSizeTable"] = openHole?["HoleSizes"]?.DeepClone() ?? new JsonArray(),
        ["OpenHoleSection"] = openHole
    };

    private static string Document(params JsonObject[] casings)
    {
        Guid id = Guid.NewGuid();
        return new JsonObject
        {
            ["MetaInfo"] = new JsonObject { ["ID"] = id },
            ["LastModificationDate"] = "2026-01-01T00:00:00+00:00",
            ["CasingSections"] = new JsonArray(casings.Select(value => value.DeepClone()).ToArray())
        }.ToJsonString();
    }

    private sealed class Database : IDisposable
    {
        private readonly string path;
        public string ConnectionString => $"Data Source={path};Pooling=False";
        public int SchemaVersion => Convert.ToInt32(Scalar("PRAGMA user_version"));

        private Database(string path) => this.path = path;

        public static Database Create(string document)
        {
            string path = Path.Combine(TestContext.CurrentContext.WorkDirectory, $"open-hole-migration-{Guid.NewGuid():N}.db");
            Database result = new(path);
            using SqliteConnection connection = new(result.ConnectionString);
            connection.Open();
            using SqliteCommand command = connection.CreateCommand();
            command.CommandText = "CREATE TABLE WellBoreArchitectureTable (ID text primary key, MetaInfo text, Name text, Description text, CreationDate text, LastModificationDate text, WellBoreArchitecture text); PRAGMA user_version = 2";
            command.ExecuteNonQuery();
            JsonObject root = JsonNode.Parse(document)!.AsObject();
            string id = root["MetaInfo"]!["ID"]!.GetValue<string>();
            command.CommandText = "INSERT INTO WellBoreArchitectureTable VALUES ($id, '{}', '', '', '', '', $document)";
            command.Parameters.AddWithValue("$id", id);
            command.Parameters.AddWithValue("$document", document);
            command.ExecuteNonQuery();
            return result;
        }

        public string ReadRawDocument() => Convert.ToString(Scalar("SELECT WellBoreArchitecture FROM WellBoreArchitectureTable"))!;
        public JsonObject ReadDocument() => JsonNode.Parse(ReadRawDocument())!.AsObject();

        private object? Scalar(string sql)
        {
            using SqliteConnection connection = new(ConnectionString);
            connection.Open();
            using SqliteCommand command = connection.CreateCommand();
            command.CommandText = sql;
            return command.ExecuteScalar();
        }

        public void Dispose()
        {
            if (File.Exists(path)) File.Delete(path);
        }
    }
}
