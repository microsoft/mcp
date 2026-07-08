# MCP 2026-07-28 Migration Proposal for Azure MCP Server

## Purpose
This document turns the 2026-07-28 MCP release candidate into an implementation plan for Azure MCP Server and the shared MCP runtime in this repository.

The goal is not only to upgrade the `ModelContextProtocol` packages, but to make the server compliant with the new protocol shape, remove assumptions that are no longer valid, and identify the product work that should follow the upgrade.

## Executive Summary
The 2026-07-28 MCP release candidate is a breaking protocol revision. The dominant change is that MCP is now stateless at the protocol layer: `initialize`/`initialized` and `Mcp-Session-Id` are removed, server-to-client interactions are restructured around active requests, and Streamable HTTP now uses explicit routing headers. In parallel, roots, sampling, and logging are deprecated, tool schemas are expanded to full JSON Schema 2020-12, and extensions become a first-class negotiation mechanism.

Azure MCP Server is structurally close to the new model because command execution is already request-scoped and transport-agnostic. The main migration work is in the server runtime, transport, elicitation, discovery, and test harness layers rather than in the individual Azure tools.

The highest-risk area is the consent and multi-round interaction path. The highest-effort area is the sampling migration, because the current namespace and proxy flows still rely on MCP sampling as part of intent-to-command routing. The highest-change-volume area is validation and compatibility testing because our current tests and helpers still speak the 2025-11-25 protocol.

## What the RC Changes

### 1. Stateless protocol core
The release candidate removes the session-oriented handshake and protocol-level session affinity. Requests carry client metadata in `_meta` on every call, and clients can use `server/discover` when they need to fetch capabilities up front.

Practical impact:
- `initialize` and `notifications/initialized` are no longer part of the wire contract.
- `Mcp-Session-Id` is no longer a protocol primitive.
- Load balancing, routing, and scale-out no longer depend on sticky sessions at the MCP protocol layer.

### 2. Server-to-client requests are request-scoped
Server-initiated prompts now need to happen while the server is actively servicing a client request. Multi-round interactions are represented by an `InputRequiredResult`, `requestState`, and a follow-up request carrying `inputResponses`.

Practical impact:
- Elicitation is still possible, but the SDK and server contract are different.
- Anything that previously depended on a long-lived session for consent or follow-up state needs to move to explicit request handles.

### 3. HTTP routing is explicit
Streamable HTTP requests now require `Mcp-Method` and `Mcp-Name` headers, and header/body mismatches are invalid.

Practical impact:
- Gateways and reverse proxies can route without parsing JSON bodies.
- The server must be validated against the new header contract in HTTP mode.

### 4. Cache metadata is part of the protocol
List and resource read responses can carry `ttlMs` and `cacheScope`.

Practical impact:
- Tool and resource discovery can become cache-aware across clients and hosts.
- Servers should stop assuming SSE is the only mechanism for cache refresh semantics.

### 5. Extensions are formalized
Tasks and MCP Apps are no longer ad hoc ideas; they now live in the extension model.

Practical impact:
- Capability negotiation needs to understand extension IDs.
- Long-running operations may want to adopt Tasks instead of bespoke polling or session carry-forward behavior.

### 6. Authorization is hardened
The spec tightens OAuth/OpenID behavior, especially around issuer validation and dynamic client registration semantics.

Practical impact:
- Current auth flows need to be verified against the new validation rules.
- External MCP client/server interop is more likely to fail if assumptions about issuer or application type are stale.

### 7. Roots, sampling, and logging are deprecated
These capabilities are not removed yet, but they are explicitly on the deprecation path.

Practical impact:
- New product work should avoid increasing dependency on these features.
- Sampling should become optional behavior, not a structural dependency.

### 8. Tool schemas are now full JSON Schema 2020-12
Input schemas can use composition and references. Output schemas are unrestricted, and `structuredContent` can be any JSON value.

Practical impact:
- Current schema generation should be revisited.
- Tool contracts can become more expressive, but the validator must stay bounded and must not dereference arbitrary external `$ref` values.

## Current Azure MCP Server State

