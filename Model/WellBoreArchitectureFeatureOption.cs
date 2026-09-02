using OSDC.DotnetLibraries.General.DataManagement;
using System;

namespace OSDC.Drilling.WellBoreArchitecture.Model;

public class WellBoreArchitectureFeatureOption : IFeatureOption
{
    public Guid ID { get; set; }
    public string? Name { get; set; }
}
