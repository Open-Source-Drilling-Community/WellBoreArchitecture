using System.Net.Http.Headers;
using NORCE.Drilling.WellBoreArchitecture.ModelShared;

namespace ServiceTest
{
    public class Tests
    {
        // testing outside Visual Studio requires using http port (https faces authentication issues both in console and on github)
        private static string host = "http://localhost:8080/";
        //private static string host = "https://localhost:5001/";
        //private static string host = "https://localhost:44368/";
        //private static string host = "http://localhost:54949/";
        private static HttpClient httpClient;
        private static Client nSwagClient;

        [OneTimeSetUp]
        public void OneTimeSetUp()
        {
            var handler = new HttpClientHandler();
            handler.ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => { return true; }; // temporary workaround for testing purposes: bypass certificate validation (not recommended for production environments due to security risks)
            httpClient = new HttpClient(handler);
            httpClient.BaseAddress = new Uri(host + "WellBoreArchitecture/api/");
            httpClient.DefaultRequestHeaders.Accept.Clear();
            httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            nSwagClient = new Client(httpClient.BaseAddress.ToString(), httpClient);
        }
       public WellBoreArchitecture InstantiateWellBore(MetaInfo metaInfo)
        {
            ScalarDrillingProperty defaultScalar = new ScalarDrillingProperty() { DiracDistributionValue = new DiracDistribution { Value = 0.0 } };
            GaussianDrillingProperty defaultGaussian = new GaussianDrillingProperty() { GaussianValue = new GaussianDistribution { Mean = 0.0 } };
               
            WellHead wellHead = new WellHead()
            {
                MaxOD = defaultScalar,
                MinOD = defaultScalar,
                Depth = defaultGaussian,
                CasingHangerDepth = defaultScalar,
                TubingHangerDepth = defaultScalar
            };
            List<WellBoreArchitectureFluid> fluidsAboveGroundLevel = new List<WellBoreArchitectureFluid>()
            {
                new WellBoreArchitectureFluid()
                {
                    Fluid = FluidType.Air,
                    Depth = defaultGaussian
                }
            };
            List<SurfaceSection> surfaceSections = new List<SurfaceSection>()
            {
                new SurfaceSection()
                {
                    Type = SurfaceSectionType.Unknown,
                    BodyID = defaultGaussian,
                    BodyOD = defaultGaussian,
                    BurstPressure = defaultGaussian,
                    CollapsePressure = defaultGaussian,
                    SideConnectors =  new List<SideConnector>()
                    {
                        new SideConnector()
                        {
                            Position = defaultGaussian,
                            VerticalDepth = defaultGaussian,
                            FirstSideElement = new SideElement()
                            {
                                Name = "",
                                Type = SideElementType.Unknown,
                                Length = defaultGaussian,
                                TopVerticalDepth = defaultGaussian,
                                OD = defaultGaussian,
                                ID = defaultGaussian
                            }
                        }
                    }
                }
            };
            List<CasingSection> casingSections = new List<CasingSection>()
            {
                new CasingSection()
                {
                    CasingSectionElements = new List<CasingSectionElement>()
                        {
                            new CasingSectionElement()
                            {
                                BodyID = defaultGaussian,
                                BodyOD = defaultGaussian,
                                CollarOD = defaultGaussian,
                                JointLength = defaultGaussian
                            }
                        },
                    Length = defaultGaussian,
                    TopCementDepth = defaultGaussian,
                    TopDepth = defaultGaussian,
                    CasingSectionSizeTable = new List<BoreHoleSize>
                        {   
                            new BoreHoleSize
                            {
                                HoleSize = defaultGaussian,
                                Length = defaultGaussian
                            }
                        }
                }
            };

            WellBoreArchitecture wellBoreArchitecture = new()
            {
                MetaInfo = metaInfo,
                Name = "My test WellBoreArchitecture",
                WellHead = wellHead,
                FluidsAboveGroundLevel = fluidsAboveGroundLevel,
                SurfaceSections = surfaceSections,
                CasingSections = casingSections

            };
            return wellBoreArchitecture;
        
        }


