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

        [OneTimeTearDown]
        public void OneTimeTearDown()
        {
        }
    }
}
