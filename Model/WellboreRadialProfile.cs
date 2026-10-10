using System;
using System.Collections.Generic;
using OSDC.DotnetLibraries.Drilling.SemanticCatalogue;

namespace OSDC.Drilling.WellBoreArchitecture.Model;

public enum RadialBoundaryKind
{
    BoreholeWall,
    CementOuter,
    CementInner,
    CasingOuter,
    CasingInner
}

public enum RadialMaterialKind
{
    Formation,
    Cement,
    Casing,
    InternalFluidOrVoid,
    Unknown
}

[Semantic(Concepts.RadialBoundary)]
public sealed class RadialBoundary
{
    [EngineeringQuantity(Concepts.RadialBoundaryDiameter, Concepts.DimensionalLengthStandardUncertainty)]
    public double Diameter { get; set; }
    [EngineeringQuantity(Concepts.RadialBoundaryDiameter, Concepts.DimensionalLengthStandardUncertainty)]
    public double? DiameterStandardDeviation { get; set; }
    public RadialBoundaryKind Kind { get; set; }
    [Semantic(Concepts.RadialMaterialKind, Role = Concepts.MaterialOutsideBoundary)]
    public RadialMaterialKind MaterialOutside { get; set; }
    [Semantic(Concepts.RadialMaterialKind, Role = Concepts.MaterialInsideBoundary)]
    public RadialMaterialKind MaterialInside { get; set; }
    [EngineeringQuantity(Concepts.AlongHoleDepth, Role = Concepts.IntervalStartCoordinate, Reference = Concepts.Wgs84AlongHoleOrigin)]
    public double IntervalTop { get; set; }
    [EngineeringQuantity(Concepts.AlongHoleDepth, Role = Concepts.IntervalEndCoordinate, Reference = Concepts.Wgs84AlongHoleOrigin)]
    public double IntervalBottom { get; set; }
    [Semantic(Concepts.ResourceIdentifier)] public Guid SourceSectionComponentID { get; set; }
    [Semantic(Concepts.ResourceIdentifier)] public Guid SourceComponentID { get; set; }
    public string? SourceDescription { get; set; }
    [EngineeringQuantity(Concepts.PipeDiameter, Concepts.DimensionalLengthStandardUncertainty, Role = Concepts.OuterDiameter)]
    public double? CasingOuterDiameter { get; set; }
    [EngineeringQuantity(Concepts.PipeDiameter, Concepts.DimensionalLengthStandardUncertainty, Role = Concepts.InnerDiameter)]
    public double? CasingInnerDiameter { get; set; }
    [EngineeringQuantity(Concepts.PipeDiameter, Concepts.DimensionalLengthStandardUncertainty, Role = Concepts.CollarOuterDiameter)]
    public double? CasingCollarOuterDiameter { get; set; }
    [Semantic(Concepts.MaterialGrade)] public string? CasingGrade { get; set; }
    [EngineeringQuantity(Concepts.MaterialDensity, Concepts.MassDensityStandardUncertainty)] public double? CasingMaterialDensity { get; set; }
    [EngineeringQuantity(Concepts.LinearMassDensity, Concepts.LinearMassDensityStandardUncertainty)] public double? CasingLinearMassDensity { get; set; }
}

[Semantic(Concepts.WellboreRadialProfile)]
public sealed class WellboreRadialProfile
{
    [Semantic(Concepts.ResourceIdentifier)] public Guid WellBoreArchitectureID { get; set; }
    [EngineeringQuantity(Concepts.AlongHoleDepth, Reference = Concepts.Wgs84AlongHoleOrigin)] public double AlongHoleDepth { get; set; }
    public List<RadialBoundary> Boundaries { get; set; } = [];
    [EngineeringQuantity(Concepts.OutermostKnownPhysicalEnvelopeDiameter)] public double OutermostKnownPhysicalEnvelopeDiameter { get; set; }
    [EngineeringQuantity(Concepts.InnermostKnownPhysicalEnvelopeDiameter)] public double InnermostKnownPhysicalEnvelopeDiameter { get; set; }
}

[Semantic(Concepts.DeepestCasingShoeResult)]
public sealed class DeepestCasingShoeResult
{
    [Semantic(Concepts.ResourceIdentifier)] public Guid WellBoreArchitectureID { get; set; }
    [EngineeringQuantity(Concepts.CasingShoeAlongHoleDepth, Reference = Concepts.Wgs84AlongHoleOrigin)] public double AlongHoleDepth { get; set; }
    [Semantic(Concepts.ResourceIdentifier)] public Guid CasingSectionComponentID { get; set; }
    public WellboreRadialProfile RadialProfile { get; set; } = new();
}
