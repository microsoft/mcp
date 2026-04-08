# Pagination for MCP List Tools

## Problem Statement

Azure MCP list and query tools can return payloads large enough to exceed model and client limits, increasing latency, memory pressure, and failure rates. Streaming helps deliver large responses progressively, but it does not solve request sizing, resumability, or recovery after interruption.

Azure MCP therefore needs pagination as shared infrastructure for list tools, not ad hoc behavior per command. That means a resumable cursor model, server-controlled page size to bound response size, and one consistent MCP-facing contract instead of provider-specific paging behavior leaking into tool responses.

The need is most acute in remote HTTP mode, where requests may be served by different instances over time and commands must remain stateless. A cursor from one request must not be reusable against a different logical request shape or caller context.

## Background and Discussion Context

This design builds on the direction discussed in:

- `microsoft/mcp#428`
- `modelcontextprotocol/modelcontextprotocol#799`
- `https://gist.github.com/xiangyan99/32bebf596ae2903e46989422dc4ea757`

For Azure MCP, the takeaway is that pagination should be first-class infrastructure for list and query tools. It prevents oversized responses, makes retries and resume predictable, gives clients explicit control over progress, and avoids per-tool paging contracts. This document focuses on the Azure MCP-specific implications of that direction, especially remote operation, cross-request isolation, and provider diversity. Cross-user isolation is handled by the MCP authentication flow itself; if a cursor is shared across caller contexts, the eventual provider request will succeed or fail based on the caller's own credentials.

## MCP Protocol Alignment

Even though MCP pagination is only explicitly defined for protocol surfaces such as tools listing, the protocol already establishes a continuation pattern that Azure MCP should reuse for list-returning tools:

1. cursor-based continuation using `cursor` and `nextCursor`
2. opaque tokens that requestors do not parse or modify
3. forward-only traversal
4. exhaustion indicated by the absence of `nextCursor`
5. invalid cursor handling through an invalid-params style protocol error

This has a few concrete implications for Azure MCP pagination design:

1. Do not expose page numbers, offsets, or numeric traversal contracts.
2. Do not require clients or LLMs to calculate the next page.
3. Keep `cursor` and `nextCursor` as the only continuation contract.
4. Treat cursors as opaque tokens end-to-end.

## Goals

1. Define one MCP-facing pagination contract for paged list tools.
2. Keep commands stateless and safe across stdio, remote multi-user deployments, multiple server instances, and restarts.
3. Prevent cursor reuse across different logical requests.
4. Keep page size server-controlled so each response stays bounded.
5. Support provider-native pagination without exposing provider-native continuation data to clients.
6. Standardize cursor lifecycle behavior, validation, errors, observability, and incremental rollout across existing tools.

## Non-Goals

1. Guarantee snapshot consistency or uniform ordering across providers beyond what each provider already offers.
2. Expose Azure-native continuation tokens, `nextLink` values, or SDK pager tokens to MCP clients.
3. Require commands to inspect transport-specific objects such as `HttpContext`.

## Design Principles

1. One public contract: clients send `cursor` and receive `nextCursor`.
2. Use an in-memory cache for cursor storage. Session affinity already guarantees that all requests within the same MCP session are routed to the owning server instance, so a distributed cache is not required for cross-instance cursor lookups.
3. Cursors are opaque, short, immutable identifiers.
4. Provider-native continuation state stays private.
5. Do not expose page numbers, offsets, counts, or other traversal internals as part of the pagination contract.
6. The server controls page size so each response stays bounded, while cursors provide controlled continuation.
7. Commands remain stateless; pagination state lives in infrastructure.
8. The system must work in both stdio and remote HTTP modes.

## Proposed External Contract

For any paged MCP list tool:

- Request field: `cursor`
- Response field: `nextCursor`

### Request Schema

```json
{
	"type": "object",
	"properties": {
		"cursor": {
			"type": "string",
			"description": "Opaque continuation cursor from a previous response. Omit on the first page."
		}
	},
	"required": []
}
```

### Request Example

```json
{
	"subscription": "00000000-0000-0000-0000-000000000000",
	"resourceGroup": "rg-demo",
	"cursor": "cur_01JV7K6Q4M0R0H7S1EJ0M8YV7A"
}
```

