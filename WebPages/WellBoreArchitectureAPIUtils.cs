using NORCE.Drilling.WellBoreArchitecture.ModelShared;
using NORCE.Drilling.WellBoreArchitecture.WebPages.Shared;
using OSDC.DotnetLibraries.Drilling.WebAppUtils;

namespace NORCE.Drilling.WellBoreArchitecture.WebPages;

public class WellBoreArchitectureAPIUtils : APIUtils, IWellBoreArchitectureAPIUtils
{
    public WellBoreArchitectureAPIUtils(IWellBoreArchitectureWebPagesConfiguration configuration)
    {
        HostNameWellBoreArchitecture = Require(configuration.WellBoreArchitectureHostURL, nameof(configuration.WellBoreArchitectureHostURL));
        HttpClientWellBoreArchitecture = SetHttpClient(HostNameWellBoreArchitecture, HostBasePathWellBoreArchitecture);
        ClientWellBoreArchitecture = new Client(HttpClientWellBoreArchitecture.BaseAddress!.ToString(), HttpClientWellBoreArchitecture);

        HostNameField = Require(configuration.FieldHostURL, nameof(configuration.FieldHostURL));
        HttpClientField = SetHttpClient(HostNameField, HostBasePathField);
        ClientField = new Client(HttpClientField.BaseAddress!.ToString(), HttpClientField);

        HostNameCluster = Require(configuration.ClusterHostURL, nameof(configuration.ClusterHostURL));
        HttpClientCluster = SetHttpClient(HostNameCluster, HostBasePathCluster);
        ClientCluster = new Client(HttpClientCluster.BaseAddress!.ToString(), HttpClientCluster);

        HostNameWell = Require(configuration.WellHostURL, nameof(configuration.WellHostURL));
        HttpClientWell = SetHttpClient(HostNameWell, HostBasePathWell);
        ClientWell = new Client(HttpClientWell.BaseAddress!.ToString(), HttpClientWell);

        HostNameRig = Require(configuration.RigHostURL, nameof(configuration.RigHostURL));
        HttpClientRig = SetHttpClient(HostNameRig, HostBasePathRig);
        ClientRig = new Client(HttpClientRig.BaseAddress!.ToString(), HttpClientRig);

        HostNameWellBore = Require(configuration.WellBoreHostURL, nameof(configuration.WellBoreHostURL));
        HttpClientWellBore = SetHttpClient(HostNameWellBore, HostBasePathWellBore);
        ClientWellBore = new Client(HttpClientWellBore.BaseAddress!.ToString(), HttpClientWellBore);

        HostNameVerticalDatum = Require(configuration.VerticalDatumHostURL, nameof(configuration.VerticalDatumHostURL));
        HttpClientVerticalDatum = SetHttpClient(HostNameVerticalDatum, HostBasePathVerticalDatum);

        HostNameUnitConversion = Require(configuration.UnitConversionHostURL, nameof(configuration.UnitConversionHostURL));
    }

    private static string Require(string? value, string propertyName)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new InvalidOperationException($"Configuration value '{propertyName}' must be assigned before WebPages is used.");
        }

        return value;
    }

    public string HostNameWellBoreArchitecture { get; }
    public string HostBasePathWellBoreArchitecture { get; } = "WellBoreArchitecture/api/";
    public HttpClient HttpClientWellBoreArchitecture { get; }
    public Client ClientWellBoreArchitecture { get; }

    public string HostNameField { get; }
    public string HostBasePathField { get; } = "Field/api/";
    public HttpClient HttpClientField { get; }
    public Client ClientField { get; }

    public string HostNameCluster { get; }
    public string HostBasePathCluster { get; } = "Cluster/api/";
    public HttpClient HttpClientCluster { get; }
    public Client ClientCluster { get; }

    public string HostNameWell { get; }
    public string HostBasePathWell { get; } = "Well/api/";
    public HttpClient HttpClientWell { get; }
    public Client ClientWell { get; }

    public string HostNameRig { get; }
    public string HostBasePathRig { get; } = "Rig/api/";
    public HttpClient HttpClientRig { get; }
    public Client ClientRig { get; }

    public string HostNameWellBore { get; }
    public string HostBasePathWellBore { get; } = "WellBore/api/";
    public HttpClient HttpClientWellBore { get; }
    public Client ClientWellBore { get; }

    public string HostNameVerticalDatum { get; }
    public string HostBasePathVerticalDatum { get; } = "VerticalDatum/api/";
    public HttpClient HttpClientVerticalDatum { get; }

    public string HostNameUnitConversion { get; }
    public string HostBasePathUnitConversion { get; } = "UnitConversion/api/";

    public WellHead DefaultWellHead()
    {
        return new WellHead
        {
            MaxOD = ConversionsFromOSDC.DoubleToScalar(null),
            MinOD = ConversionsFromOSDC.DoubleToScalar(null),
            Depth = ConversionsFromOSDC.DoubleToGaussian(null),
            CasingHangerDepth = ConversionsFromOSDC.DoubleToScalar(null),
            TubingHangerDepth = ConversionsFromOSDC.DoubleToScalar(null)
        };
    }

    public List<WellBoreArchitectureFluid> DefaultFluidsAboveGroundLevel()
    {
        return new List<WellBoreArchitectureFluid>
        {
            new()
            {
                Fluid = FluidType.Air,
                Depth = ConversionsFromOSDC.DoubleToGaussian(null)
            }
        };
    }

    public List<SurfaceSection> DefaultSurfaceSections()
    {
        return new List<SurfaceSection>
        {
            new()
            {
                Type = SurfaceSectionType.Unknown,
                SideConnectors = new List<SideConnector>
                {
                    new()
                    {
                        Position = ConversionsFromOSDC.DoubleToGaussian(null),
                        VerticalDepth = ConversionsFromOSDC.DoubleToGaussian(null),
                    }
                }
            }
        };
    }

    public List<CasingSection> DefaultCasingSections()
    {
        return new List<CasingSection>
        {
            new()
            {
                CasingSectionElements = new List<CasingSectionElement>
                {
                    new()
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
                    new()
                    {
                        HoleSize = ConversionsFromOSDC.DoubleToGaussian(null),
                        Length = ConversionsFromOSDC.DoubleToGaussian(null)
                    }
                },
                OpenHoleSection = new OpenHoleSection
                {
                    HoleSizes = new List<BoreHoleSize>
                    {
                        new()
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