### Package baseline
The repository currently pins `ModelContextProtocol` and `ModelContextProtocol.AspNetCore` to `1.1.0` in [Directory.Packages.props](../Directory.Packages.props).

### Server startup and transport wiring
The Azure server still starts MCP through the current SDK surface in [servers/Azure.Mcp.Server/src/Program.cs](../servers/Azure.Mcp.Server/src/Program.cs) and the shared host wiring in [core/Microsoft.Mcp.Core/src/Areas/Server/Commands/ServiceCollectionExtensions.cs](../core/Microsoft.Mcp.Core/src/Areas/Server/Commands/ServiceCollectionExtensions.cs).

The current shape is:
- `ServiceStartCommand` creates a host and starts MCP after DI setup in [core/Microsoft.Mcp.Core/src/Areas/Server/Commands/ServiceStartCommand.cs](../core/Microsoft.Mcp.Core/src/Areas/Server/Commands/ServiceStartCommand.cs).
- `AddMcpServer()` registers tool handlers and server instructions in [core/Microsoft.Mcp.Core/src/Areas/Server/Commands/ServiceCollectionExtensions.cs](../core/Microsoft.Mcp.Core/src/Areas/Server/Commands/ServiceCollectionExtensions.cs).
- The transport is selected via `WithStdioServerTransport()` or `WithHttpTransport()` in the same file.

### Protocol assumptions still present in tests
The test suite still uses the 2025-11-25 handshake in at least these places:
- [servers/Azure.Mcp.Server/tests/Azure.Mcp.Server.UnitTests/Infrastructure/ConsolidatedModeTests.cs](../servers/Azure.Mcp.Server/tests/Azure.Mcp.Server.UnitTests/Infrastructure/ConsolidatedModeTests.cs)
- [servers/Fabric.Mcp.Server/tests/Fabric.Mcp.Server.UnitTests/Infrastructure/ServerStartupTests.cs](../servers/Fabric.Mcp.Server/tests/Fabric.Mcp.Server.UnitTests/Infrastructure/ServerStartupTests.cs)

That tells us the current test harness still assumes initialize/initialized semantics and must be updated for the RC.

### Session-affinity code still exists in-repo
The repository still contains a dedicated session-affinity package that keys routing on `Mcp-Session-Id`, including [core/Microsoft.ModelContextProtocol.HttpServer.Distributed/src/SessionAffinityEndpointFilter.cs](../core/Microsoft.ModelContextProtocol.HttpServer.Distributed/src/SessionAffinityEndpointFilter.cs).

This is the clearest code-level sign that the repo still carries protocol-era session assumptions. Under the RC, that package is no longer a core MCP requirement. It may still be useful for non-MCP state-routing scenarios, but it should not be treated as foundational to MCP transport correctness.

Phase 2 decision (Workstream B item 4): keep this package for explicit legacy/custom state-routing scenarios only. It remains out of the default Azure MCP server runtime path and is not required for MCP 2026-07-28 compliance.

### Elicitation is implemented against the current SDK abstraction
The server currently wraps elicitation in [core/Microsoft.Mcp.Core/src/Extensions/McpServerElicitationExtensions.cs](../core/Microsoft.Mcp.Core/src/Extensions/McpServerElicitationExtensions.cs), which calls `McpServer.ElicitAsync(...)` and reads `ClientCapabilities.Elicitation`.

Tool loaders also depend on the same pattern in [core/Microsoft.Mcp.Core/src/Areas/Server/Commands/ToolLoading/BaseToolLoader.cs](../core/Microsoft.Mcp.Core/src/Areas/Server/Commands/ToolLoading/BaseToolLoader.cs).

That path is almost certainly the most sensitive migration surface because it is where user consent is enforced.

