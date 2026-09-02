using System;
using System.Text.Json.Nodes;

namespace OSDC.Drilling.WellBoreArchitecture.Service.Mcp.Tools;

internal static class McpToolArgumentHelpers
{
    public static JsonObject CreateEmptySchema() => new()
    {
        ["type"] = "object", ["properties"] = new JsonObject(), ["additionalProperties"] = false
    };

    public static JsonObject CreateGuidSchema(string key, string description) => new()
    {
        ["type"] = "object",
        ["properties"] = new JsonObject { [key] = String(description, "uuid") },
        ["required"] = new JsonArray(key),
        ["additionalProperties"] = false
    };

    public static JsonObject CreateWellBoreArchitectureSchema(bool includeId = false)
    {
        var properties = new JsonObject
        {
            ["wellBoreArchitecture"] = Ref("WellBoreArchitecture", "Complete wellbore-architecture representation. JSON property names are case-sensitive and use PascalCase.")
        };
        var required = new JsonArray("wellBoreArchitecture");
        if (includeId)
        {
            properties["id"] = String("UUID of the persisted architecture. It must exactly equal wellBoreArchitecture.MetaInfo.ID.", "uuid");
            required.Add("id");
        }

        return new JsonObject
        {
            ["type"] = "object",
            ["properties"] = properties,
            ["required"] = required,
            ["additionalProperties"] = false,
            ["$defs"] = Definitions()
        };
    }

