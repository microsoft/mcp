# Migrating Commands to Emit `outputSchema` and `structuredContent`

This guide describes how to opt a toolset's commands into MCP **`outputSchema`** and
**`structuredContent`**, following the same pattern applied to the App Configuration commands in the
pilot. It is written so that an AI agent (or a human) can follow it mechanically, one command at a
time.

> **Cardinal rule: this migration describes the result; it does not change the command's result model.**
>
> No CLI option, name, result type, or description changes. The server's startup
> `--structured-output-mode` determines whether the MCP response remains content-only, duplicates the
> complete result into `structuredContent`, or uses compact text alongside `structuredContent`.

## What this migration does

Two things happen once a command opts in, both driven by the command exposing its result type:

1. **`outputSchema`** — When the server lists tools, it generates a JSON Schema from the command's
   source-generated `JsonTypeInfo` (via `System.Text.Json.Schema.JsonSchemaExporter`) and attaches it to
   the tool as `outputSchema`. Result roots that are not JSON objects (arrays or scalars) are wrapped
   under a single `value` property so the schema is a valid MCP object schema.
2. **`structuredContent`** — When the tool is invoked, the server takes the command's result payload and
   also returns it as `structuredContent`, shaped with the **same** wrapping rule so it validates against
   the advertised `outputSchema`.

The plumbing already exists in `Microsoft.Mcp.Core`. You do **not** touch it when migrating a toolset.
It activates for any command that exposes a result type when the server starts with
`--structured-output-mode duplicated` or `--structured-output-mode compact`:

- ``Commands/BaseCommand`2.cs`` — defines the `ResultTypeInfo` hook and the `SetResult(...)` helper.
- `Areas/Server/Commands/OptionSchemaGenerator.cs` — `CreateOutputSchema(...)` builds the schema.
- `Areas/Server/Commands/ToolLoading/CommandFactoryToolLoader.cs` — `GetTool(...)` attaches
  `outputSchema`; `CallToolHandler(...)` emits `structuredContent` via `TryBuildStructuredContent(...)`.

The only per-command change is: **tell the base command what your result type is, and let the base
command store the result.**

## Server startup modes

| Mode | `outputSchema` | `structuredContent` | Text `content` |
|---|---|---|---|
| Option omitted (default) | Omitted | Omitted | Complete historical response |
| `duplicated` | Emitted | Complete result payload | Complete historical response |
| `compact` | Emitted | Complete result payload | Concise pointer to `structuredContent` |

Structured output is an explicit operator opt-in. Enable it only when the target client supports
`outputSchema` and `structuredContent`.

### Aggregate execution modes

`namespace`, `consolidated`, and `single` modes advertise stable aggregate `outputSchema` contracts;
individual child-command schemas are not combined into a large union.

In `namespace` and `consolidated` modes, each in-process aggregate tool uses one of three tagged
envelopes:

| `kind` | Fields | Used for |
|---|---|---|
| `tool-list` | `tools` | `learn=true` command discovery |
| `tool-result` | `command`, `result` | A routed child-command result |
| `message` | `message` | Aggregate-level guidance |

This aggregate contract applies even when a routed child command has not yet been migrated to
`BaseCommand<TOptions, TResult>`. Namespace selection still applies in `namespace` mode: only
namespaces enabled with `--namespace` are listed and receive the aggregate schema.

In `single` mode, the server's one aggregate tool uses the same three variants, with an additional
`tool` field on `tool-result` to identify the routed service. Its `result` field contains the complete
downstream MCP `CallToolResult`, including content, structured content, error state, and metadata.

## Prerequisites

Before migrating a command, verify both of these. If either is false, resolve it first.

1. **The command is on the two-generic pattern** `BaseCommand<TOptions, TResult>` (usually via a
   subclass such as `SubscriptionCommand<TOptions, TResult>`). The `JsonContext` and `ResultTypeInfo`
   hooks and the `SetResult` helper exist on the two-generic base. If the command is still on the legacy
   one-generic `BaseCommand<TOptions>`, convert it first — see
   [`option-conversion.md`](https://github.com/microsoft/mcp/blob/1e188cfd2b24ce7cea9120b443b2204148e6c37e/docs/option-conversion.md) — then come back here.

2. **The result type is registered in the toolset's `JsonSerializerContext`.** This is effectively
   guaranteed: the pre-migration code already passed `XxxJsonContext.Default.<TResult>` to
   `ResponseResult.Create(...)`, which requires the type to be registered. Expose that context from the
   command's `JsonContext` getter so serialization and field selection use the same metadata. A direct
   `ResultTypeInfo` override remains supported when context-level access is not needed.
   (Example: `AppConfigJsonContext` already lists all five result records.)

## Migration steps (per command)

For each command in the toolset, make these three edits.

### Step 1 — Add the JSON metadata using directive

```csharp
using System.Text.Json.Serialization;
```

### Step 2 — Expose the source-generated context

Add this property to the command class (next to the injected fields is fine). Reference the same
source-generated context already used to serialize the command's result.

```csharp
public override JsonSerializerContext JsonContext => AppConfigJsonContext.Default;
```

The base resolves `ResultTypeInfo` for `TResult` from this context. Ensure the context registers the
command result type. Direct `ResultTypeInfo` overrides remain supported; when using one, ensure its
type matches `TResult`.

### Step 3 — Replace `ResponseResult.Create` with `SetResult`

Anywhere the command sets a **success** result, replace the manual
`context.Response.Results = ResponseResult.Create(<result>, XxxJsonContext.Default.<TResult>);` with the
base helper:

```csharp
SetResult(context, <result>);
```

`SetResult` serializes with `ResultTypeInfo` resolved from the context — so the
`structuredContent` and the `outputSchema` are always generated from the same source of truth.

Leave error handling untouched: `catch` blocks continue to call `HandleException(context, ex)`. Only
success-path result assignments change.

### Optional response field selection

Commands can expose their source-generated context through the `JsonContext` getter:

```csharp
public override JsonSerializerContext JsonContext => StorageJsonContext.Default;
```

The base command resolves `ResultTypeInfo` for `TResult` from this context, so `SetResult`, output
schemas, and field selection use the same serialized property names, including naming policies and
`JsonPropertyName` attributes. The context must register `TResult` and be available during base
construction. Existing direct `ResultTypeInfo` overrides remain supported.

Commands exposing `JsonContext` or `ResultTypeInfo` can opt into `--select` by implementing
`Microsoft.Mcp.Core.Options.ISelectOption` on their options class and declaring a nullable `Select` property
with an explicit `[Option]` attribute. The interface defines the property contract but not option
metadata; put the static description in the property attribute, for example:
`Comma-separated list of response fields to include in the output. If not specified all fields will be included.`
Core appends `Choices are: ...` using the serialized response field names from the source-generated
result metadata; no reflection-based JSON serialization or per-command filtering code is needed.

Selection applies to top-level object properties. For an array result, or a result object containing
only one collection property (such as `BlobGetCommandResult.Blobs`), it applies to each item's
top-level properties while preserving the collection wrapper. Nested values are retained in full.
Object responses and object collections are supported; scalar and dictionary result roots are not.

The base command validates comma-separated field names before execution. Names are case-insensitive,
surrounding whitespace is ignored, and duplicates are allowed. Unknown names, empty selections, and
nested paths are rejected. Omitting `--select` preserves the full response. Filtering is request-local
and applies to both text and structured success results, not error responses.

## Worked example (App Configuration `KeyValueGetCommand`)

**Before:**

```csharp
using Microsoft.Mcp.Core.Models.Command;

