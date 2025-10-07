using OSDC.DotnetLibraries.General.DrillingProperties;
using System.Collections.Generic;

namespace NORCE.Drilling.WellBoreArchitecture.Model
{
    public class CasingSection
    {

        /// <summary>
        /// the top depth of the casing section is a Gaussian value that is Depth quantity and that is referred to the Well-head.
        /// </summary>
        public GaussianDrillingProperty TopDepth { get; set; } = new GaussianDrillingProperty();
        /// <summary>
        /// the length is a Gaussian value that is standard length quantity.
        /// </summary>
        public GaussianDrillingProperty Length { get; set; } = new GaussianDrillingProperty();
        /// <summary>
        /// the top of cement depth is a Gaussian value that is a Depth quantity and is referred to the well-head
        /// </summary>
        public GaussianDrillingProperty TopCementDepth { get; set; } = new GaussianDrillingProperty();
        public List<CasingSectionElement> CasingSectionElements { get; set; }
        /// <summary>
        /// Table containing length and diameter of each casing section 
        /// </summary>
        public List<BoreHoleSize> CasingSectionSizeTable { get; set; }

        /// <summary>
        /// The open hole section starts from where it finished in the previous casing section 
        /// or the ground level for the first casing section
        /// </summary>
        public OpenHoleSection? OpenHoleSection { get; set; }



        /// <summary>
        /// Default constructor
        /// </summary>
        public CasingSection()
        {

        }
        /// <summary>
        /// Realization method of the factory pattern
        /// </summary>
        /// <returns></returns>
        
        public CasingSectionRealization Realize()
        {
            CasingSectionRealization realization = new CasingSectionRealization()
            {
                TopDepth = TopDepth.Value.Realize(),
                Length = Length.Value.Realize(),
                TopCementDepth = TopCementDepth.Value.Realize(),
            };
            if (CasingSectionElements != null)
            {
                realization.CasingSectionElements = new();
                foreach (var element in CasingSectionElements)
                {
                    realization.CasingSectionElements.Add(element.Realize());
                }
            }
            if (OpenHoleSection != null)
            {
                realization.OpenHoleSection = OpenHoleSection.Realize();
            }
            return realization;
        }

    }
}
