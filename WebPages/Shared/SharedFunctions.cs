using OSDC.Drilling.WellBoreArchitecture.ModelShared;

namespace OSDC.Drilling.WellBoreArchitecture.WebPages.Shared;

public class Conversions
{
    public GaussianDrillingProperty doubleToGaussian(double? val)
    {
        val ??= 0.0;

        return new GaussianDrillingProperty
        {
            GaussianValue = new GaussianDistribution { Mean = val }
        };
    }

    public ScalarDrillingProperty doubleToScalar(double? val)
    {
        val ??= 0.0;

        return new ScalarDrillingProperty
        {
            DiracDistributionValue = new DiracDistribution
            {
                Value = val,
                MaxValue = val.Value,
                MinValue = val.Value
            }
        };
    }
}
