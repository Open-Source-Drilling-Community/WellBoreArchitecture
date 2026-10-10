using System;
using System.Collections.Generic;
using System.Linq;
using OSDC.Drilling.WellBoreArchitecture.Model;
using OSDC.DotnetLibraries.General.DrillingProperties;

namespace OSDC.Drilling.WellBoreArchitecture.Service;

internal static class WellboreRadialProfileEvaluator
{
    private const double Tolerance = 1e-9;

    internal static bool TryEvaluate(Model.WellBoreArchitecture architecture, double depth,
        out WellboreRadialProfile? result, out string? error, bool includeBottom = false)
    {
        result = null;
        error = null;
        if (!double.IsFinite(depth)) { error = "Along-hole depth must be finite."; return false; }
        var boundaries = new List<RadialBoundary>();
        double deepestShoe = double.NegativeInfinity;

        foreach (var section in architecture.CasingSections ?? [])
        {
            if (!TryMean(section.TopDepth, out var top) || !TryMean(section.Length, out var length) || length < 0) continue;
            double bottom = top + length;
            deepestShoe = Math.Max(deepestShoe, bottom);
            var holes = ApplicableHoleBoundaries(section.CasingSectionSizeTable, top, bottom, depth, includeBottom,
                section.ComponentID, "casing-section borehole").ToList();
            boundaries.AddRange(holes);

            double cursor = top;
            foreach (var element in section.CasingSectionElements ?? [])
            {
                if (!TryMean(element.SectionLength, out var elementLength) || elementLength <= 0) continue;
                double elementBottom = Math.Min(cursor + elementLength, bottom);
                if (Contains(cursor, elementBottom, depth, includeBottom))
                {
                    double? bodyOd = Mean(element.BodyOD), bodyId = Mean(element.BodyID), collarOd = Mean(element.CollarOD);
                    double? casingOd = bodyOd is > 0 ? bodyOd : collarOd;
                    bool cemented = TryMean(section.TopCementDepth, out var cementTop) && depth + Tolerance >= cementTop;
                    RadialBoundary? outerHole = holes.OrderByDescending(x => x.Diameter).FirstOrDefault();
                    if (cemented && outerHole is not null && casingOd is > 0)
                    {
                        boundaries.Add(CopyBoundary(outerHole, RadialBoundaryKind.CementOuter,
                            RadialMaterialKind.Formation, RadialMaterialKind.Cement, "cement outer boundary"));
                        boundaries.Add(CasingBoundary(casingOd.Value, element, section.ComponentID, cursor, elementBottom,
                            RadialBoundaryKind.CementInner, RadialMaterialKind.Cement, RadialMaterialKind.Casing,
                            "cement inner boundary"));
                    }
                    if (bodyOd is > 0)
                        boundaries.Add(CasingBoundary(bodyOd.Value, element, section.ComponentID, cursor, elementBottom,
                            RadialBoundaryKind.CasingOuter, cemented ? RadialMaterialKind.Cement : RadialMaterialKind.Unknown,
                            RadialMaterialKind.Casing, "casing body outer boundary"));
                    if (collarOd is > 0 && (bodyOd is not > 0 || Math.Abs(collarOd.Value - bodyOd.Value) > Tolerance))
                        boundaries.Add(CasingBoundary(collarOd.Value, element, section.ComponentID, cursor, elementBottom,
                            RadialBoundaryKind.CasingOuter, cemented ? RadialMaterialKind.Cement : RadialMaterialKind.Unknown,
                            RadialMaterialKind.Casing, "casing collar outer boundary"));
                    if (bodyId is > 0)
                        boundaries.Add(CasingBoundary(bodyId.Value, element, section.ComponentID, cursor, elementBottom,
                            RadialBoundaryKind.CasingInner, RadialMaterialKind.Casing,
                            RadialMaterialKind.InternalFluidOrVoid, "casing inner boundary"));
                }
                cursor += elementLength;
                if (cursor >= bottom - Tolerance) break;
            }
        }

        if (architecture.OpenHoleSection is { } openHole && double.IsFinite(deepestShoe))
            boundaries.AddRange(ApplicableHoleBoundaries(openHole.HoleSizes, deepestShoe, double.PositiveInfinity,
                depth, includeBottom, openHole.ComponentID, "open-hole borehole"));

        boundaries = boundaries.Where(x => double.IsFinite(x.Diameter) && x.Diameter > 0)
            .OrderByDescending(x => x.Diameter).ThenBy(x => x.Kind).ThenBy(x => x.SourceComponentID).ToList();
        if (boundaries.Count == 0) { error = "No known radial boundary contains the supplied along-hole depth."; return false; }
        result = new WellboreRadialProfile
        {
            WellBoreArchitectureID = architecture.MetaInfo.ID, AlongHoleDepth = depth, Boundaries = boundaries,
            OutermostKnownPhysicalEnvelopeDiameter = boundaries[0].Diameter,
            InnermostKnownPhysicalEnvelopeDiameter = boundaries[^1].Diameter
        };
        return true;
    }

