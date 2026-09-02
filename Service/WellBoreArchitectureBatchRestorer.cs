using Microsoft.Data.Sqlite;
using OSDC.DotnetLibraries.General.DataManagement;
using OSDC.Drilling.WellBoreArchitecture.Model;
using OSDC.Drilling.WellBoreArchitecture.Service.Managers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using WellBoreArchitectureModel = OSDC.Drilling.WellBoreArchitecture.Model.WellBoreArchitecture;

namespace OSDC.Drilling.WellBoreArchitecture.Service;

public enum WellBoreArchitectureBatchRestoreFailureKind { None, InvalidRequest, Conflict, StorageFailure }

public sealed class WellBoreArchitectureBatchRestoreOutcome
{
    public WellBoreArchitectureBatchRestoreResponse? Response { get; init; }
    public WellBoreArchitectureBatchErrorEnvelope? Error { get; init; }
    public WellBoreArchitectureBatchRestoreFailureKind FailureKind { get; init; }
    public bool IsSuccess => Response != null && FailureKind == WellBoreArchitectureBatchRestoreFailureKind.None;
}

/// <summary>Validates, maps catalogs, and restores the complete batch in one transaction.</summary>
public static class WellBoreArchitectureBatchRestorer
{
    public static WellBoreArchitectureBatchRestoreOutcome Restore(SqliteConnection connection,
        WellBoreArchitectureBatchRestoreRequest? request, DateTimeOffset restoredAtUtc)
    {
        List<WellBoreArchitectureBatchError> validationErrors = ValidateRequest(request);
        if (validationErrors.Count != 0) return Failure(WellBoreArchitectureBatchRestoreFailureKind.InvalidRequest,
            "invalid_batch_restore_request", "The WellBoreArchitecture batch-restore request is invalid. No changes were made.", validationErrors);

        using SqliteTransaction transaction = connection.BeginTransaction();
        try
        {
            CatalogState catalogs = CatalogState.Load(connection, transaction);
            List<WellBoreArchitectureModel> wellBores = CloneWellBoreArchitectures(request!.Document!.WellBoreArchitectures);
            List<WellBoreArchitectureBatchCatalogMapping> mappings = [];
            List<WellBoreArchitectureBatchError> mappingErrors = [];
            int createdDefinitions = 0;
            int createdOptions = 0;
            bool createMissing = request.CatalogPolicy == WellBoreArchitectureBatchCatalogRestorePolicy.MapOrCreateMissing;

            ResolveDependencies(request.Document.CatalogDependencies, catalogs, createMissing, request.AllowNormalizedNameMapping, mappings,
                mappingErrors, restoredAtUtc, ref createdDefinitions, ref createdOptions);
            if (mappingErrors.Count != 0)
            {
                transaction.Rollback();
                return Failure(WellBoreArchitectureBatchRestoreFailureKind.Conflict, "catalog_restore_conflict",
                    "Catalog references could not be resolved unambiguously. No changes were made.", mappingErrors);
            }
            RewriteReferences(wellBores, mappings);

            List<WellBoreArchitectureBatchError> componentErrors = [];
            for (int index = 0; index < wellBores.Count; index++)
                if (!WellBoreArchitectureComponentIdentity.Ensure(wellBores[index]))
                    componentErrors.Add(Error(index, "Document.WellBoreArchitectures", "duplicate_component_uuid",
                        "Nested ComponentID values must be unique within an architecture."));
            if (componentErrors.Count != 0)
            {
                transaction.Rollback();
                return Failure(WellBoreArchitectureBatchRestoreFailureKind.InvalidRequest, "invalid_component_ids",
                    "One or more restored architectures contain duplicate component IDs. No changes were made.", componentErrors);
            }

            List<WellBoreArchitectureBatchError> assignmentErrors = [];
            for (int index = 0; index < wellBores.Count; index++)
                assignmentErrors.AddRange(ValidateAssignments(wellBores[index], catalogs, index));
            if (assignmentErrors.Count != 0)
            {
                transaction.Rollback();
                return Failure(WellBoreArchitectureBatchRestoreFailureKind.InvalidRequest, "invalid_wellbore_architecture_assignments",
                    "One or more restored WellBoreArchitectures contain invalid identity or feature assignments. No changes were made.",
                    assignmentErrors);
            }

            List<PreparedWellBoreArchitecture> prepared = PrepareWellBoreArchitectures(wellBores);
            List<bool> exists = prepared.Select(value => RowExists(connection, transaction, value.ID)).ToList();
            if (request.ConflictPolicy == WellBoreArchitectureBatchRestoreConflictPolicy.FailIfExists)
            {
                List<WellBoreArchitectureBatchError> conflicts = prepared.Select((value, index) => (value, index))
                    .Where(value => exists[value.index])
                    .Select(value => Error(value.index, "Document.WellBoreArchitectures", "well_already_exists",
                        $"A stored WellBoreArchitecture already has UUID '{value.value.ID}'."))
                    .ToList();
                if (conflicts.Count != 0)
                {
                    transaction.Rollback();
                    return Failure(WellBoreArchitectureBatchRestoreFailureKind.Conflict, "well_restore_conflict",
                        "One or more WellBoreArchitecture UUIDs already exist. No changes were made.", conflicts);
                }
            }

            catalogs.Save(connection, transaction);
            SaveWellBoreArchitectures(connection, transaction, prepared, request.ConflictPolicy);
            transaction.Commit();
            return new WellBoreArchitectureBatchRestoreOutcome
            {
                Response = new WellBoreArchitectureBatchRestoreResponse
                {
                    RestoredAtUtc = restoredAtUtc.ToUniversalTime(),
                    CreatedCount = exists.Count(value => !value),
                    ReplacedCount = exists.Count(value => value),
                    CreatedCatalogDefinitionCount = createdDefinitions,
                    CreatedCatalogOptionCount = createdOptions,
                    CatalogMappings = mappings,
                    WellBoreArchitectureIDs = prepared.Select(value => value.ID).ToList()
                }
            };
        }
        catch (Exception exception) when (exception is SqliteException or JsonException or InvalidOperationException or KeyNotFoundException)
        {
            try { transaction.Rollback(); } catch (InvalidOperationException) { }
            return StorageFailure($"The WellBoreArchitecture database rejected the batch. No changes were committed. {exception.Message}");
        }
    }

