using System.Collections.Generic;

namespace NORCE.Drilling.WellBoreArchitecture.Model
{
    public class OpenHoleSectionRealization
    {
        /// <summary>
        /// the list of hole size of the open hole section
        /// </summary>
        public List<BoreHoleSizeRealization> HoleSizes { get; } = new List<BoreHoleSizeRealization>();
        /// <summary>
        /// default constructor
        /// </summary>
        public OpenHoleSectionRealization()
        {

        }
        /// <summary>
        /// Copy constructor
        /// </summary>
        /// <param name="src"></param>
        public OpenHoleSectionRealization(OpenHoleSectionRealization src)
        {
            if (src != null)
            {
                foreach (var holeSize in src.HoleSizes)
                {
                    HoleSizes.Add(new BoreHoleSizeRealization(holeSize));
                }
            }
        }

    }
}
