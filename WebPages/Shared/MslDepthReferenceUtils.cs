using System.Net.Http.Json;
using System.Text.Json;
using ModelShared = OSDC.Drilling.WellBoreArchitecture.ModelShared;

namespace OSDC.Drilling.WellBoreArchitecture.WebPages.Shared;

public static class MslDepthReferenceUtils
{
    public static Task<double?> ResolveMeanSeaLevelDepthReferenceAsync(
        IWellBoreArchitectureAPIUtils api,
        ModelShared.WellBore? wellBore,
        ModelShared.Well? well,
        ModelShared.Cluster? cluster,
        IReadOnlyDictionary<Guid, ModelShared.WellBore>? wellBores,
        IReadOnlyDictionary<Guid, ModelShared.Well>? wells,
        IReadOnlyDictionary<Guid, ModelShared.Cluster>? clusters)
    {
        ModelShared.WellBore? rootWellBore = ResolveRootWellBore(wellBore, wellBores);
        ModelShared.Well? rootWell = ResolveWell(rootWellBore, well, wells);
        ModelShared.Slot? slot = ResolveSlot(rootWell, cluster, clusters);

        return CalculateMeanSeaLevelDepthReferenceAsync(
            api.HttpClientEarthVerticalDatum,
            slot?.Latitude?.GaussianValue?.Mean ?? cluster?.ReferencePoint?.Latitude,
            slot?.Longitude?.GaussianValue?.Mean ?? cluster?.ReferencePoint?.Longitude);
    }

    private static ModelShared.WellBore? ResolveRootWellBore(ModelShared.WellBore? wellBore, IReadOnlyDictionary<Guid, ModelShared.WellBore>? wellBores)
    {
        ModelShared.WellBore? current = wellBore;
        HashSet<Guid> visitedIds = new();
        while (current?.IsSidetrack == true &&
            current.ParentWellBoreID is Guid parentId &&
            parentId != Guid.Empty &&
            visitedIds.Add(parentId) &&
            wellBores?.TryGetValue(parentId, out ModelShared.WellBore? parentWellBore) == true)
        {
            current = parentWellBore;
        }

        return current;
    }

    private static ModelShared.Well? ResolveWell(ModelShared.WellBore? rootWellBore, ModelShared.Well? selectedWell, IReadOnlyDictionary<Guid, ModelShared.Well>? wells)
    {
        if (rootWellBore?.WellID is Guid wellId && wells?.TryGetValue(wellId, out ModelShared.Well? well) == true)
        {
            return well;
        }

        return selectedWell;
    }

    private static ModelShared.Slot? ResolveSlot(ModelShared.Well? well, ModelShared.Cluster? selectedCluster, IReadOnlyDictionary<Guid, ModelShared.Cluster>? clusters)
    {
        if (well?.SlotID is not Guid slotId)
        {
            return null;
        }

        ModelShared.Cluster? cluster = null;
        if (well.ClusterID is Guid clusterId)
        {
            clusters?.TryGetValue(clusterId, out cluster);
        }

        cluster ??= selectedCluster;
        cluster ??= clusters?.Values.FirstOrDefault(item => item?.Slots?.Values.Any(slot => slot?.ID == slotId) == true);
        return cluster?.Slots?.Values.FirstOrDefault(slot => slot?.ID == slotId);
    }

    private static async Task<double?> CalculateMeanSeaLevelDepthReferenceAsync(HttpClient client, double? latitude, double? longitude)
    {
        if (latitude == null || longitude == null)
        {
            return null;
        }

        object request = new
        {
            Positions = new[]
            {
                new
                {
                    Latitude = latitude.Value,
                    Longitude = longitude.Value,
                    MeanSeaLevelDepth = 0.0
                }
            }
        };

        using HttpResponseMessage response = await client.PostAsJsonAsync(
            "EarthVerticalDatum/ConvertMeanSeaLevelToWgs84",
            request);
        response.EnsureSuccessStatusCode();

        using JsonDocument document = await JsonDocument.ParseAsync(
            await response.Content.ReadAsStreamAsync());
        if (!document.RootElement.TryGetProperty("Samples", out JsonElement samples) ||
            samples.ValueKind != JsonValueKind.Array ||
            samples.GetArrayLength() == 0 ||
            !samples[0].TryGetProperty("Wgs84EllipsoidalDepth", out JsonElement valueElement) ||
            valueElement.ValueKind != JsonValueKind.Number)
        {
            return null;
        }

        return valueElement.GetDouble();
    }
}
