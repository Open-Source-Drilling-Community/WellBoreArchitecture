using OSDC.DotnetLibraries.General.DrillingProperties;
using System.Collections.Generic;
using System;

namespace OSDC.Drilling.WellBoreArchitecture.Model
{
    public class CasingSection
    {
        /// <summary>Stable identifier used to address this nested component independently.</summary>
        public Guid ComponentID { get; set; }

        /// <summary>
        /// The top depth is a Gaussian depth quantity stored in metres and referenced to the WGS84 datum.
        /// </summary>
        public GaussianDrillingProperty TopDepth { get; set; } = new GaussianDrillingProperty();
        /// <summary>
        /// the length is a Gaussian value that is standard length quantity.
        /// </summary>
        public GaussianDrillingProperty Length { get; set; } = new GaussianDrillingProperty();
        /// <summary>
        /// The top-of-cement depth is a Gaussian depth quantity stored in metres and referenced to the WGS84 datum.
        /// </summary>
        public GaussianDrillingProperty TopCementDepth { get; set; } = new GaussianDrillingProperty();
        public List<CasingSectionElement> CasingSectionElements { get; set; } = new();
        /// <summary>
        /// Table containing length and diameter of each casing section 
        /// </summary>
        public List<BoreHoleSize> CasingSectionSizeTable { get; set; } = new();

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
            if (CasingSectionSizeTable != null)
            {
                realization.CasingSectionSizeTable = new();
                foreach (var size in CasingSectionSizeTable)
                {
                    realization.CasingSectionSizeTable.Add(size.Realize());
                }
            }
            return realization;
        }

    }
}
