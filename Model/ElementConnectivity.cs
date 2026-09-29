using OSDC.DotnetLibraries.Drilling.SemanticCatalogue;
using OSDC.DotnetLibraries.General.DrillingProperties;
using OSDC.UnitConversion.Conversion.DrillingEngineering;
using System.Collections.Generic;
using System;

namespace OSDC.Drilling.WellBoreArchitecture.Model
{
    [Semantic(Concepts.SideCircuitConnectivity)]
    public class ElementConnectivity
    {
        /// <summary>Stable identifier used to address this nested component independently.</summary>
        [Semantic(Concepts.ResourceIdentifier)]
        public Guid ComponentID { get; set; }
        /// <summary>
        /// the type of the element
        /// </summary>
        [Semantic(Concepts.SideCircuitElement, Role = Concepts.Upstream)]
        public SideElement? UpstreamElement { get; set; }
        [Semantic(Concepts.SideCircuitElement, Role = Concepts.Downstream)]
        public SideElement? DownstreamElement { get; set; }
        /// <summary>
        /// the length of the element
        /// </summary>

        public ElementConnectivity()
        {

        }
    }
}
