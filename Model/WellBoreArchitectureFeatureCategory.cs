using OSDC.DotnetLibraries.Drilling.SemanticCatalogue;
using OSDC.DotnetLibraries.General.ResourceClassification;

namespace OSDC.Drilling.WellBoreArchitecture.Model;

/// <summary>Architecture classification contract backed by the shared resource classification implementation.</summary>
[Semantic(Concepts.FeatureCategory)]
public class WellBoreArchitectureFeatureCategory : FeatureCategory<WellBoreArchitectureFeatureOption>
{
}