    internal static bool TryEvaluateDeepestCasingShoe(Model.WellBoreArchitecture architecture,
        out DeepestCasingShoeResult? result, out string? error)
    {
        result = null;
        var shoes = (architecture.CasingSections ?? []).Select(section =>
            TryMean(section.TopDepth, out var top) && TryMean(section.Length, out var length) && length >= 0
                ? (Valid: true, Depth: top + length, Section: section) : (Valid: false, Depth: 0d, Section: section))
            .Where(x => x.Valid && double.IsFinite(x.Depth)).OrderByDescending(x => x.Depth).ToList();
        if (shoes.Count == 0) { error = "No casing section has valid finite top and length values."; return false; }
        var shoe = shoes[0];
        if (!TryEvaluate(architecture, shoe.Depth, out var profile, out error, includeBottom: true)) return false;
        result = new DeepestCasingShoeResult
        {
            WellBoreArchitectureID = architecture.MetaInfo.ID, AlongHoleDepth = shoe.Depth,
            CasingSectionComponentID = shoe.Section.ComponentID, RadialProfile = profile!
        };
        return true;
    }

    private static IEnumerable<RadialBoundary> ApplicableHoleBoundaries(IEnumerable<BoreHoleSize>? sizes,
        double start, double limit, double depth, bool includeBottom, Guid sectionId, string description)
    {
        double cursor = start;
        foreach (var size in sizes ?? [])
        {
            if (!TryMean(size.Length, out var length) || length <= 0 || !TryMean(size.HoleSize, out var diameter) || diameter <= 0) continue;
            double bottom = Math.Min(cursor + length, limit);
            if (Contains(cursor, bottom, depth, includeBottom))
                yield return new RadialBoundary
                {
                    Diameter = diameter, DiameterStandardDeviation = size.HoleSize.GaussianValue?.StandardDeviation,
                    Kind = RadialBoundaryKind.BoreholeWall, MaterialOutside = RadialMaterialKind.Formation,
                    MaterialInside = RadialMaterialKind.Unknown, IntervalTop = cursor, IntervalBottom = bottom,
                    SourceSectionComponentID = sectionId, SourceComponentID = size.ComponentID, SourceDescription = description
                };
            cursor += length;
            if (cursor >= limit - Tolerance) break;
        }
    }

    private static RadialBoundary CopyBoundary(RadialBoundary source, RadialBoundaryKind kind,
        RadialMaterialKind outside, RadialMaterialKind inside, string description) => new()
    {
        Diameter = source.Diameter, DiameterStandardDeviation = source.DiameterStandardDeviation, Kind = kind,
        MaterialOutside = outside, MaterialInside = inside, IntervalTop = source.IntervalTop,
        IntervalBottom = source.IntervalBottom, SourceSectionComponentID = source.SourceSectionComponentID,
        SourceComponentID = source.SourceComponentID, SourceDescription = description
    };

    private static RadialBoundary CasingBoundary(double diameter, CasingSectionElement element, Guid sectionId,
        double top, double bottom, RadialBoundaryKind kind, RadialMaterialKind outside,
        RadialMaterialKind inside, string description) => new()
    {
        Diameter = diameter, Kind = kind, MaterialOutside = outside, MaterialInside = inside,
        IntervalTop = top, IntervalBottom = bottom, SourceSectionComponentID = sectionId,
        SourceComponentID = element.ComponentID, SourceDescription = description,
        CasingOuterDiameter = Mean(element.BodyOD),
        CasingInnerDiameter = Mean(element.BodyID), CasingCollarOuterDiameter = Mean(element.CollarOD),
        CasingGrade = element.Grade, CasingMaterialDensity = Mean(element.MaterialDensity),
        CasingLinearMassDensity = Mean(element.LinearWeight)
    };

    private static bool Contains(double top, double bottom, double depth, bool includeBottom) =>
        depth + Tolerance >= top && (depth < bottom - Tolerance || includeBottom && depth <= bottom + Tolerance);
    private static double? Mean(GaussianDrillingProperty? value) =>
        value?.GaussianValue?.Mean is double mean && double.IsFinite(mean) ? mean : null;
    private static bool TryMean(GaussianDrillingProperty? value, out double mean)
    { mean = Mean(value) ?? double.NaN; return double.IsFinite(mean); }
}
