# Fabric.Mcp.Tools.Core.Tests
<!-- cspell:words TMDL pbism -->

Offline tests for the Fabric Core toolset.

## Test Coverage

- **Commands/FolderCreateCommandTests.cs**: Explicit workspace/name/parent binding, additive mutation metadata, typed 201 results, safe errors/logging, and distinct pre-send, uncertain-send, and confirmed-creation guidance
- **Validation/FolderCreateInputTests.cs**: Fabric naming constraints, Unicode character boundaries, invalid surrogate rejection, UUID validation, unchanged names, and no host filesystem rules
- **Services/FolderCreateServiceTests.cs**: Exactly one authenticated POST, parent omission, safe typed metadata, identity checks, absent optional parents, malformed/Unicode/duplicate data, exact byte/depth limits, status/retry preservation, cancellation/disposal, and concurrent caller isolation
- **FolderCreateMcpTests.cs**: Actual registration, schemas and learn/output modes across all/namespace/single and stdio/HTTP, namespace/read-only exclusion, additive creation without elicitation, zero-I/O invalid input, explicit null parents, CLI duplicate/valueless rejection, and confirmed-creation errors through the wire envelope
- **Commands/FolderGetCommandTests.cs**: Explicit workspace/folder UUID binding, read-only remote metadata, typed snapshots, safe read-specific errors/logging, status/retry guidance, and cancellation forwarding
- **Services/FolderGetServiceTests.cs**: One authenticated GET without a body or preflight, matching identities, absent/null parents, unchanged legacy names, unknown-field projection, malformed/Unicode/duplicate data, exact byte/depth budgets, bounded safe errors, authentication/cancellation/disposal, and concurrent caller isolation
- **FolderGetMcpTests.cs**: Actual registration, schema/learn/output modes across all/namespace/single and stdio/HTTP, namespace/read-only catalogs, no disclosure consent, zero-I/O invalid inputs/authentication failures, CLI duplicate/valueless rejection, and shallow JSON-RPC output at the maximum input depth
- **Commands/FolderListCommandTests.cs**: Explicit workspace/root UUIDs, optional recursive/token binding, read-only remote metadata, typed pages, safe errors/logging, and cancellation forwarding
- **Services/FolderListServiceTests.cs**: One GET without a body, filter omission and retention, raw/escaped cursors, absent/null continuation, empty pages, matching continuation context, complete ordered/repeated entries, parents outside the page, malformed/Unicode/duplicate data, exact UTF-8/byte/depth budgets, status/retry guidance, disposal/cancellation, and concurrent caller isolation
- **FolderListMcpTests.cs**: Actual registration and schema/learn/output modes across all/namespace/single, stdio/HTTP, namespace/read-only filtering, no disclosure consent, zero-I/O invalid inputs, explicit continuation, CLI duplicate/valueless rejection, and JSON-RPC envelope serialization
- **Commands/FolderUpdateCommandTests.cs**: Required workspace/folder/name binding, metadata-replacement hints, typed 200 snapshots, safe errors/logging, and distinct pre-send, rejected, uncertain-send, and confirmed-update guidance
- **Validation/FolderUpdateInputTests.cs**: Shared unchanged Create/Update naming rules, exact Unicode character boundaries, invalid surrogate rejection, UUID validation, and no host filesystem rules
- **Services/FolderUpdateServiceTests.cs**: One exact PATCH with only displayName, authoritative snapshots and matching identities, optional parents, malformed/Unicode/duplicate data, exact byte/depth budgets, status/retry preservation, authentication/cancellation/disposal, and concurrent caller isolation
- **FolderUpdateMcpTests.cs**: Actual schemas and learn/output modes across all/namespace/single and stdio/HTTP, namespace/read-only exclusion, standard mutation consent, zero-I/O invalid/authentication/declined-consent cases, CLI duplicate/valueless rejection, and confirmed-update errors through the wire envelope
- **Services/FabricItemDefinitionFileServiceTests.cs**: Lossless file/part round trips, explicit file selection, source mappings, path containment, byte/count budgets, no-overwrite publication, cancellation, failure cleanup, and independent exports
- **Models/FabricItemDefinitionSerializationTests.cs**: AOT-safe definition/part JSON, optional format, and empty payload preservation
- **Services/FabricItemDefinitionInputServiceTests.cs**: Strict source-generated input parsing, mutually exclusive sources, capability checks before file access, shared path/byte limits, arbitrary safe layouts across omitted/recognized/unknown formats, cancellation, and safe errors
- **Options/DefinitionInputOptionsTests.cs**: AOT-aware optional source composition, unchanged flat names/types, operation-neutral descriptions, omitted groups, and independent per-invocation binding
- **Validation/ItemCreateRequestInputTests.cs**: Optional folder UUID normalization, creation-payload/definition/options combinations, exact UTF-8 byte/depth limits, opaque workload values, strict typed label settings, source-generated serialization, omission/default preservation, and cancellation
- **ItemCreateDefinitionMcpTests.cs**: Real registration and substituted HTTP/credentials across all/namespace/single routing, local and HTTP runtime configurations, and output modes; explicit saved-file packaging, HTTP option removal/rejection, creation properties on both input paths, validation before file access, no label-policy fallback, unchanged no-definition creation, and concurrent input isolation
- **Commands/ItemDefinitionGetCommandTests.cs**: Initial/resume option binding, typed content, conservative file-write metadata, sensitive-content classification, pre-I/O validation, and safe failures
- **Services/ItemDefinitionGetServiceTests.cs**: Exact 200/202 contracts, one-POST initiation, one-GET resume, encoded format, generic definition validation, exact JSON and decoded budgets, LRO failure/cancellation, and unchanged 1 MiB defaults
- **Services/FabricItemDefinitionExportServiceTests.cs**: Capability checks before path inspection, independent write-time checks, unchanged parts and cancellation, and safe export errors
- **ItemDefinitionGetMcpTests.cs**: Registration, typed schemas, consent, all/namespace/single routing and output modes, HTTP inline retrieval and export rejection, read-only filtering, lossless exports, collisions, and partial-write cleanup
- **Commands/ItemDefinitionUpdateCommandTests.cs**: Update definition input validation before file access, wait/metadata/options binding, no-result receipts, sanitized errors, and cancellation receipts
- **Services/FabricCoreServiceItemDefinitionUpdateTests.cs**: One normalized POST, complete definition/options preservation, metadata query omission/false/true, `.platform` semantics, empty 200 and accepted 202 handling, no-result polling, deadlines, read retries/failures, cancellation/disposal, and concurrent identity isolation
- **ItemDefinitionUpdateMcpTests.cs**: Real registration across routes, transports, and output modes; inline/saved-file updates, unchanged arbitrary safe layouts and bytes for default/known/unknown formats, format-independent generic guards, HTTP file rejection and independent invocation-time service guard, exact options limits, source/JSON/file failures, metadata opt-in, read-only exclusion, and destructive consent
- **Commands/WorkspaceAssignToCapacityCommandTests.cs**: Required GUID validation, submit-only 202/pending receipts, mutating annotations, sanitized errors, and caller cancellation
- **Services/WorkspaceCapacityAssignmentServiceTests.cs**: One body-bearing POST, empty 202 responses, no polling or body reads, long-second retry-header compatibility, cancellation/disposal, and per-invocation credentials
- **Commands/WorkspaceAssignToCapacityMcpTests.cs**: Accepted/pending receipts across all/namespace/single routing and output modes, including the existing single-proxy text fallback
- **WorkspaceAssignToCapacityToolRegistrationTests.cs**: Assignment schemas, consent, validation, and read-only rejection through real registration with substituted HTTP and credentials
- **Commands/CapacityListCommandTests.cs**: Tests for the `list-capacities` contract, metadata, option binding, cancellation, and sanitized failures
- **Services/FabricCoreServiceCapacityListTests.cs**: Offline HTTP tests for single-page requests, opaque continuation tokens, metadata validation, status/Retry-After handling, disposal, concurrent identities, and existing POST regressions
- **Models/CapacityListSerializationTests.cs**: Source-generated capacity output and unchanged continuation information
- **CapacityListToolRegistrationTests.cs**: Registered MCP handler tests with substituted HTTP and credentials, including input/output schemas and structured output modes
- **Commands/ItemCreateCommandTests.cs**: Tests for the `create-item` command
- **Services/FabricOperationServiceTests.cs**: Single-read state/result contracts, UUID validation, safe failures, exact URLs, progress/timestamps, JSON-null versus empty results, byte limits, and response/stream disposal
- **Services/FabricOperationRunnerTests.cs**: Hybrid synchronous 200/201 adapters in both modes, explicit result/no-result contracts, early/strict polling, 429/503 backoff, failed/future states, invalid receipts, and bounded pending outcomes
- **Services/FabricCoreServiceItemCreateLroTests.cs**: Real service with substituted HTTP/credentials, one POST, 201/202 handling, cancellation/deadlines during headers/body reads, completed operations with results not retrieved, and concurrent per-call identity isolation
- **FabricOperationMcpTests.cs**: Real registration, command binding, discovery, read-only enforcement, typed schemas, and legacy/compact/duplicated operation/Create Item payloads
- **Commands/ItemGetCommandTests.cs**: Get Item UUID and required-option validation, sanitized parser/service errors, typed metadata, and cancellation forwarding
- **Services/FabricCoreServiceItemGetTests.cs**: One normalized authenticated GET, metadata whitelisting, required fields and matching IDs, validated throttling guidance, cancellation/disposal, and per-request credentials
- **ItemGetToolRegistrationTests.cs**: Get Item discovery and schemas, read-only filtering, legacy/compact/duplicated output, and sanitized failures with substituted HTTP and credentials
- **Commands/ItemJobInstanceCancelCommandTests.cs**: Required UUID binding, destructive/idempotent metadata, typed cancellation-request receipts, action-specific uncertainty and permission guidance, and sanitized failures/logging
- **Services/ItemJobInstanceCancelServiceTests.cs**: One normalized POST without a body, ignored response URLs, acceptance preservation, bounded safe error-code extraction, JobAlreadyCompleted, retry guidance, malformed/oversized bodies, cancellation/disposal, and concurrent identity isolation
- **ItemJobInstanceCancelMcpTests.cs**: Real registration, consent, read-only denial, all/namespace/single routes, stdio/HTTP configurations, namespace filters, learn/input/output schemas, separate zero-delay Cancel-to-Get handoff, invalid-input rejection before auth, and registered CLI 202 handling
- **Commands/ItemJobInstanceGetCommandTests.cs**: Required UUID binding, read-only metadata, unsupported-input rejection, cancellation forwarding, safe logging, and HTTP/retry guidance
- **Services/ItemJobInstanceGetServiceTests.cs**: One normalized authenticated GET, all documented/future states, optional verbatim timestamp strings, matching IDs, safe failure fields, exact byte limits, malformed responses, retry headers, cancellation/disposal, and concurrent identities
- **ItemJobInstanceGetMcpTests.cs**: Independent Jobs area registration, stable command identity, typed schemas, read-only discovery, all/namespace/single routes, stdio/HTTP configurations, default/Core-only/Jobs-only/combined filtering, learn schemas, cross-namespace item-ID handoff, output modes, and pre-authentication validation
- **Commands/ItemJobInstanceListCommandTests.cs**: Explicit ID/token binding, read-only metadata, empty-page output, unsupported options, pre-service validation, safe errors/logging, and cancellation forwarding
- **Services/ItemJobInstanceListServiceTests.cs**: One GET per page, absent/null terminal metadata, explicit token continuation with absent/null URI, more than 100 active rows and repeated rows without reordering, shared safe snapshots, invalid continuation targets, exact UTF-8/byte/depth budgets, duplicate/Unicode/malformed responses, safe HTTP/retry guidance, disposal/cancellation, and concurrent identity isolation
- **ItemJobInstanceListMcpTests.cs**: Real registration, input/output/learn schemas, all/namespace/single routes, stdio/HTTP configurations, Jobs-only/Core-only/combined/default filtering, normal/read-only modes, empty/nonempty final pages with absent/null metadata, explicit token continuation, no List consent, explicit List-to-Get handoff, separate Cancel consent, and safe malformed-page failures
- **Commands/ItemJobRunCommandTests.cs**: Explicit context and optional JSON binding, non-idempotent/destructive metadata, accepted receipts, unsupported-option rejection, safe mutation errors/logging, and cancellation forwarding
- **Validation/ItemJobRunInputTests.cs**: Strict object/array JSON, duplicate rejection, required parameter envelope fields, case-insensitive name uniqueness and 256-character boundary, extensible types/opaque values, omission, and exact UTF-8/depth/serialized request budgets
- **Services/ItemJobRunServiceTests.cs**: One normalized authenticated POST, encoded job types, optional body preservation, trusted Location context, accepted receipts without tracking IDs, Retry-After boundaries, no body reads or retries, FeatureNotAvailable without fallback, cancellation/acceptance preservation, disposal, and concurrent per-request identity isolation
- **ItemJobRunMcpTests.cs**: Real registration, consent, schemas, all/namespace/single routes and output modes, stdio/HTTP configurations, namespace filters, reserved router parameters isolation, separate Run-to-Get handoff, read-only exclusion with Get retained, and validation before authentication
- **Commands/ItemScheduleListCommandTests.cs**: Required UUID/job-type binding, unchanged optional cursors, sensitive read-only metadata, source-generated settings, safe failures/logging, and cancellation forwarding
- **Commands/ItemScheduleCreateCommandTests.cs**: Explicit creation binding, sensitive/destructive/non-idempotent metadata, typed 201 results, safe errors/logging, scope guidance, and confirmed-creation versus uncertain-send failures
- **Validation/ItemScheduleInputTests.cs**: Shared Create/Update explicit UTC bounds and Windows time-zone intent, documented configuration bounds, future string types, opaque execution data, omission, Unicode/duplicate checks, and exact UTF-8/serialized/depth budgets
- **Services/ItemScheduleCreateServiceTests.cs**: One normalized authenticated POST, full JSON preservation, optional schedule fields, ignored Location/operation headers, bounded responses/errors, safe status/retry guidance, confirmed creation on body failure, cancellation/disposal, and concurrent caller isolation
- **ItemScheduleCreateMcpTests.cs**: Real registration, explicit Boolean value/no-default guards, schema/learn/output modes, all/namespace/single routing, stdio/HTTP, namespace/read-only filtering, consent, invalid/missing/repeated options with zero authentication/HTTP, and registered CLI 201 handling
- **Services/ItemScheduleListServiceTests.cs**: One normalized GET, encoded job types/tokens, optional schedule fields, full extensible configuration/owner/execution JSON, explicit continuation, absent/null terminal metadata, no count cap or per-row reads, malformed/Unicode/duplicate data, exact bounds, HTTP status/retry guidance, disposal/cancellation, and concurrent identity isolation
- **ItemScheduleListMcpTests.cs**: Actual registration, schemas and learn, all/namespace/single routes, stdio/HTTP configurations and output modes, namespace/read-only filtering, sensitive-read consent, zero-I/O denied consent/invalid input, complete pages, and safe malformed responses
- **Commands/ItemScheduleUpdateCommandTests.cs**: Full update binding, sensitive/destructive metadata, typed 200 output, safe status/logging/retry guidance, and confirmed-update versus uncertain-send errors
- **Services/ItemScheduleUpdateServiceTests.cs**: One exact normalized PATCH, full payload/omission preservation, matching schedule identity, optional response fields, bounded bytes/depth, malformed/Unicode metadata, ignored operation headers, cancellation/disposal, and concurrent caller isolation
- **ItemScheduleUpdateMcpTests.cs**: Explicit Boolean/schema/learn guards, one PATCH across all/namespace/single routes, transports, output modes, and namespace selections; read-only/consent exclusion, missing/invalid/repeated inputs, wrong response identity, and exact retained-output depth with JSON-RPC serialization
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
- **FabricCoreCatalogTests.cs**: Exact Core catalog and read-only membership, singleton command registration, discovery annotations, and schema consistency
- **FabricCoreSetupTests.cs**: Setup identity and configured HTTP transport, proxy, timeout, and redirect behavior
- **Services/FabricCoreServiceBaselineTests.cs**: Direct HTTP tests preserving create/search authentication, JSON bodies, responses, error handling, cancellation, and single-request behavior
- **Services/FabricCoreHttpHelpersTests.cs**: Validated `Retry-After` parsing and continuation-token encoding checked against constructed HTTP request URIs
- **Models/FabricCapacityMetadataTests.cs**: Source-generated five-field capacity serialization/schema and required metadata validation