    private static JsonObject Definitions() => new()
    {
        ["WellBoreArchitecture"] = Object(
            "Complete wellbore construction architecture. Physical values are SI. Depth references are field-specific and described below; the payload itself does not store a selected display-unit system.",
            new JsonObject
            {
                ["MetaInfo"] = Ref("MetaInfo", "Resource metadata. MetaInfo.ID is supplied by the caller and is the persistent architecture UUID."),
                ["Name"] = NullableString("Human-readable architecture name."),
                ["Description"] = NullableString("Human-readable description of the well construction design or revision."),
                ["CreationDate"] = NullableDateTime("Creation timestamp in ISO 8601 format. Use a UTC offset where possible."),
                ["LastModificationDate"] = NullableDateTime("Last-modification timestamp in ISO 8601 format. Update this when replacing the architecture."),
                ["WellBoreArchitectureIdentityAssignments"] = Array("WellBoreArchitectureIdentityAssignment", "Identity values assigned to the architecture.", nullable: true),
                ["WellBoreArchitectureFeatureAssignments"] = Array("WellBoreArchitectureFeatureAssignment", "Feature options assigned to the architecture.", nullable: true),
                ["WellBoreID"] = NullableUuid("UUID of the WellBore to which this architecture belongs. This is an external reference to the WellBore microservice, not an embedded WellBore."),
                ["WellHead"] = Ref("WellHead", "Wellhead dimensions and depth/hanger locations."),
                ["FluidsAboveGroundLevel"] = Array("WellBoreArchitectureFluid", "Ordered fluid layers above ground or mudline. The last listed fluid extends to ground level."),
                ["SurfaceSections"] = Array("SurfaceSection", "Surface equipment sections above the wellhead, ordered from top to bottom. At least one item is required because the service calculation rejects an empty list.", minItems: 1),
                ["CasingSections"] = Array("CasingSection", "Casing sections beginning at the wellhead and ordered from top to bottom.")
            }, "MetaInfo", "SurfaceSections"),

        ["WellBoreArchitectureIdentityAssignment"] = Object("One architecture-specific identity value.", new JsonObject
        {
            ["ID"] = String("Caller-generated assignment UUID.", "uuid"),
            ["IdentityID"] = String("UUID of an existing identity definition.", "uuid"),
            ["Value"] = NullableString("Architecture-specific identity value.")
        }, "ID", "IdentityID"),

        ["WellBoreArchitectureFeatureAssignment"] = Object("One architecture feature assignment.", new JsonObject
        {
            ["ID"] = String("Caller-generated assignment UUID.", "uuid"),
            ["FeatureCategoryID"] = String("UUID of an existing feature category.", "uuid"),
            ["FeatureOptionID"] = String("UUID of an option in that category.", "uuid"),
            ["FromDate"] = NullableDateTime("Optional validity start; allowed only when the category has a validity period."),
            ["ToDate"] = NullableDateTime("Optional validity end; must not precede FromDate.")
        }, "ID", "FeatureCategoryID", "FeatureOptionID"),

        ["MetaInfo"] = Object("Shared resource metadata containing the caller-owned UUID and optional HTTP location fields.", new JsonObject
        {
            ["ID"] = String("Non-empty UUID identifying the architecture. Generate it before create; the service does not assign it.", "uuid"),
            ["HttpHostName"] = NullableString("Optional source-service host metadata."),
            ["HttpHostBasePath"] = NullableString("Optional source-service base-path metadata."),
            ["HttpEndPoint"] = NullableString("Optional source-service endpoint metadata.")
        }, "ID"),

        ["WellHead"] = Object("Wellhead geometry and depth locations. Each physical field is wrapped as a scalar or Gaussian drilling property.", new JsonObject
        {
            ["MaxOD"] = NullableRef("ScalarDrillingProperty", "Maximum wellhead outside diameter in metres (m), serialized under DiracDistributionValue.Value."),
            ["MinOD"] = NullableRef("ScalarDrillingProperty", "Minimum wellhead outside diameter in metres (m), serialized under DiracDistributionValue.Value."),
            ["Depth"] = NullableRef("GaussianDrillingProperty", "Wellhead depth in metres (m), expressed in the caller's consistently selected depth reference. Mean and standard deviation are under GaussianValue."),
            ["CasingHangerDepth"] = NullableRef("ScalarDrillingProperty", "Casing-hanger depth in metres (m) in the same depth reference used for WellHead.Depth."),
            ["TubingHangerDepth"] = NullableRef("ScalarDrillingProperty", "Tubing-hanger depth in metres (m) in the same depth reference used for WellHead.Depth.")
        }),

        ["WellBoreArchitectureFluid"] = Object("A fluid layer above ground or mudline and the depth of its boundary.", new JsonObject
        {
            ["Fluid"] = Enum("Fluid type for the layer.", "Air", "Water"),
            ["Depth"] = Ref("GaussianDrillingProperty", "Boundary depth in metres (m), using the same configured depth reference as the architecture. Mean and standard deviation are SI values under GaussianValue.")
        }),

        ["SurfaceSection"] = Object("Surface well-control or riser section above the wellhead, with uncertain dimensions/material properties and optional side circuitry.", new JsonObject
        {
            ["Type"] = Enum("Surface-section classification.", "Unknown", "BOP", "HighPressureRiser", "LowPressureRiser", "MarineRiser", "ExpansionJoint", "BellNipple", "Diverter", "RotatingControlDevice"),
            ["SectionLength"] = NullableGaussian("Section length in metres (m)."),
            ["BodyOD"] = NullableGaussian("Body outside diameter in metres (m)."),
            ["BodyID"] = NullableGaussian("Body inside diameter in metres (m)."),
            ["ConnectionType"] = NullableString("Joint or connection-thread description."),
            ["Grade"] = NullableString("Material grade."),
            ["MaterialDensity"] = NullableGaussian("Material density in kilograms per cubic metre (kg/m³)."),
            ["YoungModulus"] = NullableGaussian("Young's modulus in pascals (Pa)."),
            ["LinearWeight"] = NullableGaussian("Linear mass density in kilograms per metre (kg/m)."),
            ["TensileStrength"] = NullableGaussian("Tensile strength in pascals (Pa)."),
            ["BurstPressure"] = NullableGaussian("Burst pressure in pascals (Pa)."),
            ["CollapsePressure"] = NullableGaussian("Collapse pressure in pascals (Pa)."),
            ["YieldStress"] = NullableGaussian("Yield stress in pascals (Pa)."),
            ["MakeUpTorqueRecommended"] = NullableScalar("Recommended make-up torque in newton metres (N·m)."),
            ["SideConnectors"] = Array("SideConnector", "Side ports and their connected auxiliary flow-circuit networks.", nullable: true)
        }),

        ["SideConnector"] = Object("Connection point from a surface section into an auxiliary flow circuit.", new JsonObject
        {
            ["Position"] = Ref("GaussianDrillingProperty", "Position along the host section in metres (m)."),
            ["VerticalDepth"] = Ref("GaussianDrillingProperty", "Vertical depth in metres (m), using the architecture's configured depth reference."),
            ["FirstSideElement"] = NullableRef("SideElement", "Root element of the side-circuit network."),
            ["ElementConnectivities"] = Array("ElementConnectivity", "Connectivity edges between side-circuit elements.", nullable: true)
        }),

        ["SideElement"] = Object("Pipe, hose, valve, choke, or pump in an auxiliary side circuit.", new JsonObject
        {
            ["Name"] = NullableString("Human-readable side-element name."),
            ["Type"] = Enum("Side-element classification.", "Unknown", "Pipe", "Hose", "GateValve", "Choke", "Pump"),
            ["Length"] = Ref("GaussianDrillingProperty", "Element length in metres (m)."),
            ["TopVerticalDepth"] = Ref("GaussianDrillingProperty", "Vertical depth of the element top in metres (m), using the architecture's configured depth reference."),
            ["OD"] = Ref("GaussianDrillingProperty", "Typical outside diameter in metres (m)."),
            ["ID"] = Ref("GaussianDrillingProperty", "Typical inside diameter in metres (m); this is a dimension, not a UUID.")
        }),

        ["ElementConnectivity"] = Object("Directed connectivity between two full side-element definitions.", new JsonObject
        {
            ["UpstreamElement"] = NullableRef("SideElement", "Upstream side-circuit element."),
            ["DownstreamElement"] = NullableRef("SideElement", "Downstream side-circuit element.")
        }),

        ["CasingSection"] = Object("Casing interval beginning at the wellhead. Depth properties are referenced to the wellhead and all lists describe the interval's construction.", new JsonObject
        {
            ["TopDepth"] = Ref("GaussianDrillingProperty", "Top depth in metres (m), explicitly referenced to the wellhead."),
            ["Length"] = Ref("GaussianDrillingProperty", "Casing-section length in metres (m)."),
            ["TopCementDepth"] = Ref("GaussianDrillingProperty", "Top-of-cement depth in metres (m), explicitly referenced to the wellhead."),
            ["CasingSectionElements"] = Array("CasingSectionElement", "Ordered casing-element specifications used through this interval."),
            ["CasingSectionSizeTable"] = Array("BoreHoleSize", "Borehole diameter/length rows applicable to this casing section."),
            ["OpenHoleSection"] = NullableRef("OpenHoleSection", "Optional open-hole interval following this casing section; it begins where the previous casing interval ends, or at ground level for the first section.")
        }),

        ["CasingSectionElement"] = Object("Casing tubular specification and interval length, with uncertainty wrappers for physical properties.", new JsonObject
        {
            ["BodyOD"] = Gaussian("Casing body outside diameter in metres (m)."),
            ["BodyID"] = Gaussian("Casing body inside diameter in metres (m)."),
            ["CollarOD"] = Gaussian("Casing collar outside diameter in metres (m)."),
            ["JointLength"] = Gaussian("Mean casing-joint length in metres (m)."),
            ["SectionLength"] = NullableGaussian("Length over which this element specification applies, in metres (m)."),
            ["MaxDLS"] = NullableScalar("Maximum dogleg severity in radians per metre (rad/m)."),
            ["ConnectionType"] = NullableString("Joint connection-thread description."),
            ["Grade"] = NullableString("Casing material grade."),
            ["MaterialDensity"] = NullableGaussian("Material density in kilograms per cubic metre (kg/m³)."),
            ["YoungModulus"] = NullableGaussian("Young's modulus in pascals (Pa)."),
            ["LinearWeight"] = NullableGaussian("Linear mass density including the collar in kilograms per metre (kg/m)."),
            ["TensileStrength"] = NullableGaussian("Tensile strength in pascals (Pa)."),
            ["TorsionalStrength"] = NullableGaussian("Torsional strength in newton metres (N·m)."),
            ["BurstPressure"] = NullableGaussian("Burst pressure in pascals (Pa)."),
            ["CollapsePressure"] = NullableGaussian("Collapse pressure in pascals (Pa)."),
            ["YieldStress"] = NullableGaussian("Yield stress in pascals (Pa)."),
            ["MakeUpTorqueRecommended"] = NullableScalar("Recommended make-up torque in newton metres (N·m).")
        }),

        ["OpenHoleSection"] = Object("Open-hole interval represented by ordered borehole-size rows.", new JsonObject
        {
            ["HoleSizes"] = Array("BoreHoleSize", "Ordered hole-diameter and length rows for the open-hole interval.")
        }),

        ["BoreHoleSize"] = Object("Borehole diameter valid over a stated interval length.", new JsonObject
        {
            ["HoleSize"] = Gaussian("Borehole diameter in metres (m)."),
            ["Length"] = Gaussian("Length over which the borehole diameter applies, in metres (m).")
        }),

        ["GaussianDrillingProperty"] = Object("Uncertain physical value represented by a Gaussian distribution. Mean and StandardDeviation use the unit stated on the containing field.", new JsonObject
        {
            ["GaussianValue"] = Ref("GaussianDistribution", "Gaussian distribution carrying the SI mean and uncertainty.")
        }, "GaussianValue"),

        ["GaussianDistribution"] = Object("Gaussian distribution parameters in the containing field's SI unit.", new JsonObject
        {
            ["MinValue"] = Number("Optional lower bound in the same SI unit."),
            ["MaxValue"] = Number("Optional upper bound in the same SI unit."),
            ["Mean"] = NullableNumber("Mean value in the containing field's SI unit."),
            ["StandardDeviation"] = NullableNumber("Standard deviation in the same SI unit as Mean; it must not be a variance or a display-unit value.")
        }),

        ["ScalarDrillingProperty"] = Object("Deterministic physical value wrapped as a Dirac distribution. Put the SI value under DiracDistributionValue.Value, not in a ScalarValue shortcut.", new JsonObject
        {
            ["DiracDistributionValue"] = Ref("DiracDistribution", "Dirac distribution carrying the deterministic SI value.")
        }, "DiracDistributionValue"),

        ["DiracDistribution"] = Object("Deterministic distribution parameters in the containing field's SI unit.", new JsonObject
        {
            ["MinValue"] = Number("Optional lower bound in the same SI unit."),
            ["MaxValue"] = Number("Optional upper bound in the same SI unit."),
            ["Value"] = NullableNumber("Deterministic value in the containing field's SI unit.")
        })
    };

