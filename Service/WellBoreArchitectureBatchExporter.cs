using OSDC.Drilling.WellBoreArchitecture.Model;
using System;
using System.Collections.Generic;
using System.Linq;

namespace OSDC.Drilling.WellBoreArchitecture.Service;

public enum WellBoreArchitectureBatchExportFailureKind { None, InvalidRequest, WellNotFound, StorageFailure }

public sealed class WellBoreArchitectureBatchExportOutcome
{
    public WellBoreArchitectureBatchExportDocument? Document { get; init; }
    public WellBoreArchitectureBatchErrorEnvelope? Error { get; init; }
    public WellBoreArchitectureBatchExportFailureKind FailureKind { get; init; }
    public bool IsSuccess => Document != null && FailureKind == WellBoreArchitectureBatchExportFailureKind.None;
}

public static class WellBoreArchitectureBatchExporter
{
    public static WellBoreArchitectureBatchExportOutcome Create(WellBoreArchitectureBatchExportRequest? request,
        IEnumerable<Model.WellBoreArchitecture?> snapshot, DateTimeOffset exportedAtUtc,
        IEnumerable<WellBoreArchitectureIdentity> identities, IEnumerable<WellBoreArchitectureFeatureCategory> categories)
    {
        List<WellBoreArchitectureBatchError> errors = ValidateRequest(request);
        if (errors.Count != 0) return Failure(WellBoreArchitectureBatchExportFailureKind.InvalidRequest,
            "invalid_batch_export_request", "The WellBoreArchitecture batch-export request is invalid.", errors);

        Dictionary<Guid, Model.WellBoreArchitecture> byId = [];
        int position = 0;
        foreach (Model.WellBoreArchitecture? wellBore in snapshot)
        {
            Guid? id = wellBore?.MetaInfo?.ID;
            if (wellBore == null || id == null || id == Guid.Empty || !byId.TryAdd(id.Value, wellBore))
                return Failure(WellBoreArchitectureBatchExportFailureKind.StorageFailure, "well_export_failed",
                    "A stored WellBoreArchitecture could not be represented in the export.",
                    [Error(position, "WellBoreArchitectures", "invalid_stored_well", "A stored WellBoreArchitecture is null, has no UUID, or duplicates another UUID.")]);
            position++;
        }

        List<Model.WellBoreArchitecture> selected;
        if (request!.Scope == WellBoreArchitectureBatchExportScope.All)
            selected = byId.OrderBy(pair => pair.Key).Select(pair => pair.Value).ToList();
        else
        {
            selected = [];
            for (int index = 0; index < request.WellBoreArchitectureIDs!.Count; index++)
            {
                Guid id = request.WellBoreArchitectureIDs[index];
                if (byId.TryGetValue(id, out Model.WellBoreArchitecture? wellBore)) selected.Add(wellBore);
                else errors.Add(Error(index, "WellBoreArchitectureIDs", "well_not_found", $"No stored WellBoreArchitecture has UUID '{id}'."));
            }
            if (errors.Count != 0) return Failure(WellBoreArchitectureBatchExportFailureKind.WellNotFound,
                "well_not_found", "One or more selected WellBoreArchitectures do not exist.", errors);
        }

        WellBoreArchitectureBatchCatalogDependencies dependencies = BuildDependencies(selected, identities, categories, errors);
        if (errors.Count != 0) return Failure(WellBoreArchitectureBatchExportFailureKind.StorageFailure,
            "well_export_dependency_missing", "The export could not include every referenced local catalog definition.", errors);

        return new WellBoreArchitectureBatchExportOutcome
        {
            Document = new WellBoreArchitectureBatchExportDocument
            {
                ExportedAtUtc = exportedAtUtc.ToUniversalTime(),
                CatalogDependencies = dependencies,
                WellBoreArchitectures = selected
            }
        };
    }

    public static WellBoreArchitectureBatchExportOutcome StorageFailure(string message) => Failure(
        WellBoreArchitectureBatchExportFailureKind.StorageFailure, "well_export_failed", message,
        [Error(null, "Document", "storage_failure", "The export snapshot could not be produced.")]);

