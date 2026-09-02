using Microsoft.Extensions.DependencyInjection;

namespace OSDC.Drilling.WellBoreArchitecture.WebApp;

public static class ExternalWebPagesServiceCollectionExtensions
{
    public static IServiceCollection AddExternalWebPages(this IServiceCollection services, WebPagesHostConfiguration configuration)
    {
        services.AddSingleton<NORCE.Drilling.WellBore.WebPages.IWellBoreWebPagesConfiguration>(configuration);
        services.AddSingleton<
            NORCE.Drilling.WellBore.WebPages.IWellBoreAPIUtils,
            NORCE.Drilling.WellBore.WebPages.WellBoreAPIUtils>();
        services.AddSingleton<NORCE.Drilling.Well.WebPages.IWellWebPagesConfiguration>(configuration);
        services.AddSingleton<
            NORCE.Drilling.Well.WebPages.IWellAPIUtils,
            NORCE.Drilling.Well.WebPages.WellAPIUtils>();
        services.AddSingleton<NORCE.Drilling.Cluster.WebPages.IClusterWebPagesConfiguration>(configuration);
        services.AddSingleton<
            NORCE.Drilling.Cluster.WebPages.IClusterAPIUtils,
            NORCE.Drilling.Cluster.WebPages.ClusterAPIUtils>();
        services.AddSingleton<NORCE.Drilling.Field.WebPages.IFieldWebPagesConfiguration>(configuration);
        services.AddSingleton<
            NORCE.Drilling.Field.WebPages.IFieldAPIUtils,
            NORCE.Drilling.Field.WebPages.FieldAPIUtils>();
        services.AddSingleton<NORCE.Drilling.CartographicProjection.WebPages.ICartographicProjectionWebPagesConfiguration>(configuration);
        services.AddSingleton<
            NORCE.Drilling.CartographicProjection.WebPages.ICartographicProjectionAPIUtils,
            NORCE.Drilling.CartographicProjection.WebPages.CartographicProjectionAPIUtils>();
        services.AddSingleton<NORCE.Drilling.GeodeticDatum.WebPages.IGeodeticDatumWebPagesConfiguration>(configuration);
        services.AddSingleton<
            NORCE.Drilling.GeodeticDatum.WebPages.IGeodeticDatumAPIUtils,
            NORCE.Drilling.GeodeticDatum.WebPages.GeodeticDatumAPIUtils>();
        return services;
    }
}
