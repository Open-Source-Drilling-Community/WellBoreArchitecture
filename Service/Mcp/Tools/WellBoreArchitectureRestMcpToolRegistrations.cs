using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using OSDC.Drilling.WellBoreArchitecture.Service.Controllers;
using OSDC.Drilling.WellBoreArchitecture.Service.Managers;
using ArchitectureModel = OSDC.Drilling.WellBoreArchitecture.Model.WellBoreArchitecture;
using IdentityModel = OSDC.Drilling.WellBoreArchitecture.Model.WellBoreArchitectureIdentity;
using FeatureCategoryModel = OSDC.Drilling.WellBoreArchitecture.Model.WellBoreArchitectureFeatureCategory;
using IdentityAssignmentModel = OSDC.Drilling.WellBoreArchitecture.Model.WellBoreArchitectureIdentityAssignment;
using FeatureAssignmentModel = OSDC.Drilling.WellBoreArchitecture.Model.WellBoreArchitectureFeatureAssignment;
using SurfaceSectionModel = OSDC.Drilling.WellBoreArchitecture.Model.SurfaceSection;
using CasingSectionModel = OSDC.Drilling.WellBoreArchitecture.Model.CasingSection;
using BatchExportRequestModel = OSDC.Drilling.WellBoreArchitecture.Model.WellBoreArchitectureBatchExportRequest;
using BatchRestoreRequestModel = OSDC.Drilling.WellBoreArchitecture.Model.WellBoreArchitectureBatchRestoreRequest;

namespace OSDC.Drilling.WellBoreArchitecture.Service.Mcp.Tools;

public static class WellBoreArchitectureRestMcpToolRegistrations
{
    private static readonly object ArchitectureMutationLock = new();