### Response Schema

```json
{
	"type": "object",
	"properties": {
		"items": {
			"type": "array",
			"description": "Items returned for the current page.",
			"items": {
				"type": "object"
			}
		},
		"nextCursor": {
			"type": "string",
			"description": "Opaque continuation cursor to request the next page. Absent when there are no more results."
		}
	},
	"required": ["items"]
}
```

### Response Example

```json
{
	"items": [
		{ "name": "resource-1" },
		{ "name": "resource-2" }
	],
	"nextCursor": "cur_01JV7K6Q4M0R0H7S1EJ0M8YV7A"
}
```

### Contract Rules

1. `cursor` is optional on the first request and required only for resume.
2. `nextCursor` is returned only when more results are available.
3. Clients must treat `nextCursor` as opaque.
4. Clients must resend the same logical request shape when using a cursor.
5. If the logical request shape changes, the client must start a new pagination flow without `cursor`.
6. The absence of `nextCursor` indicates that there are no more results.
7. Clients must not derive page numbers, offsets, or traversal state from the cursor.

## Why a Registry Instead of Self-Contained Tokens

The recommended design is a server-side cursor registry backed by an in-memory cache rather than self-contained signed or encrypted cursor payloads. Because session affinity already ensures all requests for a given MCP session reach the same server instance, cursor records do not need to be shared across instances via a distributed cache.

Reasons:

1. Native continuation state can be large, provider-specific, and unstable across versions.
2. A registry lets us enforce TTL, schema migration, and explicit invalidation centrally.
3. A registry avoids sending sensitive or implementation-specific state back to clients, even in encrypted form.
4. A registry supports operational observability around create, resume, expire, mismatch, and provider failure events.

## High-Level Architecture

```text
Client
	|
	| request { ...filters, cursor? }
	v
MCP Command
	|
	| normalized pagination request
	v
Pagination Service
	|-- computes request fingerprint
	|-- loads/saves cursor records
	v
Cursor Registry (in-memory cache)
	|
	v
Provider Adapter (ARM, Cosmos, KQL, SDK pager, ...)
	|
	v
Provider API
```

### Main Components

#### 1. Pagination Service

Shared infrastructure used by paged commands. Responsible for:

- determining whether a request is an initial page or a resume
- computing the semantic request fingerprint
- loading and validating cursor records
- calling the provider-specific adapter
- creating the next cursor record when another page exists

#### 2. Cursor Registry

In-memory cache-backed cursor registry. Responsibilities:

- store cursor metadata and native continuation state
- apply TTL
- support versioned record schemas

A distributed cache is not needed here because the repo's session affinity layer (see [core/Microsoft.ModelContextProtocol.HttpServer.Distributed/](../../core/Microsoft.ModelContextProtocol.HttpServer.Distributed/)) already guarantees that all requests for a given `Mcp-Session-Id` are routed to the same server instance. Cursor records are therefore always local to the instance that created them. If an instance restarts, the session affinity layer detects the stale ownership and forces a new session, which naturally invalidates any in-flight cursors.

#### 3. Provider Adapters

Provider-specific pagination handlers that know how to start and resume using native provider state. Examples:

- ARM `nextLink`
- Cosmos continuation token
- KQL continuation state
- SDK pager continuation token

Each adapter owns its provider-specific serialization and resume logic.

## Cursor Record Schema

Each public cursor ID maps to a stored record similar to the following:

```json
{
	"version": 1,
	"cursorId": "cur_01JV7K6Q4M0R0H7S1EJ0M8YV7A",
	"provider": "arm",
	"operation": "resourcegroup.list",
	"requestFingerprint": "sha256:...",
	"nativeState": {
		"nextLink": "https://management.azure.com/..."
	},
	"resourceMetadata": {
		"subscription": "...",
		"cloud": "AzureCloud"
	},
	"createdAtUtc": "2026-03-27T00:00:00Z",
	"expiresAtUtc": "2026-03-27T01:00:00Z"
}
```

### Required Fields

- `version`: schema version for compatibility during rollout and upgrades
- `cursorId`: opaque public identifier returned to the client
- `provider`: adapter family that must handle resume
- `operation`: logical MCP tool or provider operation name
- `requestFingerprint`: hash of the semantic request shape
- `nativeState`: provider-native continuation state
- `resourceMetadata`: optional data needed to resume safely, such as cloud, api version, or scope metadata
- `createdAtUtc` and `expiresAtUtc`: lifecycle metadata

