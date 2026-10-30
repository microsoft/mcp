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

## Verification

Test first, continued, and terminal pages through option binding and source-generated result serialization. Verify
opaque characters are preserved and empty-page behavior follows the tool's contract. Assert that raw ADME tool
responses never expose `result.cursor`, while Marketplace's legacy continuation argument still works. Reject conflicting
continuation inputs before service calls. Use recorded multi-page tests where available without changing the existing
request sequence unnecessarily.