        [Test]
        public async Task Test_WellBoreArchitecture_GET()
        {
            #region post a WellBoreArchitecture
            Guid guid = Guid.NewGuid();
            MetaInfo metaInfo = new() { ID = guid };
            DateTimeOffset creationDate = DateTimeOffset.UtcNow;
            
            
            ScalarDrillingProperty derivedData1Param = new() { DiracDistributionValue = new DiracDistribution() { Value = 1.0 } };
            WellBoreArchitecture wellBoreArchitecture = InstantiateWellBore(metaInfo);

            try
            {
                await nSwagClient.PostWellBoreArchitectureAsync(wellBoreArchitecture);
            }
            catch (ApiException ex)
            {
                TestContext.WriteLine("Impossible to POST given WellBoreArchitecture\n" + ex.Message);
            }
            #endregion

            #region GetAllWellBoreArchitectureId
            List<Guid> idList = [];
            try
            {
                idList = (List<Guid>)await nSwagClient.GetAllWellBoreArchitectureIdAsync();
            }
            catch (ApiException ex)
            {
                TestContext.WriteLine("Impossible to GET all WellBoreArchitecture ids\n" + ex.Message);
            }
            Assert.That(idList, Is.Not.Null);
            Assert.That(idList, Does.Contain(guid));
            #endregion

            #region GetAllWellBoreArchitectureMetaInfo
            List<MetaInfo> metaInfoList = [];
            try
            {
                metaInfoList = (List<MetaInfo>)await nSwagClient.GetAllWellBoreArchitectureMetaInfoAsync();
            }
            catch (ApiException ex)
            {
                TestContext.WriteLine("Impossible to GET all WellBoreArchitecture metainfos\n" + ex.Message);
            }
            Assert.That(metaInfoList, Is.Not.Null);
            IEnumerable<MetaInfo> metaInfoList2 =
                from elt in metaInfoList
                where elt.ID == guid
                select elt;
            Assert.That(metaInfoList2, Is.Not.Null);
            Assert.That(metaInfoList2, Is.Not.Empty);
            #endregion

            #region GetAllWellBoreArchitectureById
            WellBoreArchitecture? wellBoreArchitecture2 = null;
            try
            {
                wellBoreArchitecture2 = await nSwagClient.GetWellBoreArchitectureByIdAsync(guid);
            }
            catch (ApiException ex)
            {
                TestContext.WriteLine("Impossible to GET WellBoreArchitecture of given Id\n" + ex.Message);
            }
            Assert.That(wellBoreArchitecture2, Is.Not.Null);
            Assert.That(wellBoreArchitecture2.Name, Is.EqualTo(wellBoreArchitecture.Name));
            #endregion

            #region GetAllWellBoreArchitectureLight
            List<WellBoreArchitectureLight> wellBoreArchitectureLightList = [];
            try
            {
                wellBoreArchitectureLightList = (List<WellBoreArchitectureLight>)await nSwagClient.GetAllWellBoreArchitectureLightAsync();
            }
            catch (ApiException ex)
            {
                TestContext.WriteLine("Impossible to GET the list of WellBoreArchitectureLight\n" + ex.Message);
            }
            Assert.That(wellBoreArchitectureLightList, Is.Not.Null);
            Assert.That(wellBoreArchitectureLightList, Is.Not.Empty);
            IEnumerable<WellBoreArchitectureLight> wellBoreArchitectureLightList2 =
                from elt in wellBoreArchitectureLightList
                where elt.Name == wellBoreArchitecture.Name
                select elt;
            Assert.That(wellBoreArchitectureLightList2, Is.Not.Null);
            Assert.That(wellBoreArchitectureLightList2, Is.Not.Empty);
            #endregion

            #region GetAllWellBoreArchitecture
            List<WellBoreArchitecture> wellBoreArchitectureList = new();
            try
            {
                wellBoreArchitectureList = (List<WellBoreArchitecture>)await nSwagClient.GetAllWellBoreArchitectureAsync();
            }
            catch (ApiException ex)
            {
                TestContext.WriteLine("Impossible to GET the list of WellBoreArchitecture\n" + ex.Message);
            }
            Assert.That(wellBoreArchitectureList, Is.Not.Null);
            IEnumerable<WellBoreArchitecture> wellBoreArchitectureList2 =
                from elt in wellBoreArchitectureList
                where elt.Name == wellBoreArchitecture.Name
                select elt;
            Assert.That(wellBoreArchitectureList2, Is.Not.Null);
            Assert.That(wellBoreArchitectureList2, Is.Not.Empty);
            #endregion

            #region finally delete the new ID
            wellBoreArchitecture2 = null;
            try
            {
                await nSwagClient.DeleteWellBoreArchitectureByIdAsync(guid);
            }
            catch (ApiException ex)
            {
                TestContext.WriteLine("Impossible to DELETE WellBoreArchitecture of given Id\n" + ex.Message);
            }
            try
            {
                wellBoreArchitecture2 = await nSwagClient.GetWellBoreArchitectureByIdAsync(guid);
            }
            catch (ApiException ex)
            {
                Assert.That(ex.StatusCode, Is.EqualTo(404));
                TestContext.WriteLine("Impossible to GET WellBoreArchitecture of given Id\n" + ex.Message);
            }
            Assert.That(wellBoreArchitecture2, Is.Null);
            #endregion
        }

