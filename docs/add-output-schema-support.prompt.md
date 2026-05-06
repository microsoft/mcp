# Plan: Add `outputSchema` to MCP tools

> **Status:** Phases 1–5 implemented on branch `vcolin7/input-output-schema`, including the non-object-root wrapping fix described in step 1 below. This prompt is preserved as the executable plan so a coding agent can replay it on a fresh `main` if the merge approach in that branch is abandoned.

Advertise an `outputSchema` (JSON Schema) on every MCP tool exposed by the servers in this repo, mirroring the existing `inputSchema` pipeline. Generate the schema from each command's result `JsonTypeInfo` via `System.Text.Json.Schema.JsonSchemaExporter` (AOT-safe in .NET 10), populate `CallToolResult.StructuredContent` so clients can validate responses against the advertised schema, and normalize the small set of commands that today return more than one distinct result record so each tool produces a single, typed result object.

The success-payload type is encoded as a `TResult` parameter on a new `BaseCommand<TOptions, TResult>` so the schema, the `JsonTypeInfo`, and the value passed to `ResponseResult.Create` are guaranteed to match at compile time. This forces a one-time generic-parameter cascade through every intermediate base class (`SubscriptionCommand<TOptions>`, `BaseStorageCommand<TOptions>`, …); we accept that cost in exchange for a permanent compile-time invariant and symmetry with the existing `BaseCommand<TOptions>` generic.

**Steps**

