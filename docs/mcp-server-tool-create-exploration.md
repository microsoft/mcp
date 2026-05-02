# Exploration: end-to-end migration to `McpServerTool.Create(Delegate)`

> **Status:** thought experiment / not currently planned
> **Audience:** engineers evaluating long-term direction of the command pipeline
> **Relationship to other docs:** [`mcp-tool-schemas-rfc.md`](mcp-tool-schemas-rfc.md) keeps the existing command model and only upgrades the schema-generation step inside it. This document explores what we'd do if we went the other way and let the `ModelContextProtocol` C# SDK own the tool surface end-to-end — input schema, output schema, dispatch, validation, and metadata. It's the alternative we mentioned and rejected in the RFC's appendix; this writeup makes the cost and shape concrete so a future contributor can re-evaluate without starting from zero.

---

## Premise

The `ModelContextProtocol` C# SDK already ships an opinionated tool model:

- A type-decorated container — `[McpServerToolType]` — groups related tools.
- A method-decorated handler — `[McpServerTool]` — declares each tool. Method parameters become tool inputs; the return type becomes the tool result.
- `McpServerTool.Create(Delegate)` (and the equivalent `WithTools<T>()` extension) introspects the `MethodInfo`, generates the `inputSchema` and `outputSchema` via `AIJsonUtilities` / `JsonSchemaExporter`, wires up dispatch, populates `CallToolResult.StructuredContent` from the return value, and integrates with `IServiceProvider` for parameter injection.

We already use `[McpServerTool]` *as a metadata attribute* on every command's `ExecuteAsync` (see [`KeyValueSetCommand`](../tools/Azure.Mcp.Tools.AppConfig/src/Commands/KeyValue/KeyValueSetCommand.cs)) for `Destructive` / `ReadOnly` / `Title`. We do not, however, use it as a *registration mechanism* — the SDK never sees those methods. Our `CommandFactoryToolLoader` discovers commands, generates schemas itself, and dispatches by walking `System.CommandLine` parse results.

The exploration is: what if we let the SDK do all of that?

---

## What we'd get for free

| Concern | Today | With `McpServerTool.Create` |
| --- | --- | --- |
| `inputSchema` generation | Hand-rolled `TypeToJsonTypeMapper` + DTOs | SDK calls `JsonSchemaExporter` on `MethodInfo` parameters |
| `outputSchema` generation | (proposed in RFC) explicit `JsonTypeInfo<TResult>` slot + `JsonSchemaExporter` | SDK derives from method's return type |
| `StructuredContent` population | (proposed in RFC) loader serializes the typed payload | SDK serializes the return value automatically |
| Tool name / title / description | Implemented via per-command properties | Method/attribute metadata + XML doc comments |
| Parameter binding | `BindOptions(ParseResult)` per command | DI-aware reflection over method parameters |
| Tool dispatch | `CommandFactoryToolLoader.CallToolHandler` | `McpServer` built-in routing |
| Tool ID stability | Manual `[McpServerTool(Title = "...")]` discipline | Attribute-driven; same discipline, less code |
| AOT compatibility | We maintain it | SDK maintains it (same `JsonSchemaExporter` foundation) |

The schema gap from the RFC's findings table closes for the same reason it would close in the property-based proposal — both paths terminate at `JsonSchemaExporter`. The difference is *how much of our infrastructure goes with it.*

---

## What a command looks like

### Today

```csharp
public sealed class KeyValueSetCommand(ILogger<KeyValueSetCommand> logger, IAppConfigService appConfigService)
    : SubscriptionCommand<KeyValueSetOptions>
{
    public override string Name => "set";
    public override string Description => "Set a key-value pair in App Configuration.";
    public override string Title => "Set App Configuration Key-Value";
    public override ToolMetadata Metadata { get; } = new() { Destructive = true, /* ... */ };

    protected override void RegisterOptions(Command command)
    {
        base.RegisterOptions(command);
        command.Options.Add(AppConfigOptionDefinitions.Value);
        command.Options.Add(AppConfigOptionDefinitions.Tags);
    }

    protected override KeyValueSetOptions BindOptions(ParseResult parseResult)
    {
        var options = base.BindOptions(parseResult);
        options.Value = parseResult.GetValueOrDefault<string>(AppConfigOptionDefinitions.Value.Name);
        options.Tags = parseResult.GetValueOrDefault<string[]>(AppConfigOptionDefinitions.Tags.Name);
        return options;
    }

    [McpServerTool(Destructive = true, ReadOnly = false, Title = CommandTitle)]
    public override async Task<CommandResponse> ExecuteAsync(
        CommandContext context, ParseResult parseResult, CancellationToken cancellationToken)
    {
        if (!Validate(parseResult.CommandResult, context.Response).IsValid)
            return context.Response;

        var options = BindOptions(parseResult);

        try
        {
            await _appConfigService.SetKeyValue(
                options.Account!, options.Key!, options.Value!,
                options.Subscription!, options.Tenant, options.RetryPolicy,
                options.Label, options.ContentType, options.Tags, cancellationToken);

            context.Response.Results = ResponseResult.Create(
                new(options.Key, options.Value, options.Label, options.ContentType, options.Tags),
                AppConfigJsonContext.Default.KeyValueSetCommandResult);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An exception occurred setting value. Key: {Key}.", options.Key);
            HandleException(context, ex);
        }

        return context.Response;
    }
}
```