    public static IServiceCollection AddWellBoreArchitectureRestMcpTools(this IServiceCollection services)
    {
        services.AddLegacyMcpTool("well_bore_architecture_get_all_ids", "List the UUIDs of every stored wellbore architecture. Use this compact discovery operation when only identifiers are needed, then pass one UUID to well_bore_architecture_get_by_id for the complete construction model.", McpToolArgumentHelpers.CreateEmptySchema(),
            (sp, _, ct) => Invoke(ct, () => Controller(sp).GetAllWellBoreArchitectureId()));
        services.AddLegacyMcpTool("well_bore_architecture_get_all_meta_info", "List MetaInfo for every stored wellbore architecture without loading wellhead, surface-section, casing, fluid, and side-circuit data. Use a returned ID with well_bore_architecture_get_by_id to retrieve one full model.", McpToolArgumentHelpers.CreateEmptySchema(),
            (sp, _, ct) => Invoke(ct, () => Controller(sp).GetAllWellBoreArchitectureMetaInfo()));
        services.AddLegacyMcpTool("well_bore_architecture_get_by_id", "Retrieve one complete wellbore architecture by its resource UUID, including its external WellBore reference, wellhead, ordered fluid layers, ordered surface sections, side-circuit connectivity, casing sections, and open-hole sizes. Physical distribution values use SI units.", McpToolArgumentHelpers.CreateGuidSchema("id", "UUID of the wellbore-architecture resource to retrieve; this is not the referenced WellBoreID."),
            (sp, args, ct) => InvokeById(args, ct, id => Controller(sp).GetWellBoreArchitectureById(id)));
        services.AddLegacyMcpTool("well_bore_architecture_get_all_light", "List lightweight wellbore-architecture records containing metadata, name, description, and timestamps. Use this for human-readable discovery and selection; it intentionally omits WellBoreID and all construction geometry and material data.", McpToolArgumentHelpers.CreateEmptySchema(),
            (sp, _, ct) => Invoke(ct, () => Controller(sp).GetAllWellBoreArchitectureLight()));
        services.AddLegacyMcpTool("well_bore_architecture_get_all", "Legacy unbounded convenience operation that retrieves every complete wellbore architecture. This can be a very large response and may be removed in a future major contract version; new clients must use well_bore_architecture_search, lightweight discovery, and get-by-id.", McpToolArgumentHelpers.CreateEmptySchema(),
            (sp, _, ct) => Invoke(ct, () => Controller(sp).GetAllWellBoreArchitecture()));
        services.AddLegacyMcpTool("well_bore_architecture_search", "Return one deterministic page of complete WellBoreArchitectures together with the total match count. Filters may target name, external WellBore UUID, identity definition/value, feature category/option, and modification dates; results are ordered by resource UUID.", McpToolArgumentHelpers.CreateSearchSchema(),
            InvokeSearch);
        services.AddLegacyMcpTool("well_bore_architecture_batch_export", "Create a read-only, schema-version-1 JSON backup of all stored WellBoreArchitectures or an explicitly ordered selection. The response contains complete construction records and only the identity definitions, feature categories, and options referenced by them. WellBoreID remains an external UUID reference. An invalid or missing selected record rejects the complete export.", McpToolArgumentHelpers.CreateWellBoreArchitectureBatchExportSchema(),
            (sp, args, ct) => InvokeWithBodyResult<BatchExportRequestModel, OSDC.Drilling.WellBoreArchitecture.Model.WellBoreArchitectureBatchExportDocument>(args, "request", ct, request => Controller(sp).BatchExportWellBoreArchitectures(request)));
        services.AddLegacyMcpTool("well_bore_architecture_batch_restore", "Validate and atomically restore a schema-version-1 backup. Exact catalogue UUID matching is the safe default; missing definitions preserve source UUIDs. Mapping by normalized name requires explicit AllowNormalizedNameMapping consent and still rejects ambiguity or incompatible semantics. Catalogue mapping and all writes share one transaction, so any failure leaves the database unchanged.", McpToolArgumentHelpers.CreateWellBoreArchitectureBatchRestoreSchema(),
            (sp, args, ct) => InvokeWithBodyResult<BatchRestoreRequestModel, OSDC.Drilling.WellBoreArchitecture.Model.WellBoreArchitectureBatchRestoreResponse>(args, "request", ct, request => Controller(sp).BatchRestoreWellBoreArchitectures(request)));
        services.AddLegacyMcpTool("well_bore_architecture_create", "Persist a new complete wellbore architecture. Generate a non-empty wellBoreArchitecture.MetaInfo.ID first; an existing UUID produces a conflict. Supply at least one SurfaceSection, preserve top-to-bottom ordering, use WellBoreID only as an external reference, and encode physical values in SI through GaussianValue or DiracDistributionValue.", McpToolArgumentHelpers.CreateWellBoreArchitectureSchema(),
            (sp, args, ct) => InvokeWithBody<ArchitectureModel>(args, "wellBoreArchitecture", ct, data => Controller(sp).PostWellBoreArchitecture(data)));
        services.AddLegacyMcpTool("well_bore_architecture_update_by_id", "Replace an existing wellbore architecture using optimistic concurrency. The path id must equal MetaInfo.ID and expectedModifiedUtc must exactly match the latest LastModificationDate. Send the complete desired representation; stale or malformed writes change nothing.", McpToolArgumentHelpers.CreateWellBoreArchitectureSchema(includeId: true),
            (sp, args, ct) => InvokeFullUpdate(sp, args, ct));
        services.AddLegacyMcpTool("well_bore_architecture_details_update", "Update only Name and Description without resending construction arrays or assignments. expectedModifiedUtc must exactly match the latest architecture LastModificationDate; the complete updated architecture is returned and stale calls change nothing.", McpToolArgumentHelpers.CreateDetailsMutationSchema(),
            (sp, args, ct) => InvokeObjectMutation(sp, args, "details", ct, (stored, body) => { stored.Name = NodeString(body, "Name"); stored.Description = NodeString(body, "Description"); }));
        services.AddLegacyMcpTool("well_bore_architecture_well_bore_link_update", "Update only the externally-owned WellBoreID relationship without resending construction arrays or assignments. The reference is not synchronously validated; expectedModifiedUtc protects against stale writes and the complete updated architecture is returned.", McpToolArgumentHelpers.CreateWellBoreLinkMutationSchema(),
            (sp, args, ct) => InvokeObjectMutation(sp, args, "link", ct, (stored, body) => stored.WellBoreID = NodeGuid(body, "WellBoreID")));
        AddSectionTools<SurfaceSectionModel>(services, "surface_section", "SurfaceSection",
            architecture => architecture.SurfaceSections ??= []);
        AddSectionTools<CasingSectionModel>(services, "casing_section", "CasingSection",
            architecture => architecture.CasingSections ??= []);
        services.AddLegacyMcpTool("well_bore_architecture_identity_assignment_add", "Add one identity assignment without resending the complete architecture. Supply a caller-generated non-empty assignment UUID and the latest parent revision; definition references and duplicate IDs are validated before the updated architecture is committed.", McpToolArgumentHelpers.CreateIdentityAssignmentMutationSchema(false, true),
            (sp, args, ct) => InvokeAssignmentMutation<IdentityAssignmentModel>(sp, args, false, ct, (stored, value, _) => (stored.WellBoreArchitectureIdentityAssignments ??= []).Add(value)));
        services.AddLegacyMcpTool("well_bore_architecture_identity_assignment_update_by_id", "Replace one identity assignment selected by assignmentId without resending unrelated architecture data. The body ID must equal assignmentId and expectedModifiedUtc must match the latest parent revision; the updated architecture is returned.", McpToolArgumentHelpers.CreateIdentityAssignmentMutationSchema(true, true),
            (sp, args, ct) => InvokeAssignmentMutation<IdentityAssignmentModel>(sp, args, true, ct, ReplaceIdentityAssignment));
        services.AddLegacyMcpTool("well_bore_architecture_identity_assignment_delete_by_id", "Remove one identity assignment selected by assignmentId without changing other architecture fields. expectedModifiedUtc must match the latest parent revision; unknown assignments return not found and stale calls change nothing.", McpToolArgumentHelpers.CreateIdentityAssignmentMutationSchema(true, false),
            (sp, args, ct) => InvokeAssignmentDelete(sp, args, false, ct));
        services.AddLegacyMcpTool("well_bore_architecture_feature_assignment_add", "Add one feature assignment without resending the complete architecture. Category, option, exclusivity, validity-period, duplicate-ID, and optimistic-concurrency rules are checked before the updated architecture is committed.", McpToolArgumentHelpers.CreateFeatureAssignmentMutationSchema(false, true),
            (sp, args, ct) => InvokeAssignmentMutation<FeatureAssignmentModel>(sp, args, false, ct, (stored, value, _) => (stored.WellBoreArchitectureFeatureAssignments ??= []).Add(value)));
        services.AddLegacyMcpTool("well_bore_architecture_feature_assignment_update_by_id", "Replace one feature assignment selected by assignmentId without resending unrelated architecture data. The body and route assignment UUIDs must match, all category rules remain enforced, and stale parent revisions are rejected.", McpToolArgumentHelpers.CreateFeatureAssignmentMutationSchema(true, true),
            (sp, args, ct) => InvokeAssignmentMutation<FeatureAssignmentModel>(sp, args, true, ct, ReplaceFeatureAssignment));
        services.AddLegacyMcpTool("well_bore_architecture_feature_assignment_delete_by_id", "Remove one feature assignment selected by assignmentId without changing other architecture fields. expectedModifiedUtc must match the latest parent revision; unknown assignments return not found and stale calls change nothing.", McpToolArgumentHelpers.CreateFeatureAssignmentMutationSchema(true, false),
            (sp, args, ct) => InvokeAssignmentDelete(sp, args, true, ct));
        services.AddLegacyMcpTool("well_bore_architecture_delete_by_id", "Permanently delete one stored wellbore architecture only when expectedModifiedUtc exactly matches its latest LastModificationDate. This accepts MetaInfo.ID rather than WellBoreID; stale or unknown requests do not delete data.", McpToolArgumentHelpers.CreateWellBoreArchitectureDeleteSchema(),
            InvokeDeleteWithRevision);
        AddCatalogDiscoveryTools(services, "well_bore_architecture_identity", "WellBore Architecture Identity",
            sp => IdentityManager(sp).GetAll().Select(value => value.MetaInfo!));
        AddCatalogTools<IdentityModel>(services, "well_bore_architecture_identity", "wellBoreArchitectureIdentity", false,
            sp => IdentityController(sp).GetAll(), (sp, id) => IdentityController(sp).Get(id),
            (sp, value) => IdentityController(sp).Post(value), (sp, id, expected, value) => IdentityController(sp).Put(id, expected, value),
            (sp, id, expected) => IdentityController(sp).Delete(id, expected));
        AddCatalogDiscoveryTools(services, "well_bore_architecture_feature_category", "WellBore Architecture Feature Category",
            sp => FeatureManager(sp).GetAll().Select(value => value.MetaInfo!));
        AddCatalogTools<FeatureCategoryModel>(services, "well_bore_architecture_feature_category", "wellBoreArchitectureFeatureCategory", true,
            sp => FeatureController(sp).GetAll(), (sp, id) => FeatureController(sp).Get(id),
            (sp, value) => FeatureController(sp).Post(value), (sp, id, expected, value) => FeatureController(sp).Put(id, expected, value),
            (sp, id, expected) => FeatureController(sp).Delete(id, expected));
        return services;
    }

