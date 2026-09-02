using OSDC.DotnetLibraries.General.DataManagement;
using System;

namespace OSDC.Drilling.WellBoreArchitecture.Model;

public class WellBoreArchitectureIdentityAssignment : IIdentityAssignment
{
    public Guid ID { get; set; }
    public Guid? IdentityID { get; set; }
    public string? Value { get; set; }
}
