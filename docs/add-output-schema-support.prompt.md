# Plan: Add `outputSchema` to MCP tools

Advertise an `outputSchema` (JSON Schema) on every MCP tool exposed by the servers in this repo, mirroring the existing `inputSchema` pipeline. Generate the schema from each command's result `JsonTypeInfo` via `System.Text.Json.Schema.JsonSchemaExporter` (AOT-safe in .NET 10), populate `CallToolResult.StructuredContent` so clients can validate responses against the advertised schema, and normalize the small set of commands that today return more than one distinct result record so each tool produces a single, typed result object.

The success-payload type is encoded as a `TResult` parameter on a new `BaseCommand<TOptions, TResult>` so the schema, the `JsonTypeInfo`, and the value passed to `ResponseResult.Create` are guaranteed to match at compile time. This forces a one-time generic-parameter cascade through every intermediate base class (`SubscriptionCommand<TOptions>`, `BaseStorageCommand<TOptions>`, …); we accept that cost in exchange for a permanent compile-time invariant and symmetry with the existing `BaseCommand<TOptions>` generic.

**Steps**

1. **Phase 1 — Core infrastructure** in [core/Microsoft.Mcp.Core](core/Microsoft.Mcp.Core)
   - Introduce `BaseCommand<TOptions, TResult>` deriving from `BaseCommand<TOptions>`, with an abstract `protected JsonTypeInfo<TResult> ResultTypeInfo { get; }` and a strongly-typed `protected void SetResult(CommandContext context, TResult value)` helper that wraps `ResponseResult.Create<TResult>(value, ResultTypeInfo)`. Expose the type-info to the loader via an explicit `IBaseCommand.ResultTypeInfo` implementation so the loader has a single read site regardless of which base a command derives from.
   - Add `JsonTypeInfo? ResultTypeInfo => null;` on `IBaseCommand` as a transitional default so commands not yet reparented to the generic base compile and continue to omit `outputSchema`. Remove the default in Phase 4 once every command is on the generic base.
   - In `CommandFactoryToolLoader.GetTool()`, after setting `tool.InputSchema`, generate `tool.OutputSchema` via `JsonSchemaExporter.GetJsonSchemaAsNode(typeInfo.Options, typeInfo.Type)` when `ResultTypeInfo` is non-null.
   - Add a small post-processor for the exporter output (strip `$schema`/`title`, ensure top-level `type: "object"`, leave `additionalProperties` unset — see consideration #3 below).
   - Propagate `outputSchema` through `NamespaceToolLoader`, `ServerToolLoader`, `RegistryToolLoader`, and `SingleProxyToolLoader`.
   - Populate `CallToolResult.StructuredContent` from the same object passed to `ResponseResult.Create<T>` whenever `outputSchema` is advertised; keep existing text content for backward compatibility.

2. **Phase 2 — Result-shape normalization** (consolidate every multi-result command to a single `*CommandResult` record with optional fields). *Parallel-safe; one PR per command:*
   - [tools/Azure.Mcp.Tools.Compute/src/Commands/Vmss/VmssGetCommand.cs](../tools/Azure.Mcp.Tools.Compute/src/Commands/Vmss/VmssGetCommand.cs) — collapse 3 records into one with optional `Vmss` / `VmssItem` / `VmInstance` fields.
   - [tools/Azure.Mcp.Tools.Compute/src/Commands/Vm/VmGetCommand.cs](../tools/Azure.Mcp.Tools.Compute/src/Commands/Vm/VmGetCommand.cs) — collapse 2 records (single + list + optional instance view).
   - [tools/Azure.Mcp.Tools.EventHubs/src/Commands/Namespace/NamespaceGetCommand.cs](../tools/Azure.Mcp.Tools.EventHubs/src/Commands/Namespace/NamespaceGetCommand.cs) — collapse 2 records.
   - [tools/Azure.Mcp.Tools.Monitor/src/Commands/WebTests/WebTestsGetCommand.cs](../tools/Azure.Mcp.Tools.Monitor/src/Commands/WebTests/WebTestsGetCommand.cs) — collapse 2 records.
   - [tools/Azure.Mcp.Tools.Sql/src/Commands/Server/ServerGetCommand.cs](../tools/Azure.Mcp.Tools.Sql/src/Commands/Server/ServerGetCommand.cs) — wrap the raw `List<SqlServer>` root in a `ServerGetCommandResult` record.
   - [tools/Azure.Mcp.Tools.Search/src/Commands/Index/IndexQueryCommand.cs](../tools/Azure.Mcp.Tools.Search/src/Commands/Index/IndexQueryCommand.cs) — wrap the raw `List<JsonElement>` in an `IndexQueryCommandResult` record.
   - [tools/Azure.Mcp.Tools.Storage/src/Commands/Blob/BlobUploadCommand.cs](../tools/Azure.Mcp.Tools.Storage/src/Commands/Blob/BlobUploadCommand.cs) — already returns an object; rename to `BlobUploadCommandResult` for consistency (optional).
   - [tools/Azure.Mcp.Tools.Functions/src/Commands/Template/TemplateGetCommand.cs](../tools/Azure.Mcp.Tools.Functions/src/Commands/Template/TemplateGetCommand.cs) — already conformant; use as the reference pattern.
   - Going forward: every command must call `ResponseResult.Create` with a `*CommandResult` record root (no raw lists/primitives).

3. **Phase 3 — Per-toolset wiring** (depends on Phase 1; one PR per toolset)
   - Widen the toolset's intermediate base classes to take `TResult` (e.g., `BaseStorageCommand<TOptions>` → `BaseStorageCommand<TOptions, TResult>`).
   - Reparent every command in [tools/Azure.Mcp.Tools.*](../tools) onto the generic base and supply `protected override JsonTypeInfo<TResult> ResultTypeInfo => *JsonContext.Default.<Result>;`. Replace each `context.Response.Results = ResponseResult.Create(...)` call site with `SetResult(context, ...)`.
   - Verify all result records (and nested types) are registered in the toolset's `*JsonContext`.
   - Compilation alone proves coverage — once a command derives from `BaseCommand<TOptions, TResult>`, omitting `ResultTypeInfo` or returning a value of the wrong type is a compile error. No discovery test required.

4. **Phase 4 — Tighten and validate** (depends on Phase 3 completion)
   - Remove the transitional `JsonTypeInfo? ResultTypeInfo => null;` default on `IBaseCommand` once every command has been reparented onto `BaseCommand<TOptions, TResult>`. The interface member becomes a non-defaulted `JsonTypeInfo ResultTypeInfo { get; }` implemented exclusively by the generic base.
   - Add `ToolOutputSchemaTests` paralleling [ToolInputSchemaTests.cs](../core/Microsoft.Mcp.Core/tests/Microsoft.Mcp.Core.UnitTests/Areas/Server/Models/ToolInputSchemaTests.cs).
   - Add an integration test asserting `CallToolResult.StructuredContent` validates against the advertised `outputSchema` for a representative tool.
   - Update [servers/Azure.Mcp.Server/docs/azmcp-commands.md](../servers/Azure.Mcp.Server/docs/azmcp-commands.md) and add changelog entries to [servers/Azure.Mcp.Server/CHANGELOG.md](../servers/Azure.Mcp.Server/CHANGELOG.md).

**Relevant files**

- [core/Microsoft.Mcp.Core/src/Areas/Server/Commands/ToolLoading/CommandFactoryToolLoader.cs](../core/Microsoft.Mcp.Core/src/Areas/Server/Commands/ToolLoading/CommandFactoryToolLoader.cs) — extend `GetTool()` to emit `OutputSchema`.
- [core/Microsoft.Mcp.Core/src/Areas/Server/Commands/ToolLoading/NamespaceToolLoader.cs](../core/Microsoft.Mcp.Core/src/Areas/Server/Commands/ToolLoading/NamespaceToolLoader.cs), `ServerToolLoader.cs`, `RegistryToolLoader.cs`, `SingleProxyToolLoader.cs` — pass through `OutputSchema` from upstream tools.
- `core/Microsoft.Mcp.Core/src/Commands/IBaseCommand.cs` and `BaseCommand.cs` — introduce `BaseCommand<TOptions, TResult>` with abstract `ResultTypeInfo` and a typed `SetResult` helper; add the transitional `IBaseCommand.ResultTypeInfo => null` default.
- Per-toolset base classes under [tools/Azure.Mcp.Tools.*/src/Commands](../tools) (e.g., `BaseStorageCommand`, `BaseSqlCommand`, …) — widen each to take `TResult`.
- [core/Microsoft.Mcp.Core/src/Models/Command/CommandResponse.cs](../core/Microsoft.Mcp.Core/src/Models/Command/CommandResponse.cs) and the call-tool path — surface the typed payload to `CallToolResult.StructuredContent`.
- All Phase 2 command files plus their `*JsonContext` and unit tests.
- All toolset commands under [tools](../tools) and their `*JsonContext.cs` files for Phase 3.

**Verification**

1. `dotnet build` and `./eng/scripts/Build-Local.ps1 -BuildNative` (AOT) green. After Phase 4, omitting `ResultTypeInfo` on a new command is a compile error — verify by writing a temporary command without it and confirming the build fails.
2. New `ToolOutputSchemaTests` plus integration test that calls a representative tool, asserts `CallToolResult.StructuredContent` is populated and validates against the advertised schema.
3. Run `azmcp server start`, hit `tools/list`, and spot-check 5 tools with diverse shapes (list, single, blob upload, vmss multi-shape, sql wrapped list) — every tool exposes a valid `outputSchema`.
4. `./eng/scripts/Test-Code.ps1` and `.\eng\common\spelling\Invoke-Cspell.ps1` clean.

**Decisions**

- **Schema source:** `JsonSchemaExporter` over each command's result `JsonTypeInfo` (reuses existing source-gen contexts, no new reflection).
- **Result exposure:** `BaseCommand<TOptions, TResult>` generic with an abstract `JsonTypeInfo<TResult> ResultTypeInfo` property and a typed `SetResult` helper. Keeps the success-payload contract in the type system, symmetric with `BaseCommand<TOptions>`. Rejected the alternative of a virtual `JsonTypeInfo? ResultTypeInfo` property on `IBaseCommand` because the invariant "declared result type matches what the command actually writes" would live only in a runtime discovery test instead of being compiler-enforced, and the asymmetry with the existing `TOptions` generic is unprincipled. We pay the one-time generic-parameter cascade through intermediate base classes to get a permanent compile-time guarantee.
- **Schema scope:** describes the `Results` payload only (the `*CommandResult` record), matching what `structuredContent` carries.
- **Multi-result policy:** consolidate into one record per command with optional fields.
- **structuredContent:** populated on `CallToolResult` so clients can validate responses against `outputSchema`.
- **Coverage:** compile-time — once a command derives from `BaseCommand<TOptions, TResult>`, the schema is mandatory by definition. The transitional `IBaseCommand.ResultTypeInfo => null` default is removed in Phase 4.
- **Out of scope:** synthesizing schemas for proxied external tools (we only forward whatever they declare); changing the `CommandResponse` envelope; backward-compat shims for renamed Phase 2 result records.

**Further considerations**

1. **Rollout cadence:** Phase 1 + one pilot toolset (recommend Storage) in the same PR to prove end-to-end flow — including widening that toolset's intermediate base classes to take `TResult` — then fan out per-toolset PRs. Option A: pilot inside Phase 1 PR (recommended). Option B: pure infrastructure PR first, pilot separately.
2. **`JsonSchemaExporter` post-processing:** it emits `$schema` and draft-2020-12 keywords that the SDK's `McpJsonUtilities.IsValidMcpToolSchema` may reject. We need an allowlist/trim pass with explicit test coverage. Recommend implementing this in Phase 1 with dedicated unit tests rather than discovering edge cases per-toolset.
3. **`additionalProperties` on output schemas:** input schemas set it to `false` for OpenAI strict mode. For outputs, that would break clients when we add server-side fields. Recommend leaving it unset (permissive) on output schemas; flag this as a one-line decision in the changelog.
4. **Generic-parameter cascade scope:** every intermediate base class used by a toolset (`SubscriptionCommand<TOptions>`, `BaseStorageCommand<TOptions>`, `BaseSqlCommand<TOptions>`, …) needs to widen to `<TOptions, TResult>`. Scope each Phase 3 PR to one toolset's base hierarchy so the diff stays bounded; the change is a pure type-parameter add that the compiler verifies exhaustively.
