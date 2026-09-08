using System.Text.Json.Nodes;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.Extensions.DependencyInjection;
using ModelContextProtocol.Server;
using OSDC.Drilling.WellBoreArchitecture.Service.Controllers;
using OSDC.Drilling.WellBoreArchitecture.Service.Mcp;
using OSDC.Drilling.WellBoreArchitecture.Service.Mcp.Tools;

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
        ["DeleteWellBoreArchitectureById"] = "well_bore_architecture_delete_by_id",
        ["BatchExportWellBoreArchitectures"] = "well_bore_architecture_batch_export",
        ["BatchRestoreWellBoreArchitectures"] = "well_bore_architecture_batch_restore",
        ["ValidateExternalReferences"] = "well_bore_architecture_validate_external_references",
        ["AuditExternalReferences"] = "well_bore_architecture_audit_external_references",
        ["IdentityGetAll"] = "well_bore_architecture_identity_get_all",
        ["IdentityGetById"] = "well_bore_architecture_identity_get_by_id",
        ["IdentityCreate"] = "well_bore_architecture_identity_create",
        ["IdentityUpdate"] = "well_bore_architecture_identity_update_by_id",
        ["IdentityDelete"] = "well_bore_architecture_identity_delete_by_id",
        ["FeatureGetAll"] = "well_bore_architecture_feature_category_get_all",
        ["FeatureGetById"] = "well_bore_architecture_feature_category_get_by_id",
        ["FeatureCreate"] = "well_bore_architecture_feature_category_create",
        ["FeatureUpdate"] = "well_bore_architecture_feature_category_update_by_id",
        ["FeatureDelete"] = "well_bore_architecture_feature_category_delete_by_id"
    };

    private static readonly string[] AdditionalToolNames =
    [
        "well_bore_architecture_search",
        "well_bore_architecture_details_update",
        "well_bore_architecture_well_bore_link_update",
        "well_bore_architecture_identity_assignment_add",
        "well_bore_architecture_identity_assignment_update_by_id",
        "well_bore_architecture_identity_assignment_delete_by_id",
        "well_bore_architecture_feature_assignment_add",
        "well_bore_architecture_feature_assignment_update_by_id",
        "well_bore_architecture_feature_assignment_delete_by_id",
        "well_bore_architecture_surface_section_add",
        "well_bore_architecture_surface_section_update_by_id",
        "well_bore_architecture_surface_section_delete_by_id",
        "well_bore_architecture_surface_section_reorder",
        "well_bore_architecture_casing_section_add",
        "well_bore_architecture_casing_section_update_by_id",
        "well_bore_architecture_casing_section_delete_by_id",
        "well_bore_architecture_casing_section_reorder",
        "well_bore_architecture_identity_get_all_ids",
        "well_bore_architecture_identity_get_all_meta_info",
        "well_bore_architecture_feature_category_get_all_ids",
        "well_bore_architecture_feature_category_get_all_meta_info"
    ];

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
    public void Action_result_converter_preserves_declared_model_property_names()
    {
        ActionResult<CasingPayload> actionResult = new CasingPayload
        {
            MetaInfo = new CasingMetaInfo { ID = Guid.NewGuid() }
        };

        JsonObject response = McpActionResultConverter.FromActionResult(actionResult);
        JsonObject data = (JsonObject)response["data"]!;
        JsonObject metaInfo = (JsonObject)data["MetaInfo"]!;

        Assert.Multiple(() =>
        {
            Assert.That(metaInfo["ID"]?.GetValue<Guid>(), Is.EqualTo(actionResult.Value!.MetaInfo.ID));
            Assert.That(data.ContainsKey("metaInfo"), Is.False);
            Assert.That(metaInfo.ContainsKey("id"), Is.False);
        });
    }

    [Test]
    public void Every_non_statistics_controller_endpoint_has_a_registered_tool()
    {
        var endpoints = typeof(WellBoreArchitectureController).GetMethods()
            .Where(method => method.GetCustomAttributes(typeof(HttpMethodAttribute), true).Length > 0)
            .Select(method => method.Name);
        Assert.That(endpoints, Is.EquivalentTo(EndpointToolMap.Keys.Take(12)));
        Assert.That(_tools.Keys, Is.EquivalentTo(EndpointToolMap.Values.Concat(AdditionalToolNames).Append("ping")));
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
        Assert.That(domainTools.All(tool => tool.OutputSchema is JsonObject), Is.True);
        Assert.That(domainTools.All(tool => tool.OutputSchema["type"]?.GetValue<string>() == "object"), Is.True);
    }

    [Test]
    public void Write_schema_describes_complete_ordered_architecture_and_external_wellbore_reference()
    {
        JsonObject schema = (JsonObject)_tools["well_bore_architecture_create"].InputSchema!;
        JsonObject definitions = (JsonObject)schema["$defs"]!;
        JsonObject architecture = (JsonObject)definitions["WellBoreArchitecture"]!;
        JsonObject properties = (JsonObject)architecture["properties"]!;
        JsonObject surfaceSections = (JsonObject)properties["SurfaceSections"]!;
        string[] required = architecture["required"]!.AsArray().Select(node => node!.GetValue<string>()).ToArray();
        string json = schema.ToJsonString();

        Assert.That(definitions.Count, Is.GreaterThanOrEqualTo(16));
        Assert.That(json, Does.Contain("WellBoreID"));
        Assert.That(json, Does.Contain("external reference to the WellBore microservice"));
        Assert.That(json, Does.Contain("SurfaceSections"));
        Assert.That(surfaceSections.ContainsKey("minItems"), Is.False);
        Assert.That(required, Does.Not.Contain("SurfaceSections"));
        Assert.That(surfaceSections["description"]?.GetValue<string>(), Does.Contain("may be omitted or empty"));
        Assert.That(json, Does.Contain("ordered from top to bottom"));
        Assert.That(json, Does.Contain("CasingSectionElements"));
        Assert.That(json, Does.Contain("ElementConnectivities"));
        Assert.That(json, Does.Contain("ComponentID"));
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
        Assert.That(createSchema, Does.Contain("referenced to the WGS84 datum"));
        Assert.That(createSchema, Does.Contain("UI-only display transformations"));
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

    [Test]
    public void Create_returns_the_architecture_with_server_owned_concurrency_timestamps()
    {
        IMcpTool create = _tools["well_bore_architecture_create"];
        string outputSchema = create.OutputSchema.ToJsonString();

        Assert.Multiple(() =>
        {
            Assert.That(create.Description, Does.Contain("return it with server-owned CreationDate and LastModificationDate"));
            Assert.That(outputSchema, Does.Contain("WellBoreArchitecture"));
            Assert.That(outputSchema, Does.Contain("Server-owned optimistic-concurrency token"));
        });
    }

    [Test]
    public void Architecture_mutations_require_optimistic_concurrency_tokens()
    {
        foreach (string name in new[]
                 {
                     "well_bore_architecture_update_by_id", "well_bore_architecture_delete_by_id",
                     "well_bore_architecture_details_update", "well_bore_architecture_well_bore_link_update",
                     "well_bore_architecture_identity_assignment_add", "well_bore_architecture_identity_assignment_update_by_id",
                     "well_bore_architecture_identity_assignment_delete_by_id", "well_bore_architecture_feature_assignment_add",
                     "well_bore_architecture_feature_assignment_update_by_id", "well_bore_architecture_feature_assignment_delete_by_id",
                     "well_bore_architecture_surface_section_add", "well_bore_architecture_surface_section_update_by_id",
                     "well_bore_architecture_surface_section_delete_by_id", "well_bore_architecture_surface_section_reorder",
                     "well_bore_architecture_casing_section_add", "well_bore_architecture_casing_section_update_by_id",
                     "well_bore_architecture_casing_section_delete_by_id", "well_bore_architecture_casing_section_reorder"
                 })
        {
            JsonObject schema = (JsonObject)_tools[name].InputSchema!;
            Assert.That(schema["required"]!.AsArray().Select(node => node!.GetValue<string>()),
                Does.Contain("expectedModifiedUtc"), name);
            Assert.That(schema["additionalProperties"]!.GetValue<bool>(), Is.False, name);
        }
    }

    [Test]
    public void Search_is_bounded_and_supports_domain_filters()
    {
        string schema = _tools["well_bore_architecture_search"].InputSchema!.ToJsonString();
        Assert.That(schema, Does.Contain("\"maximum\":200"));
        Assert.That(schema, Does.Contain("wellBoreId"));
        Assert.That(schema, Does.Contain("identityValue"));
        Assert.That(schema, Does.Contain("featureCategoryId"));
        Assert.That(schema, Does.Contain("modifiedFromUtc"));
        Assert.That(schema, Does.Contain("isLinked"));
    }

    [Test]
    public void Section_mutations_use_stable_ids_and_explicit_ordering()
    {
        string add = _tools["well_bore_architecture_surface_section_add"].InputSchema!.ToJsonString();
        string update = _tools["well_bore_architecture_casing_section_update_by_id"].InputSchema!.ToJsonString();
        string reorder = _tools["well_bore_architecture_surface_section_reorder"].InputSchema!.ToJsonString();
        Assert.Multiple(() =>
        {
            Assert.That(add, Does.Contain("ComponentID"));
            Assert.That(add, Does.Contain("insertAt"));
            Assert.That(update, Does.Contain("componentId"));
            Assert.That(reorder, Does.Contain("orderedComponentIds"));
            Assert.That(reorder, Does.Contain("\"uniqueItems\":true"));
        });
    }

    [Test]
    public void Restore_requires_explicit_consent_for_normalized_name_mapping()
    {
        string schema = _tools["well_bore_architecture_batch_restore"].InputSchema!.ToJsonString();
        Assert.That(schema, Does.Contain("AllowNormalizedNameMapping"));
        Assert.That(schema, Does.Contain("\"default\":false"));
    }

    [Test]
    public void External_reference_tools_are_bounded_read_only_and_strict()
    {
        IMcpTool validate = _tools["well_bore_architecture_validate_external_references"];
        IMcpTool audit = _tools["well_bore_architecture_audit_external_references"];
        string auditInput = audit.InputSchema.ToJsonString();
        string auditOutput = audit.OutputSchema.ToJsonString();
        Assert.Multiple(() =>
        {
            Assert.That(validate.Behavior.ReadOnlyHint, Is.True);
            Assert.That(audit.Behavior.ReadOnlyHint, Is.True);
            Assert.That(auditInput, Does.Contain("\"maximum\":100"));
            Assert.That(auditInput, Does.Contain("WellBoreArchitectureIDs"));
            Assert.That(auditOutput, Does.Contain("UnavailableCount"));
            Assert.That(auditOutput, Does.Contain("WellBoreExists"));
        });
    }

    [Test]
    public void Catalog_metadata_rejects_unknown_properties()
    {
        foreach (string name in new[]
                 {
                     "well_bore_architecture_identity_create",
                     "well_bore_architecture_feature_category_create"
                 })
        {
            string schema = _tools[name].InputSchema!.ToJsonString();
            Assert.That(schema, Does.Not.Contain("\"additionalProperties\":true"), name);
        }
    }

    [Test]
    public void Protocol_contract_publishes_output_schemas_and_safety_annotations()
    {
        McpServerTool[] tools = _provider.GetServices<McpServerTool>().ToArray();
        Assert.That(tools.All(tool => tool.ProtocolTool.OutputSchema.HasValue), Is.True);
        Assert.That(tools.All(tool => tool.ProtocolTool.Annotations != null), Is.True);

        IMcpTool search = _tools["well_bore_architecture_search"];
        IMcpTool delete = _tools["well_bore_architecture_delete_by_id"];
        Assert.Multiple(() =>
        {
            Assert.That(search.Behavior.ReadOnlyHint, Is.True);
            Assert.That(search.Behavior.DestructiveHint, Is.False);
            Assert.That(delete.Behavior.ReadOnlyHint, Is.False);
            Assert.That(delete.Behavior.DestructiveHint, Is.True);
            Assert.That(delete.Behavior.IdempotentHint, Is.True);
        });
    }

    [Test]
    public async Task Unexpected_arguments_are_rejected_before_invocation()
    {
        JsonObject? response = await _tools["well_bore_architecture_get_all_ids"]
            .InvokeAsync(new JsonObject { ["typo"] = true }, CancellationToken.None) as JsonObject;
        Assert.That(response?["status"]?.GetValue<int>(), Is.EqualTo(400));
    }

    private sealed class CasingPayload
    {
        public required CasingMetaInfo MetaInfo { get; init; }
    }

    private sealed class CasingMetaInfo
    {
        public Guid ID { get; init; }
    }
}
