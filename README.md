# WellBoreArchitecture Solution

This repository delivers the `OSDC.Drilling.WellBoreArchitecture` stack: a domain model, REST and MCP service, generated client library, automated tests, and MudBlazor front-end. Everything targets .NET 8 and uses the OSDC drilling libraries for probabilistic properties and unit handling.

## Repository layout
| Project | Description | Depends on |
| --- | --- | --- |
| `Model/` | Core domain entities (probabilistic + deterministic realizations) used across the stack. | OSDC packages |
| `ModelTest/` | NUnit tests validating the model layer. | `Model` |
| `Service/` | ASP.NET Core API exposing CRUD and calculations over the model with SQLite persistence. | `Model` |
| `ServiceTest/` | NUnit test host interacting with a running service via the shared client. | `ModelSharedOut` |
| `ModelSharedOut/` | Tooling to generate the shared client (`WellBoreArchitectureMergedModel.cs`) from the service OpenAPI document using NSwag. | `Service` (swagger output) |
| `WebApp/` | Blazor Server UI for interacting with the service and related microservices. | `ModelSharedOut`, external APIs |
| `home/` | Runtime folder where the service stores `WellBoreArchitecture.db` (SQLite). Created automatically. |
| `.github/`, `charts/` | CI/CD workflows and Helm charts for Kubernetes deployment. |

Each project ships with its own README (e.g., `Model/README.md`, `Service/README.md`, `WebApp/README.md`) describing internals, build commands, and operational notes.

## Quick start
```powershell
# Restore everything
dotnet restore

# Build the solution
dotnet build WellBoreArchitecture.sln

# Run model and service tests
dotnet test ModelTest/ModelTest.csproj
dotnet test ServiceTest/ServiceTest.csproj   # requires the service to be running or configured endpoint

# Launch the API (creates sqlite database under ./home)
dotnet run --project Service/Service.csproj

# Launch the Blazor Server UI (configure service URLs via appsettings or env vars)
dotnet run --project WebApp/WebApp.csproj
```

## Generated client workflow
1. Build the service in Debug to trigger the MSBuild target `CreateSwaggerJson`. It exports `WellBoreArchitectureFullName.json` into `ModelSharedOut/json-schemas`.
2. Run the `ModelSharedOut` tool (see `ModelSharedOut/README.md`) to produce `WellBoreArchitectureMergedModel.cs`.
3. Commit the updated client so both `WebApp` and `ServiceTest` stay aligned with the REST contract.

## Deployment overview
- Containers: Dockerfiles are provided for `Service` and `WebApp`; CI publishes `digiwells/osdcdrillingwellborearchitectureservice` and `digiwells/osdcdrillingwellborearchitecturewebappclient` with semantic-version, `stable`, and commit-SHA tags.
- Orchestration: `Service/charts/osdcdrillingwellborearchitectureservice` and `WebApp/charts/osdcdrillingwellborearchitecturewebappclient` contain the Kubernetes deployments and ingress configuration.
- Base paths: Service is hosted at `/WellBoreArchitecture/api`; WebApp is served under `/WellBoreArchitecture/webapp`.

Public environments:
- Dev API swagger: https://dev.digiwells.no/WellBoreArchitecture/api/swagger
- Prod API swagger: https://app.digiwells.no/WellBoreArchitecture/api/swagger
- Dev webapp: https://dev.digiwells.no/WellBoreArchitecture/webapp/WellBoreArchitectureCase
- Prod webapp: https://app.digiwells.no/WellBoreArchitecture/webapp/WellBoreArchitectureCase

## Security notes
- Authentication and authorization are not enforced by default. Protect deployments behind gateways or add identity providers when required.
- SQLite files in `home/` are stored in clear text. Back up and secure them according to your data governance policies.
- Several helper clients bypass TLS certificate validation for development (`APIUtils.SetHttpClient`). Review before production use.

## Documentation
- Domain model API docs can be generated with DocFX (`Model/docfx.json`).
- The merged OpenAPI document served by the microservice powers the generated client and Swagger UI.
- For background on related microservices and deployment scripts, see https://github.com/NORCE-DrillingAndWells/DrillingAndWells/wiki.

## Contributing
1. Clone the repository and create a feature branch.
2. Update relevant project README files if you change architecture or workflows.
3. Keep tests green (`dotnet test`). Add coverage for new logic.
4. If you touch the API contract, regenerate `ModelSharedOut` and update the WebApp configuration if endpoints change.
5. Submit a pull request for review.

## Funding
The current work has been funded by the [Research Council of Norway](https://www.forskningsradet.no/) and [Industry partners](https://www.digiwells.no/about/board/) through the centre for research-based innovation [SFI Digiwells (2020-2028)](https://www.digiwells.no/) focused on Digitalization, Drilling Engineering and GeoSteering.

## Contributors
- Eric Cayeux, NORCE Energy Modelling and Automation
- Gilles Pelfrene, NORCE Energy Modelling and Automation
- Lucas Volpi, NORCE Energy Modelling and Automation

## Current implementation

- The service exposes the non-statistics WellBoreArchitecture REST operations, dependency-closed batch export/restore, bounded search, granular details/link/assignment mutations, identity/feature catalogue tools, and `ping` through MCP; usage-statistics endpoints are excluded.
- MCP is available over streamable HTTP at `/wellborearchitecture/api/mcp` and WebSocket at `/wellborearchitecture/api/mcp/ws`. Optional MCP-hub registration is disabled by default.
- MCP tools provide strict input/output schemas, safety annotations, optimistic concurrency for every architecture mutation, and detailed schemas for wellheads, ordered surface/casing construction, fluids, side circuits, open-hole geometry, enums, and Gaussian/scalar drilling-property wrappers. The contract documents external `WellBoreID` references, SI units, wellhead-relative casing depths, caller-generated UUIDs, replacement updates, and the required non-empty surface-section list.
- The UI integrates Vertical Datum data for mean-sea-level depth references.
- The management menu includes a Backup / Restore page for versioned JSON backups of all or selected architectures. Restore uses explicit conflict/catalogue policies and one SQLite transaction, without a database-schema migration.
- Embedded WebPages dependencies are aligned to Field 1.0.19, Cluster 1.0.12, Cartographic Projection 1.0.8, Geodetic Datum 1.0.7, Well 1.0.11, and WellBore 1.0.12.

## OSDC identity and database safety

All WellBoreArchitecture-owned namespaces, generated contracts, Razor assets, package metadata, Docker images, workflows, and Helm chart identities use `OSDC.Drilling.WellBoreArchitecture`. The public HTTP base paths remain `/wellborearchitecture/api` and `/wellborearchitecture/webapp`, so existing external URLs do not change solely because of the identity migration.

The SQLite filename and persistent-volume mount remain `WellBoreArchitecture.db` under `/home`. A valid unversioned legacy database is adopted in place by adding only a schema-version marker and missing index; rows and serialized documents are not rewritten. Unknown, malformed, empty-versioned, or newer schemas abort startup without dropping or rebuilding tables. The former automatic 90-day deletion service has been removed.

Before a Kubernetes cutover, take and verify a snapshot of `wellborearchitecture-claim`, scale the legacy service deployment to zero so two SQLite writers cannot overlap, and use the existing claim through `persistence.existingClaim=wellborearchitecture-claim` if installing under a new Helm release. The service chart uses the `Recreate` strategy and marks newly managed PVCs with `helm.sh/resource-policy: keep`.
