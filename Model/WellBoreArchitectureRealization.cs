using System.Collections.Generic;

namespace NORCE.Drilling.WellBoreArchitecture.Model
{
    public class WellBoreArchitectureRealization
    {
        /// <summary>
        /// The well head 
        /// </summary>
        public WellHeadRealization WellHead { get; set; } 
        /// <summary>
        /// the ground level depth
        /// </summary>
        public double? GroundLevelDepth { get; set; } 
        /// <summary>
        /// List of fluids above ground level. The last fluid extend to the ground level
        /// </summary>
        public List<WellBoreArchitectureFluidRealization> FluidsAboveGroundLevel { get; set; }
        /// <summary>
        /// List of SurfaceSections above the well head sorted top to down
        /// </summary>
        public List<SurfaceSectionRealization> SurfaceSections { get; set; }
        /// <summary>
        /// List of Sections starting from the well head sorted top to down
        /// </summary>
        public List<CasingSectionRealization> CasingSections { get; set; }

        /// <summary>
        /// default constructor
        /// </summary>
        public WellBoreArchitectureRealization() : base()
        {

        }
        /// <summary>
        /// copy constructor
        /// </summary>
        /// <param name="src"></param>

    }
}
