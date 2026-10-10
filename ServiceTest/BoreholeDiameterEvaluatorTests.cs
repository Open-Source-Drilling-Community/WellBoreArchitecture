using OSDC.Drilling.WellBoreArchitecture.Model;
using OSDC.Drilling.WellBoreArchitecture.Service;
using OSDC.DotnetLibraries.General.DataManagement;
using OSDC.DotnetLibraries.General.DrillingProperties;
using OSDC.DotnetLibraries.General.Statistics;

namespace ServiceTest;

public sealed class BoreholeDiameterEvaluatorTests
{
    [Test]
    public void SelectsTheOutermostApplicableDiameterAndRetainsContributors()
    {
        Guid architectureId = Guid.NewGuid();
        var architecture = new WellBoreArchitecture
        {
            MetaInfo = new MetaInfo { ID = architectureId },
            CasingSections =
            [
                Section(0, 200, (100, .30), (100, .22)),
                Section(50, 100, (100, .40))
            ],
            OpenHoleSection = new OpenHoleSection
            {
                ComponentID = Guid.NewGuid(), HoleSizes = [Size(75, .18), Size(75, .15)]
            }
        };

        Assert.That(BoreholeDiameterEvaluator.TryEvaluate(architecture, 75, out var casing, out _), Is.True);
        Assert.Multiple(() =>
        {
            Assert.That(casing!.WellBoreArchitectureID, Is.EqualTo(architectureId));
            Assert.That(casing.BoreholeDiameter, Is.EqualTo(.40));
            Assert.That(casing.SourceKind, Is.EqualTo("casing"));
            Assert.That(casing.Contributors, Has.Count.EqualTo(2));
        });

        Assert.That(BoreholeDiameterEvaluator.TryEvaluate(architecture, 210, out var open, out _), Is.True);
        Assert.That(open!.BoreholeDiameter, Is.EqualTo(.18));
        Assert.That(open.SourceKind, Is.EqualTo("open-hole"));
    }

    [Test]
    public void RejectsMissingAndInvalidIntervalsRatherThanGuessing()
    {
        var architecture = new WellBoreArchitecture
        {
            MetaInfo = new MetaInfo { ID = Guid.NewGuid() },
            CasingSections = [Section(0, 10, (10, .2))]
        };
        Assert.That(BoreholeDiameterEvaluator.TryEvaluate(architecture, 10, out _, out var atBottom), Is.False);
        Assert.That(atBottom, Does.Contain("No valid"));
        Assert.That(BoreholeDiameterEvaluator.TryEvaluate(architecture, double.NaN, out _, out var nonfinite), Is.False);
        Assert.That(nonfinite, Does.Contain("finite"));
    }

    private static CasingSection Section(double top, double length, params (double Length, double Diameter)[] sizes) => new()
    {
        ComponentID = Guid.NewGuid(), TopDepth = Gaussian(top), Length = Gaussian(length),
        CasingSectionSizeTable = sizes.Select(value => Size(value.Length, value.Diameter)).ToList()
    };

    private static BoreHoleSize Size(double length, double diameter) => new()
    {
        ComponentID = Guid.NewGuid(), Length = Gaussian(length), HoleSize = Gaussian(diameter)
    };

    private static GaussianDrillingProperty Gaussian(double mean) => new()
    {
        GaussianValue = new GaussianDistribution { Mean = mean }
    };
}
