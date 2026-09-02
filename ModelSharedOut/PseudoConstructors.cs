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
		public static Cluster ConstructCluster()
		{
			return new Cluster
			{
				MetaInfo = ConstructMetaInfo(),
				Name = "Default Name",
				Description = "Default Description",
				CreationDate = DateTimeOffset.UtcNow,
				LastModificationDate = DateTimeOffset.UtcNow,
				FieldID = null, 
				IsSingleWell = false, 
				RigID = null, 
				IsFixedPlatform = false, 
				ReferenceLatitude = ConstructGaussianDrillingProperty(),
				ReferenceLongitude = ConstructGaussianDrillingProperty(),
				ReferenceDepth = ConstructGaussianDrillingProperty(),
				GroundMudLineDepth = ConstructGaussianDrillingProperty(),
				TopWaterDepth = ConstructGaussianDrillingProperty(),
				Slots = new Dictionary<string,Slot>
					{
						{ "", ConstructSlot() }
					},
			};
		}
		public static CountPerDay ConstructCountPerDay()
		{
			return new CountPerDay
			{
				Date = DateTimeOffset.UtcNow,
				Count = 0, 
			};
		}
		public static History ConstructHistory()
		{
			return new History
			{
				Data = new List<CountPerDay>
					{
						ConstructCountPerDay(),
					},
			};
		}
		public static Slot ConstructSlot()
		{
			return new Slot
			{
				ID = new Guid(),
				Name = "Default Name",
				Description = "Default Description",
				CreationDate = DateTimeOffset.UtcNow,
				LastModificationDate = DateTimeOffset.UtcNow,
				Latitude = ConstructGaussianDrillingProperty(),
				Longitude = ConstructGaussianDrillingProperty(),
			};
		}
		public static UsageStatisticsCluster ConstructUsageStatisticsCluster()
		{
			return new UsageStatisticsCluster
			{
				LastSaved = DateTimeOffset.UtcNow,
				BackUpInterval = "Default BackUpInterval",
				GetAllClusterIdPerDay = ConstructHistory(),
				GetAllClusterMetaInfoPerDay = ConstructHistory(),
				GetClusterByIdPerDay = ConstructHistory(),
				GetAllClusterPerDay = ConstructHistory(),
				PostClusterPerDay = ConstructHistory(),
				PutClusterByIdPerDay = ConstructHistory(),
				DeleteClusterByIdPerDay = ConstructHistory(),
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
		public static UsageStatisticsCartographicProjection ConstructUsageStatisticsCartographicProjection()
		{
			return new UsageStatisticsCartographicProjection
			{
				LastSaved = DateTimeOffset.UtcNow,
				BackUpInterval = "Default BackUpInterval",
				GetAllCartographicProjectionTypeIdPerDay = ConstructHistory(),
				GetCartographicProjectionTypeByIdPerDay = ConstructHistory(),
				GetAllCartographicProjectionTypePerDay = ConstructHistory(),
				GetAllCartographicProjectionIdPerDay = ConstructHistory(),
				GetAllCartographicProjectionMetaInfoPerDay = ConstructHistory(),
				GetCartographicProjectionByIdPerDay = ConstructHistory(),
				GetAllCartographicProjectionLightPerDay = ConstructHistory(),
				GetAllCartographicProjectionPerDay = ConstructHistory(),
				PostCartographicProjectionPerDay = ConstructHistory(),
				PutCartographicProjectionByIdPerDay = ConstructHistory(),
				DeleteCartographicProjectionByIdPerDay = ConstructHistory(),
				GetAllCartographicConversionSetIdPerDay = ConstructHistory(),
				GetAllCartographicConversionSetMetaInfoPerDay = ConstructHistory(),
				GetCartographicConversionSetByIdPerDay = ConstructHistory(),
				GetAllCartographicConversionSetLightPerDay = ConstructHistory(),
				GetAllCartographicConversionSetPerDay = ConstructHistory(),
				PostCartographicConversionSetPerDay = ConstructHistory(),
				PutCartographicConversionSetByIdPerDay = ConstructHistory(),
				DeleteCartographicConversionSetByIdPerDay = ConstructHistory(),
			};
		}
		public static CartographicConversionSet ConstructCartographicConversionSet()
		{
			return new CartographicConversionSet
			{
				MetaInfo = ConstructMetaInfo(),
				Name = "Default Name",
				Description = "Default Description",
				CreationDate = DateTimeOffset.UtcNow,
				LastModificationDate = DateTimeOffset.UtcNow,
				CartographicProjectionID = null, 
				CartographicCoordinateList = new List<CartographicCoordinate>
					{
						ConstructCartographicCoordinate(),
					},
			};
		}
		public static CartographicCoordinate ConstructCartographicCoordinate()
		{
			return new CartographicCoordinate
			{
				Northing = null, 
				Easting = null, 
				VerticalDepth = null, 
				GeodeticCoordinate = ConstructGeodeticCoordinate(),
				GridConvergenceDatum = null, 
			};
		}
		public static CartographicProjection ConstructCartographicProjection()
		{
			return new CartographicProjection
			{
				MetaInfo = ConstructMetaInfo(),
				Name = "Default Name",
				Description = "Default Description",
				CreationDate = DateTimeOffset.UtcNow,
				LastModificationDate = DateTimeOffset.UtcNow,
				ProjectionType = (ProjectionType)0,
				GeodeticDatumID = null, 
				LatitudeOrigin = null, 
				Latitude1 = null, 
				Latitude2 = null, 
				LatitudeTrueScale = null, 
				LongitudeOrigin = null, 
				Scaling = null, 
				FalseEasting = null, 
				FalseNorthing = null, 
				Zone = 0, 
				IsSouth = false, 
				IsHyperbolic = false, 
				ProjectionHeight = null, 
				HeightViewPoint = null, 
				Sweep = (AxisType)0,
				AzimuthCentralLine = null, 
				Weight = null, 
				Landsat = null, 
				Path = null, 
				Alpha = null, 
				Gamma = null, 
				Longitude1 = null, 
				Longitude2 = null, 
				LongitudeCentralPoint = null, 
				NoOffset = false, 
				NoRotation = false, 
				AreaNormalizationTransform = (AreaNormalizationTransformType)0,
				PegLatitude = null, 
				PegLongitude = null, 
				PegHeading = null, 
				N = null, 
				Q = null, 
			};
		}
		public static CartographicProjectionType ConstructCartographicProjectionType()
		{
			return new CartographicProjectionType
			{
				Projection = (ProjectionType)0,
				UseLatitudeOrigin = false, 
				UseLatitude1 = false, 
				UseLatitude2 = false, 
				UseLatitudeTrueScale = false, 
				UseLongitudeOrigin = false, 
				UseScaling = false, 
				UseFalseEastingNorthing = false, 
				UseZone = false, 
				UseSouth = false, 
				UseHyperbolic = false, 
				UseProjectionHeight = false, 
				UseHeightViewPoint = false, 
				UseSweep = false, 
				UseAzimuthCentralLine = false, 
				UseWeight = false, 
				UseLandsat = false, 
				UsePath = false, 
				UseAlpha = false, 
				UseGamma = false, 
				UseLongitude1 = false, 
				UseLongitude2 = false, 
				UseLongitudeCentralPoint = false, 
				UseNoOffset = false, 
				UseNoRotation = false, 
				UseAreaNormalizationTransform = false, 
				UsePegLatitude = false, 
				UsePegLongitude = false, 
				UsePegHeading = false, 
				UseN = false, 
				UseQ = false, 
			};
		}
		public static GeodeticCoordinate ConstructGeodeticCoordinate()
		{
			return new GeodeticCoordinate
			{
				LatitudeWGS84 = null, 
				LongitudeWGS84 = null, 
				VerticalDepthWGS84 = null, 
				LatitudeDatum = null, 
				LongitudeDatum = null, 
				VerticalDepthDatum = null, 
				OctreeDepth = 0, 
				OctreeCode = ConstructOctreeCodeLong(),
			};
		}
		public static OctreeCodeLong ConstructOctreeCodeLong()
		{
			return new OctreeCodeLong
			{
				Depth = 0, 
				CodeHigh = 0, 
				CodeLow = 0, 
			};
		}
		public static UsageStatisticsGeodeticDatum ConstructUsageStatisticsGeodeticDatum()
		{
			return new UsageStatisticsGeodeticDatum
			{
				LastSaved = DateTimeOffset.UtcNow,
				BackUpInterval = "Default BackUpInterval",
				GetAllSpheroidIdPerDay = ConstructHistory(),
				GetAllSpheroidMetaInfoPerDay = ConstructHistory(),
				GetSpheroidByIdPerDay = ConstructHistory(),
				GetAllSpheroidPerDay = ConstructHistory(),
				PostSpheroidPerDay = ConstructHistory(),
				PutSpheroidByIdPerDay = ConstructHistory(),
				DeleteSpheroidByIdPerDay = ConstructHistory(),
				GetAllGeodeticDatumIdPerDay = ConstructHistory(),
				GetAllGeodeticDatumMetaInfoPerDay = ConstructHistory(),
				GetGeodeticDatumByIdPerDay = ConstructHistory(),
				GetAllGeodeticDatumLightPerDay = ConstructHistory(),
				GetAllGeodeticDatumPerDay = ConstructHistory(),
				PostGeodeticDatumPerDay = ConstructHistory(),
				PutGeodeticDatumByIdPerDay = ConstructHistory(),
				DeleteGeodeticDatumByIdPerDay = ConstructHistory(),
				GetAllGeodeticConversionSetIdPerDay = ConstructHistory(),
				GetAllGeodeticConversionSetMetaInfoPerDay = ConstructHistory(),
				GetGeodeticConversionSetByIdPerDay = ConstructHistory(),
				GetAllGeodeticConversionSetLightPerDay = ConstructHistory(),
				GetAllGeodeticConversionSetPerDay = ConstructHistory(),
				PostGeodeticConversionSetPerDay = ConstructHistory(),
				PutGeodeticConversionSetByIdPerDay = ConstructHistory(),
				DeleteGeodeticConversionSetByIdPerDay = ConstructHistory(),
			};
		}
		public static GeodeticConversionSet ConstructGeodeticConversionSet()
		{
			return new GeodeticConversionSet
			{
				MetaInfo = ConstructMetaInfo(),
				Name = "Default Name",
				Description = "Default Description",
				CreationDate = DateTimeOffset.UtcNow,
				LastModificationDate = DateTimeOffset.UtcNow,
				GeodeticDatum = ConstructGeodeticDatum(),
				OctreeBounds = ConstructBounds(),
				GeodeticCoordinates = new List<GeodeticCoordinate>
					{
						ConstructGeodeticCoordinate(),
					},
			};
		}
		public static GeodeticDatum ConstructGeodeticDatum()
		{
			return new GeodeticDatum
			{
				MetaInfo = ConstructMetaInfo(),
				Name = "Default Name",
				Description = "Default Description",
				CreationDate = DateTimeOffset.UtcNow,
				LastModificationDate = DateTimeOffset.UtcNow,
				IsDefault = false, 
				Spheroid = ConstructSpheroid(),
				DeltaX = ConstructScalarDrillingProperty(),
				DeltaY = ConstructScalarDrillingProperty(),
				DeltaZ = ConstructScalarDrillingProperty(),
				RotationX = ConstructScalarDrillingProperty(),
				RotationY = ConstructScalarDrillingProperty(),
				RotationZ = ConstructScalarDrillingProperty(),
				ScaleFactor = ConstructScalarDrillingProperty(),
			};
		}
		public static Spheroid ConstructSpheroid()
		{
			return new Spheroid
			{
				MetaInfo = ConstructMetaInfo(),
				Name = "Default Name",
				Description = "Default Description",
				CreationDate = DateTimeOffset.UtcNow,
				LastModificationDate = DateTimeOffset.UtcNow,
				IsDefault = false, 
				SemiMajorAxis = ConstructScalarDrillingProperty(),
				IsSemiMajorAxisSet = false, 
				SemiMinorAxis = ConstructScalarDrillingProperty(),
				IsSemiMinorAxisSet = false, 
				Eccentricity = ConstructScalarDrillingProperty(),
				IsEccentricitySet = false, 
				SquaredEccentricity = ConstructScalarDrillingProperty(),
				IsSquaredEccentricitySet = false, 
				Flattening = ConstructScalarDrillingProperty(),
				IsFlatteningSet = false, 
				InverseFlattening = ConstructScalarDrillingProperty(),
				IsInverseFlatteningSet = false, 
			};
		}
		public static ScalarDrillingProperty ConstructScalarDrillingProperty()
		{
			return new ScalarDrillingProperty
			{
				DiracDistributionValue = ConstructDiracDistribution(),
			};
		}
		public static Point3D ConstructPoint3D()
		{
			return new Point3D
			{
				X = null, 
				Y = null, 
				Z = null, 
			};
		}
		public static Bounds ConstructBounds()
		{
			return new Bounds
			{
				MinX = null, 
				MaxX = null, 
				MinY = null, 
				MaxY = null, 
				MinZ = null, 
				MaxZ = null, 
				MiddleX = null, 
				MiddleY = null, 
				MiddleZ = null, 
				IntervalX = null, 
				IntervalY = null, 
				IntervalZ = null, 
				Center = ConstructPoint3D(),
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
		public static Field ConstructField()
		{
			return new Field
			{
				MetaInfo = ConstructMetaInfo(),
				Name = "Default Name",
				Description = "Default Description",
				CreationDate = DateTimeOffset.UtcNow,
				LastModificationDate = DateTimeOffset.UtcNow,
				CartographicProjectionID = null, 
			};
		}
		public static FieldCartographicConversionSet ConstructFieldCartographicConversionSet()
		{
			return new FieldCartographicConversionSet
			{
				MetaInfo = ConstructMetaInfo(),
				Name = "Default Name",
				Description = "Default Description",
				CreationDate = DateTimeOffset.UtcNow,
				LastModificationDate = DateTimeOffset.UtcNow,
				FieldID = null, 
				CartographicCoordinateList = new List<CartographicCoordinate>
					{
						ConstructCartographicCoordinate(),
					},
			};
		}
		public static UsageStatisticsField ConstructUsageStatisticsField()
		{
			return new UsageStatisticsField
			{
				LastSaved = DateTimeOffset.UtcNow,
				BackUpInterval = "Default BackUpInterval",
				GetAllFieldIdPerDay = ConstructHistory(),
				GetAllFieldMetaInfoPerDay = ConstructHistory(),
				GetFieldByIdPerDay = ConstructHistory(),
				GetAllFieldLightPerDay = ConstructHistory(),
				GetAllFieldPerDay = ConstructHistory(),
				PostFieldPerDay = ConstructHistory(),
				PutFieldByIdPerDay = ConstructHistory(),
				DeleteFieldByIdPerDay = ConstructHistory(),
				GetAllFieldCartographicConversionSetIdPerDay = ConstructHistory(),
				GetAllFieldCartographicConversionSetMetaInfoPerDay = ConstructHistory(),
				GetFieldCartographicConversionSetByIdPerDay = ConstructHistory(),
				GetAllFieldCartographicConversionSetLightPerDay = ConstructHistory(),
				GetAllFieldCartographicConversionSetPerDay = ConstructHistory(),
				PostFieldCartographicConversionSetPerDay = ConstructHistory(),
				PutFieldCartographicConversionSetByIdPerDay = ConstructHistory(),
				DeleteFieldCartographicConversionSetByIdPerDay = ConstructHistory(),
			};
		}
		public static Accumulator ConstructAccumulator()
		{
			return new Accumulator
			{
				Name = "Default Name",
				Description = "Default Description",
				Manufacturer = "Default Manufacturer",
				Model = "Default Model",
				ProductCode = "Default ProductCode",
				SerialNumber = "Default SerialNumber",
				AccumulatorClass = (AccumulatorClass)0,
				Capacity = null, 
				MaxLimitDesignPressure = null, 
				MaxLimitOperatingPressure = null, 
			};
		}
		public static AutoDriller ConstructAutoDriller()
		{
			return new AutoDriller
			{
				Name = "Default Name",
				Description = "Default Description",
				Manufacturer = "Default Manufacturer",
				Model = "Default Model",
				ProductCode = "Default ProductCode",
				SerialNumber = "Default SerialNumber",
				ControlMode = (AutodrillerControlMode)0,
				MaxLimitRop = null, 
				MinLimitRop = null, 
				MaxLimitWob = null, 
				MinLimitWob = null, 
				MaxLimitDifferentialPressure = null, 
				MinLimitDifferentialPressure = null, 
				MaxLimitTrq = null, 
				MinLimitTrq = null, 
				SetpointWob = null, 
				SetpointRop = null, 
				SetpointDiffp = null, 
				SetpointTrq = null, 
			};
		}
		public static AuxSolidsControl ConstructAuxSolidsControl()
		{
			return new AuxSolidsControl
			{
				Name = "Default Name",
				Description = "Default Description",
				Manufacturer = "Default Manufacturer",
				Model = "Default Model",
				ProductCode = "Default ProductCode",
				SerialNumber = "Default SerialNumber",
				SolidsControlClass = (SolidsControlClass)0,
				DesanderOn = null, 
				DesilterOn = null, 
				DegasserOn = null, 
				CentrifugeOn = null, 
			};
		}
		public static BopLineDefinition ConstructBopLineDefinition()
		{
			return new BopLineDefinition
			{
				BopLinesClass = (BopLineClass)0,
				LineOd = null, 
				LineId = null, 
				Length = null, 
			};
		}
		public static BopStack ConstructBopStack()
		{
			return new BopStack
			{
				Name = "Default Name",
				Description = "Default Description",
				Manufacturer = "Default Manufacturer",
				Model = "Default Model",
				ProductCode = "Default ProductCode",
				SerialNumber = "Default SerialNumber",
				BopStackClass = (BopStackClass)0,
				UnitReferenceList = "Default UnitReferenceList",
				BopControlType = (ControllerType)0,
				BoreDiameter = null, 
				Height = null, 
				Weight = null, 
				BopComponents = new List<BopStackComponentDefinition>
					{
						ConstructBopStackComponentDefinition(),
					},
				BopLines = new List<BopLineDefinition>
					{
						ConstructBopLineDefinition(),
					},
				MaxLimitDesignPressure = null, 
				MaxLimitOperatingPressure = null, 
				MinLimitOperatingPressure = null, 
				BopLineMaxLimitDesignPressure = null, 
				BopLineMaxLimitOperatingPressure = null, 
				CasingPressure = null, 
				KillLinePressure = null, 
				ChokeLinePressure = null, 
				ShutInDrillpipePressure = null, 
			};
		}
		public static BopStackComponentDefinition ConstructBopStackComponentDefinition()
		{
			return new BopStackComponentDefinition
			{
				BopStackComponentClass = (BopComponentClass)0,
				BoreDiameter = null, 
				Height = null, 
			};
		}
		public static CasingDriveSystem ConstructCasingDriveSystem()
		{
			return new CasingDriveSystem
			{
				Name = "Default Name",
				Description = "Default Description",
				Manufacturer = "Default Manufacturer",
				Model = "Default Model",
				ProductCode = "Default ProductCode",
				SerialNumber = "Default SerialNumber",
				CsgDrvClass = (CasingDriveClass)0,
				HoistingCapacity = null, 
				Length = null, 
				MaxLimitDesignTorque = null, 
				MaxLimitDesignPressure = null, 
				MaxLimitDesignRotationSpeed = null, 
				MaxLimitTorque = null, 
				MaxLimitPressure = null, 
				MaxLimitRotationSpeed = null, 
				MaxLimitPushDown = null, 
			};
		}
		public static CasingRunningTool ConstructCasingRunningTool()
		{
			return new CasingRunningTool
			{
				Name = "Default Name",
				Description = "Default Description",
			};
		}
		public static CasingTongs ConstructCasingTongs()
		{
			return new CasingTongs
			{
				Name = "Default Name",
				Description = "Default Description",
			};
		}
		public static CatWalk ConstructCatWalk()
		{
			return new CatWalk
			{
				Name = "Default Name",
				Description = "Default Description",
			};
		}
		public static CementPump ConstructCementPump()
		{
			return new CementPump
			{
				Name = "Default Name",
				Description = "Default Description",
				Manufacturer = "Default Manufacturer",
				Model = "Default Model",
				ProductCode = "Default ProductCode",
				SerialNumber = "Default SerialNumber",
				PumpClass = (PumpClass)0,
				PlungerDiameter = null, 
				StrokeLength = null, 
				CementPumpDisplacement = new List<CementPumpDisplacementPoint>
					{
						ConstructCementPumpDisplacementPoint(),
					},
				MaxLimitPressure = null, 
				MaxLimitFlowRate = null, 
			};
		}
		public static CementPumpDisplacementPoint ConstructCementPumpDisplacementPoint()
		{
			return new CementPumpDisplacementPoint
			{
				StrokeRate = null, 
				FlowRate = null, 
				Pressure = null, 
			};
		}
		public static CementUnit ConstructCementUnit()
		{
			return new CementUnit
			{
				Name = "Default Name",
				Description = "Default Description",
				Manufacturer = "Default Manufacturer",
				Model = "Default Model",
				ProductCode = "Default ProductCode",
				SerialNumber = "Default SerialNumber",
				Mounting = (MountingType)0,
				Features = "Default Features",
				NumberOfPumps = null, 
			};
		}
		public static Centrifuge ConstructCentrifuge()
		{
			return new Centrifuge
			{
				Name = "Default Name",
				Description = "Default Description",
			};
		}
		public static ChokeCvCurvePoint ConstructChokeCvCurvePoint()
		{
			return new ChokeCvCurvePoint
			{
				Pressure = null, 
				Flow = null, 
			};
		}
		public static ChokeManifold ConstructChokeManifold()
		{
			return new ChokeManifold
			{
				Name = "Default Name",
				Description = "Default Description",
				Manufacturer = "Default Manufacturer",
				Model = "Default Model",
				ProductCode = "Default ProductCode",
				SerialNumber = "Default SerialNumber",
				ChokeControlClass = (ControlClass)0,
				MaxLimitDesignPressure = null, 
				MaxLimitOperatingPressure = null, 
				MinLimitOperatingPressure = null, 
				MaxLimitTestPressure = null, 
				MaxLimitOperatingTemperature = null, 
				MinLimitOperatingTemperature = null, 
			};
		}
		public static CoilDriveSystem ConstructCoilDriveSystem()
		{
			return new CoilDriveSystem
			{
				Name = "Default Name",
				Description = "Default Description",
				Manufacturer = "Default Manufacturer",
				Model = "Default Model",
				ProductCode = "Default ProductCode",
				SerialNumber = "Default SerialNumber",
				CoilDrvClass = (MountingType)0,
				ReelPayloadCapacity = null, 
				ReelPayloadLength = null, 
				ReelRemainingLength = null, 
				InjectorHeadRadius = null, 
				InjectorHeadMinTubingOd = null, 
				InjHeadDesignPullCapacity = null, 
				InjHeadDesignSnubCapacity = null, 
				InjHeadPullCapacity = null, 
				InjHeadSnubCapacity = null, 
				InjHeadMaxSpeed = null, 
				CtLoad = null, 
				CtWeightOnBit = null, 
				CtCoilSpeed = null, 
				CtCircPressure = null, 
				CtWellheadPressure = null, 
				CtEngineSpeed = null, 
				CtInjHeadDrivePressure = null, 
				CtInjTubingReelDrivePress = null, 
				CtChainTension = null, 
			};
		}
		public static ContinuousCirculationDevice ConstructContinuousCirculationDevice()
		{
			return new ContinuousCirculationDevice
			{
				Name = "Default Name",
				Description = "Default Description",
				Manufacturer = "Default Manufacturer",
				Model = "Default Model",
				ProductCode = "Default ProductCode",
				SerialNumber = "Default SerialNumber",
				CcdControlClass = (ControlClass)0,
				WorkingPumpPressure = null, 
				MaxLimitDesignPressure = null, 
				MaxLimitOperatingPressure = null, 
				MaxLimitFlowrate = null, 
				MaxLimitBackflow = null, 
				MaxLimitFluidTemperature = null, 
				MinLimitFluidTemperature = null, 
				MaxLimitMudWeight = null, 
				MaxLimitRotationRate = null, 
			};
		}
		public static CrownBlock ConstructCrownBlock()
		{
			return new CrownBlock
			{
				Name = "Default Name",
				Description = "Default Description",
				Manufacturer = "Default Manufacturer",
				Model = "Default Model",
				ProductCode = "Default ProductCode",
				SerialNumber = "Default SerialNumber",
				SheaveDiameter = null, 
				GrooveDiameter = null, 
				NumberOfSheaves = null, 
				MaxLimitDesignLoad = null, 
				MaxLimitOperatingLoad = null, 
				MaxLimitCompensatorStroke = null, 
				Hookload = null, 
			};
		}
		public static CuttingsDryer ConstructCuttingsDryer()
		{
			return new CuttingsDryer
			{
				Name = "Default Name",
				Description = "Default Description",
			};
		}
		public static CuttingsTransportSystem ConstructCuttingsTransportSystem()
		{
			return new CuttingsTransportSystem
			{
				Name = "Default Name",
				Description = "Default Description",
			};
		}
		public static Degasser ConstructDegasser()
		{
			return new Degasser
			{
				Name = "Default Name",
				Description = "Default Description",
			};
		}
		public static Derrick ConstructDerrick()
		{
			return new Derrick
			{
				Name = "Default Name",
				Description = "Default Description",
				Manufacturer = "Default Manufacturer",
				Model = "Default Model",
				ProductCode = "Default ProductCode",
				SerialNumber = "Default SerialNumber",
				DerrickClass = (DerrickClass)0,
				Height = null, 
				MaxLimitJointsPerStand = null, 
				MaxLimitDesignLoad = null, 
				MaxLimitOperatingLoad = null, 
				MaxLimitWindSpeed = null, 
			};
		}
		public static Desander ConstructDesander()
		{
			return new Desander
			{
				Name = "Default Name",
				Description = "Default Description",
			};
		}
		public static Desilter ConstructDesilter()
		{
			return new Desilter
			{
				Name = "Default Name",
				Description = "Default Description",
			};
		}
		public static Drawworks ConstructDrawworks()
		{
			return new Drawworks
			{
				Name = "Default Name",
				Description = "Default Description",
				Manufacturer = "Default Manufacturer",
				Model = "Default Model",
				ProductCode = "Default ProductCode",
				SerialNumber = "Default SerialNumber",
				DrawworksClass = (DrawworksClass)0,
				MaxLimitDesignLoad = null, 
				MaxLimitOperatingLoad = null, 
				MaxLimitContinuousDrumPower = null, 
				MaxLimitContinuousDrumTorque = null, 
			};
		}
		public static DrillLine ConstructDrillLine()
		{
			return new DrillLine
			{
				Name = "Default Name",
				Description = "Default Description",
				Manufacturer = "Default Manufacturer",
				Model = "Default Model",
				ProductCode = "Default ProductCode",
				SerialNumber = "Default SerialNumber",
				Number = null, 
				Diameter = null, 
				LinearWeight = null, 
				MaxLimitDesignBreakingLoad = null, 
				MaxLimitOperatingBreakingLoad = null, 
				Hookload = null, 
			};
		}
		public static DrillingChokeManifold ConstructDrillingChokeManifold()
		{
			return new DrillingChokeManifold
			{
				Name = "Default Name",
				Description = "Default Description",
				Manufacturer = "Default Manufacturer",
				Model = "Default Model",
				ProductCode = "Default ProductCode",
				SerialNumber = "Default SerialNumber",
				ManifoldType = (ManifoldClass)0,
				TrimSize = null, 
				FlowMeter = "Default FlowMeter",
				FlowMeterSize = null, 
				FlowMeterPressureRating = null, 
				JunkBasket = null, 
				ChokeCount = "Default ChokeCount",
				FlowMeterCount = "Default FlowMeterCount",
				PressureSensorVotingNumber = "Default PressureSensorVotingNumber",
				ChokeNumber = (ChokeNumber)0,
				ChokeFunction = (ChokeFunction)0,
				ChokeCvCurves = new List<ChokeCvCurvePoint>
					{
						ConstructChokeCvCurvePoint(),
					},
				MaxLimitDesignPressure = null, 
				MaxLimitOperatingPressure = null, 
				MaxLimitOperatingTemperature = null, 
				MinLimitOperatingTemperature = null, 
				MaxLimitOpeningSpeed = null, 
				MaxLimitBackPressure = null, 
				MinLimitFlowrate = null, 
				MaxLimitFlowrate = null, 
				PressureBeforeChoke = null, 
				PressureAfterChoke = null, 
				CvValue = null, 
				CloggingOccuring = null, 
				TemperatureBeforeChoke = null, 
				TemperatureAfterChoke = null, 
				FlowThroughChoke = null, 
				MudDensityOut = null, 
				MudDensityIn = null, 
				ReliefValvePressure = null, 
				PressureBeforeFlowMeter = null, 
				PressureAfterFlowMeter = null, 
				InletPressure = null, 
				OutletPressure = null, 
				VotingSensorsFailed = null, 
			};
		}
		public static DrillingFluidTypeDescriptor ConstructDrillingFluidTypeDescriptor()
		{
			return new DrillingFluidTypeDescriptor
			{
				Name = "Default Name",
				Description = "Default Description",
				DrillingFluidClass = (DrillingFluidClass)0,
				DrillingFluidType = (DrillingFluidType)0,
			};
		}
		public static DrillingMarineRiser ConstructDrillingMarineRiser()
		{
			return new DrillingMarineRiser
			{
				Name = "Default Name",
				Description = "Default Description",
				Manufacturer = "Default Manufacturer",
				Model = "Default Model",
				ProductCode = "Default ProductCode",
				SerialNumber = "Default SerialNumber",
				RiserClass = (RiserClass)0,
				JointWeight = null, 
				RiserInsideDiameter = null, 
				RiserOuterDiameter = null, 
				RiserJointLength = null, 
				RiserTotalLength = null, 
				MaxLimitTensionLoad = null, 
				MaxLimitOpTensionLoad = null, 
				MaxLimitDesignKillPressure = null, 
				MaxLimitOpKillPressure = null, 
				MaxLimitDesignBoosterPressure = null, 
				MaxLimitBoosterPressure = null, 
				MaxLimitOpTemperature = null, 
				MinLimitOpTemperature = null, 
				MaxLimitAngleRiser = null, 
			};
		}
		public static DrillstringHeaveCompensator ConstructDrillstringHeaveCompensator()
		{
			return new DrillstringHeaveCompensator
			{
				Name = "Default Name",
				Description = "Default Description",
				Manufacturer = "Default Manufacturer",
				Model = "Default Model",
				ProductCode = "Default ProductCode",
				SerialNumber = "Default SerialNumber",
				HeaveCompClass = (HeaveCompensatorClass)0,
				CompensatorCapacity = null, 
				CompensatorStatus = (CompensatorStatus)0,
				MaxLimitCompensatorStroke = null, 
			};
		}
		public static DriveMode ConstructDriveMode()
		{
			return new DriveMode
			{
				Name = "Default Name",
				Description = "Default Description",
				Manufacturer = "Default Manufacturer",
				Model = "Default Model",
				ProductCode = "Default ProductCode",
				SerialNumber = "Default SerialNumber",
				DriveModeClass = (DriveModeClass)0,
			};
		}
		public static FloatValve ConstructFloatValve()
		{
			return new FloatValve
			{
				Name = "Default Name",
				Description = "Default Description",
				Manufacturer = "Default Manufacturer",
				Model = "Default Model",
				ProductCode = "Default ProductCode",
				SerialNumber = "Default SerialNumber",
				FloatValveClass = (FloatValveClass)0,
				Diameter = null, 
				Length = null, 
				MaxLimitDesignPressure = null, 
				MaxLimitOperatingPressure = null, 
			};
		}
		public static FlowRoutingManifold ConstructFlowRoutingManifold()
		{
			return new FlowRoutingManifold
			{
				Name = "Default Name",
				Description = "Default Description",
				Manufacturer = "Default Manufacturer",
				Model = "Default Model",
				ProductCode = "Default ProductCode",
				SerialNumber = "Default SerialNumber",
				ManifoldType = (ManifoldClass)0,
				FlangeSize = null, 
				ReliefLineDiameter = null, 
				EqualizationLineDiameter = null, 
				PressureReliefValveTrim = null, 
				ManifoldFlowPath = (ManifoldFlowPath)0,
				ManifoldFlowcurves = new List<RoutingManifoldCurvePoint>
					{
						ConstructRoutingManifoldCurvePoint(),
					},
				MaxLimitDesignPressure = null, 
				MaxLimitOperatingPressure = null, 
				MaxLimitOperatingTemperature = null, 
				MinLimitOperatingTemperature = null, 
				MaxLimitFlowrate = null, 
				InletPressure = null, 
				OutletPressure = null, 
				ReliefValvePressure = null, 
				CloggingOccuring = null, 
				TemperatureInlet = null, 
				TemperatureOutlet = null, 
			};
		}
		public static FlowSensor ConstructFlowSensor()
		{
			return new FlowSensor
			{
				Name = "Default Name",
				Description = "Default Description",
				Manufacturer = "Default Manufacturer",
				Model = "Default Model",
				ProductCode = "Default ProductCode",
				SerialNumber = "Default SerialNumber",
				FlowTransducer = (FlowSensorType)0,
				FlowOutOfBorehole = null, 
				MudFlowrateOut = null, 
				MudFlowrateIn = null, 
			};
		}
		public static Generator ConstructGenerator()
		{
			return new Generator
			{
				Name = "Default Name",
				Description = "Default Description",
				Manufacturer = "Default Manufacturer",
				Model = "Default Model",
				ProductCode = "Default ProductCode",
				SerialNumber = "Default SerialNumber",
				GeneratorClass = (GeneratorClass)0,
				Speed = null, 
				Power = null, 
				Voltage = null, 
				PowerFactor = null, 
				SpeedMode = (SpeedMode)0,
				EngineModel = (EngineModelType)0,
				PowerplantGeneratorNumber = null, 
				PowerplantTotalPower = null, 
				StartupTimeCold = null, 
				StartupTimeWarm = null, 
				CoolingMedium = (GeneratorCooling)0,
				Phases = (GeneratorPhases)0,
				MaxLimitPower = null, 
				MaxLimitPowerIncrease = null, 
				MaxLimitSpeedIncrease = null, 
				MaxLimitSpeed = null, 
				MaxLimitVoltage = null, 
				MinLimitVoltage = null, 
				MaxLimitFrequency = null, 
				MinLimitFrequency = null, 
				EnginePower = null, 
				GeneratorPower = null, 
				EngineFuelConsumption = null, 
				EngineSpecificFuelConsumption = null, 
				RunningHours = null, 
				EngineSpeed = null, 
				GeneratorVoltage = null, 
				GridVoltage = null, 
				GridFrequency = null, 
				GeneratorFrequency = null, 
				EngineTemperature = null, 
			};
		}
		public static HoistingSystem ConstructHoistingSystem()
		{
			return new HoistingSystem
			{
				Name = "Default Name",
				Description = "Default Description",
				HoistingSystemType = (HoistingSystemType)0,
				Drawworks = ConstructDrawworks(),
				CrownBlock = ConstructCrownBlock(),
				TravellingBlock = ConstructTravellingBlock(),
				DrillLine = ConstructDrillLine(),
			};
		}
		public static IronRoughneck ConstructIronRoughneck()
		{
			return new IronRoughneck
			{
				Name = "Default Name",
				Description = "Default Description",
			};
		}
		public static Kelly ConstructKelly()
		{
			return new Kelly
			{
				Name = "Default Name",
				Description = "Default Description",
				Manufacturer = "Default Manufacturer",
				Model = "Default Model",
				ProductCode = "Default ProductCode",
				SerialNumber = "Default SerialNumber",
				KellyClass = (KellyClass)0,
				KellyJointLength = null, 
				MaxLimitDesignRotationSpeed = null, 
				MaxLimitDesignTorque = null, 
				MaxLimitIbopPressure = null, 
				MaxLimitRotationSpeed = null, 
				MaxLimitTorque = null, 
				SurfaceRotation = null, 
				SurfaceTorque = null, 
				KellyHeight = null, 
			};
		}
		public static MarineMpdEquipment ConstructMarineMpdEquipment()
		{
			return new MarineMpdEquipment
			{
				Name = "Default Name",
				Description = "Default Description",
				Manufacturer = "Default Manufacturer",
				Model = "Default Model",
				ProductCode = "Default ProductCode",
				SerialNumber = "Default SerialNumber",
				MarineMpdClass = (MarineMpdClass)0,
				Length = null, 
				Weight = null, 
				ThroughBoreDiameter = null, 
				ControlMeans = (ControllerType)0,
				ContainsFlowSpool = null, 
				ContainsNonRotatingDevice = null, 
				ContainsDrillstringIsolation = null, 
				MaxLimitDesignPressure = null, 
				MaxLimitDynamicPressure = null, 
				MaxLimitRotatingSpeed = null, 
			};
		}
		public static MeasurementAfm ConstructMeasurementAfm()
		{
			return new MeasurementAfm
			{
				Name = "Default Name",
				Description = "Default Description",
				Manufacturer = "Default Manufacturer",
				Model = "Default Model",
				ProductCode = "Default ProductCode",
				SerialNumber = "Default SerialNumber",
				UpdateRate = null, 
				Active = null, 
				AfmMudDensity = null, 
				AfmMudTemperature = null, 
				AfmPv = null, 
				AfmYp = null, 
				AfmRheometerMeasurements = new List<RheometerAfmMeasurement>
					{
						ConstructRheometerAfmMeasurement(),
					},
				RtViscConsistencyIndex = null, 
				RtViscFlowBehaviorIndex = null, 
			};
		}
		public static MpdControlDevice ConstructMpdControlDevice()
		{
			return new MpdControlDevice
			{
				Name = "Default Name",
				Description = "Default Description",
				Manufacturer = "Default Manufacturer",
				Model = "Default Model",
				ProductCode = "Default ProductCode",
				SerialNumber = "Default SerialNumber",
				MpdControlDeviceClass = (MpdControlDeviceClass)0,
				NominalSize = null, 
				ThroughBoreDiameter = null, 
				SealingElementMaterial = "Default SealingElementMaterial",
				ControlDeviceHeight = null, 
				MaxLimitStaticPressure = null, 
				MaxLimitDynamicPressure = null, 
				MaxLimitRotatingSpeed = null, 
				MaxLimitActivationPressure = null, 
			};
		}
		public static MpdController ConstructMpdController()
		{
			return new MpdController
			{
				Name = "Default Name",
				Description = "Default Description",
				Manufacturer = "Default Manufacturer",
				Model = "Default Model",
				ProductCode = "Default ProductCode",
				SerialNumber = "Default SerialNumber",
				MpdGradientMode = (MpdGradientMode)0,
				PrimaryChokeTrim = null, 
				SecondaryChokeTrim = null, 
				MaxLimitPressure = null, 
				MinLimitMudPumpFlowrate = null, 
				ManipulatedMpdChoke = null, 
				ManipulatedLiftPumpRate = null, 
				ControlledDownholePressure = null, 
				BackpressureFlowrate = null, 
				AnnulusRefillFlowrate = null, 
			};
		}
		public static MudGasSeparator ConstructMudGasSeparator()
		{
			return new MudGasSeparator
			{
				Name = "Default Name",
				Description = "Default Description",
			};
		}
		public static MudPump ConstructMudPump()
		{
			return new MudPump
			{
				Name = "Default Name",
				Description = "Default Description",
				Manufacturer = "Default Manufacturer",
				Model = "Default Model",
				ProductCode = "Default ProductCode",
				SerialNumber = "Default SerialNumber",
				Type = (MudPumpType)0,
				PumpClass = (PumpClass)0,
				PumpAction = null, 
				PumpEfficiency = null, 
				PumpDisplacement = null, 
				LinerId = null, 
				Stroke = null, 
				PulsationDamperPressure = null, 
				PulsationDamperVolume = null, 
				MaxLimitDesignPressure = null, 
				MaxLimitOperatingPressure = null, 
				MaxLimitOperatingPower = null, 
				MaxLimitOperatingFlowRate = null, 
				MaxLimitOperatingSpeed = null, 
				MudPumpStrokeRate = null, 
				MudPumpFlowRate = null, 
			};
		}
		public static MudTank ConstructMudTank()
		{
			return new MudTank
			{
				Name = "Default Name",
				Description = "Default Description",
				Manufacturer = "Default Manufacturer",
				Model = "Default Model",
				ProductCode = "Default ProductCode",
				SerialNumber = "Default SerialNumber",
				TankClass = (TankClass)0,
				TankFluidType = (TankFluidType)0,
				MaxLimitOperatingVolume = null, 
			};
		}
		public static MultiPhaseSeparator ConstructMultiPhaseSeparator()
		{
			return new MultiPhaseSeparator
			{
				Name = "Default Name",
				Description = "Default Description",
				Manufacturer = "Default Manufacturer",
				Model = "Default Model",
				ProductCode = "Default ProductCode",
				SerialNumber = "Default SerialNumber",
				SeparatorClass = (SeparatorPhaseClass)0,
				MaximumOperatingPressure = null, 
				MaximumOperatingFlowrate = null, 
				SeparationEfficiency = null, 
				SeparatorMedium = (SeparatorMedium)0,
				MaxLimitDesignPressure = null, 
				MaxLimitOperatingPressure = null, 
				MaxLimitFlowrate = null, 
				MaxLimitOperatingTemperature = null, 
				MinLimitOperatingTemperature = null, 
				PressureSeparator = null, 
				TemperatureSeparator = null, 
			};
		}
		public static PipeDeck ConstructPipeDeck()
		{
			return new PipeDeck
			{
				Name = "Default Name",
				Description = "Default Description",
			};
		}
		public static PipeRack ConstructPipeRack()
		{
			return new PipeRack
			{
				Name = "Default Name",
				Description = "Default Description",
			};
		}
		public static ReturnFlowLine ConstructReturnFlowLine()
		{
			return new ReturnFlowLine
			{
				Name = "Default Name",
				Description = "Default Description",
			};
		}
		public static RheometerAfmMeasurement ConstructRheometerAfmMeasurement()
		{
			return new RheometerAfmMeasurement
			{
				AfmViscShearRate = null, 
				AfmViscShearStress = null, 
			};
		}
		public static Rig ConstructRig()
		{
			return new Rig
			{
				MetaInfo = ConstructMetaInfo(),
				Name = "Default Name",
				Description = "Default Description",
				CreationDate = DateTimeOffset.UtcNow,
				LastModificationDate = DateTimeOffset.UtcNow,
				MudPumpList = new List<MudPump>
					{
						ConstructMudPump(),
					},
				CementPumpList = new List<CementPump>
					{
						ConstructCementPump(),
					},
				CementUnit = ConstructCementUnit(),
				DriveMode = ConstructDriveMode(),
				MainRigMast = ConstructRigMast(),
				AuxiliaryRigMast = ConstructRigMast(),
				MudTankList = new List<MudTank>
					{
						ConstructMudTank(),
					},
				GeneratorList = new List<Generator>
					{
						ConstructGenerator(),
					},
				ShaleShakerList = new List<ShaleShaker>
					{
						ConstructShaleShaker(),
					},
				AuxSolidsControl = ConstructAuxSolidsControl(),
				DrillingFluidType = ConstructDrillingFluidTypeDescriptor(),
				FlowSensor = ConstructFlowSensor(),
				MeasurementAfm = ConstructMeasurementAfm(),
				ReturnFlowLine = ConstructReturnFlowLine(),
				MudGasSeparatorList = new List<MudGasSeparator>
					{
						ConstructMudGasSeparator(),
					},
				DesanderList = new List<Desander>
					{
						ConstructDesander(),
					},
				DesilterList = new List<Desilter>
					{
						ConstructDesilter(),
					},
				CentrifugeList = new List<Centrifuge>
					{
						ConstructCentrifuge(),
					},
				DegasserList = new List<Degasser>
					{
						ConstructDegasser(),
					},
				CuttingsTransportSystem = ConstructCuttingsTransportSystem(),
				CuttingsDryerList = new List<CuttingsDryer>
					{
						ConstructCuttingsDryer(),
					},
				PipeDeck = ConstructPipeDeck(),
				Accumulator = ConstructAccumulator(),
				BopStack = ConstructBopStack(),
				FloatValve = ConstructFloatValve(),
				AutoDriller = ConstructAutoDriller(),
				MpdController = ConstructMpdController(),
				MpdControlDevice = ConstructMpdControlDevice(),
				ContinuousCirculationDevice = ConstructContinuousCirculationDevice(),
				DrillingChokeManifold = ConstructDrillingChokeManifold(),
				SurfaceMpdEquipment = ConstructSurfaceMpdEquipment(),
				MarineMpdEquipment = ConstructMarineMpdEquipment(),
				MultiPhaseSeparator = ConstructMultiPhaseSeparator(),
				FlowRoutingManifold = ConstructFlowRoutingManifold(),
				DrillstringHeaveCompensator = ConstructDrillstringHeaveCompensator(),
				DrillingMarineRiser = ConstructDrillingMarineRiser(),
				RiserHeaveCompensator = ConstructRiserHeaveCompensator(),
				DrillFloorElevation = null, 
				IsFixedPlatform = false, 
				ClusterID = null, 
			};
		}
		public static RigChoke ConstructRigChoke()
		{
			return new RigChoke
			{
				Name = "Default Name",
				Description = "Default Description",
			};
		}
		public static RigMast ConstructRigMast()
		{
			return new RigMast
			{
				Name = "Default Name",
				Description = "Default Description",
				HoistingSystem = ConstructHoistingSystem(),
				CatWalk = ConstructCatWalk(),
				PipeRack = ConstructPipeRack(),
				CasingDriveSystem = ConstructCasingDriveSystem(),
				CoilDriveSystem = ConstructCoilDriveSystem(),
				Derrick = ConstructDerrick(),
				TorqueTurnSub = ConstructTorqueTurnSub(),
				RotaryTable = ConstructRotaryTable(),
				TopDrive = ConstructTopDrive(),
				Kelly = ConstructKelly(),
				IronRoughneck = ConstructIronRoughneck(),
				CasingTongs = ConstructCasingTongs(),
				CasingRunningTool = ConstructCasingRunningTool(),
				StandPipe = ConstructStandPipe(),
				StandPipeManifold = ConstructStandPipeManifold(),
				RotaryHose = ConstructRotaryHose(),
				ChokeManifold = ConstructChokeManifold(),
				RigChokeList = new List<RigChoke>
					{
						ConstructRigChoke(),
					},
				Slips = ConstructSlips(),
			};
		}
		public static RiserHeaveCompensator ConstructRiserHeaveCompensator()
		{
			return new RiserHeaveCompensator
			{
				Name = "Default Name",
				Description = "Default Description",
				Manufacturer = "Default Manufacturer",
				Model = "Default Model",
				ProductCode = "Default ProductCode",
				SerialNumber = "Default SerialNumber",
				RiserCompensatorClass = (RiserCompensatorClass)0,
				CompensatorCapacity = null, 
				CompensatorStatus = (CompensatorStatus)0,
				MaxLimitCompensatorStroke = null, 
			};
		}
		public static RotaryHose ConstructRotaryHose()
		{
			return new RotaryHose
			{
				Name = "Default Name",
				Description = "Default Description",
			};
		}
		public static RotaryTable ConstructRotaryTable()
		{
			return new RotaryTable
			{
				Name = "Default Name",
				Description = "Default Description",
				Manufacturer = "Default Manufacturer",
				Model = "Default Model",
				ProductCode = "Default ProductCode",
				SerialNumber = "Default SerialNumber",
				RotaryTableType = (RotaryTableType)0,
				TableOpeningDiameter = null, 
				BushingType = (RotaryTableBushingType)0,
				BushingSize = null, 
				Height = null, 
				Mass = null, 
				MaxLimitOperatingSpeed = null, 
				MaxLimitDesignSpeed = null, 
				MaxLimitOperatingTorque = null, 
				MaxLimitDesignTorque = null, 
				MaxLimitOperatingStringWeight = null, 
				MaxLimitDesignStringWeight = null, 
				MaxLimitPower = null, 
				MaxLimitTemperature = null, 
			};
		}
		public static RoutingManifoldCurvePoint ConstructRoutingManifoldCurvePoint()
		{
			return new RoutingManifoldCurvePoint
			{
				Pressure = null, 
				Flow = null, 
			};
		}
		public static ShakerScreenDefinition ConstructShakerScreenDefinition()
		{
			return new ShakerScreenDefinition
			{
				ScreenDeck = null, 
				MeshSize = "Default MeshSize",
			};
		}
		public static ShaleShaker ConstructShaleShaker()
		{
			return new ShaleShaker
			{
				Name = "Default Name",
				Description = "Default Description",
				Manufacturer = "Default Manufacturer",
				Model = "Default Model",
				ProductCode = "Default ProductCode",
				SerialNumber = "Default SerialNumber",
				ShakerClass = (ShakerClass)0,
				ActiveShakers = "Default ActiveShakers",
				ShakerScreens = new List<ShakerScreenDefinition>
					{
						ConstructShakerScreenDefinition(),
					},
				MaxLimitOperatingCapacity = null, 
			};
		}
		public static Slips ConstructSlips()
		{
			return new Slips
			{
				Name = "Default Name",
				Description = "Default Description",
			};
		}
		public static StandPipe ConstructStandPipe()
		{
			return new StandPipe
			{
				Name = "Default Name",
				Description = "Default Description",
				PressureMeasurementElevation = null, 
				MudHoseHangingPointElevation = null, 
			};
		}
		public static StandPipeManifold ConstructStandPipeManifold()
		{
			return new StandPipeManifold
			{
				Name = "Default Name",
				Description = "Default Description",
				Manufacturer = "Default Manufacturer",
				Model = "Default Model",
				ProductCode = "Default ProductCode",
				SerialNumber = "Default SerialNumber",
				PipeDiameter = null, 
				StandpipeSpecLevel = (StandpipeSpecLevel)0,
				MaxLimitDesignPressure = null, 
				MaxLimitOperatingPressure = null, 
				MaxLimitOperatingTemperature = null, 
				MinLimitOperatingTemperature = null, 
			};
		}
		public static SurfaceMpdEquipment ConstructSurfaceMpdEquipment()
		{
			return new SurfaceMpdEquipment
			{
				Name = "Default Name",
				Description = "Default Description",
				Manufacturer = "Default Manufacturer",
				Model = "Default Model",
				ProductCode = "Default ProductCode",
				SerialNumber = "Default SerialNumber",
				SurfaceMpdClass = (SurfaceMpdClass)0,
				MinimumBoreholeSize = null, 
				MaximumBoreholeSize = null, 
				PressureAccuracy = null, 
				MaxLimitDesignPressure = null, 
				MaxLimitOperatingPressure = null, 
				MinLimitOperatingPressure = null, 
				MaxLimitFlowrate = null, 
				MaxLimitMudWeight = null, 
				MaxLimitPressure = null, 
				MinLimitMudPumpFlowrate = null, 
				StrokeRate = null, 
				FlowRate = null, 
				PressureAtDischarge = null, 
				Power = null, 
				PressureAtInlet = null, 
			};
		}
		public static TopDrive ConstructTopDrive()
		{
			return new TopDrive
			{
				Name = "Default Name",
				Description = "Default Description",
				Manufacturer = "Default Manufacturer",
				Model = "Default Model",
				ProductCode = "Default ProductCode",
				SerialNumber = "Default SerialNumber",
				TopDriveClass = (TopDriveClass)0,
				TopDriveControllerType = (TopDriveControllerType)0,
				Orientable = null, 
				Weight = null, 
				MaxLimitIbopPressure = null, 
				MaxLimitRotationSpeed = null, 
				MaxLimitDesignLoad = null, 
				MaxLimitDesignTorque = null, 
				MaxLimitOperatingLoad = null, 
				MaxLimitOperatingTorque = null, 
				MaxLimitMakeupTorque = null, 
				MaxLimitBreakoutTorque = null, 
				TopDriveHeight = null, 
				ProportionalGain = null, 
				IntegralGain = null, 
				TuningFrequency = null, 
				VFDFilterTimeConstant = null, 
				EncoderTimeConstant = null, 
				AccelerationFilterTimeConstant = null, 
				TorqueHighPassFilterTimeConstant = null, 
				TorqueLowPassFilterTimeConstant = null, 
				TuningFactor = null, 
				InertiaCorrectionFactor = null, 
			};
		}
		public static TorqueTurnSub ConstructTorqueTurnSub()
		{
			return new TorqueTurnSub
			{
				Name = "Default Name",
				Description = "Default Description",
				Manufacturer = "Default Manufacturer",
				Model = "Default Model",
				ProductCode = "Default ProductCode",
				SerialNumber = "Default SerialNumber",
				Length = null, 
				OutsideDiameter = null, 
				InsideDiameter = null, 
				Weight = null, 
				BatteryLife = null, 
				MaxLimitDesignLoad = null, 
				MaxLimitDesignTorque = null, 
				MaxLimitDesignPressure = null, 
				MaxLimitLoad = null, 
				MaxLimitTorque = null, 
				MaxLimitPressure = null, 
				MaxLimitTemperature = null, 
				MinLimitTemperature = null, 
				SurfaceTorque = null, 
				Hookload = null, 
				SurfaceTurnCount = null, 
				SurfaceAcceleration = null, 
				SurfaceRotationRate = null, 
				SurfaceBorePressure = null, 
				SurfaceAxialVibration = null, 
				SurfaceTorsionalVibration = null, 
				SurfaceLateralVibration = null, 
			};
		}
		public static TravellingBlock ConstructTravellingBlock()
		{
			return new TravellingBlock
			{
				Name = "Default Name",
				Description = "Default Description",
				Manufacturer = "Default Manufacturer",
				Model = "Default Model",
				ProductCode = "Default ProductCode",
				SerialNumber = "Default SerialNumber",
				Weight = null, 
				NumberOfSheaves = null, 
				GrooveDiameter = null, 
				MaxLimitBlockTravel = null, 
				MaxLimitDesignLoad = null, 
				MaxLimitOperatingLoad = null, 
				HookVelocity = null, 
				HookPosition = null, 
			};
		}
		public static UsageStatisticsRig ConstructUsageStatisticsRig()
		{
			return new UsageStatisticsRig
			{
				LastSaved = DateTimeOffset.UtcNow,
				BackUpInterval = "Default BackUpInterval",
				GetAllRigIdPerDay = ConstructHistory(),
				GetAllRigMetaInfoPerDay = ConstructHistory(),
				GetRigByIdPerDay = ConstructHistory(),
				GetAllRigLightPerDay = ConstructHistory(),
				GetAllRigPerDay = ConstructHistory(),
				PostRigPerDay = ConstructHistory(),
				PutRigByIdPerDay = ConstructHistory(),
				DeleteRigByIdPerDay = ConstructHistory(),
			};
		}
		public static VerticalDatum ConstructVerticalDatum()
		{
			return new VerticalDatum
			{
				MetaInfo = ConstructMetaInfo(),
				Name = "Default Name",
				Description = "Default Description",
				CreationDate = DateTimeOffset.UtcNow,
				LastModificationDate = DateTimeOffset.UtcNow,
				DatumSet = new List<VerticalDatumSet>
					{
						ConstructVerticalDatumSet(),
					},
				ConversionFrom = (VerticalDatumConversion)0,
				Type = (VerticalDatumType)0,
			};
		}
		public static VerticalDatumOrder ConstructVerticalDatumOrder()
		{
			return new VerticalDatumOrder
			{
				MetaInfo = ConstructMetaInfo(),
				Name = "Default Name",
				Description = "Default Description",
				CreationDate = DateTimeOffset.UtcNow,
				LastModificationDate = DateTimeOffset.UtcNow,
				VerticalDatum = ConstructVerticalDatum(),
			};
		}
		public static VerticalDatumSet ConstructVerticalDatumSet()
		{
			return new VerticalDatumSet
			{
				Latitude = 0.0, 
				Longitude = 0.0, 
				VerticalDatumWGS64 = null, 
				GenericVerticalDatum = 0.0, 
			};
		}
		public static UsageStatisticsWell ConstructUsageStatisticsWell()
		{
			return new UsageStatisticsWell
			{
				LastSaved = DateTimeOffset.UtcNow,
				BackUpInterval = "Default BackUpInterval",
				GetAllWellIdPerDay = ConstructHistory(),
				GetAllWellMetaInfoPerDay = ConstructHistory(),
				GetWellByIdPerDay = ConstructHistory(),
				GetAllWellPerDay = ConstructHistory(),
				PostWellPerDay = ConstructHistory(),
				PutWellByIdPerDay = ConstructHistory(),
				DeleteWellByIdPerDay = ConstructHistory(),
			};
		}
		public static Well ConstructWell()
		{
			return new Well
			{
				MetaInfo = ConstructMetaInfo(),
				Name = "Default Name",
				Description = "Default Description",
				CreationDate = DateTimeOffset.UtcNow,
				LastModificationDate = DateTimeOffset.UtcNow,
				SlotID = null, 
				ClusterID = null, 
				IsSingleWell = false, 
			};
		}
		public static UsageStatisticsWellBore ConstructUsageStatisticsWellBore()
		{
			return new UsageStatisticsWellBore
			{
				LastSaved = DateTimeOffset.UtcNow,
				BackUpInterval = "Default BackUpInterval",
				GetAllWellBoreIdPerDay = ConstructHistory(),
				GetAllWellBoreMetaInfoPerDay = ConstructHistory(),
				GetWellBoreByIdPerDay = ConstructHistory(),
				GetAllWellBorePerDay = ConstructHistory(),
				PostWellBorePerDay = ConstructHistory(),
				PutWellBoreByIdPerDay = ConstructHistory(),
				DeleteWellBoreByIdPerDay = ConstructHistory(),
			};
		}
		public static WellBore ConstructWellBore()
		{
			return new WellBore
			{
				MetaInfo = ConstructMetaInfo(),
				Name = "Default Name",
				Description = "Default Description",
				CreationDate = DateTimeOffset.UtcNow,
				LastModificationDate = DateTimeOffset.UtcNow,
				WellID = null, 
				RigID = null, 
				IsSidetrack = false, 
				ParentWellBoreID = null, 
				TieInPointAlongHoleDepth = ConstructGaussianDrillingProperty(),
				SidetrackType = (SidetrackType)0,
			};
		}
		public static BoreHoleSize ConstructBoreHoleSize()
		{
			return new BoreHoleSize
			{
				HoleSize = ConstructGaussianDrillingProperty(),
				Length = ConstructGaussianDrillingProperty(),
			};
		}
		public static CasingSection ConstructCasingSection()
		{
			return new CasingSection
			{
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
		public static ElementConnectivity ConstructElementConnectivity()
		{
			return new ElementConnectivity
			{
				UpstreamElement = ConstructSideElement(),
				DownstreamElement = ConstructSideElement(),
			};
		}
		public static OpenHoleSection ConstructOpenHoleSection()
		{
			return new OpenHoleSection
			{
				HoleSizes = new List<BoreHoleSize>
					{
						ConstructBoreHoleSize(),
					},
			};
		}
		public static SideConnector ConstructSideConnector()
		{
			return new SideConnector
			{
				Position = ConstructGaussianDrillingProperty(),
				VerticalDepth = ConstructGaussianDrillingProperty(),
				FirstSideElement = ConstructSideElement(),
				ElementConnectivities = new List<ElementConnectivity>
					{
						ConstructElementConnectivity(),
					},
			};
		}
		public static SideElement ConstructSideElement()
		{
			return new SideElement
			{
				Name = "Default Name",
				Type = (SideElementType)0,
				Length = ConstructGaussianDrillingProperty(),
				TopVerticalDepth = ConstructGaussianDrillingProperty(),
				OD = ConstructGaussianDrillingProperty(),
				ID = ConstructGaussianDrillingProperty(),
			};
		}
		public static SurfaceSection ConstructSurfaceSection()
		{
			return new SurfaceSection
			{
				Type = (SurfaceSectionType)0,
				SectionLength = ConstructGaussianDrillingProperty(),
				BodyOD = ConstructGaussianDrillingProperty(),
				BodyID = ConstructGaussianDrillingProperty(),
				ConnectionType = "Default ConnectionType",
				Grade = "Default Grade",
				MaterialDensity = ConstructGaussianDrillingProperty(),
				YoungModulus = ConstructGaussianDrillingProperty(),
				LinearWeight = ConstructGaussianDrillingProperty(),
				TensileStrength = ConstructGaussianDrillingProperty(),
				BurstPressure = ConstructGaussianDrillingProperty(),
				CollapsePressure = ConstructGaussianDrillingProperty(),
				YieldStress = ConstructGaussianDrillingProperty(),
				MakeUpTorqueRecommended = ConstructScalarDrillingProperty(),
				SideConnectors = new List<SideConnector>
					{
						ConstructSideConnector(),
					},
			};
		}
		public static UsageStatisticsWellBoreArchitecture ConstructUsageStatisticsWellBoreArchitecture()
		{
			return new UsageStatisticsWellBoreArchitecture
			{
				LastSaved = DateTimeOffset.UtcNow,
				BackUpInterval = "Default BackUpInterval",
				GetAllWellBoreArchitectureIdPerDay = ConstructHistory(),
				GetAllWellBoreArchitectureMetaInfoPerDay = ConstructHistory(),
				GetWellBoreArchitectureByIdPerDay = ConstructHistory(),
				GetAllWellBoreArchitectureLightPerDay = ConstructHistory(),
				GetAllWellBoreArchitecturePerDay = ConstructHistory(),
				PostWellBoreArchitecturePerDay = ConstructHistory(),
				PutWellBoreArchitectureByIdPerDay = ConstructHistory(),
				DeleteWellBoreArchitectureByIdPerDay = ConstructHistory(),
			};
		}
		public static WellBoreArchitecture ConstructWellBoreArchitecture()
		{
			return new WellBoreArchitecture
			{
				MetaInfo = ConstructMetaInfo(),
				Name = "Default Name",
				Description = "Default Description",
				CreationDate = DateTimeOffset.UtcNow,
				LastModificationDate = DateTimeOffset.UtcNow,
				WellBoreArchitectureIdentityAssignments = new List<WellBoreArchitectureIdentityAssignment>
					{
						ConstructWellBoreArchitectureIdentityAssignment(),
					},
				WellBoreArchitectureFeatureAssignments = new List<WellBoreArchitectureFeatureAssignment>
					{
						ConstructWellBoreArchitectureFeatureAssignment(),
					},
				WellBoreID = null, 
				WellHead = ConstructWellHead(),
				FluidsAboveGroundLevel = new List<WellBoreArchitectureFluid>
					{
						ConstructWellBoreArchitectureFluid(),
					},
				SurfaceSections = new List<SurfaceSection>
					{
						ConstructSurfaceSection(),
					},
				CasingSections = new List<CasingSection>
					{
						ConstructCasingSection(),
					},
			};
		}
		public static WellBoreArchitectureBatchCatalogDependencies ConstructWellBoreArchitectureBatchCatalogDependencies()
		{
			return new WellBoreArchitectureBatchCatalogDependencies
			{
				Identities = new List<WellBoreArchitectureIdentity>
					{
						ConstructWellBoreArchitectureIdentity(),
					},
				FeatureCategories = new List<WellBoreArchitectureFeatureCategory>
					{
						ConstructWellBoreArchitectureFeatureCategory(),
					},
			};
		}
		public static WellBoreArchitectureBatchCatalogMapping ConstructWellBoreArchitectureBatchCatalogMapping()
		{
			return new WellBoreArchitectureBatchCatalogMapping
			{
				Catalog = "Default Catalog",
				Name = "Default Name",
				SourceID = new Guid(),
				LocalID = new Guid(),
				Resolution = "Default Resolution",
			};
		}
		public static WellBoreArchitectureBatchError ConstructWellBoreArchitectureBatchError()
		{
			return new WellBoreArchitectureBatchError
			{
				PositionIndex = null,
				Property = "Default Property",
				Code = "Default Code",
				Message = "Default Message",
			};
		}
		public static WellBoreArchitectureBatchErrorEnvelope ConstructWellBoreArchitectureBatchErrorEnvelope()
		{
			return new WellBoreArchitectureBatchErrorEnvelope
			{
				Error = "Default Error",
				Message = "Default Message",
				Errors = new List<WellBoreArchitectureBatchError>
					{
						ConstructWellBoreArchitectureBatchError(),
					},
			};
		}
		public static WellBoreArchitectureBatchExportDocument ConstructWellBoreArchitectureBatchExportDocument()
		{
			return new WellBoreArchitectureBatchExportDocument
			{
				FormatIdentifier = "Default FormatIdentifier",
				SchemaVersion = 0,
				ExportedAtUtc = DateTimeOffset.UtcNow,
				CatalogDependencies = ConstructWellBoreArchitectureBatchCatalogDependencies(),
				WellBoreArchitectures = new List<WellBoreArchitecture>
					{
						ConstructWellBoreArchitecture(),
					},
			};
		}
		public static WellBoreArchitectureBatchExportRequest ConstructWellBoreArchitectureBatchExportRequest()
		{
			return new WellBoreArchitectureBatchExportRequest
			{
				Scope = (WellBoreArchitectureBatchExportScope)0,
				WellBoreArchitectureIDs = new List<Guid>
					{
						new Guid(),
					},
			};
		}
		public static WellBoreArchitectureBatchRestoreRequest ConstructWellBoreArchitectureBatchRestoreRequest()
		{
			return new WellBoreArchitectureBatchRestoreRequest
			{
				ConflictPolicy = (WellBoreArchitectureBatchRestoreConflictPolicy)0,
				CatalogPolicy = (WellBoreArchitectureBatchCatalogRestorePolicy)0,
				Document = ConstructWellBoreArchitectureBatchExportDocument(),
			};
		}
		public static WellBoreArchitectureBatchRestoreResponse ConstructWellBoreArchitectureBatchRestoreResponse()
		{
			return new WellBoreArchitectureBatchRestoreResponse
			{
				RestoredAtUtc = DateTimeOffset.UtcNow,
				CreatedCount = 0,
				ReplacedCount = 0,
				CreatedCatalogDefinitionCount = 0,
				CreatedCatalogOptionCount = 0,
				CatalogMappings = new List<WellBoreArchitectureBatchCatalogMapping>
					{
						ConstructWellBoreArchitectureBatchCatalogMapping(),
					},
				WellBoreArchitectureIDs = new List<Guid>
					{
						new Guid(),
					},
			};
		}
		public static WellBoreArchitectureFeatureAssignment ConstructWellBoreArchitectureFeatureAssignment()
		{
			return new WellBoreArchitectureFeatureAssignment
			{
				ID = new Guid(),
				FeatureCategoryID = null, 
				FeatureOptionID = null, 
				FromDate = DateTimeOffset.UtcNow,
				ToDate = DateTimeOffset.UtcNow,
			};
		}
		public static WellBoreArchitectureFeatureCategory ConstructWellBoreArchitectureFeatureCategory()
		{
			return new WellBoreArchitectureFeatureCategory
			{
				MetaInfo = ConstructMetaInfo(),
				Name = "Default Name",
				IsExclusive = false, 
				HasValidityPeriod = false, 
				Options = new List<WellBoreArchitectureFeatureOption>
					{
						ConstructWellBoreArchitectureFeatureOption(),
					},
				CreationDate = DateTimeOffset.UtcNow,
				LastModificationDate = DateTimeOffset.UtcNow,
			};
		}
		public static WellBoreArchitectureFeatureOption ConstructWellBoreArchitectureFeatureOption()
		{
			return new WellBoreArchitectureFeatureOption
			{
				ID = new Guid(),
				Name = "Default Name",
			};
		}
		public static WellBoreArchitectureFluid ConstructWellBoreArchitectureFluid()
		{
			return new WellBoreArchitectureFluid
			{
				Fluid = (FluidType)0,
				Depth = ConstructGaussianDrillingProperty(),
			};
		}
		public static WellBoreArchitectureIdentity ConstructWellBoreArchitectureIdentity()
		{
			return new WellBoreArchitectureIdentity
			{
				MetaInfo = ConstructMetaInfo(),
				Name = "Default Name",
				CreationDate = DateTimeOffset.UtcNow,
				LastModificationDate = DateTimeOffset.UtcNow,
			};
		}
		public static WellBoreArchitectureIdentityAssignment ConstructWellBoreArchitectureIdentityAssignment()
		{
			return new WellBoreArchitectureIdentityAssignment
			{
				ID = new Guid(),
				IdentityID = null, 
				Value = "Default Value",
			};
		}
		public static WellHead ConstructWellHead()
		{
			return new WellHead
			{
				MaxOD = ConstructScalarDrillingProperty(),
				MinOD = ConstructScalarDrillingProperty(),
				Depth = ConstructGaussianDrillingProperty(),
				CasingHangerDepth = ConstructScalarDrillingProperty(),
				TubingHangerDepth = ConstructScalarDrillingProperty(),
			};
		}
	}
}
