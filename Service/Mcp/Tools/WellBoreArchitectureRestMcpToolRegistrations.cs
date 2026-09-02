using System;
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
        services.AddLegacyMcpTool("well_bore_architecture_create", "Persist a new complete wellbore architecture. Generate a non-empty wellBoreArchitecture.MetaInfo.ID first; an existing UUID produces a conflict. Supply at least one SurfaceSection, preserve top-to-bottom ordering, use WellBoreID only as an external reference, and encode physical values in SI through GaussianValue or DiracDistributionValue.", McpToolArgumentHelpers.CreateWellBoreArchitectureSchema(),
            (sp, args, ct) => InvokeWithBody<ArchitectureModel>(args, "wellBoreArchitecture", ct, data => Controller(sp).PostWellBoreArchitecture(data)));
        services.AddLegacyMcpTool("well_bore_architecture_update_by_id", "Replace an existing wellbore architecture. The path id must exactly match wellBoreArchitecture.MetaInfo.ID. Send the complete desired representation because omitted collections may be lost, retain at least one SurfaceSection, update LastModificationDate, preserve ordering/reference conventions, and use SI distribution values.", McpToolArgumentHelpers.CreateWellBoreArchitectureSchema(includeId: true),
            (sp, args, ct) => InvokeWithIdAndBody<ArchitectureModel>(args, "wellBoreArchitecture", ct, (id, data) => Controller(sp).PutWellBoreArchitectureById(id, data)));
        services.AddLegacyMcpTool("well_bore_architecture_delete_by_id", "Permanently delete the stored wellbore-architecture resource identified by UUID. Use a read operation first when the target is uncertain. This accepts the architecture's MetaInfo.ID, not its referenced WellBoreID, and returns not found for an unknown resource.", McpToolArgumentHelpers.CreateGuidSchema("id", "UUID from wellBoreArchitecture.MetaInfo.ID of the architecture to delete."),
            (sp, args, ct) => InvokeDelete(args, ct, id => Controller(sp).DeleteWellBoreArchitectureById(id)));
        return services;
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
    private static WellBoreArchitectureController Controller(IServiceProvider sp) => new(
        sp.GetRequiredService<ILogger<WellBoreArchitectureManager>>(), sp.GetRequiredService<SqlConnectionManager>());
}