    public static WellBoreArchitectureBatchRestoreOutcome StorageFailure(string message) => Failure(
        WellBoreArchitectureBatchRestoreFailureKind.StorageFailure, "well_restore_failed", message,
        [Error(null, "Document.WellBoreArchitectures", "storage_failure", "The complete restore transaction was rolled back.")]);

    public static List<WellBoreArchitectureBatchError> ValidateRequest(WellBoreArchitectureBatchRestoreRequest? request)
    {
        if (request == null) return [Error(null, "Request", "required", "A batch-restore request is required.")];
        List<WellBoreArchitectureBatchError> errors = [];
        if (request.ConflictPolicy is not WellBoreArchitectureBatchRestoreConflictPolicy.FailIfExists and not WellBoreArchitectureBatchRestoreConflictPolicy.ReplaceExisting)
            errors.Add(Error(null, "ConflictPolicy", "invalid_conflict_policy", "ConflictPolicy must be FailIfExists or ReplaceExisting."));
        if (request.CatalogPolicy is not WellBoreArchitectureBatchCatalogRestorePolicy.MapExisting and not WellBoreArchitectureBatchCatalogRestorePolicy.MapOrCreateMissing)
            errors.Add(Error(null, "CatalogPolicy", "invalid_catalog_policy", "CatalogPolicy must be MapExisting or MapOrCreateMissing."));
        WellBoreArchitectureBatchExportDocument? document = request.Document;
        if (document == null)
        {
            errors.Add(Error(null, "Document", "required", "A batch-export document is required."));
            return errors;
        }
        if (document.FormatIdentifier != WellBoreArchitectureBatchExportDocument.CurrentFormatIdentifier)
            errors.Add(Error(null, "Document.FormatIdentifier", "unsupported_format", $"FormatIdentifier must be '{WellBoreArchitectureBatchExportDocument.CurrentFormatIdentifier}'."));
        if (document.SchemaVersion != WellBoreArchitectureBatchExportDocument.CurrentSchemaVersion)
            errors.Add(Error(null, "Document.SchemaVersion", "unsupported_schema_version", $"SchemaVersion must be {WellBoreArchitectureBatchExportDocument.CurrentSchemaVersion}."));
        if (document.ExportedAtUtc == default || document.ExportedAtUtc.Offset != TimeSpan.Zero)
            errors.Add(Error(null, "Document.ExportedAtUtc", "invalid_export_timestamp", "ExportedAtUtc must be a non-default UTC timestamp."));
        ValidateDependencies(document.CatalogDependencies, errors);
        if (document.WellBoreArchitectures == null || document.WellBoreArchitectures.Count == 0)
        {
            errors.Add(Error(null, "Document.WellBoreArchitectures", "required", "At least one WellBoreArchitecture is required for restore."));
            return errors;
        }
        ValidateReferences(document.WellBoreArchitectures, document.CatalogDependencies, errors);
        Dictionary<Guid, int> positions = [];
        for (int index = 0; index < document.WellBoreArchitectures.Count; index++)
        {
            WellBoreArchitectureModel? wellBore = document.WellBoreArchitectures[index];
            Guid? id = wellBore?.MetaInfo?.ID;
            if (wellBore == null) errors.Add(Error(index, "Document.WellBoreArchitectures", "null_well", "A restored WellBoreArchitecture must not be null."));
            else if (id == null || id == Guid.Empty) errors.Add(Error(index, "Document.WellBoreArchitectures.MetaInfo.ID", "empty_uuid", "Every restored WellBoreArchitecture must have a non-empty UUID."));
            else if (positions.TryGetValue(id.Value, out int first)) errors.Add(Error(index, "Document.WellBoreArchitectures.MetaInfo.ID", "duplicate_uuid", $"WellBoreArchitecture UUID '{id}' duplicates position {first}."));
            else positions.Add(id.Value, index);
            if (wellBore?.WellBoreID == Guid.Empty) errors.Add(Error(index, "Document.WellBoreArchitectures.WellBoreID", "empty_uuid", "WellBoreID must be omitted or a non-empty UUID."));
        }
        return errors;
    }

