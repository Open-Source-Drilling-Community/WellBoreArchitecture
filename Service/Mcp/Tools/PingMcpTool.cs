using System;
using System.Text.Json.Nodes;
using System.Threading;
using System.Threading.Tasks;
using OSDC.Drilling.WellBoreArchitecture.Service.Mcp;

namespace OSDC.Drilling.WellBoreArchitecture.Service.Mcp.Tools;

public sealed class PingMcpTool : IMcpTool
{
    public string Name => "ping";

    public string Description => "Returns a pong response so clients can verify MCP connectivity.";

    public McpToolBehavior Behavior => new("Ping", true, false, true);

    public JsonNode InputSchema => McpToolArgumentHelpers.CreateEmptySchema();

    public JsonNode OutputSchema => new JsonObject
    {
        ["type"] = "object",
        ["properties"] = new JsonObject
        {
            ["message"] = new JsonObject { ["type"] = "string", ["const"] = "pong" },
            ["timestamp"] = new JsonObject { ["type"] = "string", ["format"] = "date-time" }
        },
        ["required"] = new JsonArray("message", "timestamp"),
        ["additionalProperties"] = false
    };

    public Task<JsonNode?> InvokeAsync(JsonObject? arguments, CancellationToken cancellationToken)
    {
        var payload = new JsonObject
        {
            ["message"] = "pong",
            ["timestamp"] = DateTimeOffset.UtcNow.ToString("O")
        };

        return Task.FromResult<JsonNode?>(payload);
    }
}
