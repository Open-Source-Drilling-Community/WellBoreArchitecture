using System.Collections.Generic;
using static NORCE.Drilling.WellBoreArchitecture.Model.SideElement;

namespace NORCE.Drilling.WellBoreArchitecture.Model
{
    public class SideElementRealization
    {
        /// <summary>
        /// the type of the element
        /// </summary>
        public SideElementType Type { get; set; } = SideElementType.Unknown;
        /// <summary>
        /// the length of the element
        /// </summary>
        public double? Length { get; set; } = null;
        /// <summary>
        /// the vertical depth of the top of the element
        /// </summary>
        public double? TopVerticalDepth { get; set; } = null;
        /// <summary>
        /// the typical outer diameter of the element
        /// </summary>
        public double? OD { get; set; } = null;
        /// <summary>
        ///  the typical inner diameter of the element
        /// </summary>
        public double? ID { get; set; } = null;
        /// <summary>
        /// The element connected at the end of the element
        /// </summary>
        public SideElementRealization()
        {

        }
    }
}
