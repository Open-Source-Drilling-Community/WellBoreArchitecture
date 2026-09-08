namespace OSDC.Drilling.WellBoreArchitecture.ModelShared
{
	public class PseudoConstructors
	{
		public static MetaInfo ConstructMetaInfo()
			{
				return new MetaInfo 
				{
					ID = Guid.NewGuid(),
					HttpHostName = "https://dev.digiwells.no/",
					HttpHostBasePath = "WellBoreArchitecture/api/",
					HttpEndPoint = "WellBoreArchitectureOrder/",
				};
			}

		public static MetaInfo ConstructMetaInfo(Guid id)
			{
				return new MetaInfo 
				{
					ID = id,
					HttpHostName = "https://dev.digiwells.no/",
					HttpHostBasePath = "WellBoreArchitecture/api/",
					HttpEndPoint = "WellBoreArchitectureOrder/",
				};
			}
		public static GaussianDrillingProperty ConstructGaussianDrillingProperty()
		{
			return new GaussianDrillingProperty
			{
				GaussianValue = ConstructGaussianDistribution(),
			};
		}
		public static GaussianDistribution ConstructGaussianDistribution()
		{
			return new GaussianDistribution
			{
				MinValue = 0.0,
				MaxValue = 0.0,
				Mean = null,
				StandardDeviation = null,
			};
		}
		public static ScalarDrillingProperty ConstructScalarDrillingProperty()
		{
			return new ScalarDrillingProperty
			{
				DiracDistributionValue = ConstructDiracDistribution(),
			};
		}
		public static DiracDistribution ConstructDiracDistribution()
		{
			return new DiracDistribution
			{
				MinValue = 0.0,
				MaxValue = 0.0,
				Value = null,
			};
		}
		public static BoreHoleSize ConstructBoreHoleSize()
		{
			return new BoreHoleSize
			{
				ComponentID = new Guid(),
				HoleSize = ConstructGaussianDrillingProperty(),
				Length = ConstructGaussianDrillingProperty(),
			};
		}
		public static CasingSection ConstructCasingSection()
		{
			return new CasingSection
			{
				ComponentID = new Guid(),
				TopDepth = ConstructGaussianDrillingProperty(),
				Length = ConstructGaussianDrillingProperty(),
				TopCementDepth = ConstructGaussianDrillingProperty(),
				CasingSectionElements = new List<CasingSectionElement>
					{
						ConstructCasingSectionElement(),
					},
				CasingSectionSizeTable = new List<BoreHoleSize>
					{
						ConstructBoreHoleSize(),
					},
				OpenHoleSection = ConstructOpenHoleSection(),
			};
		}
		public static CasingSectionElement ConstructCasingSectionElement()
		{
			return new CasingSectionElement
			{
				ComponentID = new Guid(),
				BodyOD = ConstructGaussianDrillingProperty(),
				BodyID = ConstructGaussianDrillingProperty(),
				CollarOD = ConstructGaussianDrillingProperty(),
				JointLength = ConstructGaussianDrillingProperty(),
				SectionLength = ConstructGaussianDrillingProperty(),
				MaxDLS = ConstructScalarDrillingProperty(),
				ConnectionType = "Default ConnectionType",
				Grade = "Default Grade",
				MaterialDensity = ConstructGaussianDrillingProperty(),
				YoungModulus = ConstructGaussianDrillingProperty(),
				LinearWeight = ConstructGaussianDrillingProperty(),
				TensileStrength = ConstructGaussianDrillingProperty(),
				TorsionalStrength = ConstructGaussianDrillingProperty(),
				BurstPressure = ConstructGaussianDrillingProperty(),
				CollapsePressure = ConstructGaussianDrillingProperty(),
				YieldStress = ConstructGaussianDrillingProperty(),
				MakeUpTorqueRecommended = ConstructScalarDrillingProperty(),
			};
		}
		public static OpenHoleSection ConstructOpenHoleSection()
		{
			return new OpenHoleSection
			{
				ComponentID = new Guid(),
				HoleSizes = new List<BoreHoleSize>
					{
						ConstructBoreHoleSize(),
					},
			};
		}
	}
}