    private static void AddSectionTools<T>(IServiceCollection services, string toolSegment, string definitionName,
        Func<ArchitectureModel, List<T>> sections) where T : class
    {
        string prefix = "well_bore_architecture_" + toolSegment;
        services.AddLegacyMcpTool(prefix + "_add", $"Add one {definitionName} at an optional top-to-bottom position without replacing the rest of the architecture. Supply a caller-generated non-empty ComponentID and echo expectedModifiedUtc exactly from the latest read; duplicate component IDs and invalid positions change nothing.",
            McpToolArgumentHelpers.CreateSectionMutationSchema(definitionName, false, true, true),
            (sp, args, ct) => InvokeSectionAdd(sp, args, sections, ct));
        services.AddLegacyMcpTool(prefix + "_update_by_id", $"Replace one {definitionName} selected by stable componentId without resending other sections. section.ComponentID must equal componentId and expectedModifiedUtc is an opaque token copied exactly from the latest read.",
            McpToolArgumentHelpers.CreateSectionMutationSchema(definitionName, true, true),
            (sp, args, ct) => InvokeSectionUpdate(sp, args, sections, ct));
        services.AddLegacyMcpTool(prefix + "_delete_by_id", $"Delete one {definitionName} selected by stable componentId. The latest opaque expectedModifiedUtc token is required; deleting the final SurfaceSection is rejected by architecture validation.",
            McpToolArgumentHelpers.CreateSectionMutationSchema(definitionName, true, false),
            (sp, args, ct) => InvokeSectionDelete(sp, args, sections, ct));
        services.AddLegacyMcpTool(prefix + "_reorder", $"Reorder all existing {definitionName} values top-to-bottom by stable ComponentID without resending their engineering payloads. The list must contain every current section ID exactly once and expectedModifiedUtc must be copied from the latest read.",
            McpToolArgumentHelpers.CreateSectionReorderSchema(),
            (sp, args, ct) => InvokeSectionReorder(sp, args, sections, ct));
    }

    private static void AddCatalogDiscoveryTools(IServiceCollection services, string prefix, string entityName,
        Func<IServiceProvider, IEnumerable<OSDC.DotnetLibraries.General.DataManagement.MetaInfo>> metadata)
    {
        services.AddLegacyMcpTool(prefix + "_get_all_ids", $"List every stored {entityName} UUID without transferring complete definitions. Use this compact discovery operation before fetching one definition by ID or constructing an assignment; results are deterministic.", McpToolArgumentHelpers.CreateEmptySchema(),
            (sp, _, ct) => { ct.ThrowIfCancellationRequested(); return Task.FromResult<JsonNode?>(Success(metadata(sp).Select(value => value.ID).OrderBy(value => value).ToList())); });
        services.AddLegacyMcpTool(prefix + "_get_all_meta_info", $"List MetaInfo for every stored {entityName} without transferring complete definitions or feature-option arrays. Use the returned IDs and location metadata for efficient discovery before fetching selected definitions.", McpToolArgumentHelpers.CreateEmptySchema(),
            (sp, _, ct) => { ct.ThrowIfCancellationRequested(); return Task.FromResult<JsonNode?>(Success(metadata(sp).OrderBy(value => value.ID).ToList())); });
    }

