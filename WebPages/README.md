# OSDC.Drilling.WellBoreArchitecture.WebPages

This release targets MudBlazor 9.9.0 and the matching OSDC shared web component packages.

Reusable Razor class library for the WellBoreArchitecture web UI.

It contains the `WellBoreArchitectureMain` page, simplified and detailed architecture editors, dependent section editors/components, helper utilities, and static JavaScript assets.

## Package contents

- Wellbore architecture list and simplified/detailed editors, including identity and feature assignments
- User-managed identity and feature-category catalog pages
- Backup/restore page for all architectures or a selected set, including safe conflict and catalogue-mapping policies
- Usage-statistics dashboard with endpoint totals, current-day counts, last-use timestamps, sorting, and refresh
- Surface, casing, fluid, side connector, and well head editor components
- Shared API/configuration and conversion helpers
- Plotly-based visualization components

## Routes

- `/WellBoreArchitecture`
- `/WellBoreArchitectureIdentities`
- `/WellBoreArchitectureFeatures`
- `/WellBoreArchitectureBackupRestore`
- `/StatisticsWellBoreArchitecture`

The consuming host's `PathBase` is not included in these Razor route templates.

## Identity and feature catalogues

Identity definitions and feature categories are persisted, user-manageable catalogues. New databases are seeded with the default identities `NameForPlanning`, `NameForCompanyReporting`, `NameForRegulatoryReporting`, `Nickname`, and `NameForOperationReporting`. Default feature categories are `Lifecycle`, `ApprovalStatus`, `SectionRole`, and `DrillingMethod`; their exclusivity, validity-period behavior, and suggested options are defined by the service seed manager. Users may add, edit, and remove unused definitions through the catalogue pages. The feature page uses the common resource-service compact grid, bulk category/option selection, validation, deletion confirmation, and add/save/reload actions.

Both editor modes expose assignments. The simplified downhole table includes cemented state and Top of cement; the depth entry is enabled only for cemented sections.

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

## Units and depth references

The architecture pages call the stateless Earth Vertical Datum conversion API through `WellBoreArchitectureAPIUtils` and use `MslDepthReferenceUtils` plus shared reference-source helpers to offer mean-sea-level references. All simplified and detailed depth inputs react to the shared unit/depth-reference selection.

The service contract always stores SI metres relative to WGS84. Alternative references such as MSL, RKB, wellhead, ground level, and mud line are UI-only transformations and are converted back to WGS84 before save. If Earth Vertical Datum is temporarily unavailable, the editor remains usable and omits only the mean-sea-level reference. The exact shared-utility version is declared in `WebPages.csproj`.

For rotary-table references, the editor resolves the latest chronological WellBore `RigJob`. A mobile-rig job supplies its own Gaussian drill-floor depth; a Platform Rig job uses the depth owned by the Rig. An authoritative empty history does not infer a rig, while a null history retains the legacy direct-`RigID` and Cluster fallback during the migration period.
