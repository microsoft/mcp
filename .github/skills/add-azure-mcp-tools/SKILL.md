---
name: add-azure-mcp-tools
description: 'Add a new tool/command to any Azure MCP toolset. Full lifecycle from scaffolding through PR submission. USE WHEN: add new command, create tool, new MCP tool, scaffold command, implement operation, add azure service tool, create new toolset.'
argument-hint: 'Describe the new tool (e.g., "add storage container delete command" or "create new KeyVault toolset with secret get command")'
---

# Add a New Tool to Azure MCP

## Purpose

Step-by-step workflow for adding a new command to any Azure MCP toolset. Each phase has an explicit **gate** — do not proceed until the gate passes.

For deep-dive patterns, threat details, and troubleshooting, see the [new-command guide](https://github.com/microsoft/mcp/blob/main/servers/Azure.Mcp.Server/docs/new-command.md).

> **Terminology note:** In this repo, _tool_ refers to the MCP-exposed capability and _command_ refers to the underlying C# command class implementation. Both terms appear in the codebase and docs.

## Workflow & Scope Decisions

1. **New toolset vs. existing:**
   - **Adding to an existing toolset?** → Skip to [Phase 1](https://github.com/microsoft/mcp/blob/main/.github/skills/add-azure-mcp-tools/SKILL.md#phase-1-implement-command).
   - **Creating a new toolset?** → Complete [Phase 0](https://github.com/microsoft/mcp/blob/main/.github/skills/add-azure-mcp-tools/SKILL.md#phase-0-new-toolset-setup-skip-if-adding-to-existing-toolset) first.
2. **Azure service vs. non-Azure command:**

| | Azure Service Commands | Non-Azure Commands |
|---|---|---|
| **Examples** | ACR Registry List, SQL Database List, Storage Account Get | CLI wrappers, Best Practices, Documentation tools |
| **test-resources.bicep** | ✅ Required | ❌ Skip |
| **test-resources-post.ps1** | ✅ Required (even if basic) | ❌ Skip |
| **RBAC role assignments** | ✅ Required | ❌ Skip |
| **Live tests** | ✅ Required (recorded) | ❌ Skip |
| **Unit tests** | ✅ Required | ✅ Required |

3. **Follow phases in order:** [Phase 0](https://github.com/microsoft/mcp/blob/main/.github/skills/add-azure-mcp-tools/SKILL.md#phase-0-new-toolset-setup-skip-if-adding-to-existing-toolset) → [Phase 1](https://github.com/microsoft/mcp/blob/main/.github/skills/add-azure-mcp-tools/SKILL.md#phase-1-implement-command) → [Phase 2](https://github.com/microsoft/mcp/blob/main/.github/skills/add-azure-mcp-tools/SKILL.md#phase-2-unit-tests) → [Phase 3](https://github.com/microsoft/mcp/blob/main/.github/skills/add-azure-mcp-tools/SKILL.md#phase-3-live-tests-azure-service-commands-only) → [Phase 4](https://github.com/microsoft/mcp/blob/main/.github/skills/add-azure-mcp-tools/SKILL.md#phase-4-documentation-and-registration) → [Phase 5](https://github.com/microsoft/mcp/blob/main/.github/skills/add-azure-mcp-tools/SKILL.md#phase-5-tool-description-evaluation) → [Phase 6](https://github.com/microsoft/mcp/blob/main/.github/skills/add-azure-mcp-tools/SKILL.md#phase-6-final-validation) → [Phase 7](https://github.com/microsoft/mcp/blob/main/.github/skills/add-azure-mcp-tools/SKILL.md#phase-7-pr-readiness-checklist).

## Security Invariants (Apply at Every Phase)

**All tool inputs are untrusted.** Each security requirement is applied inline in the phases below; keep these invariants in mind throughout:

- **Input validation:** Validate inputs in `ValidateOptions` against the target resource's exact [Azure naming rules](https://learn.microsoft.com/azure/azure-resource-manager/management/resource-name-rules) (deterministic checks first; never a generic blocklist). Always report errors via `validationResult.AddError(message, telemetrySafeMessage)` — never mutate `ValidationResult.Errors`.
- **Safe logging & errors:** Log only individually named, known-safe fields (`options.Subscription`, `options.ResourceGroup`, `Name`) — never `{@Options}` or secrets. Return actionable, non-revealing errors without tokens, stack traces, or raw backend bodies.
- **Endpoint & URI safety:** Never concatenate unvalidated user input into URLs, queries, or commands. Validate Azure data-plane endpoints via `IAzureService.ValidateAzureServiceEndpoint` (or injected `IEndpointValidator`), known external hosts via `EndpointValidator.ValidateExternalUrl`, and arbitrary public targets via `ValidatePublicTargetUrl`. Use `ResourceIdentifier`/ARM collections for control-plane IDs.
- **Fail closed & transport-agnostic:** Require explicit context when tenant, subscription, or resource is ambiguous. Keep commands stateless, propagate `CancellationToken`, and never branch on transport or access `HttpContext`.
- **Secure provisioning defaults:** Create and create-or-update commands must enforce private access, HTTPS/TLS ≥ 1.2, and Managed Identity/RBAC by default, preserving security on update and requiring explicit opt-in options for any weaker setting.
- **AI-assisted code generation:** When prompting an AI assistant to generate command, service, or test code, include the [AI security prompt requirements](https://github.com/microsoft/mcp/blob/main/servers/Azure.Mcp.Server/docs/new-command.md#using-ai-to-generate-tool-code) and review every generated snippet against them and the [threat mitigations table](https://github.com/microsoft/mcp/blob/main/servers/Azure.Mcp.Server/docs/new-command.md#mcp-specific-threat-patterns).

---

## Phase 0: New Toolset Setup (skip if adding to existing toolset)

Create the toolset directory structure:

```
tools/Azure.Mcp.Tools.{Toolset}/
├── src/
│   ├── Azure.Mcp.Tools.{Toolset}.csproj
│   ├── {Toolset}Setup.cs
│   ├── Commands/
│   │   ├── {Resource}/
│   │   │   └── {Resource}{Operation}Command.cs
│   │   └── {Toolset}JsonContext.cs
│   ├── Options/
│   │   └── {Resource}/
│   │       └── {Resource}{Operation}Options.cs
│   ├── Services/
│   │   ├── I{Toolset}Service.cs
│   │   └── {Toolset}Service.cs
│   └── Models/
└── tests/
    ├── Azure.Mcp.Tools.{Toolset}.Tests/
    │   └── Azure.Mcp.Tools.{Toolset}.Tests.csproj
    ├── test-resources.bicep          (Azure service commands only)
    └── test-resources-post.ps1       (Azure service commands only)
```

Required setup steps:

1. If Azure SDK packages are needed, add `<PackageVersion>` to `Directory.Packages.props` **first**, then add `<PackageReference>` (without a version) to `Azure.Mcp.Tools.{Toolset}.csproj`.
2. Register the projects in solution files:
   ```powershell
   pwsh eng/scripts/Update-Solution.ps1 -All
   ```
3. Register the toolset in `servers/Azure.Mcp.Server/src/Program.cs` `RegisterAreas()` in **alphabetical order** (outside the `#if !BUILD_NATIVE` block — do not add new exclusions there).
4. Choose the command base class:
   - **Commands needing an Azure subscription** (most ARM/ARG tools) → inherit `SubscriptionCommand<TOptions, TResult>` and inject `ISubscriptionResolver`.
   - **Commands needing Azure authentication without `--subscription`** (e.g., data-plane tools using `--account` + `--tenant`) → inherit `AuthenticatedCommand<TOptions, TResult>`.
   - **Commands not needing Azure authentication** (CLI wrappers, documentation, best-practice advisors) → inherit `BaseCommand<TOptions, TResult>` directly.
   - Add an intermediate base command only when sharing real cross-command logic (validation/error handling).
5. Implement `{Toolset}Setup : IAreaSetup` and register both the service and command as singletons in `ConfigureServices`:
   ```csharp
   public void ConfigureServices(IServiceCollection services)
   {
       services.AddSingleton<I{Toolset}Service, {Toolset}Service>();
       services.AddSingleton<{Resource}{Operation}Command>();
   }
   ```
6. Configure `Azure.Mcp.Tools.{Toolset}.Tests.csproj` with `<IsTestProject>true</IsTestProject>`, `<OutputType>Exe</OutputType>`, `<HasUnitTests>true</HasUnitTests>`, `<HasLiveTests>true</HasLiveTests>` (for Azure service commands), and a `<ProjectReference>` to both the toolset `.csproj` and `$(RepoRoot)servers\Azure.Mcp.Server\src\Azure.Mcp.Server.csproj` (see [Test Project Configuration](https://github.com/microsoft/mcp/blob/main/servers/Azure.Mcp.Server/docs/new-command.md#test-project-configuration)).

**GATE:** `dotnet build servers/Azure.Mcp.Server/src` must pass.

---

## Phase 1: Implement Command

Create or update these files in order:

### 1a. Options Class

File: `src/Options/{Resource}/{Resource}{Operation}Options.cs`

```csharp
using Azure.Mcp.Core.Options;
using Microsoft.Mcp.Core.Options;

namespace Azure.Mcp.Tools.{Toolset}.Options.{Resource};

public class {Resource}{Operation}Options : ISubscriptionOption
{
    [Option(Description = "Description of what this option does (e.g., 'The name of the resource').")]
    public string? MyOption { get; set; }

    [Option(Description = OptionDescriptions.ResourceGroup)]
    public string? ResourceGroup { get; set; }

    [Option(Description = OptionDescriptions.Subscription)]
    public string? Subscription { get; set; }

    [Option(Description = OptionDescriptions.Tenant)]
    public string? Tenant { get; set; }
}
```

Rules:
- Keep options classes **flat POCOs** (no inheritance); implement `ISubscriptionOption` when subscription resolution is needed (`ISubscriptionOption` provides `Subscription` only — add `Tenant` separately if accepted).
- Decorate every bound property with `[Option(Description = "...")]` (`OptionBinder` auto-converts names to `--kebab-case`; specify `Name` only when default conversion is wrong) or `[OptionContainer<TContainer>(Prefix = "prefix")]` for nested objects. Unattributed properties are ignored; do not create a static `{Toolset}OptionDefinitions` class.
- Use `OptionDescriptions.X` for shared descriptions (`Subscription`, `Tenant`, `ResourceGroup`, `AuthMethod`, `Scope`), or a toolset `{Toolset}OptionDescriptions` constants class for options reused across commands.
- Use `required` for required options and nullable types (`?`) for optional options; non-nullable value types (e.g., `int`) default to `0` without `required`. Note that optional string options skip automatic blank-string rejection — reject blank values in `ValidateOptions` when the contract requires it.
- Naming & order: use `subscription` (never `subscriptionId`), `resourceGroup` (never `resourceGroupName`), singular nouns (`server`, `--account`, not `-name`), aligned with Azure SDK parameters and sibling commands. Order command-specific options first, then `ResourceGroup`, `Subscription`, `Tenant`, `AuthMethod`.
- **Cursor pagination (when exposing paged results):** Follow the [cursor pagination guide](https://github.com/microsoft/mcp/blob/main/docs/tool-pagination.md). Implement `ICursorOption` for `--cursor`, bind fixed arguments with `PaginationCursor`, return `nextCursor` when another page exists, document fixed vs. overridable arguments, and test continuation and completion.

See [Options and Validation](https://github.com/microsoft/mcp/blob/main/servers/Azure.Mcp.Server/docs/new-command.md#options-and-validation) for mutually exclusive, enum, and interface-constrained patterns.

### 1b. Service Interface and Implementation

Files: `src/Services/I{Toolset}Service.cs` and `src/Services/{Toolset}Service.cs`

1. **Choose return types and base class:**
   - **Azure Resource Graph (ARG) queries** → inherit `BaseAzureResourceService` and return `Task<ResourceQueryResults<MyModel>>` (includes `AreResultsTruncated`).
   - **All other operations (direct ARM writes, data plane)** → inherit `BaseAzureService` and return `Task<List<MyDetail>>` or `Task<MyResultModel>`.
   - (`BaseAzureResourceService` extends `BaseAzureService` — choose it when you need ARG queries.)

```csharp
public interface I{Toolset}Service
{
    Task<ResourceQueryResults<MyModel>> GetResourcesAsync(
        string? myOption,
        string subscription,
        string? resourceGroup = null,
        string? tenant = null,
        CancellationToken cancellationToken = default);
}
```

```csharp
public class {Toolset}Service(IAzureService azureService)
    : BaseAzureResourceService(azureService), I{Toolset}Service
{
    public async Task<ResourceQueryResults<MyModel>> GetResourcesAsync(
        string? myOption,
        string subscription,
        string? resourceGroup = null,
        string? tenant = null,
        CancellationToken cancellationToken = default)
    {
        return await ExecuteResourceQueryAsync(
            "Microsoft.{Provider}/{resourceType}",
            resourceGroup,
            subscription,
            ConvertToModel,
            tenant: tenant,
            cancellationToken: cancellationToken);
    }

    private static MyModel ConvertToModel(JsonElement item)
    {
        var data = MyModelData.FromJson(item);
        return new MyModel(
            Name: data.ResourceName,
            Id: data.ResourceId,
            Location: data.Location.ToString(),
            Tags: data.Tags as IReadOnlyDictionary<string, string>);
    }
}
```

2. **Signature, `CancellationToken`, and SDK rules:**
   - Align parameters one per line with blank lines between methods; always place `CancellationToken` last (use `= default` only when other parameters have defaults, and pass it explicitly at call sites).
   - Propagate `CancellationToken` to all async calls and use `.WithCancellation(cancellationToken)` on `await foreach` — never pass `CancellationToken.None` or `default`.
   - Use `AzureService.GetSubscription(subscription, tenant, cancellationToken: cancellationToken)` and `await subscriptionResource.GetResourceGroupAsync(resourceGroup, cancellationToken)`; access ARM resources through collections (e.g., `.GetSqlServers().GetAsync(name, cancellationToken: cancellationToken)`).
   - Verify actual Azure SDK property names via IntelliSense or assembly inspection before coding, cast `data.Tags as IReadOnlyDictionary<string, string>`, and use `Location.ToString()` (see [SDK Property Name Discovery](https://github.com/microsoft/mcp/blob/main/servers/Azure.Mcp.Server/docs/new-command.md#sdk-property-name-discovery)).
3. **Sovereign clouds & endpoint validation:**
   - ARM and ARG operations handle sovereign clouds automatically.
   - Data-plane SDKs requiring an explicit URL must switch on `AzureService.CloudConfiguration.CloudType` (fail closed on unsupported clouds; never hardcode `*.windows.net`, `*.azure.com`, etc.) and call `AzureService.ValidateAzureServiceEndpoint(endpoint, serviceType)` before credential acquisition or client creation. See [Sovereign Cloud Support](https://github.com/microsoft/mcp/blob/main/servers/Azure.Mcp.Server/docs/new-command.md#sovereign-cloud-support).
   - Without `IAzureService`, inject `IEndpointValidator` (register with `services.AddEndpointValidation()`). Use `EndpointValidator.ValidateExternalUrl(url, allowedHosts)` for known external hosts and `ValidatePublicTargetUrl(url)` for arbitrary public targets. Never construct a production validator or pass a namespace manually.
   - Control-plane ARM calls use `ResourceIdentifier` or ARM collection helpers — never string-interpolate IDs.
4. **Long-running operations (when applicable):**
   - Start with `WaitUntil.Started` and await `BaseAzureService.WaitForLroCompletionAsync(lroOperation, cancellationToken)` so playback tests skip polling delays.
5. **Secure resource provisioning defaults (for create / create-or-update):**
   - Explicitly set private network access, HTTPS-only + TLS ≥ 1.2, and Managed Identity/RBAC in the typed SDK request model (do not rely on service defaults).
   - On create-or-update, preserve existing security settings when options are omitted. Expose weaker settings only via explicit, narrowly named opt-in options (e.g., `--allow-public-network-access`, `--enable-local-auth`) with documented security consequences. See [Secure Resource Provisioning Defaults](https://github.com/microsoft/mcp/blob/main/servers/Azure.Mcp.Server/docs/new-command.md#secure-resource-provisioning-defaults).
6. **HttpClient usage:**
   - Obtain HTTP clients via `IHttpClientFactory.CreateClient()` (or `AzureService.GetClient()`) so test-proxy recording and SSRF policies apply.

### 1c. Command Class

File: `src/Commands/{Resource}/{Resource}{Operation}Command.cs`

```csharp
using System.Text.Json.Serialization.Metadata;
using Azure.Mcp.Core.Commands.Subscription;
using Azure.Mcp.Core.Services.Azure.Subscription;
using Azure.Mcp.Tools.{Toolset}.Models;
using Azure.Mcp.Tools.{Toolset}.Options.{Resource};
using Azure.Mcp.Tools.{Toolset}.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Mcp.Core.Commands;
using Microsoft.Mcp.Core.Models.Command;

namespace Azure.Mcp.Tools.{Toolset}.Commands.{Resource};

[CommandMetadata(
    Id = "<generate-new-guid>",
    Name = "operation",
    Title = "Human Readable Title",
    Description = """
        What this command does. Include required options and return format.
        """,
    OperationPlane = ToolOperationPlane.Control,
    Destructive = false,
    Idempotent = true,
    OpenWorld = false,
    ReadOnly = true,
    Secret = false,
    LocalRequired = false)]
public sealed class {Resource}{Operation}Command(
    ILogger<{Resource}{Operation}Command> logger,
    I{Toolset}Service service,
    ISubscriptionResolver subscriptionResolver)
    : SubscriptionCommand<{Resource}{Operation}Options, {Resource}{Operation}Command.{Resource}{Operation}CommandResult>(subscriptionResolver)
{
    private readonly ILogger<{Resource}{Operation}Command> _logger = logger;
    private readonly I{Toolset}Service _service = service;

    public override JsonTypeInfo<{Resource}{Operation}CommandResult>? ResultTypeInfo =>
        {Toolset}JsonContext.Default.{Resource}{Operation}CommandResult;

    public override async Task<CommandResponse> ExecuteAsync(
        CommandContext context, {Resource}{Operation}Options options, CancellationToken cancellationToken)
    {
        try
        {
            var results = await _service.GetResourcesAsync(
                options.MyOption,
                options.Subscription!,
                options.ResourceGroup,
                options.Tenant,
                cancellationToken);

            SetResult(context,
                new {Resource}{Operation}CommandResult(results.Results ?? [], results.AreResultsTruncated));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in {Operation}. Subscription: {Subscription}",
                Name, options.Subscription);
            HandleException(context, ex);
        }

        return context.Response;
    }

    public record {Resource}{Operation}CommandResult(List<MyModel> Items, bool AreResultsTruncated);
}
```

Rules:
- **Two-generic pattern:** Make the class `sealed` with a primary constructor; inherit `SubscriptionCommand<TOptions, TResult>`, `AuthenticatedCommand<TOptions, TResult>` (Azure auth without `--subscription`), or `BaseCommand<TOptions, TResult>` (no Azure auth). `ExecuteAsync` takes pre-bound `TOptions` — never `ParseResult`, `RegisterOptions()`, `BindOptions()`, or manual `Validate()`.
- **Metadata (`[CommandMetadata]`):** Generate a fresh GUID `Id`, classify `OperationPlane` (`Control`, `Data`, `Both`, or `NotApplicable` — ARM setup lookups do not make a data-plane tool `Both`), and declare all behavioral properties even when using defaults. Cross-check against [CommandMetadata Properties and Validation](https://github.com/microsoft/mcp/blob/main/servers/Azure.Mcp.Server/docs/new-command.md#commandmetadata-properties-and-validation) (set `Secret = true` whenever keys, tokens, secrets, or credential metadata may be returned; `OpenWorld = false` for Azure APIs).
- **Structured output:** Declare the result record `public` inside the command, override `ResultTypeInfo`, and call `SetResult(context, ...)` for success (see [`docs/output-schema-migration.md`](https://github.com/microsoft/mcp/blob/main/docs/output-schema-migration.md)). Return `[]` for null/empty array payloads.
- **Stateless & transport-agnostic:** Never store per-request state in fields, inspect `HttpContext`, or branch on transport (`ASPNETCORE_URLS`). Pass `options.Tenant` through to services.
- **Logging & error handling:** Never log `{@Options}` or secret values; log only known-safe fields individually. Always call `HandleException(context, ex)` in `catch`. `BaseCommand` and `AuthenticatedCommand` already map standard HTTP/ARM/MSAL/OBO/timeout errors — override `GetErrorMessage`/`GetStatusCode` only for service-specific cases, and never strip `OnBehalfOfUserTokenRequiredException` from 403 or leak raw tokens (see [Error Handling](https://github.com/microsoft/mcp/blob/main/servers/Azure.Mcp.Server/docs/new-command.md#error-handling)).
- **Custom validation & telemetry safety:** Override `ValidateOptions` for resource-specific naming rules (length, character/category checks), mutual exclusivity, and allowed values — prefer deterministic checks first. Always add errors with `ValidationResult.AddError(message, telemetrySafeMessage)`:
  - Static caller text without tool parameter values can be passed for both arguments.
  - If the caller message includes any tool parameter value (even non-secret ones), pass a separate static `telemetrySafeMessage` without parameter values, identifiers, secrets, or exception text.
  - **Never** mutate `ValidationResult.Errors` directly — any untracked error makes `TelemetrySafeMessage` fall back to `"Invalid options."` for the entire result. See [Telemetry-Safe Validation Errors](https://github.com/microsoft/mcp/blob/main/servers/Azure.Mcp.Server/docs/new-command.md#telemetry-safe-validation-errors).

```csharp
public override void ValidateOptions({Resource}{Operation}Options options, ValidationResult validationResult)
{
    base.ValidateOptions(options, validationResult);

    if (options.Account is not null && !IsValidStorageAccountName(options.Account))
    {
        const string message = "--account must be 3–24 lowercase alphanumeric characters (storage account naming rule).";
        validationResult.AddError(message, message);
    }
}
```

- **Intermediate base commands:** Add `Base{Toolset}Command<TOptions, TResult>` with interface constraints (e.g., `where TOptions : class, ISubscriptionOption, I{Toolset}Option`) only for real shared validation/behavior — never merely for option binding. See [Intermediate Base Commands](https://github.com/microsoft/mcp/blob/main/servers/Azure.Mcp.Server/docs/new-command.md#intermediate-base-commands).

### 1d. JSON Serialization Context

File: `src/Commands/{Toolset}JsonContext.cs`

```csharp
[JsonSerializable(typeof({Resource}{Operation}Command.{Resource}{Operation}CommandResult))]
[JsonSerializable(typeof(MyModel))]
[JsonSourceGenerationOptions(
    PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase,
    DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull)]
internal partial class {Toolset}JsonContext : JsonSerializerContext;
```

Rules:
- One context per toolset; filename must match class name.
- Include only types serialized as top-level result payloads, with `[JsonSerializable]` attributes sorted alphabetically by model name.
- Use `{Toolset}JsonContext.Default.{CommandResult}` via `ResultTypeInfo` and `SetResult` (never context-less `JsonSerializer.Deserialize<T>()`).

### 1e. Register Command

File: `src/{Toolset}Setup.cs`

```csharp
public class {Toolset}Setup : IAreaSetup
{
    public string Name => "{toolset}";

    public string Title => "Manage Azure {Toolset}";

    public void ConfigureServices(IServiceCollection services)
    {
        services.AddSingleton<I{Toolset}Service, {Toolset}Service>();
        services.AddSingleton<{Resource}{Operation}Command>();
    }

    public CommandGroup RegisterCommands(IServiceProvider serviceProvider)
    {
        var root = new CommandGroup(Name, "{Toolset} operations description.", Title);

        var resource = new CommandGroup("{resource}", "{Resource} operations description");
        root.AddSubGroup(resource);
        resource.AddCommand<{Resource}{Operation}Command>(serviceProvider);

        return root;
    }
}
```

Rules:
- Command group names must be concatenated lowercase or dash-separated (`"resourcegroup"`, `"entra-admin"`) — **never underscores**.
- Keep resource hierarchy separate from operations (`azmcp postgres server param set`, not `setparam`).
- If this is a new toolset, ensure it is registered alphabetically in `servers/Azure.Mcp.Server/src/Program.cs` `RegisterAreas()`.

**GATE:** `dotnet build tools/Azure.Mcp.Tools.{Toolset}/src` must pass with 0 errors and 0 warnings.

---

## Phase 2: Unit Tests

File: `tests/Azure.Mcp.Tools.{Toolset}.Tests/{Resource}/{Resource}{Operation}CommandTests.cs`

- **Base class:** Inherit `SubscriptionCommandUnitTestsBase<TCommand, TService>` for subscription commands, or `CommandUnitTestsBase<TCommand, TService>` for non-subscription commands (the wrong base breaks `ISubscriptionResolver` DI).
- **Invoke via CLI args:** Prefer `ExecuteCommandAsync("--my-option", "val", ...)` so option registration, `OptionBinder`, `ValidateOptions`, and `ISubscriptionResolver` are all exercised.
- **Mocks & tokens:** Use `Arg.Any<CancellationToken>()` in NSubstitute setups, `TestContext.Current.CancellationToken` when calling real code, and `Arg.Is(value)` or direct values (never `Arg.Is<T>(x => x == value)`). Never use `CancellationToken.None` or `default`.
- **Deserialization:** Always use `ValidateAndDeserializeResponse(response, {Toolset}JsonContext.Default.{Resource}{Operation}CommandResult)` — never custom test models.
- **Required test cases** (see [Unit Testing Patterns](https://github.com/microsoft/mcp/blob/main/servers/Azure.Mcp.Server/docs/new-command.md#unit-testing-patterns) for the full test template):
  1. Constructor & metadata (`Name`, `Description`, relevant `Metadata` flags).
  2. Valid/invalid input binding, missing required options, and negative security cases (malformed names, invalid hosts/queries).
  3. Custom `ValidateOptions` assertions checking **both** caller `Errors` and `TelemetrySafeMessage` (verify supplied values do not leak into telemetry).
  4. For create / create-or-update commands: no-option secure defaults, each downgrade opt-in option, and create-or-update omission preserving existing security settings.
  5. Deserialization of result payloads.
  6. Service errors (`InternalServerError`) and `RequestFailedException` (`NotFound`, `Forbidden`, etc.).

> Note: This repository uses **Microsoft.Testing.Platform** (`global.json`). Run targeted tests with `-- --filter-class`, not MSBuild `--filter`.

**GATE:** `dotnet test tools/Azure.Mcp.Tools.{Toolset}/tests/Azure.Mcp.Tools.{Toolset}.Tests -- --filter-class "*{Resource}{Operation}CommandTests"` must pass.

---

## Phase 3: Live Tests (Azure service commands only)

Skip this phase for non-Azure commands (CLI wrappers, best practices, documentation tools).

### 3a. Test Infrastructure

1. Create `tools/Azure.Mcp.Tools.{Toolset}/tests/test-resources.bicep` with minimal SKUs, `baseName`, `testApplicationOid`, and RBAC role assignments (see [Test Infrastructure Templates](https://github.com/microsoft/mcp/blob/main/servers/Azure.Mcp.Server/docs/new-command.md#test-infrastructure-templates)).
2. Create `tools/Azure.Mcp.Tools.{Toolset}/tests/test-resources-post.ps1` (required even if template-only).
3. Validate template syntax:
   ```powershell
   az bicep build --file tools/Azure.Mcp.Tools.{Toolset}/tests/test-resources.bicep
   ```

### 3b. Live Test Class

File: `tests/Azure.Mcp.Tools.{Toolset}.Tests/{Toolset}CommandTests.cs`

```csharp
public class {Toolset}CommandTests(ITestOutputHelper output, TestProxyFixture fixture, LiveServerFixture liveServerFixture)
    : RecordedCommandTestsBase(output, fixture, liveServerFixture)
{
    [Fact]
    public async Task {Resource}{Operation}_ReturnsExpectedResult()
    {
        if (await AssertLocalToolIsUnavailableInHttpMode("{toolset}_{resource}_{operation}"))
        {
            return;
        }

        var result = await CallToolAsync(
            "{toolset}_{resource}_{operation}",
            new()
            {
                ["subscription"] = Settings.SubscriptionId,
                ["resource-group"] = Settings.ResourceGroupName,
                ["tenant"] = Settings.TenantId,
            });

        var items = result.AssertProperty("items");
        Assert.Equal(JsonValueKind.Array, items.ValueKind);
    }
}
```

Key rules:
- **Inherit `RecordedCommandTestsBase`:** If overriding `InitializeAsync` or `DisposeAsync`, always `await base.DisposeAsync()` / `base.InitializeAsync()`.
- **`LocalRequired = true` tools:** Start every recorded test with `if (await AssertLocalToolIsUnavailableInHttpMode(toolName)) return;` (do not duplicate HTTP-mode exclusion logic).
- **Determinism & sanitizers:** Always pass `Settings.TenantId` when needed, wrap dynamic values (`Guid`, timestamps) in `RegisterOrRetrieveVariable`, and use sanitizer-safe assertions (`AssertProperty`, `TryGetProperty`) or `SanitizeAndRecord`. For create/update tools, verify the resulting secure resource state when practical.
- **HTTP recording:** Ensure service HTTP clients come from `IHttpClientFactory.CreateClient()`.

### 3c. Deploy, Record, Verify Playback, and Push

1. Create `assets.json` beside the live test `.csproj` if missing:
   ```json
   {
     "AssetsRepo": "Azure/azure-sdk-assets",
     "AssetsRepoPrefixPath": "",
     "TagPrefix": "Azure.Mcp.Tools.{Toolset}.Tests",
     "Tag": ""
   }
   ```
2. Deploy test resources:
   ```powershell
   ./eng/scripts/Deploy-TestResources.ps1 -Paths "{Toolset}"
   ```
3. Set `"TestMode": "Record"` in `.testsettings.json` and run live tests:
   ```powershell
   dotnet test tools/Azure.Mcp.Tools.{Toolset}/tests/Azure.Mcp.Tools.{Toolset}.Tests -- --filter-trait "TestType=Live"
   ```
4. Locate and inspect recordings (`.proxy\Azure.Sdk.Tools.TestProxy config locate -a ...`) for secrets or unsanitized values.
5. Set `"TestMode": "Playback"` in `.testsettings.json` and re-run the command above to confirm playback passes.
6. Push recordings and commit the updated `assets.json`:
   ```powershell
   .proxy\Azure.Sdk.Tools.TestProxy push -a tools\Azure.Mcp.Tools.{Toolset}\tests\Azure.Mcp.Tools.{Toolset}.Tests\assets.json
   ```

See [`docs/recorded-tests.md`](https://github.com/microsoft/mcp/blob/main/docs/recorded-tests.md) and [Recorded Test Pitfalls](https://github.com/microsoft/mcp/blob/main/servers/Azure.Mcp.Server/docs/new-command.md#recorded-test-pitfalls) for `AzureCliCredential` fallback, 12-hour RG expiry, and sanitizer details.

**GATE:** Live tests pass in both `Record` and `Playback` modes.

---

## Phase 4: Documentation and Registration

Build `servers/Azure.Mcp.Server/src` first so `azmcp` is up to date, then complete all applicable documentation and registry updates before evaluating tool descriptions:

1. **Command Reference (`servers/Azure.Mcp.Server/docs/azmcp-commands.md`):**
   - Add the command in alphabetical order within its service section, documenting parameters (required vs. optional) and any special RBAC or pagination requirements.
   - Regenerate metadata badges from the built server (CI fails if skipped):
     ```powershell
     ./eng/scripts/Update-AzCommandsMetadata.ps1
     ```
2. **Test Prompts (`servers/Azure.Mcp.Server/docs/e2eTestPrompts.md`):**
   - Add 2–3 natural language prompts in alphabetical order (both service sections and tool names):
     ```markdown
     | {toolset}_{resource}_{operation} | Natural language prompt | none |
     ```
3. **Consolidated Tools (`servers/Azure.Mcp.Server/src/Resources/consolidated-tools.json`):**
   - Add the new tool to its matching consolidated category (or propose a new category if none fits) and update descriptions. Verify the exact tool name from `servers/Azure.Mcp.Server/src/bin/Debug/net10.0`:
     ```powershell
     ./azmcp[.exe] tools list --name --namespace <tool_area>
     ```
4. **Package README (`servers/Azure.Mcp.Server/README.md`):**
   - Add example prompts under **What can you do with the Azure MCP Server?** (grouped by service) and update **Complete List of Supported Azure Services** if adding a toolset. Validate with:
     ```powershell
     & "eng\scripts\Process-PackageReadMe.ps1" -Command "validate" -InputReadMePath "servers/Azure.Mcp.Server/README.md"
     ```
5. **Changelog Entry:**
   - Run `./eng/scripts/New-ChangelogEntry.ps1 -ChangelogPath servers/Azure.Mcp.Server/CHANGELOG.md` (or author manually per [`docs/changelog-entries.md`](https://github.com/microsoft/mcp/blob/main/docs/changelog-entries.md)).
6. **CODEOWNERS (new toolsets only — `.github/CODEOWNERS`):**
   - Add `/tools/Azure.Mcp.Tools.{Toolset}/ @your-team`.

**GATE:** `./eng/scripts/Update-AzCommandsMetadata.ps1` succeeds.

---

## Phase 5: Tool Description Evaluation

Validate `[CommandMetadata].Description` against your Phase 4 prompts using **ToolDescriptionEvaluator** (see [`eng/tools/ToolDescriptionEvaluator/Quickstart.md`](https://github.com/microsoft/mcp/blob/main/eng/tools/ToolDescriptionEvaluator/Quickstart.md)).

### Prerequisites

```powershell
$env:AOAI_ENDPOINT = "https://<your-resource>.openai.azure.com/openai/deployments/<embeddings-deployment-name>/embeddings?api-version=<api-version>"
$env:TEXT_EMBEDDING_API_KEY = "your_api_key_here"
```

> Internal contributors: refer to **Before creating a pull request** in [mcp-getting-started](https://eng.ms/docs/products/azure-developer-experience/mcp/mcp-getting-started) for team credentials.

### Run Evaluation

**Option A — Single tool description (fastest while iterating):**

```powershell
dotnet run --project eng/tools/ToolDescriptionEvaluator/src -- --test-single-tool `
  --tool-description "Lists all user-assigned managed identities in a subscription" `
  --prompt "show me my managed identities" `
  --prompt "list managed identities in my subscription" `
  --prompt "what identities do I have"
```

**Option B — Full area against `e2eTestPrompts.md`:**

```powershell
pushd eng/tools/ToolDescriptionEvaluator
./scripts/Run-ToolDescriptionEvaluator.ps1 -Area "{Toolset}" -BuildAzureMcp
popd
```

Target: **Top 3 ranking** and confidence score **≥ 0.4** (`>= 0.6` is excellent). If `< 0.4`, refine `[CommandMetadata].Description` with action verbs, specific Azure service/resource nouns, output fields, and common synonyms, then re-test.

**GATE:** Score ≥ 0.4 and top 3 ranking (or manual check that the description is specific and action-oriented if Azure OpenAI credentials are unavailable).

---

## Phase 6: Final Validation

Run all checks in order after all code and documentation edits are complete:

```powershell
# 1. Build (0 warnings — TreatWarningsAsErrors=true)
dotnet build tools/Azure.Mcp.Tools.{Toolset}/src

# 2. Format & clean unused usings
dotnet format Microsoft.Mcp.slnx --verify-no-changes --include "tools/Azure.Mcp.Tools.{Toolset}/**"

# 3. Unit tests (no regressions)
dotnet test tools/Azure.Mcp.Tools.{Toolset}/tests/Azure.Mcp.Tools.{Toolset}.Tests -- --filter-not-trait "TestType=Live"

# 4. Spell check
.\eng\common\spelling\Invoke-Cspell.ps1

# 5. Full local verification (C#, README/npx packages)
./eng/scripts/Build-Local.ps1 -VerifyNpx

# 6. Native AOT check
./eng/scripts/Build-Local.ps1 -IncludeNative
```

If Native AOT fails, follow [`docs/aot-compatibility.md`](https://github.com/microsoft/mcp/blob/main/docs/aot-compatibility.md) (do **not** modify the `#if !BUILD_NATIVE` block in `Program.cs` or exclude arbitrary external packages).

**GATE:** All applicable checks pass.

---

## Phase 7: PR Readiness Checklist

Verify applicable items before opening a PR (**one tool per PR**):

### Core & Scope
- [ ] Flat `[Option]` POCO (`ISubscriptionOption` when needed); two-generic command (`sealed`, primary constructor, `ResultTypeInfo` + `SetResult`, `HandleException`)
- [ ] `ValidateOptions` uses `ValidationResult.AddError(message, telemetrySafeMessage)` only (never mutates `Errors`)
- [ ] `CancellationToken` propagated as final parameter across all async methods and `.WithCancellation(...)`
- [ ] Command & service registered as singletons in `{Toolset}Setup.cs`; no per-request state, transport checks, or `HttpContext` usage
- [ ] **New toolsets only:** packages in `Directory.Packages.props` + `.csproj`, `Update-Solution.ps1 -All` run, alphabetical registration in `Program.cs` `RegisterAreas()`, `.github/CODEOWNERS` entry added

### Testing
- [ ] Unit tests inherit the right base class (`SubscriptionCommandUnitTestsBase` vs `CommandUnitTestsBase`), test validation + `TelemetrySafeMessage` + negative security cases, and use `{Toolset}JsonContext`
- [ ] **Azure service commands only:** `test-resources.bicep` + `test-resources-post.ps1` exist and validate; `RecordedCommandTestsBase` tests pass in Playback (`AssertLocalToolIsUnavailableInHttpMode` for `LocalRequired = true`); recordings pushed and `assets.json` committed
- [ ] **Create / create-or-update commands only:** tests verify no-option secure defaults, each opt-in downgrade option, and create-or-update omission behavior

### Security & Documentation
- [ ] Resource-specific input validation, endpoint validation (`IAzureService`/`IEndpointValidator`), safe logging (no `{@Options}`), non-revealing errors, and `Secret = true` wherever sensitive data may be returned
- [ ] **If AI generated any code:** prompt included [AI security requirements](https://github.com/microsoft/mcp/blob/main/servers/Azure.Mcp.Server/docs/new-command.md#using-ai-to-generate-tool-code) and output was reviewed
- [ ] `azmcp-commands.md` + `Update-AzCommandsMetadata.ps1`, `e2eTestPrompts.md` (alphabetical), `consolidated-tools.json`, `README.md`, and changelog entry (`-ChangelogPath`) updated
- [ ] Phase 5 (`ToolDescriptionEvaluator` ≥ 0.4, top 3) and Phase 6 (`build`, `format`, `test`, `cspell`, `Build-Local.ps1 -VerifyNpx`, `Build-Local.ps1 -IncludeNative`) all green
