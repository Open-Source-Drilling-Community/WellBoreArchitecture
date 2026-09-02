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
            properties["expectedModifiedUtc"] = String("Exact LastModificationDate returned by the latest read. Stale writes are rejected without changing data.", "date-time");
            required.Add("id");
            required.Add("expectedModifiedUtc");
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

    public static JsonObject CreateWellBoreArchitectureDeleteSchema() => new()
    {
        ["type"] = "object",
        ["properties"] = new JsonObject
        {
            ["id"] = String("UUID from WellBoreArchitecture.MetaInfo.ID.", "uuid"),
            ["expectedModifiedUtc"] = String("Exact LastModificationDate returned by the latest read.", "date-time")
        },
        ["required"] = new JsonArray("id", "expectedModifiedUtc"),
        ["additionalProperties"] = false
    };

    public static JsonObject CreateDetailsMutationSchema() => CreateSubresourceMutationSchema("details", Object(
        "Only the human-readable architecture details to change.", new JsonObject
        {
            ["Name"] = NullableString("New architecture name."),
            ["Description"] = NullableString("New architecture description.")
        }, "Name", "Description"));

    public static JsonObject CreateWellBoreLinkMutationSchema() => CreateSubresourceMutationSchema("link", Object(
        "The externally-owned WellBore relationship to change.", new JsonObject
        {
            ["WellBoreID"] = NullableUuid("External WellBore UUID, or null to remove the relationship. The WellBore service is not synchronously validated.")
        }, "WellBoreID"));

    public static JsonObject CreateIdentityAssignmentMutationSchema(bool includeAssignmentId, bool includeBody) =>
        CreateAssignmentMutationSchema("WellBoreArchitectureIdentityAssignment", includeAssignmentId, includeBody);

    public static JsonObject CreateFeatureAssignmentMutationSchema(bool includeAssignmentId, bool includeBody) =>
        CreateAssignmentMutationSchema("WellBoreArchitectureFeatureAssignment", includeAssignmentId, includeBody);

    public static JsonObject CreateSearchSchema() => new()
    {
        ["type"] = "object",
        ["properties"] = new JsonObject
        {
            ["offset"] = new JsonObject { ["type"] = "integer", ["minimum"] = 0, ["default"] = 0 },
            ["limit"] = new JsonObject { ["type"] = "integer", ["minimum"] = 1, ["maximum"] = 200, ["default"] = 50 },
            ["name"] = NullableString("Case-insensitive substring matched against Name."),
            ["wellBoreId"] = NullableUuid("Exact externally-owned WellBore UUID."),
            ["identityId"] = NullableUuid("Identity definition UUID assigned to the architecture."),
            ["identityValue"] = NullableString("Case-insensitive substring matched against assigned identity values."),
            ["featureCategoryId"] = NullableUuid("Feature category UUID assigned to the architecture."),
            ["featureOptionId"] = NullableUuid("Feature option UUID assigned to the architecture."),
            ["modifiedFromUtc"] = NullableDateTime("Inclusive lower LastModificationDate bound."),
            ["modifiedToUtc"] = NullableDateTime("Inclusive upper LastModificationDate bound."),
            ["isLinked"] = new JsonObject { ["type"] = new JsonArray("boolean", "null"), ["description"] = "True returns architectures linked to a WellBore; false returns explicit drafts whose WellBoreID is null." }
        },
        ["additionalProperties"] = false
    };

    public static JsonObject CreateSectionMutationSchema(string definitionName, bool includeSectionId, bool includeBody, bool includeInsertAt = false)
    {
        var properties = new JsonObject
        {
            ["wellBoreArchitectureId"] = String("UUID from WellBoreArchitecture.MetaInfo.ID.", "uuid"),
            ["expectedModifiedUtc"] = String("Opaque concurrency token: echo LastModificationDate exactly as returned by the latest read; do not parse or reformat it.", "date-time")
        };
        var required = new JsonArray("wellBoreArchitectureId", "expectedModifiedUtc");
        if (includeSectionId) { properties["componentId"] = String("Stable nested ComponentID.", "uuid"); required.Add("componentId"); }
        if (includeBody) { properties["section"] = Ref(definitionName, "Complete section payload with a non-empty ComponentID."); required.Add("section"); }
        if (includeInsertAt) properties["insertAt"] = new JsonObject { ["type"] = new JsonArray("integer", "null"), ["minimum"] = 0, ["description"] = "Optional zero-based insertion position; omit to append." };
        return new JsonObject { ["type"] = "object", ["properties"] = properties, ["required"] = required,
            ["additionalProperties"] = false, ["$defs"] = Definitions() };
    }

    public static JsonObject CreateSectionReorderSchema() => new()
    {
        ["type"] = "object",
        ["properties"] = new JsonObject
        {
            ["wellBoreArchitectureId"] = String("UUID from WellBoreArchitecture.MetaInfo.ID.", "uuid"),
            ["expectedModifiedUtc"] = String("Opaque concurrency token: echo LastModificationDate exactly as returned by the latest read; do not parse or reformat it.", "date-time"),
            ["orderedComponentIds"] = new JsonObject { ["type"] = "array", ["minItems"] = 1, ["uniqueItems"] = true,
                ["items"] = String("Existing section ComponentID in desired top-to-bottom order.", "uuid") }
        },
        ["required"] = new JsonArray("wellBoreArchitectureId", "expectedModifiedUtc", "orderedComponentIds"),
        ["additionalProperties"] = false
    };

    public static JsonObject CreateIdsOutputSchema() => SuccessEnvelope(new JsonObject
    {
        ["type"] = "array", ["items"] = String("Resource UUID.", "uuid")
    });

    public static JsonObject CreateMetaInfoListOutputSchema() => SuccessEnvelope(new JsonObject
    {
        ["type"] = "array", ["items"] = new JsonObject { ["$ref"] = "#/$defs/MetaInfo" }
    }, Definitions());

    public static JsonObject CreateArchitectureOutputSchema() => SuccessEnvelope(
        new JsonObject { ["$ref"] = "#/$defs/WellBoreArchitecture" }, Definitions());

    public static JsonObject CreateArchitectureListOutputSchema() => SuccessEnvelope(new JsonObject
    {
        ["type"] = "array", ["items"] = new JsonObject { ["$ref"] = "#/$defs/WellBoreArchitecture" }
    }, Definitions());

    public static JsonObject CreateArchitectureLightListOutputSchema() => SuccessEnvelope(new JsonObject
    {
        ["type"] = "array",
        ["items"] = Object("Lightweight architecture discovery record.", new JsonObject
        {
            ["MetaInfo"] = new JsonObject { ["$ref"] = "#/$defs/MetaInfo" },
            ["Name"] = NullableString("Architecture name."),
            ["Description"] = NullableString("Architecture description."),
            ["CreationDate"] = NullableDateTime("Server-owned creation timestamp."),
            ["LastModificationDate"] = NullableDateTime("Latest optimistic-concurrency token.")
        }, "MetaInfo")
    }, Definitions());

    public static JsonObject CreateIdentityOutputSchema() => SuccessEnvelope(IdentityDefinition());
    public static JsonObject CreateIdentityListOutputSchema() => SuccessEnvelope(new JsonObject
    {
        ["type"] = "array", ["items"] = IdentityDefinition()
    });
    public static JsonObject CreateFeatureCategoryOutputSchema() => SuccessEnvelope(FeatureCategoryDefinition());
    public static JsonObject CreateFeatureCategoryListOutputSchema() => SuccessEnvelope(new JsonObject
    {
        ["type"] = "array", ["items"] = FeatureCategoryDefinition()
    });

    public static JsonObject CreateSearchOutputSchema() => SuccessEnvelope(Object("Deterministic search page.", new JsonObject
    {
        ["Offset"] = new JsonObject { ["type"] = "integer", ["minimum"] = 0 },
        ["Limit"] = new JsonObject { ["type"] = "integer", ["minimum"] = 1, ["maximum"] = 200 },
        ["TotalCount"] = new JsonObject { ["type"] = "integer", ["minimum"] = 0 },
        ["Items"] = new JsonObject { ["type"] = "array", ["items"] = new JsonObject { ["$ref"] = "#/$defs/WellBoreArchitecture" } }
    }, "Offset", "Limit", "TotalCount", "Items"), Definitions());

    public static JsonObject CreateExternalReferenceValidationOutputSchema() =>
        SuccessEnvelope(ExternalReferenceValidationSchema());

    public static JsonObject CreateExternalReferenceAuditSchema() => new()
    {
        ["type"] = "object",
        ["properties"] = new JsonObject
        {
            ["request"] = Object("Bounded external-reference audit request.", new JsonObject
            {
                ["Scope"] = Enum("Audit every architecture or an explicit UUID selection.", "All", "Selected"),
                ["WellBoreArchitectureIDs"] = new JsonObject { ["type"] = new JsonArray("array", "null"), ["uniqueItems"] = true,
                    ["items"] = String("Architecture UUID.", "uuid") },
                ["Offset"] = new JsonObject { ["type"] = "integer", ["minimum"] = 0, ["default"] = 0 },
                ["Limit"] = new JsonObject { ["type"] = "integer", ["minimum"] = 1, ["maximum"] = 100, ["default"] = 100 }
            }, "Scope")
        },
        ["required"] = new JsonArray("request"), ["additionalProperties"] = false
    };

    public static JsonObject CreateExternalReferenceAuditOutputSchema() => SuccessEnvelope(Object("Bounded external-reference audit result.", new JsonObject
    {
        ["CheckedAtUtc"] = String("UTC timestamp shared by this audit page.", "date-time"),
        ["Total"] = NonNegativeInteger(), ["Offset"] = NonNegativeInteger(),
        ["Limit"] = new JsonObject { ["type"] = "integer", ["minimum"] = 1, ["maximum"] = 100 },
        ["ValidCount"] = NonNegativeInteger(), ["InvalidCount"] = NonNegativeInteger(), ["UnavailableCount"] = NonNegativeInteger(),
        ["Items"] = new JsonObject { ["type"] = "array", ["items"] = ExternalReferenceValidationSchema() }
    }, "CheckedAtUtc", "Total", "Offset", "Limit", "ValidCount", "InvalidCount", "UnavailableCount", "Items"));

    private static JsonObject ExternalReferenceValidationSchema() => Object("One architecture's WellBore-reference validation result.", new JsonObject
    {
        ["WellBoreArchitectureID"] = String("Architecture UUID.", "uuid"),
        ["WellBoreID"] = NullableUuid("Stored external WellBore UUID, or null for an intentionally unlinked draft."),
        ["WellBoreExists"] = new JsonObject { ["type"] = new JsonArray("boolean", "null") },
        ["Status"] = Enum("Validation outcome.", "Valid", "Invalid", "Unavailable"),
        ["CheckedAtUtc"] = String("UTC validation timestamp.", "date-time"),
        ["Issues"] = new JsonObject { ["type"] = "array", ["items"] = Object("Reference-validation issue.", new JsonObject
        {
            ["Property"] = String("Property containing the reference."), ["Code"] = String("Stable machine-readable issue code."),
            ["Message"] = String("Human-readable diagnostic message.")
        }, "Property", "Code", "Message") }
    }, "WellBoreArchitectureID", "WellBoreID", "WellBoreExists", "Status", "CheckedAtUtc", "Issues");

    public static JsonObject CreateGenericOutputSchema() => SuccessEnvelope(new JsonObject());

    private static JsonObject SuccessEnvelope(JsonObject data, JsonObject? definitions = null)
    {
        var result = Object("Successful MCP tool response envelope.", new JsonObject
        {
            ["status"] = new JsonObject { ["type"] = "integer", ["minimum"] = 200, ["maximum"] = 299 },
            ["data"] = data
        }, "status");
        if (definitions != null) result["$defs"] = definitions;
        return result;
    }

    private static JsonObject CreateSubresourceMutationSchema(string bodyName, JsonObject body) => new()
    {
        ["type"] = "object",
        ["properties"] = new JsonObject
        {
            ["wellBoreArchitectureId"] = String("UUID from WellBoreArchitecture.MetaInfo.ID.", "uuid"),
            ["expectedModifiedUtc"] = String("Exact LastModificationDate returned by the latest read.", "date-time"),
            [bodyName] = body
        },
        ["required"] = new JsonArray("wellBoreArchitectureId", "expectedModifiedUtc", bodyName),
        ["additionalProperties"] = false
    };

    private static JsonObject CreateAssignmentMutationSchema(string definitionName, bool includeAssignmentId, bool includeBody)
    {
        var properties = new JsonObject
        {
            ["wellBoreArchitectureId"] = String("UUID from WellBoreArchitecture.MetaInfo.ID.", "uuid"),
            ["expectedModifiedUtc"] = String("Exact LastModificationDate returned by the latest read.", "date-time")
        };
        var required = new JsonArray("wellBoreArchitectureId", "expectedModifiedUtc");
        if (includeAssignmentId)
        {
            properties["assignmentId"] = String("Caller-generated assignment UUID.", "uuid");
            required.Add("assignmentId");
        }
        if (includeBody)
        {
            properties["assignment"] = Ref(definitionName, "Complete assignment payload.");
            required.Add("assignment");
        }
        return new JsonObject
        {
            ["type"] = "object", ["properties"] = properties, ["required"] = required,
            ["additionalProperties"] = false, ["$defs"] = Definitions()
        };
    }

    private static JsonObject Definitions() => new()
    {
        ["WellBoreArchitecture"] = Object(
            "Complete wellbore construction architecture. Physical values are SI and every persisted depth is referenced to the WGS84 datum. Alternative depth references are UI-only display transformations and are never persisted in this payload.",
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
            ["Depth"] = NullableRef("GaussianDrillingProperty", "Wellhead depth in metres (m), referenced to the WGS84 datum. Mean and standard deviation are under GaussianValue."),
            ["CasingHangerDepth"] = NullableRef("ScalarDrillingProperty", "Casing-hanger depth in metres (m), referenced to the WGS84 datum."),
            ["TubingHangerDepth"] = NullableRef("ScalarDrillingProperty", "Tubing-hanger depth in metres (m), referenced to the WGS84 datum.")
        }),

        ["WellBoreArchitectureFluid"] = Object("A fluid layer above ground or mudline and the depth of its boundary.", new JsonObject
        {
            ["Fluid"] = Enum("Fluid type for the layer.", "Air", "Water"),
            ["Depth"] = Ref("GaussianDrillingProperty", "Boundary depth in metres (m), referenced to the WGS84 datum. Mean and standard deviation are SI values under GaussianValue.")
        }),

        ["SurfaceSection"] = Object("Surface well-control or riser section above the wellhead, with uncertain dimensions/material properties and optional side circuitry.", new JsonObject
        {
            ["ComponentID"] = String("Stable UUID used by granular nested-component mutations.", "uuid"),
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
            ["ComponentID"] = String("Stable UUID used to address this nested connector.", "uuid"),
            ["Position"] = Ref("GaussianDrillingProperty", "Position along the host section in metres (m)."),
            ["VerticalDepth"] = Ref("GaussianDrillingProperty", "Vertical depth in metres (m), referenced to the WGS84 datum."),
            ["FirstSideElement"] = NullableRef("SideElement", "Root element of the side-circuit network."),
            ["ElementConnectivities"] = Array("ElementConnectivity", "Connectivity edges between side-circuit elements.", nullable: true)
        }),

        ["SideElement"] = Object("Pipe, hose, valve, choke, or pump in an auxiliary side circuit.", new JsonObject
        {
            ["ComponentID"] = String("Stable UUID used to address this side-circuit element; ID below remains the physical inside diameter.", "uuid"),
            ["Name"] = NullableString("Human-readable side-element name."),
            ["Type"] = Enum("Side-element classification.", "Unknown", "Pipe", "Hose", "GateValve", "Choke", "Pump"),
            ["Length"] = Ref("GaussianDrillingProperty", "Element length in metres (m)."),
            ["TopVerticalDepth"] = Ref("GaussianDrillingProperty", "Vertical depth of the element top in metres (m), referenced to the WGS84 datum."),
            ["OD"] = Ref("GaussianDrillingProperty", "Typical outside diameter in metres (m)."),
            ["ID"] = Ref("GaussianDrillingProperty", "Typical inside diameter in metres (m); this is a dimension, not a UUID.")
        }),

        ["ElementConnectivity"] = Object("Directed connectivity between two full side-element definitions.", new JsonObject
        {
            ["ComponentID"] = String("Stable UUID used to address this connectivity edge.", "uuid"),
            ["UpstreamElement"] = NullableRef("SideElement", "Upstream side-circuit element."),
            ["DownstreamElement"] = NullableRef("SideElement", "Downstream side-circuit element.")
        }),

        ["CasingSection"] = Object("Casing interval beginning at the wellhead. Depth properties are stored in metres relative to the WGS84 datum; all lists describe the interval's construction.", new JsonObject
        {
            ["ComponentID"] = String("Stable UUID used by granular nested-component mutations.", "uuid"),
            ["TopDepth"] = Ref("GaussianDrillingProperty", "Top depth in metres (m), referenced to the WGS84 datum."),
            ["Length"] = Ref("GaussianDrillingProperty", "Casing-section length in metres (m)."),
            ["TopCementDepth"] = Ref("GaussianDrillingProperty", "Top-of-cement depth in metres (m), referenced to the WGS84 datum."),
            ["CasingSectionElements"] = Array("CasingSectionElement", "Ordered casing-element specifications used through this interval."),
            ["CasingSectionSizeTable"] = Array("BoreHoleSize", "Borehole diameter/length rows applicable to this casing section."),
            ["OpenHoleSection"] = NullableRef("OpenHoleSection", "Optional open-hole interval following this casing section; it begins where the previous casing interval ends, or at ground level for the first section.")
        }),

        ["CasingSectionElement"] = Object("Casing tubular specification and interval length, with uncertainty wrappers for physical properties.", new JsonObject
        {
            ["ComponentID"] = String("Stable UUID used to address this nested casing element.", "uuid"),
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
            ["ComponentID"] = String("Stable UUID used to address this nested open-hole interval.", "uuid"),
            ["HoleSizes"] = Array("BoreHoleSize", "Ordered hole-diameter and length rows for the open-hole interval.")
        }),

        ["BoreHoleSize"] = Object("Borehole diameter valid over a stated interval length.", new JsonObject
        {
            ["ComponentID"] = String("Stable UUID used to address this nested size row.", "uuid"),
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
                    ["CatalogPolicy"] = Enum("Use exact UUID matches or create missing definitions/options while preserving their source UUIDs.", "MapExisting", "MapOrCreateMissing"),
                    ["AllowNormalizedNameMapping"] = new JsonObject { ["type"] = "boolean", ["default"] = false,
                        ["description"] = "Explicit opt-in for mapping a source catalogue item to a different local UUID by one compatible normalized name. Leave false unless a human has confirmed semantic identity." },
                    ["Document"] = document
                }, "ConflictPolicy", "CatalogPolicy", "AllowNormalizedNameMapping", "Document")
            },
            ["required"] = new JsonArray("request"), ["additionalProperties"] = false,
            ["$defs"] = Definitions()
        };
    }

    private static JsonObject IdentityDefinition() => Object("User-managed identity definition.", new JsonObject
    {
        ["MetaInfo"] = Object("Identity-definition metadata.", new JsonObject
        {
            ["ID"] = String("Caller-generated definition UUID.", "uuid"),
            ["HttpHostName"] = NullableString("Optional source host metadata."),
            ["HttpHostBasePath"] = NullableString("Optional source base-path metadata."),
            ["HttpEndPoint"] = NullableString("Optional source endpoint metadata.")
        }, "ID"),
        ["Name"] = NullableString("Identity category name."), ["CreationDate"] = NullableDateTime("Server-owned creation time."), ["LastModificationDate"] = NullableDateTime("Server-owned concurrency token.")
    }, "MetaInfo");

    private static JsonObject FeatureCategoryDefinition() => Object("User-managed feature category and options.", new JsonObject
    {
        ["MetaInfo"] = Object("Feature-category metadata.", new JsonObject
        {
            ["ID"] = String("Caller-generated definition UUID.", "uuid"),
            ["HttpHostName"] = NullableString("Optional source host metadata."),
            ["HttpHostBasePath"] = NullableString("Optional source base-path metadata."),
            ["HttpEndPoint"] = NullableString("Optional source endpoint metadata.")
        }, "ID"),
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
    private static JsonObject NonNegativeInteger() => new() { ["type"] = "integer", ["minimum"] = 0 };

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
