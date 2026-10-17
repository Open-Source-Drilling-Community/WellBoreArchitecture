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

Both editor modes expose assignments. The simplified downhole table includes cemented state and Top of cement; the depth entry is enabled only for cemented sections. Both editors use `CasingSectionSizeTable` for per-casing borehole diameters and the nullable aggregate-level `OpenHoleSection` for the final uncased interval. The detailed editor presents these as separate collapsible areas.

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

## Curated architecture semantics (0.10.0)

Extent and host-position inputs use LengthStandard engineering precision. Existing depth-reference selectors and SI persistence remain intact. Simplified connector depth edits update VerticalDepth and the first side-element's TopVerticalDepth without overwriting the independent host-relative Position. Existing Position values are preserved, not retroactively converted; any historical values require an informed review. A newly created connector's local position remains unspecified until provided through its position editor.

## Bounded light discovery

Resource and catalog discovery returns light pages, with `offset` defaulting to 0 and `limit` defaulting to 50 (allowed range 1–200). Names and relevant resource-reference filters are evaluated on the server. Unbounded full-object `HeavyData` routes and tools, including filtered full-object lists, have been removed. Existing UUID and HTTP-metadata listings remain available; use named light pages for routine discovery.

REST routes and MCP tool names (relative to the service's existing API path base):

- `/WellBoreArchitecture/LightData` / `well_bore_architecture_discover`
- `/WellBoreArchitectureFeatureCategory/LightData` / `well_bore_architecture_feature_category_discover`
- `/WellBoreArchitectureIdentity/LightData` / `well_bore_architecture_identity_discover`

Pages contain `Items`, `Offset`, `Limit`, `TotalCount`, and `NextOffset`; `NextOffset` is null on the final page. Offset pages are individually consistent reads, not a snapshot spanning requests: concurrent inserts/deletes can change later pages. Refresh discovery when the underlying collection changes. EarthGeodesy's existing search pages instead expose `Items[].Value`, `Offset`, `Limit`, and `TotalCount`.

Single-record reads, result/chunk reads, and deliberate backup/export/restore operations are preserved. Backups can still contain complete resources and should be treated as explicit bulk operations.

This is a breaking REST/MCP contract change: update generated clients, UIs, and external integrations together with the service release. Removed heavy-list names are not compatibility aliases. No persisted engineering shapes, database names, or storage volumes are changed. Historical usage counters are retained; bounded discovery has a separate additive `DiscoveryRequestsPerDay` history in resource services.


Tables/selectors use light discovery. Editors retrieve the selected complete resource; category options load only for chosen or already assigned definitions. Slot context uses paginated light locations, and rig-history context reads the selected wellbore. Backup selection uses discovery while the dedicated export/restore APIs retain full backup semantics.

## Package release 2.0.0

This release packages the current WebPages and generated contracts. Update consuming WebApps to `OSDC.Drilling.WellBoreArchitecture.WebPages` version `2.0.0` after it is available on nuget.org. This is a major update for the bounded light discovery contracts; rebuild consumers together with their updated service clients.

## Trusted NuGet publishing

The `publish-webpages-nuget.yml` workflow authenticates with GitHub OIDC through `NuGet/login@v1`; no long-lived `NUGET_API_KEY` secret is used. Set `NUGET_USER` to the nuget.org profile username as a repository variable (a repository secret is also accepted). The publishing job requests `contents: read` and `id-token: write` and obtains its temporary credential immediately before uploading.

On nuget.org, the package owner must configure a trusted-publishing policy for `Open-Source-Drilling-Community/WellBoreArchitecture`, workflow filename `publish-webpages-nuget.yml`, with permission to push new versions of `OSDC.Drilling.WellBoreArchitecture.WebPages`. Leave the policy environment empty because the workflow does not declare a GitHub environment. Trigger the workflow manually with the intended version or push a `webpages-v*` tag. Already published versions are skipped.

## Package release 2.0.1

Pins OSDC.UnitConversion.DrillingRazorMudComponents 3.4.8 explicitly so embedded unit selectors consume paginated UnitSystem discovery. This replaces the obsolete indirect dependency inherited through WebAppUtils.