    private static void ValidateDependencies(WellBoreArchitectureBatchCatalogDependencies? dependencies, List<WellBoreArchitectureBatchError> errors)
    {
        if (dependencies == null)
        {
            errors.Add(Error(null, "Document.CatalogDependencies", "required", "CatalogDependencies is required."));
            return;
        }
        HashSet<Guid> ids = [];
        void Check(Guid id, string? name, string property)
        {
            if (id == Guid.Empty) errors.Add(Error(null, property, "empty_uuid", "Catalog UUIDs must be non-empty."));
            else if (!ids.Add(id)) errors.Add(Error(null, property, "duplicate_uuid", $"Catalog UUID '{id}' occurs more than once."));
            if (string.IsNullOrWhiteSpace(name)) errors.Add(Error(null, property + ".Name", "required", "Catalog names must not be empty."));
        }
        foreach (WellBoreArchitectureIdentity? identity in dependencies.Identities ?? [])
            Check(identity?.MetaInfo?.ID ?? Guid.Empty, identity?.Name, "Document.CatalogDependencies.Identities");
        foreach (WellBoreArchitectureFeatureCategory? category in dependencies.FeatureCategories ?? [])
        {
            Check(category?.MetaInfo?.ID ?? Guid.Empty, category?.Name, "Document.CatalogDependencies.FeatureCategories");
            foreach (WellBoreArchitectureFeatureOption? option in category?.Options ?? [])
                Check(option?.ID ?? Guid.Empty, option?.Name, "Document.CatalogDependencies.FeatureCategories.Options");
        }
    }

    private static void ValidateReferences(List<WellBoreArchitectureModel> wellBores, WellBoreArchitectureBatchCatalogDependencies? dependencies,
        List<WellBoreArchitectureBatchError> errors)
    {
        if (dependencies == null) return;
        HashSet<Guid> identityIds = (dependencies.Identities ?? [])
            .Where(value => value?.MetaInfo?.ID is Guid id && id != Guid.Empty)
            .Select(value => value.MetaInfo!.ID).ToHashSet();
        Dictionary<Guid, HashSet<Guid>> categoryOptions = [];
        foreach (WellBoreArchitectureFeatureCategory? category in dependencies.FeatureCategories ?? [])
        {
            if (category?.MetaInfo?.ID is not Guid categoryId || categoryId == Guid.Empty || categoryOptions.ContainsKey(categoryId))
                continue;
            categoryOptions.Add(categoryId, (category.Options ?? []).Where(option => option != null).Select(option => option.ID).ToHashSet());
        }
        for (int index = 0; index < wellBores.Count; index++)
        {
            foreach (WellBoreArchitectureIdentityAssignment? assignment in wellBores[index]?.WellBoreArchitectureIdentityAssignments ?? [])
            {
                if (assignment?.IdentityID is not Guid id || id == Guid.Empty || !identityIds.Contains(id))
                    errors.Add(Error(index, "Document.WellBoreArchitectures.WellBoreArchitectureIdentityAssignments.IdentityID", "catalog_dependency_missing", $"Referenced identity '{assignment?.IdentityID}' is absent from CatalogDependencies."));
            }
            foreach (WellBoreArchitectureFeatureAssignment? assignment in wellBores[index]?.WellBoreArchitectureFeatureAssignments ?? [])
            {
                if (assignment?.FeatureCategoryID is not Guid categoryId || !categoryOptions.TryGetValue(categoryId, out HashSet<Guid>? options))
                    errors.Add(Error(index, "Document.WellBoreArchitectures.WellBoreArchitectureFeatureAssignments.FeatureCategoryID", "catalog_dependency_missing", $"Referenced category '{assignment?.FeatureCategoryID}' is absent from CatalogDependencies."));
                else if (assignment.FeatureOptionID is not Guid optionId || !options.Contains(optionId))
                    errors.Add(Error(index, "Document.WellBoreArchitectures.WellBoreArchitectureFeatureAssignments.FeatureOptionID", "catalog_dependency_missing", $"Referenced option '{assignment.FeatureOptionID}' is absent from category '{categoryId}'."));
            }
        }
    }

