using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using ModelContextProtocol.Server;
using System;
using System.Linq;
using System.Text.Json.Nodes;
using System.Threading;
using System.Threading.Tasks;

namespace OSDC.Drilling.WellBoreArchitecture.Service.Mcp;

public static class McpServiceCollectionExtensions
{
    public static IServiceCollection AddLegacyMcpTool<TTool>(this IServiceCollection services)
        where TTool : class, IMcpTool
    {
        services.AddSingleton<TTool>();
        services.AddSingleton<IMcpTool>(sp => sp.GetRequiredService<TTool>());
        services.AddSingleton<McpServerTool>(sp =>
        {
            var tool = sp.GetRequiredService<TTool>();
            var loggerFactory = sp.GetRequiredService<ILoggerFactory>();
            return new LegacyMcpServerToolAdapter(tool, loggerFactory);
        });

        return services;
    }

    public static IServiceCollection AddLegacyMcpTool(this IServiceCollection services, string name, string description,
        JsonNode? inputSchema, Func<IServiceProvider, JsonObject?, CancellationToken, Task<JsonNode?>> invokeAsync)
    {
        services.AddSingleton<IMcpTool>(sp => new DelegateMcpTool(
            name, description, inputSchema ?? EmptyInputSchema(), InferOutputSchema(name), InferBehavior(name),
            (arguments, cancellationToken) => invokeAsync(sp, arguments, cancellationToken)));
        services.AddSingleton<McpServerTool>(sp => new LegacyMcpServerToolAdapter(
            sp.GetServices<IMcpTool>().Last(tool => tool.Name == name), sp.GetRequiredService<ILoggerFactory>()));
        return services;
    }

    private sealed class DelegateMcpTool : IMcpTool
    {
        private readonly Func<JsonObject?, CancellationToken, Task<JsonNode?>> _invokeAsync;
        public DelegateMcpTool(string name, string description, JsonNode inputSchema, JsonNode outputSchema, McpToolBehavior behavior,
            Func<JsonObject?, CancellationToken, Task<JsonNode?>> invokeAsync)
        {
            Name = name;
            Description = description;
            InputSchema = inputSchema;
            OutputSchema = outputSchema;
            Behavior = behavior;
            _invokeAsync = invokeAsync;
        }
        public string Name { get; }
        public string Description { get; }
        public JsonNode InputSchema { get; }
        public JsonNode OutputSchema { get; }
        public McpToolBehavior Behavior { get; }
        public Task<JsonNode?> InvokeAsync(JsonObject? arguments, CancellationToken cancellationToken)
        {
            JsonObject? properties = InputSchema["properties"] as JsonObject;
            string? unexpected = arguments?.Select(item => item.Key)
                .FirstOrDefault(key => properties == null || !properties.ContainsKey(key));
            return unexpected == null
                ? _invokeAsync(arguments, cancellationToken)
                : Task.FromResult<JsonNode?>(new JsonObject { ["status"] = 400, ["error"] = $"Unexpected argument '{unexpected}'." });
        }
    }

    private static McpToolBehavior InferBehavior(string name)
    {
        bool readOnly = name.Contains("_get_", StringComparison.Ordinal) || name.EndsWith("_get_all", StringComparison.Ordinal) ||
                        name.EndsWith("_search", StringComparison.Ordinal) || name.EndsWith("_batch_export", StringComparison.Ordinal);
        bool destructive = name.Contains("_delete_", StringComparison.Ordinal) || name.EndsWith("_batch_restore", StringComparison.Ordinal);
        bool idempotent = readOnly || name.Contains("_update", StringComparison.Ordinal) || name.Contains("_delete_", StringComparison.Ordinal) ||
                          name.EndsWith("_reorder", StringComparison.Ordinal);
        string title = string.Join(' ', name.Split('_').Select(word => char.ToUpperInvariant(word[0]) + word[1..]));
        return new McpToolBehavior(title, readOnly, destructive, idempotent);
    }

    private static JsonNode InferOutputSchema(string name)
    {
        if (name.EndsWith("_get_all_ids", StringComparison.Ordinal)) return Tools.McpToolArgumentHelpers.CreateIdsOutputSchema();
        if (name.EndsWith("_get_all_meta_info", StringComparison.Ordinal)) return Tools.McpToolArgumentHelpers.CreateMetaInfoListOutputSchema();
        if (name == "well_bore_architecture_search") return Tools.McpToolArgumentHelpers.CreateSearchOutputSchema();
        if (name == "well_bore_architecture_get_by_id" || name.Contains("_assignment_", StringComparison.Ordinal) ||
            name.Contains("_surface_section_", StringComparison.Ordinal) || name.Contains("_casing_section_", StringComparison.Ordinal) ||
            name is "well_bore_architecture_update_by_id" or "well_bore_architecture_details_update" or "well_bore_architecture_well_bore_link_update")
            return Tools.McpToolArgumentHelpers.CreateArchitectureOutputSchema();
        if (name == "well_bore_architecture_get_all") return Tools.McpToolArgumentHelpers.CreateArchitectureListOutputSchema();
        if (name == "well_bore_architecture_get_all_light") return Tools.McpToolArgumentHelpers.CreateArchitectureLightListOutputSchema();
        if (name.StartsWith("well_bore_architecture_identity_", StringComparison.Ordinal))
            return name.EndsWith("_get_by_id", StringComparison.Ordinal)
                ? Tools.McpToolArgumentHelpers.CreateIdentityOutputSchema()
                : name.EndsWith("_get_all", StringComparison.Ordinal)
                    ? Tools.McpToolArgumentHelpers.CreateIdentityListOutputSchema()
                    : Tools.McpToolArgumentHelpers.CreateGenericOutputSchema();
        if (name.StartsWith("well_bore_architecture_feature_category_", StringComparison.Ordinal))
            return name.EndsWith("_get_by_id", StringComparison.Ordinal)
                ? Tools.McpToolArgumentHelpers.CreateFeatureCategoryOutputSchema()
                : name.EndsWith("_get_all", StringComparison.Ordinal)
                    ? Tools.McpToolArgumentHelpers.CreateFeatureCategoryListOutputSchema()
                    : Tools.McpToolArgumentHelpers.CreateGenericOutputSchema();
        return Tools.McpToolArgumentHelpers.CreateGenericOutputSchema();
    }

    private static JsonNode EmptyInputSchema() => JsonNode.Parse("""{"type":"object","additionalProperties":false}""")!;
}
