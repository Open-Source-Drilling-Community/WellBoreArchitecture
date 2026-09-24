using Microsoft.Data.Sqlite;
using OSDC.DotnetLibraries.General.DataManagement;
using OSDC.Drilling.WellBoreArchitecture.Service.Managers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace OSDC.Drilling.WellBoreArchitecture.Service;

public enum OpenHoleSectionMigrationMode { Audit, Apply }

public sealed record OpenHoleSectionMigrationIssue(string ArchitectureID, string Code, string Message);

public sealed record OpenHoleSectionMigrationReport(
    OpenHoleSectionMigrationMode Mode,
    int ExaminedCount,
    int ReadyCount,
    int UnchangedCount,
    int MigratedCount,
    IReadOnlyList<OpenHoleSectionMigrationIssue> Issues)
{
    public bool CanApply => Issues.Count == 0;
}

/// <summary>
/// Audits and rewrites the legacy casing-level OpenHoleSection JSON shape. The complete database
/// is validated before any row is changed, and apply mode commits every document and the schema
/// version in one transaction.
/// </summary>
public static class OpenHoleSectionDocumentMigration
{
    public const int TargetSchemaVersion = 3;

    public static OpenHoleSectionMigrationReport Run(string connectionString, OpenHoleSectionMigrationMode mode, DateTimeOffset now)
    {
        using SqliteConnection connection = new(connectionString);
        connection.Open();
        using SqliteCommand versionCommand = connection.CreateCommand();
        versionCommand.CommandText = "PRAGMA user_version";
        int version = Convert.ToInt32(versionCommand.ExecuteScalar());
        if (version > TargetSchemaVersion)
            throw new InvalidOperationException($"Database schema version {version} is newer than supported migration version {TargetSchemaVersion}.");

        List<PreparedDocument> prepared = [];
        List<OpenHoleSectionMigrationIssue> issues = [];
        int examined = 0;
        using (SqliteCommand command = connection.CreateCommand())
        {
            command.CommandText = "SELECT ID, WellBoreArchitecture FROM WellBoreArchitectureTable ORDER BY ID";
            using SqliteDataReader reader = command.ExecuteReader();
            while (reader.Read())
            {
                examined++;
                string id = reader.GetString(0);
                string document = reader.GetString(1);
                try
                {
                    JsonObject root = JsonNode.Parse(document)?.AsObject()
                        ?? throw new JsonException("The architecture document is not a JSON object.");
                    prepared.Add(Prepare(id, root, now, issues));
                }
                catch (Exception exception) when (exception is JsonException or InvalidOperationException)
                {
                    issues.Add(new(id, "invalid_document", exception.Message));
                }
            }
        }

        int ready = prepared.Count(value => value.Changed);
        if (mode == OpenHoleSectionMigrationMode.Audit || issues.Count != 0)
            return new(mode, examined, ready, prepared.Count - ready, 0, issues);

        using SqliteTransaction transaction = connection.BeginTransaction();
        try
        {
            foreach (PreparedDocument value in prepared.Where(value => value.Changed))
            {
                using SqliteCommand update = connection.CreateCommand();
                update.Transaction = transaction;
                update.CommandText = "UPDATE WellBoreArchitectureTable SET WellBoreArchitecture = $document, LastModificationDate = $modified WHERE ID = $id";
                update.Parameters.AddWithValue("$document", value.Document.ToJsonString(JsonSettings.Options));
                update.Parameters.AddWithValue("$modified", now.UtcDateTime.ToString(SqlConnectionManager.DATE_TIME_FORMAT));
                update.Parameters.AddWithValue("$id", value.ID);
                if (update.ExecuteNonQuery() != 1)
                    throw new InvalidOperationException($"Architecture '{value.ID}' was not updated exactly once.");
            }
            using SqliteCommand setVersion = connection.CreateCommand();
            setVersion.Transaction = transaction;
            setVersion.CommandText = $"PRAGMA user_version = {TargetSchemaVersion}";
            setVersion.ExecuteNonQuery();
            transaction.Commit();
        }
        catch
        {
            transaction.Rollback();
            throw;
        }
        return new(mode, examined, ready, prepared.Count - ready, ready, issues);
    }

