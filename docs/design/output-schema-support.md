# Supporting `outputSchema` in the Azure MCP Server

> **Status:** Phase 1 (outputSchema on all tools) and Phase 3 (broad adoption) are **implemented**.
> Phase 2 (runtime `structuredContent`) is not yet implemented.

---

## Background

The MCP specification (starting with version `2025-03-26`) introduced two related features for structured tool output:

1. **`outputSchema`** on `Tool` — a JSON Schema describing the shape of a tool's structured response.
2. **`structuredContent`** on `CallToolResult` — the actual structured JSON payload returned alongside (or instead of) the existing `content` blocks.

The upstream `ModelContextProtocol` NuGet package (v1.0.0) already supports both of these:

- `Tool.OutputSchema` — `JsonElement?`
- `CallToolResult.StructuredContent` — `JsonElement?`

The MCPB CLI (.NET, v0.3.5) serializes the full `Tool` object via `tool.ProtocolTool` → `JsonElement` → `FilterNullProperties()`. This is schema-agnostic: if `OutputSchema` is non-null on the `Tool`, it will be preserved in the manifest. **No changes to the MCPB CLI are required.**

However, the Azure MCP Server originally:

- Advertised protocol version `"2024-11-05"`, which predates `outputSchema`.
- Never set `Tool.OutputSchema` during tool registration.
- Never set `CallToolResult.StructuredContent` in tool call responses.
- Wrapped the entire `CommandResponse` (including status, message, and results) as a single `TextContentBlock.Text` string.

This document records the design options that were considered, the decision that was made, and the implementation details.

---

## Prior Architecture

### How `inputSchema` is produced

1. Each command class (e.g., `AccountGetCommand`) extends `BaseCommand<TOptions>` and calls `RegisterOptions(Command)` to declare its `System.CommandLine.Option` parameters.
2. At server startup, `CommandFactoryToolLoader.GetTool()` iterates over these options and builds a `ToolInputSchema` object (type, properties, required), then serializes it into `Tool.InputSchema`:
   ```csharp
   // CommandFactoryToolLoader.cs — GetTool()
   tool.InputSchema = JsonSerializer.SerializeToElement(schema, ServerJsonContext.Default.ToolInputSchema);
   ```
3. `NamespaceToolLoader.CreateToolFromCommand()` follows the same pattern.
4. When `mcpb pack --update` runs, it launches the server, sends `tools/list`, and captures the response (including `InputSchema`) into `manifest.json`.

### How tool results are produced

1. Commands call `ResponseResult.Create<T>(result, JsonTypeInfo<T>)` to wrap their result:
   ```csharp
   context.Response.Results = ResponseResult.Create(
       new(accounts?.Results ?? [], accounts?.AreResultsTruncated ?? false),
       StorageJsonContext.Default.AccountGetCommandResult);
   ```
2. The tool call handler serializes the entire `CommandResponse` (status + message + results) into a `TextContentBlock`:
   ```csharp
   var jsonResponse = JsonSerializer.Serialize(commandResponse, ModelsJsonContext.Default.CommandResponse);
   return new CallToolResult
   {
       Content = [new TextContentBlock { Text = jsonResponse }],
       IsError = isError
   };
   ```

### Key observation

The `JsonTypeInfo` for each command's result type **already exists** at call time (via the per-toolset `JsonSerializerContext`), but it is only used during response serialization. To produce `outputSchema` at registration time, this type information needs to be made available during tool loading.

---

## Design Options Considered

### Option A: New Interface (`IOutputSchemaProvider`)

Create a dedicated opt-in interface for commands to supply their output schema:

```csharp
public interface IOutputSchemaProvider
{
    JsonElement GetOutputSchema();
}
```

Commands opt in by implementing this interface. The tool loaders check for it during tool creation:

```csharp
if (command is IOutputSchemaProvider outputProvider)
{
    tool.OutputSchema = outputProvider.GetOutputSchema();
}
```

**Pros:**
- Backward-compatible — existing commands that don't implement the interface are unaffected; no forced changes across 100+ commands.

**Cons:**
- Each command must manually implement the interface and build the schema.
- Schemas can drift out of sync with the actual result types.

### Option B: Derive Schema from `JsonTypeInfo` ✅ Chosen

Rather than having each command hand-author a schema, leverage the `JsonTypeInfo<T>` that commands already use for serialization. Add a static helper that generates a JSON Schema from the source-generated type info.

Commands expose their result `JsonTypeInfo` via a property on `IBaseCommand`:

```csharp
// Default interface member — all existing commands remain compatible
JsonTypeInfo? ResultTypeInfo => null;
```

Commands opt in with a single override:

