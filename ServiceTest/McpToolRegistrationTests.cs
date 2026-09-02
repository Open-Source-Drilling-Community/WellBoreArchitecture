using System.Text.Json.Nodes;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.Extensions.DependencyInjection;
using ModelContextProtocol.Server;
using NORCE.Drilling.WellBoreArchitecture.Service.Controllers;
using NORCE.Drilling.WellBoreArchitecture.Service.Mcp;
using NORCE.Drilling.WellBoreArchitecture.Service.Mcp.Tools;

namespace ServiceTest;

[TestFixture]
public sealed class McpToolRegistrationTests
{
    private static readonly IReadOnlyDictionary<string, string> EndpointToolMap = new Dictionary<string, string>
    {
        ["GetAllWellBoreArchitectureId"] = "well_bore_architecture_get_all_ids",
        ["GetAllWellBoreArchitectureMetaInfo"] = "well_bore_architecture_get_all_meta_info",
        ["GetWellBoreArchitectureById"] = "well_bore_architecture_get_by_id",
        ["GetAllWellBoreArchitectureLight"] = "well_bore_architecture_get_all_light",
        ["GetAllWellBoreArchitecture"] = "well_bore_architecture_get_all",
        ["PostWellBoreArchitecture"] = "well_bore_architecture_create",
        ["PutWellBoreArchitectureById"] = "well_bore_architecture_update_by_id",
        ["DeleteWellBoreArchitectureById"] = "well_bore_architecture_delete_by_id"
    };

    private ServiceProvider _provider = null!;
    private IReadOnlyDictionary<string, IMcpTool> _tools = null!;

    [SetUp]
    public void SetUp()
    {
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddLegacyMcpTool<PingMcpTool>();
        services.AddWellBoreArchitectureRestMcpTools();
        _provider = services.BuildServiceProvider();
        _tools = _provider.GetServices<IMcpTool>().ToDictionary(tool => tool.Name);
    }

    [TearDown]
    public void TearDown() => _provider.Dispose();

    [Test]
    public void Every_non_statistics_controller_endpoint_has_a_registered_tool()
    {
        var endpoints = typeof(WellBoreArchitectureController).GetMethods()
            .Where(method => method.GetCustomAttributes(typeof(HttpMethodAttribute), true).Length > 0)
            .Select(method => method.Name);
        Assert.That(endpoints, Is.EquivalentTo(EndpointToolMap.Keys));
        Assert.That(_tools.Keys, Is.EquivalentTo(EndpointToolMap.Values.Append("ping")));
    }

    [Test]
    public void Usage_statistics_are_not_exposed() => Assert.That(_tools.Keys, Has.None.Contains("statistics"));

    [Test]
    public void Domain_tools_have_detailed_descriptions_and_explicit_object_schemas()
    {
        IMcpTool[] domainTools = _tools.Values.Where(tool => tool.Name != "ping").ToArray();

        Assert.That(domainTools.All(tool => tool.Description.Length >= 150), Is.True);
        Assert.That(domainTools.All(tool => tool.InputSchema is JsonObject), Is.True);
        Assert.That(domainTools.All(tool => tool.InputSchema?["type"]?.GetValue<string>() == "object"), Is.True);
    }

    [Test]
    public void Write_schema_describes_complete_ordered_architecture_and_external_wellbore_reference()
    {
        JsonObject schema = (JsonObject)_tools["well_bore_architecture_create"].InputSchema!;
        JsonObject definitions = (JsonObject)schema["$defs"]!;
        string json = schema.ToJsonString();

        Assert.That(definitions.Count, Is.GreaterThanOrEqualTo(16));
        Assert.That(json, Does.Contain("WellBoreID"));
        Assert.That(json, Does.Contain("external reference to the WellBore microservice"));
        Assert.That(json, Does.Contain("SurfaceSections"));
        Assert.That(json, Does.Contain("\"minItems\":1"));
        Assert.That(json, Does.Contain("ordered from top to bottom"));
        Assert.That(json, Does.Contain("CasingSectionElements"));
        Assert.That(json, Does.Contain("ElementConnectivities"));
        Assert.That(json, Does.Contain("RotatingControlDevice"));
    }

    [Test]
    public void Write_schema_documents_distribution_shapes_si_units_and_depth_references()
    {
        string createSchema = _tools["well_bore_architecture_create"].InputSchema!.ToJsonString();
        string updateSchema = _tools["well_bore_architecture_update_by_id"].InputSchema!.ToJsonString();

        Assert.That(createSchema, Does.Contain("GaussianValue"));
        Assert.That(createSchema, Does.Contain("StandardDeviation"));
        Assert.That(createSchema, Does.Contain("DiracDistributionValue"));
        Assert.That(createSchema, Does.Contain("referenced to the wellhead"));
        Assert.That(createSchema, Does.Contain("metres (m)"));
        Assert.That(createSchema, Does.Contain("pascals (Pa)"));
        Assert.That(createSchema, Does.Contain("radians per metre"));
        Assert.That(updateSchema, Does.Contain("must exactly equal wellBoreArchitecture.MetaInfo.ID"));
    }

    [Test]
    public void Protocol_tool_names_are_valid_and_unique()
    {
        string[] names = _provider.GetServices<McpServerTool>().Select(tool => tool.ProtocolTool.Name).ToArray();
        Assert.That(names, Has.Length.EqualTo(_tools.Count));
        Assert.That(names, Is.Unique);
        Assert.That(names.All(name => !name.Contains('.')), Is.True);
    }

    [Test]
    public async Task Get_by_id_requires_an_id()
    {
        JsonObject? response = await _tools["well_bore_architecture_get_by_id"].InvokeAsync(new JsonObject(), CancellationToken.None) as JsonObject;
        Assert.That(response?["status"]?.GetValue<int>(), Is.EqualTo(400));
    }

    [Test]
    public async Task Create_requires_a_request_body()
    {
        JsonObject? response = await _tools["well_bore_architecture_create"].InvokeAsync(new JsonObject(), CancellationToken.None) as JsonObject;
        Assert.That(response?["status"]?.GetValue<int>(), Is.EqualTo(400));
    }
}
