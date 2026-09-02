using NORCE.Drilling.WellBoreArchitecture.WebPages;

namespace NORCE.Drilling.WellBoreArchitecture.WebApp;

public class WebPagesHostConfiguration :
    IWellBoreArchitectureWebPagesConfiguration,
    NORCE.Drilling.WellBore.WebPages.IWellBoreWebPagesConfiguration,
    NORCE.Drilling.Well.WebPages.IWellWebPagesConfiguration,
    NORCE.Drilling.Cluster.WebPages.IClusterWebPagesConfiguration,
    NORCE.Drilling.Field.WebPages.IFieldWebPagesConfiguration,
    NORCE.Drilling.CartographicProjection.WebPages.ICartographicProjectionWebPagesConfiguration,
    NORCE.Drilling.GeodeticDatum.WebPages.IGeodeticDatumWebPagesConfiguration
{
    public string FieldHostURL { get; set; } = string.Empty;
    public string ClusterHostURL { get; set; } = string.Empty;
    public string WellHostURL { get; set; } = string.Empty;
    public string RigHostURL { get; set; } = string.Empty;
    public string WellBoreHostURL { get; set; } = string.Empty;
    public string WellBoreArchitectureHostURL { get; set; } = string.Empty;
    public string TrajectoryHostURL { get; set; } = string.Empty;
    public string CartographicProjectionHostURL { get; set; } = string.Empty;
    public string GeodeticDatumHostURL { get; set; } = string.Empty;
    public string VerticalDatumHostURL { get; set; } = string.Empty;
    public string UnitConversionHostURL { get; set; } = string.Empty;
}
