using OSDC.UnitConversion.DrillingRazorMudComponents;
using NORCE.Drilling.WellBoreArchitecture.ModelShared;

public static class APIUtils
{
    // API parameters
    public static readonly string HostNameWellBoreArchitecture = NORCE.Drilling.WellBoreArchitecture.WebApp.Configuration.WellBoreArchitectureHostURL!;
    public static readonly string HostBasePathWellBoreArchitecture = "WellBoreArchitecture/api/";
    public static readonly HttpClient HttpClientWellBoreArchitecture = APIUtils.SetHttpClient(HostNameWellBoreArchitecture, HostBasePathWellBoreArchitecture);
    public static readonly NORCE.Drilling.WellBoreArchitecture.ModelShared.Client ClientWellBoreArchitecture = new NORCE.Drilling.WellBoreArchitecture.ModelShared.Client(APIUtils.HttpClientWellBoreArchitecture.BaseAddress!.ToString(), APIUtils.HttpClientWellBoreArchitecture);
    // Field api
    public static readonly string HostDevDigiWells = "https://dev.digiwells.no/";
    public static readonly string HostBasePathField = "Field/api/";
    public static readonly HttpClient HttpClientField = APIUtils.SetHttpClient(HostDevDigiWells, HostBasePathField);
    public static readonly NORCE.Drilling.WellBoreArchitecture.ModelShared.Client ClientField = new NORCE.Drilling.WellBoreArchitecture.ModelShared.Client(APIUtils.HttpClientField.BaseAddress!.ToString(), APIUtils.HttpClientField);
    // Cluster api
    public static readonly string HostBasePathCluster = "Cluster/api/";
    public static readonly HttpClient HttpClientCluster = APIUtils.SetHttpClient(HostDevDigiWells, HostBasePathCluster);
    public static readonly NORCE.Drilling.WellBoreArchitecture.ModelShared.Client ClientCluster = new NORCE.Drilling.WellBoreArchitecture.ModelShared.Client(APIUtils.HttpClientCluster.BaseAddress!.ToString(), APIUtils.HttpClientCluster);
    // Well api
    public static readonly string HostBasePathWell = "Well/api/";
    public static readonly HttpClient HttpClientWell = APIUtils.SetHttpClient(HostDevDigiWells, HostBasePathWell);
    public static readonly NORCE.Drilling.WellBoreArchitecture.ModelShared.Client ClientWell = new NORCE.Drilling.WellBoreArchitecture.ModelShared.Client(APIUtils.HttpClientWell.BaseAddress!.ToString(), APIUtils.HttpClientWell);
    // WellBore api
    public static readonly string HostBasePathWellBore = "WellBore/api/";
    public static readonly HttpClient HttpClientWellBore = APIUtils.SetHttpClient(HostDevDigiWells, HostBasePathWellBore);
    public static readonly NORCE.Drilling.WellBoreArchitecture.ModelShared.Client ClientWellBore = new NORCE.Drilling.WellBoreArchitecture.ModelShared.Client(APIUtils.HttpClientWellBore.BaseAddress!.ToString(), APIUtils.HttpClientWellBore);

    public static readonly string HostNameUnitConversion = NORCE.Drilling.WellBoreArchitecture.WebApp.Configuration.UnitConversionHostURL!;
    public static readonly string HostBasePathUnitConversion = "UnitConversion/api/";

    // API utility methods
    public static HttpClient SetHttpClient(string host, string microServiceUri)
    {
        var handler = new HttpClientHandler();
        handler.ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => { return true; }; // temporary workaround for testing purposes: bypass certificate validation (not recommended for production environments due to security risks)
        HttpClient httpClient = new(handler)
        {
            BaseAddress = new Uri(host + microServiceUri)
        };
        httpClient.DefaultRequestHeaders.Accept.Clear();
        httpClient.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));
        return httpClient;
    }

    public static WellHead DefaultWellHead()
    {
        return new WellHead()
        {
            MaxOD = ConversionsFromOSDC.DoubleToScalar(null),
            MinOD = ConversionsFromOSDC.DoubleToScalar(null),
            Depth = ConversionsFromOSDC.DoubleToGaussian(null),
            CasingHangerDepth = ConversionsFromOSDC.DoubleToScalar(null),
            TubingHangerDepth = ConversionsFromOSDC.DoubleToScalar(null)
        };
    }
    public static List<WellBoreArchitectureFluid> DefaultFluidsAboveGroundLevel()
    {
        return new List<WellBoreArchitectureFluid>()
        {
            new WellBoreArchitectureFluid()
            {
                Fluid = FluidType.Air,
                Depth = ConversionsFromOSDC.DoubleToGaussian(null)
            }
        };
    }
    public static List<SurfaceSection> DefaultSurfaceSections()
    {
        return new List<SurfaceSection>()
        {
            new SurfaceSection()
            {
                Type = SurfaceSectionType.Unknown,
                SideConnectors =  new List<SideConnector>()
                    {
                        new SideConnector()
                        {
                            Position = ConversionsFromOSDC.DoubleToGaussian(null),
                            VerticalDepth = ConversionsFromOSDC.DoubleToGaussian(null),
                        }
                    }
            }
        };
    }
    public static List<CasingSection> DefaultCasingSections()
    {
        return new List<CasingSection>()
        {
            new CasingSection()
            {
                CasingSectionElements = new List<CasingSectionElement>()
                    {
                        new CasingSectionElement()
                        {
                            BodyID = ConversionsFromOSDC.DoubleToGaussian(null),
                            BodyOD = ConversionsFromOSDC.DoubleToGaussian(null),
                            CollarOD = ConversionsFromOSDC.DoubleToGaussian(null),
                            JointLength = ConversionsFromOSDC.DoubleToGaussian(null)
                        }
                    },
                Length = ConversionsFromOSDC.DoubleToGaussian(null),
                TopCementDepth = ConversionsFromOSDC.DoubleToGaussian(null),
                TopDepth = ConversionsFromOSDC.DoubleToGaussian(null),
                CasingSectionSizeTable = new List<BoreHoleSize>
                {
                    new BoreHoleSize
                    {
                        HoleSize = ConversionsFromOSDC.DoubleToGaussian(null),
                        Length = ConversionsFromOSDC.DoubleToGaussian(null)
                    }
                },
                OpenHoleSection = new OpenHoleSection
                {
                    HoleSizes = new List<BoreHoleSize>
                    {
                        new BoreHoleSize
                        {
                            HoleSize = ConversionsFromOSDC.DoubleToGaussian(null),
                            Length = ConversionsFromOSDC.DoubleToGaussian(null)
                        }
                    }
                }
            }
        };
    }
}

public class GroundMudLineDepthReferenceSource : IGroundMudLineDepthReferenceSource
{
    public double? GroundMudLineDepthReference { get; set; } = null;
}

public class RotaryTableDepthReferenceSource : IRotaryTableDepthReferenceSource
{
    public double? RotaryTableDepthReference { get; set; } = null;
}

public class SeaWaterLevelDepthReferenceSource : ISeaWaterLevelDepthReferenceSource
{
    public double? SeaWaterLevelDepthReference { get; set; } = null;
}
public class WellHeadDepthReferenceSource : IWellHeadDepthReferenceSource
{
    public double? WellHeadDepthReference { get; set; } = null;
}

