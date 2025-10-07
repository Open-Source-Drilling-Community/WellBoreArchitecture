using NORCE.Drilling.WellBoreArchitecture.ModelShared;
public class Conversions()
{
    public GaussianDrillingProperty doubleToGaussian(double? val)
    {
        if (val == null) { val = 0.0; }

        GaussianDrillingProperty gaussValue = new GaussianDrillingProperty()
        {
            GaussianValue = new GaussianDistribution() { Mean = val }
        };
        return gaussValue;
    }
    public ScalarDrillingProperty doubleToScalar(double? val)
    {
        if(val == null){ val = 0.0; }  
        ScalarDrillingProperty scalValue = new ScalarDrillingProperty()
        {
            DiracDistributionValue = new DiracDistribution()
            {
                Value = val, 
                MaxValue = (double) val, 
                MinValue = (double)val
            }
        };
        return scalValue;
    }

}