### Proxy and discovery flows still depend on sampling and current client behavior
The current runtime already has three layers of discovery/loading:
- command-group discovery in [core/Microsoft.Mcp.Core/src/Areas/Server/Commands/Discovery](../core/Microsoft.Mcp.Core/src/Areas/Server/Commands/Discovery)
- tool loaders in [core/Microsoft.Mcp.Core/src/Areas/Server/Commands/ToolLoading](../core/Microsoft.Mcp.Core/src/Areas/Server/Commands/ToolLoading)
- runtime execution in [core/Microsoft.Mcp.Core/src/Areas/Server/Commands/Runtime/McpRuntime.cs](../core/Microsoft.Mcp.Core/src/Areas/Server/Commands/Runtime/McpRuntime.cs)

The impact report in [docs/mcp-2026-07-28-rc-impact-report.md](../mcp-2026-07-28-rc-impact-report.md) already confirms that sampling is used in the proxy flows and that this needs de-risking. This proposal treats that as required migration work, not a nice-to-have.

## Migration Workstreams

### Workstream A: Protocol and SDK upgrade
Objective: Move the repository to an MCP SDK version that understands the 2026-07-28 contract and then remove any code that still relies on removed wire semantics.

Work items:
1. Upgrade `ModelContextProtocol` and `ModelContextProtocol.AspNetCore` from `1.1.0` to the first RC-compatible package release.
2. Re-run server startup and client interop tests against the new SDK.
3. Audit `McpServer`, `McpClient`, `RequestContext`, `CallToolResult`, `ListToolsResult`, and any transport options for breaking API changes.
4. Remove or adapt any code that assumes `initialize`, `notifications/initialized`, or protocol-level session state.
5. Validate that the Azure server can still start in both stdio and HTTP modes without any session bootstrap logic.

Acceptance criteria:
- Azure MCP Server can start and serve tools in both transports under the RC-compatible SDK.
- No test or runtime path depends on `initialize` or `Mcp-Session-Id`.
- Any SDK API migration is isolated to shared core code, not scattered across toolsets.

### Workstream B: Stateless HTTP transport and routing
Objective: Make HTTP mode compliant with the stateless protocol and explicit routing contract.

Work items:
1. Verify that the HTTP transport emits and accepts `MCP-Protocol-Version`, `Mcp-Method`, and `Mcp-Name` correctly.
2. Add negative tests for header/body disagreement.
3. Review the Azure HTTP startup path in [core/Microsoft.Mcp.Core/src/Areas/Server/Commands/ServiceStartCommand.cs](../core/Microsoft.Mcp.Core/src/Areas/Server/Commands/ServiceStartCommand.cs) and [servers/Azure.Mcp.Server/src/Program.cs](../servers/Azure.Mcp.Server/src/Program.cs) for any transport-specific assumptions that only existed because of session affinity.
4. Decide whether [core/Microsoft.ModelContextProtocol.HttpServer.Distributed/src/SessionAffinityEndpointFilter.cs](../core/Microsoft.ModelContextProtocol.HttpServer.Distributed/src/SessionAffinityEndpointFilter.cs) should be:
   - retired from the MCP path,
   - kept only for legacy compatibility experiments, or
   - refactored into a generic state-routing package with no MCP-specific contract names.
5. Remove any deployment guidance that tells operators to treat session affinity as a protocol requirement.

Status update:
- Item 4 decision: kept only for explicit legacy/custom state-routing scenarios; not part of default Azure MCP runtime behavior.
- Item 5 update: operator guidance is now header-first and stateless-first for MCP 2026-07-28 (`Mcp-Method` and `Mcp-Name` routing), with session affinity documented as optional.

Acceptance criteria:
- HTTP requests are routable without any session store or sticky routing requirement.
- Gateway/operator docs describe header-based routing instead of session-based routing.
- Tests cover both success and rejection of malformed headers.

### Workstream C: Capability discovery and list caching
Objective: Align discovery surfaces with `server/discover`, cache metadata, and the new client capability model.

Work items:
1. Determine how much of `ServiceStartCommand` and `ServiceCollectionExtensions` should move to capability discovery versus static server startup metadata.
2. Add or adapt the runtime to expose server discovery in a way compatible with the RC SDK.
3. Inspect list responses for opportunities to include `ttlMs` and `cacheScope`, especially for stable tool and namespace discovery surfaces.
4. Review `tools/list` and any derived discovery result caching in [core/Microsoft.Mcp.Core/src/Areas/Server/Commands/ToolLoading](../core/Microsoft.Mcp.Core/src/Areas/Server/Commands/ToolLoading) for protocol-level cache metadata exposure.
5. Keep server-side caches if they are still valuable, but do not rely on them to represent protocol freshness semantics.

