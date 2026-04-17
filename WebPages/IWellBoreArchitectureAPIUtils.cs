using NORCE.Drilling.WellBoreArchitecture.ModelShared;

namespace NORCE.Drilling.WellBoreArchitecture.WebPages;

public interface IWellBoreArchitectureAPIUtils
{
    string HostNameWellBoreArchitecture { get; }
    string HostBasePathWellBoreArchitecture { get; }
    HttpClient HttpClientWellBoreArchitecture { get; }
    Client ClientWellBoreArchitecture { get; }

    string HostNameField { get; }
    string HostBasePathField { get; }
    HttpClient HttpClientField { get; }
    Client ClientField { get; }

    string HostNameCluster { get; }
    string HostBasePathCluster { get; }
    HttpClient HttpClientCluster { get; }
    Client ClientCluster { get; }

    string HostNameWell { get; }
    string HostBasePathWell { get; }
    HttpClient HttpClientWell { get; }
    Client ClientWell { get; }

    string HostNameRig { get; }
    string HostBasePathRig { get; }
    HttpClient HttpClientRig { get; }
    Client ClientRig { get; }

    string HostNameWellBore { get; }
    string HostBasePathWellBore { get; }
    HttpClient HttpClientWellBore { get; }
    Client ClientWellBore { get; }

    string HostNameUnitConversion { get; }
    string HostBasePathUnitConversion { get; }

    WellHead DefaultWellHead();
    List<WellBoreArchitectureFluid> DefaultFluidsAboveGroundLevel();
    List<SurfaceSection> DefaultSurfaceSections();
    List<CasingSection> DefaultCasingSections();
}
