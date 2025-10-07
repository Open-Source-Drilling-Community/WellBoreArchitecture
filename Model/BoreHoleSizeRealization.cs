
namespace NORCE.Drilling.WellBoreArchitecture.Model
{
    public class BoreHoleSizeRealization
    {
        /// <summary>
        /// The hole size 
        /// </summary>
        public double? HoleSize { get; set; }
        /// <summary>
        /// The borehole length for which the borehole size is valid
        /// </summary>
        public double? Length { get; set; }
        /// <summary>
        /// default constructor
        /// </summary>
        public BoreHoleSizeRealization() { }
        /// <summary>
        /// copy constructor
        /// </summary>
        /// <param name="src"></param>
        public BoreHoleSizeRealization(BoreHoleSizeRealization src)
        {
            if (src != null)
            {
                HoleSize = src.HoleSize;
                Length = src.Length;
            }
        }
    }
}
