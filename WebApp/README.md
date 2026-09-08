# WellBoreArchitecture WebApp

The WebApp project is the .NET 8 Blazor Server host for the reusable `OSDC.Drilling.WellBoreArchitecture.WebPages` library. It provides the application shell, dependency injection, navigation, route discovery, calculator wrappers, environment configuration, and container/Helm packaging.

## Structure

- `WebApp.csproj` - references the local `WebPages` project and the external OSDC contextual-data/calculator WebPages packages.
- `Program.cs` - configures Blazor Server, `IHttpClientFactory`, MudBlazor, the WebPages API services, forwarded headers, and `/wellborearchitecture/webapp` as the path base.
- `WebPagesHostConfiguration.cs` - implements the configuration interfaces required by the local and external Razor libraries.
- `ExternalWebPagesServiceCollectionExtensions.cs` - registers API helpers required by embedded Field, Cluster, Rig, Well, WellBore, and Earth pages.
- `ExternalRazorAssemblies.cs` - supplies route-bearing Razor assemblies to the Blazor router.
- `Pages/` - contains the app home, three local calculator route wrappers, and the server-side host/error pages. Architecture pages and components live in `../WebPages/`.
- `Shared/NavMenu.razor` - provides Home, WellBore Architecture Management, Import/Export, Contextual Data, Calculators, and Monitoring groups.
- `charts/` - Helm chart `osdcdrillingwellborearchitecturewebappclient`.
- `wwwroot/` - host CSS, Bootstrap, favicon, and third-party Open Iconic assets.

## Service dependencies

- The local `WebPages` library compiles the generated `ModelSharedOut` client aligned with the service REST contract. Regenerate it whenever the API changes.
- The service is expected at `/WellBoreArchitecture/api`; its host is configured through `WellBoreArchitectureHostURL`.
- External Field, Cluster, Rig, Well, WellBore, Trajectory, Unit Conversion, Cartographic Projection, Earth Geodesy, Earth Gravity, Earth Magnetic Field, and Earth Vertical Datum services provide contextual data, conversions, and calculators.

## Configuration

- `appsettings.json` – baseline configuration used in all environments.
- `appsettings.Development.json`, `appsettings.Production.json` – environment-specific overrides for API hosts, logging, and tracing.
- Environment variables can override the host settings, including `FieldHostURL`, `ClusterHostURL`, `RigHostURL`, `WellHostURL`, `WellBoreHostURL`, `WellBoreArchitectureHostURL`, `TrajectoryHostURL`, `UnitConversionHostURL`, and the `Earth*HostURL` settings. Kubernetes production resolves Trajectory through `http://osdctrajectoryservice/`.
- `Program.cs` applies `UsePathBase("/wellborearchitecture/webapp")`; keep ingress rules in sync with this base path.

## Build and run locally

```powershell
# Restore dependencies (ensure ModelSharedOut is built beforehand)
dotnet restore WebApp/WebApp.csproj

# Launch the Blazor Server host
dotnet run --project WebApp/WebApp.csproj
```
The app serves the UI on the standard ASP.NET Core ports. Configure `WellBoreArchitectureHostURL` (and other URLs as required) so the UI can reach the backing services during local development.

The collapsed **Import/Export** menu includes **Backup / Restore**. It exports all architectures or a selected set and restores a version-1 JSON backup. Restore defaults to stopping on an existing UUID and mapping only compatible existing catalogue definitions; replacement and creation of missing definitions must be selected explicitly. The service commits the complete restore in one transaction.

## Docker and Helm packaging

- The Dockerfile builds the `digiwells/osdcdrillingwellborearchitecturewebappclient` image published by the repository workflow.
- `charts/osdcdrillingwellborearchitecturewebappclient` contains Helm manifests; adjust `values.yaml` (ingress path, URLs, secrets) before deploying.

## Hosted environments

- Dev Swagger UI: https://dev.digiwells.no/WellBoreArchitecture/api/swagger
- Prod Swagger UI: https://app.digiwells.no/WellBoreArchitecture/api/swagger
- Dev WebApp: https://dev.digiwells.no/WellBoreArchitecture/webapp/WellBoreArchitecture
- Prod WebApp: https://app.digiwells.no/WellBoreArchitecture/webapp/WellBoreArchitecture

## Testing

No dedicated automated tests ship with this project. If you need regression coverage, add Playwright/Selenium suites in a sibling project and execute them against local or deployed environments.

## Funding

The current work has been funded by the [Research Council of Norway](https://www.forskningsradet.no/) and [Industry partners](https://www.digiwells.no/about/board/) in the framework of the centre for research-based innovation [SFI Digiwells (2020-2028)](https://www.digiwells.no/) focused on Digitalization, Drilling Engineering and GeoSteering.

## Contributors

- Eric Cayeux, NORCE Energy Modelling and Automation
- Gilles Pelfrene, NORCE Energy Modelling and Automation
- Lucas Volpi, NORCE Energy Modelling and Automation

## Current integrations

The host uses OSDC Field, Cluster, Rig, Well, and WellBore page packages for contextual data. The contextual menu intentionally excludes Cartographic Projection, Geodetic Datum, and Spheroid management pages. Cartographic conversion, Earth Vertical Datum, Earth Gravity, and Earth Magnetic Field remain available under Calculators. Local wrapper routes prevent the Earth packages' own home routes from conflicting with the application home page.

Keep development and production settings aligned with their respective deployments.
