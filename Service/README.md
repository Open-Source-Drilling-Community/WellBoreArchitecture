# WellBoreArchitecture Service

The `Service` project exposes the WellBoreArchitecture domain as a REST API backed by a SQLite database. It is an ASP.NET Core web service targeting `net8.0` and reuses the domain types from the `Model` project.

## Structure

- `Service.csproj` – web SDK project referencing `Model`. NuGet dependencies cover SQLite (`Microsoft.Data.Sqlite`) and Swagger tooling (`Swashbuckle.AspNetCore.*`, `Microsoft.OpenApi`).
- `Program.cs` – bootstraps the web host, sets the base path (`/WellBoreArchitecture/api`), wires dependency injection, configures Swagger UI, and maps controllers.
- `Controllers/WellBoreArchitectureController.cs` – the public API surface; each action delegates to the manager layer and returns domain models (`Model.WellBoreArchitecture`, `WellBoreArchitectureLight`, etc.).
- `Managers/SqlConnectionManager.cs` – singleton managing transactional creation, additive legacy adoption, schema-version checks, and fail-safe refusal of unknown structures.
- `Managers/WellBoreArchitectureManager.cs` – singleton handling CRUD operations and serialization/deserialization of the domain objects.
- `SwaggerMiddlewareExtensions.cs` – middleware helpers that serve a merged OpenAPI document and adjust server URLs for reverse-proxy scenarios.
- `JsonSettings.cs` – centralizes the `System.Text.Json` options so models keep their C# casing and enums are rendered as strings.

## Runtime data

By default the service stores data in `..\home\WellBoreArchitecture.db`, resolved from the process working directory. In the container, `/app/../home` resolves to the persistent `/home` volume. Schema version 2 adds identity and feature-category catalogue tables in one transaction. Version 0/1 databases retain the existing architecture table and every row unchanged; the migration only creates missing catalogue tables/indexes and advances `PRAGMA user_version`. Unexpected, malformed, or newer schemas stop startup without changing data. No background retention process deletes old architectures.

## Interaction with other solution projects

- Depends on `Model` (domain types) and uses their `Realize()` logic before persistence when needed.
- `ModelSharedOut` consumes this service's Swagger output. A post-build target (`CreateSwaggerJson`) runs `dotnet swagger tofile` to export the API descriptor into `../ModelSharedOut/json-schemas/WellBoreArchitectureFullName.json`, which eventually feeds NSwag code generation.
- `ServiceTest` references `ModelSharedOut` to validate the externally generated contract against service behavior.
- `WebApp` (Blazor frontend) uses the `ModelSharedOut` client to call this API and therefore relies on the service being available at `/WellBoreArchitecture/api`.

## Endpoints

All endpoints are relative to `/WellBoreArchitecture/api/WellBoreArchitecture` and are implemented in `Controllers/WellBoreArchitectureController.cs`. Highlights:
- `GET /` – list all architecture IDs.
- `GET /MetaInfo` – metadata for all architectures.
- `GET /{id}` – retrieve a full architecture.
- `GET /LightData` / `GET /HeavyData` – list light or heavy payloads.
- `POST /` – add a new architecture after running `Calculate()`.
- `PUT /{id}` – update an existing architecture with recalculated fields.
- `DELETE /{id}` – remove an architecture.
- `GET /{id}/ExternalReferences` – validate one optional external `WellBoreID`.
- `POST /ExternalReferenceAudit` – validate a bounded page of all or selected records.
- `POST /BatchExport` / `POST /BatchRestore` – export and transactionally restore version-1 backups.

Usage statistics are exposed separately at `/WellBoreArchitectureUsageStatistics`.

The parallel `/WellBoreArchitectureIdentity` and `/WellBoreArchitectureFeatureCategory` resources provide list/get/create/update/delete operations. Updates and deletes require `expectedModifiedUtc`; referenced definitions and referenced feature options cannot be removed. Architecture writes validate assignment IDs, catalog references, options, validity periods, and exclusivity.

New databases are seeded idempotently with the identities `NameForPlanning`, `NameForCompanyReporting`, `NameForRegulatoryReporting`, `Nickname`, and `NameForOperationReporting`, plus the feature categories `Lifecycle`, `ApprovalStatus`, `SectionRole`, and `DrillingMethod`. Concurrent first reads are serialized so each default feature category is created once. These are ordinary persisted definitions: users may add custom entries and may update or remove definitions when referential-integrity rules permit it. Existing databases are never reseeded by deleting or replacing user data.

`POST /BatchExport` exports all architectures or an ordered selection as a dependency-closed, versioned JSON document. `POST /BatchRestore` validates and atomically restores one of those documents with explicit UUID-conflict and catalogue-mapping policies. Exact catalogue UUID matching is the default. Missing definitions created by `MapOrCreateMissing` preserve their source UUIDs; normalized-name mapping across different UUIDs requires the explicit `AllowNormalizedNameMapping` opt-in.

Batch restore does not require or perform a database-schema migration. Catalogue mapping/creation, assignment-reference rewriting, and all architecture inserts or replacements execute in one SQLite transaction. Any validation, mapping, conflict, or storage failure rolls back the complete operation. `FailIfExists` and `MapExisting` are the conservative defaults exposed by the UI; replacing records or creating definitions requires an explicit choice.

