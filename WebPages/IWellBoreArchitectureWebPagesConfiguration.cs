using OSDC.DotnetLibraries.Drilling.WebAppUtils;

namespace OSDC.Drilling.WellBoreArchitecture.WebPages;

public interface IWellBoreArchitectureWebPagesConfiguration :
    IFieldHostURL,
    IClusterHostURL,
    IWellHostURL,
    IRigHostURL,
    IWellBoreHostURL,
    IWellBoreArchitectureHostURL,
    IUnitConversionHostURL
{
    string VerticalDatumHostURL { get; set; }
}
