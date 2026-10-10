using OSDC.Drilling.WellBoreArchitecture.Model;
using OSDC.Drilling.WellBoreArchitecture.Service;
using OSDC.DotnetLibraries.General.DataManagement;
using OSDC.DotnetLibraries.General.DrillingProperties;
using OSDC.DotnetLibraries.General.Statistics;

namespace ServiceTest;

public sealed class WellboreRadialProfileEvaluatorTests
{
    [Test]
    public void ReturnsCoincidentConstructionMeaningsOutsideToInsideWithSymmetricExtrema()
    {
        Guid sectionId = Guid.NewGuid(), elementId = Guid.NewGuid();
        var architecture = new WellBoreArchitecture
        {
            MetaInfo = new MetaInfo { ID = Guid.NewGuid() },
            CasingSections =
            [
                new CasingSection
                {
                    ComponentID = sectionId, TopDepth = Gaussian(0), Length = Gaussian(1000), TopCementDepth = Gaussian(100),
                    CasingSectionSizeTable = [Size(1000, .31115)],
                    CasingSectionElements =
                    [
                        new CasingSectionElement
                        {
                            ComponentID = elementId, SectionLength = Gaussian(1000), BodyOD = Gaussian(.244475),
                            BodyID = Gaussian(.21679), CollarOD = Gaussian(.260), Grade = "P110",
                            LinearWeight = Gaussian(70), MaterialDensity = Gaussian(7850)
                        }
                    ]
                }
            ]
        };

        Assert.That(WellboreRadialProfileEvaluator.TryEvaluate(architecture, 500, out var profile, out _), Is.True);
        Assert.Multiple(() =>
        {
            Assert.That(profile!.Boundaries.Select(x => x.Diameter), Is.Ordered.Descending);
            Assert.That(profile.OutermostKnownPhysicalEnvelopeDiameter, Is.EqualTo(.31115));
            Assert.That(profile.InnermostKnownPhysicalEnvelopeDiameter, Is.EqualTo(.21679));
            Assert.That(profile.Boundaries.Count(x => x.Diameter == .31115), Is.EqualTo(2));
            Assert.That(profile.Boundaries.Count(x => x.Diameter == .244475), Is.EqualTo(2));
            Assert.That(profile.Boundaries.Count(x => x.Diameter == .260), Is.EqualTo(1));
            Assert.That(profile.Boundaries.Any(x => x.Kind == RadialBoundaryKind.CementOuter), Is.True);
            Assert.That(profile.Boundaries.Any(x => x.Kind == RadialBoundaryKind.CementInner), Is.True);
            Assert.That(profile.Boundaries.Single(x => x.Kind == RadialBoundaryKind.CasingInner).MaterialInside,
                Is.EqualTo(RadialMaterialKind.InternalFluidOrVoid));
            Assert.That(profile.Boundaries.First(x => x.SourceComponentID == elementId).CasingGrade, Is.EqualTo("P110"));
            Assert.That(profile.Boundaries.First(x => x.SourceComponentID == elementId).CasingOuterDiameter, Is.EqualTo(.244475));
            Assert.That(profile.Boundaries.First(x => x.SourceComponentID == elementId).CasingCollarOuterDiameter, Is.EqualTo(.260));
        });
    }

    [Test]
    public void ReturnsDeepestCasingShoeAndRadialProfileAtThatAbscissa()
    {
        var shallow = Section(0, 400, .30, .24, .21);
        var deep = Section(100, 900, .28, .20, .18);
        var architecture = new WellBoreArchitecture
        {
            MetaInfo = new MetaInfo { ID = Guid.NewGuid() }, CasingSections = [shallow, deep]
        };
        Assert.That(WellboreRadialProfileEvaluator.TryEvaluateDeepestCasingShoe(architecture, out var result, out _), Is.True);
        Assert.Multiple(() =>
        {
            Assert.That(result!.AlongHoleDepth, Is.EqualTo(1000));
            Assert.That(result.CasingSectionComponentID, Is.EqualTo(deep.ComponentID));
            Assert.That(result.RadialProfile.AlongHoleDepth, Is.EqualTo(1000));
            Assert.That(result.RadialProfile.Boundaries, Is.Not.Empty);
        });
    }

    [Test]
    public void RejectsUnknownDepthAndInvalidShoeRatherThanGuessing()
    {
        var architecture = new WellBoreArchitecture { MetaInfo = new MetaInfo { ID = Guid.NewGuid() } };
        Assert.That(WellboreRadialProfileEvaluator.TryEvaluate(architecture, 10, out _, out var missing), Is.False);
        Assert.That(missing, Does.Contain("No known"));
        Assert.That(WellboreRadialProfileEvaluator.TryEvaluate(architecture, double.NaN, out _, out var nonfinite), Is.False);
        Assert.That(nonfinite, Does.Contain("finite"));
        Assert.That(WellboreRadialProfileEvaluator.TryEvaluateDeepestCasingShoe(architecture, out _, out var shoe), Is.False);
        Assert.That(shoe, Does.Contain("No casing"));
    }

    private static CasingSection Section(double top, double length, double hole, double od, double id) => new()
    {
        ComponentID = Guid.NewGuid(), TopDepth = Gaussian(top), Length = Gaussian(length),
        CasingSectionSizeTable = [Size(length, hole)],
        CasingSectionElements = [new CasingSectionElement { ComponentID = Guid.NewGuid(), SectionLength = Gaussian(length), BodyOD = Gaussian(od), BodyID = Gaussian(id) }]
    };
    private static BoreHoleSize Size(double length, double diameter) => new()
    { ComponentID = Guid.NewGuid(), Length = Gaussian(length), HoleSize = Gaussian(diameter) };
    private static GaussianDrillingProperty Gaussian(double mean) => new()
    { GaussianValue = new GaussianDistribution { Mean = mean } };
}
