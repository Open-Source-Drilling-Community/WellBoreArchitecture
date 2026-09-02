# OSDC.Drilling.WellBoreArchitecture.WebPages

Reusable Razor class library for the WellBoreArchitecture web UI.

It contains the `WellBoreArchitectureMain` page, the architecture editor panels, dependent section editors/components, helper utilities, and the feature JavaScript asset.

## Package contents

- Wellbore architecture list and editor pages, including identity and feature assignments
- User-managed identity and feature-category catalog pages
- Backup/restore page for all architectures or a selected set, including safe conflict and catalogue-mapping policies
- Surface, casing, fluid, side connector, and well head editor components
- Shared API/configuration and conversion helpers
- Plotly-based visualization components

## Dependencies

- `OSDC.DotnetLibraries.Drilling.WebAppUtils`
- `MudBlazor`
- `OSDC.UnitConversion.DrillingRazorMudComponents`
- `Plotly.Blazor`
- `ModelSharedOut`

## Host integration

The consuming app should:

1. Reference this package.
2. Provide an implementation of `IWellBoreArchitectureWebPagesConfiguration`.
3. Register that configuration and `IWellBoreArchitectureAPIUtils` in DI.
4. Add the `WebPages` assembly to the Blazor router `AdditionalAssemblies`.
5. Reference the static asset script `_content/OSDC.Drilling.WellBoreArchitecture.WebPages/js/wellbore-architecture.js`.

The backup page dynamically loads `_content/OSDC.Drilling.WellBoreArchitecture.WebPages/wellBoreArchitectureBatchBackup.js` to download the versioned JSON document. Razor class-library static assets require no additional host registration.

## Required configuration

- `FieldHostURL`
- `ClusterHostURL`
- `WellHostURL`
- `RigHostURL`
- `WellBoreHostURL`
- `WellBoreArchitectureHostURL`
- `UnitConversionHostURL`
- `EarthVerticalDatumHostURL` for the current stateless Earth Vertical Datum API

## Mean-sea-level depth references

The architecture pages call the stateless Earth Vertical Datum conversion API through `WellBoreArchitectureAPIUtils` and use `MslDepthReferenceUtils` and the shared reference-source helpers to display mean-sea-level depth references consistently. If that downstream service is temporarily unavailable, the editor remains usable and omits only the mean-sea-level reference. This package uses `OSDC.DotnetLibraries.Drilling.WebAppUtils` 1.1.3.
