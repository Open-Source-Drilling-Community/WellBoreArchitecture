# ModelSharedOut

This project generates client-facing contracts from the WellBoreArchitecture service and its external schema inputs.

The merged client and generated pseudo-constructors use the `OSDC.Drilling.WellBoreArchitecture.ModelShared` namespace. Regenerate both artifacts after an API or namespace change; do not hand-edit the merged client.

The generated contract includes the `BatchExportWellBoreArchitecturesAsync` and `BatchRestoreWellBoreArchitecturesAsync` operations and their versioned backup document, catalogue dependency, policy, response, and structured-error DTOs.

## Current schema input

`json-schemas/VerticalDatumModel.json` provides the generated Vertical Datum types used by the WebPages project for mean-sea-level depth references.

Regenerate the shared output after changing this schema or the service REST contract, then rebuild its consumers to verify compatibility.