    private static void ResolveDependencies(WellBoreArchitectureBatchCatalogDependencies dependencies, CatalogState local,
        bool createMissing, bool allowNormalizedNameMapping, List<WellBoreArchitectureBatchCatalogMapping> mappings, List<WellBoreArchitectureBatchError> errors,
        DateTimeOffset now, ref int createdDefinitions, ref int createdOptions)
    {
        foreach (WellBoreArchitectureIdentity source in dependencies.Identities ?? [])
        {
            Guid sourceId = source.MetaInfo!.ID;
            WellBoreArchitectureIdentity? target = ResolveFlat(sourceId, source.Name, local.Identities, createMissing, allowNormalizedNameMapping, errors);
            bool created = false;
            if (target == null && createMissing && !HasErrorFor(errors, sourceId))
            {
                target = new WellBoreArchitectureIdentity { MetaInfo = new MetaInfo { ID = sourceId }, Name = source.Name,
                    CreationDate = now, LastModificationDate = now };
                local.Identities.Add(target); local.DirtyIdentities.Add(target); createdDefinitions++; created = true;
            }
            if (target != null) AddMapping(mappings, "Identity", source.Name, sourceId, target.MetaInfo!.ID,
                created ? "created_preserving_uuid" : sourceId == target.MetaInfo.ID ? "exact_uuid" : "normalized_name_with_consent");
        }
        foreach (WellBoreArchitectureFeatureCategory source in dependencies.FeatureCategories ?? [])
            ResolveCategory(source, local, createMissing, allowNormalizedNameMapping, mappings, errors, now, ref createdDefinitions, ref createdOptions);
    }

    private static void ResolveCategory(WellBoreArchitectureFeatureCategory source, CatalogState local, bool createMissing, bool allowNormalizedNameMapping,
        List<WellBoreArchitectureBatchCatalogMapping> mappings, List<WellBoreArchitectureBatchError> errors, DateTimeOffset now,
        ref int createdDefinitions, ref int createdOptions)
    {
        Guid sourceId = source.MetaInfo!.ID;
        WellBoreArchitectureFeatureCategory? target = local.Features.FirstOrDefault(value => value.MetaInfo!.ID == sourceId);
        bool created = false;
        if (target != null && (!SameName(target.Name, source.Name) || target.IsExclusive != source.IsExclusive || target.HasValidityPeriod != source.HasValidityPeriod))
        {
            AddSemanticConflict(errors, "feature category", sourceId, source.Name); return;
        }
        if (target == null)
        {
            List<WellBoreArchitectureFeatureCategory> matches = local.Features.Where(value => SameName(value.Name, source.Name)).ToList();
            if (matches.Count != 0 && !allowNormalizedNameMapping) { AddMappingConsentRequired(errors, "feature category", sourceId, source.Name); return; }
            if (matches.Count > 1) { AddAmbiguous(errors, "feature category", sourceId, source.Name); return; }
            if (matches.Count == 1)
            {
                target = matches[0];
                if (target.IsExclusive != source.IsExclusive || target.HasValidityPeriod != source.HasValidityPeriod)
                { AddSemanticConflict(errors, "feature category", sourceId, source.Name); return; }
            }
            else if (createMissing)
            {
                target = new WellBoreArchitectureFeatureCategory { MetaInfo = new MetaInfo { ID = sourceId }, Name = source.Name,
                    IsExclusive = source.IsExclusive, HasValidityPeriod = source.HasValidityPeriod, Options = [],
                    CreationDate = now, LastModificationDate = now };
                local.Features.Add(target); local.DirtyFeatures.Add(target); createdDefinitions++; created = true;
            }
            else { AddMissing(errors, "feature category", sourceId, source.Name); return; }
        }
        AddMapping(mappings, "FeatureCategory", source.Name, sourceId, target.MetaInfo!.ID,
            created ? "created_preserving_uuid" : sourceId == target.MetaInfo.ID ? "exact_uuid" : "normalized_name_with_consent");
        foreach (WellBoreArchitectureFeatureOption sourceOption in source.Options ?? [])
        {
            WellBoreArchitectureFeatureOption? targetOption = (target.Options ?? []).FirstOrDefault(value => value.ID == sourceOption.ID);
            bool optionCreated = false;
            if (targetOption != null && !SameName(targetOption.Name, sourceOption.Name))
            { AddSemanticConflict(errors, "feature option", sourceOption.ID, sourceOption.Name); continue; }
            if (targetOption == null)
            {
                List<WellBoreArchitectureFeatureOption> matches = (target.Options ?? []).Where(value => SameName(value.Name, sourceOption.Name)).ToList();
                if (matches.Count != 0 && !allowNormalizedNameMapping) { AddMappingConsentRequired(errors, "feature option", sourceOption.ID, sourceOption.Name); continue; }
                if (matches.Count > 1) { AddAmbiguous(errors, "feature option", sourceOption.ID, sourceOption.Name); continue; }
                if (matches.Count == 1) targetOption = matches[0];
                else if (createMissing)
                {
                    targetOption = new WellBoreArchitectureFeatureOption { ID = sourceOption.ID, Name = sourceOption.Name };
                    target.Options ??= []; target.Options.Add(targetOption); target.LastModificationDate = now;
                    local.DirtyFeatures.Add(target); createdOptions++; optionCreated = true;
                }
                else { AddMissing(errors, "feature option", sourceOption.ID, sourceOption.Name); continue; }
            }
            AddMapping(mappings, "FeatureOption", sourceOption.Name, sourceOption.ID, targetOption.ID,
                optionCreated ? "created_preserving_uuid" : sourceOption.ID == targetOption.ID ? "exact_uuid" : "normalized_name_with_consent");
        }
    }