    private static WellBoreArchitectureBatchCatalogDependencies BuildDependencies(IReadOnlyList<Model.WellBoreArchitecture> wellBores,
        IEnumerable<WellBoreArchitectureIdentity> identities, IEnumerable<WellBoreArchitectureFeatureCategory> categories,
        List<WellBoreArchitectureBatchError> errors)
    {
        Dictionary<Guid, WellBoreArchitectureIdentity> identityIndex = identities
            .Where(value => value?.MetaInfo?.ID is Guid id && id != Guid.Empty)
            .GroupBy(value => value.MetaInfo!.ID).ToDictionary(group => group.Key, group => group.First());
        Dictionary<Guid, WellBoreArchitectureFeatureCategory> categoryIndex = categories
            .Where(value => value?.MetaInfo?.ID is Guid id && id != Guid.Empty)
            .GroupBy(value => value.MetaInfo!.ID).ToDictionary(group => group.Key, group => group.First());
        HashSet<Guid> identityIds = [];
        Dictionary<Guid, HashSet<Guid>> optionIdsByCategory = [];

        for (int index = 0; index < wellBores.Count; index++)
        {
            foreach (WellBoreArchitectureIdentityAssignment? assignment in wellBores[index].WellBoreArchitectureIdentityAssignments ?? [])
            {
                if (assignment?.IdentityID is Guid id && id != Guid.Empty) identityIds.Add(id);
                else errors.Add(Error(index, "WellBoreArchitectures.WellBoreArchitectureIdentityAssignments.IdentityID", "invalid_catalog_reference", "Identity references must be non-empty UUIDs."));
            }
            foreach (WellBoreArchitectureFeatureAssignment? assignment in wellBores[index].WellBoreArchitectureFeatureAssignments ?? [])
            {
                if (assignment?.FeatureCategoryID is not Guid categoryId || categoryId == Guid.Empty ||
                    assignment.FeatureOptionID is not Guid optionId || optionId == Guid.Empty)
                {
                    errors.Add(Error(index, "WellBoreArchitectures.WellBoreArchitectureFeatureAssignments", "invalid_catalog_reference", "Feature category and option references must be non-empty UUIDs."));
                    continue;
                }
                if (!optionIdsByCategory.TryGetValue(categoryId, out HashSet<Guid>? optionIds))
                    optionIdsByCategory.Add(categoryId, optionIds = []);
                optionIds.Add(optionId);
            }
        }

        WellBoreArchitectureBatchCatalogDependencies result = new();
        foreach (Guid id in identityIds.Order())
        {
            if (identityIndex.TryGetValue(id, out WellBoreArchitectureIdentity? identity)) result.Identities.Add(identity);
            else errors.Add(Error(null, "CatalogDependencies.Identities", "referenced_definition_missing", $"Referenced identity '{id}' does not exist."));
        }
        foreach ((Guid categoryId, HashSet<Guid> requiredOptions) in optionIdsByCategory.OrderBy(pair => pair.Key))
        {
            if (!categoryIndex.TryGetValue(categoryId, out WellBoreArchitectureFeatureCategory? category))
            {
                errors.Add(Error(null, "CatalogDependencies.FeatureCategories", "referenced_definition_missing", $"Referenced feature category '{categoryId}' does not exist."));
                continue;
            }
            Dictionary<Guid, WellBoreArchitectureFeatureOption> available = (category.Options ?? []).Where(value => value != null && value.ID != Guid.Empty)
                .GroupBy(value => value.ID).ToDictionary(group => group.Key, group => group.First());
            List<WellBoreArchitectureFeatureOption> options = [];
            foreach (Guid optionId in requiredOptions.Order())
            {
                if (available.TryGetValue(optionId, out WellBoreArchitectureFeatureOption? option)) options.Add(option);
                else errors.Add(Error(null, "CatalogDependencies.FeatureCategories.Options", "referenced_option_missing",
                    $"Referenced option '{optionId}' does not exist in category '{categoryId}'."));
            }
            result.FeatureCategories.Add(new WellBoreArchitectureFeatureCategory
            {
                MetaInfo = category.MetaInfo, Name = category.Name, IsExclusive = category.IsExclusive,
                HasValidityPeriod = category.HasValidityPeriod, Options = options,
                CreationDate = category.CreationDate, LastModificationDate = category.LastModificationDate
            });
        }
        return result;
    }

    private static List<WellBoreArchitectureBatchError> ValidateRequest(WellBoreArchitectureBatchExportRequest? request)
    {
        if (request == null) return [Error(null, "Request", "required", "A batch-export request is required.")];
        List<WellBoreArchitectureBatchError> errors = [];
        if (request.Scope == WellBoreArchitectureBatchExportScope.All)
        {
            if (request.WellBoreArchitectureIDs is { Count: > 0 }) errors.Add(Error(null, "WellBoreArchitectureIDs", "forbidden", "WellBoreArchitectureIDs must be omitted for an All export."));
        }
        else if (request.Scope == WellBoreArchitectureBatchExportScope.Selected)
        {
            if (request.WellBoreArchitectureIDs == null || request.WellBoreArchitectureIDs.Count == 0) errors.Add(Error(null, "WellBoreArchitectureIDs", "required", "Selected export requires at least one UUID."));
            else
            {
                HashSet<Guid> ids = [];
                for (int index = 0; index < request.WellBoreArchitectureIDs.Count; index++)
                {
                    Guid id = request.WellBoreArchitectureIDs[index];
                    if (id == Guid.Empty) errors.Add(Error(index, "WellBoreArchitectureIDs", "empty_uuid", "WellBoreArchitecture UUIDs must be non-empty."));
                    else if (!ids.Add(id)) errors.Add(Error(index, "WellBoreArchitectureIDs", "duplicate_uuid", $"WellBoreArchitecture UUID '{id}' occurs more than once."));
                }
            }
        }
        else errors.Add(Error(null, "Scope", "invalid_scope", "Scope must be All or Selected."));
        return errors;
    }

    private static WellBoreArchitectureBatchExportOutcome Failure(WellBoreArchitectureBatchExportFailureKind kind, string error,
        string message, List<WellBoreArchitectureBatchError> errors) => new()
        { FailureKind = kind, Error = new() { Error = error, Message = message, Errors = errors } };
    private static WellBoreArchitectureBatchError Error(int? index, string property, string code, string message) =>
        new() { PositionIndex = index, Property = property, Code = code, Message = message };
}


