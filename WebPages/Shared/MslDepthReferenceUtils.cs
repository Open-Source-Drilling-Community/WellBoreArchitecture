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
            api.HttpClientVerticalDatum,
            api.HostNameVerticalDatum,
            api.HostBasePathVerticalDatum,
            slot?.Latitude?.GaussianValue?.Mean ?? cluster?.ReferenceLatitude?.GaussianValue?.Mean,
            slot?.Longitude?.GaussianValue?.Mean ?? cluster?.ReferenceLongitude?.GaussianValue?.Mean);
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

    private static async Task<double?> CalculateMeanSeaLevelDepthReferenceAsync(HttpClient client, string hostName, string hostBasePath, double? latitude, double? longitude)
    {
        if (latitude == null || longitude == null)
        {
            return null;
        }

        Guid orderId = Guid.NewGuid();
        object order = new
        {
            MetaInfo = new { ID = orderId, HttpHostName = hostName, HttpHostBasePath = hostBasePath, HttpEndPoint = "VerticalDatumOrder/" },
            Name = $"MSL reference {orderId}",
            Description = "Temporary MSL-to-WGS84 conversion.",
            CreationDate = DateTimeOffset.UtcNow,
            LastModificationDate = DateTimeOffset.UtcNow,
            VerticalDatum = new
            {
                MetaInfo = new { ID = Guid.NewGuid(), HttpHostName = hostName, HttpHostBasePath = hostBasePath, HttpEndPoint = "VerticalDatum/" },
                Name = $"MSL reference {orderId}",
                Description = "Temporary MSL-to-WGS84 conversion.",
                CreationDate = DateTimeOffset.UtcNow,
                LastModificationDate = DateTimeOffset.UtcNow,
                DatumSet = new[] { new { Latitude = latitude.Value, Longitude = longitude.Value, GenericVerticalDatum = 0 } },
                ConversionFrom = "FromMeanSeaLevel",
                Type = "Raw"
            }
        };

        try
        {
            using HttpResponseMessage postResponse = await client.PostAsJsonAsync("VerticalDatumOrder", order);
            postResponse.EnsureSuccessStatusCode();

            using JsonDocument document = await client.GetFromJsonAsync<JsonDocument>($"VerticalDatumOrder/{orderId}") ?? throw new InvalidOperationException("VerticalDatumOrder response was empty.");
            JsonElement datumSet = document.RootElement.GetProperty("VerticalDatum").GetProperty("DatumSet");
            if (datumSet.GetArrayLength() == 0 ||
                !datumSet[0].TryGetProperty("VerticalDatumWGS64", out JsonElement valueElement) ||
                valueElement.ValueKind == JsonValueKind.Null)
            {
                return null;
            }

            return -valueElement.GetDouble();
        }
        finally
        {
            try
            {
                await client.DeleteAsync($"VerticalDatumOrder/{orderId}");
            }
            catch
            {
                // Best-effort cleanup of a temporary calculation order.
            }
        }
    }
}