    private static void AddCatalogTools<T>(IServiceCollection services, string prefix, string bodyName, bool feature,
        Func<IServiceProvider, ActionResult<IEnumerable<T>>> all, Func<IServiceProvider, Guid, ActionResult<T>> get,
        Func<IServiceProvider, T?, ActionResult> create, Func<IServiceProvider, Guid, DateTimeOffset, T?, ActionResult> update,
        Func<IServiceProvider, Guid, DateTimeOffset, ActionResult> delete)
    {
        services.AddLegacyMcpTool(prefix + "_get_all", "List every user-manageable catalog definition, including stable UUIDs, server-owned timestamps, names, and feature options where applicable. Use this read before assigning a definition to an architecture or attempting an optimistic-concurrency update.", McpToolArgumentHelpers.CreateEmptySchema(),
            (sp, _, ct) => Invoke(ct, () => all(sp)));
        services.AddLegacyMcpTool(prefix + "_get_by_id", "Retrieve one complete catalog definition by its stable UUID, including the latest LastModificationDate concurrency token and all feature options where applicable. An unknown UUID returns not found and does not change service state.", McpToolArgumentHelpers.CreateGuidSchema("id", "Catalog definition UUID."),
            (sp, args, ct) => InvokeById(args, ct, id => get(sp, id)));
        services.AddLegacyMcpTool(prefix + "_create", "Create a custom catalog definition using a caller-generated non-empty UUID. The service owns CreationDate and LastModificationDate, rejects duplicate identifiers, and preserves the supplied category flags and stable option UUIDs for later assignments.", McpToolArgumentHelpers.CreateCatalogSchema(bodyName, feature),
            (sp, args, ct) => InvokeWithBody<T>(args, bodyName, ct, value => create(sp, value)));
        services.AddLegacyMcpTool(prefix + "_update_by_id", "Replace one catalog definition. The path and body UUIDs must match, and expectedModifiedUtc must exactly match the latest LastModificationDate. A stale request or removal of an option referenced by any stored architecture is rejected without changing data.", McpToolArgumentHelpers.CreateCatalogSchema(bodyName, feature, true, true),
            (sp, args, ct) => InvokeCatalogUpdate<T>(args, bodyName, ct, (id, expected, value) => update(sp, id, expected, value)));
        services.AddLegacyMcpTool(prefix + "_delete_by_id", "Delete one unused catalog definition using its stable UUID and latest LastModificationDate. Definitions referenced by any stored architecture are rejected with conflict, stale requests are rejected, and neither case changes catalog or architecture data.", McpToolArgumentHelpers.CreateCatalogDeleteSchema(),
            (sp, args, ct) => InvokeCatalogDelete(args, ct, (id, expected) => delete(sp, id, expected)));
    }

    private static Task<JsonNode?> InvokeFullUpdate(IServiceProvider sp, JsonObject? args, CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        if (!TryMutationHeader(args, "id", out Guid id, out DateTimeOffset expected, out JsonNode? error)) return Task.FromResult(error);
        if (!TryDeserialize(args, "wellBoreArchitecture", out ArchitectureModel? value, out error)) return Task.FromResult(error);
        if (value?.MetaInfo == null || value.MetaInfo.ID != id)
            return Task.FromResult<JsonNode?>(McpToolResponses.CreateValidationError("Argument 'id' must equal wellBoreArchitecture.MetaInfo.ID."));
        return Task.FromResult<JsonNode?>(Mutate(sp, id, expected, _ => value));
    }

    private static Task<JsonNode?> InvokeObjectMutation(IServiceProvider sp, JsonObject? args, string bodyName,
        CancellationToken ct, Action<ArchitectureModel, JsonObject> mutation)
    {
        ct.ThrowIfCancellationRequested();
        if (!TryMutationHeader(args, "wellBoreArchitectureId", out Guid id, out DateTimeOffset expected, out JsonNode? error)) return Task.FromResult(error);
        if (args?[bodyName] is not JsonObject body)
            return Task.FromResult<JsonNode?>(McpToolResponses.CreateValidationError($"Argument '{bodyName}' is required and must be an object."));
        return Task.FromResult<JsonNode?>(Mutate(sp, id, expected, stored => { mutation(stored, body); return stored; }));
    }

    private static Task<JsonNode?> InvokeSectionAdd<T>(IServiceProvider sp, JsonObject? args,
        Func<ArchitectureModel, List<T>> select, CancellationToken ct) where T : class
    {
        ct.ThrowIfCancellationRequested();
        if (!TryMutationHeader(args, "wellBoreArchitectureId", out Guid architectureId, out DateTimeOffset expected, out JsonNode? error)) return Task.FromResult(error);
        if (!TryDeserialize(args, "section", out T? section, out error)) return Task.FromResult(error);
        Guid componentId = ComponentId(section!);
        if (componentId == Guid.Empty) return Task.FromResult<JsonNode?>(McpToolResponses.CreateValidationError("section.ComponentID must be a non-empty caller-generated UUID."));
        int? insertAt = null;
        if (args?["insertAt"] is JsonNode positionNode && positionNode.GetValueKind() != JsonValueKind.Null)
        {
            try { insertAt = positionNode.GetValue<int>(); }
            catch (Exception ex) when (ex is InvalidOperationException or FormatException)
            { return Task.FromResult<JsonNode?>(McpToolResponses.CreateValidationError("insertAt must be a non-negative integer.")); }
        }
        return Task.FromResult<JsonNode?>(Mutate(sp, architectureId, expected, stored =>
        {
            if (ContainsComponentId(stored, componentId)) throw new MutationConflictException($"ComponentID '{componentId}' already exists in this architecture.");
            List<T> values = select(stored);
            int index = insertAt ?? values.Count;
            if (index < 0 || index > values.Count) throw new MutationValidationException($"insertAt must be between 0 and {values.Count}.");
            values.Insert(index, section!);
            return stored;
        }));
    }

