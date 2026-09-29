using System.Reflection;
using OSDC.Drilling.WellBoreArchitecture.ModelShared;
using OSDC.Drilling.WellBoreArchitecture.WebPages.Pages;

namespace WebPagesTest;

public class ConnectorReferenceTests
{
    [TestCase("UpdateBopConnectorDepth", "ChokeLine", SurfaceSectionType.BOP)]
    [TestCase("UpdateBopConnectorDepth", "KillLine", SurfaceSectionType.BOP)]
    [TestCase("UpdateBoosterLineDepth", "BoosterLine", SurfaceSectionType.MarineRiser)]
    [TestCase("UpdateLiftPumpDepth", "LiftPump", SurfaceSectionType.BOP)]
    public void VerticalDepthEditPreservesHostRelativePosition(string method, string name, SurfaceSectionType type)
    {
        var connector = new SideConnector
        {
            Position = new GaussianDrillingProperty { GaussianValue = new GaussianDistribution { Mean = 3.25, StandardDeviation = 0.05 } },
            FirstSideElement = new SideElement { Name = name }
        };
        var architecture = new WellBoreArchitecture
        {
            SurfaceSections = [new SurfaceSection { Type = type, SideConnectors = [connector] }]
        };
        var editor = new WellBoreArchiteturePanel();
        // Set the component input without rendering: exercise the same mutation used by its event callbacks.
        typeof(WellBoreArchiteturePanel).GetProperty(nameof(editor.WellBoreArchitecture))!.SetValue(editor, architecture);
        var update = typeof(WellBoreArchiteturePanel).GetMethod(method, BindingFlags.Instance | BindingFlags.NonPublic)!;
        update.Invoke(editor, method == "UpdateBopConnectorDepth" ? new object?[] { name, 125.0 } : new object?[] { 125.0 });
        Assert.That(connector.VerticalDepth.GaussianValue.Mean, Is.EqualTo(125.0));
        Assert.That(connector.FirstSideElement.TopVerticalDepth.GaussianValue.Mean, Is.EqualTo(125.0));
        Assert.That(connector.Position.GaussianValue.Mean, Is.EqualTo(3.25));
        Assert.That(connector.Position.GaussianValue.StandardDeviation, Is.EqualTo(0.05));
        update.Invoke(editor, method == "UpdateBopConnectorDepth" ? new object?[] { name, null } : new object?[] { null });
        Assert.That(connector.Position.GaussianValue.Mean, Is.EqualTo(3.25));
    }
}
