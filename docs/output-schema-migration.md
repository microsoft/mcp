# Migrating Commands to Emit `outputSchema` and `structuredContent`

This guide describes how to opt a toolset's commands into MCP **`outputSchema`** and
**`structuredContent`**, following the same pattern applied to the App Configuration commands in the
pilot. It is written so that an AI agent (or a human) can follow it mechanically, one command at a
time.

> **Cardinal rule: this migration is purely additive. We are describing the result, not changing it.**
>
> The existing text `content` block of every tool response stays byte-for-byte identical, and no CLI
> option, name, or description changes. You are only (a) advertising a schema for the result and
> (b) echoing the same result payload into `structuredContent`. If a `tools list` diff shows any change
> to `name`, `description`, `inputSchema`, or annotations — or if the text `content` of a response
> changes — stop and investigate.

## What this migration does

Two things happen once a command opts in, both driven by the command exposing its result type:

1. **`outputSchema`** — When the server lists tools, it generates a JSON Schema from the command's
   source-generated `JsonTypeInfo` (via `System.Text.Json.Schema.JsonSchemaExporter`) and attaches it to
   the tool as `outputSchema`. Result roots that are not JSON objects (arrays or scalars) are wrapped
   under a single `value` property so the schema is a valid MCP object schema.
2. **`structuredContent`** — When the tool is invoked, the server takes the command's result payload and
   also returns it as `structuredContent`, shaped with the **same** wrapping rule so it validates against
   the advertised `outputSchema`.

The plumbing already exists in `Microsoft.Mcp.Core`. You do **not** touch it when migrating a toolset —
it activates automatically for any command that exposes a result type:

