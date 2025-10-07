using System.Collections.Generic;

namespace NORCE.Drilling.WellBoreArchitecture.Model
{
	public class OpenHoleSection
	{
		/// <summary>
		/// the list of hole size of the open hole section
		/// </summary>
		public List<BoreHoleSize> HoleSizes { get; set; } = new List<BoreHoleSize>();
		/// <summary>
		/// default constructor
		/// </summary>
		public OpenHoleSection()
		{

		}

		/// <summary>
		/// Realization method of the factory pattern
		/// </summary>
		/// <returns></returns>
		public OpenHoleSectionRealization Realize()
		{
			OpenHoleSectionRealization realization = new OpenHoleSectionRealization();
			foreach (var holeSize in HoleSizes)
			{
				realization.HoleSizes.Add(holeSize.Realize());
			}
			return realization;
		}
	}
}
