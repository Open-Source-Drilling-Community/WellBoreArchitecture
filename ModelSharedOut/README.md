# ModelSharedOut

This project generates client-facing contracts from the WellBoreArchitecture service and its external schema inputs.

The merged client and generated pseudo-constructors use the `OSDC.Drilling.WellBoreArchitecture.ModelShared` namespace. Regenerate both artifacts after an API or namespace change; do not hand-edit the merged client.

The generator deliberately emits `List<T>` for arrays and array responses because the existing Razor editors perform indexed insertion/removal and bind directly to list properties. Keep `ArrayType`, `ArrayInstanceType`, and `ResponseArrayType` aligned when updating NSwag.

The generated contract includes batch backup/restore and the single/paged WellBore external-reference diagnostics, together with their request, response, catalogue dependency, policy, and structured-error DTOs.

## Current schema input

`json-schemas/VerticalDatumModel.json` provides the generated Vertical Datum types used by the WebPages project for mean-sea-level depth references.

Regenerate the shared output after changing this schema or the service REST contract, then rebuild its consumers to verify compatibility.
