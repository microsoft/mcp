# Supporting `outputSchema` in the Azure MCP Server

## Background

The MCP specification (starting with version `2025-03-26`) introduced two related features for structured tool output:

1. **`outputSchema`** on `Tool` — a JSON Schema describing the shape of a tool's structured response.
2. **`structuredContent`** on `CallToolResult` — the actual structured JSON payload returned alongside (or instead of) the existing `content` blocks.

The upstream `ModelContextProtocol` NuGet package (v1.0.0) already supports both of these:

- `Tool.OutputSchema` — `JsonElement?`
- `CallToolResult.StructuredContent` — `JsonElement?`

The MCPB CLI (.NET, v0.3.5) serializes the full `Tool` object via `tool.ProtocolTool` → `JsonElement` → `FilterNullProperties()`. This is schema-agnostic: if `OutputSchema` is non-null on the `Tool`, it will be preserved in the manifest. **No changes to the MCPB CLI are required.**

However, the Azure MCP Server currently:

- Advertises protocol version `"2024-11-05"`, which predates `outputSchema`.
- Never sets `Tool.OutputSchema` during tool registration.
- Never sets `CallToolResult.StructuredContent` in tool call responses.
- Wraps the entire `CommandResponse` (including status, message, and results) as a single `TextContentBlock.Text` string.

This document outlines the changes needed so that `outputSchema` is populated on tools at registration time and flows through to the MCPB CLI manifest.

---

## Current Architecture

### How `inputSchema` is produced today

1. Each command class (e.g., `AccountGetCommand`) extends `BaseCommand<TOptions>` and calls `RegisterOptions(Command)` to declare its `System.CommandLine.Option` parameters.
2. At server startup, `CommandFactoryToolLoader.GetTool()` iterates over these options and builds a `ToolInputSchema` object (type, properties, required), then serializes it into `Tool.InputSchema`:
   ```csharp
   // CommandFactoryToolLoader.cs — GetTool()
   tool.InputSchema = JsonSerializer.SerializeToElement(schema, ServerJsonContext.Default.ToolInputSchema);
   ```
3. `NamespaceToolLoader.CreateToolFromCommand()` follows the same pattern.
4. When `mcpb pack --update` runs, it launches the server, sends `tools/list`, and captures the response (including `InputSchema`) into `manifest.json`.

### How tool results are produced today

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

The `JsonTypeInfo` for each command's result type **already exists** at call time (via the per-toolset `JsonSerializerContext`), but it is only used during response serialization.  To produce `outputSchema` at registration time, this type information needs to be made available during tool loading.

---

## Required Changes

### 1. Bump Protocol Version

**File:** `core/Microsoft.Mcp.Core/src/Areas/Server/Commands/ServiceCollectionExtensions.cs`

```diff
- mcpServerOptions.ProtocolVersion = "2024-11-05";
+ mcpServerOptions.ProtocolVersion = "2025-03-26";
```

This is required for clients to recognize that the server supports `outputSchema` and `structuredContent`.

### 2. Create a `ToolOutputSchema` Model

**New file:** `core/Microsoft.Mcp.Core/src/Areas/Server/Models/ToolOutputSchema.cs`

A JSON Schema representation for the output. The structure mirrors `ToolInputSchema` since both are JSON Schema objects:

```csharp
using System.Text.Json.Serialization;

namespace Microsoft.Mcp.Core.Areas.Server.Models;

/// <summary>
/// Represents the JSON schema for a tool's output (structured content).
/// </summary>
public sealed class ToolOutputSchema
{
    [JsonPropertyName("type")]
    public string Type { get; init; } = "object";

    [JsonPropertyName("properties")]
    public Dictionary<string, ToolPropertySchema> Properties { get; init; } = new();

    [JsonPropertyName("required")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string[]? Required { get; set; }
}
```

> **Alternative:** Reuse `ToolInputSchema` directly (rename to `ToolSchema` or use as-is). Structurally they are identical since both are JSON Schema `"object"` definitions. A separate type provides clearer intent.

### 3. Register `ToolOutputSchema` in AOT Serialization Context

**File:** `core/Microsoft.Mcp.Core/src/Areas/Server/ServerJsonContext.cs`

```diff
  [JsonSerializable(typeof(ToolInputSchema))]
+ [JsonSerializable(typeof(ToolOutputSchema))]
  [JsonSerializable(typeof(ToolPropertySchema))]
```

### 4. Expose Output Type Metadata on Commands

Commands need a way to declare their output type at registration time. Two approaches:

#### Option A: New Interface (Recommended)

**New file:** `core/Microsoft.Mcp.Core/src/Commands/IOutputSchemaProvider.cs`

```csharp
using System.Text.Json;

namespace Microsoft.Mcp.Core.Commands;

/// <summary>
/// Implemented by commands that declare a structured output schema.
/// </summary>
public interface IOutputSchemaProvider
{
    /// <summary>
    /// Returns the JSON Schema describing this command's structured output.
    /// </summary>
    JsonElement GetOutputSchema();
}
```