    public static JsonObject CreateCatalogSchema(string bodyName, bool feature, bool includeId = false, bool includeExpected = false)
    {
        JsonObject body = feature ? FeatureCategoryDefinition() : IdentityDefinition();
        JsonObject properties = new() { [bodyName] = body };
        JsonArray required = new(bodyName);
        if (includeId) { properties["id"] = String("Catalog definition UUID.", "uuid"); required.Add("id"); }
        if (includeExpected) { properties["expectedModifiedUtc"] = String("Exact LastModificationDate returned by the latest read.", "date-time"); required.Add("expectedModifiedUtc"); }
        return new() { ["type"] = "object", ["properties"] = properties, ["required"] = required, ["additionalProperties"] = false };
    }

    public static JsonObject CreateCatalogDeleteSchema() => new()
    {
        ["type"] = "object",
        ["properties"] = new JsonObject { ["id"] = String("Catalog definition UUID.", "uuid"), ["expectedModifiedUtc"] = String("Exact LastModificationDate returned by the latest read.", "date-time") },
        ["required"] = new JsonArray("id", "expectedModifiedUtc"), ["additionalProperties"] = false
    };

    public static JsonObject CreateWellBoreArchitectureBatchExportSchema() => new()
    {
        ["type"] = "object",
        ["properties"] = new JsonObject
        {
            ["request"] = new JsonObject
            {
                ["type"] = "object",
                ["properties"] = new JsonObject
                {
                    ["Scope"] = Enum("Choose All for every stored architecture or Selected for the supplied ordered UUID list.", "All", "Selected"),
                    ["WellBoreArchitectureIDs"] = new JsonObject
                    {
                        ["type"] = new JsonArray("array", "null"), ["uniqueItems"] = true,
                        ["items"] = String("Architecture resource UUID.", "uuid")
                    }
                },
                ["required"] = new JsonArray("Scope"), ["additionalProperties"] = false
            }
        },
        ["required"] = new JsonArray("request"), ["additionalProperties"] = false
    };

