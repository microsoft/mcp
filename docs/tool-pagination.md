# Cursor Pagination for Tools

Tools can expose continuation using optional `cursor` input and `nextCursor` output. These are tool arguments and
result fields, not additions to the MCP protocol's `tools/call` envelope.

## Options contract

Implement [`ICursorOption`](https://github.com/microsoft/mcp/blob/main/core/Microsoft.Mcp.Core/src/Options/ICursorOption.cs)
in the tool's options class and declare a concrete `[Option]` property:

```csharp
public sealed class ListOptions() : ICursorOption
{
    [Option(Description = ICursorOption.CursorDescription)]
    public string? Cursor { get; set; }
}
```

Use `ICursorOption.CursorDescription` as the entire parameter description, or append tool-specific guidance after it.
The interface does not register options or fetch pages automatically. The attribute belongs on the concrete property,
and the command passes the cursor to its service. Keep any page size, filters, expiration, and continuation
restrictions specific to that tool.

## Result contract

- Return `nextCursor` in the tool's result payload when another page can be requested. Callers pass it unchanged as
  `cursor`.
- Omit `nextCursor`, or return null, when enumeration is complete. Callers should also treat an empty `nextCursor` as completion.
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

## Verification

Test first, continued, and terminal pages through option binding and source-generated result serialization. Verify
opaque characters are preserved and empty-page behavior follows the tool's contract. Assert that raw ADME tool
responses never expose `result.cursor`, while Marketplace's legacy continuation argument still works. Reject conflicting
continuation inputs before service calls. Use recorded multi-page tests where available without changing the existing
request sequence unnecessarily.

For fixed-size migrations, also verify the exact SDK page-size request, that a call does not drain subsequent pages,
and that continued calls preserve filters and return a terminal page. Test empty pages with continuation, invalid
tokens, and unchanged specific-resource lookups. Recorded tests follow every returned cursor and check the page limits.
