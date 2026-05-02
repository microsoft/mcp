# Plan: Modernize `inputSchema` generation

Today the `inputSchema` advertised on each MCP tool is built by hand-rolled code that predates the maturity of the `ModelContextProtocol` C# SDK and the `JsonSchemaExporter` that ships in `System.Text.Json`. This plan migrates `inputSchema` generation onto the SDK/runtime primitives so we stop maintaining a private re-implementation, close several spec gaps, and share one schema-generation pipeline with the upcoming `outputSchema` work.

## Current state

Three pieces of bespoke infrastructure in `core/Microsoft.Mcp.Core`:

- [ToolInputSchema.cs](core/Microsoft.Mcp.Core/src/Areas/Server/Models/ToolInputSchema.cs) — DTO for `{ type, properties, required, additionalProperties }`.
- [ToolPropertySchema.cs](core/Microsoft.Mcp.Core/src/Areas/Server/Models/ToolPropertySchema.cs) — DTO for `{ type, description, items, enum }`.
- [TypeToJsonTypeMapper.cs](core/Microsoft.Mcp.Core/src/Areas/Server/Commands/TypeToJsonTypeMapper.cs) — reflective C# → JSON-Schema type mapping; called per `System.CommandLine.Option`.

Wired together in [CommandFactoryToolLoader.GetTool()](core/Microsoft.Mcp.Core/src/Areas/Server/Commands/ToolLoading/CommandFactoryToolLoader.cs): walk `Command.Options`, build a `ToolInputSchema`, serialize it with `JsonSerializer.SerializeToElement(...)`, assign to `Tool.InputSchema`.

## Findings

### Spec conformance

The MCP 2025-11-25 spec requires `inputSchema` to be a JSON Schema with `type: "object"`, plus `properties` and optional `required`. Our schema satisfies that minimum. Several encouraged features the SDK already supports are missing:

| Feature | Status today |
| --- | --- |
| `type: "object"` root | ✅ |
| `properties` map | ✅ |
| `required` array | ✅ |
| `additionalProperties` | ✅ (set to `false` for OpenAI strict mode) |
| `description` per property | ✅ (from `Option.Description`) |
| `enum` for enum types | ✅ |
| `items` for arrays (recursive) | ✅ |
| `format` for `DateTime`, `DateTimeOffset`, `Guid`, `Uri`, `TimeSpan` | ❌ — collapsed to plain `"type": "string"` |
| `default` | ❌ |
| Numeric constraints (`minimum`, `maximum`, `multipleOf`) | ❌ |
| String constraints (`minLength`, `maxLength`, `pattern`) | ❌ |
| `nullable` / union with `null` | ❌ — nullable is unwrapped silently; the schema doesn't reflect it |
| Nested object properties (option types beyond primitives) | ❌ — any non-primitive becomes a flat `{ "type": "object" }` with no `properties` |
| Dictionary keys/values | ⚠️ flat `"object"` with no shape |
| Per-enum-value descriptions | ❌ |
| Schema validation against MCP rules (`McpJsonUtilities.IsValidMcpToolSchema`) | ❌ — we rely on the `Tool.InputSchema` setter to throw at assignment time, so failures surface only during server startup |

### Are we reinventing the wheel?

Partially, yes:

1. **`ToolInputSchema` and `ToolPropertySchema` duplicate JSON-Schema shapes that the SDK already accepts directly.** The SDK's `Tool.InputSchema` is just a `JsonElement`; we don't need typed DTOs to feed it.
2. **`TypeToJsonTypeMapper` is a less-capable re-implementation of `System.Text.Json.Schema.JsonSchemaExporter`** (built into `System.Text.Json` as of .NET 9, AOT-safe via `JsonTypeInfo`). The exporter handles every case the mapper handles, plus `format`, nested objects, nullable unions, references, and validation-attribute hints. `Microsoft.Extensions.AI`'s `AIJsonUtilities.CreateJsonSchema` adds further attribute handling on top.
3. **We don't validate generated schemas with `McpJsonUtilities.IsValidMcpToolSchema` ourselves.** The `Tool.InputSchema` setter does, but only at assignment time during startup — a discovery test would surface invalid schemas at build time.

### What's *not* worth changing

The MCP SDK's automatic schema path (`McpServerTool.Create(Delegate)`) operates on `MethodInfo` parameters, not on `System.CommandLine.Option` collections. Switching the entire command model away from `System.CommandLine` to plain methods is a much larger architectural change with little incremental schema benefit. We keep the existing command model and keep our own per-option iteration; we just upgrade the *type-to-fragment* step inside it.

## Plan

**Steps**

