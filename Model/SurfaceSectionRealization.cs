using System.Collections.Generic;
using static NORCE.Drilling.WellBoreArchitecture.Model.SurfaceSection;

namespace NORCE.Drilling.WellBoreArchitecture.Model
{
    public class SurfaceSectionRealization
    {
        public SurfaceSectionType Type { get; set; } = SurfaceSectionType.Unknown;
        /// <summary>
        /// The length of the section element is a gaussian distribution.
        /// </summary>
        public double? SectionLength { get; set; }
        /// <summary>
        /// the body OD of the element
        /// </summary>
        public double? BodyOD { get; set; } 
        /// <summary>
        /// the body ID of the element
        /// </summary>
        public double? BodyID { get; set; } 
        /// <summary>
        /// Description of the joint connection thread
        /// </summary>
        public string ConnectionType { get; set; }
        /// <summary>
        /// Material grade
        /// </summary>
        public string Grade { get; set; }
        /// <summary>
        /// Average material density of the section
        /// </summary>
        public double? MaterialDensity { get; set; }
        /// <summary>
        /// Material Young modulus
        /// </summary>
        public double? YoungModulus { get; set; } 
        /// <summary>
        /// Average linear weight of the section
        /// </summary>
        public double? LinearWeight { get; set; } 
        /// <summary>
        /// Average tensile strength of the section
        /// </summary>
        public double? TensileStrength { get; set; } 
        /// <summary>
        /// Mean value of the burst pressure
        /// </summary>
        public double? BurstPressure { get; set; } 
        /// <summary>
        /// Mean value of the collapse pressure
        /// </summary>
        public double? CollapsePressure { get; set; } 
        /// <summary>
        /// Mean value of the yield stress
        /// </summary>
        public double? YieldStress { get; set; } 
        /// <summary>
        /// Recommended make up torque
        /// </summary>
        public double? MakeUpTorqueRecommended { get; set; }
        /// <summary>
        /// possible side ports that connect to additional circuitry
        /// </summary>
        public List<SideConnectorRealization> SideConnectors { get; set; }
        /// <summary>
        /// Default Constructor
        /// </summary>
        public SurfaceSectionRealization()
        {

        }
    }
}