    private static Task<JsonNode?> InvokeSectionUpdate<T>(IServiceProvider sp, JsonObject? args,
        Func<ArchitectureModel, List<T>> select, CancellationToken ct) where T : class
    {
        ct.ThrowIfCancellationRequested();
        if (!TryMutationHeader(args, "wellBoreArchitectureId", out Guid architectureId, out DateTimeOffset expected, out JsonNode? error)) return Task.FromResult(error);
        if (!McpToolArgumentHelpers.TryParseGuid(args, "componentId", out Guid componentId, out error)) return Task.FromResult(error);
        if (!TryDeserialize(args, "section", out T? section, out error)) return Task.FromResult(error);
        if (ComponentId(section!) != componentId) return Task.FromResult<JsonNode?>(McpToolResponses.CreateValidationError("componentId must equal section.ComponentID."));
        return Task.FromResult<JsonNode?>(Mutate(sp, architectureId, expected, stored =>
        {
            List<T> values = select(stored);
            int index = values.FindIndex(value => ComponentId(value) == componentId);
            if (index < 0) throw new MutationNotFoundException("The section does not exist on this architecture.");
            values[index] = section!;
            return stored;
        }));
    }

    private static Task<JsonNode?> InvokeSectionDelete<T>(IServiceProvider sp, JsonObject? args,
        Func<ArchitectureModel, List<T>> select, CancellationToken ct) where T : class
    {
        ct.ThrowIfCancellationRequested();
        if (!TryMutationHeader(args, "wellBoreArchitectureId", out Guid architectureId, out DateTimeOffset expected, out JsonNode? error)) return Task.FromResult(error);
        if (!McpToolArgumentHelpers.TryParseGuid(args, "componentId", out Guid componentId, out error)) return Task.FromResult(error);
        return Task.FromResult<JsonNode?>(Mutate(sp, architectureId, expected, stored =>
        {
            List<T> values = select(stored);
            int removed = values.RemoveAll(value => ComponentId(value) == componentId);
            if (removed == 0) throw new MutationNotFoundException("The section does not exist on this architecture.");
            return stored;
        }));
    }

    private static Task<JsonNode?> InvokeSectionReorder<T>(IServiceProvider sp, JsonObject? args,
        Func<ArchitectureModel, List<T>> select, CancellationToken ct) where T : class
    {
        ct.ThrowIfCancellationRequested();
        if (!TryMutationHeader(args, "wellBoreArchitectureId", out Guid architectureId, out DateTimeOffset expected, out JsonNode? error)) return Task.FromResult(error);
        if (args?["orderedComponentIds"] is not JsonArray requested)
            return Task.FromResult<JsonNode?>(McpToolResponses.CreateValidationError("orderedComponentIds is required and must be an array."));
        var ids = new List<Guid>();
        foreach (JsonNode? node in requested)
            if (node == null || !Guid.TryParse(node.ToString(), out Guid parsed) || parsed == Guid.Empty)
                return Task.FromResult<JsonNode?>(McpToolResponses.CreateValidationError("Every orderedComponentIds item must be a non-empty UUID."));
            else ids.Add(parsed);
        if (ids.Count != ids.Distinct().Count()) return Task.FromResult<JsonNode?>(McpToolResponses.CreateValidationError("orderedComponentIds must not contain duplicates."));
        return Task.FromResult<JsonNode?>(Mutate(sp, architectureId, expected, stored =>
        {
            List<T> values = select(stored);
            Dictionary<Guid, T> current = values.ToDictionary(ComponentId);
            if (ids.Count != current.Count || ids.Any(id => !current.ContainsKey(id)))
                throw new MutationValidationException("orderedComponentIds must contain every current section ComponentID exactly once.");
            values.Clear();
            values.AddRange(ids.Select(id => current[id]));
            return stored;
        }));
    }

    private static Task<JsonNode?> InvokeAssignmentMutation<T>(IServiceProvider sp, JsonObject? args,
        bool requireAssignmentId, CancellationToken ct, Action<ArchitectureModel, T, Guid?> mutation) where T : class
    {
        ct.ThrowIfCancellationRequested();
        if (!TryMutationHeader(args, "wellBoreArchitectureId", out Guid id, out DateTimeOffset expected, out JsonNode? error)) return Task.FromResult(error);
        Guid? assignmentId = null;
        if (requireAssignmentId)
        {
            if (!McpToolArgumentHelpers.TryParseGuid(args, "assignmentId", out Guid parsed, out error)) return Task.FromResult(error);
            assignmentId = parsed;
        }
        if (!TryDeserialize(args, "assignment", out T? value, out error)) return Task.FromResult(error);
        Guid bodyId = AssignmentId(value!);
        if (bodyId == Guid.Empty)
            return Task.FromResult<JsonNode?>(McpToolResponses.CreateValidationError("assignment.ID must be a non-empty caller-generated UUID."));
        if (assignmentId.HasValue && assignmentId.Value != bodyId)
            return Task.FromResult<JsonNode?>(McpToolResponses.CreateValidationError("assignmentId must equal assignment.ID."));

        return Task.FromResult<JsonNode?>(Mutate(sp, id, expected, stored =>
        {
            bool exists = AssignmentExists(stored, bodyId);
            if (!requireAssignmentId && exists) throw new MutationConflictException($"Assignment UUID '{bodyId}' already exists.");
            mutation(stored, value!, assignmentId);
            return stored;
        }));
    }

    private static Task<JsonNode?> InvokeAssignmentDelete(IServiceProvider sp, JsonObject? args,
        bool feature, CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        if (!TryMutationHeader(args, "wellBoreArchitectureId", out Guid id, out DateTimeOffset expected, out JsonNode? error)) return Task.FromResult(error);
        if (!McpToolArgumentHelpers.TryParseGuid(args, "assignmentId", out Guid assignmentId, out error)) return Task.FromResult(error);
        return Task.FromResult<JsonNode?>(Mutate(sp, id, expected, stored =>
        {
            int removed = feature
                ? (stored.WellBoreArchitectureFeatureAssignments ??= []).RemoveAll(value => value.ID == assignmentId)
                : (stored.WellBoreArchitectureIdentityAssignments ??= []).RemoveAll(value => value.ID == assignmentId);
            if (removed == 0) throw new MutationNotFoundException("The assignment does not exist on this architecture.");
            return stored;
        }));
    }