    private static WellBoreArchitectureIdentity? ResolveFlat(Guid sourceId, string? sourceName, List<WellBoreArchitectureIdentity> local,
        bool createMissing, bool allowNormalizedNameMapping, List<WellBoreArchitectureBatchError> errors)
    {
        WellBoreArchitectureIdentity? exact = local.FirstOrDefault(value => value.MetaInfo!.ID == sourceId);
        if (exact != null)
        {
            if (!SameName(exact.Name, sourceName)) AddSemanticConflict(errors, "identity", sourceId, sourceName);
            return HasErrorFor(errors, sourceId) ? null : exact;
        }
        List<WellBoreArchitectureIdentity> matches = local.Where(value => SameName(value.Name, sourceName)).ToList();
        if (matches.Count != 0 && !allowNormalizedNameMapping)
        {
            AddMappingConsentRequired(errors, "identity", sourceId, sourceName);
            return null;
        }
        if (matches.Count == 1) return matches[0];
        if (matches.Count > 1) AddAmbiguous(errors, "identity", sourceId, sourceName);
        else if (!createMissing) AddMissing(errors, "identity", sourceId, sourceName);
        return null;
    }

    private static void RewriteReferences(List<WellBoreArchitectureModel> wellBores, List<WellBoreArchitectureBatchCatalogMapping> mappings)
    {
        Dictionary<Guid, Guid> map = mappings.ToDictionary(value => value.SourceID, value => value.LocalID);
        foreach (WellBoreArchitectureModel wellBore in wellBores)
        {
            foreach (WellBoreArchitectureIdentityAssignment assignment in wellBore.WellBoreArchitectureIdentityAssignments ?? [])
                if (assignment.IdentityID is Guid id) assignment.IdentityID = map[id];
            foreach (WellBoreArchitectureFeatureAssignment assignment in wellBore.WellBoreArchitectureFeatureAssignments ?? [])
            {
                if (assignment.FeatureCategoryID is Guid categoryId) assignment.FeatureCategoryID = map[categoryId];
                if (assignment.FeatureOptionID is Guid optionId) assignment.FeatureOptionID = map[optionId];
            }
        }
    }