## Catalog Expectations

`FabricCoreCatalogTests.ExpectedTools` is the single test-owned catalog manifest. When adding a Core
tool, add one entry with its MCP name, command type, and read-only eligibility. The expected normal
and read-only catalogs are derived only from this manifest, never from production registration or
command metadata. Exact comparisons reject missing, unexpected, duplicate, or misclassified tools;
focused regression cases verify those failures in both modes.

Keep each tool's registration tests focused on its own options, input/output schemas, annotations,
operation behavior, validation before I/O, and read-only/consent behavior. Do not copy complete Core
catalogs or tool counts into those tests. `ServerStartupTests` checks actual host transports and
representative Core schemas and annotations, not exhaustive membership or hard-coded totals.

## Folder Lookup

`core_get-folder` requires explicit workspace and folder UUIDs and sends exactly one GET. It checks
both returned IDs, requires a string display name without imposing creation naming rules, and
preserves an available parent UUID without looking it up. Absent/null parent metadata stays omitted.
Unknown fields are validated then dropped; malformed, duplicate, invalid-Unicode, oversized, or
wrong-identity responses fail instead of returning partial/default metadata. Tests cover the 1 MiB
byte limit and 64-level JSON depth boundary, including the final JSON-RPC envelope.

The read-only tool is not secret-bearing and does not elicit disclosure consent. Known HTTP errors
and Retry-After survive unusable optional error metadata, with only safe codes exposed. Tests
substitute HTTP and credentials; real host startup tests cover discovery only, not live Fabric,
OBO/RBAC, or recorded playback. No live fixture is assumed or discovered.

