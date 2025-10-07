using OSDC.DotnetLibraries.General.DrillingProperties;
using OSDC.UnitConversion.Conversion.DrillingEngineering;
using System.Collections.Generic;

namespace NORCE.Drilling.WellBoreArchitecture.Model
{
    public class ElementConnectivity
    {
        /// <summary>
        /// the type of the element
        /// </summary>
        public SideElement? UpstreamElement { get; set; }
        public SideElement? DownstreamElement { get; set; }
        /// <summary>
        /// the length of the element
        /// </summary>

        public ElementConnectivity()
        {

        }
    }
}