    private static Task<JsonNode?> InvokeDeleteWithRevision(IServiceProvider sp, JsonObject? args, CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        if (!TryMutationHeader(args, "id", out Guid id, out DateTimeOffset expected, out JsonNode? error)) return Task.FromResult(error);
        lock (ArchitectureMutationLock)
        {
            ArchitectureModel? stored = Manager(sp).GetWellBoreArchitectureById(id);
            if (stored == null) return Task.FromResult<JsonNode?>(Error(404, "not_found", "The architecture does not exist."));
            if (!RevisionMatches(stored, expected)) return Task.FromResult<JsonNode?>(Stale(stored, expected));
            return Task.FromResult<JsonNode?>(Manager(sp).DeleteWellBoreArchitectureById(id)
                ? Success(null) : Error(500, "storage_failure", "The delete could not be committed."));
        }
    }

    private static Task<JsonNode?> InvokeSearch(IServiceProvider sp, JsonObject? args, CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        try
        {
            int offset = args?["offset"]?.GetValue<int>() ?? 0;
            int limit = args?["limit"]?.GetValue<int>() ?? 50;
            if (offset < 0 || limit is < 1 or > 200)
                return Task.FromResult<JsonNode?>(McpToolResponses.CreateValidationError("offset must be non-negative and limit must be between 1 and 200."));
            Guid? GuidFilter(string key)
            {
                if (args?[key] is null) return null;
                if (!Guid.TryParse(args[key]!.ToString(), out Guid value)) throw new FormatException(key);
                return value;
            }
            DateTimeOffset? DateFilter(string key)
            {
                if (args?[key] is null) return null;
                if (!DateTimeOffset.TryParse(args[key]!.ToString(), out DateTimeOffset value)) throw new FormatException(key);
                return value;
            }
            string? name = NodeString(args, "name");
            string? identityValue = NodeString(args, "identityValue");
            Guid? wellBoreId = GuidFilter("wellBoreId");
            Guid? identityId = GuidFilter("identityId");
            Guid? categoryId = GuidFilter("featureCategoryId");
            Guid? optionId = GuidFilter("featureOptionId");
            DateTimeOffset? from = DateFilter("modifiedFromUtc");
            DateTimeOffset? to = DateFilter("modifiedToUtc");
            bool? isLinked = args?["isLinked"] is JsonNode linkedNode && linkedNode.GetValueKind() != JsonValueKind.Null
                ? linkedNode.GetValue<bool>() : null;
            if (from > to) return Task.FromResult<JsonNode?>(McpToolResponses.CreateValidationError("modifiedFromUtc must not be after modifiedToUtc."));

            IEnumerable<ArchitectureModel> query = (Manager(sp).GetAllWellBoreArchitecture() ?? [])
                .Where(value => value != null).Cast<ArchitectureModel>();
            if (!string.IsNullOrWhiteSpace(name)) query = query.Where(value => value.Name?.Contains(name, StringComparison.OrdinalIgnoreCase) == true);
            if (wellBoreId.HasValue) query = query.Where(value => value.WellBoreID == wellBoreId);
            if (identityId.HasValue) query = query.Where(value => value.WellBoreArchitectureIdentityAssignments?.Any(a => a.IdentityID == identityId) == true);
            if (!string.IsNullOrWhiteSpace(identityValue)) query = query.Where(value => value.WellBoreArchitectureIdentityAssignments?.Any(a => a.Value?.Contains(identityValue, StringComparison.OrdinalIgnoreCase) == true) == true);
            if (categoryId.HasValue) query = query.Where(value => value.WellBoreArchitectureFeatureAssignments?.Any(a => a.FeatureCategoryID == categoryId) == true);
            if (optionId.HasValue) query = query.Where(value => value.WellBoreArchitectureFeatureAssignments?.Any(a => a.FeatureOptionID == optionId) == true);
            if (from.HasValue) query = query.Where(value => Revision(value) >= from.Value);
            if (to.HasValue) query = query.Where(value => Revision(value) <= to.Value);
            if (isLinked.HasValue) query = query.Where(value => isLinked.Value ? value.WellBoreID.HasValue : !value.WellBoreID.HasValue);
            List<ArchitectureModel> matches = query.OrderBy(value => value.MetaInfo!.ID).ToList();
            return Task.FromResult<JsonNode?>(Success(new { Offset = offset, Limit = limit, TotalCount = matches.Count, Items = matches.Skip(offset).Take(limit).ToList() }));
        }
        catch (Exception ex) when (ex is InvalidOperationException or FormatException)
        {
            return Task.FromResult<JsonNode?>(McpToolResponses.CreateValidationError("One or more search arguments have an invalid type or format."));
        }
    }

    private static JsonNode Mutate(IServiceProvider sp, Guid id, DateTimeOffset expected,
        Func<ArchitectureModel, ArchitectureModel> mutation)
    {
        lock (ArchitectureMutationLock)
        {
            ArchitectureModel? stored = Manager(sp).GetWellBoreArchitectureById(id);
            if (stored == null) return Error(404, "not_found", "The architecture does not exist.");
            if (!RevisionMatches(stored, expected)) return Stale(stored, expected);
            try
            {
                ArchitectureModel updated = mutation(stored);
                updated.CreationDate = stored.CreationDate;
                updated.LastModificationDate = stored.LastModificationDate;
                if (!Manager(sp).UpdateWellBoreArchitectureById(id, updated))
                    return Error(400, "invalid_architecture", "The mutation violates an architecture or assignment invariant.");
                return Success(updated);
            }
            catch (MutationConflictException ex) { return Error(409, "already_exists", ex.Message); }
            catch (MutationNotFoundException ex) { return Error(404, "not_found", ex.Message); }
            catch (MutationValidationException ex) { return Error(400, "invalid_component_mutation", ex.Message); }
        }
    }

