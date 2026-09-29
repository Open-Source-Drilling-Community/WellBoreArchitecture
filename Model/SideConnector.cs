using OSDC.DotnetLibraries.Drilling.SemanticCatalogue;
using System.Collections.Generic;
using System;
using OSDC.DotnetLibraries.General.DrillingProperties;

namespace OSDC.Drilling.WellBoreArchitecture.Model
{
    [Semantic(Concepts.SideConnector)]
    public class SideConnector
    {
        /// <summary>Stable identifier used to address this nested component independently.</summary>
        [Semantic(Concepts.ResourceIdentifier)]
        public Guid ComponentID { get; set; }
        /// <summary>
        /// The position of the side connector along the element
        /// </summary>
        [EngineeringQuantity(Concepts.HostComponentAbscissa, Concepts.DimensionalLengthStandardUncertainty)]
        public GaussianDrillingProperty Position { get; set; } = new GaussianDrillingProperty();
        /// <summary>
        /// the vertical depth of the connector
        /// </summary>
        [EngineeringQuantity(Concepts.EllipsoidalDepth, Concepts.LinearStandardUncertainty, Role = Concepts.ConnectorLocation)]
        public GaussianDrillingProperty VerticalDepth { get; set; } = new GaussianDrillingProperty();
        /// <summary>
        /// the root of the side circuitry network
        /// </summary>
        public SideElement? FirstSideElement { get; set; }
        public List<ElementConnectivity>? ElementConnectivities { get; set; }

        /// <summary>
        /// default constructor
        /// </summary>
        public SideConnector() { }

        /// <summary>
        ///  The realize method of the factory pattern
        /// </summary>
        /// <returns></returns>

        public SideConnectorRealization Realize()
        {
            SideConnectorRealization realization = new SideConnectorRealization()
            {
                Position = Position.GaussianValue.Realize(),
                VerticalDepth = VerticalDepth.GaussianValue.Realize()
            };
            if (FirstSideElement != null)
            {
                realization.FirstSideElement = FirstSideElement.Realize();
            }
            return realization;
        }
    }
}
