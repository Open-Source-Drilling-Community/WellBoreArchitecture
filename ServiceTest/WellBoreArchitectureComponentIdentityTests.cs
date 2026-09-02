using OSDC.DotnetLibraries.General.DataManagement;
using OSDC.Drilling.WellBoreArchitecture.Model;
using OSDC.Drilling.WellBoreArchitecture.Service;

namespace ServiceTest;

[TestFixture]
public sealed class WellBoreArchitectureComponentIdentityTests
{
    [Test]
    public void Legacy_components_receive_deterministic_unique_ids()
    {
        var architecture = new WellBoreArchitecture
        {
            MetaInfo = new MetaInfo { ID = Guid.NewGuid() },
            SurfaceSections = [new SurfaceSection { SideConnectors = [new SideConnector { FirstSideElement = new SideElement() }] }],
            CasingSections = [new CasingSection { CasingSectionElements = [new CasingSectionElement()], OpenHoleSection = new OpenHoleSection { HoleSizes = [new BoreHoleSize()] } }]
        };

        Assert.That(WellBoreArchitectureComponentIdentity.Ensure(architecture), Is.True);
        Guid[] first = IDs(architecture);
        Assert.That(WellBoreArchitectureComponentIdentity.Ensure(architecture), Is.True);
        Assert.Multiple(() =>
        {
            Assert.That(first, Has.None.EqualTo(Guid.Empty));
            Assert.That(first, Is.Unique);
            Assert.That(IDs(architecture), Is.EqualTo(first));
        });
    }

    [Test]
    public void Duplicate_caller_supplied_component_ids_are_rejected()
    {
        Guid duplicate = Guid.NewGuid();
        var architecture = new WellBoreArchitecture
        {
            MetaInfo = new MetaInfo { ID = Guid.NewGuid() },
            SurfaceSections = [new SurfaceSection { ComponentID = duplicate }],
            CasingSections = [new CasingSection { ComponentID = duplicate }]
        };
        Assert.That(WellBoreArchitectureComponentIdentity.Ensure(architecture), Is.False);
    }

    private static Guid[] IDs(WellBoreArchitecture value) =>
    [
        value.SurfaceSections[0].ComponentID,
        value.SurfaceSections[0].SideConnectors[0].ComponentID,
        value.SurfaceSections[0].SideConnectors[0].FirstSideElement!.ComponentID,
        value.CasingSections[0].ComponentID,
        value.CasingSections[0].CasingSectionElements[0].ComponentID,
        value.CasingSections[0].OpenHoleSection!.ComponentID,
        value.CasingSections[0].OpenHoleSection!.HoleSizes[0].ComponentID
    ];
}