    public static JsonObject CreateWellBoreArchitectureBatchRestoreSchema()
    {
        JsonObject document = new()
        {
            ["type"] = "object",
            ["properties"] = new JsonObject
            {
                ["FormatIdentifier"] = new JsonObject { ["type"] = "string", ["const"] = "OSDC.Drilling.WellBoreArchitecture.BatchExport" },
                ["SchemaVersion"] = new JsonObject { ["type"] = "integer", ["const"] = 1 },
                ["ExportedAtUtc"] = String("UTC timestamp at which the snapshot was created.", "date-time"),
                ["CatalogDependencies"] = Object("Dependency-closed identity and feature catalogue subset.", new JsonObject
                {
                    ["Identities"] = new JsonObject { ["type"] = "array", ["items"] = IdentityDefinition() },
                    ["FeatureCategories"] = new JsonObject { ["type"] = "array", ["items"] = FeatureCategoryDefinition() }
                }, "Identities", "FeatureCategories"),
                ["WellBoreArchitectures"] = new JsonObject
                {
                    ["type"] = "array", ["minItems"] = 1,
                    ["items"] = new JsonObject { ["$ref"] = "#/$defs/WellBoreArchitecture" }
                }
            },
            ["required"] = new JsonArray("FormatIdentifier", "SchemaVersion", "ExportedAtUtc", "CatalogDependencies", "WellBoreArchitectures"),
            ["additionalProperties"] = false
        };
        return new JsonObject
        {
            ["type"] = "object",
            ["properties"] = new JsonObject
            {
                ["request"] = Object("Atomic restore request.", new JsonObject
                {
                    ["ConflictPolicy"] = Enum("Fail safely on existing UUIDs or explicitly replace them.", "FailIfExists", "ReplaceExisting"),
                    ["CatalogPolicy"] = Enum("Map existing compatible definitions or create missing definitions and options.", "MapExisting", "MapOrCreateMissing"),
                    ["Document"] = document
                }, "ConflictPolicy", "CatalogPolicy", "Document")
            },
            ["required"] = new JsonArray("request"), ["additionalProperties"] = false,
            ["$defs"] = Definitions()
        };
    }

