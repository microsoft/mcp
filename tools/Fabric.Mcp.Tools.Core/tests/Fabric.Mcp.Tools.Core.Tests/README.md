# Fabric.Mcp.Tools.Core.Tests

Offline tests for the Fabric Core toolset.

## Test Coverage

- **Commands/CapacityListCommandTests.cs**: Tests for the `list-capacities` contract, metadata, option binding, cancellation, and sanitized failures
- **Services/FabricCoreServiceCapacityListTests.cs**: Offline HTTP tests for single-page requests, opaque continuation tokens, metadata validation, status/Retry-After handling, disposal, concurrent identities, and existing POST regressions
- **Models/CapacityListSerializationTests.cs**: Source-generated capacity output and unchanged continuation information
- **CapacityListToolRegistrationTests.cs**: Registered MCP handler tests with substituted HTTP and credentials, including input/output schemas and structured output modes
- **Commands/ItemCreateCommandTests.cs**: Tests for the `create-item` command
- **Commands/CatalogSearchCommandTests.cs**: Tests for the `search-catalog` command
- **Commands/CapacityGetCommandTests.cs**: Capacity UUID validation, typed output, cancellation forwarding, and sanitized errors
- **Services/CapacityGetServiceTests.cs**: Get Capacity requests, required metadata, forward-compatible values, retry headers, cancellation/disposal, per-request credentials, and create/search regressions with substituted HTTP and credentials
- **CapacityGetMcpTests.cs**: Get Capacity discovery, schemas, read-only filtering, execution, and sanitized errors through the real Core setup, command factory, and MCP handlers
- **FabricCoreSetupTests.cs**: Tests for service registration and command setup
- **Services/FabricCoreServiceBaselineTests.cs**: Direct HTTP tests preserving create/search authentication, JSON bodies, responses, error handling, cancellation, and single-request behavior
- **Services/FabricCoreHttpHelpersTests.cs**: Validated `Retry-After` parsing and continuation-token encoding checked against constructed HTTP request URIs
- **Models/FabricCapacityMetadataTests.cs**: Source-generated five-field capacity serialization/schema and required metadata validation

## Shared Core Building Blocks

`FabricCoreService.SendFabricHttpRequestAsync` performs one authenticated send and returns the response to its caller. The caller owns disposal, status handling, and deserialization. Its default completion option is `ResponseContentRead`; operations that explicitly need headers first can select `ResponseHeadersRead`. Existing create/search operations keep their legacy stream wrapper and error behavior.

`FabricCoreHttpHelpers.GetRetryAfter` returns a single valid nonnegative delta or HTTP date, or `null` for a missing, malformed, or multiple-valued header. It does not interpret status codes, retry, format messages, or treat backend response text as safe output. Keep tool-specific exception types, integer bounds, date support, and uncertain-mutation outcomes in the owning operation/command.

`FabricCoreHttpHelpers.EncodeContinuationToken` is for request query values only. It preserves existing `%HH` escapes and encodes raw spans, including `+`, query delimiters, and invalid percent sequences. Keep normal .NET URI canonicalization (for example, `%41` becomes `A` and `%7e` becomes `~`). Return API tokens unchanged; never use `continuationUri` or endpoint metadata to choose the next request URL. The tests cover the official continuation examples for [capacities](https://learn.microsoft.com/rest/api/fabric/core/capacities/list-capacities), [workspaces](https://learn.microsoft.com/rest/api/fabric/core/workspaces/list-workspaces), and [items](https://learn.microsoft.com/rest/api/fabric/core/items/list-items).

`FabricCapacityMetadata` is the shared capacity contract: required `Guid Id` followed by required string `DisplayName`, `Sku`, `Region`, and `State`. `IsValid` rejects null metadata, an empty ID, and blank strings without closing the string values to enums. A get operation must additionally compare the returned ID with its requested ID. Workspace and item response models remain operation-specific.

`TestSupport/FabricCoreHttpMessageHandler` accepts an asynchronous request/cancellation callback and exposes a thread-safe `CallCount`. Reuse it for offline HTTP tests; inspect or capture request details inside the callback rather than introducing per-tool handler copies. Keep specialized content/cancellation doubles and all operation-specific assertions.

## Running Tests

```powershell
# Run all Core tests
dotnet test --project tools\Fabric.Mcp.Tools.Core\tests\Fabric.Mcp.Tools.Core.Tests\Fabric.Mcp.Tools.Core.Tests.csproj

# Run specific test
dotnet test --project tools\Fabric.Mcp.Tools.Core\tests\Fabric.Mcp.Tools.Core.Tests\Fabric.Mcp.Tools.Core.Tests.csproj --filter-class "*ItemCreateCommandTests"

# Run only Get Capacity command tests
dotnet test --project tools\Fabric.Mcp.Tools.Core\tests\Fabric.Mcp.Tools.Core.Tests\Fabric.Mcp.Tools.Core.Tests.csproj --filter-class "*CapacityGetCommandTests"

# Run capacity listing tests
dotnet test --project tools\Fabric.Mcp.Tools.Core\tests\Fabric.Mcp.Tools.Core.Tests\Fabric.Mcp.Tools.Core.Tests.csproj --filter-class '*CapacityList*'
```

Run these commands from the repository root. These are offline unit tests, not live Fabric, OBO, or recorded/playback validation.

## Test Structure

Tests follow the standard MCP pattern:
- Constructor validation
- Command metadata verification
- Option binding tests
- Service interaction tests
- Error handling scenarios

## Coverage Boundaries

Get Capacity and List Capacities service tests substitute `HttpMessageHandler` and `TokenCredential`. Registered-handler tests exercise the actual Core setup, command factory, service, serialization, and MCP loader, including default/compact/duplicated output modes. Setting the runtime transport to HTTP does not start an HTTP server or exercise OBO authorization.

Live Fabric calls, real OBO authorization, service-principal/managed-identity access, and recorded playback were not exercised. Fabric live/recorded infrastructure is outside this scoped tool change; mocked tests are not evidence of live permissions or playback. No shared authentication, host, or recording-proxy changes are required.
