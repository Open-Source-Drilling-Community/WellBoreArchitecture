using OSDC.DotnetLibraries.General.DrillingProperties;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static NORCE.Drilling.WellBoreArchitecture.Model.WellBoreArchitectureFluid;

namespace NORCE.Drilling.WellBoreArchitecture.Model
{
    public class WellBoreArchitectureFluidRealization
    {
        public FluidType Fluid { get; set; }
        public double? Depth { get; set; } 
        /// <summary>
        /// default constructor
        /// </summary>
        public WellBoreArchitectureFluidRealization() { }
    }
}
