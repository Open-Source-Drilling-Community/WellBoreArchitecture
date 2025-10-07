using OSDC.DotnetLibraries.General.DrillingProperties;
using OSDC.UnitConversion.Conversion.DrillingEngineering;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NORCE.Drilling.WellBoreArchitecture.Model
{
    public class WellHeadRealization
    {
        /// <summary>
        /// 
        /// </summary>
        public double? MaxOD { get; set; } 

        /// <summary>
        /// 
        /// </summary>
        public double? MinOD { get; set; } 
        /// <summary>
        /// 
        /// </summary>
        public double? Depth { get; set; } 

        /// <summary>
        /// 
        /// </summary>
        public double? CasingHangerDepth { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public double? TubingHangerDepth { get; set; } 

        /// <summary>
        /// Default constructor
        /// </summary>
        public WellHeadRealization()
        {

        }
    }
}