Status update:
- Item 2 progress: added stateless HTTP integration coverage for `server/discover` in consolidated mode (`Mcp-Method`/`Mcp-Name` headers, no initialize handshake, no `Mcp-Session-Id`).
- Remaining for this workstream: finalize startup metadata vs discovery-surface split (item 1) and caching semantics review (items 4 and 5).

Acceptance criteria:
- Discovery works without the initialize handshake.
- Tool list surfaces advertise freshness accurately when the SDK supports it.
- Caching behavior is explicit and testable.

### Workstream D: Elicitation migration
Objective: Rebuild user-consent flows to the RC multi-round model and keep security behavior correct.

Work items:
1. Replace or adapt the current wrapper in [core/Microsoft.Mcp.Core/src/Extensions/McpServerElicitationExtensions.cs](../core/Microsoft.Mcp.Core/src/Extensions/McpServerElicitationExtensions.cs) to the RC client/server API.
2. Update [core/Microsoft.Mcp.Core/src/Areas/Server/Commands/ToolLoading/BaseToolLoader.cs](../core/Microsoft.Mcp.Core/src/Areas/Server/Commands/ToolLoading/BaseToolLoader.cs) so consent checks work with request-scoped multi-round interactions.
3. Ensure sensitive or destructive operations still fail closed when elicitation is unavailable.
4. Decide whether current consent UI text should stay as-is or be revised to reflect the new request/response flow.
5. Add tests for accept, decline, cancel, and continuation behavior.

Acceptance criteria:
- A sensitive tool call can start, pause for consent, resume, and complete under the new protocol.
- No consent request escapes the active request lifecycle.
- Client-not-supported paths still fail safely.

### Workstream E: Sampling deprecation and deterministic routing
Objective: Remove hard dependence on sampling for tool routing and intent resolution.

Work items:
1. Inventory all sampling-based flows in proxy and namespace loaders, including the paths called out in [docs/mcp-2026-07-28-rc-impact-report.md](../mcp-2026-07-28-rc-impact-report.md).
2. Introduce deterministic fallback routing for intent-to-command selection where it is currently inferred by sampling.
3. Preserve sampling only as an enhancement while the deprecation window is open.
4. Add telemetry so we can measure how often sampling is used versus deterministic resolution.
5. Rewrite user-facing guidance so the routing experience no longer depends on sampling being present.

Acceptance criteria:
- Core namespace and proxy flows still work when sampling is absent.
- Routing quality can be evaluated from telemetry.
- The runtime has a clear fallback if the SDK removes sampling support later.

### Workstream F: JSON Schema 2020-12 tool contracts
Objective: Make tool schemas compatible with the expanded schema model and prepare for richer tool definitions.

Work items:
1. Review current schema generation in the command/tool loader pipeline.
2. Upgrade input-schema generation so it can express composition and references when needed.
3. Add support for output schemas where a tool benefits from structured results.
4. Ensure any validator limits schema depth and execution time.
5. Explicitly block unsafe external `$ref` dereference behavior.
6. Select a small number of high-value tools as pilots for richer schemas before broad rollout.

Acceptance criteria:
- Existing tools continue to work with the new schema model.
- One or more pilot tools demonstrate richer schema expressiveness.
- Validation remains bounded and safe.

### Workstream G: Authorization and remote-host compliance
Objective: Verify that the server still behaves correctly with the hardened auth model.

Work items:
1. Re-test incoming auth in HTTP mode after the SDK upgrade.
2. Verify issuer validation and OAuth protected resource metadata behavior with RC-compliant clients.
3. Review outbound MCP-to-MCP auth for registry-backed servers in [core/Microsoft.Mcp.Core/src/Areas/Server/Commands/Discovery](../core/Microsoft.Mcp.Core/src/Areas/Server/Commands/Discovery).
4. Reconfirm that the OBO path, hosting-environment identity path, and no-auth dev path still map cleanly to supported deployments.
5. Update auth guidance where the spec now expects stricter issuer and registration behavior.

