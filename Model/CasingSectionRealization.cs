using System.Collections.Generic;

namespace OSDC.Drilling.WellBoreArchitecture.Model
{
    public class CasingSectionRealization
    {
        /// <summary>
        /// the top depth of the casing section 
        /// </summary>
        public double? TopDepth { get; set; } 
        /// <summary>
        /// the length 
        /// </summary>
        public double? Length { get; set; } 
        /// <summary>
        /// the top of cement depth 
        /// </summary>
        public double? TopCementDepth { get; set; } 
        public List<CasingSectionElementRealization> CasingSectionElements { get; set; } = new();
        /// <summary>
        /// Borehole diameter and interval-length rows applicable to this casing section.
        /// </summary>
        public List<BoreHoleSizeRealization> CasingSectionSizeTable { get; set; } = new();

        /// <summary>
        /// Default constructor
        /// </summary>
        public CasingSectionRealization()
        {

        }
    }
}