```csharp
public override JsonTypeInfo? ResultTypeInfo =>
    StorageJsonContext.Default.AccountGetCommandResult;
```

The tool loaders call:

```csharp
if (command.ResultTypeInfo is { } typeInfo)
{
    tool.OutputSchema = OutputSchemaGenerator.Generate(typeInfo);
}
```

**Pros:**
- Eliminates hand-authoring; schema stays in sync with actual result types.
- Commands opt in with a single property.
- Leverages existing `JsonSerializerContext` infrastructure (AOT-safe).

**Cons:**
- The generated schema may be shallow (only top-level properties with types, no nested descriptions).
- May require refinement for complex nested models.

### Decision

**Option B** was chosen because it eliminates manual schema maintenance and keeps output
schemas automatically in sync with result types. The per-toolset `JsonSerializerContext`
infrastructure already exists, so the incremental cost per command is a single property
override.

---

## Implementation

### How it works

1. **`ResultTypeInfo`** — a default interface member on `IBaseCommand` (`=> null`) and a
   `virtual` property on `BaseCommand<TOptions>`. Commands override it to point at their
   source-generated `JsonTypeInfo`:

   ```csharp
   public override JsonTypeInfo? ResultTypeInfo =>
       StorageJsonContext.Default.AccountGetCommandResult;
   ```

2. **`OutputSchemaGenerator`** — a static helper that accepts `JsonTypeInfo?`:
   - When non-null, walks `JsonTypeInfo.Properties` and builds a
     `ToolOutputSchema` with property names and JSON types.
   - When null, returns a **cached default** schema describing the generic `CommandResponse`
     envelope (status, message, results, duration). Uses `Lazy<JsonElement>` for thread-safety.

3. **Both tool loaders** (`CommandFactoryToolLoader` and `NamespaceToolLoader`)
   **unconditionally** set `tool.OutputSchema = OutputSchemaGenerator.Generate(command.ResultTypeInfo)`.
   Every tool gets an `outputSchema` — either a specific one or the default envelope.

4. **`InputSchemaGenerator`** — input schema generation was also extracted into a parallel
   static helper class, following the same pattern. Both loaders delegate to
   `InputSchemaGenerator.Generate(options)` instead of inline schema construction.

5. **Caching** — both `CommandFactoryToolLoader` and `NamespaceToolLoader` cache their
   `ListToolsResult` via a `_cachedListToolsResult` field with an early-return pattern,
   so schema generation only runs once.

### Default schema

When `ResultTypeInfo` is null, `OutputSchemaGenerator.Generate()` returns a cached
default schema describing the `CommandResponse` envelope:

```json
{
  "type": "object",
  "properties": {
    "status": { "type": "integer", "description": "HTTP status code." },
    "message": { "type": "string", "description": "Status or error message." },
    "results": { "type": "object", "description": "Command-specific result payload." },
    "duration": { "type": "integer", "description": "Execution duration in milliseconds." }
  }
}
```

This ensures **every tool** advertises an `outputSchema`, even if it's the generic envelope.

### Architecture diagrams

#### Output schema generation pipeline

```
Command.ResultTypeInfo ──► OutputSchemaGenerator.Generate(typeInfo)
                                    │
                         ┌──────────┴──────────┐
                    typeInfo != null         typeInfo == null
                         │                      │
              Walk JsonTypeInfo.Properties   Return cached default
              Build ToolOutputSchema         CommandResponse schema
                         │                      │
                         └──────────┬──────────┘
                                    ▼
                    JsonSerializer.SerializeToElement(schema)
                                    │
                                    ▼
                           tool.OutputSchema
```

#### Input schema generation (extracted)

Input schema generation was extracted from both loaders into `InputSchemaGenerator` for
consistency with `OutputSchemaGenerator`:

```
Command.Options ──► InputSchemaGenerator.Generate(options)
                            │
               ┌────────────┼────────────┐
          options.Count==1          normal options       no options
          && IsRawMcpToolInput          │                   │
               │               Build ToolInputSchema   Empty schema
         Parse JSON from       with properties/required
         option.Description
               │                        │                   │
               └────────────┬───────────┘───────────────────┘
                            ▼
                    tool.InputSchema
```

#### Caching in tool loaders

Both `CommandFactoryToolLoader` and `NamespaceToolLoader` cache the `ListToolsResult`:

```csharp
private ListToolsResult? _cachedListToolsResult;

// In ListToolsHandler:
if (_cachedListToolsResult is { } cached)
    return cached;
// ... build tools ...
_cachedListToolsResult = result;
return result;
```

This means schema generation only runs once per server lifetime, regardless of how many
`tools/list` requests are received.

---