Acceptance criteria:
- Authenticated HTTP mode still works end to end.
- External MCP server calls remain functional against RC-compliant servers.
- Operator docs reflect the stricter auth assumptions.

### Workstream H: Observability and trace context
Objective: Preserve and improve tracing as the protocol reshapes request metadata.

Work items:
1. Verify W3C trace context propagation through `_meta` and across SDK boundaries.
2. Audit [core/Microsoft.Mcp.Core/src/Areas/Server/Commands/Runtime/McpRuntime.cs](../core/Microsoft.Mcp.Core/src/Areas/Server/Commands/Runtime/McpRuntime.cs) for any metadata keys that should migrate to the spec-standard names.
3. Keep existing Azure telemetry enrichment, but make sure it remains compatible with the new request metadata conventions.
4. Confirm that support logging remains a local troubleshooting feature and is not conflated with protocol logging capability.

Acceptance criteria:
- A request can be traced from host to MCP server and downstream Azure calls.
- No trace metadata is lost during the protocol upgrade.

### Workstream I: Compatibility test harness
Objective: Replace the old handshake-based tests with RC-native integration coverage.

Work items:
1. Update hardcoded protocol-version strings in the test suite.
2. Replace initialize/initialized-based smoke tests with RC-style request tests.
3. Add HTTP-mode tests that validate the new header contract.
4. Add elicitation round-trip tests that exercise the new multi-round flow.
5. Add protocol compatibility tests for `single`, `namespace`, `all`, and `consolidated` modes.
6. Add interop tests for external MCP server discovery and proxying.
7. Add regression tests for the removal or replacement of session-affinity assumptions.

Acceptance criteria:
- The test suite covers the same user-visible behaviors without depending on removed protocol primitives.
- CI fails if the server regresses to the legacy handshake model.

## Detailed File-Level Impact

### Likely update required
- [Directory.Packages.props](../Directory.Packages.props)
- [servers/Azure.Mcp.Server/src/Program.cs](../servers/Azure.Mcp.Server/src/Program.cs)
- [core/Microsoft.Mcp.Core/src/Areas/Server/Commands/ServiceStartCommand.cs](../core/Microsoft.Mcp.Core/src/Areas/Server/Commands/ServiceStartCommand.cs)
- [core/Microsoft.Mcp.Core/src/Areas/Server/Commands/ServiceCollectionExtensions.cs](../core/Microsoft.Mcp.Core/src/Areas/Server/Commands/ServiceCollectionExtensions.cs)
- [core/Microsoft.Mcp.Core/src/Extensions/McpServerElicitationExtensions.cs](../core/Microsoft.Mcp.Core/src/Extensions/McpServerElicitationExtensions.cs)
- [core/Microsoft.Mcp.Core/src/Areas/Server/Commands/ToolLoading/BaseToolLoader.cs](../core/Microsoft.Mcp.Core/src/Areas/Server/Commands/ToolLoading/BaseToolLoader.cs)
- [core/Microsoft.Mcp.Core/src/Areas/Server/Commands/ToolLoading/NamespaceToolLoader.cs](../core/Microsoft.Mcp.Core/src/Areas/Server/Commands/ToolLoading/NamespaceToolLoader.cs)
- [core/Microsoft.Mcp.Core/src/Areas/Server/Commands/ToolLoading/SingleProxyToolLoader.cs](../core/Microsoft.Mcp.Core/src/Areas/Server/Commands/ToolLoading/SingleProxyToolLoader.cs)
- [core/Microsoft.Mcp.Core/src/Areas/Server/Commands/ToolLoading/ServerToolLoader.cs](../core/Microsoft.Mcp.Core/src/Areas/Server/Commands/ToolLoading/ServerToolLoader.cs)
- [core/Microsoft.Mcp.Core/src/Areas/Server/Commands/Discovery](../core/Microsoft.Mcp.Core/src/Areas/Server/Commands/Discovery)
- [core/Microsoft.ModelContextProtocol.HttpServer.Distributed/src/SessionAffinityEndpointFilter.cs](../core/Microsoft.ModelContextProtocol.HttpServer.Distributed/src/SessionAffinityEndpointFilter.cs)
- [servers/Azure.Mcp.Server/tests/Azure.Mcp.Server.UnitTests/Infrastructure/ConsolidatedModeTests.cs](../servers/Azure.Mcp.Server/tests/Azure.Mcp.Server.UnitTests/Infrastructure/ConsolidatedModeTests.cs)
- [servers/Fabric.Mcp.Server/tests/Fabric.Mcp.Server.UnitTests/Infrastructure/ServerStartupTests.cs](../servers/Fabric.Mcp.Server/tests/Fabric.Mcp.Server.UnitTests/Infrastructure/ServerStartupTests.cs)

