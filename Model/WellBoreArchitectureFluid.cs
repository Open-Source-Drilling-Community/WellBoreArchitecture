using OSDC.DotnetLibraries.Drilling.SemanticCatalogue;
using OSDC.DotnetLibraries.General.DrillingProperties;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OSDC.Drilling.WellBoreArchitecture.Model
{
    [Semantic(Concepts.EnvironmentalFluidLayer)]
    public class WellBoreArchitectureFluid
    {
        public FluidType Fluid { get; set; }
        [EngineeringQuantity(Concepts.EllipsoidalDepth, Concepts.LinearStandardUncertainty, Role = Concepts.FluidLayerTopBoundary)]
        public GaussianDrillingProperty Depth { get; set; } = new GaussianDrillingProperty();
        /// <summary>
        /// default constructor
        /// </summary>
        public WellBoreArchitectureFluid() { }
        /// <summary>
        /// Copy constructor
        /// </summary>
        /// <param name="src"></param>
        /// <summary>
        /// The realization method of the factory pattern
        /// </summary>
        /// <returns></returns>
        /// 
        
        public WellBoreArchitectureFluidRealization Realize()
        {
            return new WellBoreArchitectureFluidRealization()
            {
                Fluid = Fluid,
                Depth = Depth.Value.Realize()
            };
        }
    }
}
