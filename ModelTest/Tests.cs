using OSDC.DotnetLibraries.Drilling.DrillingProperties;
using OSDC.DotnetLibraries.General.DataManagement;
using OSDC.DotnetLibraries.General.Statistics;
using OSDC.Drilling.WellBoreArchitecture.Model;

namespace OSDC.Drilling.WellBoreArchitecture.ModelTest
{
    public class Tests
    {
        [OneTimeSetUp]
        public void OneTimeSetUp()
        {
        }

        [Test]
        public void Calculate_accepts_an_architecture_without_surface_sections()
        {
            Guid guid = Guid.NewGuid();
            MetaInfo metaInfo = new() { ID = guid };
            DateTimeOffset creationDate = DateTimeOffset.UtcNow;

            Guid guid2 = Guid.NewGuid();
            MetaInfo metaInfo2 = new() { ID = guid2 };
            DateTimeOffset creationDate2 = DateTimeOffset.UtcNow;
            ScalarDrillingProperty derivedData1Param = new() { DiracDistributionValue = new DiracDistribution() { Value = 2.0 } };

            Model.WellBoreArchitecture wellBoreArchitecture = new()
            {
                MetaInfo = metaInfo,
                Name = "My test WellBoreArchitecture",
                Description = "My test WellBoreArchitecture",
                CreationDate = creationDate,
                LastModificationDate = creationDate,
            };

            Assert.Multiple(() =>
            {
                Assert.That(wellBoreArchitecture.SurfaceSections, Is.Empty);
                Assert.That(wellBoreArchitecture.Calculate(), Is.True);
            });
        }

        [Test]
        public void Realize_keeps_casing_borehole_diameters_and_root_open_hole_separate()
        {
            OSDC.DotnetLibraries.General.DrillingProperties.GaussianDrillingProperty value = new()
            {
                GaussianValue = new GaussianDistribution { Mean = 1.0 }
            };
            Model.WellBoreArchitecture architecture = new()
            {
                CasingSections =
                [
                    new CasingSection
                    {
                        TopDepth = value,
                        Length = value,
                        TopCementDepth = value,
                        CasingSectionElements = [],
                        CasingSectionSizeTable = [new BoreHoleSize { HoleSize = value, Length = value }]
                    }
                ],
                OpenHoleSection = new OpenHoleSection
                {
                    HoleSizes = [new BoreHoleSize { HoleSize = value, Length = value }]
                }
            };

            WellBoreArchitectureRealization realization = architecture.Realize();

            Assert.Multiple(() =>
            {
                Assert.That(realization.CasingSections[0].CasingSectionSizeTable, Has.Count.EqualTo(1));
                Assert.That(realization.OpenHoleSection?.HoleSizes, Has.Count.EqualTo(1));
            });
        }

        [OneTimeTearDown]
        public void OneTimeTearDown()
        {
        }
    }
}
