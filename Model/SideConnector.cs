using System.Collections.Generic;
using OSDC.DotnetLibraries.General.DrillingProperties;

namespace NORCE.Drilling.WellBoreArchitecture.Model
{
    public class SideConnector
    {
        /// <summary>
        /// The position of the side connector along the element
        /// </summary>
        public GaussianDrillingProperty Position { get; set; } = new GaussianDrillingProperty();
        /// <summary>
        /// the vertical depth of the connector
        /// </summary>
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