    private static List<WellBoreArchitectureBatchError> ValidateAssignments(
        WellBoreArchitectureModel architecture, CatalogState catalogs, int index)
    {
        List<WellBoreArchitectureBatchError> errors = [];
        architecture.WellBoreArchitectureIdentityAssignments ??= [];
        architecture.WellBoreArchitectureFeatureAssignments ??= [];
        string root = $"Document.WellBoreArchitectures[{index}]";

        HashSet<Guid> assignmentIds = [];
        foreach (WellBoreArchitectureIdentityAssignment assignment in architecture.WellBoreArchitectureIdentityAssignments)
        {
            if (assignment.ID == Guid.Empty)
                errors.Add(Error(index, $"{root}.WellBoreArchitectureIdentityAssignments.ID", "empty_uuid", "Identity assignment UUIDs must be non-empty."));
            else if (!assignmentIds.Add(assignment.ID))
                errors.Add(Error(index, $"{root}.WellBoreArchitectureIdentityAssignments.ID", "duplicate_uuid", $"Assignment UUID '{assignment.ID}' occurs more than once."));
        }
        foreach (WellBoreArchitectureFeatureAssignment assignment in architecture.WellBoreArchitectureFeatureAssignments)
        {
            if (assignment.ID == Guid.Empty)
                errors.Add(Error(index, $"{root}.WellBoreArchitectureFeatureAssignments.ID", "empty_uuid", "Feature assignment UUIDs must be non-empty."));
            else if (!assignmentIds.Add(assignment.ID))
                errors.Add(Error(index, $"{root}.WellBoreArchitectureFeatureAssignments.ID", "duplicate_uuid", $"Assignment UUID '{assignment.ID}' occurs more than once."));
        }

        HashSet<Guid> identityIds = catalogs.Identities
            .Where(value => value.MetaInfo?.ID is Guid id && id != Guid.Empty)
            .Select(value => value.MetaInfo!.ID).ToHashSet();
        foreach (WellBoreArchitectureIdentityAssignment assignment in architecture.WellBoreArchitectureIdentityAssignments)
            if (assignment.IdentityID is not Guid id || !identityIds.Contains(id))
                errors.Add(Error(index, $"{root}.WellBoreArchitectureIdentityAssignments.IdentityID", "identity_not_found", $"Identity '{assignment.IdentityID}' does not exist."));

        Dictionary<Guid, WellBoreArchitectureFeatureCategory> categories = catalogs.Features
            .Where(value => value.MetaInfo?.ID is Guid id && id != Guid.Empty)
            .GroupBy(value => value.MetaInfo!.ID).ToDictionary(group => group.Key, group => group.First());
        foreach (WellBoreArchitectureFeatureAssignment assignment in architecture.WellBoreArchitectureFeatureAssignments)
        {
            if (assignment.FeatureCategoryID is not Guid categoryId || !categories.TryGetValue(categoryId, out WellBoreArchitectureFeatureCategory? category))
            {
                errors.Add(Error(index, $"{root}.WellBoreArchitectureFeatureAssignments.FeatureCategoryID", "feature_category_not_found", $"Feature category '{assignment.FeatureCategoryID}' does not exist."));
                continue;
            }
            if (assignment.FeatureOptionID is not Guid optionId || category.Options?.Any(option => option.ID == optionId) != true)
                errors.Add(Error(index, $"{root}.WellBoreArchitectureFeatureAssignments.FeatureOptionID", "feature_option_not_found", $"Feature option '{assignment.FeatureOptionID}' does not belong to category '{categoryId}'."));
            if (!category.HasValidityPeriod && (assignment.FromDate != null || assignment.ToDate != null))
                errors.Add(Error(index, $"{root}.WellBoreArchitectureFeatureAssignments", "validity_not_supported", $"Category '{category.Name}' does not support validity dates."));
            if (assignment.FromDate > assignment.ToDate)
                errors.Add(Error(index, $"{root}.WellBoreArchitectureFeatureAssignments", "invalid_validity_period", "FromDate must not be later than ToDate."));
        }
        foreach (WellBoreArchitectureFeatureCategory category in categories.Values.Where(value => value.IsExclusive))
        {
            List<WellBoreArchitectureFeatureAssignment> assignments = architecture.WellBoreArchitectureFeatureAssignments
                .Where(value => value.FeatureCategoryID == category.MetaInfo!.ID).ToList();
            for (int left = 0; left < assignments.Count; left++)
                for (int right = left + 1; right < assignments.Count; right++)
                    if (PeriodsOverlap(assignments[left], assignments[right]))
                        errors.Add(Error(index, $"{root}.WellBoreArchitectureFeatureAssignments", "exclusive_period_overlap", $"Assignments in exclusive category '{category.Name}' have overlapping validity periods."));
        }
        return errors;
    }

    private static bool PeriodsOverlap(WellBoreArchitectureFeatureAssignment left, WellBoreArchitectureFeatureAssignment right) =>
        (left.ToDate == null || right.FromDate == null || left.ToDate >= right.FromDate) &&
        (right.ToDate == null || left.FromDate == null || right.ToDate >= left.FromDate);

