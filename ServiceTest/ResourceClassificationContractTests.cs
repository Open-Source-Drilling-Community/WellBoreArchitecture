using System.Globalization;
using System.Text.Json.Nodes;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi.Writers;
using OSDC.Drilling.WellBoreArchitecture.Service;
using OSDC.Drilling.WellBoreArchitecture.Service.Controllers;
using Swashbuckle.AspNetCore.Swagger;

namespace ServiceTest;

public class ResourceClassificationContractTests
{
    [Test]
    public async Task SharedClassificationPreservesPublishedOpenApiSchemas()
    {
        var builder = WebApplication.CreateBuilder(new WebApplicationOptions
        { ApplicationName = typeof(WellBoreArchitectureController).Assembly.FullName });
        var services = builder.Services;
        services.AddLogging();
        services.AddControllers().AddApplicationPart(typeof(WellBoreArchitectureController).Assembly)
            .AddJsonOptions(options => JsonSettings.ApplyTo(options.JsonSerializerOptions));
        services.AddSwaggerGen(options =>
        {
            options.CustomSchemaIds(type => type.FullName);
            options.SchemaFilter<SemanticSchemaFilter>();
        });
        await using var app = builder.Build();
        var document = app.Services.GetRequiredService<ISwaggerProvider>().GetSwagger("v1");
        using var text = new StringWriter(CultureInfo.InvariantCulture);
        document.SerializeAsV3(new OpenApiJsonWriter(text));
        var actual = JsonNode.Parse(text.ToString())!;
        var directory = new DirectoryInfo(AppContext.BaseDirectory);
        while (directory != null && !File.Exists(Path.Combine(directory.FullName, "WellBoreArchitecture.sln")))
            directory = directory.Parent;
        Assert.That(directory, Is.Not.Null);
        var expected = JsonNode.Parse(File.ReadAllText(Path.Combine(directory!.FullName,
            "ModelSharedOut", "json-schemas", "WellBoreArchitectureFullName.json")))!;
        Assert.That(JsonNode.DeepEquals(actual["components"]!["schemas"], expected["components"]!["schemas"]), Is.True,
            "Inheritance must preserve the published model schemas, including nullability and property names.");
        Assert.That(JsonNode.DeepEquals(actual["paths"], expected["paths"]), Is.True,
            "Classification extraction must not change REST operations.");
    }
}
