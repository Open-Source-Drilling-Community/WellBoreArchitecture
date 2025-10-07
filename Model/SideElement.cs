using OSDC.DotnetLibraries.General.DrillingProperties;
using OSDC.UnitConversion.Conversion.DrillingEngineering;
using System.Collections.Generic;

namespace NORCE.Drilling.WellBoreArchitecture.Model
{
    public class SideElement
    {
        /// <summary>
        /// Name of the element
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// the type of the element
        /// </summary>
        public SideElementType Type { get; set; } = SideElementType.Unknown;
        /// <summary>
        /// the length of the element
        /// </summary>
        public GaussianDrillingProperty Length { get; set; } = new GaussianDrillingProperty();
        /// <summary>
        /// the vertical depth of the top of the element
        /// </summary>
        public GaussianDrillingProperty TopVerticalDepth { get; set; } = new GaussianDrillingProperty();
        /// <summary>
        /// the typical outer diameter of the element
        /// </summary>
        public GaussianDrillingProperty OD { get; set; } = new GaussianDrillingProperty();
        /// <summary>
        ///  the typical inner diameter of the element
        /// </summary>
        public GaussianDrillingProperty ID { get; set; } = new GaussianDrillingProperty();
        /// <summary>
        /// The element connected at the end of the element
        /// </summary>
        public SideElement()
        {

        }
        public SideElementRealization Realize()
        {
            SideElementRealization realization = new SideElementRealization()
            {
                Type = Type,
                Length = Length.GaussianValue.Realize(),
                TopVerticalDepth = TopVerticalDepth.GaussianValue.Realize(),
                OD = OD.GaussianValue.Realize(),
                ID = ID.GaussianValue.Realize()
            };
            return realization;
        }
    }
}
