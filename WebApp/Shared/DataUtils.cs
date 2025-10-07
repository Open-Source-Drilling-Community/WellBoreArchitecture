
public static class DataUtils
{
    // default values
    public const double DEFAULT_VALUE = 999.25;
    public static string DEFAULT_NAME_WellBoreArchitecture = "Default WellBoreArchitecture Name";
    public static string DEFAULT_DESCR_WellBoreArchitecture = "Default WellBoreArchitecture Description";

    // unit management
    public static class UnitAndReferenceParameters
    {
        public static string? UnitSystemName { get; set; } = "Metric";
        public static string? DepthReferenceName { get; set; } = "Rotary table";
        public static string? PositionReferenceName { get; set; }
        public static string? AzimuthReferenceName { get; set; }
        public static string? PressureReferenceName { get; set; }
        public static string? DateReferenceName { get; set; }
        public static GroundMudLineDepthReferenceSource GroundMudLineDepthReferenceSource { get; set; } = new GroundMudLineDepthReferenceSource();
        public static RotaryTableDepthReferenceSource RotaryTableDepthReferenceSource { get; set; } = new RotaryTableDepthReferenceSource();
        public static SeaWaterLevelDepthReferenceSource SeaWaterLevelDepthReferenceSource { get; set; } = new SeaWaterLevelDepthReferenceSource();
        public static WellHeadDepthReferenceSource WellHeadDepthReferenceSource { get; set; } = new WellHeadDepthReferenceSource();

    }

    public static void UpdateUnitSystemName(string val)
    {
        UnitAndReferenceParameters.UnitSystemName = (string)val;
    }

    public static void UpdateDepthReferenceName(string val)
    {
        UnitAndReferenceParameters.DepthReferenceName = (string)val;
    }

    // units and labels
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
    
}