using OSDC.DotnetLibraries.General.DrillingProperties;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NORCE.Drilling.WellBoreArchitecture.Model
{
    public class WellBoreArchitectureFluid
    {
        public FluidType Fluid { get; set; }
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