    private static JsonObject IdentityDefinition() => Object("User-managed identity definition.", new JsonObject
    {
        ["MetaInfo"] = new JsonObject { ["type"] = "object", ["properties"] = new JsonObject { ["ID"] = String("Caller-generated definition UUID.", "uuid") }, ["required"] = new JsonArray("ID"), ["additionalProperties"] = true },
        ["Name"] = NullableString("Identity category name."), ["CreationDate"] = NullableDateTime("Server-owned creation time."), ["LastModificationDate"] = NullableDateTime("Server-owned concurrency token.")
    }, "MetaInfo");

    private static JsonObject FeatureCategoryDefinition() => Object("User-managed feature category and options.", new JsonObject
    {
        ["MetaInfo"] = new JsonObject { ["type"] = "object", ["properties"] = new JsonObject { ["ID"] = String("Caller-generated definition UUID.", "uuid") }, ["required"] = new JsonArray("ID"), ["additionalProperties"] = true },
        ["Name"] = NullableString("Feature category name."), ["IsExclusive"] = new JsonObject { ["type"] = "boolean" }, ["HasValidityPeriod"] = new JsonObject { ["type"] = "boolean" },
        ["Options"] = new JsonObject { ["type"] = "array", ["items"] = Object("Feature option.", new JsonObject { ["ID"] = String("Stable option UUID.", "uuid"), ["Name"] = NullableString("Option name.") }, "ID") },
        ["CreationDate"] = NullableDateTime("Server-owned creation time."), ["LastModificationDate"] = NullableDateTime("Server-owned concurrency token.")
    }, "MetaInfo", "IsExclusive", "HasValidityPeriod", "Options");