        [Test]
        public async Task Test_WellBoreArchitecture_POST()
        {
            #region trying to post an empty guid
            Guid guid = Guid.Empty;
            MetaInfo metaInfo = new() { ID = guid };
            DateTimeOffset creationDate = DateTimeOffset.UtcNow;
            
            ScalarDrillingProperty derivedData1Param = new() { DiracDistributionValue = new DiracDistribution() { Value = 1.0 } };
  
            WellBoreArchitecture wellBoreArchitecture = InstantiateWellBore(metaInfo);

            WellBoreArchitecture? wellBoreArchitecture2 = null;
            try
            {
                await nSwagClient.PostWellBoreArchitectureAsync(wellBoreArchitecture);
            }
            catch (ApiException ex)
            {
                Assert.That(ex.StatusCode, Is.EqualTo(400));
                TestContext.WriteLine("Impossible to POST WellBoreArchitecture with empty Guid\n" + ex.Message);
            }
            try
            {
                wellBoreArchitecture2 = await nSwagClient.GetWellBoreArchitectureByIdAsync(Guid.Empty);
            }
            catch (ApiException ex)
            {
                Assert.That(ex.StatusCode, Is.EqualTo(400));
                TestContext.WriteLine("Impossible to GET WellBoreArchitecture identified by an empty Guid\n" + ex.Message);
            }
            Assert.That(wellBoreArchitecture2, Is.Null);
            #endregion

            #region post some corrupted data
            // post data with missing input that fails the calculation process
            #endregion

            #region posting a new ID in a valid state
            guid = Guid.NewGuid();
            metaInfo = new() { ID = guid };
            wellBoreArchitecture.MetaInfo = metaInfo;
            try
            {
                await nSwagClient.PostWellBoreArchitectureAsync(wellBoreArchitecture);
            }
            catch (ApiException ex)
            {
                TestContext.WriteLine("Impossible to POST WellBoreArchitecture although it is in a valid state\n" + ex.Message);
            }
            try
            {
                wellBoreArchitecture2 = await nSwagClient.GetWellBoreArchitectureByIdAsync(guid);
            }
            catch (ApiException ex)
            {
                TestContext.WriteLine("Impossible to GET the WellBoreArchitecture of given Id\n" + ex.Message);
            }
            Assert.That(wellBoreArchitecture2, Is.Not.Null);
            Assert.That(wellBoreArchitecture2.MetaInfo, Is.Not.Null);
            Assert.That(wellBoreArchitecture2.MetaInfo.ID, Is.EqualTo(guid));
            Assert.That(wellBoreArchitecture2.Name, Is.EqualTo(wellBoreArchitecture.Name));
            #endregion

            #region trying to repost the same ID
            bool conflict = false;
            try
            {
                await nSwagClient.PostWellBoreArchitectureAsync(wellBoreArchitecture);
            }
            catch (ApiException ex)
            {
                conflict = true;
                Assert.That(ex.StatusCode, Is.EqualTo(409));
                TestContext.WriteLine("Impossible to POST existing WellBoreArchitecture\n" + ex.Message);
            }
            Assert.That(conflict, Is.True);
            #endregion

            #region finally delete the new ID
            wellBoreArchitecture2 = null;
            try
            {
                await nSwagClient.DeleteWellBoreArchitectureByIdAsync(guid);
            }
            catch (ApiException ex)
            {
                TestContext.WriteLine("Impossible to DELETE WellBoreArchitecture of given Id\n" + ex.Message);
            }
            try
            {
                wellBoreArchitecture2 = await nSwagClient.GetWellBoreArchitectureByIdAsync(guid);
            }
            catch (ApiException ex)
            {
                Assert.That(ex.StatusCode, Is.EqualTo(404));
                TestContext.WriteLine("Impossible to GET deleted WellBoreArchitecture of given Id\n" + ex.Message);
            }
            Assert.That(wellBoreArchitecture2, Is.Null);
            #endregion
        }