public sealed class KeyValueGetCommand(...)
    : SubscriptionCommand<KeyValueGetOptions, KeyValueGetCommand.KeyValueGetCommandResult>(subscriptionResolver)
{
    // ...
    public override async Task<CommandResponse> ExecuteAsync(CommandContext context, KeyValueGetOptions options, CancellationToken cancellationToken)
    {
        try
        {
            var settings = await _appConfigService.GetKeyValues(/* ... */);
            context.Response.Results = ResponseResult.Create(new(settings ?? []), AppConfigJsonContext.Default.KeyValueGetCommandResult);
        }
        catch (Exception ex)
        {
            _logger.LogError("An exception occurred processing command. Exception: {Exception}", ex);
            HandleException(context, ex);
        }

        return context.Response;
    }
}
```

**After:**

```csharp
using Microsoft.Mcp.Core.Models.Command;
using System.Text.Json.Serialization;                        // Step 1

public sealed class KeyValueGetCommand(...)
    : SubscriptionCommand<KeyValueGetOptions, KeyValueGetCommand.KeyValueGetCommandResult>(subscriptionResolver)
{
    // ...
    public override JsonSerializerContext JsonContext         // Step 2
        => AppConfigJsonContext.Default;

    public override async Task<CommandResponse> ExecuteAsync(CommandContext context, KeyValueGetOptions options, CancellationToken cancellationToken)
    {
        try
        {
            var settings = await _appConfigService.GetKeyValues(/* ... */);
            SetResult(context, new(settings ?? []));                          // Step 3
        }
        catch (Exception ex)
        {
            _logger.LogError("An exception occurred processing command. Exception: {Exception}", ex);
            HandleException(context, ex);
        }

        return context.Response;
    }
}
```

That is the entire change. The five App Configuration commands (`AccountListCommand`,
`KeyValueGetCommand`, `KeyValueSetCommand`, `KeyValueDeleteCommand`, `KeyValueLockSetCommand`) were each
migrated with exactly these three edits.

## Variations and gotchas

- **Multi-line `ResponseResult.Create` calls.** Some commands wrap the call across several lines (e.g.
  `KeyValueSetCommand`). Collapse the whole statement into a single `SetResult(context, new(...));`.
- **Multiple success call sites.** If a command assigns `context.Response.Results` in more than one
  success branch, replace every one of them with `SetResult`.
- **Array or scalar result roots need no special handling.** If a command's `TResult` serializes to a
  JSON array or scalar rather than an object, the infrastructure automatically wraps both the schema and
  the `structuredContent` under a `value` property. Do not hand-wrap it yourself.
- **`SetResult` requires serialization metadata.** Calling `SetResult` without providing either
  `JsonContext` or `ResultTypeInfo` throws `InvalidOperationException` at runtime. Always do Step 2 and
  Step 3 together.
- **Do not change `TResult` or its JSON shape.** The result type, the stored value, and `ResultTypeInfo`
  must all describe the same shape. Changing a record's members changes both the text `content` and the
  advertised schema — that is out of scope for this migration.
- **Commands with no result payload.** A command that never sets `context.Response.Results` on success
  has nothing to describe — leave it alone (do not add an empty `ResultTypeInfo`).
- **Aggregate modes are handled centrally.** `namespace` and `consolidated` wrap in-process command
  results in their shared aggregate contract, while `single` wraps the complete downstream MCP result.
  No per-command schema migration is needed for those aggregate envelopes.
- **External proxy declarations are separate.** `RegistryToolLoader` forwards schemas for individually
  exposed external tools in `all` mode. External tools exposed alongside namespace or consolidated
  routers are not normalized into the in-process aggregate contract.

## Verification

Run these from the repository root, scoped to the toolset you migrated (App Configuration shown):

1. **Build the tool project.**
   ```powershell
   dotnet build tools/Azure.Mcp.Tools.AppConfig/src
   ```

2. **Run the toolset's unit tests.** Existing tests must stay green. Duplicated mode preserves the
   historical text `content`; compact mode replaces successful structured responses with concise text.
   ```powershell
   ./eng/scripts/Test-Code.ps1 -Paths AppConfig
   ```

3. **Confirm the schema appears (optional but recommended).** Start the server in the execution mode
   you are validating with
   `--structured-output-mode duplicated` or `--structured-output-mode compact`, regenerate the
   tools-list snapshot, and
   confirm each migrated tool now carries an `outputSchema`, while `name`, `description`, and
   `inputSchema` are unchanged.
   ```powershell
   eng/scripts/New-ToolsListFile.ps1 -Suffix after
   ```

4. **Format.**
   ```powershell
   dotnet format --include="tools/Azure.Mcp.Tools.AppConfig/**/*.cs"
   ```

5. **Changelog.** The core infrastructure ships its own changelog entry. If your namespace opt-in is a
   user-facing change worth calling out, add a changelog entry per
   [`changelog-entries.md`](https://github.com/microsoft/mcp/blob/1e188cfd2b24ce7cea9120b443b2204148e6c37e/docs/changelog-entries.md#L4), using the appropriate `-ChangelogPath`.

## Per-command checklist

- [ ] Command is on the two-generic `BaseCommand<TOptions, TResult>` pattern (convert first if not)
- [ ] Added `using System.Text.Json.Serialization;`
- [ ] Exposed the source-generated `JsonSerializerContext` through `JsonContext` (or directly overrode `ResultTypeInfo`)
- [ ] Replaced every success-path `ResponseResult.Create(...)` with `SetResult(context, ...)`
- [ ] Left `catch` / `HandleException` paths unchanged
- [ ] Did **not** change `TResult`, its members, or the text `content`
- [ ] If enabling `--select`, implemented `ISelectOption`, added `[Option(Description = "...")]` to `Select`, and tested both filtered and unfiltered responses
- [ ] Built the tool project
- [ ] Ran the toolset's unit tests (all green)
- [ ] Confirmed `outputSchema` present and `name`/`description`/`inputSchema` unchanged via `tools list`
- [ ] Ran `dotnet format`

## Quick reference: old vs new

| Aspect | Before (pre-migration) | After (migrated) |
|---|---|---|
| Result type exposed to server | Not exposed | `public override JsonSerializerContext JsonContext => XxxJsonContext.Default;` |
| Storing the success result | `context.Response.Results = ResponseResult.Create(<result>, XxxJsonContext.Default.<TResult>);` | `SetResult(context, <result>);` |
| `using` directives | — | `+ using System.Text.Json.Serialization;` |
| Tool `outputSchema` | Absent | Generated from `ResultTypeInfo` when structured output is enabled |
| Response `structuredContent` | Absent | Result payload, same wrapping as the schema, when enabled |
| Text `content` block | Complete response | Complete when disabled or in `duplicated`; concise in `compact` |
| CLI options / `inputSchema` | Unchanged | Unchanged |
| Error handling | `HandleException(context, ex)` | `HandleException(context, ex)` (unchanged) |
