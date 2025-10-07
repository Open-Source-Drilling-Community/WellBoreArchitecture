using NORCE.Drilling.WellBoreArchitecture.ModelShared;
public class ConversionsFromOSDC
{
    public static double? ScalarToDouble(ScalarDrillingProperty? val)
    {

        if (val == null)
        {
            return null;
        }
        else
        {
            if (val.DiracDistributionValue == null)
            {
                return null;
            }
            else
            {
                double? valOut = val.DiracDistributionValue.Value;
                if (valOut != null)
                {
                    return (double)valOut;
                }
                else
                {
                    return null;
                }
            }
        }
    }
    public static double? GaussianToDouble(GaussianDrillingProperty? val)
    {

        if (val == null)
        {
            return null;
        }
        else
        {
            if (val.GaussianValue == null)
            {
                return null;
            }
            else
            {
                double? valOut = val.GaussianValue.Mean;
                if (valOut != null)
                {
                    return (double)valOut;
                }
                else
                {
                    return null;
                }
            }
        }
    }
    public static ScalarDrillingProperty DoubleToScalar(double? val)
    {
       double MaxMinVal = 0.0;
        if (val != null) { MaxMinVal = (double)val; }
        ScalarDrillingProperty scalValue = new ScalarDrillingProperty()
        {
            DiracDistributionValue = new DiracDistribution()
            {
                Value = val,
                MaxValue = MaxMinVal,
                MinValue = MaxMinVal
            }
        };
        return scalValue;
    }
    public static GaussianDrillingProperty DoubleToGaussian(double? val)
    {          
        GaussianDrillingProperty gaussValue = new GaussianDrillingProperty()
        {
            GaussianValue = new GaussianDistribution(){Mean = val}
        };
        return gaussValue;
    }




}