## Files Changed

| File | Change Type | Description |
|------|-------------|-------------|
| `core/Microsoft.Mcp.Core/src/Areas/Server/Models/ToolOutputSchema.cs` | **New** | JSON Schema model (type, properties, required) |
| `core/Microsoft.Mcp.Core/src/Areas/Server/Commands/OutputSchemaGenerator.cs` | **New** | Generates outputSchema from `JsonTypeInfo?` with default fallback |
| `core/Microsoft.Mcp.Core/src/Areas/Server/Commands/InputSchemaGenerator.cs` | **New** | Extracted inputSchema generation (mirrors OutputSchemaGenerator) |
| `core/Microsoft.Mcp.Core/src/Commands/IBaseCommand.cs` | Modify | Added `JsonTypeInfo? ResultTypeInfo => null;` default member |
| `core/Microsoft.Mcp.Core/src/Commands/BaseCommand.cs` | Modify | Added `public virtual JsonTypeInfo? ResultTypeInfo => null;` |
| `core/Microsoft.Mcp.Core/src/Areas/Server/ServerJsonContext.cs` | Modify | Registered `ToolOutputSchema` for AOT |
| `core/Microsoft.Mcp.Core/src/Areas/Server/Commands/ToolLoading/CommandFactoryToolLoader.cs` | Modify | Uses `InputSchemaGenerator`, `OutputSchemaGenerator`, adds caching |
| `core/Microsoft.Mcp.Core/src/Areas/Server/Commands/ToolLoading/NamespaceToolLoader.cs` | Modify | Same refactoring as CommandFactoryToolLoader |
| 243 command files across ~40 toolsets | Modify | Added `ResultTypeInfo` override |

---

## Command Coverage

### Summary

| Category | Count |
|----------|-------|
| Total concrete commands | ~249 |
| Commands with specific `ResultTypeInfo` | **243** (98%) |
| Commands using default `CommandResponse` schema | **6** (2%) |

### Commands with specific `ResultTypeInfo` (223)

All commands that have a result type registered in a per-toolset `JsonSerializerContext` now
override `ResultTypeInfo`. This includes all commands across these toolsets:

- **Core:** Group, Subscription, ServiceInfo, ToolsList
- **Azure tools:** Acr, Advisor, Aks, AppConfig, AppLens, ApplicationInsights, AppService,
  Authorization, AzureBestPractices, AzureIsv, AzureMigrate, AzureTerraformBestPractices,
  BicepSchema, CloudArchitect, Communication, ConfidentialLedger, Compute, Cosmos,
  EventGrid, EventHubs, Extension, FileShares, Foundry, FunctionApp, Grafana, KeyVault,
  Kusto, LoadTesting, ManagedLustre, Marketplace, Monitor, MySql, Policy, Postgres,
  Pricing, Quota, Redis, ResourceHealth, Search, ServiceBus, ServiceFabric, SignalR,
  Speech, Sql, Storage, StorageSync, VirtualDesktop, Workbooks
- **Fabric tools:** OneLake, PublicApi

### Commands with multiple result types

Seven commands produce different result types depending on input parameters (e.g., single
vs. list). Since `ResultTypeInfo` is a single property, the **single-item result type** was
chosen as the representative schema:

| Command | Toolset | Selected `ResultTypeInfo` | Other result types |
|---------|---------|--------------------------|-------------------|
| `VmGetCommand` | Compute | `VmGetSingleResult` | `VmGetListResult` |
| `VmssGetCommand` | Compute | `VmssGetSingleResult` | `VmssGetListResult`, `VmssGetVmInstanceResult` |
| `NamespaceGetCommand` | EventHubs | `NamespaceGetCommandResult` | `NamespacesGetCommandResult` |
| `WebTestsGetCommand` | Monitor | `WebTestsGetCommandResult` | `WebTestsGetCommandListResult` |
| `AutoexportJobGetCommand` | ManagedLustre | `AutoexportJobGetResult` | `AutoexportJobListResult` |
| `AutoimportJobGetCommand` | ManagedLustre | `AutoimportJobGetResult` | `AutoimportJobListResult` |
| `ImportJobGetCommand` | ManagedLustre | `ImportJobGetResult` | `ImportJobListResult` |

### Commands without `ResultTypeInfo` (6)

These commands do not produce structured result objects via `ResponseResult.Create`.
They receive the default `CommandResponse` envelope schema.

#### Commands that do not produce typed results (6)

