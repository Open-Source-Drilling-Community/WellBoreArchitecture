using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Text.Json.Nodes;
using OSDC.DotnetLibraries.Drilling.SemanticCatalogue;
using OSDC.DotnetLibraries.General.DataManagement;
using OSDC.DotnetLibraries.General.DrillingProperties;
using Catalogue = OSDC.DotnetLibraries.Drilling.SemanticCatalogue.SemanticCatalogue;

namespace OSDC.Drilling.WellBoreArchitecture.Model;

/// <summary>Property-context binding for a shared Gaussian or scalar representation; does not change serialization.</summary>
[AttributeUsage(AttributeTargets.Property)]
public sealed class EngineeringQuantityAttribute(string measurand, string? uncertainty = null) : Attribute
{
    public string Measurand { get; } = measurand;
    public string? Uncertainty { get; } = uncertainty;
    public string? Reference { get; set; }
    public string? Role { get; set; }
}

/// <summary>
/// Provider-owned bindings for third-party properties and inherited classification members.
/// REST and MCP use this same registry. Domain types remain owned by their NuGet packages.
/// </summary>
public static class ProviderSemantics
{
    public const string NestedBindingsExtension = "x-osdc-semantic-bindings";

    public static JsonObject Metadata(string concept, string? role = null, string? reference = null)
    {
        return SemanticMetadata.Create(concept, role, reference,
            Catalogue.OsdcCanonicalDrilling, assertionSource: "provider-binding-registry");
    }

    public static JsonObject ResourceIdentifier(string resourceType)
    {
        JsonObject metadata = Metadata(Concepts.ResourceIdentifier);
        metadata["resourceType"] = resourceType;
        return metadata;
    }

    public static JsonObject? ForType(Type type)
    {
        if (SemanticMetadata.For(type) is JsonObject direct) return direct;
        if (type == typeof(SurfaceSectionType)) return Metadata(Concepts.SurfaceSectionKind);
        if (type == typeof(SideElementType)) return Metadata(Concepts.SideCircuitElementKind);
        if (type == typeof(FluidType)) return Metadata(Concepts.EnvironmentalFluidKind);
        if (type == typeof(MetaInfo)) return Metadata(Concepts.ResourceMetadata);
        if (type == typeof(GaussianDrillingProperty)) return Metadata(Concepts.GaussianUncertainValue);
        if (type == typeof(ScalarDrillingProperty)) return Metadata(Concepts.ScalarValueRepresentation);
        if (type == typeof(BoreholeDiameterAtAbscissaResult)) return Metadata(Concepts.BoreholeDiameterAtAbscissaResult);
        if (type == typeof(BoreholeDiameterContributor)) return Metadata(Concepts.BoreholeDiameterAtAbscissaResult, Concepts.ResultContribution);
        return null;
    }