    private static List<WellBoreArchitectureModel> CloneWellBoreArchitectures(List<WellBoreArchitectureModel> values) => JsonSerializer.Deserialize<List<WellBoreArchitectureModel>>(
        JsonSerializer.Serialize(values, JsonSettings.Options), JsonSettings.Options) ?? throw new JsonException("WellBoreArchitectures could not be cloned.");
    private static List<PreparedWellBoreArchitecture> PrepareWellBoreArchitectures(List<WellBoreArchitectureModel> values) => values.Select(value => new PreparedWellBoreArchitecture(
        value.MetaInfo!.ID, JsonSerializer.Serialize(value.MetaInfo, JsonSettings.Options),
        value.Name, value.Description,
        value.CreationDate?.ToString(Managers.SqlConnectionManager.DATE_TIME_FORMAT),
        value.LastModificationDate?.ToString(Managers.SqlConnectionManager.DATE_TIME_FORMAT),
        JsonSerializer.Serialize(value, JsonSettings.Options))).ToList();
    private static bool RowExists(SqliteConnection connection, SqliteTransaction transaction, Guid id)
    { using SqliteCommand command = connection.CreateCommand(); command.Transaction = transaction; command.CommandText = "SELECT COUNT(*) FROM WellBoreArchitectureTable WHERE ID=$id"; command.Parameters.AddWithValue("$id", id.ToString()); return Convert.ToInt64(command.ExecuteScalar()) != 0; }
    private static void SaveWellBoreArchitectures(SqliteConnection connection, SqliteTransaction transaction,
        List<PreparedWellBoreArchitecture> wellBores, WellBoreArchitectureBatchRestoreConflictPolicy policy)
    {
        foreach (PreparedWellBoreArchitecture wellBore in wellBores)
        {
            using SqliteCommand command = connection.CreateCommand(); command.Transaction = transaction;
            command.CommandText = policy == WellBoreArchitectureBatchRestoreConflictPolicy.ReplaceExisting
                ? "INSERT INTO WellBoreArchitectureTable (ID,MetaInfo,Name,Description,CreationDate,LastModificationDate,WellBoreArchitecture) VALUES ($id,$meta,$name,$description,$created,$modified,$doc) ON CONFLICT(ID) DO UPDATE SET MetaInfo=excluded.MetaInfo,Name=excluded.Name,Description=excluded.Description,CreationDate=excluded.CreationDate,LastModificationDate=excluded.LastModificationDate,WellBoreArchitecture=excluded.WellBoreArchitecture"
                : "INSERT INTO WellBoreArchitectureTable (ID,MetaInfo,Name,Description,CreationDate,LastModificationDate,WellBoreArchitecture) VALUES ($id,$meta,$name,$description,$created,$modified,$doc)";
            command.Parameters.AddWithValue("$id", wellBore.ID.ToString());
            command.Parameters.AddWithValue("$meta", wellBore.MetaInfoJson);
            command.Parameters.AddWithValue("$name", (object?)wellBore.Name ?? DBNull.Value);
            command.Parameters.AddWithValue("$description", (object?)wellBore.Description ?? DBNull.Value);
            command.Parameters.AddWithValue("$created", (object?)wellBore.CreationDate ?? DBNull.Value);
            command.Parameters.AddWithValue("$modified", (object?)wellBore.LastModificationDate ?? DBNull.Value);
            command.Parameters.AddWithValue("$doc", wellBore.WellBoreArchitectureJson);
            command.ExecuteNonQuery();
        }
    }

