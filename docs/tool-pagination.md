# Cursor Pagination for Tools

Tools can expose continuation using optional `cursor` input and `nextCursor` output. These are tool arguments and
result fields, not additions to the MCP protocol's `tools/call` envelope.

## Options contract

Implement [`ICursorOption`](https://github.com/microsoft/mcp/blob/main/core/Microsoft.Mcp.Core/src/Options/ICursorOption.cs)
in the tool's options class and declare a concrete `[Option]` property:

```csharp
public sealed class ListOptions() : ICursorOption
{
    [Option(Description = "Opaque nextCursor from a previous call. Omit for the first page.")]
    public string? Cursor { get; set; }
}
```

The interface does not register options or fetch pages automatically. The attribute belongs on the concrete property,
and the command passes the cursor to its service. Keep any page size, filters, expiration, and continuation
restrictions specific to that tool.

## Result contract

- Return `nextCursor` in the tool's result payload when another page can be requested. Callers pass it unchanged as
  `cursor`.
- Omit `nextCursor`, or return null, when enumeration is complete.
- Treat the value as opaque: do not trim, parse, or re-encode it at the command boundary. Use the service's required
  URL/body encoding when sending it.
- An empty page may still have a continuation. Apply a service-specific terminal condition only when that service
  defines it.
- Preserve existing result collections and wrappers. Register result types in their source-generated `System.Text.Json`
  context; do not add a shared result wrapper or put `nextCursor` on `CommandResponse`.
- Do not log cursors, silently restart on invalid tokens, or replace a tool's existing all-pages behavior with a single
  page without a migration decision.

The CLI and default MCP text response contain the tool payload inside `results`; `nextCursor` is at the root of that
payload, not the command response envelope.

## Initial adopters

| Tool | Why it is a candidate | Compatibility |
|---|---|---|
| `marketplace_product_list` | Already returns one product page with `nextCursor`. | Adds `cursor`; retains `next-cursor`. Supplying both is rejected. Repeat the same filters. |
| `adme_storage_record_list` | Already accepts a cursor and returns one record-ID page. | Uses root `nextCursor` instead of `result.cursor`; retains `result.results` and `correlationId`. This is a pre-GA breaking output change. |
| `adme_search` | Already supports explicit cursor-based search as well as offset queries. | Uses root `nextCursor` for cursor searches and removes `result.cursor`; preserves other result fields, offset defaults, and paging controls. This is a pre-GA breaking output change. |

ADME search requires the original criteria to be repeated and cursors expire after about one minute. Its empty
cursor-search response is terminal, even if the service repeats its backend `cursor`; `nextCursor` is omitted in
that case. Record listing instead preserves a non-null cursor even for an empty page.

The ADME REST-facing models still deserialize the service's `cursor` field. It is not part of the tool's nested
result: callers use only root `nextCursor` for continuation, then supply it as the unchanged `cursor` input.
`limit`, `offset`, `cursor-pagination-mode`, and `search-after` keep their existing meanings.

These tools already paginate, so adopting the interface does not introduce new result caps or automatic page draining.
The standalone `top`/`skip` alternative is a separate design; this implementation adds only the cursor contract.

## Fixed-size data-plane migrations

The following tools return one service page per call instead of draining the enumeration. Page sizes are immutable
maximums, not caller-configurable options; a service may return fewer items, including an empty page with a continuation.

| Tool | Page size | Migration |
|---|---:|---|
| `storage_blob_get` | 100 blobs | Listing without `blob` now returns one page. Specific-blob lookups are unchanged and cannot be combined with `cursor`. Repeat the same account, container, and prefix. |
| `storage_blob_container_get` | 100 containers | Listing without `container` now returns one page. Specific-container lookups are unchanged and cannot be combined with `cursor`. Repeat the same account and prefix. |
| `storage_table_list` | 100 table names | Returns one page rather than every table. Repeat the same account. |
| `cosmos_database_container_item_query` | 100 documents | Returns one query response page rather than every page. Preserves `items` as a collection of Cosmos DB response envelopes containing `Documents`. Repeat the same account, database, container, and SQL query. SQL limits such as `TOP` still apply to the overall query. |
| `cosmos_database_container_item_list-recent` | 10 documents | Removes `count` and the overall `TOP` limit. Returns successive pages ordered by `_ts` descending. Repeat the same account, database, and container. |

These are intentional breaking pagination changes. Omit `cursor` for the first page. Pass root `nextCursor` unchanged
as `cursor` with the same tenant, subscription/authentication context (where applicable), resource, and filters.
Continue even after an empty page when `nextCursor` is present; enumeration is complete only when it is omitted or null.
The services use native Azure SDK continuation tokens, request only one page, and propagate invalid/expired-token
errors without restarting the listing. Cursors do not provide snapshot isolation: concurrent writes may affect results.

## Verification

Test first, continued, and terminal pages through option binding and source-generated result serialization. Verify
opaque characters are preserved and empty-page behavior follows the tool's contract. Assert that raw ADME tool
responses never expose `result.cursor`, while Marketplace's legacy continuation argument still works. Reject conflicting
continuation inputs before service calls. Use recorded multi-page tests where available without changing the existing
request sequence unnecessarily.

For fixed-size migrations, also verify the exact SDK page-size request, that a call does not drain subsequent pages,
and that continued calls preserve filters and return a terminal page. Test empty pages with continuation, invalid
tokens, and unchanged specific-resource lookups. Recorded tests follow every returned cursor and check the page limits.