    public static JsonObject? ForProperty(PropertyInfo property)
    {
        if (property.DeclaringType == typeof(WellBoreArchitecture) && property.Name == nameof(WellBoreArchitecture.WellBoreID))
            return ResourceIdentifier(Concepts.WellBore);
        if (SemanticMetadata.For(property) is JsonObject direct) return direct;
        if (property.GetCustomAttribute<EngineeringQuantityAttribute>() is { } engineering)
            return Metadata(engineering.Uncertainty != null ? Concepts.GaussianUncertainValue : Concepts.ScalarValueRepresentation, engineering.Role);
        if (property.PropertyType.IsEnum) return ForType(property.PropertyType);
        Type type = property.DeclaringType!;
        bool classification = typeof(IIdentity).IsAssignableFrom(type) || typeof(IIdentityAssignment).IsAssignableFrom(type) ||
            typeof(IFeatureCategory).IsAssignableFrom(type) || typeof(IFeatureOption).IsAssignableFrom(type) || typeof(IFeatureAssignment).IsAssignableFrom(type) ||
            typeof(IMembershipCategory).IsAssignableFrom(type) || typeof(IMembershipOption).IsAssignableFrom(type) || typeof(IMembershipAssignment).IsAssignableFrom(type);
        if (type != typeof(MetaInfo) && !classification) return null;
        return property.Name switch
        {
            "MetaInfo" => Metadata(Concepts.ResourceMetadata),
            "ID" or "IdentityID" or "FeatureCategoryID" or "FeatureOptionID" or "MembershipCategoryID" or "MembershipOptionID" => Metadata(Concepts.ResourceIdentifier),
            "Name" => Metadata(Concepts.ResourceName),
            "Value" => Metadata(Concepts.IdentityValue),
            "CreationDate" => Metadata(Concepts.Instant, Concepts.CreationTime, Concepts.Utc),
            "LastModificationDate" => Metadata(Concepts.Instant, Concepts.LastModificationTime, Concepts.Utc),
            "FromDate" => Metadata(Concepts.Instant, Concepts.ValidityStart, Concepts.Utc),
            "ToDate" => Metadata(Concepts.Instant, Concepts.ValidityEnd, Concepts.Utc),
            "IsExclusive" => Metadata(Concepts.CategoryExclusivity),
            "HasValidityPeriod" => Metadata(Concepts.CategoryValidityPeriodEnabled),
            _ => null
        };
    }

    private static string? UnitDescription(string? unit) => unit switch
    {
        "m" => "metres (m)", "Pa" => "pascals (Pa)", "rad/m" => "radians per metre (rad/m)",
        "kg/m" => "kilograms per metre (kg/m)", _ => unit
    };

    public static string? Description(PropertyInfo property, JsonObject? metadata)
    {
        if (property.GetCustomAttribute<EngineeringQuantityAttribute>() is { } field)
        {
            var scalar = Metadata(field.Measurand, field.Role, field.Reference);
            string text = Catalogue.Default.Get(field.Measurand).Definition;
            if (field.Role != null) text += " " + Catalogue.Default.Get(field.Role).Definition;
            text += $" Physical quantity: {Catalogue.Default.Quantity(field.Measurand)!.Name}; SI unit: {UnitDescription(Catalogue.Default.SiUnit(field.Measurand))}.";
            text += field.Uncertainty != null
                ? " GaussianValue.Mean is the expected value; StandardDeviation is nonnegative uncertainty in the same SI unit, with no origin offset. Null deviation is unspecified, not zero. MinValue/MaxValue are domain bounds in the mean's unit and reference, not confidence limits or truncation instructions."
                : " The SI value is stored at DiracDistributionValue.Value; MinValue/MaxValue are domain bounds in the same unit and reference.";
            if (scalar["reference"] is JsonNode reference)
                text += " " + Catalogue.Default.Get(reference.GetValue<string>()).Definition + " This reference applies to canonical storage and APIs; supported user-selected presentation references remain allowed.";
            return text;
        }
        if (property.Name == "WellBoreID") return "UUID of the externally owned WellBore resource to which this architecture belongs; not an embedded path or architecture UUID.";
        if (property.Name == "ComponentID") return "Stable component UUID scoped to the containing architecture; not a physical dimension.";
        if (property.Name == "LastModificationDate") return "Server-owned last-modification timestamp and optimistic-concurrency token; echo exactly on update or delete.";
        if (metadata == null) return property.GetCustomAttribute<DescriptionAttribute>()?.Description;
        return Catalogue.Default.Get(metadata["concept"]!.GetValue<string>()).Definition;
    }

