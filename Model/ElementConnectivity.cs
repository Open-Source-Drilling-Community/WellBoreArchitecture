using OSDC.DotnetLibraries.General.DrillingProperties;
using OSDC.UnitConversion.Conversion.DrillingEngineering;
using System.Collections.Generic;
using System;

namespace OSDC.Drilling.WellBoreArchitecture.Model
{
    public class ElementConnectivity
    {
        /// <summary>Stable identifier used to address this nested component independently.</summary>
        public Guid ComponentID { get; set; }
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
