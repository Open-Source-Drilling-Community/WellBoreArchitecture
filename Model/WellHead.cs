using OSDC.DotnetLibraries.Drilling.SemanticCatalogue;
using System;
using System.Collections.Generic;
using System.Text;
using OSDC.DotnetLibraries.General.DrillingProperties;
using OSDC.UnitConversion.Conversion;
using OSDC.UnitConversion.Conversion.DrillingEngineering;

namespace OSDC.Drilling.WellBoreArchitecture.Model
{
    [Semantic(Concepts.Wellhead)]
    public class WellHead
    {
        /// <summary>
        /// 
        /// </summary>
        [EngineeringQuantity(Concepts.PipeDiameter, Role = Concepts.MaximumWellheadOutsideDiameter)]
        public ScalarDrillingProperty? MaxOD { get; set; } = new ScalarDrillingProperty();

        /// <summary>
        /// 
        /// </summary>
        [EngineeringQuantity(Concepts.PipeDiameter, Role = Concepts.MinimumWellheadOutsideDiameter)]
        public ScalarDrillingProperty? MinOD { get; set; } = new ScalarDrillingProperty();

        [EngineeringQuantity(Concepts.EllipsoidalDepth, Concepts.LinearStandardUncertainty, Role = Concepts.WellheadLocation)]
        public GaussianDrillingProperty? Depth { get; set; } = new GaussianDrillingProperty();

        /// <summary>
        /// 
        /// </summary>
        [EngineeringQuantity(Concepts.EllipsoidalDepth, Role = Concepts.CasingHangerLocation)]
        public ScalarDrillingProperty? CasingHangerDepth { get; set; } = new ScalarDrillingProperty();

        /// <summary>
        /// 
        /// </summary>
        [EngineeringQuantity(Concepts.EllipsoidalDepth, Role = Concepts.TubingHangerLocation)]
        public ScalarDrillingProperty? TubingHangerDepth { get; set; } = new ScalarDrillingProperty();

        /// <summary>
        /// Default constructor
        /// </summary>
        public WellHead()
        {

        }
        /// <summary>
        /// the realization method of the factory pattern
        /// </summary>
        /// <returns></returns>        
        public WellHeadRealization Realize()
        {
            return new WellHeadRealization()
            {
                Depth = Depth.Value.Realize(),
                CasingHangerDepth = CasingHangerDepth.Value.Realize(),
                TubingHangerDepth = TubingHangerDepth.Value.Realize(),
                MinOD = MinOD.Value.Realize(),
                MaxOD = MaxOD.Value.Realize()
            };
        }
    }
}