### Probably update later, depending on SDK shape
- Schema generation utilities in [core/Microsoft.Mcp.Core/src/Areas/Server/Commands](../core/Microsoft.Mcp.Core/src/Areas/Server/Commands)
- Any test utilities that assume session IDs or initialize handshakes in [core/Microsoft.Mcp.Core/tests](../core/Microsoft.Mcp.Core/tests)
- Any operator or deployment docs that still explain MCP in session terms

### May remain unchanged
- Most Azure tool implementations in `tools/Azure.Mcp.Tools.*`.
- Core command execution patterns, because they are already stateless and per-request.
- Existing Azure resource/service business logic, unless a specific tool wants richer output schemas or extension support.

## Beta SDK Migration Classification and Sequencing

This section maps the workstreams to the `ModelContextProtocol` C# beta (`2.0.0-preview.1`) and distinguishes what is required to adopt the beta SDK from what remains supported for backward compatibility but is now deprecated.

### Migration Gate (Required for beta adoption)
These items should be treated as release blockers for a beta-based Azure MCP Server build.

- Workstream A (Protocol and SDK upgrade):
   - Required now: items 1, 2, 3, and 5.
   - Required now: item 4 only for code paths that must serve `2026-07-28` behavior (stateless + no handshake assumptions).
- Workstream D (Elicitation migration):
   - Required now: items 1, 2, 3, and 5.
   - Reason: under `2026-07-28`, legacy server-to-client request methods are no longer valid on the modern protocol path; consent must be MRTR-compatible.
- Workstream I (Compatibility test harness):
   - Required now: items 1, 2, 3, 4, and 7.
   - Required now: item 5 for mode coverage parity.
- Workstream B (Stateless HTTP transport and routing):
   - Required now: items 1, 2, and 3.
   - Reason: validate `Mcp-Method`/`Mcp-Name` and header/body behavior in HTTP mode behind real gateways.
- Workstream G (Authorization and remote-host compliance):
   - Required now: items 1, 2, and 4.
   - Reason: verify issuer and protected-resource behavior with the new protocol/auth expectations.

### Beta Hardening (Not strict blockers, but should be done during beta cycle)
These items are important for production readiness on top of the beta migration gate.

- Workstream B: items 4 and 5 (session-affinity package disposition and operator guidance cleanup).
- Workstream C: items 1, 2, 4, and 5 (discovery and cache-surface alignment).
- Workstream G: items 3 and 5 (outbound MCP-to-MCP auth review and documentation updates).
- Workstream H: items 1 through 4 (trace and metadata hygiene).
- Workstream I: item 6 (external MCP server interop regression coverage).

### Deprecated but Backward-Compatible in beta SDK
These areas still work for compatibility, but are explicitly on the deprecation path and should not be expanded.

- Workstream E (Sampling deprecation and deterministic routing):
   - Backward-compatible today: sampling-based behavior remains functional.
   - Deprecated: sampling is marked obsolete in the C# beta surface and should become optional, not structural.
