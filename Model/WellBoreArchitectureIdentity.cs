using OSDC.DotnetLibraries.General.DataManagement;
using System;

namespace OSDC.Drilling.WellBoreArchitecture.Model;

public class WellBoreArchitectureIdentity : IIdentity
{
    public MetaInfo? MetaInfo { get; set; }
    public string? Name { get; set; }
    public DateTimeOffset? CreationDate { get; set; }
    public DateTimeOffset? LastModificationDate { get; set; }
}