## Request Fingerprinting

Request fingerprinting is the primary defense against context crossing between different logical requests.

### Fingerprint Inputs

The fingerprint should include the normalized semantic request shape, excluding only fields that are expected to change across pages.

Include:

- tool name or logical operation
- subscription, resource group, workspace, database, or other target scope
- filter/search/order/select expressions
- cloud/environment selection
- api version or mode flags if they affect results

Exclude:

- `cursor`
- transport metadata
- correlation IDs
- transient tracing metadata

### Validation Rule

On resume:

1. Recompute the fingerprint from the incoming request.
2. Compare it to the stored fingerprint.
3. Reject mismatches with `InvalidCursor`.

This prevents a cursor created for one filter or scope from being replayed against another filter or scope.

## Cursor Lifecycle

### Initial Request

1. Client calls a list tool without `cursor`.
2. Command delegates to the pagination service.
3. The appropriate adapter fetches the first provider page.
4. If the provider indicates more data, the pagination service stores a cursor record and returns `nextCursor`.

### Resume Request

1. Client calls the same tool with `cursor`.
2. Pagination service loads the cursor record from the in-memory cursor registry.
3. Pagination service validates TTL, version, and request fingerprint.
4. The stored provider and operation select the correct adapter.
5. The adapter resumes with the stored native continuation state.
6. If another page exists, a new cursor record is written and returned as `nextCursor`.

### Immutability and Replay

Cursors should be immutable. The server should never mutate the meaning of an existing cursor ID.

We should allowing replay of the same cursor while it remains valid, rather than making it single-use. Reasons:

1. It simplifies retry behavior for clients.
2. It avoids server-side coordination just to prevent duplicate resume attempts.
3. It preserves stateless command behavior.

Clients should advance using the most recent `nextCursor`. Replaying an older cursor may return a duplicate page.

## Provider Adapter Contract

Each adapter should implement a common contract along these lines:

1. Start pagination from the initial request.
2. Resume pagination from stored native state.
3. Return items plus provider-native state for the next page, if any.
4. Serialize and deserialize its own native state.
5. Avoid leaking provider-native state outside the adapter boundary.

We should never translate one provider's native continuation token into another provider-neutral continuation model. The only provider-neutral artifact is the public cursor ID.

## Error Model

External errors should be normalized even if their internal causes differ.

### Recommended External Errors

- `InvalidCursor`: malformed cursor, unknown cursor, fingerprint mismatch, provider mismatch, or unsupported cursor version

At the protocol boundary, invalid cursor cases should map to MCP invalid-params behavior, which means `-32602` when surfaced as a protocol error.

### Internal Reasons to Distinguish in Logs and Metrics

- malformed cursor format
- registry miss
- request fingerprint mismatch
- unsupported version
- adapter deserialization failure
- upstream provider rejected native continuation state

## Future Configuration Points

The following aspects should be made configurable in later iterations of the design:

1. **Cursor TTL**: The time-to-live for cursor records should be configurable rather than hardcoded. Different providers or tool types may benefit from different expiry windows depending on the cost of re-fetching and the likelihood of long pauses between pages.
2. **Cache backing store**: The initial implementation uses an in-memory cache, but the cursor registry should support swapping to a distributed cache (e.g., Redis) via configuration. This allows deployments that cannot rely on session affinity, or that need cursor survival across instance restarts, to opt into a shared store without changing the pagination contract.

## Open Questions

1. What default TTL should we use for cursors, and should it vary by provider or tool type?
2. Do we need explicit invalidation hooks for deployments that revoke or rotate provider-specific continuation semantics?
3. The cursor registry needs a concrete cache strategy. The existing `ICacheService` abstraction (with `SingleUserCliCacheService` for stdio and `HttpServiceCacheService` for HTTP) could be extended to support cursor storage, or a dedicated `IMemoryCache`-based cursor store could be introduced. The choice should account for per-session key isolation in HTTP mode, TTL enforcement, and alignment with the repo's existing caching patterns. This decision should be resolved before implementation begins.