        [Test]
        public async Task Test_WellBoreArchitecture_PUT()
        {
            #region posting a new ID
            Guid guid = Guid.NewGuid();
            MetaInfo metaInfo = new() { ID = guid };
            DateTimeOffset creationDate = DateTimeOffset.UtcNow;
            
            ScalarDrillingProperty derivedData1Param = new() { DiracDistributionValue = new DiracDistribution() { Value = 1.0 } };
  
            WellBoreArchitecture wellBoreArchitecture = InstantiateWellBore(metaInfo);

            WellBoreArchitecture? wellBoreArchitecture2 = null;
            try
            {
                await nSwagClient.PostWellBoreArchitectureAsync(wellBoreArchitecture);
            }
            catch (ApiException ex)
            {
                TestContext.WriteLine("Impossible to POST WellBoreArchitecture\n" + ex.Message);
            }
            try
            {
                wellBoreArchitecture2 = await nSwagClient.GetWellBoreArchitectureByIdAsync(guid);
            }
            catch (ApiException ex)
            {
                TestContext.WriteLine("Impossible to GET the WellBoreArchitecture of given Id\n" + ex.Message);
            }
            Assert.That(wellBoreArchitecture2, Is.Not.Null);
            Assert.That(wellBoreArchitecture2.MetaInfo, Is.Not.Null);
            Assert.That(wellBoreArchitecture2.MetaInfo.ID, Is.EqualTo(guid));
            Assert.That(wellBoreArchitecture2.Name, Is.EqualTo(wellBoreArchitecture.Name));
            #endregion

            #region updating the new Id
            wellBoreArchitecture.Name = "My test WellBoreArchitecture with modified name";
            wellBoreArchitecture.LastModificationDate = DateTimeOffset.UtcNow;
            try
            {
                await nSwagClient.PutWellBoreArchitectureByIdAsync(wellBoreArchitecture.MetaInfo.ID, wellBoreArchitecture);
            }
            catch (ApiException ex)
            {
                TestContext.WriteLine("Impossible to PUT WellBoreArchitecture of given Id\n" + ex.Message);
            }
            try
            {
                wellBoreArchitecture2 = await nSwagClient.GetWellBoreArchitectureByIdAsync(guid);
            }
            catch (ApiException ex)
            {
                TestContext.WriteLine("Impossible to GET the updated WellBoreArchitecture of given Id\n" + ex.Message);
            }
            Assert.That(wellBoreArchitecture2, Is.Not.Null);
            Assert.That(wellBoreArchitecture2.MetaInfo, Is.Not.Null);
            Assert.That(wellBoreArchitecture2.MetaInfo.ID, Is.EqualTo(wellBoreArchitecture.MetaInfo.ID));
            Assert.That(wellBoreArchitecture2.Name, Is.EqualTo(wellBoreArchitecture.Name));
            #endregion

            #region finally delete the new ID
            wellBoreArchitecture2 = null;
            try
            {
                await nSwagClient.DeleteWellBoreArchitectureByIdAsync(guid);
            }
            catch (ApiException ex)
            {
                TestContext.WriteLine("Impossible to DELETE WellBoreArchitecture of given Id\n" + ex.Message);
            }
            try
            {
                wellBoreArchitecture2 = await nSwagClient.GetWellBoreArchitectureByIdAsync(guid);
            }
            catch (ApiException ex)
            {
                Assert.That(ex.StatusCode, Is.EqualTo(404));
                TestContext.WriteLine("Impossible to GET deleted WellBoreArchitecture of given Id\n" + ex.Message);
            }
            Assert.That(wellBoreArchitecture2, Is.Null);
            #endregion
        }

