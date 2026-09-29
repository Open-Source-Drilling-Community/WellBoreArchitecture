using System.Text.Json;
using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;

namespace OSDC.Drilling.WellBoreArchitecture.Contracts;

/// <summary>Preserve JSON types in semantic extensions; the OpenAPI reader can interpret version strings as dates.</summary>
internal static class SemanticOpenApiExtensions
{
    public static void Restore(OpenApiDocument document, string json)
    {
        using var source = JsonDocument.Parse(json);
        if (!source.RootElement.TryGetProperty("components", out var components) ||
            !components.TryGetProperty("schemas", out var schemas)) return;
        foreach (var schema in schemas.EnumerateObject())
            if (document.Components.Schemas.TryGetValue(schema.Name, out var target)) RestoreSchema(target, schema.Value);
    }

    private static void RestoreSchema(OpenApiSchema target, JsonElement source)
    {
        if (source.ValueKind != JsonValueKind.Object) return;
        foreach (var property in source.EnumerateObject())
        {
            if (property.Name.StartsWith("x-osdc-semantic", System.StringComparison.Ordinal))
                target.Extensions[property.Name] = ReadJson(property.Value);
            else if (property.Name == "properties")
            {
                foreach (var child in property.Value.EnumerateObject())
                    if (target.Properties.TryGetValue(child.Name, out var childSchema)) RestoreSchema(childSchema, child.Value);
            }
            else if (property.Name == "items" && target.Items != null) RestoreSchema(target.Items, property.Value);
            else if (property.Name == "additionalProperties" && target.AdditionalProperties != null)
                RestoreSchema(target.AdditionalProperties, property.Value);
            else if (property.Name is "allOf" or "anyOf" or "oneOf")
            {
                var variants = property.Name == "allOf" ? target.AllOf : property.Name == "anyOf" ? target.AnyOf : target.OneOf;
                int index = 0;
                foreach (var child in property.Value.EnumerateArray()) RestoreSchema(variants[index++], child);
            }
        }
    }

    private static IOpenApiAny ReadJson(JsonElement value)
    {
        switch (value.ValueKind)
        {
            case JsonValueKind.Object:
                var obj = new OpenApiObject();
                foreach (var property in value.EnumerateObject()) obj[property.Name] = ReadJson(property.Value);
                return obj;
            case JsonValueKind.Array:
                var array = new OpenApiArray();
                foreach (var item in value.EnumerateArray()) array.Add(ReadJson(item));
                return array;
            case JsonValueKind.String: return new OpenApiString(value.GetString());
            case JsonValueKind.Number:
                if (value.TryGetInt32(out int integer)) return new OpenApiInteger(integer);
                if (value.TryGetInt64(out long large)) return new OpenApiLong(large);
                return new OpenApiDouble(value.GetDouble());
            case JsonValueKind.True: return new OpenApiBoolean(true);
            case JsonValueKind.False: return new OpenApiBoolean(false);
            default: return new OpenApiNull();
        }
    }
}
