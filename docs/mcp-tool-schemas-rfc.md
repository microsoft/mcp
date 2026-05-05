# Aligning MCP tool schemas with the protocol — `inputSchema` cleanup + `outputSchema` support

> **Status:** RFC / seeking feedback
> **Audience:** engineers and PMs on the MCP team
> **TL;DR:** The MCP spec's optional `outputSchema` lets servers tell clients exactly what shape their tools' results will have. We don't advertise it today. While we're in the area, our hand-rolled `inputSchema` generator predates `System.Text.Json.Schema.JsonSchemaExporter` and the maturity of the `ModelContextProtocol` C# SDK — it's missing several spec features and duplicates code the platform now provides for free. This doc proposes (1) replacing the homegrown schema generator with `JsonSchemaExporter`, then (2) layering `outputSchema` on top of the same pipeline.

---

## Background

Each MCP tool we expose carries a JSON Schema describing its **inputs** (`inputSchema`) and, as of MCP 2025-11-25, optionally its **outputs** (`outputSchema`). When `outputSchema` is present, the server is also expected to populate `CallToolResult.StructuredContent` so clients can validate responses and route them through structured-output paths in their own pipelines.

We have ~150 command-based tools across `Azure.Mcp.Server`, `Fabric.Mcp.Server`, and friends. Today every one of them advertises `inputSchema`. None advertise `outputSchema`.

### What we do today (input)

```
System.CommandLine Options
        ↓ (per option)
  TypeToJsonTypeMapper         ← reflective, hand-rolled
        ↓
  ToolPropertySchema (DTO)     ← typed C# model of a JSON-Schema property
        ↓
  ToolInputSchema (DTO)        ← typed C# model of the JSON-Schema root
        ↓
  JsonSerializer.SerializeToElement
        ↓
  Tool.InputSchema (JsonElement)
```

Wired together in [`CommandFactoryToolLoader.GetTool()`](../core/Microsoft.Mcp.Core/src/Areas/Server/Commands/ToolLoading/CommandFactoryToolLoader.cs).

### Findings against the spec

| Spec Feature | Implemented Today |
| --- | --- |
| `type: "object"`, `properties`, `required`, `description`, `enum`, `items` | ✅ |
| `additionalProperties: false` for OpenAI strict-mode | ✅ |
| `format` for `Guid`, `Uri`, `DateTime`, `DateTimeOffset`, `TimeSpan` | ❌ collapsed to `"string"` |
| Nullable / union with `null` | ❌ silently unwrapped |
| Nested object properties | ❌ flat `{"type":"object"}` with no shape |
| Numeric / string constraints (`minimum`, `pattern`, …) | ❌ |
| Schema validation against `McpJsonUtilities.IsValidMcpToolSchema` | ❌ relies on the `Tool.InputSchema` setter throwing at startup |

The misses are mostly low-impact today because every option is a primitive, enum, or array of primitives. They become real bugs the moment a command takes a structured option type.

### Are we reinventing the wheel?

Yes, partially:

