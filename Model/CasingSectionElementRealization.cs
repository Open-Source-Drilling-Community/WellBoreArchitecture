namespace NORCE.Drilling.WellBoreArchitecture.Model
{
    public class CasingSectionElementRealization
    {
        /// <summary>
        /// Casing joint OD
        /// </summary>
        public double? BodyOD { get; set; }
        /// <summary>
        /// Casing joint ID
        /// </summary>
        public double? BodyID { get; set; }
        /// <summary>
        /// Casing joint collar OD
        /// </summary>
        public double? CollarOD { get; set; }
        /// <summary>
        /// the mean length of casing joint
        /// </summary>
        public double? JointLength { get; set; } 
        /// <summary>
        /// Casing section length
        /// </summary>
        public double? SectionLength { get; set; } 
        /// <summary>
        /// Max acceptable curvature
        /// </summary>
        public double? MaxDLS { get; set; } 
        /// <summary>
        /// Description of the joint connection thread
        /// </summary>
        public string ConnectionType { get; set; }
        /// <summary>
        /// Material grade
        /// </summary>
        public string Grade { get; set; }
        /// <summary>
        /// Material density mean value
        /// </summary>
        public double? MaterialDensity { get; set; }
        /// <summary>
        /// Material Young modulus
        /// </summary>
        public double? YoungModulus { get; set; } 
        /// <summary>
        /// Mean value of the linear weight including the collar
        /// </summary>
        public double? LinearWeight { get; set; } 
        /// <summary>
        /// Mean value for the tensile strength
        /// </summary>
        public double? TensileStrength { get; set; } 
        /// <summary>
        /// Mean value for the torsional strength
        /// </summary>
        public double? TorsionalStrength { get; set; } 
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
        /// default constructor
        /// </summary>
        public CasingSectionElementRealization()
        {

        }

    }
}
