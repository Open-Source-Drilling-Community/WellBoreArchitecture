# WellBoreArchitecture ServiceTest

This project validates the WellBoreArchitecture service API and its MCP surface.

## Database safety coverage

`SqlConnectionManagerSafetyTests.cs` verifies transactional creation, lossless adoption of the valid legacy table, and fail-safe rejection of unexpected, malformed, or newer schemas. The tests assert that marker rows and version metadata remain unchanged when startup is refused.

`WellBoreArchitectureBatchBackupRestoreTests.cs` verifies dependency-closed export, UUID-preserving transactional catalogue creation, explicit consent before normalized-name mapping, architecture restore, and complete rollback when an assignment is invalid. `WellBoreArchitectureComponentIdentityTests.cs` verifies deterministic legacy-ID materialization and duplicate component-ID rejection.

`WellBoreArchitectureExternalReferenceValidatorTests.cs` verifies that unlinked drafts require no dependency call and that linked references distinguish found, missing, and unavailable WellBore-service outcomes.

`Tests.cs` exercises the generated REST client against an in-process service. `ModelTest/Tests.cs` separately covers model calculation behavior.

## MCP coverage

- `McpToolRegistrationTests.cs` verifies all 45 registered tools: architecture reads/search/mutations, the read-only borehole-diameter-at-abscissa evaluator, bounded WellBore-reference validation/audit, granular details/link/assignment/surface-section/casing-section mutations, identity/feature catalogue tools, backup/restore, and `ping`. It also verifies exclusion of usage-statistics operations.
- The registration tests also guard detailed descriptions, strict input and output schemas, safety annotations, optimistic-concurrency tokens, external WellBore references, ordered/required sections, uncertainty-wrapper shapes, SI units, depth-reference guidance, and rejection of unexpected arguments.
- `McpServerHttpTests.cs` exercises MCP initialization, tool listing, and representative calls against the in-process service.

The HTTP fixtures use `WebApplicationFactory`; no external service or listening port is required. Run the suite with `dotnet test ServiceTest/ServiceTest.csproj`.

## Shared resource classification

`ResourceClassificationContractTests` creates an unstarted ASP.NET host and uses the service Swagger configuration to compare schemas and REST paths with `ModelSharedOut/json-schemas/WellBoreArchitectureFullName.json`. It requires no running endpoint or database. The model project also tests stored classification JSON compatibility and typed option adapters.

## Curated architecture semantics (0.20.0)

`SemanticContractTests` checks all 56 engineering bindings for REST/MCP equality, Reviewed bindings, physical quantities, SI units, canonical references, origin-free uncertainties and quantity-neutral shared wrappers. `ResourceClassificationContractTests` includes the semantic filter when comparing live-generated OpenAPI with the checked-in own-service contract. `WellboreRadialProfileEvaluatorTests` covers ordered and coincident boundaries, adjacent materials, symmetric extrema, casing annotations, deepest-shoe selection and invalid geometry rejection.

The service tests include merged/served semantic metadata preservation and the in-process REST and MCP HTTP fixtures.

## Bounded light discovery

Resource and catalog discovery returns light pages, with `offset` defaulting to 0 and `limit` defaulting to 50 (allowed range 1–200). Names and relevant resource-reference filters are evaluated on the server. Unbounded full-object `HeavyData` routes and tools, including filtered full-object lists, have been removed. Existing UUID and HTTP-metadata listings remain available; use named light pages for routine discovery.

REST routes and MCP tool names (relative to the service's existing API path base):

- `/WellBoreArchitecture/LightData` / `well_bore_architecture_discover`
- `/WellBoreArchitectureFeatureCategory/LightData` / `well_bore_architecture_feature_category_discover`
- `/WellBoreArchitectureIdentity/LightData` / `well_bore_architecture_identity_discover`

Pages contain `Items`, `Offset`, `Limit`, `TotalCount`, and `NextOffset`; `NextOffset` is null on the final page. Offset pages are individually consistent reads, not a snapshot spanning requests: concurrent inserts/deletes can change later pages. Refresh discovery when the underlying collection changes. EarthGeodesy's existing search pages instead expose `Items[].Value`, `Offset`, `Limit`, and `TotalCount`.

Single-record reads, result/chunk reads, and deliberate backup/export/restore operations are preserved. Backups can still contain complete resources and should be treated as explicit bulk operations.

This is a breaking REST/MCP contract change: update generated clients, UIs, and external integrations together with the service release. Removed heavy-list names are not compatibility aliases. No persisted engineering shapes, database names, or storage volumes are changed. Historical usage counters are retained; bounded discovery has a separate additive `DiscoveryRequestsPerDay` history in resource services.


Contract checks cover removed heavy routes/tools, bounded pagination and strict invalid-argument rejection, and omission of engineering collections from discovery. Persistence tests exercise ordering, filters, totals, continuation, empty results, and retained selected reads. Run transport tests against isolated test storage.