    /// <summary>Relative JSON Pointers bind shared wrappers in the containing property's context.</summary>
    public static JsonObject? NestedBindings(PropertyInfo property)
    {
        var field = property.GetCustomAttribute<EngineeringQuantityAttribute>();
        if (field == null) return null;
        if (field.Uncertainty == null) return new JsonObject
        {
            ["/DiracDistributionValue/Value"] = Metadata(field.Measurand, field.Role, field.Reference),
            ["/DiracDistributionValue/MinValue"] = Metadata(field.Measurand, Concepts.DistributionLowerBound, field.Reference),
            ["/DiracDistributionValue/MaxValue"] = Metadata(field.Measurand, Concepts.DistributionUpperBound, field.Reference)
        };
        return new JsonObject
        {
            ["/GaussianValue/Mean"] = Metadata(field.Measurand, Concepts.ExpectedValue, field.Reference),
            ["/GaussianValue/StandardDeviation"] = Metadata(field.Uncertainty),
            ["/GaussianValue/MinValue"] = Metadata(field.Measurand, Concepts.DistributionLowerBound, field.Reference),
            ["/GaussianValue/MaxValue"] = Metadata(field.Measurand, Concepts.DistributionUpperBound, field.Reference)
        };
    }

    /// <summary>Annotate an existing JSON schema without changing validation keywords or accepted payload shape.</summary>
    public static JsonObject Annotate(JsonObject schema, Type type)
    {
        Walk(schema, type, schema, new HashSet<(JsonObject, Type)>());
        return schema;
    }

    public static void AnnotateDefinition(JsonObject root, string name, Type type)
    {
        if (root["$defs"]?[name] is JsonObject schema)
            Walk(schema, type, root, new HashSet<(JsonObject, Type)>());
    }

    private static void Walk(JsonObject schema, Type type, JsonObject root, HashSet<(JsonObject, Type)> seen)
    {
        type = Nullable.GetUnderlyingType(type) ?? type;
        if (!seen.Add((schema, type))) return;
        if (schema["$ref"]?.GetValue<string>() is string reference && reference.StartsWith("#/"))
        {
            JsonNode? resolved = root;
            foreach (string part in reference[2..].Split('/')) resolved = resolved?[part.Replace("~1", "/").Replace("~0", "~")];
            if (resolved is JsonObject target) Walk(target, type, root, seen);
            return;
        }
        if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Dictionary<,>))
        {
            if (schema["additionalProperties"] is JsonObject values) Walk(values, type.GetGenericArguments()[1], root, seen);
            return;
        }
        if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(List<>))
        {
            if (schema["items"] is JsonObject items) Walk(items, type.GetGenericArguments()[0], root, seen);
            return;
        }
        if (schema[SemanticMetadata.ExtensionName] == null && ForType(type) is JsonObject typeMetadata)
            schema[SemanticMetadata.ExtensionName] = typeMetadata;
        foreach (string keyword in new[] { "oneOf", "anyOf", "allOf" })
            if (schema[keyword] is JsonArray variants)
                foreach (var variant in variants.OfType<JsonObject>()) Walk(variant, type, root, seen);
        if (schema["properties"] is not JsonObject properties) return;
        foreach (var property in type.GetProperties())
        {
            if (properties[property.Name] is not JsonObject target) continue;
            var metadata = ForProperty(property);
            if (metadata != null) target[SemanticMetadata.ExtensionName] = metadata;
            if (Description(property, metadata) is string description)
            {
                string? prior = target["description"]?.GetValue<string>();
                target["description"] = property.GetCustomAttribute<EngineeringQuantityAttribute>() != null || string.IsNullOrWhiteSpace(prior)
                    ? description : prior!.EndsWith(description, StringComparison.Ordinal) ? prior : prior + " " + description;
            }
            if (NestedBindings(property) is JsonObject nested)
            {
                target[NestedBindingsExtension] = nested;
                foreach (var binding in nested)
                {
                    JsonNode? scalar = target;
                    foreach (string part in binding.Key[1..].Split('/')) scalar = scalar?["properties"]?[part];
                    if (scalar is JsonObject scalarSchema)
                    {
                        scalarSchema[SemanticMetadata.ExtensionName] = binding.Value!.DeepClone();
                        var value = binding.Value!;
                        scalarSchema["description"] = Catalogue.Default.Get(value["concept"]!.GetValue<string>()).Definition +
                            $" Physical quantity: {value["physicalQuantity"]!["name"]}; SI unit: {value["siUnit"]}.";
                    }
                }
            }
            Walk(target, property.PropertyType, root, seen);
        }
    }
}
