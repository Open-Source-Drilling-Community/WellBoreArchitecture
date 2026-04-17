using NORCE.Drilling.WellBoreArchitecture.ModelShared;

namespace NORCE.Drilling.WellBoreArchitecture.WebPages.Shared;

public class GuidHandler
{
    public static string CheckGuidExistance(Guid? idToCheck, Dictionary<Guid, string>? idToNameDictExtracted)
    {
        if (idToCheck == null || idToCheck == Guid.Empty)
        {
            return "No value assigned";
        }

        if (idToNameDictExtracted == null)
        {
            return "Guid empty";
        }

        if (idToNameDictExtracted.ContainsKey(idToCheck.Value))
        {
            return idToNameDictExtracted[idToCheck.Value] ?? string.Empty;
        }

        return "Name unavailable";
    }
}
