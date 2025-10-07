# WellBoreArchitecture Solution

This repository delivers a full stack for managing wellbore architecture data: a domain model, a REST microservice, a generated client library, automated tests, and a MudBlazor front-end. Everything targets .NET 8 and relies on the NORCE/OSDC drilling libraries for probabilistic properties and unit handling.

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
- Containers: Dockerfiles are provided for `Service` and `WebApp`, published under the Digiwells organization (`wellborearchitecturewellborearchitecture` images).
- Orchestration: `WebApp/charts/norcedrillingwellborearchitecturewebappclient` and service charts (in sibling repositories) model Kubernetes deployments, ingress rules, secrets, and environment variables.
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
