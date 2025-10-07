using System;
using System.Collections.Generic;
using OSDC.DotnetLibraries.General.Statistics;
using OSDC.DotnetLibraries.General.DataManagement;
using OSDC.DotnetLibraries.Drilling.DrillingProperties;
using System.Text.Json;


namespace NORCE.Drilling.WellBoreArchitecture.Model
{
    public class WellBoreArchitecture
    {
        /// <summary>
        /// a MetaInfo for the WellBore Architecture
        /// </summary>
        public MetaInfo MetaInfo { get; set; } = new MetaInfo();

        public string? Name { get; set; }

        /// <summary>
        /// a description of the data
        /// </summary>
        public string? Description { get; set; }

        /// <summary>
        /// the date when the data was created
        /// </summary>
        public DateTimeOffset? CreationDate { get; set; }

        /// <summary>
        /// the date when the data was last modified
        /// </summary>
        public DateTimeOffset? LastModificationDate { get; set; }
        /// <summary>
        ///  the ID of the wellbore in which this architecture belongs to
        /// </summary>
        public Guid? WellBoreID { get; set; }
        /// <summary>
        /// The well head 
        /// </summary>
        public WellHead WellHead { get; set; } = new WellHead();
        /// <summary>
        /// List of fluids above ground level. The last fluid extend to the ground level
        /// </summary>
        public List<WellBoreArchitectureFluid> FluidsAboveGroundLevel { get; set; } = new List<WellBoreArchitectureFluid>();
        /// <summary>
        /// List of SurfaceSections above the well head sorted top to down
        /// </summary>
        public List<SurfaceSection> SurfaceSections { get; set; } = new List<SurfaceSection>();
        /// <summary>
        /// List of Sections starting from the well head sorted top to down
        /// </summary>
        public List<CasingSection> CasingSections { get; set; } = new List<CasingSection>();

        /// <summary>
        /// default constructor
        /// </summary>
        public WellBoreArchitecture() : base()
        {

        }

        public WellBoreArchitectureRealization Realize()
        {
            WellBoreArchitectureRealization realization = new WellBoreArchitectureRealization()
            {
                WellHead = WellHead.Realize(),
            };
            if (FluidsAboveGroundLevel != null)
            {
                realization.FluidsAboveGroundLevel = new();
                foreach (var fluid in FluidsAboveGroundLevel)
                {
                    realization.FluidsAboveGroundLevel.Add(fluid.Realize());
                }
            }
            if (SurfaceSections != null)
            {
                realization.SurfaceSections = new();
                foreach (var surface in SurfaceSections)
                {
                    realization.SurfaceSections.Add(surface.Realize());
                }
            }
            if (CasingSections != null)
            {
                realization.CasingSections = new();
                foreach (var casing in CasingSections)
                {
                    realization.CasingSections.Add(casing.Realize());
                }
            }
            return realization;
        }
        public bool Calculate()
        {
            return (this.SurfaceSections.Count > 0);
        }
    }
}