    private static Guid ComponentId<T>(T value) => value switch
    {
        SurfaceSectionModel surface => surface.ComponentID,
        CasingSectionModel casing => casing.ComponentID,
        _ => Guid.Empty
    };

    private static bool ContainsComponentId(ArchitectureModel architecture, Guid id)
    {
        bool SideElementHas(OSDC.Drilling.WellBoreArchitecture.Model.SideElement? value) => value?.ComponentID == id;
        foreach (var surface in architecture.SurfaceSections ?? [])
        {
            if (surface.ComponentID == id) return true;
            foreach (var connector in surface.SideConnectors ?? [])
            {
                if (connector.ComponentID == id || SideElementHas(connector.FirstSideElement)) return true;
                foreach (var connectivity in connector.ElementConnectivities ?? [])
                    if (connectivity.ComponentID == id || SideElementHas(connectivity.UpstreamElement) || SideElementHas(connectivity.DownstreamElement)) return true;
            }
        }
        foreach (var casing in architecture.CasingSections ?? [])
        {
            if (casing.ComponentID == id || casing.OpenHoleSection?.ComponentID == id) return true;
            if ((casing.CasingSectionElements ?? []).Any(value => value.ComponentID == id)) return true;
            if ((casing.OpenHoleSection?.HoleSizes ?? []).Any(value => value.ComponentID == id)) return true;
        }
        return false;
    }

    private static void ReplaceIdentityAssignment(ArchitectureModel stored, IdentityAssignmentModel value, Guid? id)
    {
        var assignments = stored.WellBoreArchitectureIdentityAssignments ??= [];
        int index = assignments.FindIndex(item => item.ID == id);
        if (index < 0) throw new MutationNotFoundException("The identity assignment does not exist.");
        assignments[index] = value;
    }

    private static void ReplaceFeatureAssignment(ArchitectureModel stored, FeatureAssignmentModel value, Guid? id)
    {
        var assignments = stored.WellBoreArchitectureFeatureAssignments ??= [];
        int index = assignments.FindIndex(item => item.ID == id);
        if (index < 0) throw new MutationNotFoundException("The feature assignment does not exist.");
        assignments[index] = value;
    }

    private static bool TryMutationHeader(JsonObject? args, string idName, out Guid id,
        out DateTimeOffset expected, out JsonNode? error)
    {
        expected = default;
        if (!McpToolArgumentHelpers.TryParseGuid(args, idName, out id, out error)) return false;
        if (args?["expectedModifiedUtc"] is not JsonNode timestamp ||
            !DateTimeOffset.TryParse(timestamp.ToString(), out expected) || expected == default)
        {
            error = McpToolResponses.CreateValidationError("Argument 'expectedModifiedUtc' must be a non-default ISO 8601 date-time.");
            return false;
        }
        return true;
    }

    private static Guid AssignmentId<T>(T assignment) => assignment switch
    {
        IdentityAssignmentModel value => value.ID,
        FeatureAssignmentModel value => value.ID,
        _ => Guid.Empty
    };

    private static bool AssignmentExists(ArchitectureModel value, Guid id) =>
        (value.WellBoreArchitectureIdentityAssignments ?? []).Any(item => item.ID == id) ||
        (value.WellBoreArchitectureFeatureAssignments ?? []).Any(item => item.ID == id);

    private static string? NodeString(JsonObject? value, string key) =>
        value?[key] is JsonNode node && node.GetValueKind() != JsonValueKind.Null ? node.GetValue<string>() : null;

    private static Guid? NodeGuid(JsonObject value, string key) =>
        value[key] is JsonNode node && node.GetValueKind() != JsonValueKind.Null && Guid.TryParse(node.ToString(), out Guid id) ? id : null;

    private static DateTimeOffset Revision(ArchitectureModel value) =>
        value.LastModificationDate ?? value.CreationDate ?? DateTimeOffset.UnixEpoch;

    private static bool RevisionMatches(ArchitectureModel value, DateTimeOffset expected) =>
        Revision(value).UtcTicks == expected.UtcTicks;

    private static JsonObject Stale(ArchitectureModel value, DateTimeOffset expected) =>
        Error(409, "concurrency_conflict", $"Expected {expected:O}, but the architecture was modified at {Revision(value):O}.");

    private static JsonObject Success(object? value)
    {
        var result = new JsonObject { ["status"] = 200 };
        if (value != null) result["data"] = JsonSerializer.SerializeToNode(value, JsonSettings.Options);
        return result;
    }

    private static JsonObject Error(int status, string code, string message) => new()
    {
        ["status"] = status, ["error"] = code, ["message"] = message
    };

    private sealed class MutationConflictException(string message) : Exception(message);
    private sealed class MutationNotFoundException(string message) : Exception(message);
    private sealed class MutationValidationException(string message) : Exception(message);