1. **`TypeToJsonTypeMapper` is a less-capable re-implementation of [`System.Text.Json.Schema.JsonSchemaExporter`](https://learn.microsoft.com/dotnet/standard/serialization/system-text-json/extract-schema)** — built into `System.Text.Json` since .NET 9, AOT-safe via `JsonTypeInfo`. The exporter handles every case the mapper handles, plus `format`, nested objects, nullable unions, references, and validation-attribute hints. `Microsoft.Extensions.AI`'s `AIJsonUtilities.CreateJsonSchema` adds further attribute handling on top.
2. **`ToolInputSchema` and `ToolPropertySchema` become unnecessary intermediate DTOs** once we move to `JsonSchemaExporter`. They exist today to give `TypeToJsonTypeMapper` a typed place to put its output before serialization. The exporter already returns a `JsonNode` with the right shape, and `Tool.InputSchema` accepts `JsonElement` directly — so keeping the DTOs would mean marshalling a `JsonNode` into a strongly-typed wrapper just to serialize it back out to JSON. They aren't duplicating anything that exists elsewhere; they simply stop carrying their weight.
3. **We don't run schemas through `McpJsonUtilities.IsValidMcpToolSchema` ourselves**; we discover invalid schemas only at server startup.

### What's *not* worth changing

The SDK's auto-schema path (`McpServerTool.Create(Delegate)`) takes a `MethodInfo`. Our commands are `System.CommandLine`-driven. Migrating the entire command model to attribute-decorated methods is a much larger refactor that the schema gap alone doesn't justify. We keep the existing command model and only upgrade the *type → schema fragment* step inside it.

---

## Proposal

Two related changes, ordered for review-ability:

1. **Modernize `inputSchema`** — replace the homegrown generator with `JsonSchemaExporter`. Delete `TypeToJsonTypeMapper`, `ToolInputSchema`, `ToolPropertySchema`. No protocol-visible regression; closes the table above.
2. **Add `outputSchema`** — declare a result `JsonTypeInfo` per command, run it through the same `JsonSchemaExporter`, populate `Tool.OutputSchema` and `CallToolResult.StructuredContent`. Normalize the handful of commands that today return more than one distinct result record into a single record with optional fields.

Detailed plans live in [`docs/modernize-input-schema.prompt.md`](modernize-input-schema.prompt.md) and [`docs/add-output-schema-support.prompt.md`](add-output-schema-support.prompt.md). This document is the cross-functional summary.

---

## Part 1 — Modernize `inputSchema`

### Before

```csharp
// CommandFactoryToolLoader.GetTool() — current
var schema = new ToolInputSchema();

foreach (var option in options)
{
    var propName = NameNormalization.NormalizeOptionName(option.Name);
    schema.Properties.Add(
        propName,
        TypeToJsonTypeMapper.CreatePropertySchema(option.ValueType, option.Description));
}

schema.Required = [.. options.Where(p => p.Required)
                             .Select(p => NameNormalization.NormalizeOptionName(p.Name))];

tool.InputSchema = JsonSerializer.SerializeToElement(schema, ServerJsonContext.Default.ToolInputSchema);
```

For an option of type `Guid?` with description `"Subscription ID."`, that produces:

```json
{
  "type": "object",
  "properties": {
    "subscription": { "type": "string", "description": "Subscription ID." }
  },
  "required": ["subscription"],
  "additionalProperties": false
}
```

### After

```csharp
// CommandFactoryToolLoader.GetTool() — proposed
var properties = new JsonObject();
var required = new JsonArray();

foreach (var option in options)
{
    var propName = NameNormalization.NormalizeOptionName(option.Name);
    var propSchema = JsonSchemaExporter.GetJsonSchemaAsNode(
        SchemaJsonOptions,
        option.ValueType);

    // Apply description from System.CommandLine onto the exported schema.
    if (!string.IsNullOrWhiteSpace(option.Description))
        propSchema["description"] = option.Description;

    properties[propName] = propSchema;
    if (option.Required) required.Add(propName);
}

var root = new JsonObject
{
    ["type"] = "object",
    ["properties"] = properties,
    ["required"] = required,
    ["additionalProperties"] = false,   // OpenAI strict-mode contract preserved
};

tool.InputSchema = JsonSerializer.SerializeToElement(root, ServerJsonContext.Default.JsonObject);
```

For the same `Guid?` option, the output now correctly carries `format` and a nullable union:

```json
{
  "type": "object",
  "properties": {
    "subscription": {
      "type": ["string", "null"],
      "format": "uuid",
      "description": "Subscription ID."
    }
  },
  "required": ["subscription"],
  "additionalProperties": false
}
```

### Files removed

- `core/Microsoft.Mcp.Core/src/Areas/Server/Commands/TypeToJsonTypeMapper.cs`
- `core/Microsoft.Mcp.Core/src/Areas/Server/Models/ToolInputSchema.cs`
- `core/Microsoft.Mcp.Core/src/Areas/Server/Models/ToolPropertySchema.cs`
- Their `[JsonSerializable]` registrations in `ServerJsonContext`

### New discovery test

```csharp
[Fact]
public void AllCommands_ProduceValidMcpToolSchemas()
{
    var tools = _factoryLoader.GetTools();   // exposed for tests

    Assert.All(tools, t =>
    {
        Assert.True(McpJsonUtilities.IsValidMcpToolSchema(t.InputSchema),
            $"Command '{t.Name}' produced an invalid inputSchema.");
    });
}
```

Catches invalid schemas at build time instead of at server startup.

---

## Part 2 — Add `outputSchema`

### Where the result type lives today

```csharp
// CommandResponse.cs (excerpt)
public sealed class ResponseResult
{
    private readonly object? _result;
    private readonly JsonTypeInfo _typeInfo;

    public static ResponseResult Create<T>(T result, JsonTypeInfo<T> typeInfo)
        => new(result, typeInfo);
}

// A typical command's success path
context.Response.Results = ResponseResult.Create(
    new(containers ?? []),
    StorageJsonContext.Default.ContainerListCommandResult);
```

The `JsonTypeInfo` is already source-generated and AOT-safe. We just don't expose it anywhere the loader can see at startup.

### Surface a typed result token on commands

Extend `BaseCommand<TOptions>` with a second type parameter that names the success-payload shape:

```csharp
// BaseCommand.cs
public abstract class BaseCommand<TOptions, TResult> : BaseCommand<TOptions>
    where TOptions : class, new()
    where TResult : class
{
    /// <summary>
    /// JsonTypeInfo describing the successful result payload. Concrete commands
    /// supply this; the loader uses it to derive the tool's outputSchema.
    /// </summary>
    protected abstract JsonTypeInfo<TResult> ResultTypeInfo { get; }

    /// <summary>
    /// Exposed to the tool loader for outputSchema generation.
    /// </summary>
    JsonTypeInfo IBaseCommand.ResultTypeInfo => ResultTypeInfo;

    /// <summary>
    /// Strongly-typed helper for the success path. Replaces the loose
    /// ResponseResult.Create&lt;T&gt;(T, JsonTypeInfo&lt;T&gt;) call site.
    /// </summary>
    protected void SetResult(CommandContext context, TResult value)
        => context.Response.Results = ResponseResult.Create(value, ResultTypeInfo);
}
```

`IBaseCommand` gains a corresponding `JsonTypeInfo? ResultTypeInfo => null;` so the loader has one place to read from regardless of which base a command derives from. Commands that have no structured payload (rare — mostly proxy/passthrough cases) can keep deriving from `BaseCommand<TOptions>` and inherit the null.

Each concrete command picks up the contract by naming `TResult`:

```csharp
// e.g. Azure.Mcp.Tools.Storage/.../ContainerListCommand.cs
public sealed class ContainerListCommand
    : SubscriptionCommand<ContainerListOptions, ContainerListCommandResult>
{
    protected override JsonTypeInfo<ContainerListCommandResult> ResultTypeInfo
        => StorageJsonContext.Default.ContainerListCommandResult;

    public override async Task<CommandResponse> ExecuteAsync(...)
    {
        // ...
        SetResult(context, new ContainerListCommandResult(containers ?? []));
        return context.Response;
    }
}
```

### Why `BaseCommand<TOptions, TResult>` and not a virtual property

We considered surfacing the result type as a virtual `JsonTypeInfo? ResultTypeInfo => null;` property instead of a type parameter. We rejected that approach because it's strictly weaker on the dimensions that matter:

1. **The invariant belongs in the type system.** "This command produces results of shape `T`, described by `JsonTypeInfo<T>`" is a property of the command, not a runtime fact to be discovered and asserted. A virtual property models it as metadata you fetch and hope is consistent with what `ExecuteAsync` actually writes. A `TResult` parameter makes the mismatch unrepresentable — `SetResult(context, value)` only compiles when `value` has the declared shape, and the `JsonTypeInfo<TResult>` slot can't drift from it.
2. **Self-documenting command signatures.** `class VmGetCommand : BaseCommand<VmGetOptions, VmGetCommandResult>` tells a reader the full IO contract in the most prominent location in the file. The property variant spreads the same information across the class declaration, a separate property body, and the `ResponseResult.Create` call site, with no compiler-enforced consistency between them.
3. **Refactor safety compounds.** Result types *will* change shape — that's the point of the multi-result normalization in the next section. With a generic, renaming a result record propagates through the type system; the compiler points to every call site. With a property, the rename works in the property body but the schema and the emitted JSON drift apart silently until a discovery test catches it. Compile errors are strictly better than test failures for this class of mistake.
4. **Symmetric with `BaseCommand<TOptions>`.** We already decided that *options* deserve a generic parameter. Results are the same kind of thing on the other side of the I/O boundary. Treating one with a generic and the other with a virtual property is an asymmetry with no principled justification.

Two objections to address head-on:

- **"`ResponseResult` is type-erased at the envelope, so what's the point?"** The envelope *has* to be type-erased — it holds either a result or an `ExceptionResult`. The generic gives us compile-time safety on the success path (command → `ResponseResult.Create`), and the error path stays type-erased exactly as it is today. Two different concerns, two different mechanisms; the generic doesn't have to solve both to be the right choice for the one it does solve.
- **"The generic lies about the error path."** `TResult` describes the success payload. `ExceptionResult` describes failures. These are separate channels in the response envelope, and `IsError` distinguishes them — a `TResult` parameter declaring "when this command succeeds, the payload has shape T" isn't a lie about errors any more than `Task<User> GetUserAsync()` lies because it can throw. The success-path type and the error-path type are orthogonal.

#### Cost: the cascade refactor

The honest cost of this design is that `TResult` has to thread through every intermediate base class — `SubscriptionCommand<TOptions>` becomes `SubscriptionCommand<TOptions, TResult>`, `BaseStorageCommand<TOptions>` becomes `BaseStorageCommand<TOptions, TResult>`, and so on for every per-toolset base. Each layer either pins `TResult` to a concrete type or stays generic. That's a large, mechanical, repo-wide refactor — larger than the schema work itself.

We accept it because the safety guarantee is permanent and the refactor is one-time. The discovery-test alternative would re-encode the same invariant in test code that any future contributor can forget to extend, and would leave the next author of a command-base class wondering why options got generics but results didn't.

### Generate the schema and populate `StructuredContent`

```csharp
// CommandFactoryToolLoader.GetTool() — outputSchema branch
// ResultTypeInfo comes from IBaseCommand; non-null whenever the command derives
// from BaseCommand<TOptions, TResult>.
var resultTypeInfo = command.ResultTypeInfo;
if (resultTypeInfo is not null)
{
    var schema = JsonSchemaExporter.GetJsonSchemaAsNode(
        resultTypeInfo.Options,
        resultTypeInfo.Type);

    // additionalProperties intentionally NOT set on outputs — keeps server free to
    // add fields without breaking strict-mode clients.
    tool.OutputSchema = JsonSerializer.SerializeToElement(
        schema, ServerJsonContext.Default.JsonNode);
}
```

```csharp
// CommandFactoryToolLoader.CallToolHandler() — populate StructuredContent
if (command.ResultTypeInfo is not null && response.Results is { } results)
{
    return new CallToolResult
    {
        Content = [new TextContentBlock { Text = jsonResponse }],
        StructuredContent = results.ToJsonElement(),  // typed payload
        IsError = isError,
    };
}
```

A client now gets, for `azmcp storage container list`:

```jsonc
// tool advertisement
{
  "name": "azmcp-storage-container-list",
  "outputSchema": {
    "type": "object",
    "properties": {
      "containers": {
        "type": "array",
        "items": { "$ref": "#/$defs/ContainerInfo" }
      }
    },
    "required": ["containers"]
  }
}

// tool call response
{
  "content": [{ "type": "text", "text": "{...}" }],
  "structuredContent": { "containers": [{ "name": "logs", ... }] }
}
```

### Multi-result normalization

A small number of commands today return more than one distinct result record depending on inputs. Each gets consolidated into one record with optional fields. [`TemplateGetCommand`](../tools/Azure.Mcp.Tools.Functions/src/Commands/Template/TemplateGetCommand.cs) already does this and serves as the reference pattern.

| Command | Today | After |
| --- | --- | --- |
| `VmssGetCommand` | 3 records (`Single`, `List`, `VmInstance`) | 1 record with optional `Vmss?`, `VmssItem?`, `VmInstance?` |
| `VmGetCommand` | `Single` + `List` | 1 record with optional `Vms?`, `Vm?`, `InstanceView?` |
| `NamespaceGetCommand` (EventHubs) | Single + plural | 1 record |
| `WebTestsGetCommand` (Monitor) | Single + plural | 1 record |
| `ServerGetCommand` (Sql) | raw `List<SqlServer>` | wrapped in `ServerGetCommandResult` |
| `IndexQueryCommand` (Search) | raw `List<JsonElement>` | wrapped in `IndexQueryCommandResult` |
| `ImportJobGetCommand` (ManagedLustre) | `ImportJobGetResult` + `ImportJobListResult` | 1 record with optional `Job?`, `Jobs?` |
| `AutoimportJobGetCommand` (ManagedLustre) | `AutoimportJobGetResult` + `AutoimportJobListResult` | 1 record with optional `Job?`, `Jobs?` |
| `AutoexportJobGetCommand` (ManagedLustre) | `AutoexportJobGetResult` + `AutoexportJobListResult` | 1 record with optional `Job?`, `Jobs?` |
| `BlobUploadCommand` | `BlobUploadResult` (already an object) | rename to `BlobUploadCommandResult` for naming consistency (optional) |

Going-forward rule: every command's `ResponseResult.Create<T>` call site writes a `*CommandResult` record root — no raw lists or primitives.

### Example: consolidating `VmGetCommand`

**Before**

```csharp
internal record VmGetSingleResult(VmInfo Vm, VmInstanceView? InstanceView);
internal record VmGetListResult(List<VmInfo> Vms);

if (vmName is not null)
    context.Response.Results = ResponseResult.Create(
        new VmGetSingleResult(vm, instanceView),
        ComputeJsonContext.Default.VmGetSingleResult);
else
    context.Response.Results = ResponseResult.Create(
        new VmGetListResult(vms),
        ComputeJsonContext.Default.VmGetListResult);
```

**After**

```csharp
public sealed class VmGetCommand
    : SubscriptionCommand<VmGetOptions, VmGetCommandResult>
{
    internal record VmGetCommandResult(
        List<VmInfo>? Vms = null,
        VmInfo? Vm = null,
        VmInstanceView? InstanceView = null);

    protected override JsonTypeInfo<VmGetCommandResult> ResultTypeInfo
        => ComputeJsonContext.Default.VmGetCommandResult;

    public override async Task<CommandResponse> ExecuteAsync(...)
    {
        // ...
        SetResult(context, vmName is not null
            ? new VmGetCommandResult(Vm: vm, InstanceView: instanceView)
            : new VmGetCommandResult(Vms: vms));
        return context.Response;
    }
}
```

`outputSchema` for that command is now a single, well-typed object with three optional fields — clients can validate the response, and the schema documents both shapes the tool can return.

---

## Rollout

Suggested sequencing (each numbered item is one or more PRs):

1. **Modernize `inputSchema`.** Single PR. Includes the discovery test. Behavior diff documented in PR description; new `format` keywords and nullable unions are intentional.
2. **`outputSchema` core infra + base-class generics + Storage pilot.** Single PR. Introduces `BaseCommand<TOptions, TResult>`, threads `TResult` through the intermediate base classes used by Storage (`SubscriptionCommand`, `BaseStorageCommand`, …), adds the `JsonSchemaExporter` post-processor, populates `StructuredContent`, and migrates every Storage command end-to-end as proof.
3. **Phase 2 result-shape normalization.** One PR per command in the table below.
4. **Phase 3 per-toolset wiring.** One PR per toolset (~25 PRs). Each PR (a) widens that toolset's intermediate base classes to take `TResult`, then (b) reparents every command in the toolset onto `BaseCommand<TOptions, TResult>` (or the toolset-specific equivalent), supplies `ResultTypeInfo`, and verifies `*JsonContext` registrations. Compilation alone proves coverage — once a command derives from the generic base, omitting `ResultTypeInfo` is a compile error.
5. **Tighten and document.** Remove the temporary `ResultTypeInfo => null` default on `IBaseCommand` once every command derives from the generic base. Update changelog and `azmcp-commands.md`.

Estimated review surface: items 1 and 2 are the substantive infra PRs; items 3 and 4 are mechanical (rote rename + rote reparent) but unavoidable given the cascade.

---

## Risks

- **Behavioral diff in `inputSchema`.** `JsonSchemaExporter` produces more accurate schemas (e.g., `format: "uuid"` for `Guid`, `["string", "null"]` for nullable types). Strict downstream clients that pinned themselves to the current loose schema could break. Mitigation: announce in changelog; produce before/after diff for representative commands in the migration PR.
- **`JsonSchemaExporter` post-processing.** The exporter emits draft-2020-12 keywords (`$schema`, `$defs`, sometimes `title`). The MCP SDK's `IsValidMcpToolSchema` is permissive but rejects some shapes. Mitigation: a small allow-list trimmer with explicit tests, landed in step 1.
- **`additionalProperties` on outputs.** Inputs use `false` for OpenAI strict mode. We intentionally leave it **unset** on outputs so the server can add fields later without breaking strict-mode clients consuming `structuredContent`. Documented in the changelog.
- **Result-record renames in Phase 2.** Eight commands change result-record names. The JSON shape is a strict superset, so existing clients reading the text content keep working. Anyone depending on a specific record type-name in the JSON would break — none observed today.
- **Generic-parameter cascade through intermediate base classes.** Adding `TResult` forces every per-toolset base (`SubscriptionCommand`, `BaseStorageCommand`, `BaseSqlCommand`, …) to either widen to `<TOptions, TResult>` or pin `TResult` to a concrete type. This is the largest single mechanical change in the rollout. Mitigation: do the widening per-toolset in Phase 3 so each PR stays scoped to one base hierarchy; the change is a pure type-parameter add that the compiler verifies exhaustively.

---

## Open questions for the team

1. Do we want the inputSchema modernization and outputSchema core infra in **one PR** or **two**? The plan defaults to two for review-ability; combining them halves the test churn on `ToolInputSchemaTests`.
2. Should we add **golden-file snapshot tests** for `inputSchema` of a few stable commands to catch unintended drift across future `System.Text.Json` updates? Cheap to add, ongoing maintenance cost.
3. Do PMs want anything called out about `structuredContent` in user-facing release notes? It unlocks better client UX (validation, structured rendering) but is invisible if the client doesn't ask for it.
4. Is "one toolset per PR" still the right cadence for Phase 3 given how mechanical it is? Could reasonably batch 3–5 small toolsets per PR to cut review overhead.

---

## Appendix — what we considered and rejected

- **`McpServerTool.Create(Delegate)` end-to-end.** Would require rewriting all commands as attribute-decorated methods. Schema gap alone doesn't justify it; we'd also lose the `System.CommandLine` parsing layer.
- **Virtual `JsonTypeInfo? ResultTypeInfo` property on `IBaseCommand` instead of a `TResult` generic.** Cheaper to roll out (no cascade through intermediate base classes), but the invariant "declared result type matches what the command actually writes" lives only in a runtime discovery test instead of the type system. Drift becomes possible whenever a contributor adds a new command base class and forgets to extend the test, and the asymmetry with the existing `BaseCommand<TOptions>` generic is unprincipled. We pay the one-time cascade cost to get the permanent compile-time guarantee.
- **Hand-written `outputSchema` per command.** Would not scale to 150+ tools and duplicates information already encoded in the source-generated `*JsonContext`.
- **Describe the full `CommandResponse` envelope** (status / message / duration) in `outputSchema`. The MCP spec says `outputSchema` should describe `structuredContent`, which is the result payload only. Including envelope fields would mislead clients.