Swagger UI is served at `/WellBoreArchitecture/api/swagger` with a merged schema defined in `wwwroot/json-schema/WellBoreArchitectureMergedModel.json`.

## Build and run

```powershell
# Restore dependencies
dotnet restore Service/Service.csproj

# Build (runs the swagger export target in Debug mode)
dotnet build Service/Service.csproj

# Run the web service
dotnet run --project Service/Service.csproj
```

The service listens on the standard ASP.NET Core ports. Reverse proxies should forward the `X-Forwarded-Host` header so the custom Swagger middleware emits correct server URLs.

## Testing

```powershell
dotnet test ServiceTest/ServiceTest.csproj
```
`ServiceTest` contains both self-contained database safety tests and live API/MCP tests using the generated shared client.

## Operational tips

- Ensure file-system write access to `..\home` and independently back up `WellBoreArchitecture.db` before an upgrade.
- Never run two service replicas against the same SQLite file. The Helm chart uses `Recreate`; during the NORCE-to-OSDC cutover, scale the old deployment to zero before starting the new identity.
- Preserve and reuse `wellborearchitecture-claim`. When a new Helm release adopts it, set `persistence.existingClaim=wellborearchitecture-claim` so Helm does not attempt to create or replace the claim.
- Regenerate the shared client (`ModelSharedOut`) after modifying controllers or DTOs to keep the generated schema in sync.
- Keep swagger contract up to date by running a Debug build (or invoke the `CreateSwaggerJson` MSBuild target manually) whenever the API changes.

## MCP server

The service publishes 44 tools: architecture reads/search/mutations, ordered section mutations, user-manageable identity and feature catalogues, backup/restore, external-reference diagnostics, and `ping`. Architecture and catalogue mutations use `expectedModifiedUtc` optimistic concurrency; stale calls return conflict without changing stored data. Access-statistics operations are deliberately omitted.

The MCP contract also publishes `well_bore_architecture_batch_export` and `well_bore_architecture_batch_restore`. They use the same strict version-1 document and transactional implementation as the REST endpoints.

Read-only external-reference diagnostics are available as `GET /WellBoreArchitecture/{id}/ExternalReferences`, `POST /WellBoreArchitecture/ExternalReferenceAudit`, `well_bore_architecture_validate_external_references`, and `well_bore_architecture_audit_external_references`. They check the optional `WellBoreID` against `WellBoreHostURL` without participating in writes. Results distinguish `Valid`, `Invalid`, and dependency `Unavailable`; an intentionally unlinked draft is valid. Audits are deterministic and bounded to 100 records per page.

Descriptions distinguish compact discovery (`get_all_ids`, `get_all_meta_info`, and `get_all_light`) from complete construction-model retrieval. The unbounded `get_all` tool is retained only as a legacy convenience; new callers should use `well_bore_architecture_search`, whose deterministic bounded pages filter by name, WellBore, linked/unlinked state, identity, feature, and modification date. Narrow details, WellBore-link, assignment, surface-section, and casing-section mutations avoid full-document replacement for routine changes. Section add/update/delete/reorder tools address items by `ComponentID` and preserve explicit top-to-bottom ordering.

Create and update publish explicit nested schemas for the wellhead, fluid layers, surface equipment, side-circuit connectivity, casing elements, open-hole size tables, enums, and uncertainty wrappers. Nested construction objects carry stable `ComponentID` values. Legacy documents without them are assigned deterministic IDs when read and persist those IDs on their next normal write; this needs no SQLite migration and does not rewrite existing rows during startup. Creation assigns server-owned `CreationDate` and `LastModificationDate` values and returns the stored architecture, giving the caller its first usable concurrency token without another discovery call. A legacy record missing both timestamps is returned with the effective Unix-epoch revision already used by concurrency comparison, so it can be updated without first provoking a conflict. `expectedModifiedUtc` is an opaque token that callers must echo exactly without reformatting. `MetaInfo.ID` is caller-owned, update path/body IDs must match, and `WellBoreID` is an externally-owned, nullable WellBore reference rather than the architecture's own identifier. Successful MCP envelopes use lowercase `status` and `data`, while architecture and catalogue properties inside `data` retain their declared PascalCase names, exactly matching the output schemas. All tools reject unexpected top-level arguments. The MCP protocol advertises output schemas and read-only, destructive, and idempotent behavior annotations; failed tool calls are returned as MCP errors as well as structured error payloads.

Physical values use SI units. Deterministic properties use `DiracDistributionValue.Value`; uncertain properties use `GaussianValue.Mean` and optionally `GaussianValue.StandardDeviation`, with the deviation expressed in the same unit as the mean. Every depth is persisted in metres referenced to the WGS84 datum. Alternative references such as MSL, RKB, wellhead, or ground level are presentation-only adjustments performed by the web UI and must be converted back to WGS84 before a service write. Surface and casing collections are ordered top-to-bottom. `SurfaceSections` is optional and may be omitted or empty when the architecture has no surface equipment; the wellhead still anchors the downhole construction.

- Streamable HTTP: `/wellborearchitecture/api/mcp`
- WebSocket: `/wellborearchitecture/api/mcp/ws`
- Utility tool: `ping`
- Optional external MCP-hub registration: configured in `appsettings.json`, disabled by default
- WellBore reference checks: configure `WellBoreHostURL`; the Helm chart defaults to the in-cluster `http://osdcwellboreservice/`
