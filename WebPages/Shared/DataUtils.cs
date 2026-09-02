using OSDC.Drilling.WellBoreArchitecture.ModelShared;

namespace OSDC.Drilling.WellBoreArchitecture.WebPages.Shared;

public static class DataUtils
{
    public const double DEFAULT_VALUE = 999.25;
    public static string DEFAULT_NAME_WellBoreArchitecture = "Default WellBoreArchitecture Name";
    public static string DEFAULT_DESCR_WellBoreArchitecture = "Default WellBoreArchitecture Description";

    public static class UnitAndReferenceParameters
    {
        public static string? UnitSystemName { get; set; } = "Metric";
        public static string? DepthReferenceName { get; set; } = "Rotary table";
        public static string? PositionReferenceName { get; set; }
        public static string? AzimuthReferenceName { get; set; }
        public static string? PressureReferenceName { get; set; }
        public static string? DateReferenceName { get; set; }
        public static GroundMudLineDepthReferenceSource GroundMudLineDepthReferenceSource { get; set; } = new();
        public static MeanSeaLevelDepthReferenceSource MeanSeaLevelDepthReferenceSource { get; set; } = new();
        public static RotaryTableDepthReferenceSource RotaryTableDepthReferenceSource { get; set; } = new();
        public static SeaWaterLevelDepthReferenceSource SeaWaterLevelDepthReferenceSource { get; set; } = new();
        public static WellHeadDepthReferenceSource WellHeadDepthReferenceSource { get; set; } = new();
    }

    public static void UpdateUnitSystemName(string val)
    {
        UnitAndReferenceParameters.UnitSystemName = val;
    }

    public static void UpdateDepthReferenceName(string val)
    {
        UnitAndReferenceParameters.DepthReferenceName = val;
    }

    public static readonly string WellBoreArchitectureNameLabel = "WellBoreArchitecture name";
    public static readonly string WellBoreArchitectureParamLabel = "WellBoreArchitectureParam";
    public static readonly string WellBoreArchitectureParamQty = "DepthDrilling";
    public static readonly string WellBoreArchitectureTypeLabel = "WellBoreArchitecture type";
    public static readonly string DerivedData1Label = "DerivedData1 name";
    public static readonly string DerivedData1ParamLabel = "DerivedData1Param";
    public static readonly string DerivedData1ParamQty = "DepthDrilling";
    public static readonly string DerivedData2Label = "DerivedData2 name";
    public static readonly string DerivedData2ParamLabel = "DerivedData2Param";
    public static readonly string DerivedData2ParamQty = "DepthDrilling";
    public static readonly string InputXValuesTitle = "X value";
    public static readonly string InputXValuesQty = "DepthDrilling";
    public static readonly string InputYValuesTitle = "Y value";
    public static readonly string InputYValuesQty = "Length";
    public static readonly string OutputXValuesTitle = "X value";
    public static readonly string OutputXValuesQty = "DepthDrilling";
    public static readonly string OutputYValuesTitle = "Y value";
    public static readonly string OutputYValuesQty = "Length";
    public static BoreHoleSize? CreateCopy(BoreHoleSize boreHoleSize)
    {
        return new BoreHoleSize
        {
            HoleSize = boreHoleSize.HoleSize,
            Length = boreHoleSize.Length
        };
    }
    public static CasingSection? CreateCopy(CasingSection casingSection)
    {
        List<CasingSectionElement> elements = new();
        foreach (CasingSectionElement? ele in casingSection.CasingSectionElements)
        {
            if (ele != null)
            {
                elements.Add(CreateCopy(ele));                
            }
        }
        return new CasingSection
        {
            TopDepth = casingSection.TopDepth,
            Length = casingSection.Length, 
            TopCementDepth = casingSection.TopCementDepth,
            CasingSectionElements = elements,
            CasingSectionSizeTable = casingSection.CasingSectionSizeTable,
            OpenHoleSection = casingSection.OpenHoleSection
        };
    }
    public static CasingSectionElement? CreateCopy(CasingSectionElement element)
    {
        return new CasingSectionElement
        {
            BodyOD = element.BodyOD,
            BodyID = element.BodyID,
            CollarOD = element.CollarOD,
            JointLength = element.JointLength,
            SectionLength = element.SectionLength,
            MaxDLS = element.MaxDLS,
            ConnectionType = element.ConnectionType,
            Grade = element.Grade,
            MaterialDensity = element.MaterialDensity,
            YoungModulus = element.YoungModulus,
            LinearWeight = element.LinearWeight,
            TensileStrength = element.TensileStrength,
            TorsionalStrength = element.TorsionalStrength,
            BurstPressure = element.BurstPressure,
            CollapsePressure = element.CollapsePressure,
            YieldStress = element.YieldStress,
            MakeUpTorqueRecommended = element.MakeUpTorqueRecommended
        };
    }
}
