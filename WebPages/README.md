# NORCE.Drilling.WellBoreArchitecture.WebPages

Reusable Razor class library for the WellBoreArchitecture web UI.

It contains the `WellBoreArchitectureMain` page, the architecture editor panels, dependent section editors/components, helper utilities, and the feature JavaScript asset.

## Package contents

- Wellbore architecture list and editor pages
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
5. Reference the static asset script `_content/NORCE.Drilling.WellBoreArchitecture.WebPages/js/wellbore-architecture.js`.

## Required configuration

- `FieldHostURL`
- `ClusterHostURL`
- `WellHostURL`
- `RigHostURL`
- `WellBoreHostURL`
- `WellBoreArchitectureHostURL`
- `UnitConversionHostURL`