## Shared Core Building Blocks

`FabricCoreCommand` replaces raw parser failures with fixed guidance and reports missing options only when their names exactly match registered required options. Get Workspace, List Items, and List Workspaces tests exercise real option binding with private invalid Boolean values and unknown options, verify sanitized public responses, and assert that the service is not called. Create Item and Catalog Search retain their command-specific validation guidance.

`FabricCoreService.SendFabricHttpRequestAsync` performs one authenticated send and returns the response to its caller. The caller owns disposal, status handling, and deserialization. Its default completion option is `ResponseContentRead`; operations that explicitly need headers first can select `ResponseHeadersRead`. Its optional `onSending` callback runs only after authentication, request construction, and the final pre-send cancellation check, letting a mutation distinguish unsent requests from possible sends without adding another authentication path. Catalog Search keeps its legacy stream wrapper and error behavior. Create Item uses `ResponseHeadersRead` and the shared LRO dispatcher.

`FabricCoreHttpHelpers.GetRetryAfter` returns a single valid nonnegative delta or HTTP date, or `null` for a missing, malformed, or multiple-valued header. It does not interpret status codes, retry, format messages, or treat backend response text as safe output. Keep tool-specific exception types, integer bounds, date support, and uncertain-mutation outcomes in the owning operation/command.