### After

```csharp
[McpServerToolType]
public sealed class AppConfigKeyValueTools(ILogger<AppConfigKeyValueTools> logger, IAppConfigService appConfigService)
{
    [McpServerTool(
        Name = "azmcp-appconfig-keyvalue-set",
        Title = "Set App Configuration Key-Value",
        Destructive = true,
        ReadOnly = false)]
    [Description("Set a key-value pair in App Configuration.")]
    public async Task<KeyValueSetResult> SetAsync(
        [Description("The Azure subscription (ID or name).")] string subscription,
        [Description("The App Configuration store account name.")] string account,
        [Description("The key to set.")] string key,
        [Description("The value to assign.")] string value,
        [Description("Optional label.")] string? label = null,
        [Description("Content-type for the value.")] string? contentType = null,
        [Description("Tags to associate with the key-value.")] string[]? tags = null,
        [Description("Tenant ID or name.")] string? tenant = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            await appConfigService.SetKeyValue(
                account, key, value, subscription, tenant, retryPolicy: null,
                label, contentType, tags, cancellationToken);

            return new KeyValueSetResult(key, value, label, contentType, tags);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An exception occurred setting value. Key: {Key}.", key);
            throw;  // SDK turns exceptions into IsError=true CallToolResults
        }
    }

    public sealed record KeyValueSetResult(
        string? Key, string? Value, string? Label, string? ContentType, string[]? Tags);
}
```

Roughly half the lines disappear. `RegisterOptions`, `BindOptions`, `Validate`, the option-definition statics, the per-command `Name`/`Description`/`Title`/`Metadata` overrides, the `CommandResponse`/`ResponseResult` plumbing, and the `JsonContext` lookup are all gone.

---

## What we'd delete

If we went all-in, the following infrastructure becomes unnecessary:

