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
By default the service stores data in `..\home\WellBoreArchitecture.db` (relative to the service project). Schema version 2 adds identity and feature-category catalog tables in one transaction. Version 0/1 databases retain the existing architecture table and every row unchanged; the migration only creates missing catalog tables/indexes and advances `PRAGMA user_version`. Unexpected, malformed, or newer schemas stop startup without changing data. No background retention process deletes old architectures.

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

The parallel `/WellBoreArchitectureIdentity` and `/WellBoreArchitectureFeatureCategory` resources provide list/get/create/update/delete operations. Updates and deletes require `expectedModifiedUtc`; referenced definitions and referenced feature options cannot be removed. Architecture writes validate assignment IDs, catalog references, options, validity periods, and exclusivity.

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

The service publishes the architecture operations and user-manageable identity/feature catalogs as MCP tools. Catalog mutations use optimistic concurrency and enforce reference integrity; access-statistics operations are deliberately omitted.

Descriptions distinguish compact discovery (`get_all_ids`, `get_all_meta_info`, and `get_all_light`) from complete construction-model retrieval. Create and update publish explicit nested schemas for the wellhead, fluid layers, surface equipment, side-circuit connectivity, casing elements, open-hole size tables, enums, and uncertainty wrappers. `MetaInfo.ID` is caller-owned, update path/body IDs must match, and `WellBoreID` is an external WellBore reference rather than the architecture's own identifier.

Physical values use SI units. Deterministic properties use `DiracDistributionValue.Value`; uncertain properties use `GaussianValue.Mean` and optionally `GaussianValue.StandardDeviation`, with the deviation expressed in the same unit as the mean. Casing `TopDepth` and `TopCementDepth` are metres referenced to the wellhead. Other depth fields must consistently use the caller's configured depth reference because the persisted payload has no field identifying a display reference. Surface and casing collections are ordered top-to-bottom, and at least one `SurfaceSection` is required by the service calculation.

- Streamable HTTP: `/wellborearchitecture/api/mcp`
- WebSocket: `/wellborearchitecture/api/mcp/ws`
- Utility tool: `ping`
- Optional external MCP-hub registration: configured in `appsettings.json`, disabled by default
