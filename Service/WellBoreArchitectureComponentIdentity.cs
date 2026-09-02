using OSDC.Drilling.WellBoreArchitecture.Model;
using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using ArchitectureModel = OSDC.Drilling.WellBoreArchitecture.Model.WellBoreArchitecture;

namespace OSDC.Drilling.WellBoreArchitecture.Service;

/// <summary>
/// Materializes stable IDs for nested components. Legacy JSON omitted these IDs; deterministic
/// path-derived values make those records immediately addressable without a database migration.
/// The next normal write persists the materialized IDs.
/// </summary>
public static class WellBoreArchitectureComponentIdentity
{
    public static bool Ensure(ArchitectureModel? architecture)
    {
        if (architecture?.MetaInfo == null || architecture.MetaInfo.ID == Guid.Empty) return false;
        HashSet<Guid> used = [];

        bool Assign(ref Guid value, string path)
        {
            if (value != Guid.Empty) return used.Add(value);
            int salt = 0;
            do value = Derive(architecture.MetaInfo.ID, salt++ == 0 ? path : $"{path}#{salt}");
            while (!used.Add(value));
            return true;
        }

        bool SideElementTree(SideElement? element, string path)
        {
            if (element == null) return true;
            Guid id = element.ComponentID;
            if (!Assign(ref id, path)) return false;
            element.ComponentID = id;
            return true;
        }

        architecture.SurfaceSections ??= [];
        for (int surfaceIndex = 0; surfaceIndex < architecture.SurfaceSections.Count; surfaceIndex++)
        {
            SurfaceSection surface = architecture.SurfaceSections[surfaceIndex];
            Guid surfaceId = surface.ComponentID;
            if (!Assign(ref surfaceId, $"surface/{surfaceIndex}")) return false;
            surface.ComponentID = surfaceId;
            surface.SideConnectors ??= [];
            for (int connectorIndex = 0; connectorIndex < surface.SideConnectors.Count; connectorIndex++)
            {
                SideConnector connector = surface.SideConnectors[connectorIndex];
                string connectorPath = $"surface/{surfaceIndex}/connector/{connectorIndex}";
                Guid connectorId = connector.ComponentID;
                if (!Assign(ref connectorId, connectorPath)) return false;
                connector.ComponentID = connectorId;
                if (!SideElementTree(connector.FirstSideElement, connectorPath + "/first")) return false;
                connector.ElementConnectivities ??= [];
                for (int connectivityIndex = 0; connectivityIndex < connector.ElementConnectivities.Count; connectivityIndex++)
                {
                    ElementConnectivity connectivity = connector.ElementConnectivities[connectivityIndex];
                    string connectivityPath = $"{connectorPath}/connectivity/{connectivityIndex}";
                    Guid connectivityId = connectivity.ComponentID;
                    if (!Assign(ref connectivityId, connectivityPath)) return false;
                    connectivity.ComponentID = connectivityId;
                    if (!SideElementTree(connectivity.UpstreamElement, connectivityPath + "/upstream") ||
                        !SideElementTree(connectivity.DownstreamElement, connectivityPath + "/downstream")) return false;
                }
            }
        }

        architecture.CasingSections ??= [];
        for (int casingIndex = 0; casingIndex < architecture.CasingSections.Count; casingIndex++)
        {
            CasingSection casing = architecture.CasingSections[casingIndex];
            string casingPath = $"casing/{casingIndex}";
            Guid casingId = casing.ComponentID;
            if (!Assign(ref casingId, casingPath)) return false;
            casing.ComponentID = casingId;
            casing.CasingSectionElements ??= [];
            for (int elementIndex = 0; elementIndex < casing.CasingSectionElements.Count; elementIndex++)
            {
                CasingSectionElement element = casing.CasingSectionElements[elementIndex];
                Guid elementId = element.ComponentID;
                if (!Assign(ref elementId, $"{casingPath}/element/{elementIndex}")) return false;
                element.ComponentID = elementId;
            }
            if (casing.OpenHoleSection != null)
            {
                Guid openHoleId = casing.OpenHoleSection.ComponentID;
                if (!Assign(ref openHoleId, casingPath + "/open-hole")) return false;
                casing.OpenHoleSection.ComponentID = openHoleId;
                casing.OpenHoleSection.HoleSizes ??= [];
                for (int sizeIndex = 0; sizeIndex < casing.OpenHoleSection.HoleSizes.Count; sizeIndex++)
                {
                    BoreHoleSize size = casing.OpenHoleSection.HoleSizes[sizeIndex];
                    Guid sizeId = size.ComponentID;
                    if (!Assign(ref sizeId, $"{casingPath}/open-hole/size/{sizeIndex}")) return false;
                    size.ComponentID = sizeId;
                }
            }
        }
        return true;
    }

    private static Guid Derive(Guid architectureId, string path)
    {
        byte[] hash = SHA256.HashData(Encoding.UTF8.GetBytes($"{architectureId:D}/{path}"));
        Span<byte> value = hash.AsSpan(0, 16);
        value[6] = (byte)((value[6] & 0x0F) | 0x50); // Mark as a deterministic name-derived UUID.
        value[8] = (byte)((value[8] & 0x3F) | 0x80);
        return new Guid(value);
    }
}
