# WellBoreArchitecture WebApp

The WebApp project is a Blazor Server UI that lets engineers browse, create, and edit wellbore architecture models exposed by the `Service` microservice. It targets `net8.0`, uses MudBlazor for the component library, and consumes the generated API client published in `ModelSharedOut`.

## Structure
- `WebApp.csproj` – ASP.NET Core Web project referencing `ModelSharedOut`, `Plotly.Blazor`, and related UI packages.
- `Program.cs` – configures Blazor Server, MudBlazor services, forwarded headers, and the application base path `/WellBoreArchitecture/webapp`. It also loads external endpoint URLs into `Configuration.cs`.
- `Configuration.cs` – static storage for service endpoints (field, cluster, well, wellbore, wellbore architecture, unit conversion). Values come from `appsettings.*.json` or environment variables.
- `Pages/` – main UI flows:
  - `HomePage.razor` – entry point listing cases.
  - `WellBoreArchitectureMain.razor`, `...AddPanel.razor`, `...EditPanel.razor` – CRUD workspace around the wellbore architecture model.
  - `WellHeadEditor.razor` plus `CasingSectionPages/`, `SurfaceSectionPages/`, `FluidPropertiesPages/`, `SideConnectorPages/` – detailed editors for each part of the model.
  - `_Host.cshtml`, `_Layout.cshtml`, `Error.*` – server-side hosting shell and error views.
- `Components/` – reusable pieces (`ScatterPlot.razor`, `DialogDeleteTemplate.razor`, `InputHeader.razor`).
- `Shared/` – helper classes and API plumbing:
  - `APIUtils.cs`, `APIGetMethods.cs` – configure `HttpClient` instances and provide strongly typed clients generated from `ModelSharedOut`.
  - `ConversionsFromOSDC.cs`, `DataUtils.cs`, `SharedFunctions.cs` – convert units, build defaults, and apply shared calculations.
- `charts/` – Helm chart (`norcedrillingwellborearchitecturewebappclient`) used to deploy the container on Kubernetes.
- `wwwroot/` – static assets (MudBlazor themes, CSS, JS, favicon).

## Service dependencies
- `ModelSharedOut` – generated client aligned with the REST contract from the `Service` project. Regenerate the shared client whenever the microservice API changes.
- `Service` – expected to be reachable at `/WellBoreArchitecture/api`; host name is set via configuration (`WellBoreArchitectureHostURL`).
- Optional external microservices (Field, Cluster, Well, WellBore, UnitConversion) provide complementary data and are also configured through the static `Configuration` class.

## Configuration
- `appsettings.json` – baseline configuration used in all environments.
- `appsettings.Development.json`, `appsettings.Production.json` – environment-specific overrides for API hosts, logging, and tracing.
- Environment variables (`FieldHostURL`, `ClusterHostURL`, `WellHostURL`, `WellBoreHostURL`, `WellBoreArchitectureHostURL`, `UnitConversionHostURL`) can override JSON settings. Helm templates populate these when deploying to Kubernetes.
- `Program.cs` applies `UsePathBase("/WellBoreArchitecture/webapp")`; keep ingress rules in sync with this base path.

## Build and run locally
```powershell
# Restore dependencies (ensure ModelSharedOut is built beforehand)
dotnet restore WebApp/WebApp.csproj

# Launch the Blazor Server host
dotnet run --project WebApp/WebApp.csproj
```
The app serves the UI on the standard ASP.NET Core ports. Configure `WellBoreArchitectureHostURL` (and other URLs as required) so the UI can reach the backing services during local development.

## Docker and Helm packaging
- The Dockerfile builds the container image `wellborearchitecturewellborearchitecturewebappclient`, published under the Digiwells organization on Docker Hub.
- `charts/norcedrillingwellborearchitecturewebappclient` contains Helm manifests; adjust `values.yaml` (ingress path, URLs, secrets) before deploying.

## Hosted environments
- Dev Swagger UI: https://dev.digiwells.no/WellBoreArchitecture/api/swagger
- Prod Swagger UI: https://app.digiwells.no/WellBoreArchitecture/api/swagger
- Dev WebApp: https://dev.digiwells.no/WellBoreArchitecture/webapp/WellBoreArchitectureCase
- Prod WebApp: https://app.digiwells.no/WellBoreArchitecture/webapp/WellBoreArchitectureCase

## Testing
No dedicated automated tests ship with this project. If you need regression coverage, add Playwright/Selenium suites in a sibling project and execute them against local or deployed environments.

## Funding
The current work has been funded by the [Research Council of Norway](https://www.forskningsradet.no/) and [Industry partners](https://www.digiwells.no/about/board/) in the framework of the centre for research-based innovation [SFI Digiwells (2020-2028)](https://www.digiwells.no/) focused on Digitalization, Drilling Engineering and GeoSteering.

## Contributors
- Eric Cayeux, NORCE Energy Modelling and Automation
- Gilles Pelfrene, NORCE Energy Modelling and Automation
- Lucas Volpi, NORCE Energy Modelling and Automation