`FabricCoreHttpHelpers.EncodeContinuationToken` is for request query values only. It preserves existing `%HH` escapes and encodes raw spans, including `+`, query delimiters, and invalid percent sequences. Keep normal .NET URI canonicalization (for example, `%41` becomes `A` and `%7e` becomes `~`). Return API tokens unchanged; never use `continuationUri` or endpoint metadata to choose the next request URL. The tests cover the official continuation examples for [capacities](https://learn.microsoft.com/rest/api/fabric/core/capacities/list-capacities), [workspaces](https://learn.microsoft.com/rest/api/fabric/core/workspaces/list-workspaces), and [items](https://learn.microsoft.com/rest/api/fabric/core/items/list-items).

`FabricCapacityMetadata` is the shared capacity contract: required `Guid Id` followed by required string `DisplayName`, `Sku`, `Region`, and `State`. `IsValid` rejects null metadata, an empty ID, and blank strings without closing the string values to enums. A get operation must additionally compare the returned ID with its requested ID. Workspace and item response models remain operation-specific.

`TestSupport/FabricCoreHttpMessageHandler` accepts an asynchronous request/cancellation callback and exposes a thread-safe `CallCount`. Reuse it for offline HTTP tests; inspect or capture request details inside the callback rather than introducing per-tool handler copies. Keep specialized content/cancellation doubles and all operation-specific assertions.

`FabricResponseBodyReader` contains only the bounded byte-reading loop shared by job-instance snapshots, pages,
and existing operation responses. Callers keep their own media, empty-body, parsing, and error contracts.
The existing operation reader still defaults to 1 MiB, and definition retrieval retains its explicit
64 MiB limit. Existing operation/definition tests cover those unchanged contracts.

## Folder Creation

`core_create-folder` sends one POST for an explicit workspace UUID and unchanged display name.
An optional parent UUID is normalized; absent/null input omits the body field and selects the
workspace root. Local validation follows Fabric naming rules, not filesystem rules. Fabric alone
checks uniqueness, supported workspaces, depth, and folder count. There are no lookups or retries.

Only HTTP 201 confirms creation. The typed `FabricFolder` projection checks the workspace and
available parent context without filling missing optional metadata. JSON is bounded to 1 MiB and
64 levels; error-body failures retain the known status and retry guidance. Tests distinguish
validation/authentication/pre-send failures from uncertain sends, and preserve confirmed creation
when response reading, Unicode decoding, validation, or cancellation prevents returning metadata.

Like existing additive Core-create tools, this command is non-destructive and not secret-bearing,
so existing policy does not require elicitation. Read-only and excluded-namespace calls cannot
execute it. Substituted credentials/HTTP and actual host discovery establish local wiring, not
live Fabric permissions, OBO/RBAC behavior, or authorization to create a real folder.

## Folder Listing

`core_list-folders` reads one complete page of logical workspace folder metadata. It preserves
optional `root-folder-id` and `recursive` omission, normalizes explicit UUIDs, and never performs
folder walking, enrichment, lookups, or automatic pagination. Empty pages may still have tokens.
Missing or null terminal continuation fields are accepted; non-null URI-only, unsafe, wrong-type,
or contradictory continuation metadata fails without a partial result. Optional URI filter values
must agree with the requested context; cursor-only metadata is safe because the next explicit call
always uses the caller's workspace/root/recursive options, never the returned URL.

`FabricContinuationToken` is the unchanged Jobs cursor validator shared with folder listing.
`FabricCoreHttpHelpers.ValidateMetadataJson` and `GetMatchingCollectionUri` contain only the existing
Unicode/duplicate-property and collection-address guards extracted from Jobs. Endpoint-specific
paging and filter checks stay with each operation. Jobs instance/schedule list regressions cover
the extraction; no generic pagination engine or existing Core/Jobs contract changes are introduced.

Offline HTTP/credential fixtures cover folder pages, auth failures, cancellation, per-call identities,
and response disposal. The exact Core catalog and actual Fabric host discovery include the new
read-only tool. These tests make no live Fabric calls and do not establish live OBO/RBAC behavior;
that requires separately approved identities and workspace fixtures.

## Job-instance Cancellation

`jobs_cancel-item-job-instance` sends one POST without a body for explicitly supplied, nonempty workspace,
item, and job-instance UUIDs, with no job-type. It does not preflight, discover, poll, wait, follow
response URLs, fetch results, or retry. It reuses the authenticated sender rather than the
`FabricOperationRunner`. A 202 receipt reports `cancellationStatus: "Requested"`, never a final job state.
The known IDs come from the request; Location and operation-ID headers are ignored even when missing,
malformed, multiple, or mismatched. Late cancellation does not erase confirmed acceptance.

The safe error-code reader is limited to 1 MiB, including streams without Content-Length. Tests
exercise the exact boundary, duplicate codes, invalid shapes, depth limits, code-length/character
validation, unpaired Unicode escapes in values and property names, non-JSON media, and failed/canceled reads.
Service and routed MCP cases retain HTTP 429 and Retry-After after lazy Unicode decoding fails,
while unrelated response-reader exceptions are not swallowed. Invalid error metadata retains the known HTTP
failure with an explicit limitation, never raw bodies or stack traces. JobAlreadyCompleted stays a
failure, without inferring success or failure of the job. A backend retry hint never resubmits.
Valid Retry-After seconds/dates are retained; missing, malformed, out-of-range, or repeated values
leave timing explicitly unknown rather than inventing a delay.

Cancel is destructive and excluded in read-only mode. Its idempotent hint describes repeatedly
targeting the same job, not repeated success or automatic retry. Declined/canceled consent,
invalid/missing/unsupported/repeated inputs, authentication failures, and pre-send cancellation
produce zero HTTP calls. Accepted, failed, and metadata-anomaly invocations each send one POST.
The registered CLI retains the standard 202 status. The offline handoff uses a zero-delay receipt:
only after Cancel returns does the test independently call Get, which can still report InProgress
or subsequently report Cancelled. Existing Run/Get and Core/definition/LRO tests remain regressions.

Real host startup tests verify discovery and annotations over stdio/HTTP; substituted request-aware
credentials test per-call isolation, not real OBO/RBAC. No live target or cancellation authorization
was supplied. Live Fabric, recordings/playback, and real OBO/RBAC validation were **not** run;
the tests never start or cancel real jobs or discover fixtures.

## Schedule Deletion

`jobs_delete-item-schedule` requires explicit workspace/item/schedule UUIDs and a job type. It sends
one authenticated DELETE without a body, lookup, preflight, verification, polling, retry, or job
execution/cancellation. Only HTTP 200 returns `deletionStatus: "Deleted"` with the requested context,
not a returned snapshot. Empty success and an unreadable irrelevant body are both valid; Location
and operation headers are ignored. Late cancellation does not erase confirmed deletion.
Deleting a schedule does not cancel running jobs or promise rollback. Cancellation is separately authorized.

`Commands/ItemScheduleDeleteCommandTests.cs` covers binding, metadata, source-generated receipts,
safe errors/logging, and distinct invalid/not-sent/uncertain/confirmed outcomes.
`Services/ItemScheduleDeleteServiceTests.cs` covers the exact method/path and absence of a body, escaping and
UUID normalization, exact 1,024-byte UTF-8 job-type bounds, zero-call invalid/auth/cancellation paths,
single-call success/failure/unknown outcomes, disposal, and concurrent caller isolation. Repeated
deletion returning 404 remains a failure. Optional error metadata retains status and Retry-After
through malformed Unicode, exact 1 MiB boundaries, and failed/canceled reads; it never exposes raw
backend messages or follows retry hints.
`ItemScheduleDeleteMcpTests.cs` covers all/namespace/single routes, stdio/HTTP configurations, output
modes, namespace selection, input/output schemas, mutation consent, read-only catalogs and invocation
denial, and late cancellation. Host startup tests discover the real registered tool without calling Fabric.
Existing Core, Jobs, and operation/definition tests cover the unchanged shared authenticated sender.

The idempotent hint describes repeated effects on the same target, not guaranteed success or a retry
policy. Delegated deletion needs `Item.ReadWrite.All` or its item-specific equivalent, not the
Create/Update execution-scope combination. Offline credentials do not establish real OBO/RBAC behavior.
No live schedule fixture or mutation authorization was supplied. Live Fabric, recordings/playback,
real OBO/RBAC, and resource deployment were not run.

## Job-instance Lookup

`jobs_get-item-job-instance` reads an existing job instance exactly once, independently of the Fabric
LRO protocol. Tests cover every documented status plus future status, invocation, and job-type strings.
The public API and bundled spec declare timing fields as plain UTC strings, so optional timestamps
round-trip without host-local date parsing. Required identity/state fields must be present and valid;
returned job and item UUIDs must match the request. Failure output excludes backend messages and nested
details, retaining only an identifier-shaped code, nonempty request UUID, and optional retry flag.

`FabricJobsSetup` is a real top-level `jobs` area registered in the same server and physical assembly
as the existing Fabric Core infrastructure. The host configures all areas before filtering, so
Jobs-only sessions reuse the Core-configured transport and services without exposing Core commands.
The Core catalog stays unchanged. Tests reject the obsolete unmerged Core alias and wrong-area
routing; Core-only sessions hide and deny Jobs, Jobs-only sessions execute the lookup, and combined
or unfiltered sessions expose both. Namespace/single learn responses advertise only the selected areas'
implemented commands and schemas.

The offline cross-namespace workflow uses `--namespace core --namespace jobs --mode all` (or
`--mode namespace` / `--mode single` with both namespace arguments): a real registered Core item
metadata read hands workspace/item UUIDs to the registered Jobs lookup with a separately supplied
existing job-instance fixture. It asserts one GET per invocation. This is deterministic routing
coverage with substituted HTTP, not a natural-language agent benchmark or live workflow.

Offline tests exercise the real command/service pipeline with substituted HTTP and credentials, plus
real server startup discovery over stdio and HTTP. No existing live job fixture was supplied:
live Fabric calls, recorded/playback validation, and real OBO/RBAC validation have **not** been run.
Tests do not start jobs, deploy resources, or discover a fixture.

## Job-instance Listing

`jobs_list-item-job-instances` uses the real Jobs area, authenticated sender, continuation encoder,
bounded response reader, and source-generated snapshot projection. The only required inputs are
nonempty workspace/item UUIDs. Optional tokens are passed unchanged and limited to 8,192 UTF-8 bytes
of valid Unicode that is not blank and has no control characters before authentication. Tests cover raw,
already-escaped, mixed, Unicode, whitespace-surrounded, and query-delimiter tokens, including the
[documented percent-escaped example](https://learn.microsoft.com/rest/api/fabric/core/job-scheduler/list-item-job-instances).

Each invocation sends exactly one GET and returns one complete `jobInstances` page. No row triggers
an enrichment GET. The caller must explicitly request the next page with the same IDs and returned
token. Tests preserve original ordering and repeated entries, include more than 100 active instances,
and accept empty/nonempty final pages with absent or null continuation metadata. The API's completed-instance listing
limit is not a local page cap, physical retention policy, or full-history guarantee.

Only a token can request another page. The [general Fabric pagination contract](https://learn.microsoft.com/rest/api/fabric/articles/pagination)
allows terminal continuation fields to be absent or JSON null. Service and MCP tests cover every
absent/null combination on empty and nonempty final pages, plus explicit token continuation with an
absent/null URI even from an empty page. No arbitrary wrong-type value is treated as absence.
Response URIs are neither exposed nor followed; non-null URIs must match the fixed Fabric origin,
collection IDs, and token. Non-null URI-only, contradictory, duplicate, or otherwise invalid
continuation metadata is a safe failure, not exhaustion.
Dot-segment representation checks apply only before the query, so raw or escaped `/./` and `/../`
within opaque cursor values remain data. Regression cases preserve those tokens and keep each
explicit request on the same collection while still rejecting genuine path traversal.
Required response inspection rejects malformed rows, invalid Unicode even in omitted fields, and
duplicate/ambiguously cased properties. No partial result is returned. Exact 1 MiB byte and
64-level depth thresholds retain the existing default response budget; token boundaries also test
multibyte UTF-8. Lazy JSON decoding failures are caught only in JSON inspection, not the response reader.
HTTP error bodies remain unread so even malformed error metadata cannot replace known status/retry guidance.

The read-only catalog contains Get Job Instance, List Job Instances, and List Schedules, not Run or Cancel. MCP tests run both read-only
and normal configurations across all routing/output modes and selected namespaces, including
legacy text learn responses. An explicit selected result supplies the IDs for a separate Get;
subsequent Cancel still requests its own consent and sends no mutation when consent is declined.
Neither listing nor selecting the latest row correlates an ambiguous Run receipt or one without a tracking ID.
Offline credentials, concurrency, and HTTP runtime configuration do not establish real OBO/RBAC
authorization. Live/recorded validation is unavailable without an explicitly approved fixture.

## Schedule Creation

`jobs_create-item-schedule` requires workspace/item UUIDs, job type, an explicit Boolean `enabled`,
and configuration JSON; optional execution data is an opaque static object. Registered CLI/MCP
tests reject missing, null, valueless, repeated, and invalid enabled inputs before authentication.
System.CommandLine's implicit false Boolean default is removed for schedule mutation commands only.
Every valid invocation sends exactly one POST, with no schedule-count preflight, job/type discovery,
hidden List/Get, retry, parameter stripping, operation polling, or follow-up execution.

The current public API and bundled common/platform schemas define Cron as an interval in minutes,
not a cron expression. Tests exercise Daily/Weekly/Monthly fields and exact documented ranges,
future configuration/occurrence/string-enum values, opaque extension fields, and case-sensitive
execution-data keys. Input UTC bounds must be ordered; offset-less API examples are UTC, never
machine-local. Timezone IDs are explicit and not looked up on the host. A past start time is valid
and may execute immediately when enabled; descriptions/prompts require explaining this before
the existing destructive/sensitive consent gate. No local calendar simulation is performed.

Schedule JSON validation is shared with List through `FabricItemScheduleJson`, preserving List's
existing optional-field and timestamp contracts. Create and Update share ordered UTC input validation.
The error-code reader extracted to `FabricCoreHttpHelpers.ReadErrorCodeAsync` retains cancellation's
known-status/Retry-After semantics; all affected cancellation and schedule-list tests run alongside
the new tests. No pagination infrastructure or existing read contract is changed.

The 201 result contains a typed schedule; an unusable 201 body reports confirmed creation but
unavailable metadata. Lost confirmation remains uncertain, never an instruction to resubmit.
Location/operation headers are not used. Response/error bodies have the existing 1 MiB budget;
input JSON has a 32-level and response JSON a 64-level bound. Error metadata is optional and cannot
replace a known failure status. Tests cover exact byte/depth thresholds, lazy Unicode decoding,
read cancellation/failures, response disposal, independent identities, and schema/output paths.
Retained schedule output is serialized to a null stream with a 60-level limit inside the confirmed
201 boundary, reserving four levels for result/routing/protocol envelopes. Routed regressions cover
60/61/64-level configuration, owner, and execution data in every output mode and transport, including
serialization and parsing of the JSON-RPC envelope. Excess depth returns confirmed-creation and
do-not-resubmit guidance rather than a late, ambiguous serializer failure. List's limits are unchanged.
No live schedule, resource provisioning, recorded playback, or real OBO/RBAC validation is performed.

## Schedule Updates

`jobs_update-item-schedule` requires explicit workspace/item/schedule UUIDs, job type, enabled state,
and full configuration, even though its single initiating request is PATCH. There is no hidden
Get/List, name lookup, read/merge/PATCH, retry, field removal, polling, result retrieval, or job run.
The same `ItemScheduleInput` validator and existing source-generated request shape serve Create
and Update; their required body contracts are identical. Shared tests cover known and future
configurations, UTC bounds, time-zone intent, opaque execution data, and exact input/serialized
budgets. Service and routed tests confirm that Update invokes these checks before authentication.
Omitted execution data is absent on the wire, `{}` remains an object, and JSON null is invalid.
The API makes no preservation/clearing promise when execution data is omitted.

The 200 snapshot must identify the requested schedule. Tests reject wrong, missing, empty, duplicate,
or malformed IDs without replacing the confirmed update with an ambiguous failure. Optional absent
or null response fields stay absent; no settings are synthesized from the request. Configuration,
owner, and execution data remain extensible and potentially sensitive. An update's owner identifies
its creator or last modifier independently of item ownership. Descriptions require confirming
identity/timing/enablement before consent: an enabled past start may execute immediately, while
disabling is not cancellation.

`FabricItemScheduleJson.ReadResponse` performs the shared 60-level retained-output check
inside each operation's confirmed-success boundary (201 for Create, 200 for Update). Routed tests
exercise exact 60/61/64-level configuration, owner, and execution data across all routes, stdio/HTTP,
and legacy/compact/duplicated output, then serialize and parse the JSON-RPC envelope. Fields that
are not retained keep the 64-level input limit. Exact 1 MiB response thresholds cover streams with and without
Content-Length. Known HTTP errors retain status and Retry-After after unreadable optional metadata;
uncertain sends and late cancellation never cause automatic resubmission.

Create, List, Run/Cancel catalog, shared validation, and actual Fabric host discovery regressions
cover the shared surfaces. The read-only Jobs catalog retains all implemented read tools,
excluding Update along with the other mutations. Tests use existing HTTP/credential fixtures and
do not discover customer IDs, mutate live schedules, deploy resources, or claim live/OBO/RBAC proof.

## Schedule Lookup

`jobs_get-item-schedule` reads exactly one schedule with explicit workspace/item/schedule UUIDs and
job type. It shares `ItemScheduleInput.ValidateScheduleContext` with Update and
`FabricItemScheduleJson.ReadResponse` with Create/Update, without changing their contracts.
No List preflight, discovery, enrichment, pagination, mutation, retry, or polling occurs.

Command/service tests cover exact normalized GET paths, no body/query, safe error/status/retry
guidance, matching identity, required id/enabled, absent/null optional metadata, and preserved
extensible configuration/owner/execution data. Returned configuration has its documented shape,
but does not use creation-time UTC ordering, current-time, or machine-time-zone policy.
The snapshots remain valid after response disposal. Malformed/duplicate metadata, lazy Unicode
decoding, invalid UTF-8, known/unknown Content-Length, 1 MiB and raw 64-level response boundaries,
and 1,024-byte job-type boundaries are covered. Pre-send cancellation and invalid inputs make
zero credential/HTTP calls; failed authentication makes no HTTP call. During-send/body cancellation,
reader failures, response disposal, and concurrent caller/target/sensitive-result isolation are tested.

Routing tests cover all/namespace/single, stdio/HTTP, legacy/compact/duplicated output, namespace
filters, discovery/learn schemas, and normal/read-only operation. They verify exact retained
60/61/64-level boundaries for configuration, owner, and execution data through JSON-RPC serialization.
Get never returns a mutation receipt or late output-serialization failure. Invalid snapshots retain
retry guidance; unreadable optional error metadata cannot replace a known HTTP error status.
Accepted disclosure consent permits one GET; declined/canceled/rejected/unsupported consent makes
no credential/HTTP calls even in read-only mode. Read-only discovery keeps Get Schedule, Get Job
Instance, and both listing tools, while excluding all mutations.

Actual Fabric startup discovery covers the new command and four required string options in stdio
and HTTP. Fixtures reuse `FabricCoreHttpMessageHandler`, existing schedule data, and substituted
credentials. No customer IDs are discovered and no live Fabric, OBO/RBAC, resource deployment, or
recorded playback coverage is claimed without approved fixtures and API-operation authorization.

## Schedule Listing

`jobs_list-item-schedules` sends one GET for explicit workspace/item UUIDs and a workload job type.
It reuses the Run context validator, List Job Instances token validator and continuation metadata
check, authenticated transport, bounded reader, and source-generated JSON context. The continuation
check compares the raw expected path separately from query data, including an escaped, case-sensitive
job type. Existing job-instance listing behavior is covered in the same regression run; no generic
pagination service or automatic page enumeration is introduced.

The current bundled `BaseItemSchedule` and `ItemSchedule` schemas require only `id` and `enabled`.
Tests preserve absent/null creation time, configuration, owner, and optional `executionData`, and
exercise Cron (minute interval), Daily, Weekly, Monthly, and future schedule/occurrence/principal
types. Full nested JSON survives response disposal and source-generated serialization. Opaque
execution-data keys remain case-sensitive. UTC text and Windows time-zone IDs are not converted;
no local scheduling calculations or job-history/count limits are added.

Known configuration shapes retain their published ranges and required fields. Tests cover exact
1 MiB/64-level page budgets, 1,024-byte job types, 8,192-byte cursors, Unicode multibyte boundaries,
documented arrays/ranges, malformed/duplicate metadata, and the lazy Unicode-decoding boundary.
An invalid later row fails the whole page. Unknown top-level row properties are not projected.
Empty/nonempty terminal pages allow absent/null continuation fields; token-only continuation
requires an explicit subsequent invocation. Untrusted or contradictory URI metadata fails safely.
HTTP error bodies remain unread, preserving known status and Retry-After even for malformed errors.

The tool is read-only and available remotely, but `Secret=true` retains the host's sensitive-data
confirmation for owner and execution data. Routing tests accept that confirmation for reads and
prove that declining it sends no authentication or HTTP request, including in read-only mode.
Real server startup tests verify normal/read-only discovery and schemas over stdio and HTTP.
All authorization fixtures remain offline substitutes; no live Fabric, recorded playback, or real
OBO/RBAC result is claimed without an explicitly approved fixture.

## On-demand Job Submission

`jobs_run-on-demand-item-job` submits exactly one POST using the current job-type path route.
It does not use `FabricOperationRunner`, poll, wait, fetch status/results, discover items/jobs, or
resubmit. The optional JSON `execution-data` object and `job-parameters` array map to `executionData`
and `parameters`, with omission and caller values preserved. The distinct option name avoids
collision with the router's outer `parameters` object. Parameter names are case-insensitively unique
and at most 256 characters; the bundled schema requires `name`, `type`, and `value`. Type strings
remain extensible and values remain opaque, including the scalar values in the public examples.
Tests distinguish the JSON text `null` (invalid object/array input) from an RPC option value of JSON
null, which the shared `CommandExtensions` binder intentionally treats as omitted for every command.
That existing conversion contract is preserved; Run does not introduce raw-RPC parsing or change
shared argument binding.

Each input JSON text is limited to 1 MiB UTF-8 and 32 nesting levels, with duplicate properties
rejected throughout. The final serialized request has a separate 1 MiB budget including its envelope.
Job types have a 1,024-byte UTF-8 tool budget; URI-unsafe representations are rejected before encoding.
These are generic resource limits, not workload allowlists or content validation.

A 202 remains `submissionStatus: "Accepted"` even when Location is absent, invalid, multiple,
mismatched, or collection-only. Only a trusted matching job-instance path supplies `jobInstanceId`.
No response URL or failure body is exposed or followed, and `x-ms-operation-id` cannot supply a job ID.
`trackingIssue` and `retryAfterIssue` make incomplete receipts explicit. Valid Retry-After seconds or
dates are retained with guidance to wait at least that long before the first separate Get; there is
no early-poll shortcut or fabricated fallback. A zero-delay offline fixture demonstrates the two
independent calls, asserting one POST before the caller explicitly issues one GET.

Failure tests assert one POST with no retries, including FeatureNotAvailable, throttling, network
failure, and in-flight cancellation. Late cancellation does not erase confirmed response headers.
Input/authentication/pre-send cancellation and blocked invocations produce zero HTTP requests.
Run is destructive, non-read-only, and non-idempotent; read-only mode retains Get Job Instance and both listing tools and denies Run.
The host credential composition is unchanged. Concurrent substituted credentials test isolation,
not real delegated OBO/RBAC. No live execution target or permission was supplied: live, recorded/
playback, and real OBO validation were not run, and these tests never start a Fabric job.

## Long-running Operations

`FabricOperationRunner` dispatches endpoint-specific synchronous success or accepted 202 responses. An adapter supplies its documented success codes and a typed result reader; an explicitly absent reader declares a no-result API. Create Item accepts only 201/202, while generic tests cover both 200 and 201, with and without results, in both sync modes. An absent result URL or 404 never selects the no-result contract.

Public operation commands and the poller share the same low-level methods in `FabricCoreService.Operations.cs`. Requests continue through the existing authenticated sender; the poller cannot submit mutations or follow arbitrary URLs. `FabricOperationHttp` defaults to 1 MiB and reuses the existing validated Retry-After parser. Get Item Definition alone selects a 64 MiB bound for typed definition responses, including polled and explicitly resumed results; state reads and existing result contracts stay at 1 MiB. Each returned JSON element outlives its disposed response/stream.

`OperationTestTimeProvider` and its BCL `ITimer` double advance virtual time deterministically. Tests do not sleep for the 3-second early probe, 20-second fallback, or 120-second default budget. Request budget timers cancel authentication, headers, and body reads; caller cancellation is distinct and retains a known operation reference. Late confirmed completion remains completed even when the result cannot be retrieved before the deadline.

The shared wait options enforce a 600-second server maximum while keeping the 120-second default and positive fractional budgets. Command and service tests reject invalid and over-limit budgets before authentication or POST in both sync modes. Virtual-clock cases verify that waiting for the allowed 600 seconds returns `Pending` with the last-known state and never sends another GET after the deadline; the limit does not cancel Fabric's operation.

All LRO/Create Item coverage is offline. It does not call real Fabric, validate real OBO permissions, deploy resources, run recorded tests, or establish production latency percentiles. Capacity assignment and Catalog Search retain their pre-existing published contracts and operation-specific tests.

## Local Definition File Infrastructure

`IFabricItemDefinitionFileService` is an injectable, stateless file/part converter registered by
`FabricCoreSetup`. It uses ordinary `System.IO`, following Azure's area-specific file services,
without depending on or changing Azure tools. It makes no network requests, exposes no MCP commands,
and is reused by Create Item's saved-file input and Get Item Definition's explicit completed export.

The wire models are `FabricItemDefinition` (`parts`, optional `format`) and
`FabricItemDefinitionPart` (`path`, `payload`, `payloadType`), registered in `CoreJsonContext`.
File I/O supports `InlineBase64` only and preserves bytes, including binary assets, text encodings,
line endings, empty files, nested paths, and `.platform`. Invalid Base64 and unsupported payload
types are errors, never skipped parts.

### Reading Saved Files

`ReadAsync` takes an existing absolute directory, an explicit list of `FabricItemDefinitionFile`
entries, and an optional format. `PartPath` is the logical Fabric path; optional `SourcePath` maps
it to a different relative file under the selected directory. Source paths accept native or `/`
separators, but logical part paths use `/`. The reader never enumerates the directory implicitly:
unselected editor files, local caches, and secrets are not included simply because they share a folder.
It reads saved files, not Git HEAD or unsaved editor buffers, and does not rename or modify them.

```csharp
var definition = await definitionFiles.ReadAsync(
    sourceDirectory,
    [new("artifact.content.ipynb", "notebooks/sales.ipynb")],
    format: "ipynb",
    cancellationToken: cancellationToken);
```

Length and modification-time checks detect changes observed during packaging. This is not an atomic
snapshot of a concurrently edited directory: callers should save their files and stop editing while
packaging. The returned parts are a bounded in-memory snapshot for the subsequent API request.

### Writing Completed Definitions

`WriteAsync` takes a completed definition and an explicit new absolute output directory whose parent
already exists. It validates all part paths and payloads first, writes into a temporary sibling
directory, then moves that completed directory into place. Existing destinations are never merged,
overwritten, or deleted. It returns the published absolute directory; the caller already has the
definition's part list and format for its result. Retain that format when reading the files back.

```csharp
var outputDirectory = await definitionFiles.WriteAsync(
    newOutputDirectory, definition, cancellationToken);
```

Failures and cancellation are propagated rather than returning success for partial output. The writer
attempts to remove only its temporary directory and logs cleanup failures without masking the original
error. Unlike Azure Resiliency's general artifact writer, it must not flatten paths, invent replacement
file names, or fall back silently to an incomplete file set.

### Budgets and Filesystem Boundaries

`FabricItemDefinitionFileLimits` supplies host-level packaging budgets, not MCP options or Fabric API
quotas. Defaults are 1,024 parts, 16 MiB decoded per part, and 32 MiB decoded in total; hosts and tests
can supply smaller or different explicit budgets. Encoded input per part is also bounded to the
Base64 length implied by its decoded budget. Paths are limited to 1,024 characters and 255 characters
per segment. No content is truncated to meet a budget.

Only local absolute directories are accepted. Relative part/source paths cannot escape the selected
directory. Logical paths reject traversal, absolute paths, alternate data streams, device names,
non-portable characters, duplicate names (case-insensitively), and file/directory collisions.
The implementation checks existing path components for symbolic links, junctions, and device paths,
including ancestors of the selected root. Use actual local paths rather than linked directories.
These checks are not an OS sandbox against hostile processes concurrently replacing filesystem
entries; the local filesystem and its permissions must be trusted.

Tools dedicated to disk reads or writes should declare `LocalRequired = true`, reusing the existing
whole-tool HTTP restrictions. Create Item and Update Item Definition remain remote-capable: each registered command omits only
the saved-file options on HTTP hosts, and `FabricItemDefinitionInputService` separately rejects local
input before parsing paths or calling the file service unless the configured transport is stdio.
Command logic and the low-level file converter contain no transport checks. Inline content remains
available over HTTP and uses the same content-independent validator and budgets, with no disk access.
Local file conversion adds no Fabric requests.

### Item Layout and Content Validation Belong to Fabric

The bundled [item-definition articles](https://github.com/microsoft/mcp/tree/main/tools/Fabric.Mcp.Tools.Docs/src/Resources/item-definitions)
and the `docs_item-definitions` tool describe required parts and supported formats for most item
types. Callers can consult those resources to prepare explicit file selections without an extra
Fabric request. They are documentation, not executable schemas or an input-validation policy;
Markdown tables are not parsed at runtime.

This infrastructure validates the generic envelope and filesystem representation only. It does not
claim a supplied part list is complete or valid for a particular item type, discover an item from
its folder name, convert report bindings, apply `.platform` metadata, or resolve dependencies.
The input service has no item-type argument and applies no required-filename, notebook-language,
known/default-format, or format-mixing rules. All definitions receive generic JSON/Base64, path,
count/byte/depth, source-exclusivity, and local-file capability checks. Tests verify that arbitrary
safe layouts, including combinations formerly rejected locally, reach the single Create POST unchanged
for omitted, recognized, and unknown formats. Create still sends its actual API item type unchanged.
Fabric decides whether the supplied layout, format, content, and dependencies are valid.
`.platform` is included only when explicitly selected or supplied inline and is sent unchanged,
not interpreted or synthesized.

### Shared Definition Source Options

`DefinitionInputOptions` groups only four optional inputs: `Definition`, `DefinitionDirectory`,
`DefinitionFiles`, and `DefinitionFormat`. Operations accepting uploaded definitions compose it using
the existing AOT-aware attribute, without an option inheritance hierarchy or a framework change:

```csharp
[OptionContainer<DefinitionInputOptions>(Prefix = "")]
public DefinitionInputOptions? DefinitionInput { get; set; }
```

The empty prefix preserves the existing flat CLI/MCP names and string shapes: `definition`,
`definition-directory`, `definition-files`, and `definition-format`. The group stays null when no source
option was provided. `HasInput` means at least one source option was supplied, including incomplete input
that still needs validation; it does not claim the source is complete. Shared descriptions do not refer
to Create-only fields. Creation payloads, folder IDs, sensitivity labels, API options, and LRO settings
remain owned by Create. An operation that requires a definition must enforce that requirement itself;
the group does not make its children globally required. Read/export operations have a different
contract and should not acquire unused upload options.

`IFabricItemDefinitionInputService.ReadAsync` accepts only the four grouped source values and cancellation:

```csharp
Task<FabricItemDefinition?> ReadAsync(
    string? definition,
    string? directory,
    string? files,
    string? format,
    CancellationToken cancellationToken = default);
```

There is no item-type hint or layout-policy argument. Generic validation and the local-file guard
are independent of any workload. The current
Create registration still removes saved-file options from HTTP discovery without mutating shared binder
options. Registered-handler and actual startup tests verify that composition does not change the schema
or allow remote filesystem access.

The command accepts either inline definition JSON or an absolute selected directory plus explicit
file-to-part mappings and optional format. Unknown/duplicate JSON properties and conflicting source
options are rejected, rather than silently discarded or preferred. It does not enumerate folders.
The create-request definition and label-settings members are typed; the existing response model and
output modes, workspace UUID behavior, no-definition requests, and bounded LRO processing remain unchanged.
Registered-handler tests verify one POST and no hidden reads, while the LRO service matrix covers
with/without definitions and the additional creation properties, synchronous completion, submit-only
acceptance, and explicit waiting.

`FabricItemJsonInput` supplies the shared content-independent bounded object parser for Create Item
and Update Item Definition, with safe `FabricItemInputException` guidance. `ItemCreateRequestInput`
prepares optional folder IDs, creation payloads, definition options, and
sensitivity label settings before definition files can be read. Creation payloads conflict with every
definition option; the API's top-level options object requires a definition and never controls LRO
waiting. Workload JSON remains opaque `JsonElement` data with one-MiB UTF-8 input budgets, 32-level
nesting limits, and duplicate-property rejection, including nested objects. Tests measure the exact
byte boundary with both ASCII and multibyte text, and the exact nesting boundary through request serialization.

Label settings have a separate four-KiB input budget and a source-generated typed model. The entire
object is optional, but every supplied settings object must have a nonempty `labelId` UUID. The only
accepted strategies are `ApplyOrFail` and `Ignore`; the optional strategy stays omitted when not supplied,
preserving Fabric's default. Required-constructor-parameter validation rejects empty and strategy-only
settings before file access, authentication, or HTTP. Explicit nulls, unknown fields, and invalid
strategies are rejected, not silently discarded. Tests
verify that errors cause no second POST, label omission, or strategy downgrade. Workload semantics,
folder existence, and label availability/permissions are left to Fabric; there are no preflight calls.

API adapters own LRO receipts, waiting and result retrieval. A 202 receipt is not a definition to
export. Endpoint-specific wire sizes remain separate from the decoded file budgets.

### Get Item Definition Adapter

The initial mode requires workspace/item UUIDs and optionally encodes a format query value; it makes
one POST and accepts only 200/202. Explicit sync waiting reuses `FabricOperationRunner`. The mutually
exclusive resume mode accepts only an operation UUID (plus optional output settings), reads its result
once, and does not poll, perform a state preflight, or resubmit the POST. Item provenance cannot be
verified from this result endpoint. Missing response formats are never inferred from request options.

`FabricCoreService.ItemDefinition.cs` parses a typed definition envelope with the existing source
generation context, rejects duplicate JSON properties, and uses `FabricItemDefinitionValidator`.
It applies no item-specific filename, layout, or format-combination rules to service-returned definitions,
matching the shared generic-only validation policy. Tests preserve arbitrary safe layouts for omitted,
recognized, and unknown formats through both initial retrieval and operation-result resumption.
Unknown response fields are ignored while all known definition parts must be valid; no invalid parts
are skipped. The per-result 64 MiB JSON bound is checked both with and without Content-Length, at the
exact limit and one byte beyond. Tests also exercise decoded/count boundaries and definition bodies
above the unchanged 1 MiB limit of Create Item and generic operation-result.

Combined Get/Update tests pass the retrieved parts, including `.platform` and a payload larger than
1 MiB, unchanged into Update. They verify synchronous and accepted responses, metadata query
omission/false/true, result retrieval only for Get, and no-result state-only completion for Update.
Update's synchronous response reader retains the default 1 MiB bound. Actual server startup checks
discover both tools together while keeping their distinct local-file options hidden over HTTP.

`FabricItemDefinitionExportService` checks host capability before filesystem inspection and again
before calling the existing writer. Registration removes only `output-directory` over HTTP without
mutating shared cached binder options. Inline content remains available over HTTP after sensitive-data
consent. Because the tool can write files, its conservative ReadOnly=false metadata excludes it in
read-only hosts; Secret=true requires the existing elicitation flow. Tests use synthetic accepted
consent, and verify rejected/unsupported consent never reaches authentication or file access.

No output is attempted for receipts, failed operations, invalid definitions, or absent destinations.
Successful export returns the published directory and typed definition, including only a format
actually supplied by Fabric. Existing destinations are preserved and staging is removed on injected
write failure/cancellation, with no inline-success fallback. All coverage is offline: no deployed
resources, real Fabric calls, OBO/RBAC validation, or recorded playback.

All file tests use isolated temporary directories with no Azure or Fabric requests. The fixture
resolves system temporary-directory aliases, such as macOS `/var`, before creating its test data;
the product continues to reject linked paths. Symbolic-link
tests run when the host permits link creation; on Windows without the necessary privilege they
report an explicit skip, rather than silently passing.

## Item Metadata

Get Item reuses `FabricCoreCommand`, the shared authenticated sender with `ResponseHeadersRead`,
`FabricCoreHttpHelpers.GetRetryAfter`, and `FabricThrottledException`. It returns only the five-field
metadata contract, requires both returned IDs to match the request, and retains nonempty future item
types. Optional descriptions may be absent, null, or explicitly empty. It makes no definition/data
requests, follows no returned URLs, and does not retry. Tests reuse `FabricCoreHttpMessageHandler`
and an existing stream cancellation double, without introducing another sender, handler, or exception.

All Get Item coverage here is offline. Registered-handler transport configuration does not establish
live Fabric behavior, real OBO/RBAC authorization, or recorded playback.

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

## Item Definition Updates

`core_update-item-definition` requires known workspace/item UUIDs and exactly one complete definition
source. It reuses `IFabricItemDefinitionInputService`, `IFabricItemDefinitionFileService`, the wire
models, the shared `DefinitionInputOptions` group through `OptionContainer`, and shared validation
without fetching or merging the old definition. Public input names stay flat and the absent group
does not invent a source; Update requires a source while Create permits omission. There is no
item-type hint or format-specific layout policy. Generic validation applies regardless of format;
safe arbitrary layouts, supplied format strings, and payload bytes reach Fabric unchanged.
`.platform` never supplies an inferred type, and no metadata preflight is performed.
IDs, wait settings, and options are checked before file access. The service also validates typed
definitions/options before authentication. Configured definition budgets are shared with file input.

The payload contains only `definition` and optional `options`. Nullable `update-metadata` preserves
query omission, explicit false, and explicit true. Only true requests applying supplied `.platform`
metadata; bytes are never rewritten or auto-selected. Sensitivity labels are outside this API.

The endpoint-specific `FabricOperationContract` explicitly has no result reader and accepts only
empty 200 completion or 202 acceptance. A synchronous completion returns a `Succeeded` receipt;
sync waiting after 202 reads state only and stops at `Succeeded` without `/result` or Get Item.
Timeouts, cancellation, and read failures retain resumable receipts and never resubmit the POST.
Generic LRO result-size limits are unchanged. Shared uncertain-outcome guidance now refers to the
mutation rather than assuming every caller is creating an item.

Tests use isolated temporary files, substituted HTTP/credentials, and the existing virtual clock.
Catalog and real server-startup coverage include the new schema without copying full catalogs.
These checks do not perform live Fabric mutations, test real permissions, or establish recorded
playback. The Create Item regression suite covers the shared parser extraction.

## Item Deletion

Delete Item retains the shared authenticated send's buffered `ResponseContentRead` default and uses `FabricCoreHttpHelpers.GetRetryAfter` without changing its nullable-header fallback, status/error wording, or uncertain-deletion outcomes. Tests reuse `FabricCoreHttpMessageHandler`, capture requests inside callbacks for disposal assertions, and retain `ItemDeleteTrackingContent` and all operation-specific cases.

The optional Boolean safeguard is configured by the `FabricCoreSetup` registration factory before discovery or execution, using one explicit value and command-local validators. It rejects valueless, invalid, repeated, or ambiguous input without changing the shared binder. Missing/null/false never enables hard deletion; the existing MCP coercion boundary is retained.

## Workspace Deletion

Delete Workspace selects `ResponseHeadersRead` on the inherited authenticated sender and reuses `FabricCoreHttpHelpers.GetRetryAfter`, retaining its own throttling exception and unknown-outcome wording. It keeps explicit cancellation checks before authentication and after the response, plus the shared sender's post-credential check before HTTP. Tests reuse `FabricCoreHttpMessageHandler` and retain `WorkspaceDeleteResponseContent` to verify that deletion response bodies are never read.

Workspace deletion tests substitute every credential and HTTP request. They never delete real workspaces. These tests do not establish live Fabric behavior, actual identity permissions, OBO authorization, or recorded playback. Approval to implement or run these offline tests does not authorize a real deletion.

## Folder Updates

Update Folder sends one authenticated PATCH with only the required replacement display name.
Create and Update share unchanged Fabric name validation, while Get retains its separate read
semantics. The service reuses the bounded folder snapshot reader and checks both requested
identities; absent/null parent metadata stays omitted. HTTP 200 with unreadable metadata remains
a confirmed update, not proof of failure to apply the mutation. Pre-send failures, known HTTP
rejections, and uncertain sends are distinguished without exposing backend payloads or retrying.
The advertised name schema is a string. The existing MCP argument binder can join a string array
into one space-separated string; tests preserve this shared conversion rather than claiming strict
original-JSON-type rejection or changing other tools. Repeated option keys are rejected.

The replacement follows Core Item/Workspace Update's destructive/idempotent hints, not execution
or schedule semantics. It is not secret-bearing and uses normal host mutation consent. Read-only
mode excludes Update while retaining Get/List. All request fixtures are offline, with substituted
credentials and HTTP; real host startup tests inspect discovery without calling Fabric. No live
folder, customer-ID discovery, OBO/RBAC validation, or recorded playback is implied.

## Running Tests

Run from the repository root using the SDK pinned in `global.json` and Microsoft.Testing.Platform:

```powershell
# Run all Core tests
dotnet test --project tools\Fabric.Mcp.Tools.Core\tests\Fabric.Mcp.Tools.Core.Tests\Fabric.Mcp.Tools.Core.Tests.csproj

# Run assignment command, service, and registered-handler tests
dotnet test --project tools\Fabric.Mcp.Tools.Core\tests\Fabric.Mcp.Tools.Core.Tests\Fabric.Mcp.Tools.Core.Tests.csproj --filter-class '*WorkspaceAssignToCapacity*' '*WorkspaceCapacityAssignment*'

# Run specific test
dotnet test --project tools\Fabric.Mcp.Tools.Core\tests\Fabric.Mcp.Tools.Core.Tests\Fabric.Mcp.Tools.Core.Tests.csproj --filter-class "*ItemCreateCommandTests"

# Run definition packaging/input and Create Item coverage
dotnet test --project tools\Fabric.Mcp.Tools.Core\tests\Fabric.Mcp.Tools.Core.Tests\Fabric.Mcp.Tools.Core.Tests.csproj --filter-class '*FabricItemDefinition*' '*ItemCreate*' '*FabricOperationMcpTests'

# Run Get Item Definition, export, and existing LRO/creation regression coverage
dotnet test --project tools\Fabric.Mcp.Tools.Core\tests\Fabric.Mcp.Tools.Core.Tests\Fabric.Mcp.Tools.Core.Tests.csproj --filter-class '*ItemDefinitionGet*' '*FabricItemDefinition*' '*FabricOperation*' '*ItemCreate*'

# Run definition updates and shared definition/Create Item/LRO regressions
dotnet test --project tools\Fabric.Mcp.Tools.Core\tests\Fabric.Mcp.Tools.Core.Tests\Fabric.Mcp.Tools.Core.Tests.csproj --filter-class '*ItemDefinitionUpdate*' '*ItemCreate*' '*FabricItemDefinition*' '*FabricOperation*' '*FabricCoreCatalogTests'

# Run LRO service, polling, command/schema, and Create Item integration tests
dotnet test --project tools\Fabric.Mcp.Tools.Core\tests\Fabric.Mcp.Tools.Core.Tests\Fabric.Mcp.Tools.Core.Tests.csproj --filter-class '*FabricOperation*' '*ItemCreate*'

# Run Get Item command, service, and registered-handler tests
dotnet test --project tools\Fabric.Mcp.Tools.Core\tests\Fabric.Mcp.Tools.Core.Tests\Fabric.Mcp.Tools.Core.Tests.csproj --filter-class '*ItemGet*'

# Run job-instance lookup and its shared-reader/catalog regressions
dotnet test --project tools\Fabric.Mcp.Tools.Core\tests\Fabric.Mcp.Tools.Core.Tests\Fabric.Mcp.Tools.Core.Tests.csproj --filter-class '*ItemJobInstance*' '*FabricCoreHttpHelpersTests' '*FabricCoreCatalogTests' '*FabricOperation*' '*ItemDefinitionGet*'

# Run job submission, lookup, JSON-input, and shared infrastructure regressions
dotnet test --project tools\Fabric.Mcp.Tools.Core\tests\Fabric.Mcp.Tools.Core.Tests\Fabric.Mcp.Tools.Core.Tests.csproj --filter-class '*ItemJobRun*' '*ItemJobInstance*' '*ItemCreateRequestInput*' '*FabricCoreHttpHelpersTests' '*FabricCoreCatalogTests' '*FabricCoreSetupTests'

# Run cancellation command/service/routing coverage
dotnet test --project tools\Fabric.Mcp.Tools.Core\tests\Fabric.Mcp.Tools.Core.Tests\Fabric.Mcp.Tools.Core.Tests.csproj --filter-class '*ItemJobInstanceCancel*'

# Run one-page listing command/service/routing coverage
dotnet test --project tools\Fabric.Mcp.Tools.Core\tests\Fabric.Mcp.Tools.Core.Tests\Fabric.Mcp.Tools.Core.Tests.csproj --filter-class '*ItemJobInstanceList*'

# Run schedule listing and the affected Jobs/input/pagination/catalog regressions
dotnet test --project tools\Fabric.Mcp.Tools.Core\tests\Fabric.Mcp.Tools.Core.Tests\Fabric.Mcp.Tools.Core.Tests.csproj --filter-class '*ItemScheduleList*' '*ItemJobInstance*' '*ItemJobRun*' '*FabricCoreHttpHelpersTests' '*FabricCoreCatalogTests' '*FabricCoreSetupTests'

# Run only Get Capacity command tests
dotnet test --project tools\Fabric.Mcp.Tools.Core\tests\Fabric.Mcp.Tools.Core.Tests\Fabric.Mcp.Tools.Core.Tests.csproj --filter-class "*CapacityGetCommandTests"

# Run capacity listing tests
dotnet test --project tools\Fabric.Mcp.Tools.Core\tests\Fabric.Mcp.Tools.Core.Tests\Fabric.Mcp.Tools.Core.Tests.csproj --filter-class '*CapacityList*'

# Run only Get Workspace command tests
dotnet test --project tools\Fabric.Mcp.Tools.Core\tests\Fabric.Mcp.Tools.Core.Tests\Fabric.Mcp.Tools.Core.Tests.csproj --filter-class "*WorkspaceGetCommandTests"

# Run folder lookup command, service, and registered MCP tests
dotnet test --project tools\Fabric.Mcp.Tools.Core\tests\Fabric.Mcp.Tools.Core.Tests\Fabric.Mcp.Tools.Core.Tests.csproj --filter-class "*FolderGet*"

# Run folder updates and the shared naming/snapshot/catalog regressions
dotnet test --project tools\Fabric.Mcp.Tools.Core\tests\Fabric.Mcp.Tools.Core.Tests\Fabric.Mcp.Tools.Core.Tests.csproj --filter-class "*Folder*" "*FabricCoreCatalogTests" "*FabricCoreSetupTests"

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

Tests reuse `FabricCoreHttpMessageHandler` and retain `UnreadableHttpContent`. All new and inherited operation suites remain offline; no actual assignment or other resource call is authorized. The existing single-proxy local-command text-only fallback and Item Delete explicit Boolean safeguard are unchanged; read-only Core membership is checked by the catalog manifest.
