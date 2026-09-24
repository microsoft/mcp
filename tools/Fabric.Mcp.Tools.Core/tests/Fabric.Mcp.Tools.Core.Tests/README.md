# Fabric.Mcp.Tools.Core.Tests

Unit tests and offline MCP integration tests for the Fabric Core toolset.

## Test Coverage

- **Commands/ItemCreateCommandTests.cs**: Tests for the `create-item` command
- **Commands/CatalogSearchCommandTests.cs**: Tests for the `search-catalog` command
- **Commands/ItemGetCommandTests.cs**: Tests for `get-item` validation, metadata output, cancellation, and sanitized failures
- **Services/FabricCoreServiceTests.cs**: Mock HTTP tests for string-ID validation and normalization, metadata requests, response validation, failure status, valid and malformed retry headers, authentication, cancellation, and existing create/search behavior
- **FabricCoreSetupTests.cs**: Tests for service registration, command setup, and registered MCP tool execution with real Core services and substituted HTTP and credentials

## Running Tests

```powershell
# Run all Core tests from the repository root
dotnet test --project tools/Fabric.Mcp.Tools.Core/tests/Fabric.Mcp.Tools.Core.Tests/Fabric.Mcp.Tools.Core.Tests.csproj

# Run only the item metadata command tests
dotnet test --project tools/Fabric.Mcp.Tools.Core/tests/Fabric.Mcp.Tools.Core.Tests/Fabric.Mcp.Tools.Core.Tests.csproj --filter-class '*ItemGetCommandTests'

# Run registration and offline MCP integration tests
dotnet test --project tools/Fabric.Mcp.Tools.Core/tests/Fabric.Mcp.Tools.Core.Tests/Fabric.Mcp.Tools.Core.Tests.csproj --filter-class '*FabricCoreSetupTests'
```

The tests use mocked HTTP responses and token credentials. They do not contact Fabric or require a
signed-in account. Registered-tool tests cover schemas, content-only, compact and duplicated output,
HTTP-mode tool availability, sanitized failures with retry guidance, and rejection before
authentication. They exercise the MCP handlers, not a running stdio or HTTP server. Hosted
on-behalf-of authorization is not covered. Live or recorded verification requires an approved
Fabric fixture and is not provided by this test project.

## Test Structure

Tests follow the standard MCP pattern:
- Constructor validation
- Command metadata verification
- Option binding tests
- Service interaction tests
- Error handling scenarios
