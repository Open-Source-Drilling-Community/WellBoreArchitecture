# WellBoreArchitecture ServiceTest

This project validates the WellBoreArchitecture service API and its MCP surface.

## Database safety coverage

`SqlConnectionManagerSafetyTests.cs` verifies transactional creation, lossless adoption of the valid legacy table, and fail-safe rejection of unexpected, malformed, or newer schemas. The tests assert that marker rows and version metadata remain unchanged when startup is refused.

`WellBoreArchitectureBatchBackupRestoreTests.cs` verifies dependency-closed export, UUID-preserving transactional catalogue creation, explicit consent before normalized-name mapping, architecture restore, and complete rollback when an assignment is invalid. `WellBoreArchitectureComponentIdentityTests.cs` verifies deterministic legacy-ID materialization and duplicate component-ID rejection.

## MCP coverage

- `McpToolRegistrationTests.cs` verifies the architecture REST tools, bounded search, granular details/link/assignment/surface-section/casing-section mutations, identity/feature catalogue tools, and `ping`, including exclusion of usage-statistics operations.
- The registration tests also guard detailed descriptions, strict input and output schemas, safety annotations, optimistic-concurrency tokens, external WellBore references, ordered/required sections, uncertainty-wrapper shapes, SI units, depth-reference guidance, and rejection of unexpected arguments.
- `McpServerHttpTests.cs` exercises MCP initialization, tool listing, and representative calls against a running service.

The live HTTP tests require the service at the configured test base URL. Run the suite with `dotnet test ServiceTest/ServiceTest.csproj`.