Commands opt in by implementing this interface. The tool loaders check for it during tool creation:

```csharp
// In CommandFactoryToolLoader.GetTool() / NamespaceToolLoader.CreateToolFromCommand()
if (command is IOutputSchemaProvider outputProvider)
{
    tool.OutputSchema = outputProvider.GetOutputSchema();
}
```

**Pros:** Backward-compatible — existing commands that don't implement the interface are unaffected; no forced changes across 100+ commands.

**Cons:** Each command must manually implement the interface and build the schema.

#### Option B: Derive Schema from `JsonTypeInfo`

Rather than having each command hand-author a schema, leverage the `JsonTypeInfo<T>` that commands already use for serialization. Add a static helper that generates a JSON Schema from the source-generated type info.

**New file:** `core/Microsoft.Mcp.Core/src/Areas/Server/Commands/OutputSchemaGenerator.cs`

```csharp
using System.Text.Json;
using System.Text.Json.Serialization.Metadata;
using Microsoft.Mcp.Core.Areas.Server.Models;

namespace Microsoft.Mcp.Core.Areas.Server.Commands;

/// <summary>
/// Generates a ToolOutputSchema from a JsonTypeInfo, reflecting
/// the shape of a command's result type for use as outputSchema.
/// </summary>
public static class OutputSchemaGenerator
{
    public static JsonElement Generate(JsonTypeInfo typeInfo)
    {
        var schema = new ToolOutputSchema();

        if (typeInfo is JsonTypeInfo objectInfo
            && objectInfo.Kind == JsonTypeInfoKind.Object)
        {
            foreach (var prop in objectInfo.Properties)
            {
                schema.Properties[prop.Name] = TypeToJsonTypeMapper
                    .CreatePropertySchema(prop.PropertyType, description: null);
            }
        }

        return JsonSerializer.SerializeToElement(schema, ServerJsonContext.Default.ToolOutputSchema);
    }
}
```

Then the command interface is extended to expose the result `JsonTypeInfo`:

**File:** `core/Microsoft.Mcp.Core/src/Commands/IBaseCommand.cs`

```diff
+ /// <summary>
+ /// Gets the JsonTypeInfo for this command's result type, if available.
+ /// Used to auto-generate outputSchema.
+ /// </summary>
+ JsonTypeInfo? ResultTypeInfo => null;
```

With a default interface member (`=> null`), all existing commands remain compatible. Commands that want output schema override it:

```csharp
// In AccountGetCommand
public override JsonTypeInfo? ResultTypeInfo =>
    StorageJsonContext.Default.AccountGetCommandResult;
```

The tool loaders then call:

```csharp
if (command.ResultTypeInfo is { } typeInfo)
{
    tool.OutputSchema = OutputSchemaGenerator.Generate(typeInfo);
}
```

**Pros:** Eliminates hand-authoring; schema stays in sync with actual result types; commands opt in with a single property.

**Cons:** The generated schema may be shallow (only top-level properties with types, no nested descriptions). May require refinement for complex nested models.

### 5. Set `OutputSchema` in Tool Loaders

**File:** `core/Microsoft.Mcp.Core/src/Areas/Server/Commands/ToolLoading/CommandFactoryToolLoader.cs`

In the `GetTool()` method, after setting `InputSchema`:

```diff
  tool.InputSchema = JsonSerializer.SerializeToElement(schema, ServerJsonContext.Default.ToolInputSchema);

+ // Set output schema if the command provides one
+ if (command is IOutputSchemaProvider outputProvider)
+ {
+     tool.OutputSchema = outputProvider.GetOutputSchema();
+ }

  return tool;
```

Or, if using Option B (JsonTypeInfo-based):

```diff
  tool.InputSchema = JsonSerializer.SerializeToElement(schema, ServerJsonContext.Default.ToolInputSchema);

+ if (command.ResultTypeInfo is { } typeInfo)
+ {
+     tool.OutputSchema = OutputSchemaGenerator.Generate(typeInfo);
+ }

  return tool;
```

**File:** `core/Microsoft.Mcp.Core/src/Areas/Server/Commands/ToolLoading/NamespaceToolLoader.cs`

Apply the same change in `CreateToolFromCommand()`.

### 6. (Optional) Populate `StructuredContent` in Tool Call Handlers

While not strictly needed for `outputSchema` to appear in the manifest, returning `StructuredContent` in `CallToolResult` completes the structured output story. This allows MCP clients to consume typed JSON instead of parsing the `TextContentBlock` string.

**File:** `core/Microsoft.Mcp.Core/src/Areas/Server/Commands/ToolLoading/CommandFactoryToolLoader.cs`

In the call handler, after serializing the command response:

