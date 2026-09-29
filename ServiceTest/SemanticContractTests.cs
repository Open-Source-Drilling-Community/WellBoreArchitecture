using System.Reflection;
using System.Text.Json;
using System.Text.Json.Nodes;
using Microsoft.OpenApi.Writers;
using Microsoft.OpenApi;
using Microsoft.OpenApi.Extensions;
using OSDC.DotnetLibraries.Drilling.SemanticCatalogue;
using OSDC.DotnetLibraries.General.DrillingProperties;
using OSDC.Drilling.WellBoreArchitecture.Model;
using OSDC.Drilling.WellBoreArchitecture.Service;
using OSDC.Drilling.WellBoreArchitecture.Service.Mcp.Tools;
using Swashbuckle.AspNetCore.SwaggerGen;
using Architecture = OSDC.Drilling.WellBoreArchitecture.Model.WellBoreArchitecture;

namespace ServiceTest;

public class SemanticContractTests
{
    private const string Binding = ProviderSemantics.NestedBindingsExtension;
    private const string Semantic = SemanticMetadata.ExtensionName;

    private static JsonObject Rest(Type type)
    {
        var options = new SchemaGeneratorOptions { SchemaIdSelector = t => t.FullName! };
        options.SchemaFilters.Add(new SemanticSchemaFilter());
        var generator = new SchemaGenerator(options, new JsonSerializerDataContractResolver(new JsonSerializerOptions()));
        var repository = new SchemaRepository();
        generator.GenerateSchema(type, repository);
        using var text = new StringWriter();
        var writer = new OpenApiJsonWriter(text);
        repository.Schemas[type.FullName!].SerializeAsV3(writer);
        writer.Flush();
        return JsonNode.Parse(text.ToString())!.AsObject();
    }

