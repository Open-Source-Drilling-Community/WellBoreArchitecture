using OSDC.DotnetLibraries.Drilling.SemanticCatalogue;
using OSDC.DotnetLibraries.General.DrillingProperties;
using System;

namespace OSDC.Drilling.WellBoreArchitecture.Model
{
    [Semantic(Concepts.CasingElementSpecification)]
    public class CasingSectionElement
    {
        /// <summary>Stable identifier used to address this nested component independently.</summary>
        [Semantic(Concepts.ResourceIdentifier)]
        public Guid ComponentID { get; set; }

        /// <summary>
        /// Casing joint OD
        /// </summary>
        [EngineeringQuantity(Concepts.PipeDiameter, Concepts.DimensionalLengthStandardUncertainty, Role = Concepts.OuterDiameter)]
        public GaussianDrillingProperty BodyOD { get; set; } = new GaussianDrillingProperty();
        /// <summary>
        /// Casing joint ID
        /// </summary>
        [EngineeringQuantity(Concepts.PipeDiameter, Concepts.DimensionalLengthStandardUncertainty, Role = Concepts.InnerDiameter)]
        public GaussianDrillingProperty BodyID { get; set; } = new GaussianDrillingProperty();
        /// <summary>
        /// Casing joint collar OD
        /// </summary>
        [EngineeringQuantity(Concepts.PipeDiameter, Concepts.DimensionalLengthStandardUncertainty, Role = Concepts.CollarOuterDiameter)]
        public GaussianDrillingProperty CollarOD { get; set; } = new GaussianDrillingProperty();
        /// <summary>
        /// the mean length of casing joint
        /// </summary>
        [EngineeringQuantity(Concepts.PhysicalLengthExtent, Concepts.DimensionalLengthStandardUncertainty, Role = Concepts.JointExtent)]
        public GaussianDrillingProperty JointLength { get; set; } = new GaussianDrillingProperty();
        /// <summary>
        /// Casing section length
        /// </summary>
        [EngineeringQuantity(Concepts.PhysicalLengthExtent, Concepts.DimensionalLengthStandardUncertainty, Role = Concepts.SectionExtent)]
        public GaussianDrillingProperty? SectionLength { get; set; } = new GaussianDrillingProperty();
        /// <summary>
        /// Max acceptable curvature
        /// </summary>
        [EngineeringQuantity(Concepts.MaximumPermittedDoglegSeverity)]
        public ScalarDrillingProperty? MaxDLS { get; set; } = new ScalarDrillingProperty();
        /// <summary>
        /// Description of the joint connection thread
        /// </summary>
        [Semantic(Concepts.ConnectionThreadDescription)]
        public string? ConnectionType { get; set; }
        /// <summary>
        /// Material grade
        /// </summary>
        [Semantic(Concepts.MaterialGrade)]
        public string? Grade { get; set; }
        /// <summary>
        /// Material density mean value
        /// </summary>
        [EngineeringQuantity(Concepts.MaterialDensity, Concepts.MassDensityStandardUncertainty)]
        public GaussianDrillingProperty? MaterialDensity { get; set; } = new GaussianDrillingProperty();
        /// <summary>
        /// Material Young modulus
        /// </summary>
        [EngineeringQuantity(Concepts.YoungModulus, Concepts.ElasticModulusStandardUncertainty)]
        public GaussianDrillingProperty? YoungModulus { get; set; } = new GaussianDrillingProperty();
        /// <summary>
        /// Mean value of the linear weight including the collar
        /// </summary>
        [EngineeringQuantity(Concepts.LinearMassDensity, Concepts.LinearMassDensityStandardUncertainty)]
        public GaussianDrillingProperty? LinearWeight { get; set; } = new GaussianDrillingProperty();
        /// <summary>
        /// Mean value for the tensile strength
        /// </summary>
        [EngineeringQuantity(Concepts.TensileStrength, Concepts.MaterialStrengthStandardUncertainty)]
        public GaussianDrillingProperty? TensileStrength { get; set; } = new GaussianDrillingProperty();
        /// <summary>
        /// Mean value for the torsional strength
        /// </summary>
        [EngineeringQuantity(Concepts.TorsionalCapacity, Concepts.TorqueStandardUncertainty)]
        public GaussianDrillingProperty? TorsionalStrength { get; set; } = new GaussianDrillingProperty();
        /// <summary>
        /// Mean value of the burst pressure
        /// </summary>
        [EngineeringQuantity(Concepts.BurstPressureCapacity, Concepts.PressureDifferenceStandardUncertainty)]
        public GaussianDrillingProperty? BurstPressure { get; set; } = new GaussianDrillingProperty();
        /// <summary>
        /// Mean value of the collapse pressure
        /// </summary>
        [EngineeringQuantity(Concepts.CollapsePressureCapacity, Concepts.PressureDifferenceStandardUncertainty)]
        public GaussianDrillingProperty? CollapsePressure { get; set; } = new GaussianDrillingProperty();
        /// <summary>
        /// Mean value of the yield stress
        /// </summary>
        [EngineeringQuantity(Concepts.YieldStress, Concepts.MaterialStrengthStandardUncertainty)]
        public GaussianDrillingProperty? YieldStress { get; set; } = new GaussianDrillingProperty();
        /// <summary>
        /// Recommended make up torque
        /// </summary>
        [EngineeringQuantity(Concepts.RecommendedMakeUpTorque)]
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
