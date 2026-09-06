# ModelSharedOut

This .NET 8 console project merges the OpenAPI documents in `json-schemas/`, generates the client-facing C# contract, and publishes the merged OpenAPI document used by the service Swagger UI.

The merged client and generated pseudo-constructors use the `OSDC.Drilling.WellBoreArchitecture.ModelShared` namespace. Regenerate both artifacts after an API or namespace change; do not hand-edit the merged client.

The generator deliberately emits `List<T>` for arrays and array responses because the existing Razor editors perform indexed insertion/removal and bind directly to list properties. Keep `ArrayType`, `ArrayInstanceType`, and `ResponseArrayType` aligned when updating NSwag.

The generated contract includes batch backup/restore and the single/paged WellBore external-reference diagnostics, together with their request, response, catalogue dependency, policy, and structured-error DTOs.

## Generated outputs

- `WellBoreArchitectureMergedModel.cs` - generated API clients and DTOs in `OSDC.Drilling.WellBoreArchitecture.ModelShared`.
- `PseudoConstructors.cs` - generated helpers that construct initialized DTO graphs.
- `../Service/wwwroot/json-schema/WellBoreArchitectureMergedModel.json` - merged OpenAPI document served by the service.

The generator is interactive when outputs already exist. Run it from the solution or project directory and answer `Y` only after confirming that the schema inputs are current:

```powershell
dotnet build Service/Service.csproj -c Debug
dotnet run --project ModelSharedOut/ModelSharedOut.csproj
dotnet build WellBoreArchitecture.sln
```

Do not hand-edit generated outputs. Review their diff after generation because every WebApp and test consumer compiles against these files.

## Current schema input

`json-schemas/WellBoreArchitectureFullName.json` is exported from the service by its Debug build target. `json-schemas/VerticalDatumModel.json` supplies the Vertical Datum types used by WebPages for mean-sea-level display references.

Trajectory is not a ModelSharedOut schema dependency of WellBore Architecture;
only its host WebApp carries the shared Trajectory service URL for composed pages.

Regenerate the shared output after changing this schema or the service REST contract, then rebuild its consumers to verify compatibility.