    private static PreparedDocument Prepare(string idText, JsonObject root, DateTimeOffset now,
        List<OpenHoleSectionMigrationIssue> issues)
    {
        if (!Guid.TryParse(idText, out Guid architectureId) || architectureId == Guid.Empty)
        {
            issues.Add(new(idText, "invalid_architecture_id", "The SQLite row ID is not a non-empty UUID."));
            return new(idText, root, false);
        }

        JsonArray casings = root["CasingSections"] as JsonArray ?? [];
        List<(int Index, JsonObject Value)> legacy = [];
        for (int index = 0; index < casings.Count; index++)
        {
            if (casings[index] is not JsonObject casing) continue;
            if (casing["OpenHoleSection"] is JsonObject openHole)
                legacy.Add((index, openHole));
        }
        if (legacy.Count == 0) return new(idText, root, false);

        JsonNode referenceEngineering = EngineeringValue(legacy[0].Value);
        if (legacy.Skip(1).Any(value => !JsonNode.DeepEquals(referenceEngineering, EngineeringValue(value.Value))))
        {
            issues.Add(new(idText, "conflicting_legacy_open_holes", "Multiple casing-level OpenHoleSection values differ and require per-record review."));
            return new(idText, root, false);
        }

        int finalIndex = casings.Count - 1;
        foreach ((int index, JsonObject openHole) in legacy.Where(value => value.Index != finalIndex))
        {
            JsonObject casing = casings[index]!.AsObject();
            if (!JsonNode.DeepEquals(EngineeringSizes(openHole["HoleSizes"]),
                    EngineeringSizes(casing["CasingSectionSizeTable"])))
            {
                issues.Add(new(idText, "conflicting_casing_borehole_diameters",
                    $"Casing {index} has legacy OpenHoleSection rows that differ from CasingSectionSizeTable and requires per-record review."));
                return new(idText, root, false);
            }
        }
        JsonObject? finalLegacy = legacy.FirstOrDefault(value => value.Index == finalIndex).Value;
        JsonObject? existingRoot = root["OpenHoleSection"] as JsonObject;
        if (finalLegacy == null && existingRoot == null)
        {
            issues.Add(new(idText, "legacy_source_not_on_final_casing", "Legacy open-hole data exists, but the final casing has no OpenHoleSection and the root is empty."));
            return new(idText, root, false);
        }
        if (finalLegacy != null && existingRoot != null &&
            !JsonNode.DeepEquals(EngineeringValue(finalLegacy), EngineeringValue(existingRoot)))
        {
            issues.Add(new(idText, "conflicting_root_open_hole", "The root and final-casing OpenHoleSection values differ and require per-record review."));
            return new(idText, root, false);
        }

        JsonObject? selected = existingRoot?.DeepClone().AsObject() ?? finalLegacy?.DeepClone().AsObject();
        if (selected != null)
        {
            EnsureComponentIDs(selected, architectureId, finalIndex);
            root["OpenHoleSection"] = selected;
        }
        foreach (JsonNode? node in casings)
            if (node is JsonObject casing) casing.Remove("OpenHoleSection");
        root["LastModificationDate"] = JsonValue.Create(now.ToUniversalTime());
        Model.WellBoreArchitecture? migrated = root.Deserialize<Model.WellBoreArchitecture>(JsonSettings.Options);
        if (migrated?.MetaInfo?.ID != architectureId || !WellBoreArchitectureComponentIdentity.Ensure(migrated))
        {
            issues.Add(new(idText, "invalid_migrated_document", "The migrated document has a mismatched resource ID or duplicate nested component UUIDs."));
            return new(idText, root, false);
        }
        return new(idText, root, true);
    }

    private static JsonNode EngineeringValue(JsonObject openHole)
    {
        JsonObject copy = openHole.DeepClone().AsObject();
        copy.Remove("ComponentID");
        if (copy["HoleSizes"] is JsonArray sizes)
            foreach (JsonNode? node in sizes)
                if (node is JsonObject size) size.Remove("ComponentID");
        return copy;
    }

    private static JsonNode EngineeringSizes(JsonNode? source)
    {
        JsonArray copy = source?.DeepClone() as JsonArray ?? [];
        foreach (JsonNode? node in copy)
            if (node is JsonObject size) size.Remove("ComponentID");
        return copy;
    }

    private static void EnsureComponentIDs(JsonObject openHole, Guid architectureId, int sourceCasingIndex)
    {
        if (!TryNonEmptyGuid(openHole["ComponentID"]))
            openHole["ComponentID"] = Derive(architectureId, $"casing/{sourceCasingIndex}/open-hole");
        if (openHole["HoleSizes"] is not JsonArray sizes) return;
        for (int index = 0; index < sizes.Count; index++)
            if (sizes[index] is JsonObject size && !TryNonEmptyGuid(size["ComponentID"]))
                size["ComponentID"] = Derive(architectureId, $"casing/{sourceCasingIndex}/open-hole/size/{index}");
    }

    private static bool TryNonEmptyGuid(JsonNode? node) =>
        node != null && Guid.TryParse(node.GetValue<string>(), out Guid value) && value != Guid.Empty;

    private static Guid Derive(Guid architectureId, string path)
    {
        byte[] hash = SHA256.HashData(Encoding.UTF8.GetBytes($"{architectureId:D}/{path}"));
        Span<byte> value = hash.AsSpan(0, 16);
        value[6] = (byte)((value[6] & 0x0F) | 0x50);
        value[8] = (byte)((value[8] & 0x3F) | 0x80);
        return new Guid(value);
    }

    private sealed record PreparedDocument(string ID, JsonObject Document, bool Changed);
}