- ``Commands/BaseCommand`2.cs`` — defines the `ResultTypeInfo` hook and the `SetResult(...)` helper.
- `Areas/Server/Commands/OptionSchemaGenerator.cs` — `CreateOutputSchema(...)` builds the schema.
- `Areas/Server/Commands/ToolLoading/CommandFactoryToolLoader.cs` — `GetTool(...)` attaches
  `outputSchema`; `CallToolHandler(...)` emits `structuredContent` via `TryBuildStructuredContent(...)`.

The only per-command change is: **tell the base command what your result type is, and let the base
command store the result.**

## Prerequisites

Before migrating a command, verify both of these. If either is false, resolve it first.

1. **The command is on the two-generic pattern** `BaseCommand<TOptions, TResult>` (usually via a
   subclass such as `SubscriptionCommand<TOptions, TResult>`). The `ResultTypeInfo` hook and the
   `SetResult` helper only exist on the two-generic base. If the command is still on the legacy
   one-generic `BaseCommand<TOptions>`, convert it first — see
   [`option-conversion.md`](./option-conversion.md) — then come back here.

2. **The result type is registered in the toolset's `JsonSerializerContext`.** This is effectively
   guaranteed: the pre-migration code already passed `XxxJsonContext.Default.<TResult>` to
   `ResponseResult.Create(...)`, which requires the type to be registered. You will reference that exact
   same `JsonTypeInfo` from the new `ResultTypeInfo` override, so no new registration is normally needed.
   (Example: `AppConfigJsonContext` already lists all five result records.)

## Migration steps (per command)

For each command in the toolset, make these three edits.

### Step 1 — Add the `JsonTypeInfo` using directive

```csharp
using System.Text.Json.Serialization.Metadata;
```

### Step 2 — Override `ResultTypeInfo`

Add this property to the command class (next to the injected fields is fine). Reference the **same**
source-generated `JsonTypeInfo` that the command already used when creating its response, and make sure
its type argument matches the command's declared `TResult`.

```csharp
public override JsonTypeInfo<KeyValueGetCommandResult>? ResultTypeInfo => AppConfigJsonContext.Default.KeyValueGetCommandResult;
```

> If the `JsonTypeInfo` you previously passed to `ResponseResult.Create` is for a type that is **not**
> this command's `TResult`, that is a bug in the original command (the advertised schema would not match
> the serialized result). Fix the mismatch — align `TResult`, the stored result, and `ResultTypeInfo` on
> one type — rather than papering over it.

### Step 3 — Replace `ResponseResult.Create` with `SetResult`

Anywhere the command sets a **success** result, replace the manual
`context.Response.Results = ResponseResult.Create(<result>, XxxJsonContext.Default.<TResult>);` with the
base helper:

```csharp
SetResult(context, <result>);
```

`SetResult` serializes with `ResultTypeInfo` — the exact `JsonTypeInfo` you just declared — so the
`structuredContent` and the `outputSchema` are always generated from the same source of truth.

Leave error handling untouched: `catch` blocks continue to call `HandleException(context, ex)`. Only
success-path result assignments change.

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
using System.Text.Json.Serialization.Metadata;              // Step 1

public sealed class KeyValueGetCommand(...)
    : SubscriptionCommand<KeyValueGetOptions, KeyValueGetCommand.KeyValueGetCommandResult>(subscriptionResolver)
{
    // ...
    public override JsonTypeInfo<KeyValueGetCommandResult>? ResultTypeInfo   // Step 2
        => AppConfigJsonContext.Default.KeyValueGetCommandResult;

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
- **`SetResult` requires the override.** Calling `SetResult` without overriding `ResultTypeInfo` throws
  `InvalidOperationException` at runtime. Always do Step 2 and Step 3 together.
- **Do not change `TResult` or its JSON shape.** The result type, the stored value, and `ResultTypeInfo`
  must all describe the same shape. Changing a record's members changes both the text `content` and the
  advertised schema — that is out of scope for this migration.
- **Commands with no result payload.** A command that never sets `context.Response.Results` on success
  has nothing to describe — leave it alone (do not add an empty `ResultTypeInfo`).
- **Proxy / namespace mode is already handled.** `RegistryToolLoader` forwards `outputSchema` when it
  re-exposes prefixed tools, so migrated tools keep their schema in namespace-proxy mode. No action
  needed.

## Verification

Run these from the repository root, scoped to the toolset you migrated (App Configuration shown):

1. **Build the tool project.**
   ```powershell
   dotnet build tools/Azure.Mcp.Tools.AppConfig/src
   ```

2. **Run the toolset's unit tests.** Existing tests must stay green — the text `content` is unchanged,
   so no assertions should need updating.
   ```powershell
   ./eng/scripts/Test-Code.ps1 -Paths AppConfig
   ```

3. **Confirm the schema appears (optional but recommended).** Regenerate the tools-list snapshot and
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
   [`changelog-entries.md`](./changelog-entries.md), using the appropriate `-ChangelogPath`.

## Per-command checklist

- [ ] Command is on the two-generic `BaseCommand<TOptions, TResult>` pattern (convert first if not)
- [ ] Added `using System.Text.Json.Serialization.Metadata;`
- [ ] Overrode `ResultTypeInfo` with the source-generated `JsonTypeInfo<TResult>` for the command's `TResult`
- [ ] Replaced every success-path `ResponseResult.Create(...)` with `SetResult(context, ...)`
- [ ] Left `catch` / `HandleException` paths unchanged
- [ ] Did **not** change `TResult`, its members, or the text `content`
- [ ] Built the tool project
- [ ] Ran the toolset's unit tests (all green)
- [ ] Confirmed `outputSchema` present and `name`/`description`/`inputSchema` unchanged via `tools list`
- [ ] Ran `dotnet format`

## Quick reference: old vs new

| Aspect | Before (pre-migration) | After (migrated) |
|---|---|---|
| Result type exposed to server | Not exposed | `public override JsonTypeInfo<TResult>? ResultTypeInfo => XxxJsonContext.Default.<TResult>;` |
| Storing the success result | `context.Response.Results = ResponseResult.Create(<result>, XxxJsonContext.Default.<TResult>);` | `SetResult(context, <result>);` |
| `using` directives | — | `+ using System.Text.Json.Serialization.Metadata;` |
| Tool `outputSchema` | Absent | Generated from `ResultTypeInfo` |
| Response `structuredContent` | Absent | Result payload, same wrapping as the schema |
| Text `content` block | Unchanged | Unchanged |
| CLI options / `inputSchema` | Unchanged | Unchanged |
| Error handling | `HandleException(context, ex)` | `HandleException(context, ex)` (unchanged) |
