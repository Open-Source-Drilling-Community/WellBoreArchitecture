using System;
using System.Collections.Generic;
using System.Linq;
using OSDC.Drilling.WellBoreArchitecture.Model;
using OSDC.DotnetLibraries.General.DrillingProperties;

namespace OSDC.Drilling.WellBoreArchitecture.Service;

internal static class BoreholeDiameterEvaluator
{
    private const double BoundaryTolerance = 1e-9;

    internal static bool TryEvaluate(Model.WellBoreArchitecture architecture, double alongHoleDepth,
        out BoreholeDiameterAtAbscissaResult? result, out string? error)
    {
        result = null;
        error = null;
        if (!double.IsFinite(alongHoleDepth))
        {
            error = "Along-hole depth must be finite.";
            return false;
        }

        var intervals = new List<BoreholeDiameterContributor>();
        double deepestCasingShoe = double.NegativeInfinity;
        foreach (var section in architecture.CasingSections ?? [])
        {
            if (!TryMean(section.TopDepth, out double sectionTop) || !TryMean(section.Length, out double sectionLength) || sectionLength < 0)
                continue;
            double sectionBottom = sectionTop + sectionLength;
            deepestCasingShoe = Math.Max(deepestCasingShoe, sectionBottom);
            double cursor = sectionTop;
            foreach (var size in section.CasingSectionSizeTable ?? [])
            {
                if (!TryMean(size.Length, out double length) || length <= 0 || !TryMean(size.HoleSize, out double diameter) || diameter <= 0)
                    continue;
                double bottom = Math.Min(cursor + length, sectionBottom);
                if (bottom > cursor && Contains(cursor, bottom, alongHoleDepth))
                    intervals.Add(Contributor(section.ComponentID, size, "casing", cursor, bottom, diameter));
                cursor += length;
                if (cursor >= sectionBottom - BoundaryTolerance) break;
            }
        }

        if (architecture.OpenHoleSection is { } openHole && double.IsFinite(deepestCasingShoe))
        {
            double cursor = deepestCasingShoe;
            foreach (var size in openHole.HoleSizes ?? [])
            {
                if (!TryMean(size.Length, out double length) || length <= 0 || !TryMean(size.HoleSize, out double diameter) || diameter <= 0)
                    continue;
                double bottom = cursor + length;
                if (Contains(cursor, bottom, alongHoleDepth))
                    intervals.Add(Contributor(openHole.ComponentID, size, "open-hole", cursor, bottom, diameter));
                cursor = bottom;
            }
        }

        if (intervals.Count == 0)
        {
            error = "No valid casing or open-hole interval contains the supplied along-hole depth.";
            return false;
        }

        var selected = intervals.OrderByDescending(value => value.BoreholeDiameter)
            .ThenBy(value => value.IntervalTop).ThenBy(value => value.BoreholeSizeComponentID).First();
        result = new BoreholeDiameterAtAbscissaResult
        {
            WellBoreArchitectureID = architecture.MetaInfo.ID,
            AlongHoleDepth = alongHoleDepth,
            BoreholeDiameter = selected.BoreholeDiameter,
            BoreholeDiameterStandardDeviation = selected.BoreholeDiameterStandardDeviation,
            IntervalTop = selected.IntervalTop,
            IntervalBottom = selected.IntervalBottom,
            SourceKind = selected.SourceKind,
            SourceSectionComponentID = selected.SourceSectionComponentID,
            BoreholeSizeComponentID = selected.BoreholeSizeComponentID,
            Contributors = intervals.OrderBy(value => value.IntervalTop).ThenByDescending(value => value.BoreholeDiameter).ToList()
        };
        return true;
    }

    private static bool Contains(double top, double bottom, double depth) => depth >= top && depth < bottom;

    private static BoreholeDiameterContributor Contributor(Guid sectionId, BoreHoleSize size, string kind,
        double top, double bottom, double diameter) => new()
    {
        BoreholeDiameter = diameter,
        BoreholeDiameterStandardDeviation = size.HoleSize.GaussianValue?.StandardDeviation,
        IntervalTop = top,
        IntervalBottom = bottom,
        SourceKind = kind,
        SourceSectionComponentID = sectionId,
        BoreholeSizeComponentID = size.ComponentID
    };

    private static bool TryMean(GaussianDrillingProperty? property, out double value)
    {
        value = property?.GaussianValue?.Mean ?? double.NaN;
        return double.IsFinite(value);
    }
}
