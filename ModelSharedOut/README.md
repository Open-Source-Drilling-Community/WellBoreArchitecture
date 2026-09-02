# ModelSharedOut

This project generates client-facing contracts from the WellBoreArchitecture service and its external schema inputs.

## Current schema input

`json-schemas/VerticalDatumModel.json` provides the generated Vertical Datum types used by the WebPages project for mean-sea-level depth references.

Regenerate the shared output after changing this schema or the service REST contract, then rebuild its consumers to verify compatibility.