1. **Replace `TypeToJsonTypeMapper.CreatePropertySchema` with `JsonSchemaExporter`.** For each `System.CommandLine.Option`, call `JsonSchemaExporter.GetJsonSchemaAsNode(jsonOptions, option.ValueType)` and use the returned `JsonNode` as the property schema. Keep the per-option iteration over `command.GetCommand().Options` as-is. Side effect: nullable, nested-object, and `format` gaps close automatically.
2. **Build the root schema as a `JsonObject` directly** (instead of through `ToolInputSchema`). Compose `{ type: "object", properties, required, additionalProperties: false }` as a `JsonObject` and assign it to `Tool.InputSchema` via `JsonElement`. Delete `ToolInputSchema` and `ToolPropertySchema` once nothing else depends on them.
3. **Delete `TypeToJsonTypeMapper`** once all callers (input schema generation, the new output-schema path, and any test code) are off it. Confirm with a workspace-wide search before removal.
4. **Add a discovery test** in `core/Microsoft.Mcp.Core/tests` that iterates every command discovered by `CommandFactory` and asserts the generated `inputSchema` (and, once Phase 1 of the outputSchema plan lands, `outputSchema`) passes `ModelContextProtocol.Protocol.McpJsonUtilities.IsValidMcpToolSchema`.
5. **Preserve special-case handling.** The current `IsRawMcpToolInputOption` branch in `CommandFactoryToolLoader.GetTool()` (where a single option's `Description` *is* the raw schema JSON) keeps its existing behavior; only the standard option path moves to `JsonSchemaExporter`.
6. **Keep the OpenAI strict-mode contract.** Continue setting `additionalProperties: false` on input schemas and continue normalizing option names via `NameNormalization.NormalizeOptionName(option.Name)`. The exporter's output for primitives never includes `additionalProperties`, so the root-level setting still applies cleanly.

**Relevant files**

- [core/Microsoft.Mcp.Core/src/Areas/Server/Commands/ToolLoading/CommandFactoryToolLoader.cs](core/Microsoft.Mcp.Core/src/Areas/Server/Commands/ToolLoading/CommandFactoryToolLoader.cs) — replace the `ToolInputSchema`/`TypeToJsonTypeMapper` calls in `GetTool()` with `JsonSchemaExporter` + a `JsonObject` root.
- [core/Microsoft.Mcp.Core/src/Areas/Server/Commands/TypeToJsonTypeMapper.cs](core/Microsoft.Mcp.Core/src/Areas/Server/Commands/TypeToJsonTypeMapper.cs) — delete.
- [core/Microsoft.Mcp.Core/src/Areas/Server/Models/ToolInputSchema.cs](core/Microsoft.Mcp.Core/src/Areas/Server/Models/ToolInputSchema.cs) and [ToolPropertySchema.cs](core/Microsoft.Mcp.Core/src/Areas/Server/Models/ToolPropertySchema.cs) — delete.
- [core/Microsoft.Mcp.Core/src/Areas/Server/Models/ServerJsonContext.cs](core/Microsoft.Mcp.Core/src/Areas/Server/Models/ServerJsonContext.cs) — drop the `ToolInputSchema` registration; ensure `JsonObject` is registered if not already.
- [core/Microsoft.Mcp.Core/tests/Microsoft.Mcp.Core.UnitTests/Areas/Server/Models/ToolInputSchemaTests.cs](core/Microsoft.Mcp.Core/tests/Microsoft.Mcp.Core.UnitTests/Areas/Server/Models/ToolInputSchemaTests.cs) — replace with tests against the new generation pipeline (assert `IsValidMcpToolSchema`, assert specific shape for representative option types).
- Any other tests referencing `ToolInputSchema`, `ToolPropertySchema`, or `TypeToJsonTypeMapper` — migrate or delete.

**Verification**

1. `dotnet build` and `./eng/scripts/Build-Local.ps1 -BuildNative` (AOT) green.
2. New discovery test asserts every command's `inputSchema` passes `McpJsonUtilities.IsValidMcpToolSchema`.
3. Snapshot-style tests for representative option types: `string`, `int`, `bool`, `Guid`, `DateTimeOffset`, `Uri`, `enum`, `string[]`, nullable variants. Assert `format` appears where expected (e.g., `"format": "uuid"` for `Guid`).
4. Manually compare the generated `inputSchema` for a handful of commands (e.g., one storage, one compute, one keyvault) against `main` to confirm there are no unintended shape regressions; document any intentional changes.
5. Run `./eng/scripts/Test-Code.ps1` and `.\eng\common\spelling\Invoke-Cspell.ps1`.

**Decisions**

- **Schema source:** `System.Text.Json.Schema.JsonSchemaExporter` for both inputs and outputs (single pipeline, AOT-safe, no reflection beyond what `JsonTypeInfo` already encodes).
- **Schema shape:** built as a `JsonObject` directly; no typed DTO wrapper. `Tool.InputSchema` is `JsonElement` and accepts whatever we hand it.
- **`additionalProperties`:** stays `false` on input schemas (OpenAI strict mode). Output schemas remain unset (per the outputSchema plan) for forward compatibility.
- **`McpJsonUtilities.IsValidMcpToolSchema`:** enforced by a discovery test, not at runtime — the `Tool.InputSchema` setter remains as a defense-in-depth.
- **Command model:** keep `System.CommandLine`-based commands. Do **not** migrate to `McpServerTool.Create(Delegate)`.
- **Special cases:** the raw-MCP-tool-input pathway keeps its existing behavior.

**Out of scope**

- Surfacing constraint attributes (`[Range]`, `[MinLength]`, etc.) on options. The exporter will pick them up automatically if any command adds them later, but no command uses them today.
- Per-enum-value descriptions. Easy cleanup once we're on the exporter, but not blocking.
- Replacing the command model with `[McpServerTool]`-attributed methods. Not justified by the schema gap alone.

**Further considerations**

1. **Sequencing with the outputSchema plan.** The outputSchema plan (Phase 1) will already introduce `JsonSchemaExporter` into the codebase. Two reasonable orderings:
   - **A. Land this `inputSchema` modernization first**, then layer outputSchema on top. Smaller individual diffs; the outputSchema PR re-uses an existing helper. **Recommended.**
   - **B. Land both together in one PR.** Bigger blast radius but only one round of test churn for `ToolInputSchemaTests`. Acceptable if the diff stays reviewable.
2. **Behavioral diff risk.** The exporter's output is more accurate, which means it may not be byte-identical to today's output for some option types. We should produce a before/after diff for a representative set of tools as part of the PR description and call out any externally visible changes (e.g., new `format` keywords, nullable unions). This is a feature, not a regression, but downstream clients should be informed.
3. **Snapshot tests.** Consider adding a small set of golden-file snapshots for input schemas of stable commands, to catch unintended drift across future `System.Text.Json` updates.
