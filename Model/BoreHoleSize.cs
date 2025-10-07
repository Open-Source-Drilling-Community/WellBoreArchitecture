using OSDC.DotnetLibraries.General.DrillingProperties;
using OSDC.UnitConversion.Conversion;
using OSDC.UnitConversion.Conversion.DrillingEngineering;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NORCE.Drilling.WellBoreArchitecture.Model
{
    public class BoreHoleSize
    {
        /// <summary>
        /// The hole size is a Gaussian distribution of quantity pipe diameter
        /// </summary>
        public GaussianDrillingProperty HoleSize { get; set; } = new GaussianDrillingProperty();
        /// <summary>
        /// The borehole length mean value for which the borehole size is valid
        /// </summary>
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
