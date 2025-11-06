# OneLake Test Configuration

This document describes how to configure and run tests for the Fabric.Mcp.Tools.OneLake project.

## Test Coverage

The test suite includes comprehensive coverage for OneLake MCP commands:

- **OneLakeWorkspaceListCommandTests** - Workspace listing functionality
- **PathListCommandTests** - File system path navigation  
- **BlobListCommandTests** - Blob storage operations
- **OneLakeItemListCommandTests** - OneLake item enumeration
- **OneLakeItemListDfsCommandTests** - OneLake DFS API item enumeration
- **ItemCreateCommandTests** - Item creation functionality
- **FileReadCommandTests** - File reading operations
- **FileWriteCommandTests** - File writing operations
- **FileDeleteCommandTests** - File deletion operations
- **DirectoryCreateCommandTests** - Directory creation functionality
- **DirectoryDeleteCommandTests** - Directory deletion functionality
- **OneLakeServiceTests** - Testable service architecture patterns

## Unit Tests

Unit tests can be run without any external dependencies. They use mocking to test the command logic and parsing.

### Running Unit Tests

```bash
# Run all unit tests
dotnet test tools/Fabric.Mcp.Tools.OneLake/tests/

# Run specific test class
dotnet test tools/Fabric.Mcp.Tools.OneLake/tests/ --filter "OneLakeWorkspaceListCommandTests"

# Run with coverage
dotnet test tools/Fabric.Mcp.Tools.OneLake/tests/ --collect:"XPlat Code Coverage"
```

## Integration Tests

Integration tests require a live OneLake environment and proper authentication.

### Prerequisites

1. **Authentication**: Ensure you're authenticated to Azure/Fabric:
   ```bash
   az login
   # or use other authentication methods supported by Azure Identity
   ```

2. **Test Environment Variables**: Set the following environment variables:
   ```bash
   # Enable integration tests
   export ONELAKE_INTEGRATION_TESTS=true
   
   # Test workspace and item IDs (replace with your test environment)
   export ONELAKE_TEST_WORKSPACE_ID="your-test-workspace-id"
   export ONELAKE_TEST_ITEM_ID="your-test-item-id"
   ```

### Running Integration Tests

```bash
# Run only integration tests
dotnet test tools/Fabric.Mcp.Tools.OneLake/tests/ --filter "Integration"

# Run all tests including integration
ONELAKE_INTEGRATION_TESTS=true dotnet test tools/Fabric.Mcp.Tools.OneLake/tests/
```

## Test Categories

### Command Tests
- `OneLakeWorkspaceListCommandTests` - Tests workspace listing command
- `PathListCommandTests` - Tests path listing with raw response functionality
- `BlobListCommandTests` - Tests blob listing with raw response functionality
- `OneLakeItemListCommandTests` - Tests OneLake item enumeration
- `OneLakeItemListDfsCommandTests` - Tests OneLake DFS API item enumeration
- `ItemCreateCommandTests` - Tests item creation functionality
- `FileReadCommandTests` - Tests file reading operations
- `FileWriteCommandTests` - Tests file writing operations
- `FileDeleteCommandTests` - Tests file deletion operations
- `DirectoryCreateCommandTests` - Tests directory creation functionality
- `DirectoryDeleteCommandTests` - Tests directory deletion functionality

### Service Tests
- `OneLakeServiceTests` - Demonstrates testable architecture patterns with dependency injection and mocking

### Setup Tests
- `FabricOneLakeSetupTests` - Tests dependency injection and command registration

## Raw Response Testing

The tests specifically validate the raw response functionality you implemented:

1. **Raw JSON Responses** (DFS API): Tests that `--format raw` returns unprocessed JSON
2. **Raw XML Responses** (Blob API): Tests that `--format raw` returns unprocessed XML
3. **Parsing Robustness**: Tests handling of string vs. boolean fields in API responses

## Test Data

Tests use the `TestHelper` class to create mock data:
- `CreateTestWorkspaces()` - Creates sample workspace data
- `CreateTestFileSystemItems()` - Creates sample DFS API response data
- `CreateTestOneLakeFileInfos()` - Creates sample Blob API response data

## Troubleshooting

### Authentication Issues
If integration tests fail with authentication errors:
1. Run `az login` to authenticate
2. Verify you have access to the test workspace/item
3. Check that the workspace and item IDs are correct

### API Errors
If tests fail with 400/404 errors:
1. Verify the test workspace and item exist
2. Check that you have appropriate permissions
3. Ensure the workspace is in an active Fabric capacity

### Test Environment
The integration tests are designed to be non-destructive and only perform read operations.