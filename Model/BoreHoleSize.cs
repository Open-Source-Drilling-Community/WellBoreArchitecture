using OSDC.DotnetLibraries.Drilling.SemanticCatalogue;
using OSDC.DotnetLibraries.General.DrillingProperties;
using OSDC.UnitConversion.Conversion;
using OSDC.UnitConversion.Conversion.DrillingEngineering;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OSDC.Drilling.WellBoreArchitecture.Model
{
    [Semantic(Concepts.BoreholeSizeInterval)]
    public class BoreHoleSize
    {
        /// <summary>Stable identifier used to address this nested component independently.</summary>
        [Semantic(Concepts.ResourceIdentifier)]
        public Guid ComponentID { get; set; }
        /// <summary>
        /// The hole size is a Gaussian distribution of quantity pipe diameter
        /// </summary>
        [EngineeringQuantity(Concepts.BoreholeDiameter, Concepts.DimensionalLengthStandardUncertainty)]
        public GaussianDrillingProperty HoleSize { get; set; } = new GaussianDrillingProperty();
        /// <summary>
        /// The borehole length mean value for which the borehole size is valid
        /// </summary>
        [EngineeringQuantity(Concepts.PhysicalLengthExtent, Concepts.DimensionalLengthStandardUncertainty, Role = Concepts.BoreholeIntervalExtent)]
        public GaussianDrillingProperty Length { get; set; } = new GaussianDrillingProperty();
        /// <summary>
        /// default constructor
        /// </summary>
        public BoreHoleSize() { }
        public BoreHoleSizeRealization Realize()
        {
            return new BoreHoleSizeRealization() { HoleSize = HoleSize.Value.Realize(), Length = Length.Value.Realize() };
        }
    }
}