- Legacy initialize/session-era assumptions in tests and compatibility paths:
   - Backward-compatible today: beta clients/servers can still interoperate with legacy protocol peers.
   - Deprecated as primary model: modern path is `server/discover` + stateless request metadata.
- Roots and logging capability dependencies:
   - Backward-compatible today: still available for legacy compatibility.
   - Deprecated: avoid new product investments that increase dependency.
- Legacy resource-not-found error handling semantics:
   - Backward-compatible today on older protocol versions.
   - Deprecated for modern path: `2026-07-28` aligns to standard JSON-RPC error behavior.

### Optional Adoption (Post-migration feature work)
These are valuable but not required to ship a compliant beta migration.

- Workstream F (JSON Schema 2020-12 tool contracts), except baseline validation checks needed to keep existing tools safe.
- Workstream C item 3 (`ttlMs`/`cacheScope`) as an optimization pass.
- Tasks and MCP Apps extension productization.

### Updated Phase Plan

### Phase 1: Beta migration gate
Complete required migration work and validate both modern (`2026-07-28`) and legacy interop behavior.

Workstreams in this phase:
- Primary: Workstream A (required subset)
- Primary: Workstream D (required subset)
- Primary: Workstream I (required subset)
- Supporting: Workstream B (required subset)
- Supporting: Workstream G (required subset)

### Phase 2: Beta hardening
Stabilize HTTP routing, auth edges, observability, and external interop.

Workstreams in this phase:
- Primary: Workstream B (hardening subset)
- Primary: Workstream C (hardening subset)
- Primary: Workstream H
- Supporting: Workstream G (hardening subset)
- Supporting: Workstream I (interop tests)

### Phase 3: Deprecation burn-down
Reduce dependence on deprecated capabilities while preserving compatibility.

Workstreams in this phase:
- Primary: Workstream E
- Supporting: Workstream H (quality telemetry)
- Supporting: Workstream I (fallback and parity tests)

### Phase 4: Optional feature adoption
Add richer schemas, cache hints, and extension-driven features once baseline migration is stable.

Workstreams in this phase:
- Primary: Workstream F
- Primary: deferred Workstream C item 3 (`ttlMs`/`cacheScope`)
- Optional roadmap extension work: Tasks/MCP Apps capability adoption

## Risks

### Security regression in consent flows
The elicitation migration could accidentally weaken approval gates if the new request/response lifecycle is not implemented correctly.

Mitigation:
- Treat elicitation as a security boundary.
- Require explicit decline/cancel tests.
- Fail closed whenever client support is missing.

### Hidden SDK API breakage
The RC-compatible SDK may change method names or transport behavior in ways that are not obvious from the spec alone.

Mitigation:
- Keep SDK upgrade in its own PR.
- Isolate compatibility code in shared runtime layers.
- Add black-box integration tests before broad tool changes.

### Sampling quality drop
If intent routing depends too much on sampling today, removing it too quickly may reduce tool-selection quality.

Mitigation:
- Introduce deterministic fallback first.
- Instrument the fallback path.
- Retain sampling only as a non-critical enhancement during the transition window.

### Over-investment in optional RC features
Tasks and MCP Apps are useful, but they are not required for protocol compliance.

Mitigation:
- Separate mandatory migration work from feature-adoption work.
- Pilot optional features only after the core protocol upgrade is stable.

## Deliverables
1. RC-compatible MCP package upgrade.
2. Updated server and runtime wiring for stateless MCP operation.
3. Migrated elicitation path.
4. Sampling fallback plan and instrumentation.
5. Updated test harness with RC-native protocol coverage.
6. Optional follow-up design for Tasks, MCP Apps, and richer schemas.

## Notes
The earlier assessment in [docs/mcp-2026-07-28-rc-impact-report.md](../mcp-2026-07-28-rc-impact-report.md) intentionally stayed at the impact-analysis layer. This document is the implementation proposal: it breaks the work into buildable slices and makes the dependency order explicit.

The main takeaway is simple: Azure MCP Server should be able to adopt the RC without rewriting its business logic, but it does need a careful runtime migration. The right order is protocol compliance first, security-sensitive flow migration second, and feature adoption third.