- `IBaseCommand`, `BaseCommand<TOptions>`, and every per-toolset base (`SubscriptionCommand<TOptions>`, `BaseStorageCommand<TOptions>`, `BaseSqlCommand<TOptions>`, …).
- `CommandContext`, `CommandResponse`, `ResponseResult`, `ExceptionResult`. The SDK's `CallToolResult` becomes the only response type.
- `CommandFactory`, `CommandFactoryToolLoader`, `NamespaceToolLoader`'s command-discovery half. `ServerToolLoader`, `RegistryToolLoader`, and `SingleProxyToolLoader` may shrink to just the proxy concerns.
- `ToolInputSchema`, `ToolPropertySchema`, `TypeToJsonTypeMapper` (already deleted by the RFC's Part 1).
- `OptionDefinitions` static classes per toolset, and the `.AsRequired()` / `.AsOptional()` extension methods. Per-method parameters with `[Description]` and nullability replace them.
- The bulk of every `*Setup.cs` file. Tool registration becomes `services.AddMcpServer().WithTools<AppConfigKeyValueTools>()` — one line per tool-type per toolset.

Estimated deletion footprint: most of `core/Microsoft.Mcp.Core/src/Commands` and `core/Microsoft.Mcp.Core/src/Areas/Server/Commands`, plus the `Options/` and `Commands/` shape from every toolset.

---

## What we'd lose, and what we'd have to replace

This is where the cost lives.

### 1. The `System.CommandLine` parsing layer

Today, every tool is *also* a CLI verb under `azmcp <area> <resource> <op> --flag …`. That's not just dev convenience — it's how:

- We expose the same surface to humans and to LLMs (single source of truth for arg names, types, descriptions).
- We support `azmcp tools list` / `azmcp tools call` parity with the MCP wire surface.
- Test prompts in [`e2eTestPrompts.md`](../servers/Azure.Mcp.Server/docs/e2eTestPrompts.md) exercise tools via CLI invocation paths that mirror the MCP path.
- Live tests build on `RecordedCommandTestsBase` which drives commands via `ParseResult`.

Going all-in on `McpServerTool.Create` does not delete the CLI need; it splits it. We would have to either:
- **(a) Generate `System.CommandLine` verbs from the same attributed methods** — i.e., write a small adapter that reflects over `[McpServerToolType]` classes and produces a `RootCommand` whose handlers call into the same instances. Doable; ~one source generator or runtime adapter.
- **(b) Drop the CLI surface** for command-style invocation and keep only the MCP server entry point. This breaks every existing test path and every documented CLI prompt. Almost certainly unacceptable.

Option (a) recovers most of what we lose, but it means we end up writing a *second* tool-discovery system (this one for the CLI) on top of the SDK's. The simplification we got by deleting `CommandFactory` partly comes back as a CLI adapter.

### 2. The unified response envelope

`CommandResponse` carries `Status`, `Message`, `Duration`, plus the typed `Results`. The SDK's `CallToolResult` carries `Content[]`, `StructuredContent`, and `IsError`. They're not interchangeable:

- **Duration / telemetry hooks.** Today every command's response includes execution duration; our telemetry layer reads it. With the SDK path, we'd have to attach this via middleware (`McpServerHandler`) or accept losing it as a first-class field.
- **Status semantics.** The SDK's `IsError` is binary; our `Status` is HTTP-style (200/400/404/500/…). The error-message differentiation in [`BaseCommand.HandleException`](../core/Microsoft.Mcp.Core/src/Commands/BaseCommand.cs) (e.g., 401 for `AuthenticationFailedException`, 404 for missing resources) feeds clients useful triage info today. Squashing to `IsError=true` loses that signal unless we encode it inside the structured content.
- **Centralized error formatting.** `HandleException` is one place we control the shape of every error response. With per-method `try/throw`, we lose that single point — either every method re-implements the mapping, or we add a global `IMcpExceptionHandler` middleware to recover it.

### 3. The validation layer

`BaseCommand.Validate(parseResult.CommandResult, context.Response)` currently performs cross-option validation (e.g., "if X is set, Y is required") before binding. The SDK validates against `inputSchema` only — schema constraints, not arbitrary C# logic. Cross-option rules would have to be re-expressed either as:
- C# guards at the top of each method (manual, error-prone, no schema visibility), or
- A custom `IMcpServerToolFilter` middleware that runs before dispatch.

### 4. Per-command behaviour overrides

Commands today override:
- `ExecuteAsync` — replaced by the SDK invoking the method.
- `BindOptions` — replaced by SDK parameter binding.
- `Validate` — see above.
- `RegisterOptions` — replaced by method parameters.
- `Title` / `Description` / `Name` / `Metadata` — replaced by attributes.

But several commands also override less obvious things:
- **Composite tool grouping** (`NamespaceToolLoader`'s `--mode namespace` and `--mode single` proxy modes). The SDK has no built-in equivalent for collapsing 100+ tools into a single namespace-routed tool. Today this is handled by `NamespaceToolLoader` and `SingleProxyToolLoader` consuming our `IBaseCommand` abstraction. With SDK-native tools, we'd need to write a similar proxy that wraps the SDK's tool collection — same code, different abstraction underneath.
- **Subscription / tenant resolution** plumbed through `BaseAzureService`. This is independent of the command model and would carry over unchanged.

### 5. AOT and source-gen JsonContexts

The SDK uses `AIJsonUtilities` which falls back to runtime reflection for types not in a registered `JsonSerializerContext`. Our toolsets all use source-gen contexts (`StorageJsonContext`, `ComputeJsonContext`, …) and pass them explicitly. The SDK supports source-gen contexts via `McpServerOptions.JsonSerializerOptions`, but we'd have to register every toolset's context globally on the server, and verify there are no unintended fallbacks to reflection for types the SDK touches but we forgot to register. AOT testing (`Build-Local.ps1 -BuildNative`) becomes more important, not less, during the migration.

### 6. RBAC / OBO / transport-aware behaviour

Our [authentication architecture](Authentication.md) assumes commands are stateless and authenticate via `IAzureTokenCredentialProvider`. That's pure DI and ports cleanly to SDK methods. But anything that *reads* `CommandContext` for transport hints would have to switch to `HttpContext` accessors — which is also exactly what the RFC tells us not to do. Going all-in here forces us to re-litigate that decision; the SDK methods receive an `RequestContext<CallToolRequestParams>` and we'd have to encode our transport-agnostic discipline as code review, not as a type signature.

---

## Migration shape

If we ever decided to do this, it's a multi-quarter effort. A reasonable sequencing:

1. **Pilot toolset behind a flag.** Pick one small toolset (e.g., `AzureTerraformBestPractices`, ~3 tools, no Azure resource dependencies). Stand up a parallel `[McpServerToolType]` class. Register both the legacy command and the SDK-native tool; gate which one wins via a startup flag. Validate parity on every public surface (tool list, schemas, dispatch, error shape).
2. **CLI adapter.** Build the runtime adapter or source generator that produces `System.CommandLine` verbs from `[McpServerToolType]` classes. Validate that `azmcp <area> <op>` invocations still work for the pilot toolset. This is the long-pole work item — without it, every test infrastructure breaks.
3. **Response envelope decisions.** Pin down how `Status`, `Duration`, and centralized error formatting carry over (middleware vs. per-method vs. accept the loss). Update telemetry to read the new shape.
4. **Validation middleware.** Build the cross-option validation hook so commands with multi-field invariants don't have to re-implement them inline.
5. **Toolset-by-toolset migration.** ~25 PRs, one per toolset. Each PR rewrites every command in the toolset, deletes the toolset's `Commands/` and `Options/` folders, and updates tests. Recorded tests (the playback ones, see [`recorded-tests.md`](recorded-tests.md)) need to be re-validated against the new dispatch path; some may need to be re-recorded.
6. **Delete the legacy infrastructure.** Once every toolset is migrated and the flag is off in all environments, remove `BaseCommand`, `CommandFactory`, the loader hierarchy, and the option-definition statics.

Total deletion footprint outweighs total addition footprint; the codebase shrinks. Every PR is mechanical for the toolset itself, but the cross-cutting work in steps 2–4 is real engineering.

---

## When this would be the right call

The trade-off only becomes attractive if **multiple** of the following are true:

- We commit to deprecating the `azmcp <verb>` CLI surface. (We won't; it's documented and used.)
- The SDK's tool model gains first-class support for our missing pieces (namespace proxy, transport-agnostic context, hierarchical error codes).
- We're already rewriting the response envelope for unrelated reasons (e.g., introducing streaming responses, partial results, or per-tool middleware).
- A future MCP spec revision requires features the SDK exposes but our hand-rolled loader doesn't (e.g., tool-set negotiation, capability flags, structured progress).

Absent those, the RFC's path — keep the command model, add the schemas — buys 95% of the protocol benefit at <10% of the code churn.

---

## Open questions if we ever revisit

1. **Source generator vs. runtime adapter for CLI verbs?** A source generator gives compile-time safety and AOT-friendliness; a runtime adapter is simpler to author and debug. Both work.
2. **Could we keep `BaseCommand` as a thin wrapper that *adapts* to `[McpServerTool]` registration?** Possibly — it would let us migrate incrementally without splitting the class hierarchy. The risk is ending up with both abstractions in the codebase forever.
3. **Does the SDK's `JsonSchemaExporter` integration handle our naming conventions?** We use `NameNormalization.NormalizeOptionName` for kebab/snake/camel translations. The SDK's parameter-name → schema-property mapping is the C# parameter name verbatim; we'd need to verify and possibly customize via `[JsonPropertyName]`.
4. **Error-shape compatibility.** Existing MCP clients consuming `azmcp` may have parsed error responses that include our `Status` / `Message` envelope. Switching to `IsError=true` plus content blocks is a wire-format change. Mitigation: bake the equivalent fields into the structured content as a transitional shim.

---

## Bottom line

The end-to-end SDK-native model is a more *idiomatic* design for an MCP server in 2026 than what we have today. It's also a much larger commitment than fixing the schema layer, and it pulls in concerns (CLI surface, response envelope, error shape, validation) that the schema work has the luxury of leaving alone. The RFC's recommendation — keep the command model, upgrade the schema-generation step inside it — is the correct call given our current constraints. This document exists so the next contributor who looks at the SDK and asks "wait, why didn't we just…?" has a complete answer.
