using OSDC.DotnetLibraries.General.DrillingProperties;

namespace NORCE.Drilling.WellBoreArchitecture.Model
{
    public class CasingSectionElement
    {

        /// <summary>
        /// Casing joint OD
        /// </summary>
        public GaussianDrillingProperty BodyOD { get; set; } = new GaussianDrillingProperty();
        /// <summary>
        /// Casing joint ID
        /// </summary>
        public GaussianDrillingProperty BodyID { get; set; } = new GaussianDrillingProperty();
        /// <summary>
        /// Casing joint collar OD
        /// </summary>
        public GaussianDrillingProperty CollarOD { get; set; } = new GaussianDrillingProperty();
        /// <summary>
        /// the mean length of casing joint
        /// </summary>
        public GaussianDrillingProperty JointLength { get; set; } = new GaussianDrillingProperty();
        /// <summary>
        /// Casing section length
        /// </summary>
        public GaussianDrillingProperty? SectionLength { get; set; } = new GaussianDrillingProperty();
        /// <summary>
        /// Max acceptable curvature
        /// </summary>
        public ScalarDrillingProperty? MaxDLS { get; set; } = new ScalarDrillingProperty();
        /// <summary>
        /// Description of the joint connection thread
        /// </summary>
        public string? ConnectionType { get; set; }
        /// <summary>
        /// Material grade
        /// </summary>
        public string? Grade { get; set; }
        /// <summary>
        /// Material density mean value
        /// </summary>
        public GaussianDrillingProperty? MaterialDensity { get; set; } = new GaussianDrillingProperty();
        /// <summary>
        /// Material Young modulus
        /// </summary>
        public GaussianDrillingProperty? YoungModulus { get; set; } = new GaussianDrillingProperty();
        /// <summary>
        /// Mean value of the linear weight including the collar
        /// </summary>
        public GaussianDrillingProperty? LinearWeight { get; set; } = new GaussianDrillingProperty();
        /// <summary>
        /// Mean value for the tensile strength
        /// </summary>
        public GaussianDrillingProperty? TensileStrength { get; set; } = new GaussianDrillingProperty();
        /// <summary>
        /// Mean value for the torsional strength
        /// </summary>
        public GaussianDrillingProperty? TorsionalStrength { get; set; } = new GaussianDrillingProperty();
        /// <summary>
        /// Mean value of the burst pressure
        /// </summary>
        public GaussianDrillingProperty? BurstPressure { get; set; } = new GaussianDrillingProperty();
        /// <summary>
        /// Mean value of the collapse pressure
        /// </summary>
        public GaussianDrillingProperty? CollapsePressure { get; set; } = new GaussianDrillingProperty();
        /// <summary>
        /// Mean value of the yield stress
        /// </summary>
        public GaussianDrillingProperty? YieldStress { get; set; } = new GaussianDrillingProperty();
        /// <summary>
        /// Recommended make up torque
        /// </summary>
        public ScalarDrillingProperty? MakeUpTorqueRecommended { get; set; } = new ScalarDrillingProperty();
        /// <summary>
        /// default constructor
        /// </summary>
        public CasingSectionElement()
        {

        }
        /// <summary>
        /// the realization method of the factory pattern
        /// </summary>
        /// <returns></returns>

        public CasingSectionElementRealization Realize()
        {
            return new CasingSectionElementRealization()
            {
                BodyID = BodyID.Value.Realize(),
                BodyOD = BodyOD.Value.Realize(),
                CollarOD = CollarOD.Value.Realize(),
                JointLength = JointLength.Value.Realize(),
                SectionLength = SectionLength.Value.Realize(),
                MaxDLS = MaxDLS.Value.Realize(),
                ConnectionType = ConnectionType,
                Grade = Grade,
                MaterialDensity = MaterialDensity.Value.Realize(),
                YoungModulus = YoungModulus.Value.Realize(),
                LinearWeight = LinearWeight.Value.Realize(),
                TensileStrength = TensileStrength.Value.Realize(),
                TorsionalStrength = TorsionalStrength.Value.Realize(),
                BurstPressure = BurstPressure.Value.Realize(),
                CollapsePressure = CollapsePressure.Value.Realize(),
                YieldStress = YieldStress.Value.Realize(),
                MakeUpTorqueRecommended = MakeUpTorqueRecommended.Value.Realize()
			};
        }
    }
}
