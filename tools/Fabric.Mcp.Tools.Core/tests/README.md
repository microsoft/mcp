# Fabric.Mcp.Tools.Core.UnitTests

Unit tests for the Fabric Core toolset.

## Test Coverage

- **Commands/ItemCreateCommandTests.cs**: Tests for the `create_item` command
- **FabricCoreSetupTests.cs**: Tests for service registration and command setup

## Running Tests

```powershell
# Run all Core tests
dotnet test

# Run specific test
dotnet test --filter "FullyQualifiedName~ItemCreateCommandTests"
```

## Test Structure

Tests follow the standard MCP pattern:
- Constructor validation
- Command metadata verification
- Option binding tests
- Service interaction tests
- Error handling scenarios
