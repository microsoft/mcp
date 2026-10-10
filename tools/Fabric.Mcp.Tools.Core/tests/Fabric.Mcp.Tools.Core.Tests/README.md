# Fabric.Mcp.Tools.Core.Tests

Offline tests for the Fabric Core toolset.

## Test Coverage

- **Commands/WorkspaceAssignToCapacityCommandTests.cs**: Required GUID validation, submit-only 202/pending receipts, mutating annotations, sanitized errors, and caller cancellation
- **Services/WorkspaceCapacityAssignmentServiceTests.cs**: One body-bearing POST, empty 202 responses, no polling or body reads, long-second retry-header compatibility, cancellation/disposal, and per-invocation credentials
- **Commands/WorkspaceAssignToCapacityMcpTests.cs**: Accepted/pending receipts across all/namespace/single routing and output modes, including the existing single-proxy text fallback
- **WorkspaceAssignToCapacityToolRegistrationTests.cs**: Cumulative Core discovery/schema/read-only invariants and assignment consent and validation through real registration with substituted HTTP and credentials
- **Commands/CapacityListCommandTests.cs**: Tests for the `list-capacities` contract, metadata, option binding, cancellation, and sanitized failures
- **Services/FabricCoreServiceCapacityListTests.cs**: Offline HTTP tests for single-page requests, opaque continuation tokens, metadata validation, status/Retry-After handling, disposal, concurrent identities, and existing POST regressions
- **Models/CapacityListSerializationTests.cs**: Source-generated capacity output and unchanged continuation information
- **CapacityListToolRegistrationTests.cs**: Registered MCP handler tests with substituted HTTP and credentials, including input/output schemas and structured output modes
- **Commands/ItemCreateCommandTests.cs**: Tests for the `create-item` command
- **Services/FabricOperationServiceTests.cs**: Single-read state/result contracts, UUID validation, safe failures, exact URLs, progress/timestamps, JSON-null versus empty results, byte limits, and response/stream disposal
- **Services/FabricOperationRunnerTests.cs**: Hybrid synchronous 200/201 adapters in both modes, explicit result/no-result contracts, early/strict polling, 429/503 backoff, failed/future states, invalid receipts, and bounded pending outcomes
- **Services/FabricCoreServiceItemCreateLroTests.cs**: Real service with substituted HTTP/credentials, one POST, 201/202 handling, cancellation/deadlines during headers/body reads, completed operations with results not retrieved, and concurrent per-call identity isolation
- **FabricOperationMcpTests.cs**: Real registration, command binding, discovery, read-only enforcement, typed schemas, and legacy/compact/duplicated operation/Create Item payloads
- **Commands/ItemDeleteCommandTests.cs**: Registered command validation, explicit hard-delete opt-in, typed confirmation, and sanitized failures
- **Services/FabricCoreServiceItemDeleteTests.cs**: Mocked single-request deletion, omitted/false/true query behavior, empty HTTP 200 responses, pre-authentication validation, status preservation, Retry-After validation, cancellation, and disposal
- **ItemDeleteToolRegistrationTests.cs**: Registered CLI and MCP paths, schemas, output modes, Boolean argument coercions, repeated/valueless option rejection, read-only exclusion, and destructive-operation consent
- **Commands/ItemUpdateCommandTests.cs**: Update validation, omitted/empty descriptions, annotations, safe failures, and cancellation
- **Commands/CatalogSearchCommandTests.cs**: Tests for the `search-catalog` command
- **Services/FabricCoreServiceItemUpdateTests.cs**: Exact PATCH requests, response validation, status preservation, retry headers, cancellation/disposal, concurrent isolation, and create/search regressions
- **Models/ItemUpdateSerializationTests.cs**: Narrow request JSON and metadata-only source-generated output
- **ItemUpdateToolRegistrationTests.cs**: Real Core registration, command factory, and MCP handlers with substituted HTTP/credentials; input/output schemas, output modes, read-only restrictions, and error responses
- **Commands/CapacityGetCommandTests.cs**: Capacity UUID validation, typed output, cancellation forwarding, and sanitized errors
- **Services/CapacityGetServiceTests.cs**: Get Capacity requests, required metadata, forward-compatible values, retry headers, cancellation/disposal, per-request credentials, and create/search regressions with substituted HTTP and credentials
- **CapacityGetMcpTests.cs**: Get Capacity discovery, schemas, read-only filtering, execution, and sanitized errors through the real Core setup, command factory, and MCP handlers
- **Commands/WorkspaceGetCommandTests.cs**: Workspace UUID and endpoint-preference validation, typed output, cancellation, and sanitized errors
- **Services/WorkspaceGetServiceTests.cs**: Get Workspace request/response handling, optional metadata, validated retry headers, cancellation/disposal, and create/search regression coverage with substituted HTTP and credentials
- **WorkspaceGetMcpTests.cs**: Get Workspace discovery/execution through the real Core command factory and MCP handlers
- **Commands/WorkspaceListCommandTests.cs**: Optional input binding, role normalization, read-only metadata, cancellation forwarding, and sanitized failures for `list-workspaces`
- **Services/FabricCoreServiceWorkspaceListTests.cs**: Fixed-endpoint HTTP requests, encoded cursors, single-page pagination, optional metadata, response validation/disposal, cancellation, per-request tokens, and existing POST-operation regression coverage
- **Models/WorkspaceListSerializationTests.cs**: Source-generated result serialization, optional metadata, and independent continuation fields
- **WorkspaceListToolRegistrationTests.cs**: Registered MCP discovery/call handlers, optional input and typed output schemas, read-only filtering, and legacy/compact/duplicated output
- **Commands/ItemListCommandTests.cs**: Input binding, UUID validation, recursive defaults, typed results, cancellation mapping, and sanitized failures for `list-items`
- **Services/ItemListServiceTests.cs**: Substituted-HTTP tests for one-page listing, metadata whitelisting, optional filters, continuation encoding, invalid responses, throttling, cancellation, disposal, and create/search regressions
- **ItemListMcpTests.cs**: Read-only discovery, input/output schemas, and calls through the real Core command factory, loader, and service with substituted HTTP and credentials
- **Commands/WorkspaceCreateCommandTests.cs**: Creation options, length/UUID validation, typed results, annotations, and sanitized failures
- **Services/FabricCoreServiceWorkspaceCreateTests.cs**: Offline request/body omission, capacity/domain assignment, 201/Location handling, response validation, cancellation/disposal, concurrent credentials, no retries, and existing create/search regressions
- **Models/WorkspaceCreateSerializationTests.cs**: Source-generated request/result contracts, optional metadata, and future type/region strings
- **WorkspaceCreateToolRegistrationTests.cs**: Real Core DI/command factory/MCP handler registration, schemas, output modes, read-only restrictions, and Retry-After handling with substituted credentials and HTTP
- **Commands/WorkspaceUpdateCommandTests.cs**: Validation, partial updates, metadata, typed results, and sanitized failures for `update-workspace`
- **Services/FabricCoreServiceWorkspaceUpdateTests.cs**: Mocked PATCH bodies, field limits, response validation, status and retry guidance, cancellation, disposal, concurrent credentials, and existing create/search regressions
- **Models/WorkspaceUpdateSerializationTests.cs**: Source-generated request omission and whitelisted workspace output
- **WorkspaceUpdateToolRegistrationTests.cs**: Real command/service registration and MCP handlers with mocked credentials and HTTP, including output modes, empty-description binding, mutation confirmation, and read-only enforcement
- **Commands/WorkspaceDeleteCommandTests.cs**: Required UUID validation, destructive metadata, typed deletion acknowledgement, and sanitized failures
- **Services/FabricCoreServiceWorkspaceDeleteTests.cs**: Substituted HTTP tests for the exact DELETE request, empty 200 response, status and retry guidance, cancellation, disposal, concurrency, and existing create/search regressions
- **WorkspaceDeleteToolRegistrationTests.cs**: Registered MCP handler tests for consent, read-only discovery/execution restrictions, input/output schemas, and legacy/compact/duplicated output
- **FabricCoreSetupTests.cs**: Tests for service registration and cumulative command setup
- **Services/FabricCoreServiceBaselineTests.cs**: Direct HTTP tests preserving create/search authentication, JSON bodies, responses, error handling, cancellation, and single-request behavior
- **Services/FabricCoreHttpHelpersTests.cs**: Validated `Retry-After` parsing and continuation-token encoding checked against constructed HTTP request URIs
- **Models/FabricCapacityMetadataTests.cs**: Source-generated five-field capacity serialization/schema and required metadata validation

