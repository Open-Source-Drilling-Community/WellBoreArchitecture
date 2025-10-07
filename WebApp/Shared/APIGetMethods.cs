using System.Text.Json;
using NORCE.Drilling.WellBoreArchitecture.ModelShared;
using SharpYaml.Tokens;

public class GuidHandler
{
    public static string CheckGuidExistance(Guid? idToCheck, Dictionary<Guid, string>? idToNameDictExtracted)
    {
        if (idToCheck == null)
        {
            return $"No value assigned";
        }
        else
        {
            if (idToCheck == new Guid())
            {
                return $"No value assigned";
            }
            if (idToNameDictExtracted == null)
            {
                return "Guid empty";
            }
            if (idToNameDictExtracted.ContainsKey((Guid)idToCheck))
            {
                if (idToNameDictExtracted[(Guid)idToCheck] != null)
                {
                    return idToNameDictExtracted[(Guid)idToCheck];
                }
                else
                {
                    return "";
                }
            }
            else
            {
                return "Name unavailable";
            }
        }
    }

}
