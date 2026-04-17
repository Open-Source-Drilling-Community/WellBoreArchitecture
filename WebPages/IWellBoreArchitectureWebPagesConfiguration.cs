using OSDC.DotnetLibraries.Drilling.WebAppUtils;

namespace NORCE.Drilling.WellBoreArchitecture.WebPages;

public interface IWellBoreArchitectureWebPagesConfiguration :
    IFieldHostURL,
    IClusterHostURL,
    IWellHostURL,
    IRigHostURL,
    IWellBoreHostURL,
    IWellBoreArchitectureHostURL,
    IUnitConversionHostURL
{
}