1. **Phase 1 — Core infrastructure** in [core/Microsoft.Mcp.Core](core/Microsoft.Mcp.Core)
   - Introduce `BaseCommand<TOptions, TResult>` deriving from `BaseCommand<TOptions>`, with an abstract `protected JsonTypeInfo<TResult> ResultTypeInfo { get; }` and a strongly-typed `protected void SetResult(CommandContext context, TResult value)` helper that wraps `ResponseResult.Create<TResult>(value, ResultTypeInfo)`. Expose the type-info to the loader via an explicit `IBaseCommand.ResultTypeInfo` implementation so the loader has a single read site regardless of which base a command derives from.
   - Add `JsonTypeInfo? ResultTypeInfo => null;` on `IBaseCommand` as the supported escape hatch for text-only / passthrough commands that populate only `CommandResponse.Message` (e.g., `ServiceStartCommand`, `PluginTelemetryCommand`). During the rollout it also lets commands not yet reparented to the generic base compile and continue to omit `outputSchema`; once Phase 3 wraps up, the only remaining `null` overrides should be the by-design escape-hatch commands.
   - In `CommandFactoryToolLoader.GetTool()`, after setting `tool.InputSchema`, generate `tool.OutputSchema` via `JsonSchemaExporter.GetJsonSchemaAsNode(typeInfo.Options, typeInfo.Type)` when `ResultTypeInfo` is non-null. The input-schema modernization (RFC Part 1, tracked in [modernize-input-schema.prompt.md](modernize-input-schema.prompt.md)) already centralized input-schema generation in `OptionSchemaGenerator`; consider adding an `OptionSchemaGenerator.CreateOutputSchema(JsonTypeInfo)` sibling so both callers share one helper. The output path uses the command's source-generated `JsonTypeInfo`, which is fully AOT-safe and does **not** need the `UnconditionalSuppressMessage` attributes the input path carries.
   - Add a small post-processor for the exporter output (strip `$schema`/`title`, ensure top-level `type: "object"`, leave `additionalProperties` unset — see consideration #3 below).
   - **Wrap non-object roots.** `JsonSchemaExporter` emits non-object roots (`{type: "array"}`, `{type: "string"}`) for `JsonTypeInfo` describing `List<T>` or a scalar (e.g., `List<string>`, `string`, `List<JsonNode>` — currently used by a handful of `azureterraformbestpractices`, `azurebestpractices`, and `monitor` commands). The MCP SDK's `IsValidMcpToolSchema` requires the root to be `"object"` and rejects these. In `OptionSchemaGenerator.CreateOutputSchema`, detect any root whose `type` isn't `"object"` and wrap the inner schema under `{type: "object", properties: {value: <inner>}, required: ["value"]}`. Apply the matching wrapping to `CallToolResult.StructuredContent` in `CommandFactoryToolLoader.CallToolHandler` (`raw.ValueKind == JsonValueKind.Object ? raw : { "value": <raw> }`) so the runtime payload validates against the advertised schema. Add unit-test coverage for both `List<T>` and scalar return types under `OptionSchemaGeneratorTests`, plus a discovery-style probe test that walks every visible command's `ResultTypeInfo` and asserts `tool.OutputSchema = JsonSerializer.SerializeToElement(schema)` does not throw — this is what catches the bug on day one.
   - Propagate `outputSchema` through `NamespaceToolLoader`, `ServerToolLoader`, `RegistryToolLoader`, and `SingleProxyToolLoader`.
   - Populate `CallToolResult.StructuredContent` from the same object passed to `ResponseResult.Create<T>` whenever `outputSchema` is advertised; keep existing text content for backward compatibility.

2. **Phase 2 — Result-shape normalization.** Three buckets, mirroring the RFC tables. *Parallel-safe within each bucket; one PR per bucket:*

   **Bucket A — Multi-result commands.** Today they return different result records depending on input. Each gets one `*CommandResult` record with optional fields.
   - [tools/Azure.Mcp.Tools.Compute/src/Commands/Vmss/VmssGetCommand.cs](../tools/Azure.Mcp.Tools.Compute/src/Commands/Vmss/VmssGetCommand.cs) — collapse 3 records into one with optional `Vmss` / `VmssItem` / `VmInstance` fields.
   - [tools/Azure.Mcp.Tools.Compute/src/Commands/Vm/VmGetCommand.cs](../tools/Azure.Mcp.Tools.Compute/src/Commands/Vm/VmGetCommand.cs) — collapse 2 records (single + list + optional instance view).
   - [tools/Azure.Mcp.Tools.EventHubs/src/Commands/Namespace/NamespaceGetCommand.cs](../tools/Azure.Mcp.Tools.EventHubs/src/Commands/Namespace/NamespaceGetCommand.cs) — collapse 2 records.
   - [tools/Azure.Mcp.Tools.ManagedLustre/src/Commands/ImportJob/ImportJobGetCommand.cs](../tools/Azure.Mcp.Tools.ManagedLustre/src/Commands/ImportJob/ImportJobGetCommand.cs) — merge `ImportJobGetResult` + `ImportJobListResult` into one record with optional `Job?` / `Jobs?`.
   - [tools/Azure.Mcp.Tools.ManagedLustre/src/Commands/AutoimportJob/AutoimportJobGetCommand.cs](../tools/Azure.Mcp.Tools.ManagedLustre/src/Commands/AutoimportJob/AutoimportJobGetCommand.cs) — same merge pattern.
   - [tools/Azure.Mcp.Tools.ManagedLustre/src/Commands/AutoexportJob/AutoexportJobGetCommand.cs](../tools/Azure.Mcp.Tools.ManagedLustre/src/Commands/AutoexportJob/AutoexportJobGetCommand.cs) — same merge pattern.
   - [tools/Azure.Mcp.Tools.Monitor/src/Commands/WebTests/WebTestsGetCommand.cs](../tools/Azure.Mcp.Tools.Monitor/src/Commands/WebTests/WebTestsGetCommand.cs) — collapse 2 records.
   - [tools/Azure.Mcp.Tools.Sql/src/Commands/Server/ServerGetCommand.cs](../tools/Azure.Mcp.Tools.Sql/src/Commands/Server/ServerGetCommand.cs) — wrap the raw `List<SqlServer>` root in a `ServerGetCommandResult` record.
   - [tools/Azure.Mcp.Tools.Search/src/Commands/Index/IndexQueryCommand.cs](../tools/Azure.Mcp.Tools.Search/src/Commands/Index/IndexQueryCommand.cs) — wrap the raw `List<JsonElement>` in an `IndexQueryCommandResult` record.
   - [core/Microsoft.Mcp.Core/src/Areas/Tools/Commands/ToolsListCommand.cs](../core/Microsoft.Mcp.Core/src/Areas/Tools/Commands/ToolsListCommand.cs) — merge `ToolNamesResult` + `List<CommandInfo>` into one `ToolsListCommandResult` with optional `Names?` / `Tools?`.

   **Bucket B — Non-standard single-result commands.** Return a single result today but don't wrap it in a `*CommandResult` record root — either a raw list/primitive in `Results`, or only `Response.Message`. Each gets wrapped.
   - [tools/Azure.Mcp.Tools.Deploy/src/Commands/Architecture/DiagramGenerateCommand.cs](../tools/Azure.Mcp.Tools.Deploy/src/Commands/Architecture/DiagramGenerateCommand.cs) — wrap the diagram template string in `DiagramGenerateCommandResult(string Diagram)`; today populates `Response.Message` only.
   - [tools/Azure.Mcp.Tools.Deploy/src/Commands/Infrastructure/RulesGetCommand.cs](../tools/Azure.Mcp.Tools.Deploy/src/Commands/Infrastructure/RulesGetCommand.cs) — wrap in `RulesGetCommandResult(string Rules)`; today `Response.Message` only.
   - [tools/Azure.Mcp.Tools.Deploy/src/Commands/Plan/GetCommand.cs](../tools/Azure.Mcp.Tools.Deploy/src/Commands/Plan/GetCommand.cs) — wrap in `GetCommandResult(string Plan)`; today `Response.Message` only.
   - [core/Microsoft.Mcp.Core/src/Areas/Server/Commands/ServiceInfoCommand.cs](../core/Microsoft.Mcp.Core/src/Areas/Server/Commands/ServiceInfoCommand.cs) — wrap in `ServiceInfoCommandResult`; today `Response.Message` only.
   - [tools/Azure.Mcp.Tools.Storage/src/Commands/Blob/BlobUploadCommand.cs](../tools/Azure.Mcp.Tools.Storage/src/Commands/Blob/BlobUploadCommand.cs) — wrap in a new object `BlobUploadCommandResult` for consistency; today returns a `BlobUploadResult` object.

   **Bucket C — Won't be normalized.** Pure passthrough/lifecycle commands with no structured payload by design. They stay on `BaseCommand<TOptions>` and inherit the `IBaseCommand.ResultTypeInfo => null` default — no `outputSchema`, no `structuredContent`. List the rationale next to each so future contributors don't migrate them by reflex.
   - `ServiceStartCommand` — bootstraps the MCP server; nothing to describe.
   - `PluginTelemetryCommand` — pure side-effect; nothing to describe.

   Going-forward rule: every new command must call `ResponseResult.Create` with a `*CommandResult` record root (no raw lists/primitives) unless it falls into bucket C.

3. **Phase 3 — Per-namespace wiring** (depends on Phase 1; one PR per namespace)
   - Widen the namespace's intermediate base classes to take `TResult` (e.g., `BaseStorageCommand<TOptions>` → `BaseStorageCommand<TOptions, TResult>`).
   - Reparent every command in [tools/Azure.Mcp.Tools.*](../tools) onto the generic base and supply `protected override JsonTypeInfo<TResult> ResultTypeInfo => *JsonContext.Default.<Result>;`. Replace each `context.Response.Results = ResponseResult.Create(...)` call site with `SetResult(context, ...)`.
   - Verify all result records (and nested types) are registered in the namespace's `*JsonContext`.
   - Compilation alone proves coverage — once a command derives from `BaseCommand<TOptions, TResult>`, omitting `ResultTypeInfo` or returning a value of the wrong type is a compile error. No discovery test required.

4. **Phase 4 — Tighten and validate** (depends on Phase 3 completion)
   - Add `ToolOutputSchemaTests` paralleling [ToolInputSchemaTests.cs](../core/Microsoft.Mcp.Core/tests/Microsoft.Mcp.Core.UnitTests/Areas/Server/Models/ToolInputSchemaTests.cs).
   - Add an integration test asserting `CallToolResult.StructuredContent` validates against the advertised `outputSchema` for a representative tool.
   - Audit remaining `BaseCommand<TOptions>`-only commands; the only legitimate residents at this point are bucket C above. Anything else missed by Phase 3 should be reparented now.

5. **Phase 5 — Document and finalize** (depends on Phase 4 completion)
   - Update the `IBaseCommand.ResultTypeInfo` xml-doc to describe the `null` default as the supported escape hatch for text-only / passthrough commands (bucket C), not transitional source-compat. Cross-link the bucket C list so future contributors know when omitting `ResultTypeInfo` is appropriate.

**Relevant files**

- [core/Microsoft.Mcp.Core/src/Areas/Server/Commands/ToolLoading/CommandFactoryToolLoader.cs](../core/Microsoft.Mcp.Core/src/Areas/Server/Commands/ToolLoading/CommandFactoryToolLoader.cs) — extend `GetTool()` to emit `OutputSchema`.
- [core/Microsoft.Mcp.Core/src/Areas/Server/Commands/ToolLoading/NamespaceToolLoader.cs](../core/Microsoft.Mcp.Core/src/Areas/Server/Commands/ToolLoading/NamespaceToolLoader.cs), `ServerToolLoader.cs`, `RegistryToolLoader.cs`, `SingleProxyToolLoader.cs` — pass through `OutputSchema` from upstream tools.
- `core/Microsoft.Mcp.Core/src/Commands/IBaseCommand.cs` and `BaseCommand.cs` — introduce `BaseCommand<TOptions, TResult>` with abstract `ResultTypeInfo` and a typed `SetResult` helper; keep `IBaseCommand.ResultTypeInfo => null` as the documented escape hatch for bucket C commands.
- Per-namespace base classes under [tools/Azure.Mcp.Tools.*/src/Commands](../tools) (e.g., `BaseStorageCommand`, `BaseSqlCommand`, …) — widen each to take `TResult`.
- [core/Microsoft.Mcp.Core/src/Models/Command/CommandResponse.cs](../core/Microsoft.Mcp.Core/src/Models/Command/CommandResponse.cs) and the call-tool path — surface the typed payload to `CallToolResult.StructuredContent`.
- All Phase 2 command files plus their `*JsonContext` and unit tests.
- All namespace commands under [tools](../tools) and their `*JsonContext.cs` files for Phase 3.

**Verification**

1. `dotnet build` and `./eng/scripts/Build-Local.ps1 -IncludeNative` (AOT) green. Within `BaseCommand<TOptions, TResult>` itself, omitting `ResultTypeInfo` is a compile error — verify by removing the override on a temporary command and confirming the build fails. Bucket C commands deliberately stay on `BaseCommand<TOptions>` and inherit the `null` default.
2. New `ToolOutputSchemaTests` plus integration test that calls a representative tool, asserts `CallToolResult.StructuredContent` is populated and validates against the advertised schema.
3. Run `azmcp server start`, hit `tools/list`, and spot-check 5 tools with diverse shapes (list, single, blob upload, vmss multi-shape, sql wrapped list) — every tool exposes a valid `outputSchema`. Confirm bucket C commands (`ServiceStartCommand`, `PluginTelemetryCommand`) advertise no `outputSchema`, as expected.
4. `./eng/scripts/Test-Code.ps1` and `.\eng\common\spelling\Invoke-Cspell.ps1` clean.

**Decisions**

- **Schema source:** `JsonSchemaExporter` over each command's result `JsonTypeInfo` (reuses existing source-gen contexts, no new reflection).
- **Result exposure:** `BaseCommand<TOptions, TResult>` generic with an abstract `JsonTypeInfo<TResult> ResultTypeInfo` property and a typed `SetResult` helper. Keeps the success-payload contract in the type system, symmetric with `BaseCommand<TOptions>`. Rejected the alternative of a virtual `JsonTypeInfo? ResultTypeInfo` property on `IBaseCommand` because the invariant "declared result type matches what the command actually writes" would live only in a runtime discovery test instead of being compiler-enforced, and the asymmetry with the existing `TOptions` generic is unprincipled. We pay the one-time generic-parameter cascade through intermediate base classes to get a permanent compile-time guarantee.
- **Schema scope:** describes the `Results` payload only (the `*CommandResult` record), matching what `structuredContent` carries.
- **Multi-result policy:** consolidate into one record per command with optional fields.
- **Non-standard single-result policy:** wrap raw lists/primitives and `Response.Message`-only commands in a dedicated `*CommandResult` record (bucket B).
- **Escape hatch:** `IBaseCommand.ResultTypeInfo => null` stays as a documented opt-out for pure passthrough/lifecycle commands (bucket C: `ServiceStartCommand`, `PluginTelemetryCommand`). It is **not** a transitional shim.
- **structuredContent:** populated on `CallToolResult` so clients can validate responses against `outputSchema`.
- **Coverage:** compile-time — once a command derives from `BaseCommand<TOptions, TResult>`, the schema is mandatory by definition. Bucket C commands deliberately stay on `BaseCommand<TOptions>`.
- **Out of scope:** synthesizing schemas for proxied external tools (we only forward whatever they declare); changing the `CommandResponse` envelope; backward-compat shims for renamed Phase 2 result records.

**Further considerations**

1. **Rollout cadence:** Phase 1 + one pilot namespace (recommend Storage) in the same PR to prove end-to-end flow — including widening that namespace's intermediate base classes to take `TResult` — then fan out per-namespace PRs. Option A: pilot inside Phase 1 PR (recommended). Option B: pure infrastructure PR first, pilot separately.
2. **`JsonSchemaExporter` post-processing:** it emits `$schema` and draft-2020-12 keywords that the SDK's `McpJsonUtilities.IsValidMcpToolSchema` may reject. We need an allowlist/trim pass with explicit test coverage. Recommend implementing this in Phase 1 with dedicated unit tests rather than discovering edge cases per-namespace.
3. **Non-object roots are not theoretical.** A handful of commands return `List<string>`, `string`, or `List<JsonNode>` directly from their `JsonTypeInfo`, and `JsonSchemaExporter` produces `{type:array}`/`{type:string}` roots for these — which `IsValidMcpToolSchema` rejects. The wrap-under-`value` fallback in step 1 handles these as defense-in-depth, but the cleaner fix is to wrap each such payload in a `*CommandResult` record at the source as part of Phase 2 normalization. Either approach produces a valid schema; the record-at-source approach also gives the field a meaningful name in `structuredContent`.
4. **`additionalProperties` on output schemas:** input schemas set it to `false` for OpenAI strict mode. For outputs, that would break clients when we add server-side fields. Recommend leaving it unset (permissive) on output schemas; flag this as a one-line decision in the changelog.
5. **Generic-parameter cascade scope:** every intermediate base class used by a namespace (`SubscriptionCommand<TOptions>`, `BaseStorageCommand<TOptions>`, `BaseSqlCommand<TOptions>`, …) needs to widen to `<TOptions, TResult>`. Scope each Phase 3 PR to one namespace's base hierarchy so the diff stays bounded; the change is a pure type-parameter add that the compiler verifies exhaustively.