    private static string Normalize(string? value) => string.Join(' ', (value ?? "").Normalize(NormalizationForm.FormKC)
        .Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries)).ToUpperInvariant();
    private static bool SameName(string? left, string? right) => Normalize(left) == Normalize(right);
    private static bool HasErrorFor(List<WellBoreArchitectureBatchError> errors, Guid id) => errors.Any(error => error.Message.Contains(id.ToString(), StringComparison.OrdinalIgnoreCase));
    private static void AddMissing(List<WellBoreArchitectureBatchError> errors, string kind, Guid id, string? name) => errors.Add(Error(null, $"Document.CatalogDependencies[{id}]", "catalog_definition_missing", $"No compatible local {kind} exists for '{name}' ({id}), and creation is disabled."));
    private static void AddAmbiguous(List<WellBoreArchitectureBatchError> errors, string kind, Guid id, string? name) => errors.Add(Error(null, $"Document.CatalogDependencies[{id}]", "ambiguous_catalog_match", $"More than one local {kind} has normalized name '{name}' for source UUID '{id}'."));
    private static void AddSemanticConflict(List<WellBoreArchitectureBatchError> errors, string kind, Guid id, string? name) => errors.Add(Error(null, $"Document.CatalogDependencies[{id}]", "catalog_semantic_conflict", $"The local {kind} corresponding to '{name}' ({id}) has incompatible semantics."));
    private static void AddMappingConsentRequired(List<WellBoreArchitectureBatchError> errors, string kind, Guid id, string? name) => errors.Add(Error(null,
        $"Document.CatalogDependencies[{id}]", "normalized_name_mapping_requires_consent",
        $"A local {kind} named '{name}' has a different UUID. Set AllowNormalizedNameMapping=true only after confirming that the definitions are semantically identical."));
    private static void AddMapping(List<WellBoreArchitectureBatchCatalogMapping> mappings, string catalog, string? name, Guid source, Guid local, string resolution) => mappings.Add(new() { Catalog = catalog, Name = name ?? "", SourceID = source, LocalID = local, Resolution = resolution });
    private static WellBoreArchitectureBatchRestoreOutcome Failure(WellBoreArchitectureBatchRestoreFailureKind kind, string error, string message, List<WellBoreArchitectureBatchError> errors) => new() { FailureKind = kind, Error = new() { Error = error, Message = message, Errors = errors } };
    private static WellBoreArchitectureBatchError Error(int? index, string property, string code, string message) => new() { PositionIndex = index, Property = property, Code = code, Message = message };
    private sealed record PreparedWellBoreArchitecture(Guid ID, string MetaInfoJson, string? Name, string? Description,
        string? CreationDate, string? LastModificationDate, string WellBoreArchitectureJson);

    private sealed class CatalogState
    {
        public List<WellBoreArchitectureIdentity> Identities { get; } = [];
        public List<WellBoreArchitectureFeatureCategory> Features { get; } = [];
        public HashSet<WellBoreArchitectureIdentity> DirtyIdentities { get; } = [];
        public HashSet<WellBoreArchitectureFeatureCategory> DirtyFeatures { get; } = [];

        public static CatalogState Load(SqliteConnection connection, SqliteTransaction transaction)
        {
            CatalogState state = new();
            state.Identities.AddRange(Read<WellBoreArchitectureIdentity>(connection, transaction, "WellBoreArchitectureIdentityTable", "WellBoreArchitectureIdentity"));
            state.Features.AddRange(Read<WellBoreArchitectureFeatureCategory>(connection, transaction, "WellBoreArchitectureFeatureCategoryTable", "WellBoreArchitectureFeatureCategory"));
            return state;
        }
        private static List<T> Read<T>(SqliteConnection connection, SqliteTransaction transaction, string table, string column)
        {
            using SqliteCommand command = connection.CreateCommand(); command.Transaction = transaction;
            command.CommandText = $"SELECT {column} FROM {table}";
            using SqliteDataReader reader = command.ExecuteReader(); List<T> result = [];
            while (reader.Read()) result.Add(JsonSerializer.Deserialize<T>(reader.GetString(0), JsonSettings.Options) ?? throw new JsonException($"Invalid {table} document."));
            return result;
        }
        public void Save(SqliteConnection connection, SqliteTransaction transaction)
        {
            foreach (WellBoreArchitectureIdentity value in DirtyIdentities)
            {
                using SqliteCommand command = connection.CreateCommand(); command.Transaction = transaction;
                command.CommandText = "INSERT INTO WellBoreArchitectureIdentityTable (ID,MetaInfo,Name,CreationDate,LastModificationDate,WellBoreArchitectureIdentity) VALUES ($id,$meta,$name,$created,$modified,$doc)";
                AddCommon(command, value.MetaInfo!, value.Name, value.CreationDate, value.LastModificationDate, value); command.ExecuteNonQuery();
            }
            foreach (WellBoreArchitectureFeatureCategory value in DirtyFeatures)
            {
                using SqliteCommand command = connection.CreateCommand(); command.Transaction = transaction;
                command.CommandText = "INSERT INTO WellBoreArchitectureFeatureCategoryTable (ID,MetaInfo,Name,IsExclusive,HasValidityPeriod,CreationDate,LastModificationDate,WellBoreArchitectureFeatureCategory) VALUES ($id,$meta,$name,$exclusive,$validity,$created,$modified,$doc) ON CONFLICT(ID) DO UPDATE SET MetaInfo=excluded.MetaInfo,Name=excluded.Name,IsExclusive=excluded.IsExclusive,HasValidityPeriod=excluded.HasValidityPeriod,CreationDate=excluded.CreationDate,LastModificationDate=excluded.LastModificationDate,WellBoreArchitectureFeatureCategory=excluded.WellBoreArchitectureFeatureCategory";
                AddCommon(command, value.MetaInfo!, value.Name, value.CreationDate, value.LastModificationDate, value);
                command.Parameters.AddWithValue("$exclusive", value.IsExclusive ? 1 : 0);
                command.Parameters.AddWithValue("$validity", value.HasValidityPeriod ? 1 : 0); command.ExecuteNonQuery();
            }
        }
        private static void AddCommon(SqliteCommand command, MetaInfo metaInfo, string? name,
            DateTimeOffset? created, DateTimeOffset? modified, object document)
        {
            command.Parameters.AddWithValue("$id", metaInfo.ID.ToString());
            command.Parameters.AddWithValue("$meta", JsonSerializer.Serialize(metaInfo, JsonSettings.Options));
            command.Parameters.AddWithValue("$name", name ?? "");
            command.Parameters.AddWithValue("$created", created?.ToString(Managers.SqlConnectionManager.DATE_TIME_FORMAT) ?? "");
            command.Parameters.AddWithValue("$modified", modified?.ToString(Managers.SqlConnectionManager.DATE_TIME_FORMAT) ?? "");
            command.Parameters.AddWithValue("$doc", JsonSerializer.Serialize(document, JsonSettings.Options));
        }
    }
}
