# WellBoreArchitecture Service

The `Service` project exposes the WellBoreArchitecture domain as a REST API backed by a SQLite database. It is an ASP.NET Core web service targeting `net8.0` and reuses the domain types from the `Model` project.

## Structure
- `Service.csproj` – web SDK project referencing `Model`. NuGet dependencies cover SQLite (`Microsoft.Data.Sqlite`) and Swagger tooling (`Swashbuckle.AspNetCore.*`, `Microsoft.OpenApi`).
- `Program.cs` – bootstraps the web host, sets the base path (`/WellBoreArchitecture/api`), wires dependency injection, configures Swagger UI, and maps controllers.
- `Controllers/WellBoreArchitectureController.cs` – the public API surface; each action delegates to the manager layer and returns domain models (`Model.WellBoreArchitecture`, `WellBoreArchitectureLight`, etc.).
- `Managers/SqlConnectionManager.cs` – singleton managing the SQLite database lifecycle (table creation, schema checks, backup of incompatible databases).
- `Managers/WellBoreArchitectureManager.cs` – singleton handling CRUD operations and serialization/deserialization of the domain objects.
- `Managers/DatabaseCleanerService.cs` – `BackgroundService` that prunes records older than 90 days every 24 hours.
- `SwaggerMiddlewareExtensions.cs` – middleware helpers that serve a merged OpenAPI document and adjust server URLs for reverse-proxy scenarios.
- `JsonSettings.cs` – centralizes the `System.Text.Json` options so models keep their C# casing and enums are rendered as strings.

## Runtime data
By default the service stores data in `..\home\WellBoreArchitecture.db` (relative to the service project). The `SqlConnectionManager` ensures the folder exists, validates schema, and creates backups when migrations are required. Most operations open short-lived SQLite connections for thread safety.

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
The service itself does not include unit tests, but the `ServiceTest` project validates API interactions using the generated shared client.

## Operational tips
- Ensure file-system write access to `..\home` for SQLite database creation and backups.
- Regenerate the shared client (`ModelSharedOut`) after modifying controllers or DTOs to keep the generated schema in sync.
- Keep swagger contract up to date by running a Debug build (or invoke the `CreateSwaggerJson` MSBuild target manually) whenever the API changes.
