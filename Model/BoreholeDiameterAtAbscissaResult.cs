using System;
using System.Collections.Generic;
using OSDC.DotnetLibraries.Drilling.SemanticCatalogue;

namespace OSDC.Drilling.WellBoreArchitecture.Model;

[Semantic(Concepts.BoreholeDiameterAtAbscissaResult)]
public sealed class BoreholeDiameterAtAbscissaResult
{
    [Semantic(Concepts.ResourceIdentifier)]
    public Guid WellBoreArchitectureID { get; set; }

    [EngineeringQuantity(Concepts.AlongHoleDepth, Reference = Concepts.Wgs84AlongHoleOrigin)]
    public double AlongHoleDepth { get; set; }

    [EngineeringQuantity(Concepts.BoreholeDiameter)]
    public double BoreholeDiameter { get; set; }

    [EngineeringQuantity(Concepts.DimensionalLengthStandardUncertainty)]
    public double? BoreholeDiameterStandardDeviation { get; set; }

    [EngineeringQuantity(Concepts.AlongHoleDepth, Role = Concepts.IntervalStartCoordinate, Reference = Concepts.Wgs84AlongHoleOrigin)]
    public double IntervalTop { get; set; }

    [EngineeringQuantity(Concepts.AlongHoleDepth, Role = Concepts.IntervalEndCoordinate, Reference = Concepts.Wgs84AlongHoleOrigin)]
    public double IntervalBottom { get; set; }

    public string SourceKind { get; set; } = "";

    [Semantic(Concepts.ResourceIdentifier)]
    public Guid SourceSectionComponentID { get; set; }

    [Semantic(Concepts.ResourceIdentifier)]
    public Guid BoreholeSizeComponentID { get; set; }

    public List<BoreholeDiameterContributor> Contributors { get; set; } = [];
}

public sealed class BoreholeDiameterContributor
{
    [EngineeringQuantity(Concepts.BoreholeDiameter, Role = Concepts.ResultContribution)]
    public double BoreholeDiameter { get; set; }

    [EngineeringQuantity(Concepts.DimensionalLengthStandardUncertainty)]
    public double? BoreholeDiameterStandardDeviation { get; set; }

    [EngineeringQuantity(Concepts.AlongHoleDepth, Role = Concepts.IntervalStartCoordinate, Reference = Concepts.Wgs84AlongHoleOrigin)]
    public double IntervalTop { get; set; }

    [EngineeringQuantity(Concepts.AlongHoleDepth, Role = Concepts.IntervalEndCoordinate, Reference = Concepts.Wgs84AlongHoleOrigin)]
    public double IntervalBottom { get; set; }

    public string SourceKind { get; set; } = "";

    [Semantic(Concepts.ResourceIdentifier)]
    public Guid SourceSectionComponentID { get; set; }

    [Semantic(Concepts.ResourceIdentifier)]
    public Guid BoreholeSizeComponentID { get; set; }
}
