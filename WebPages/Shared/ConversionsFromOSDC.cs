using NORCE.Drilling.WellBoreArchitecture.ModelShared;

namespace NORCE.Drilling.WellBoreArchitecture.WebPages.Shared;

public class ConversionsFromOSDC
{
    public static double? ScalarToDouble(ScalarDrillingProperty? val)
    {
        if (val?.DiracDistributionValue?.Value == null)
        {
            return null;
        }

        return val.DiracDistributionValue.Value.Value;
    }

    public static double? GaussianToDouble(GaussianDrillingProperty? val)
    {
        if (val?.GaussianValue?.Mean == null)
        {
            return null;
        }

        return val.GaussianValue.Mean.Value;
    }

    public static ScalarDrillingProperty DoubleToScalar(double? val)
    {
        double maxMinVal = val ?? 0.0;
        return new ScalarDrillingProperty
        {
            DiracDistributionValue = new DiracDistribution
            {
                Value = val,
                MaxValue = maxMinVal,
                MinValue = maxMinVal
            }
        };
    }

    public static GaussianDrillingProperty DoubleToGaussian(double? val)
    {
        return new GaussianDrillingProperty
        {
            GaussianValue = new GaussianDistribution { Mean = val }
        };
    }
}