| Toolset | Command | Reason |
|---------|---------|--------|
| Deploy | `LogsGetCommand` | Unstructured text output |
| Deploy | `DiagramGenerateCommand` | Raw MCP tool input, unstructured output |
| Deploy | `RulesGetCommand` | Unstructured text output |
| Deploy | `GuidanceGetCommand` | Unstructured text output |
| Deploy | `GetCommand` | Unstructured text output |
| Microsoft.Mcp.Core | `ServiceStartCommand` | Server lifecycle, not a data command |

These commands cannot be opted in because they do not produce structured result
objects via `ResponseResult.Create`.

---

## Rollout Strategy

### Phase 1: outputSchema Only (Manifest Enrichment) — ✅ Complete

| # | Change | File(s) | Status |
|---|--------|---------|--------|
| 1 | Bump protocol version | `ServiceCollectionExtensions.cs` | ✅ Done |
| 2 | Create `ToolOutputSchema` model | New `ToolOutputSchema.cs` | ✅ Done |
| 3 | Register in AOT context | `ServerJsonContext.cs` | ✅ Done |
| 4 | Add `ResultTypeInfo` property | `IBaseCommand.cs`, `BaseCommand.cs` | ✅ Done |
| 5 | Set `Tool.OutputSchema` in tool loaders | `CommandFactoryToolLoader.cs`, `NamespaceToolLoader.cs` | ✅ Done |
| 6 | Opt in select commands | Storage commands (initial batch) | ✅ Done |

### Phase 2: StructuredContent (Runtime) — Not Yet Implemented

| # | Change | File(s) | Status |
|---|--------|---------|--------|
| 7 | Populate `CallToolResult.StructuredContent` | `CommandFactoryToolLoader.cs`, `NamespaceToolLoader.cs` | ⬜ Pending |
| 8 | Ensure schema/content consistency | Validation tests | ⬜ Pending |

Returning `StructuredContent` in `CallToolResult` completes the structured output story,
allowing MCP clients to consume typed JSON instead of parsing the `TextContentBlock` string.

```csharp
return new CallToolResult
{
    Content = [new TextContentBlock { Text = jsonResponse }],
    StructuredContent = commandResponse.Results != null
        ? SerializeStructuredContent(commandResponse)
        : null,
    IsError = isError
};
```

The MCP spec requires that if `outputSchema` is declared, `structuredContent` **must**
validate against that schema. Since our `outputSchema` describes the result type and
`structuredContent` would serialize the same result type, consistency is automatic.

### Phase 3: Broad Adoption — ✅ Complete

| # | Change | Status |
|---|--------|--------|
| 9 | All commands with registered `JsonTypeInfo` override `ResultTypeInfo` | ✅ 243 commands |
| 10 | All tools without `ResultTypeInfo` get default `CommandResponse` schema | ✅ 6 commands |

---

## MCPB CLI Compatibility

The MCPB CLI (.NET implementation, `mcpb.cli` v0.3.5, source: `github.com/asklar/mcpb` branch `user/asklar/dotnet`) captures the `tools/list` response as follows:

```csharp
// ManifestCommandHelpers.cs
var jsonTool = JsonSerializer.SerializeToElement(tool.ProtocolTool);
var filteredTool = FilterNullProperties(jsonTool);
```

`FilterNullProperties` recursively removes null-valued JSON properties but preserves all non-null values. Since the flow is:

1. `Tool` object → `JsonSerializer.SerializeToElement` → `JsonElement`
2. Filter nulls
3. Store in manifest

Any non-null `OutputSchema` property on the `Tool` will be preserved verbatim. **No changes to the MCPB CLI packaging or scripts are required.**

---

## Testing

| Test | Location | Status |
|------|----------|--------|
| `OutputSchemaGenerator` unit tests (6) | `Azure.Mcp.Core.UnitTests/.../OutputSchemaGeneratorTests.cs` | ✅ Passing |
| `CommandFactoryToolLoader` tests — all tools have outputSchema | `Microsoft.Mcp.Core.UnitTests/.../CommandFactoryToolLoaderTests.cs` | ✅ Passing |
| `NamespaceToolLoader` tests — all commands have outputSchema | `Microsoft.Mcp.Core.UnitTests/.../NamespaceToolLoaderTests.cs` | ✅ Passing |
| Caching tests (Assert.Same on repeated ListTools calls) | Both loader test files | ✅ Passing |
| Full solution build (0 errors, 0 warnings) | `dotnet build Microsoft.Mcp.slnx` | ✅ Passing |
| AOT compatibility | `Build-Local.ps1 -BuildNative` | Not yet verified |
| Schema/content consistency (Phase 2) | — | Not yet implemented |

---

## Future Improvements

- Enrich generated schemas with nested property descriptions for complex models.
- Add a Roslyn analyzer to enforce that all commands with `ResponseResult.Create` also
  override `ResultTypeInfo`.
