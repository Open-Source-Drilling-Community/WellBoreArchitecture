namespace NORCE.Drilling.WellBoreArchitecture.Model
{
    public class SideConnectorRealization
    {
        /// <summary>
        /// The position of the side connector along the element
        /// </summary>
        public double? Position { get; set; } = null;
        /// <summary>
        /// the vertical depth of the connector
        /// </summary>
        public double? VerticalDepth { get; set; } = null;
        /// <summary>
        /// the root of the side circuitry network
        /// </summary>
        /// <summary>
        /// default constructor
        /// </summary>

        public SideElementRealization FirstSideElement { get; set; } 

        public SideConnectorRealization() { }

    }
}
