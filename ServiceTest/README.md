# WellBoreArchitecture ServiceTest

This project validates the WellBoreArchitecture service API and its MCP surface.

## MCP coverage

- `McpToolRegistrationTests.cs` verifies the eight service REST tools and `ping`, including exclusion of usage-statistics operations.
- The registration tests also guard detailed descriptions, the complete nested write schema, external WellBore references, ordered/required sections, exact enums, uncertainty-wrapper shapes, SI units, depth-reference guidance, and update ID matching.
- `McpServerHttpTests.cs` exercises MCP initialization, tool listing, and representative calls against a running service.

The live HTTP tests require the service at the configured test base URL. Run the suite with `dotnet test ServiceTest/ServiceTest.csproj`.