        [Test]
        public async Task Test_WellBoreArchitecture_DELETE()
        {
            #region posting a new ID
            Guid guid = Guid.NewGuid();
            MetaInfo metaInfo = new() { ID = guid };
            DateTimeOffset creationDate = DateTimeOffset.UtcNow;
            
            ScalarDrillingProperty derivedData1Param = new() { DiracDistributionValue = new DiracDistribution() { Value = 1.0 } };
  
            WellBoreArchitecture wellBoreArchitecture = InstantiateWellBore(metaInfo);

            WellBoreArchitecture? wellBoreArchitecture2 = null;
            try
            {
                await nSwagClient.PostWellBoreArchitectureAsync(wellBoreArchitecture);
            }
            catch (ApiException ex)
            {
                TestContext.WriteLine("Impossible to POST WellBoreArchitecture\n" + ex.Message);
            }
            try
            {
                wellBoreArchitecture2 = await nSwagClient.GetWellBoreArchitectureByIdAsync(guid);
            }
            catch (ApiException ex)
            {
                TestContext.WriteLine("Impossible to GET the WellBoreArchitecture of given Id\n" + ex.Message);
            }
            Assert.That(wellBoreArchitecture2, Is.Not.Null);
            Assert.That(wellBoreArchitecture2.MetaInfo, Is.Not.Null);
            Assert.That(wellBoreArchitecture2.MetaInfo.ID, Is.EqualTo(wellBoreArchitecture.MetaInfo.ID));
            Assert.That(wellBoreArchitecture2.Name, Is.EqualTo(wellBoreArchitecture.Name));
            #endregion

            #region finally delete the new ID
            wellBoreArchitecture2 = null;
            try
            {
                await nSwagClient.DeleteWellBoreArchitectureByIdAsync(guid);
            }
            catch (ApiException ex)
            {
                TestContext.WriteLine("Impossible to DELETE WellBoreArchitecture of given Id\n" + ex.Message);
            }
            try
            {
                wellBoreArchitecture2 = await nSwagClient.GetWellBoreArchitectureByIdAsync(guid);
            }
            catch (ApiException ex)
            {
                Assert.That(ex.StatusCode, Is.EqualTo(404));
                TestContext.WriteLine("Impossible to GET deleted WellBoreArchitecture of given Id\n" + ex.Message);
            }
            Assert.That(wellBoreArchitecture2, Is.Null);
            #endregion
        }

    

        [OneTimeTearDown]
        public void OneTimeTearDown()
        {
            httpClient?.Dispose();
        }
    }
}