    private static JsonObject Object(string description, JsonObject properties, params string[] required)
    {
        var schema = new JsonObject
        {
            ["type"] = "object", ["description"] = description, ["properties"] = properties, ["additionalProperties"] = false
        };
        if (required.Length > 0)
        {
            var values = new JsonArray(); foreach (string value in required) values.Add(value); schema["required"] = values;
        }
        return schema;
    }

    private static JsonObject Ref(string name, string description) => new() { ["$ref"] = $"#/$defs/{name}", ["description"] = description };
    private static JsonObject NullableRef(string name, string description) => new()
    {
        ["description"] = description,
        ["anyOf"] = new JsonArray(new JsonObject { ["$ref"] = $"#/$defs/{name}" }, new JsonObject { ["type"] = "null" })
    };
    private static JsonObject Gaussian(string description) => Ref("GaussianDrillingProperty", description + " Supply Mean and, when known, StandardDeviation under GaussianValue.");
    private static JsonObject NullableGaussian(string description) => NullableRef("GaussianDrillingProperty", description + " Supply Mean and, when known, StandardDeviation under GaussianValue.");
    private static JsonObject NullableScalar(string description) => NullableRef("ScalarDrillingProperty", description + " Supply the value under DiracDistributionValue.Value.");
    private static JsonObject Array(string itemName, string description, bool nullable = false, int? minItems = null)
    {
        var schema = new JsonObject
        {
            ["type"] = nullable ? new JsonArray("array", "null") : "array",
            ["description"] = description,
            ["items"] = new JsonObject { ["$ref"] = $"#/$defs/{itemName}" }
        };
        if (minItems is not null) schema["minItems"] = minItems.Value;
        return schema;
    }
    private static JsonObject Enum(string description, params string[] values)
    {
        var items = new JsonArray(); foreach (string value in values) items.Add(value);
        return new JsonObject { ["type"] = "string", ["description"] = description, ["enum"] = items };
    }
    private static JsonObject String(string description, string? format = null)
    {
        var schema = new JsonObject { ["type"] = "string", ["description"] = description };
        if (format is not null) schema["format"] = format;
        return schema;
    }
    private static JsonObject NullableString(string description) => new() { ["type"] = new JsonArray("string", "null"), ["description"] = description };
    private static JsonObject NullableUuid(string description) => new() { ["type"] = new JsonArray("string", "null"), ["format"] = "uuid", ["description"] = description };
    private static JsonObject NullableDateTime(string description) => new() { ["type"] = new JsonArray("string", "null"), ["format"] = "date-time", ["description"] = description };
    private static JsonObject Number(string description) => new() { ["type"] = "number", ["description"] = description };
    private static JsonObject NullableNumber(string description) => new() { ["type"] = new JsonArray("number", "null"), ["description"] = description };

    public static bool TryParseGuid(JsonObject? arguments, string key, out Guid value, out JsonNode? error)
    {
        value = Guid.Empty;
        error = null;
        JsonNode? node = arguments?[key];
        if (node is null)
        {
            error = McpToolResponses.CreateValidationError($"Argument '{key}' is required.");
            return false;
        }
        if (!Guid.TryParse(node.ToString(), out value))
        {
            error = McpToolResponses.CreateValidationError($"Argument '{key}' must be a valid UUID.");
            return false;
        }
        return true;
    }

    public static bool TryParseDouble(JsonObject? arguments, string key, out double value, out JsonNode? error)
    {
        value = 0d;
        error = null;
        JsonNode? node = arguments?[key];
        if (node is null)
        {
            error = McpToolResponses.CreateValidationError($"Argument '{key}' is required.");
            return false;
        }
        try { value = node.GetValue<double>(); }
        catch (Exception ex) when (ex is InvalidOperationException or FormatException)
        {
            error = McpToolResponses.CreateValidationError($"Argument '{key}' must be a number.");
            return false;
        }
        if (double.IsNaN(value) || double.IsInfinity(value))
        {
            error = McpToolResponses.CreateValidationError($"Argument '{key}' must be a finite number.");
            return false;
        }
        return true;
    }
}