```diff
  var commandResponse = await command.ExecuteAsync(commandContext, commandOptions, cancellationToken);
  var jsonResponse = JsonSerializer.Serialize(commandResponse, ModelsJsonContext.Default.CommandResponse);
  var isError = commandResponse.Status < HttpStatusCode.OK || commandResponse.Status >= HttpStatusCode.Ambiguous;

  return new CallToolResult
  {
      Content = [new TextContentBlock { Text = jsonResponse }],
+     StructuredContent = commandResponse.Results != null
+         ? SerializeStructuredContent(commandResponse)
+         : null,
      IsError = isError
  };
```

Where `SerializeStructuredContent` uses the existing `ResponseResult.Write()` method to produce a `JsonElement`:

```csharp
private static JsonElement? SerializeStructuredContent(CommandResponse response)
{
    if (response.Results is null) return null;

    using var buffer = new System.Buffers.ArrayBufferWriter<byte>();
    using var writer = new Utf8JsonWriter(buffer);
    response.Results.Write(writer);
    writer.Flush();
    return JsonDocument.Parse(buffer.WrittenMemory).RootElement.Clone();
}
```

Apply the same change in `NamespaceToolLoader`'s call handler.

> **Note:** The MCP spec requires that if `outputSchema` is declared, the `structuredContent` MUST validate against that schema. This coupling means both features should be enabled together per command, gated behind the same opt-in mechanism.

---

## Rollout Strategy

### Phase 1: outputSchema Only (Manifest Enrichment)

Minimum changes to get `outputSchema` into the MCPB-generated manifest:

| # | Change | File(s) |
|---|--------|---------|
| 1 | Bump protocol version | `ServiceCollectionExtensions.cs` |
| 2 | Create `ToolOutputSchema` model | New `ToolOutputSchema.cs` |
| 3 | Register in AOT context | `ServerJsonContext.cs` |
| 4 | Add `IOutputSchemaProvider` or `ResultTypeInfo` | New interface or `IBaseCommand.cs` |
| 5 | Set `Tool.OutputSchema` in tool loaders | `CommandFactoryToolLoader.cs`, `NamespaceToolLoader.cs` |
| 6 | Opt in select commands | Individual command files |

After Phase 1, running `mcpb pack --update` will produce a manifest with `outputSchema` entries.

### Phase 2: StructuredContent (Runtime)

| # | Change | File(s) |
|---|--------|---------|
| 7 | Populate `CallToolResult.StructuredContent` | `CommandFactoryToolLoader.cs`, `NamespaceToolLoader.cs` |
| 8 | Ensure schema/content consistency | Validation tests |

### Phase 3: Broad Adoption

- Migrate all commands to declare output types.
- Consider making `ResultTypeInfo` abstract (`required`) in a future `BaseCommand<TOptions, TResult>` variant.
- Add tooling/analyzers to enforce output schema coverage.

---

## Files Changed Summary

| File | Change Type | Description |
|------|-------------|-------------|
| `core/Microsoft.Mcp.Core/src/Areas/Server/Commands/ServiceCollectionExtensions.cs` | Modify | Protocol version bump |
| `core/Microsoft.Mcp.Core/src/Areas/Server/Models/ToolOutputSchema.cs` | **New** | Output schema model |
| `core/Microsoft.Mcp.Core/src/Areas/Server/ServerJsonContext.cs` | Modify | Register `ToolOutputSchema` for AOT |
| `core/Microsoft.Mcp.Core/src/Commands/IOutputSchemaProvider.cs` | **New** | Opt-in interface (Option A) |
| `core/Microsoft.Mcp.Core/src/Commands/IBaseCommand.cs` | Modify | Add `ResultTypeInfo` property (Option B) |
| `core/Microsoft.Mcp.Core/src/Areas/Server/Commands/OutputSchemaGenerator.cs` | **New** | Schema generation from `JsonTypeInfo` (Option B) |
| `core/Microsoft.Mcp.Core/src/Areas/Server/Commands/ToolLoading/CommandFactoryToolLoader.cs` | Modify | Set `Tool.OutputSchema` and optionally `StructuredContent` |
| `core/Microsoft.Mcp.Core/src/Areas/Server/Commands/ToolLoading/NamespaceToolLoader.cs` | Modify | Same as above |
| Per-toolset command files (e.g., `AccountGetCommand.cs`) | Modify | Opt in to output schema |

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

## Testing Considerations

1. **Unit tests for `OutputSchemaGenerator`** — verify schema generation from known `JsonTypeInfo` types.
2. **Unit tests for tool loaders** — verify `Tool.OutputSchema` is set when a command provides output type info, and null when it doesn't.
3. **Integration test** — run `tools/list` against the server and verify `outputSchema` appears on opted-in tools.
4. **MCPB manifest test** — run `mcpb pack --update` and verify the generated manifest contains `outputSchema` entries.
5. **Schema/content consistency** — if `structuredContent` is implemented, validate that the runtime response conforms to the declared schema.
6. **AOT compatibility** — ensure `ToolOutputSchema` serialization works under native AOT (`Build-Local.ps1 -BuildNative`).
