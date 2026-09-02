using OSDC.DotnetLibraries.General.DataManagement;
using System;
using System.Collections.Generic;
using System.Linq;

namespace OSDC.Drilling.WellBoreArchitecture.Model;

public class WellBoreArchitectureFeatureCategory : IFeatureCategory
{
    public MetaInfo? MetaInfo { get; set; }
    public string? Name { get; set; }
    public bool IsExclusive { get; set; }
    public bool HasValidityPeriod { get; set; }
    public List<WellBoreArchitectureFeatureOption>? Options { get; set; }
    List<IFeatureOption>? IFeatureCategory.Options
    {
        get => Options?.Cast<IFeatureOption>().ToList();
        set => Options = value?.Select(option => option is WellBoreArchitectureFeatureOption architectureOption
            ? architectureOption
            : new WellBoreArchitectureFeatureOption { ID = option.ID, Name = option.Name }).ToList();
    }
    public DateTimeOffset? CreationDate { get; set; }
    public DateTimeOffset? LastModificationDate { get; set; }
}
