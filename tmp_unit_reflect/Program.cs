using System.Reflection;

var dirs = new[]
{
    @"C:\Users\erca\.nuget\packages\osdc.unitconversion.conversion\3.3.21\lib\net8.0",
    @"C:\Users\erca\.nuget\packages\osdc.unitconversion.conversion.unitsystem\3.3.24\lib\net8.0",
    @"C:\Users\erca\.nuget\packages\osdc.unitconversion.conversion.drillingengineering\3.3.22\lib\net8.0",
    @"C:\Users\erca\.nuget\packages\osdc.unitconversion.conversion.unitsystem.drillingengineering\3.3.21\lib\net8.0"
};
AppDomain.CurrentDomain.AssemblyResolve += (_, e) =>
{
    var name = new AssemblyName(e.Name).Name + ".dll";
    foreach (var dir in dirs)
    {
        var path = Path.Combine(dir, name);
        if (File.Exists(path)) return Assembly.LoadFrom(path);
    }
    return null;
};
var asm = Assembly.LoadFrom(@"C:\Users\erca\.nuget\packages\osdc.unitconversion.conversion.unitsystem.drillingengineering\3.3.21\lib\net8.0\ConversionUnitSystem.DrillingEngineering.dll");
var t = asm.GetExportedTypes().Single(x => x.FullName == "OSDC.UnitConversion.Conversion.UnitSystem.DrillingEngineering.DrillingUnitSystem");
var method = t.GetMethods().First(m => m.Name == "FromSIString" && m.GetParameters().Length == 2);
var paramType = method.GetParameters()[0].ParameterType;
Console.WriteLine(paramType.FullName);
foreach (var name in Enum.GetNames(paramType).Where(n => n.Contains("Depth") || n.Contains("Diameter")).OrderBy(n => n)) Console.WriteLine(name);
