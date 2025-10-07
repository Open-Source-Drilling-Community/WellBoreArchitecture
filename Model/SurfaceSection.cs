using OSDC.DotnetLibraries.General.DrillingProperties;
using System.Collections.Generic;

namespace NORCE.Drilling.WellBoreArchitecture.Model
{
    public class SurfaceSection
    {

        public SurfaceSectionType Type { get; set; } = SurfaceSectionType.Unknown;
        /// <summary>
        /// The length of the section element is a gaussian distribution.
        /// </summary>
        public GaussianDrillingProperty? SectionLength { get; set; } = new GaussianDrillingProperty();
        /// <summary>
        /// the body OD of the element
        /// </summary>
        public GaussianDrillingProperty? BodyOD { get; set; } = new GaussianDrillingProperty();
        /// <summary>
        /// the body ID of the element
        /// </summary>
        public GaussianDrillingProperty? BodyID { get; set; } = new GaussianDrillingProperty();
        /// <summary>
        /// Description of the joint connection thread
        /// </summary>
        public string? ConnectionType { get; set; }
        /// <summary>
        /// Material grade
        /// </summary>
        public string? Grade { get; set; }
        /// <summary>
        /// Average material density of the section
        /// </summary>
        public GaussianDrillingProperty? MaterialDensity { get; set; } = new GaussianDrillingProperty();
        /// <summary>
        /// Material Young modulus
        /// </summary>
        public GaussianDrillingProperty? YoungModulus { get; set; } = new GaussianDrillingProperty();
        /// <summary>
        /// Average linear weight of the section
        /// </summary>
        public GaussianDrillingProperty? LinearWeight { get; set; } = new GaussianDrillingProperty();
        /// <summary>
        /// Average tensile strength of the section
        /// </summary>
        public GaussianDrillingProperty? TensileStrength { get; set; } = new GaussianDrillingProperty();
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
        /// possible side ports that connect to additional circuitry
        /// </summary>
        public List<SideConnector> SideConnectors { get; set; }
        /// <summary>
        /// Default Constructor
        /// </summary>
        public SurfaceSection()
        {

        }
        /// <summary>
        /// Realize method of the factory pattern
        /// </summary>
        /// <returns></returns>
        public SurfaceSectionRealization Realize()
        {
            SurfaceSectionRealization realization = new SurfaceSectionRealization()
            {
                Type = Type,
                SectionLength = SectionLength.Value.Realize(),
                BodyOD = BodyOD.Value.Realize(),
                BodyID = BodyID.Value.Realize(),
                ConnectionType = ConnectionType,
                Grade = Grade,
                MaterialDensity = MaterialDensity.Value.Realize(),
                YoungModulus = YoungModulus.Value.Realize(),
                LinearWeight = LinearWeight.Value.Realize(),
                TensileStrength = TensileStrength.Value.Realize(),
                BurstPressure = BurstPressure.Value.Realize(),
                CollapsePressure = CollapsePressure.Value.Realize(),
                YieldStress = YieldStress.Value.Realize(),
                MakeUpTorqueRecommended = MakeUpTorqueRecommended.Value.Realize()
			};
            if (SideConnectors != null)
            {
                realization.SideConnectors = new List<SideConnectorRealization>();
                foreach (var sideConnector in SideConnectors)
                {
                    realization.SideConnectors.Add(sideConnector.Realize());
                }
            }
            return realization;
        }
    }
}
