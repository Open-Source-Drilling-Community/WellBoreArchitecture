using System.Collections.Generic;

namespace NORCE.Drilling.WellBoreArchitecture.Model
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
        public List<CasingSectionElementRealization> CasingSectionElements { get; set; }
        /// <summary>
        /// The open hole section starts from where it finished in the previous casing section 
        /// or the ground level for the first casing section
        /// </summary>
        public OpenHoleSectionRealization OpenHoleSection { get; set; }

        /// <summary>
        /// Default constructor
        /// </summary>
        public CasingSectionRealization()
        {

        }
    }
}
