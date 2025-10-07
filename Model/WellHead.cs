using System;
using System.Collections.Generic;
using System.Text;
using OSDC.DotnetLibraries.General.DrillingProperties;
using OSDC.UnitConversion.Conversion;
using OSDC.UnitConversion.Conversion.DrillingEngineering;

namespace NORCE.Drilling.WellBoreArchitecture.Model
{
    public class WellHead
    {
        /// <summary>
        /// 
        /// </summary>
        public ScalarDrillingProperty? MaxOD { get; set; } = new ScalarDrillingProperty();

        /// <summary>
        /// 
        /// </summary>
        public ScalarDrillingProperty? MinOD { get; set; } = new ScalarDrillingProperty();

        public GaussianDrillingProperty? Depth { get; set; } = new GaussianDrillingProperty();

        /// <summary>
        /// 
        /// </summary>
        public ScalarDrillingProperty? CasingHangerDepth { get; set; } = new ScalarDrillingProperty();

        /// <summary>
        /// 
        /// </summary>
        public ScalarDrillingProperty? TubingHangerDepth { get; set; } = new ScalarDrillingProperty();

        /// <summary>
        /// Default constructor
        /// </summary>
        public WellHead()
        {

        }
        /// <summary>
        /// the realization method of the factory pattern
        /// </summary>
        /// <returns></returns>        
        public WellHeadRealization Realize()
        {
            return new WellHeadRealization()
            {
                Depth = Depth.Value.Realize(),
                CasingHangerDepth = CasingHangerDepth.Value.Realize(),
                TubingHangerDepth = TubingHangerDepth.Value.Realize(),
                MinOD = MinOD.Value.Realize(),
                MaxOD = MaxOD.Value.Realize()
            };
        }
    }
}