## Shared Core Building Blocks

`FabricCoreCommand` replaces raw parser failures with fixed guidance and reports missing options only when their names exactly match registered required options. Get Workspace, List Items, and List Workspaces tests exercise real option binding with private invalid Boolean values and unknown options, verify sanitized public responses, and assert that the service is not called. Create Item and Catalog Search retain their command-specific validation guidance.

`FabricCoreService.SendFabricHttpRequestAsync` performs one authenticated send and returns the response to its caller. The caller owns disposal, status handling, and deserialization. Its default completion option is `ResponseContentRead`; operations that explicitly need headers first can select `ResponseHeadersRead`. Catalog Search keeps its legacy stream wrapper and error behavior. Create Item uses `ResponseHeadersRead` and the shared LRO dispatcher.

`FabricCoreHttpHelpers.GetRetryAfter` returns a single valid nonnegative delta or HTTP date, or `null` for a missing, malformed, or multiple-valued header. It does not interpret status codes, retry, format messages, or treat backend response text as safe output. Keep tool-specific exception types, integer bounds, date support, and uncertain-mutation outcomes in the owning operation/command.

`FabricCoreHttpHelpers.EncodeContinuationToken` is for request query values only. It preserves existing `%HH` escapes and encodes raw spans, including `+`, query delimiters, and invalid percent sequences. Keep normal .NET URI canonicalization (for example, `%41` becomes `A` and `%7e` becomes `~`). Return API tokens unchanged; never use `continuationUri` or endpoint metadata to choose the next request URL. The tests cover the official continuation examples for [capacities](https://learn.microsoft.com/rest/api/fabric/core/capacities/list-capacities), [workspaces](https://learn.microsoft.com/rest/api/fabric/core/workspaces/list-workspaces), and [items](https://learn.microsoft.com/rest/api/fabric/core/items/list-items).

`FabricCapacityMetadata` is the shared capacity contract: required `Guid Id` followed by required string `DisplayName`, `Sku`, `Region`, and `State`. `IsValid` rejects null metadata, an empty ID, and blank strings without closing the string values to enums. A get operation must additionally compare the returned ID with its requested ID. Workspace and item response models remain operation-specific.

`TestSupport/FabricCoreHttpMessageHandler` accepts an asynchronous request/cancellation callback and exposes a thread-safe `CallCount`. Reuse it for offline HTTP tests; inspect or capture request details inside the callback rather than introducing per-tool handler copies. Keep specialized content/cancellation doubles and all operation-specific assertions.

## Long-running Operations

`FabricOperationRunner` dispatches endpoint-specific synchronous success or accepted 202 responses. An adapter supplies its documented success codes and a typed result reader; an explicitly absent reader declares a no-result API. Create Item accepts only 201/202, while generic tests cover both 200 and 201, with and without results, in both sync modes. An absent result URL or 404 never selects the no-result contract.

Public operation commands and the poller share the same low-level methods in `FabricCoreService.Operations.cs`. Requests continue through the existing authenticated sender; the poller cannot submit mutations or follow arbitrary URLs. `FabricOperationHttp` bounds JSON reads to 1 MiB and reuses the existing validated Retry-After parser. Each returned JSON element outlives its disposed response/stream.

`OperationTestTimeProvider` and its BCL `ITimer` double advance virtual time deterministically. Tests do not sleep for the 3-second early probe, 20-second fallback, or 120-second default budget. Request budget timers cancel authentication, headers, and body reads; caller cancellation is distinct and retains a known operation reference. Late confirmed completion remains completed even when the result cannot be retrieved before the deadline.

All LRO/Create Item coverage is offline. It does not call real Fabric, validate real OBO permissions, deploy resources, run recorded tests, or establish production latency percentiles. Capacity assignment and Catalog Search retain their pre-existing published contracts and operation-specific tests.

## Item Listing

Pagination cases include Fabric's documented `ABCsMTAwMDAwLDA%3D` token/URI pair, raw and escaped delimiters, invalid percent sequences, normal .NET URI unreserved-character normalization, and query-injection prevention. The documented `%3D` remains intact on the wire; raw `+` is encoded as data, not treated as a space. Empty pages retain continuation information, and each invocation performs only one request.

## Workspace Creation

Creation uses the shared authenticated send with `ResponseHeadersRead` and the shared Retry-After parser without changing its operation-specific status, optional Location, or uncertain-outcome handling. It performs one application-level POST with optional existing capacity/domain assignments, never retries, and does not follow returned URLs. Tests reuse `FabricCoreHttpMessageHandler` while retaining the specialized cancellation stream.

## Workspace Updates

Updates retain the shared authenticated send's buffered `ResponseContentRead` default and reuse the shared Retry-After parser only for HTTP 429. The tool-specific throttling exception, status/messages, uncertain-outcome handling, and four-field response stay unchanged. It performs one application-level PATCH without retries or response-URL following; existing HTTP redirects are unchanged. Tests reuse `FabricCoreHttpMessageHandler`, retain `WorkspaceUpdateCancellationContent`, and verify omitted versus explicitly empty descriptions without widening the request to other workspace properties.

## Item Updates

Update Item uses the shared authenticated send with `ResponseHeadersRead`, preserving validation of both returned item/workspace IDs and the metadata-only response. It projects only nonnegative integer deltas up to `int.MaxValue` from the shared Retry-After parser; HTTP dates and out-of-range values remain ignored. Error wording and uncertain-update outcomes remain operation-specific. Tests reuse `FabricCoreHttpMessageHandler` and retain `ItemUpdateCancellationStream`.

Update Item coverage is offline only. Tests never call real Fabric APIs or mutate resources.
Registered-handler tests exercise stdio/HTTP configuration, not actual transports, real OBO authorization,
service-principal/managed-identity access, or recorded playback.

## Item Deletion

Delete Item retains the shared authenticated send's buffered `ResponseContentRead` default and uses `FabricCoreHttpHelpers.GetRetryAfter` without changing its nullable-header fallback, status/error wording, or uncertain-deletion outcomes. Tests reuse `FabricCoreHttpMessageHandler`, capture requests inside callbacks for disposal assertions, and retain `ItemDeleteTrackingContent` and all operation-specific cases.

The optional Boolean safeguard is configured by the `FabricCoreSetup` registration factory before discovery or execution, using one explicit value and command-local validators. It rejects valueless, invalid, repeated, or ambiguous input without changing the shared binder. Missing/null/false never enables hard deletion; the existing MCP coercion boundary is retained.

## Workspace Deletion

Delete Workspace selects `ResponseHeadersRead` on the inherited authenticated sender and reuses `FabricCoreHttpHelpers.GetRetryAfter`, retaining its own throttling exception and unknown-outcome wording. It keeps explicit cancellation checks before authentication and after the response, plus the shared sender's post-credential check before HTTP. Tests reuse `FabricCoreHttpMessageHandler` and retain `WorkspaceDeleteResponseContent` to verify that deletion response bodies are never read.

Workspace deletion tests substitute every credential and HTTP request. They never delete real workspaces. These tests do not establish live Fabric behavior, actual identity permissions, OBO authorization, or recorded playback. Approval to implement or run these offline tests does not authorize a real deletion.

## Running Tests

Run from the repository root using the SDK pinned in `global.json` and Microsoft.Testing.Platform:

```powershell
# Run all Core tests
dotnet test --project tools\Fabric.Mcp.Tools.Core\tests\Fabric.Mcp.Tools.Core.Tests\Fabric.Mcp.Tools.Core.Tests.csproj

# Run assignment command, service, and registered-handler tests
dotnet test --project tools\Fabric.Mcp.Tools.Core\tests\Fabric.Mcp.Tools.Core.Tests\Fabric.Mcp.Tools.Core.Tests.csproj --filter-class '*WorkspaceAssignToCapacity*' '*WorkspaceCapacityAssignment*'

# Run specific test
dotnet test --project tools\Fabric.Mcp.Tools.Core\tests\Fabric.Mcp.Tools.Core.Tests\Fabric.Mcp.Tools.Core.Tests.csproj --filter-class "*ItemCreateCommandTests"

# Run LRO service, polling, command/schema, and Create Item integration tests
dotnet test --project tools\Fabric.Mcp.Tools.Core\tests\Fabric.Mcp.Tools.Core.Tests\Fabric.Mcp.Tools.Core.Tests.csproj --filter-class '*FabricOperation*' '*ItemCreate*'

# Run only Get Capacity command tests
dotnet test --project tools\Fabric.Mcp.Tools.Core\tests\Fabric.Mcp.Tools.Core.Tests\Fabric.Mcp.Tools.Core.Tests.csproj --filter-class "*CapacityGetCommandTests"

# Run capacity listing tests
dotnet test --project tools\Fabric.Mcp.Tools.Core\tests\Fabric.Mcp.Tools.Core.Tests\Fabric.Mcp.Tools.Core.Tests.csproj --filter-class '*CapacityList*'

# Run only Get Workspace command tests
dotnet test --project tools\Fabric.Mcp.Tools.Core\tests\Fabric.Mcp.Tools.Core.Tests\Fabric.Mcp.Tools.Core.Tests.csproj --filter-class "*WorkspaceGetCommandTests"

# Run workspace listing tests
dotnet test --project tools\Fabric.Mcp.Tools.Core\tests\Fabric.Mcp.Tools.Core.Tests\Fabric.Mcp.Tools.Core.Tests.csproj --filter-class "*WorkspaceList*"

# Run only List Items command tests
dotnet test --project tools\Fabric.Mcp.Tools.Core\tests\Fabric.Mcp.Tools.Core.Tests\Fabric.Mcp.Tools.Core.Tests.csproj --filter-class "Fabric.Mcp.Tools.Core.Tests.Commands.ItemListCommandTests"

# Run only Create Workspace command tests
dotnet test --project tools\Fabric.Mcp.Tools.Core\tests\Fabric.Mcp.Tools.Core.Tests\Fabric.Mcp.Tools.Core.Tests.csproj --filter-class "*WorkspaceCreateCommandTests"

# Run only Update Workspace command tests
dotnet test --project tools\Fabric.Mcp.Tools.Core\tests\Fabric.Mcp.Tools.Core.Tests\Fabric.Mcp.Tools.Core.Tests.csproj --filter-class "*WorkspaceUpdateCommandTests"

# Run only Update Item command tests
dotnet test --project .\tools\Fabric.Mcp.Tools.Core\tests\Fabric.Mcp.Tools.Core.Tests\Fabric.Mcp.Tools.Core.Tests.csproj --filter-class '*ItemUpdateCommandTests'
# Run only the Delete Item coverage.
dotnet test --project tools\Fabric.Mcp.Tools.Core\tests\Fabric.Mcp.Tools.Core.Tests\Fabric.Mcp.Tools.Core.Tests.csproj --filter-class "*ItemDelete*"

# Run the workspace deletion command tests
dotnet test --project tools\Fabric.Mcp.Tools.Core\tests\Fabric.Mcp.Tools.Core.Tests\Fabric.Mcp.Tools.Core.Tests.csproj --filter-class "*WorkspaceDeleteCommandTests"
```

Run these commands from the repository root. These are offline unit tests, not live Fabric, OBO, or recorded/playback validation.

Workspace creation tests never create a real workspace. Their in-process MCP handler checks do not establish live Fabric behavior, real OBO authorization, or recorded playback. Live creation requires separate authorization; this project does not add live/recording infrastructure.

Workspace-update tests do not call Fabric or modify real workspaces. Transport configuration in
registered-handler tests is not evidence of real HTTP/OBO authorization, live service behavior, or
recorded playback. No live-test or recording infrastructure is added by this tool.

## Test Structure

Tests follow the standard MCP pattern:
- Constructor validation
- Command metadata verification
- Option binding tests
- Service interaction tests
- Error handling scenarios

## Coverage Boundaries

Get Capacity, List Capacities, Get Workspace, List Workspaces, List Items, Create Workspace, Update Workspace, Update Item, Delete Item, Delete Workspace, and Assign Workspace to Capacity service tests substitute `HttpMessageHandler` and `TokenCredential`. Registered-handler tests exercise the actual Core setup, command factory, service, serialization, and MCP loader, including default/compact/duplicated output modes. Setting the runtime transport to HTTP does not start an HTTP server or exercise OBO authorization. Existing `Fabric.Mcp.Server.Tests` startup tests separately check discovery over local stdio and HTTP transports.

Live Fabric calls, real OBO authorization, service-principal/managed-identity access, and recorded playback were not exercised. Fabric live/recorded infrastructure is outside this scoped tool change; mocked tests are not evidence of live permissions or playback. No shared authentication, host, or recording-proxy changes are required.

Delete Item tests use fake credentials and mocked HTTP handlers; no real Fabric item may be deleted during development or testing. Registration tests use the actual `FabricCoreSetup` factory, not unguarded direct command construction. The optional Boolean requires one explicit CLI value, and repeated/ambiguous occurrences fail before service or authentication. MCP retains the shared argument conversion behavior, including explicitly supplied `"true"` and `["true"]`; tests do not claim strict original JSON-type validation.

Registered MCP checks exercise the in-process tool pipeline with stdio/HTTP runtime settings and text/structured output modes. They are not live transport, real Fabric, real OBO/RBAC, or recorded/playback evidence. The existing host credential bridge is unchanged. ToolDescriptionEvaluator requires separate configuration and remains pending/not run when that configuration is absent. These offline tests do not waive required CI.

## Workspace Capacity Assignment

Assignment uses the shared authenticated sender with `ResponseHeadersRead`, preserving its post-credential cancellation check. Exactly one application-level POST accepts only 202 and returns requested target IDs plus `accepted: true` and `state: "Pending"`. It never requires a response body, claims completion, polls, retries, invents an operation handle, or follows a Location header.

The assignment-specific Retry-After projection remains nullable `long` seconds and ignores dates. Its existing BCL parser accepts deltas up to `int.MaxValue`; returning a `long` does not widen those accepted inputs. The typed header getter can retain the first valid value when multiple values are supplied, whereas the shared helper rejects every multiple-valued header. The local projection preserves that behavior rather than changing the accepted input set. Boundary tests cover raw and typed deltas, dates, and multiple headers.

Tests reuse `FabricCoreHttpMessageHandler` and retain `UnreadableHttpContent`. All new and inherited operation suites remain offline; no actual assignment or other resource call is authorized. The existing single-proxy local-command text-only fallback is unchanged, as are the Item Delete explicit Boolean safeguard and the six-tool read-only Core set.
