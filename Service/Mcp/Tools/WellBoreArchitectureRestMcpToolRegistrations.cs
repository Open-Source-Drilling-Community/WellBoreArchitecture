using System;
using System.Collections.Generic;
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
using BatchExportRequestModel = OSDC.Drilling.WellBoreArchitecture.Model.WellBoreArchitectureBatchExportRequest;
using BatchRestoreRequestModel = OSDC.Drilling.WellBoreArchitecture.Model.WellBoreArchitectureBatchRestoreRequest;

namespace OSDC.Drilling.WellBoreArchitecture.Service.Mcp.Tools;

public static class WellBoreArchitectureRestMcpToolRegistrations
{
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
        services.AddLegacyMcpTool("well_bore_architecture_get_all", "Retrieve every wellbore architecture with complete wellhead, surface, casing, side-circuit, fluid, material, uncertainty, and open-hole data. This can be a large response; prefer IDs, metadata, or light records for discovery and retrieve one selected model by UUID.", McpToolArgumentHelpers.CreateEmptySchema(),
            (sp, _, ct) => Invoke(ct, () => Controller(sp).GetAllWellBoreArchitecture()));
        services.AddLegacyMcpTool("well_bore_architecture_batch_export", "Create a read-only, schema-version-1 JSON backup of all stored WellBoreArchitectures or an explicitly ordered selection. The response contains complete construction records and only the identity definitions, feature categories, and options referenced by them. WellBoreID remains an external UUID reference. An invalid or missing selected record rejects the complete export.", McpToolArgumentHelpers.CreateWellBoreArchitectureBatchExportSchema(),
            (sp, args, ct) => InvokeWithBodyResult<BatchExportRequestModel, OSDC.Drilling.WellBoreArchitecture.Model.WellBoreArchitectureBatchExportDocument>(args, "request", ct, request => Controller(sp).BatchExportWellBoreArchitectures(request)));
        services.AddLegacyMcpTool("well_bore_architecture_batch_restore", "Validate and atomically restore a schema-version-1 WellBoreArchitecture backup. Catalogue UUIDs map by exact UUID or one compatible normalized name, and MapOrCreateMissing may create missing definitions and options. ReplaceExisting must be explicitly selected. Catalogue mapping, reference rewriting, definition creation, and all architecture writes share one transaction, so any validation, conflict, or storage failure leaves the database unchanged.", McpToolArgumentHelpers.CreateWellBoreArchitectureBatchRestoreSchema(),
            (sp, args, ct) => InvokeWithBodyResult<BatchRestoreRequestModel, OSDC.Drilling.WellBoreArchitecture.Model.WellBoreArchitectureBatchRestoreResponse>(args, "request", ct, request => Controller(sp).BatchRestoreWellBoreArchitectures(request)));
        services.AddLegacyMcpTool("well_bore_architecture_create", "Persist a new complete wellbore architecture. Generate a non-empty wellBoreArchitecture.MetaInfo.ID first; an existing UUID produces a conflict. Supply at least one SurfaceSection, preserve top-to-bottom ordering, use WellBoreID only as an external reference, and encode physical values in SI through GaussianValue or DiracDistributionValue.", McpToolArgumentHelpers.CreateWellBoreArchitectureSchema(),
            (sp, args, ct) => InvokeWithBody<ArchitectureModel>(args, "wellBoreArchitecture", ct, data => Controller(sp).PostWellBoreArchitecture(data)));
        services.AddLegacyMcpTool("well_bore_architecture_update_by_id", "Replace an existing wellbore architecture. The path id must exactly match wellBoreArchitecture.MetaInfo.ID. Send the complete desired representation because omitted collections may be lost, retain at least one SurfaceSection, update LastModificationDate, preserve ordering/reference conventions, and use SI distribution values.", McpToolArgumentHelpers.CreateWellBoreArchitectureSchema(includeId: true),
            (sp, args, ct) => InvokeWithIdAndBody<ArchitectureModel>(args, "wellBoreArchitecture", ct, (id, data) => Controller(sp).PutWellBoreArchitectureById(id, data)));
        services.AddLegacyMcpTool("well_bore_architecture_delete_by_id", "Permanently delete the stored wellbore-architecture resource identified by UUID. Use a read operation first when the target is uncertain. This accepts the architecture's MetaInfo.ID, not its referenced WellBoreID, and returns not found for an unknown resource.", McpToolArgumentHelpers.CreateGuidSchema("id", "UUID from wellBoreArchitecture.MetaInfo.ID of the architecture to delete."),
            (sp, args, ct) => InvokeDelete(args, ct, id => Controller(sp).DeleteWellBoreArchitectureById(id)));
        AddCatalogTools<IdentityModel>(services, "well_bore_architecture_identity", "wellBoreArchitectureIdentity", false,
            sp => IdentityController(sp).GetAll(), (sp, id) => IdentityController(sp).Get(id),
            (sp, value) => IdentityController(sp).Post(value), (sp, id, expected, value) => IdentityController(sp).Put(id, expected, value),
            (sp, id, expected) => IdentityController(sp).Delete(id, expected));
        AddCatalogTools<FeatureCategoryModel>(services, "well_bore_architecture_feature_category", "wellBoreArchitectureFeatureCategory", true,
            sp => FeatureController(sp).GetAll(), (sp, id) => FeatureController(sp).Get(id),
            (sp, value) => FeatureController(sp).Post(value), (sp, id, expected, value) => FeatureController(sp).Put(id, expected, value),
            (sp, id, expected) => FeatureController(sp).Delete(id, expected));
        return services;
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
    private static WellBoreArchitectureIdentityController IdentityController(IServiceProvider sp) => new(sp.GetRequiredService<SqlConnectionManager>());
    private static WellBoreArchitectureFeatureCategoryController FeatureController(IServiceProvider sp) => new(sp.GetRequiredService<SqlConnectionManager>());
}