    [Test]
    public void MergedAndServedSchemasPreserveSemanticJsonTypesAndVersions()
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);
        while (directory != null && !File.Exists(Path.Combine(directory.FullName, "WellBoreArchitecture.sln")))
            directory = directory.Parent;
        string root = directory!.FullName;
        var source = JsonNode.Parse(File.ReadAllText(Path.Combine(root, "ModelSharedOut/json-schemas/WellBoreArchitectureFullName.json")))!;
        string mergedPath = Path.Combine(root, "Service/wwwroot/json-schema/WellBoreArchitectureMergedModel.json");
        var merged = JsonNode.Parse(File.ReadAllText(mergedPath))!;
        var document = SwaggerMiddlewareExtensions.ReadOpenApiDocument(mergedPath);
        // Exercise the exact serializer used by the HTTP middleware, including invariant number formatting.
        var served = JsonNode.Parse(document.Serialize(OpenApiSpecVersion.OpenApi3_0, OpenApiFormat.Json))!;
        int count = 0;
        foreach (var schema in source["components"]!["schemas"]!.AsObject())
        {
            if (!schema.Key.StartsWith("OSDC.Drilling.WellBoreArchitecture.Model.")) continue;
            string name = schema.Key.Split('.').Last();
            foreach (var property in schema.Value?["properties"]?.AsObject() ?? new JsonObject())
            {
                if (property.Value?[Binding] is not JsonObject binding) continue;
                count++;
                foreach (var target in new[] { merged, served })
                    Assert.That(JsonNode.DeepEquals(binding, target["components"]!["schemas"]![name]!["properties"]![property.Key]![Binding]),
                        Is.True, $"Semantic metadata changed during merge or HTTP serialization: {name}.{property.Key}");
            }
        }
        Assert.That(count, Is.EqualTo(43));
    }

    [Test]
    public void EveryEngineeringFieldHasReviewedAndIdenticalRestMcpBindings()
    {
        var definitions = McpToolArgumentHelpers.CreateWellBoreArchitectureSchema()["$defs"]!.AsObject();
        int count = 0;
        foreach (var type in typeof(Architecture).Assembly.GetTypes().Where(t => t.IsClass))
        {
            var fields = type.GetProperties().Where(p => p.PropertyType == typeof(GaussianDrillingProperty)
                || p.PropertyType == typeof(ScalarDrillingProperty)).ToArray();
            if (fields.Length == 0) continue;
            var rest = Rest(type);
            foreach (var field in fields)
            {
                count++;
                var attribute = field.GetCustomAttribute<EngineeringQuantityAttribute>();
                Assert.That(attribute, Is.Not.Null, $"Missing engineering binding: {type.Name}.{field.Name}");
                var expected = ProviderSemantics.NestedBindings(field)!;
                var restField = rest["properties"]![field.Name]!;
                var mcpField = definitions[type.Name]!["properties"]![field.Name]!;
                Assert.That(JsonNode.DeepEquals(restField[Binding], expected), Is.True, $"REST {type.Name}.{field.Name}");
                Assert.That(JsonNode.DeepEquals(mcpField[Binding], expected), Is.True, $"MCP {type.Name}.{field.Name}");
                foreach (var entry in expected)
                {
                    Assert.That(entry.Value!["catalogueVersion"]!.GetValue<string>(), Is.EqualTo("0.10.0"));
                    Assert.That(entry.Value["curationStatus"]!.GetValue<string>(), Is.EqualTo("Reviewed"));
                    Assert.That(entry.Value["physicalQuantity"]!["name"]!.GetValue<string>(), Is.Not.Empty);
                    Assert.That(entry.Value["siUnit"]!.GetValue<string>(), Is.Not.Empty);
                }
            }
        }
        Assert.That(count, Is.EqualTo(43));
    }

    [TestCase(typeof(CasingSection), "TopDepth", Concepts.AlongHoleDepth, Concepts.Wgs84AlongHoleOrigin)]
    [TestCase(typeof(CasingSection), "TopCementDepth", Concepts.AlongHoleDepth, Concepts.Wgs84AlongHoleOrigin)]
    [TestCase(typeof(WellHead), "Depth", Concepts.EllipsoidalDepth, Concepts.Wgs84)]
    [TestCase(typeof(WellHead), "CasingHangerDepth", Concepts.EllipsoidalDepth, Concepts.Wgs84)]
    [TestCase(typeof(WellHead), "TubingHangerDepth", Concepts.EllipsoidalDepth, Concepts.Wgs84)]
    [TestCase(typeof(SideConnector), "Position", Concepts.HostComponentAbscissa, Concepts.HostTopDownward)]
    [TestCase(typeof(WellBoreArchitectureFluid), "Depth", Concepts.EllipsoidalDepth, Concepts.Wgs84)]
    public void CoordinatesUseTheApprovedOriginWithoutOffsettingUncertainty(Type type, string name, string concept, string reference)
    {
        var bindings = ProviderSemantics.NestedBindings(type.GetProperty(name)!)!;
        var value = bindings["/GaussianValue/Mean"] ?? bindings["/DiracDistributionValue/Value"]!;
        Assert.That(value["concept"]!.GetValue<string>(), Is.EqualTo(concept));
        Assert.That(value["reference"]!.GetValue<string>(), Is.EqualTo(reference));
        Assert.That(value["referenceScope"]!.GetValue<string>(), Is.EqualTo("canonical-storage-and-api"));
        Assert.That(value["presentationReferencesAllowed"]!.GetValue<bool>(), Is.True);
        if (bindings["/GaussianValue/StandardDeviation"] is JsonObject deviation)
            Assert.That(deviation["reference"], Is.Null);
    }

    [Test]
    public void StressTorqueAndDifferentialPressureAreNotAbsolutePressure()
    {
        foreach (var name in new[] { "TensileStrength", "YieldStress", "TorsionalStrength", "BurstPressure", "CollapsePressure" })
        {
            var binding = ProviderSemantics.NestedBindings(typeof(CasingSectionElement).GetProperty(name)!)!;
            Assert.That(binding["/GaussianValue/Mean"]!["reference"], Is.Null);
        }
        var tensile = ProviderSemantics.NestedBindings(typeof(CasingSectionElement).GetProperty("TensileStrength")!)!;
        Assert.That(tensile["/GaussianValue/Mean"]!["concept"]!.GetValue<string>(), Is.EqualTo(Concepts.TensileStrength));
        Assert.That(tensile["/GaussianValue/Mean"]!["siUnit"]!.GetValue<string>(), Is.EqualTo("Pa"));
        Assert.That(typeof(CasingSectionElement).GetProperty("TensileCapacity"), Is.Null);
        var sideId = ProviderSemantics.NestedBindings(typeof(SideElement).GetProperty("ID")!)!;
        Assert.That(sideId["/GaussianValue/Mean"]!["concept"]!.GetValue<string>(), Is.EqualTo(Concepts.PipeDiameter));
        Assert.That(ProviderSemantics.ForProperty(typeof(SideElement).GetProperty("ComponentID")!)!["concept"]!.GetValue<string>(),
            Is.EqualTo(Concepts.ResourceIdentifier));
    }

    [Test]
    public void SharedWrappersRemainQuantityNeutralAndMetadataDoesNotChangePayloads()
    {
        Assert.That(ProviderSemantics.ForType(typeof(GaussianDrillingProperty))!["physicalQuantity"], Is.Null);
        Assert.That(ProviderSemantics.ForType(typeof(ScalarDrillingProperty))!["physicalQuantity"], Is.Null);
        var value = new CasingSection();
        string before = JsonSerializer.Serialize(value);
        Rest(typeof(CasingSection));
        McpToolArgumentHelpers.CreateWellBoreArchitectureSchema();
        Assert.That(JsonSerializer.Serialize(value), Is.EqualTo(before));
    }

    [Test]
    public void MutationsAndBackupKeepTheSameContextualBindings()
    {
        var root = McpToolArgumentHelpers.CreateWellBoreArchitectureSchema();
        var backup = McpToolArgumentHelpers.CreateWellBoreArchitectureBatchRestoreSchema();
        Assert.That(JsonNode.DeepEquals(root["$defs"]!["CasingSection"], backup["$defs"]!["CasingSection"]), Is.True);
        var details = McpToolArgumentHelpers.CreateDetailsMutationSchema();
        Assert.That(details["properties"]!["details"]!["properties"]!["Name"]![Semantic]!["concept"]!.GetValue<string>(),
            Is.EqualTo(Concepts.ResourceName));
        var definitions = root["$defs"]!;
        Assert.That(definitions["WellBoreArchitectureFeatureAssignment"]!["properties"]!["ToDate"]![Semantic]!["role"]!.GetValue<string>(),
            Is.EqualTo(Concepts.ValidityEnd));
    }
}