    private static Task<JsonNode?> Invoke<T>(CancellationToken ct, Func<ActionResult<T>> action)
    {
        ct.ThrowIfCancellationRequested();
        return Task.FromResult<JsonNode?>(McpActionResultConverter.FromActionResult(action()));
    }
    private static Task<JsonNode?> InvokeById<T>(JsonObject? args, CancellationToken ct, Func<Guid, ActionResult<T>> action)
    {
        ct.ThrowIfCancellationRequested();
        return McpToolArgumentHelpers.TryParseGuid(args, "id", out Guid id, out JsonNode? error)
            ? Task.FromResult<JsonNode?>(McpActionResultConverter.FromActionResult(action(id))) : Task.FromResult(error);
    }
    private static Task<JsonNode?> InvokeDelete(JsonObject? args, CancellationToken ct, Func<Guid, ActionResult> action)
    {
        ct.ThrowIfCancellationRequested();
        return McpToolArgumentHelpers.TryParseGuid(args, "id", out Guid id, out JsonNode? error)
            ? Task.FromResult<JsonNode?>(McpActionResultConverter.FromActionResult(action(id))) : Task.FromResult(error);
    }
    private static Task<JsonNode?> InvokeWithBody<T>(JsonObject? args, string bodyName, CancellationToken ct, Func<T?, ActionResult> action)
    {
        ct.ThrowIfCancellationRequested();
        return TryDeserialize(args, bodyName, out T? data, out JsonNode? error)
            ? Task.FromResult<JsonNode?>(McpActionResultConverter.FromActionResult(action(data))) : Task.FromResult(error);
    }
    private static Task<JsonNode?> InvokeWithBodyResult<TBody, TResult>(JsonObject? args, string bodyName,
        CancellationToken ct, Func<TBody?, ActionResult<TResult>> action)
    {
        ct.ThrowIfCancellationRequested();
        return TryDeserialize(args, bodyName, out TBody? data, out JsonNode? error)
            ? Task.FromResult<JsonNode?>(McpActionResultConverter.FromActionResult(action(data)))
            : Task.FromResult(error);
    }
    private static Task<JsonNode?> InvokeWithIdAndBody<T>(JsonObject? args, string bodyName, CancellationToken ct, Func<Guid, T?, ActionResult> action)
    {
        ct.ThrowIfCancellationRequested();
        if (!McpToolArgumentHelpers.TryParseGuid(args, "id", out Guid id, out JsonNode? idError)) return Task.FromResult(idError);
        return TryDeserialize(args, bodyName, out T? data, out JsonNode? error)
            ? Task.FromResult<JsonNode?>(McpActionResultConverter.FromActionResult(action(id, data))) : Task.FromResult(error);
    }
    private static bool TryDeserialize<T>(JsonObject? args, string bodyName, out T? data, out JsonNode? error)
    {
        data = default;
        error = null;
        if (args?[bodyName] is not JsonNode node)
        {
            error = McpToolResponses.CreateValidationError($"Argument '{bodyName}' is required.");
            return false;
        }
        try
        {
            data = node.Deserialize<T>(JsonSettings.Options);
            if (data is null) throw new InvalidOperationException();
            return true;
        }
        catch (Exception ex) when (ex is JsonException or InvalidOperationException)
        {
            error = McpToolResponses.CreateValidationError($"Argument '{bodyName}' could not be deserialized.");
            return false;
        }
    }
    private static Task<JsonNode?> InvokeCatalogUpdate<T>(JsonObject? args, string bodyName, CancellationToken ct, Func<Guid, DateTimeOffset, T?, ActionResult> action)
    {
        ct.ThrowIfCancellationRequested();
        if (!McpToolArgumentHelpers.TryParseGuid(args, "id", out Guid id, out JsonNode? error)) return Task.FromResult(error);
        if (!DateTimeOffset.TryParse(args?["expectedModifiedUtc"]?.ToString(), out DateTimeOffset expected)) return Task.FromResult<JsonNode?>(McpToolResponses.CreateValidationError("Argument 'expectedModifiedUtc' must be an ISO 8601 date-time."));
        return TryDeserialize(args, bodyName, out T? value, out error) ? Task.FromResult<JsonNode?>(McpActionResultConverter.FromActionResult(action(id, expected, value))) : Task.FromResult(error);
    }
    private static Task<JsonNode?> InvokeCatalogDelete(JsonObject? args, CancellationToken ct, Func<Guid, DateTimeOffset, ActionResult> action)
    {
        ct.ThrowIfCancellationRequested();
        if (!McpToolArgumentHelpers.TryParseGuid(args, "id", out Guid id, out JsonNode? error)) return Task.FromResult(error);
        if (!DateTimeOffset.TryParse(args?["expectedModifiedUtc"]?.ToString(), out DateTimeOffset expected)) return Task.FromResult<JsonNode?>(McpToolResponses.CreateValidationError("Argument 'expectedModifiedUtc' must be an ISO 8601 date-time."));
        return Task.FromResult<JsonNode?>(McpActionResultConverter.FromActionResult(action(id, expected)));
    }
    private static WellBoreArchitectureController Controller(IServiceProvider sp) => new(
        sp.GetRequiredService<ILogger<WellBoreArchitectureManager>>(), sp.GetRequiredService<SqlConnectionManager>());
    private static WellBoreArchitectureManager Manager(IServiceProvider sp) => WellBoreArchitectureManager.GetInstance(
        sp.GetRequiredService<ILogger<WellBoreArchitectureManager>>(), sp.GetRequiredService<SqlConnectionManager>());
    private static WellBoreArchitectureIdentityController IdentityController(IServiceProvider sp) => new(sp.GetRequiredService<SqlConnectionManager>());
    private static WellBoreArchitectureFeatureCategoryController FeatureController(IServiceProvider sp) => new(sp.GetRequiredService<SqlConnectionManager>());
    private static WellBoreArchitectureIdentityManager IdentityManager(IServiceProvider sp) => new(sp.GetRequiredService<SqlConnectionManager>());
    private static WellBoreArchitectureFeatureCategoryManager FeatureManager(IServiceProvider sp) => new(sp.GetRequiredService<SqlConnectionManager>());
}
