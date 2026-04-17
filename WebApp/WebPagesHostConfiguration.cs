using NORCE.Drilling.WellBoreArchitecture.WebPages;

namespace NORCE.Drilling.WellBoreArchitecture.WebApp;

public class WebPagesHostConfiguration : IWellBoreArchitectureWebPagesConfiguration
{
    public string FieldHostURL { get; set; } = string.Empty;
    public string ClusterHostURL { get; set; } = string.Empty;
    public string WellHostURL { get; set; } = string.Empty;
    public string RigHostURL { get; set; } = string.Empty;
    public string WellBoreHostURL { get; set; } = string.Empty;
    public string WellBoreArchitectureHostURL { get; set; } = string.Empty;
    public string UnitConversionHostURL { get; set; } = string.Empty;
}
