<!-- Copyright (c) Microsoft Corporation. -->
<!-- Licensed under the MIT License. -->

# Implementing a New Command in Azure MCP

This document is the implementation reference for adding new commands ("toolset commands") to Azure MCP. For the gated, step-by-step workflow, use the [add-azure-mcp-tools skill](https://github.com/microsoft/mcp/blob/main/.github/skills/add-azure-mcp-tools/SKILL.md).

## Contents

- [Toolset Pattern and Command Architecture](https://github.com/microsoft/mcp/blob/main/servers/Azure.Mcp.Server/docs/new-command.md#toolset-pattern-and-command-architecture)
- [Options and Validation](https://github.com/microsoft/mcp/blob/main/servers/Azure.Mcp.Server/docs/new-command.md#options-and-validation)
- [Command Metadata and Result Contracts](https://github.com/microsoft/mcp/blob/main/servers/Azure.Mcp.Server/docs/new-command.md#command-metadata-and-result-contracts)
- [Error Handling](https://github.com/microsoft/mcp/blob/main/servers/Azure.Mcp.Server/docs/new-command.md#error-handling)
- [Azure Service Integration and Sovereign Clouds](https://github.com/microsoft/mcp/blob/main/servers/Azure.Mcp.Server/docs/new-command.md#azure-service-integration-and-sovereign-clouds)
- [Security and Threat Mitigations](https://github.com/microsoft/mcp/blob/main/servers/Azure.Mcp.Server/docs/new-command.md#security-and-threat-mitigations)
- [Testing Reference](https://github.com/microsoft/mcp/blob/main/servers/Azure.Mcp.Server/docs/new-command.md#testing-reference)
- [Remote MCP Server Considerations](https://github.com/microsoft/mcp/blob/main/servers/Azure.Mcp.Server/docs/new-command.md#remote-mcp-server-considerations)
- [Consolidated Mode Requirements](https://github.com/microsoft/mcp/blob/main/servers/Azure.Mcp.Server/docs/new-command.md#consolidated-mode-requirements)
- [Code Quality, AOT, and Troubleshooting](https://github.com/microsoft/mcp/blob/main/servers/Azure.Mcp.Server/docs/new-command.md#code-quality-aot-and-troubleshooting)

## Toolset Pattern and Command Architecture

### Toolset Directory Layout

All new Azure services and their commands use the Toolset pattern:

- **Toolset code** goes in `tools/Azure.Mcp.Tools.{Toolset}/src` (e.g., `tools/Azure.Mcp.Tools.Storage/src`)
- **Tests** go in `tools/Azure.Mcp.Tools.{Toolset}/tests` (e.g., `tools/Azure.Mcp.Tools.Storage/tests`)

This keeps all code, options, models, JSON serialization contexts, and tests for a toolset together. See `tools/Azure.Mcp.Tools.Storage` for a canonical reference implementation.

### Command Hierarchy and Design Principles

1. **`IBaseCommand`:** Defines `Name`, `Description`, `Title`, `Metadata`, `ResultTypeInfo`, `GetCommand()`, and `ExecuteAsync()`. Option registration and validation are driven by `[Option]` attributes and `ValidateOptions()` on `BaseCommand<TOptions, TResult>`.
2. **Two-generic command hierarchy:**
   ```
   IBaseCommand
   └── BaseCommand<TOptions, TResult>
       └── AuthenticatedCommand<TOptions, TResult>
           └── SubscriptionCommand<TOptions, TResult>  (where TOptions : ISubscriptionOption)
               └── {Resource}{Operation}Command  (sealed, concrete)
   ```
   - Commands use primary constructors with `ILogger`, service interface, and `ISubscriptionResolver` (for subscription commands).
   - Command classes are always `sealed` unless explicitly designed as an intermediate base class.
   - Choose the narrowest base class: `SubscriptionCommand<TOptions, TResult>` (needs Azure auth + `--subscription`), `AuthenticatedCommand<TOptions, TResult>` (needs Azure auth without `--subscription`, e.g., data-plane commands like `BlobGetCommand`), or `BaseCommand<TOptions, TResult>` (no Azure auth).
   - Set behavioral characteristics using `[CommandMetadata]`.

### Command Naming and Group Hierarchy

Commands follow the CLI naming convention:

```
azmcp <azure service> <resource> <operation>
```

Example: `azmcp storage container get`

- In code, Azure services are called **Toolsets** to avoid ambiguity with service classes.
- Commands are registered in `RegisterCommands` in `tools/Azure.Mcp.Tools.{Toolset}/src/{Toolset}Setup.cs`.
- **Command group names** must use concatenated lowercase or dash-separated names — **never underscores**:
  - ✅ `new CommandGroup("entraadmin", ...)` / `"resourcegroup"` / `"entra-admin"`
  - ❌ `new CommandGroup("entra_admin", ...)`
- **Separate resources from operations** in the group hierarchy:
  - ✅ `azmcp postgres server param set` (groups: `server` → `param`, operation: `set`)
  - ❌ `azmcp postgres server setparam`
  - ✅ `azmcp storage blob upload permission set`
  - ❌ `azmcp storage blobupload`

**File and Class Naming Pattern:** `{Resource}{SubResource?}{Operation}Command`
- `Resource` = top-level domain entity (`Server`, `Database`, `FileSystem`)
- `SubResource` (optional) = nested concept (`Config`, `Param`, `SubnetSize`)
- `Operation` = action verb (`List`, `Get`, `Set`, `Show`, `Delete`) or computed domain noun (`SubnetSize` in `FileSystemSubnetSizeCommand`)
- ✅ `ServerListCommand`, `ServerConfigGetCommand`, `ServerParamSetCommand`, `TableSchemaGetCommand`, `FileSystemSubnetSizeCommand`
- ❌ `GetConfigCommand` (missing resource), `ListServerCommand` (verb precedes resource), `FileSystemRequiredSubnetSizeCommand` (overly verbose)

Apply this consistently to command (`FileSystemListCommand.cs`), options (`FileSystemListOptions.cs`), and unit test (`FileSystemListCommandTests.cs`) files.

### Intermediate Base Commands

Each toolset may add intermediate base command classes only when sharing real cross-command behavior (validation, error handling). **If a base class only existed for `RegisterOptions`/`BindOptions`, delete it** and extend `SubscriptionCommand<TOptions, TResult>` directly.

When shared behavior is needed, keep options classes flat and use **interface constraints** on `TOptions`:

```csharp
using System.Diagnostics.CodeAnalysis;
using Azure.Mcp.Core.Commands.Subscription;
using Azure.Mcp.Tools.{Toolset}.Options;
using Microsoft.Mcp.Core.Commands;
using Microsoft.Mcp.Core.Extensions;

namespace Azure.Mcp.Tools.{Toolset}.Commands;

public interface I{Toolset}AccountOption
{
    string Account { get; }
}

public interface I{Resource}Option : I{Toolset}AccountOption
{
    string {Resource}Name { get; }
}

public abstract class Base{Toolset}Command<
    [DynamicallyAccessedMembers(TrimAnnotations.CommandAnnotations)] TOptions, TResult>(
    ISubscriptionResolver subscriptionResolver)
    : SubscriptionCommand<TOptions, TResult>(subscriptionResolver)
    where TOptions : class, ISubscriptionOption, I{Toolset}AccountOption
{
    public override void ValidateOptions(TOptions options, ValidationResult validationResult)
    {
        base.ValidateOptions(options, validationResult);
        // Shared validation using options.Account
    }
}

public abstract class Base{Resource}Command<
    [DynamicallyAccessedMembers(TrimAnnotations.CommandAnnotations)] TOptions, TResult>(
    ISubscriptionResolver subscriptionResolver)
    : Base{Toolset}Command<TOptions, TResult>(subscriptionResolver)
    where TOptions : class, ISubscriptionOption, I{Resource}Option
{
    public override void ValidateOptions(TOptions options, ValidationResult validationResult)
    {
        base.ValidateOptions(options, validationResult);
        // Shared resource-level validation using options.{Resource}Name
    }
}
```

### Required Files Summary

1. Flat options POCO with `[Option]` attributes: `src/Options/{Resource}/{Resource}{Operation}Options.cs`
2. Command class: `src/Commands/{Resource}/{Resource}{Operation}Command.cs`
3. Service interface & implementation (when service logic is needed): `src/Services/I{ServiceName}Service.cs` and `src/Services/{ServiceName}Service.cs`
4. Source-generated JSON context: `src/Commands/{Toolset}JsonContext.cs`
5. Command & service registration in `src/{Toolset}Setup.cs` (`ConfigureServices` and `RegisterCommands`)
6. Unit tests: `tests/Azure.Mcp.Tools.{Toolset}.Tests/{Resource}/{Resource}{Operation}CommandTests.cs`
7. **New toolsets only:** package versions in `Directory.Packages.props`, solution registration via `pwsh eng/scripts/Update-Solution.ps1 -All`, and alphabetical `IAreaSetup` registration in `servers/Azure.Mcp.Server/src/Program.cs` `RegisterAreas()`.
8. **Azure service commands only:** recorded live tests (`{Toolset}CommandTests.cs`), `assets.json`, `tests/test-resources.bicep` (with minimal SKUs and RBAC), and `tests/test-resources-post.ps1` (required even if basic template). Non-Azure commands skip Bicep and live tests.

## Options and Validation

Options classes are flat POCOs with `[Option]` attributes. `OptionBinder` handles registration and binding automatically — never write `RegisterOptions`/`BindOptions` overrides, `OptionDefinitions` static classes, or `Command.Validators.Add` calls.

### Option Attribute Conventions

- **Flat POCOs:** No class inheritance; implement `ISubscriptionOption` when accepting `--subscription` (`ISubscriptionOption` defines `Subscription` only — add `Tenant` separately if accepted).
- **Binding & naming:** Public writable properties with `[Option(Description = "...")]` bind as `--kebab-case` (`LocalFilePath` → `--local-file-path`); unattributed properties are ignored. Set `Name` only when kebab-case is wrong (`[Option(Name = "foobar", ...)]` on `FooBar`). Use `[OptionContainer<TContainer>(Prefix = "prefix")]` for nested option objects (`--prefix-child-name`; optional containers cannot contain `required` children).
- **Required vs. optional:** Driven by the `required` keyword (`RequiredMemberAttribute`), not nullability. Use `required` when callers must supply the option; use nullable types (`?`) for optional values. Non-nullable value types (e.g., `int`) default to `0` without `required`.
- **Blank-string gotcha:** `OptionTypeHandler` skips blank/whitespace checks on optional options even when `AllowEmptyOrWhiteSpaceString` is `false`. Explicitly reject supplied blank values in `ValidateOptions` when the tool contract requires it (`ICursorOption.Cursor` is already rejected by `OptionBinder` when blank).
- **Descriptions & naming:** Use `OptionDescriptions.X` (`Subscription`, `Tenant`, `ResourceGroup`, `AuthMethod`, `Scope`) or a toolset `{Toolset}OptionDescriptions` constants class. Use `subscription` (never `subscriptionId`), `resourceGroup` (never `resourceGroupName`), singular nouns (`server`, `--account` without redundant `-name`), and align names/order with Azure SDK and sibling commands. Order command-specific options first, then `ResourceGroup`, `Subscription`, `Tenant`, `AuthMethod`.
- **Cursor pagination:** For paged tools, follow [`docs/tool-pagination.md`](https://github.com/microsoft/mcp/blob/main/docs/tool-pagination.md): implement `ICursorOption` with `[Option(Description = ICursorOption.CursorDescription)]`, bind fixed arguments with `PaginationCursor`, return `nextCursor` in the result payload (never on `CommandResponse`), and omit `nextCursor`/return `null` on completion.

### Option Usage Patterns

**Mutually exclusive options** — declare both optional and validate in `ValidateOptions`:

```csharp
public class MyCommandOptions : ISubscriptionOption
{
    [Option(Description = "First exclusive option.")]
    public string? EitherThis { get; set; }

    [Option(Description = "Second exclusive option.")]
    public string? OrThat { get; set; }

    [Option(Description = OptionDescriptions.Subscription)]
    public string? Subscription { get; set; }

    [Option(Description = OptionDescriptions.Tenant)]
    public string? Tenant { get; set; }
}

// In ValidateOptions:
var hasEither = !string.IsNullOrWhiteSpace(options.EitherThis);
var hasOr = !string.IsNullOrWhiteSpace(options.OrThat);
if (!hasEither && !hasOr)
{
    const string message = "Either --either-this or --or-that must be provided.";
    validationResult.AddError(message, message);
}
else if (hasEither && hasOr)
{
    const string message = "Cannot specify both --either-this and --or-that. Use only one.";
    validationResult.AddError(message, message);
}
```

**Constrained / allowed-value options** — validate deterministically in `ValidateOptions`:

```csharp
if (!s_allowedFormats.Contains(options.Format))
{
    const string message = "--format must be one of: json, table, or csv.";
    validationResult.AddError(message, message);
}
```

**Shared option interfaces (for intermediate base commands)** — compose small interfaces while keeping the concrete options POCO flat:

```csharp
public interface IStorageAccountOption { string Account { get; } }
public interface IContainerOption : IStorageAccountOption { string Container { get; } }

public class BlobUploadOptions : ISubscriptionOption, IContainerOption
{
    [Option(Description = "The name of the Azure Storage account.")]
    public required string Account { get; set; }

    [Option(Description = "The name of the container within the storage account.")]
    public required string Container { get; set; }

    [Option(Description = OptionDescriptions.Subscription)]
    public string? Subscription { get; set; }
}
```

### Telemetry-Safe Validation Errors

Always use `ValidationResult.AddError(message, telemetrySafeMessage)` for every validation error, including static messages and shared base-command validation. A static caller message without tool parameter values is telemetry-safe and can be used for both arguments. Fixed option names and allowed values are fine:

```csharp
const string message = "--format must be one of: json, table, or csv.";
validationResult.AddError(message, message);
```

If an existing caller message includes any tool parameter value, use a separate static telemetry message instead. Any parameter can contain sensitive data, even if it is normally considered non-sensitive:

```csharp
validationResult.AddError(callerMessage, "Invalid format option.");
```

The first message is returned to the caller unchanged; only the second is used for failure telemetry. The telemetry message must be static and non-empty. Never include option values, resource identifiers, secrets, or exception text in it. Caller messages must not expose secrets either.

Do not use the `Errors` property to add, replace, remove, or reorder errors, even when the caller message is static. Direct collection changes bypass the tracked caller/telemetry message pairs. If any error is untracked or the collection no longer matches those pairs, `TelemetrySafeMessage` falls back to `"Invalid options."` for the entire validation result, losing the specific telemetry-safe failure reasons. This also applies if a base validator adds an untracked error. Existing direct writes remain compatible with caller-facing validation, but must not be copied into new code.

Test custom validation by asserting both the caller-facing errors and `TelemetrySafeMessage`, including that supplied values do not appear in telemetry.

## Command Metadata and Result Contracts

See [Phase 1c](https://github.com/microsoft/mcp/blob/main/.github/skills/add-azure-mcp-tools/SKILL.md#1c-command-class) for the canonical two-generic command template. Key contract rules:

- **Structured output:** Declare the result record `public` inside the command, override `ResultTypeInfo` with the source-generated `JsonTypeInfo<TResult>`, and call `SetResult(context, ...)` on success. When `--structured-output-mode` is enabled, the same type supplies `outputSchema` and `structuredContent`; see [`docs/output-schema-migration.md`](https://github.com/microsoft/mcp/blob/main/docs/output-schema-migration.md).
- **Empty arrays:** Coalesce null/empty array payloads to `[]`.

### CommandMetadata Properties and Validation

Every `[CommandMetadata]` property is required and must be declared explicitly (even when using defaults):

- **`Id`:** Fresh unique GUID per command.
- **`OperationPlane`:** Classify by the API the tool acts against to produce the user's result (see [`docs/design/operation-plane-metadata.md`](https://github.com/microsoft/mcp/blob/main/docs/design/operation-plane-metadata.md)):
  - `Control`: Azure Resource Manager or other management-plane APIs (listing/creating/updating resources).
  - `Data`: Service data-plane APIs. **ARM lookups performed only as setup** (resolving a subscription or reading an endpoint) do **not** make a tool `Both`.
  - `Both`: Distinct user-facing actions on both planes in one call, or argument-dependent dispatch between planes.
  - `NotApplicable`: Calls no service (embedded docs, local code generation, server control).

Cross-check behavioral flags against these heuristics before committing:

| Property | Set `true` when | Set `false` when | Validation heuristic |
|---|---|---|---|
| `Destructive` | Deletes, purges, resets, revokes, updates, or replaces resources | Read-only or non-destructive operations | `true` for `delete`/`remove`/`purge`/`reset`/`revoke`/`update`; `false` for `list`/`get`/`show`/`query`/`describe` |
| `Idempotent` | Repeated calls with identical inputs return the same result or converge to the same state | Rotates keys/secrets, auto-generates names, or accumulates side effects | `false` if calling twice with identical arguments produces a different result |
| `OpenWorld` | Interacts with arbitrary user-supplied URLs or unpredictable external systems | Calls ARM, Microsoft Graph, or well-defined Microsoft APIs | Most Azure tools are `false` |
| `ReadOnly` | Only retrieves/queries data | Creates, updates, deletes, writes, or uploads | Usually the inverse of `Destructive` (both `false` for create operations) |
| `Secret` | May return keys, secrets, connection strings, passwords, tokens, certificates, or credential metadata | Returns only resource names or non-sensitive configuration | Default `true` if name/resource mentions `credential`/`secret`/`key`/`password`/`certificate`/`token`/`connectionstring` — when in doubt, set `true` |
| `LocalRequired` | Requires local files, CLI tools (Azure CLI, Docker, npm), or locally installed software | Pure remote API calls | `LocalRequired = true` tools are hidden/rejected in remote HTTP mode |

### JSON Serialization Context

Register every top-level result record and returned model in one source-generated context per toolset (`src/Commands/{Toolset}JsonContext.cs`, matching class name), with `[JsonSerializable]` attributes sorted alphabetically by model name:

```csharp
[JsonSerializable(typeof({Resource}{Operation}Command.{Resource}{Operation}CommandResult))]
[JsonSerializable(typeof(YourModelType))]
[JsonSourceGenerationOptions(
    PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase,
    DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull)]
internal partial class {Toolset}JsonContext : JsonSerializerContext;
```

### Toolset Registration Reference

Register new toolsets alphabetically in `servers/Azure.Mcp.Server/src/Program.cs` `IAreaSetup[] RegisterAreas()` (outside `#if !BUILD_NATIVE`; do **not** add exclusions there — see [Native AOT Compatibility](https://github.com/microsoft/mcp/blob/main/servers/Azure.Mcp.Server/docs/new-command.md#native-aot-compatibility)).

## Error Handling

Always call `HandleException(context, ex)` inside command `catch` blocks. `BaseCommand` and `AuthenticatedCommand` already handle standard error mapping, telemetry, and response formatting:

- **`HandleException` (`BaseCommand`):**
  - Handles `CommandValidationException` first (status code, formatted missing options, `TelemetryFailureMessage`, `Results = null`).
  - Records structured telemetry tags (`StatusCode`, plus `ErrorCode`/`RequestId` for `RequestFailedException` and `ErrorCode`/`CorrelationId`/`AadStsErrorCode` for MSAL exceptions).
  - Sets `response.Status = GetStatusCode(ex)`, appends the troubleshooting link (`https://aka.ms/azmcp/troubleshooting`) to `GetErrorMessage(ex)`, and writes `ExceptionResult` using `CoreJsonContext.Default.ExceptionResult` (stack trace included only in `DEBUG` builds).
- **Default `GetStatusCode` mappings:**
  - `BaseCommand`: `ArgumentException` → `400`, `OnBehalfOfUserTokenRequiredException` → `403`, `InvalidOperationException` → `422`, `HttpRequestException` → status or `503`, `RequestFailedException` → Azure status, `MsalServiceException` → MSAL status, fallback → `500`.
  - `AuthenticatedCommand` (and `SubscriptionCommand`) adds: `KeyNotFoundException` → `404`, `AuthenticationFailedException` → `401`, `TimeoutException` / `TaskCanceledException` → `504`.
- **Default `GetErrorMessage` mappings (`AuthenticatedCommand`):** Already produces actionable messages for `CredentialUnavailableException`, `AuthenticationFailedException`, `RequestFailedException` with `InvalidAuthenticationTokenTenant` (401 tenant mismatch), `HttpRequestException`, `TimeoutException`, and `TaskCanceledException`.

### Overriding Error Handlers

Override `GetErrorMessage` or `GetStatusCode` **only** to add service-specific guidance (such as specific `RequestFailedException.ErrorCode` values or data-plane RBAC hints), and always fall back to `base`:

```csharp
protected override string GetErrorMessage(Exception ex) => ex switch
{
    RequestFailedException reqEx when reqEx.Status == (int)HttpStatusCode.NotFound =>
        "Resource not found. Verify the resource name and that you have access.",
    RequestFailedException reqEx when reqEx.Status == (int)HttpStatusCode.Forbidden =>
        "Access denied. Ensure your identity has the required RBAC role on the target resource.",
    _ => base.GetErrorMessage(ex)
};
```

- Do **not** remap `OnBehalfOfUserTokenRequiredException` away from `403 Forbidden` (it derives from `Exception`, not `InvalidOperationException`, specifically so commands do not mask it as a retryable 5xx error).
- Never include raw option objects (`{@Options}`), tokens, keys, or raw backend payloads in logs or caller messages — log only individually named, known-safe fields (`Name`, `options.Subscription`, `options.ResourceGroup`).

## Azure Service Integration and Sovereign Clouds

### Package Management and Base Class Selection

**Package Management:**
- **Resource Graph reads:** No extra Azure SDK package needed (`Azure.ResourceManager.ResourceGraph` is in core). Add toolset-specific `Azure.ResourceManager.*` packages only for specialized ARM reads or writes.
- **Add to `Directory.Packages.props` first** (`<PackageVersion Include="Azure.ResourceManager.{Service}" Version="..." />`), then add `<PackageReference Include="Azure.ResourceManager.{Service}" />` (without version) in `Azure.Mcp.Tools.{Toolset}.csproj`. Keep versions consistent across projects.

**Service Base Class Selection:**
1. **Azure Resource Graph (ARG) queries (`BaseAzureResourceService`):** Provides `ExecuteResourceQueryAsync<T>()`, `ExecuteSingleResourceQueryAsync<T>()`, and `ExecuteManagementGroupResourceQueryAsync<T>()` with subscription resolution and tenant lookup.
   ```csharp
   public class MyService(IAzureService azureService)
       : BaseAzureResourceService(azureService), IMyService
   {
       public async Task<ResourceQueryResults<MyResource>> ListResourcesAsync(
           string? resourceGroup,
           string subscription,
           string? tenant = null,
           CancellationToken cancellationToken = default)
       {
           return await ExecuteResourceQueryAsync(
               "Microsoft.MyService/resources",
               resourceGroup,
               subscription,
               ConvertToMyResourceModel,
               tenant: tenant,
               cancellationToken: cancellationToken);
       }

       public async Task<MyResource?> GetResourceAsync(
           string resourceName,
           string? resourceGroup,
           string subscription,
           string? tenant = null,
           CancellationToken cancellationToken = default)
       {
           return await ExecuteSingleResourceQueryAsync(
               "Microsoft.MyService/resources",
               resourceGroup,
               subscription,
               ConvertToMyResourceModel,
               additionalFilter: $"name =~ '{EscapeKqlString(resourceName)}'",
               tenant: tenant,
               cancellationToken: cancellationToken);
       }

       private static MyResource ConvertToMyResourceModel(JsonElement item)
       {
           var data = MyResourceData.FromJson(item);
           return new MyResource(
               Name: data.ResourceName,
               Id: data.ResourceId,
               Location: data.Location.ToString(),
               Tags: data.Tags as IReadOnlyDictionary<string, string>);
       }
   }
   ```
2. **Direct ARM or data-plane operations (`BaseAzureService`):** Use `AzureService.GetSubscription(subscription, tenant, cancellationToken: cancellationToken)` and `await subscriptionResource.GetResourceGroupAsync(resourceGroup, cancellationToken)`:
   ```csharp
   public class MyService(IAzureService azureService)
       : BaseAzureService(azureService), IMyService
   {
       public async Task<MyResource> GetResourceAsync(
           string subscription,
           string resourceGroup,
           string resourceName,
           string? tenant = null,
           CancellationToken cancellationToken = default)
       {
           var subscriptionResource = await AzureService.GetSubscription(subscription, tenant, cancellationToken: cancellationToken);
           var rgResource = await subscriptionResource.GetResourceGroupAsync(resourceGroup, cancellationToken);
           var resource = await rgResource.Value
               .GetMyResources()
               .GetAsync(resourceName, cancellationToken: cancellationToken);
           return resource.Value;
       }
   }
   ```
   - Always access ARM resources through collection methods (e.g., `.GetSqlServers().GetAsync(name, cancellationToken: cancellationToken)`, `.GetVirtualMachineScaleSetRollingUpgrade().GetAsync(cancellationToken)`, `.GetVirtualMachineScaleSetVms().GetAllAsync(cancellationToken: cancellationToken)`), never `.GetSqlServerAsync(...)`.
   - Always `await` `GetResourceGroupAsync(...)` rather than calling synchronous `.GetResourceGroup(...)`.

### Method Signature Consistency and CancellationToken

All async service methods must take `CancellationToken` as the final parameter (enforced by [CA2016](https://learn.microsoft.com/dotnet/fundamentals/code-analysis/quality-rules/ca2016)):

```csharp
// ✅ Correct: parameters aligned with line breaks
Task<List<string>> GetStorageAccounts(
    string subscription,
    string? tenant = null,
    CancellationToken cancellationToken = default);

// ❌ Incorrect: single line or missing CancellationToken
Task<List<string>> GetStorageAccounts(string subscription, string? tenant = null);
```

- Align parameters one per line with blank lines between declarations.
- Use `= default` on `CancellationToken` only when other parameters also have default values; always pass `cancellationToken` explicitly at call sites.
- Pass `cancellationToken: cancellationToken` to all Azure SDK calls and use `.WithCancellation(cancellationToken)` on `await foreach`:
  ```csharp
  await foreach (var rg in subscription.GetResourceGroups().WithCancellation(cancellationToken))
  ```
- Never pass `CancellationToken.None` or `default` as a `CancellationToken` argument.

### Long-Running Operations

Long-running operations do not offer arbitrary polling-interval configuration, so polling during playback tests can waste CPU time. Use the two-call pattern: start with `WaitUntil.Started`, then wait with `BaseAzureService.WaitForLroCompletionAsync` so playback can bypass polling delays:

```csharp
var lroOperation = Service.LroAsync(WaitUntil.Started, cancellationToken);
await WaitForLroCompletionAsync(lroOperation, cancellationToken);
```

### Sovereign Cloud Support

All services **must** support sovereign clouds by default. Never hardcode cloud-specific endpoints.

- **ARM & Resource Graph (`BaseAzureResourceService` / `CreateArmClientWithApiVersionAsync`):** Automatically cloud-aware via `CloudConfiguration.ArmEnvironment`.
- **Data-plane SDKs requiring an explicit endpoint URL:** Switch on `AzureService.CloudConfiguration.CloudType` and validate the completed endpoint with `AzureService.ValidateAzureServiceEndpoint` before credential acquisition or client construction (replace `my-service` with the validator's service type):

```csharp
public class MyService(IAzureService azureService)
    : BaseAzureResourceService(azureService), IMyService
{
    private async Task<MyDataPlaneClient> CreateDataPlaneClientAsync(
        string resourceName,
        string? tenant = null,
        CancellationToken cancellationToken = default)
    {
        var endpoint = GetResourceEndpoint(resourceName);
        AzureService.ValidateAzureServiceEndpoint(endpoint: endpoint, serviceType: "my-service");
        var options = AddDefaultPolicies(new MyClientOptions());
        options.Transport = new HttpClientTransport(AzureService.GetClient());
        return new MyDataPlaneClient(
            new Uri(endpoint),
            await GetCredential(tenant, cancellationToken),
            options);
    }

    private string GetResourceEndpoint(string resourceName) =>
        AzureService.CloudConfiguration.CloudType switch
        {
            AzureCloudConfiguration.AzureCloud.AzurePublicCloud =>
                $"https://{resourceName}.service.core.windows.net",
            AzureCloudConfiguration.AzureCloud.AzureChinaCloud =>
                $"https://{resourceName}.service.core.chinacloudapi.cn",
            AzureCloudConfiguration.AzureCloud.AzureUSGovernmentCloud =>
                $"https://{resourceName}.service.core.usgovcloudapi.net",
            _ => throw new ArgumentException(
                $"The configured Azure cloud is not supported. Value given: '{AzureService.CloudConfiguration.CloudType}'.",
                nameof(AzureService.CloudConfiguration.CloudType))
        };
}
```

Keep cloud suffixes synchronized with `core/Microsoft.Mcp.Core/src/Helpers/EndpointValidator.AllowLists.cs` so `ValidateAzureServiceEndpoint` accepts constructed endpoints for every supported cloud.

| Scenario | Requirement |
|---|---|
| Resource Graph or ARM via `BaseAzureResourceService` / `CreateArmClientWithApiVersionAsync` | ✅ Cloud-aware automatically |
| Data-plane SDK requiring an explicit URL | ✅ `AzureService.CloudConfiguration.CloudType` switch (fail closed on unknown cloud) + `ValidateAzureServiceEndpoint` |
| Any hardcoded `*.windows.net`, `*.azure.com`, `*.chinacloudapi.cn`, etc. | ❌ Not allowed |

Reference implementations: `StorageService`, `CosmosService`, `SearchService`, and `ConfidentialLedgerService`.

### SDK Property Name Discovery

Azure SDK property names often differ from REST documentation. Verify them before coding via IntelliSense or assembly reflection:

```powershell
$dll = Get-ChildItem -Path "." -Recurse -Filter "Azure.ResourceManager.*.dll" | Select-Object -First 1 -ExpandProperty FullName
Add-Type -Path $dll
[Azure.ResourceManager.Compute.Models.VirtualMachineExtensionInstanceView].GetProperties() | Select-Object Name, PropertyType
```

Common patterns and gotchas:
- **Timestamps & dates:** `StartOn`/`LastActionOn` (not `StartTime`), `CreatedOn` (not `CreationDate`), `EarliestRestoreOn` (not `EarliestRestoreDate`).
- **Extension types & SKUs:** `VirtualMachineExtensionInstanceViewType` (not `TypeHandlerType`), `CurrentSku?.Name` (not `Edition`).
- **`AzureLocation`:** Struct — use `Location.ToString()` or `Location.Name`, never `Location?.Name`.
- **Tags:** Cast `data.Tags as IReadOnlyDictionary<string, string>` (direct `IDictionary<string, string>` assignment fails with CS1503).
- **Missing properties:** Set to `null` and document why in comments; do not derive missing values from unrelated sources unless required.

## Security and Threat Mitigations

### Secure Resource Provisioning Defaults

Tools that create or create-or-update Azure resources must produce a secure configuration when the caller omits security-related options. Do not rely on an Azure service's current defaults. Explicitly set every supported security property in the typed SDK request model so a service-side default change cannot make the tool less secure.

- Disable public network access, public endpoints or IPs, and anonymous access by default where the service supports it. Do not add public connectivity merely to make a newly created resource immediately reachable.
- Require encrypted transport by default: enable HTTPS-only access, disable non-TLS endpoints or ports, and set the minimum TLS version to the service's current secure recommendation (at least TLS 1.2). A weaker transport setting requires an explicit opt-in.
- Prefer Managed Identity and RBAC for authentication. Enable a system-assigned identity or accept a user-assigned identity where supported, and disable local or key-based authentication when possible. For service-to-service access, do not make connection strings, account keys, or API keys the default path.
- For create-or-update operations, omitted security options must never weaken an existing resource. Apply secure defaults when creating a resource; when updating one, preserve its current security settings unless the caller explicitly requests a supported downgrade.

A tool may expose a less secure behavior for a legitimate compatibility or development scenario, but only as a deliberate caller choice:

- Use a narrowly scoped option whose name states the behavior, such as `--allow-public-network-access`, `--allow-http`, `--minimum-tls-version`, or `--enable-local-auth`. Avoid ambiguous options such as `--secure false`.
- Keep the secure behavior as the effective default when the option is omitted. Never infer consent to a downgrade from another option, selected SKU, or pre-existing dependency.
- State the secure default and the security consequence in the `[Option]` description and command documentation. Validate allowed values and incompatible combinations before sending the request.
- Do not accept, log, persist, or return connection strings or keys merely because local authentication was enabled. Follow the existing secret-handling requirements for any command that must handle them.

Add tests proving that an invocation without security options sends or creates the secure configuration. Add focused tests for each downgrade option and for create-or-update omission behavior; use a recorded live test to verify the resulting Azure resource state when practical.

### MCP-Specific Threat Patterns

| Threat | Established mitigation in this project |
|--------|----------------------------------------|
| Input abuse (oversized/malformed names) | Override `ValidateOptions` with resource-specific length and format checks using deterministic validation first (length bounds, allowed-value sets, character/category checks). For query inputs, use a dedicated validator class — see `CosmosQueryValidator.EnsureReadOnlySelect` (`tools/Azure.Mcp.Tools.Cosmos/src/Validation/CosmosQueryValidator.cs`) as a reference for length cap, keyword blocking, and injection pattern detection. |
| Injection into downstream systems | For user-supplied queries: use a validator class that enforces a single read-only statement, caps length, strips/blocks dangerous tokens, and detects tautology patterns. Do not interpolate user input into query strings directly — prefer parameterized APIs where available. For blob/resource URIs: call `IAzureService.ValidateAzureServiceEndpoint` or the injected `IEndpointValidator.ValidateAzureServiceEndpoint` before constructing any client (see `tools/Azure.Mcp.Tools.Compute/src/Services/ComputeService.cs` blob URI handling as a reference). |
| Secret leakage via logs or error responses | Log only individually named, non-sensitive fields: `options.Subscription`, `options.ResourceGroup`, `Name`. Never use `{@Options}` or log connection strings, keys, or endpoint values. Override `GetErrorMessage` to return actionable but non-revealing messages — strip raw `RequestFailedException` bodies that may contain tokens or account metadata. |
| Cross-tenant/resource confusion | `SubscriptionCommand` base class enforces that `--subscription` is always present and resolved via `ISubscriptionResolver` before `ExecuteAsync` is called. Pass `options.Tenant` to all service calls so `IAzureService` can validate tenant context per-request. Fail explicitly if tenant context is ambiguous — do not fall back silently. |
| SSRF-like endpoint misuse | Prefer the existing `IAzureService` validation methods. Without that dependency, inject `IEndpointValidator`: call `ValidateAzureServiceEndpoint(endpoint, serviceType, armEnvironment)` for Azure data-plane endpoints and `ValidatePublicTargetUrl(url)` for arbitrary user-controlled targets. `EndpointValidator.ValidateExternalUrl(url, allowedHosts)` remains static for exact known external hosts. The injected validator resolves the command namespace through `ICommandContextAccessor`; callers never pass it. Register injected `IEndpointValidator` instances with `services.AddEndpointValidation()`. |
| Proxy behavior | Do not treat proxy routing as endpoint authorization. The shared factory selects one explicit proxy in `ALL_PROXY`, `HTTPS_PROXY`, then `HTTP_PROXY` order for both HTTP and HTTPS. Requests routed through a proxy omit transport DNS/IP checks because that proxy is the network boundary. `NO_PROXY`, operating-system bypass, and PAC `DIRECT` routes use the protected direct transport and retain filtering. Debug recording proxies remain a handler-wide exception. Endpoint validation remains required in every case. |

### Using AI to Generate Tool Code

Include these requirements in every prompt when scaffolding or generating command, service, or test code, and verify generated code complies before committing:

```
Requirements:
- Validate user-controlled inputs in `ValidateOptions` using resource-specific rules (naming rules, allowed values, length caps) where applicable; do not use one generic rule for all options.
- Prefer SDK/runtime validators and deterministic checks for user input validation.
- Use `ValidationResult.AddError(message, telemetrySafeMessage)` for every validation error; never add or modify errors through `ValidationResult.Errors`. A static caller message without tool parameter values is telemetry-safe and can be passed for both arguments. Otherwise, use a separate static telemetry message without user input or exception text.
- Log only individually named, known-safe parameters; never log option objects, credentials, keys, connection strings, or other secret-bearing fields.
- For endpoint/URL inputs, prefer `IAzureService` validation methods or inject the host-owned `IEndpointValidator`; use static `EndpointValidator.ValidateExternalUrl` only for exact known external hosts. Avoid direct interpolation of unvalidated input into URLs or downstream queries.
- For create or create-or-update tools, explicitly configure secure resource defaults (private access, HTTPS and strong TLS, and Managed Identity/RBAC where supported). Any weaker behavior must require a narrowly named, documented opt-in option, and omission during an update must not weaken the resource.
- Add negative tests for relevant security cases introduced by the command (for example malformed names, invalid endpoint hosts, or unsafe query text) rather than a one-size-fits-all set of tests.
- Keep error messages actionable but non-revealing: avoid exposing stack traces, raw backend payloads, or sensitive values to callers.
```

## Testing Reference

### Unit Testing Patterns

- **Base class:** Inherit `SubscriptionCommandUnitTestsBase<TCommand, TService>` for subscription commands (registers a mock `ISubscriptionResolver` in DI — using `CommandUnitTestsBase` causes DI failures), or `CommandUnitTestsBase<TCommand, TService>` for `AuthenticatedCommand` / `BaseCommand` commands.
- **Argument-based execution & context reuse:** Prefer `ExecuteCommandAsync("--param", "value")` so `[Option]` registration, `OptionBinder`, `ValidateOptions`, and `ISubscriptionResolver` run end-to-end. Note that `CommandUnitTestsBase` reuses a single `Context` across `ExecuteCommandAsync` calls within one test — create a fresh `CommandContext` or separate test methods when testing error and success cases.
- **Mocks & tokens:** Use `Arg.Any<CancellationToken>()` in NSubstitute setups, `TestContext.Current.CancellationToken` when invoking product code, and `Arg.Is(value)` or direct values (never `Arg.Is<T>(x => x == value)`). Never use `CancellationToken.None` or `default`.
- **Deserialization:** Always use `ValidateAndDeserializeResponse(response, {Toolset}JsonContext.Default.{Operation}CommandResult)` — never define custom test models or call context-less `JsonSerializer.Deserialize<TestModel>(json)`.
- **Environment variable mutation:** If any test mutates environment variables, reference `$(RepoRoot)core\Azure.Mcp.Core\tests\Azure.Mcp.Tests\Azure.Mcp.Tests.csproj` and add `AssemblyAttributes.cs`:
  ```csharp
  [assembly: Azure.Mcp.Tests.Helpers.ClearEnvironmentVariablesBeforeTest]
  [assembly: Xunit.CollectionBehavior(Xunit.CollectionBehavior.CollectionPerAssembly)]
  ```

```csharp
[Fact]
public void Constructor_InitializesCommandCorrectly()
{
    var command = Command.GetCommand();
    Assert.Equal("operation", command.Name);
    Assert.NotNull(command.Description);
    Assert.NotEmpty(command.Description);
}

[Theory]
[InlineData("", false, "required")]
[InlineData("--param invalid", false, "Invalid")]
[InlineData("--param value --subscription sub123", true, null)]
public async Task ExecuteAsync_ValidatesInput(
    string args, bool shouldSucceed, string? expectedError)
{
    if (shouldSucceed)
    {
        Service.Operation(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns([]);
    }

    var response = await ExecuteCommandAsync(args);
    Assert.Equal(shouldSucceed ? HttpStatusCode.OK : HttpStatusCode.BadRequest, response.Status);
    if (!shouldSucceed && expectedError is not null)
        Assert.Contains(expectedError, response.Message);
}

[Fact]
public async Task ExecuteAsync_DeserializationValidation()
{
    Service.Operation(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>())
        .Returns([]);

    var response = await ExecuteCommandAsync("--param", "value", "--subscription", "sub123");
    var result = ValidateAndDeserializeResponse(
        response, {Toolset}JsonContext.Default.{Operation}CommandResult);
    Assert.Empty(result.Items);
}

[Fact]
public async Task ExecuteAsync_HandlesServiceError()
{
    Service.Operation(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>())
        .ThrowsAsync(new Exception("Test error"));

    var response = await ExecuteCommandAsync("--param", "value", "--subscription", "sub123");

    Assert.Equal(HttpStatusCode.InternalServerError, response.Status);
    Assert.Contains("Test error", response.Message);
    Assert.Contains("troubleshooting", response.Message);
}
```

Run targeted tests using **Microsoft.Testing.Platform** syntax from the test project directory:

```powershell
pushd ./tools/Azure.Mcp.Tools.{Toolset}/tests/Azure.Mcp.Tools.{Toolset}.Tests

# Run tests for one command class
dotnet test -- --filter-class "*{Resource}{Operation}CommandTests"

# Run all unit tests in the toolset
dotnet test -- --filter-not-trait "TestType=Live"
popd
```

### Test Infrastructure Templates

Azure service commands require both `tests/test-resources.bicep` and `tests/test-resources-post.ps1` (required even if template-only). Non-Azure commands skip both and all live tests. See `/tools/Azure.Mcp.Tools.Storage/tests/test-resources.bicep` and `test-resources-post.ps1` for canonical examples.

**Bicep template (`tools/Azure.Mcp.Tools.{Toolset}/tests/test-resources.bicep`):**

```bicep
targetScope = 'resourceGroup'

@minLength(3)
@maxLength(17)  // Adjust based on service naming limits
@description('The base resource name. Service names have specific length restrictions.')
param baseName string = resourceGroup().name

@description('The client OID to grant access to test resources.')
param testApplicationOid string = deployer().objectId

// The test infrastructure will only provide baseName and testApplicationOid.
// Any additional parameters are for local deployments only and require default values.

@description('The location of the resource. By default, this is the same as the resource group.')
param location string = resourceGroup().location

resource serviceResource 'Microsoft.{Provider}/{resourceType}@{apiVersion}' = {
  name: baseName
  location: location
  properties: {
    // Minimal cost-effective SKU/config
  }

  resource testResource 'childResourceType@{apiVersion}' = {
    name: 'test{resource}'
    properties: {}
  }
}

resource serviceRoleDefinition 'Microsoft.Authorization/roleDefinitions@2018-01-01-preview' existing = {
  scope: subscription()
  // See https://learn.microsoft.com/azure/role-based-access-control/built-in-roles
  name: '{role-guid}'
}

resource appServiceRoleAssignment 'Microsoft.Authorization/roleAssignments@2022-04-01' = {
  name: guid(serviceRoleDefinition.id, testApplicationOid, serviceResource.id)
  scope: serviceResource
  properties: {
    principalId: testApplicationOid
    roleDefinitionId: serviceRoleDefinition.id
    description: '{Role Name} for testApplicationOid'
  }
}

output serviceResourceName string = serviceResource.name
output testResourceName string = serviceResource::testResource.name
```

Guidelines:
- Use minimal SKUs (Basic, Standard S0, LRS, serverless/minimal RU/s) and deploy only resources needed for testing.
- Use `baseName` (or `{baseName}{suffix}` if disambiguation is needed) and `test{resource}` for child resources.

**Post-deployment script (`tools/Azure.Mcp.Tools.{Toolset}/tests/test-resources-post.ps1`):**

```powershell
#!/usr/bin/env pwsh

# Copyright (c) Microsoft Corporation.
# Licensed under the MIT License.

#Requires -Version 6.0
#Requires -PSEdition Core

[CmdletBinding()]
param (
    [Parameter(Mandatory)]
    [hashtable] $DeploymentOutputs,

    [Parameter(Mandatory)]
    [hashtable] $AdditionalParameters
)

Write-Host "Running {Toolset} post-deployment setup..."

try {
    $serviceName = $DeploymentOutputs['{Toolset}']['serviceResourceName']['value']
    $resourceGroup = $AdditionalParameters['ResourceGroupName']
    Write-Host "Setting up test data for $serviceName in $resourceGroup..."
    Write-Host "{Toolset} post-deployment setup completed successfully."
}
catch {
    Write-Error "Failed to complete {Toolset} post-deployment setup: $_"
    throw
}
```

Validate with `az bicep build --file tools/Azure.Mcp.Tools.{Toolset}/tests/test-resources.bicep` and test deployment with `./eng/scripts/Deploy-TestResources.ps1 -Paths "{Toolset}"`.

### Test Project Configuration

The test `.csproj` **must** include `OutputType=Exe`, `HasLiveTests`/`HasUnitTests`, and a reference to `Azure.Mcp.Server.csproj`, or live tests will fail with `"MCP server process exited unexpectedly"` / `"azmcp.exe not found"`:

```xml
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <IsTestProject>true</IsTestProject>
    <OutputType>Exe</OutputType>
    <HasLiveTests>true</HasLiveTests>
    <HasUnitTests>true</HasUnitTests>
  </PropertyGroup>

  <ItemGroup>
    <ProjectReference Include="$(RepoRoot)servers\Azure.Mcp.Server\src\Azure.Mcp.Server.csproj" />
    <ProjectReference Include="$(RepoRoot)core\Azure.Mcp.Core\tests\Azure.Mcp.Tests\Azure.Mcp.Tests.csproj" />
    <ProjectReference Include="..\..\src\Azure.Mcp.Tools.{Toolset}.csproj" />
  </ItemGroup>
</Project>
```

### Recorded Test Lifecycle and Helpers

Live tests must inherit from `RecordedCommandTestsBase` and follow [`/docs/recorded-tests.md`](https://github.com/microsoft/mcp/blob/main/docs/recorded-tests.md):

- **HTTP client recording:** Obtain service `HttpClient` instances through `IHttpClientFactory.CreateClient()`.
- **`LocalRequired = true` tools:** Start every test with `if (await AssertLocalToolIsUnavailableInHttpMode("{toolset}_{resource}_{operation}")) return;` — do not duplicate HTTP-mode exclusion checks.
- **Async teardown:** `RecordedCommandTestsBase` implements `IAsyncLifetime`. Always call `await base.InitializeAsync()` / `await base.DisposeAsync()` if overridden (calling `base.Dispose()` skips teardown and failure diagnostics).

### Recorded Test Pitfalls

- **Always pass `Settings.TenantId`:** Prevents `InvalidAuthenticationTokenTenant` when the test subscription is in a non-default tenant.
- **Wrap dynamic values in `RegisterOrRetrieveVariable`:**
  ```csharp
  var topicName = RegisterOrRetrieveVariable("create_topic_name", $"topic-{Guid.NewGuid():N}"[..24]);
  ```
- **Write sanitizer-resilient assertions:** Inspect recordings (`.proxy/Azure.Sdk.Tools.TestProxy.exe config locate -a <assets.json>`) after recording. Prefer `AssertProperty("name")`, `TryGetProperty(...)`, `JsonValueKind` checks, and unsanitized enum assertions (or use `SanitizeAndRecord` like `ManagedLustre` when exact name assertions are required).
- **Credential type in `.testsettings.json`:** `Deploy-TestResources.ps1` sets `AZURE_TOKEN_CREDENTIALS=AzurePowerShellCredential`. If the server subprocess cannot access that cache, switch `.testsettings.json`'s `EnvironmentVariables` to `"AZURE_TOKEN_CREDENTIALS": "AzureCliCredential"` with an active `az login --tenant <tenant-id>`.
- **12-hour resource group expiry:** If tests fail with `ResourceGroupNotFound`, redeploy via `./eng/scripts/Deploy-TestResources.ps1 -Paths "{Toolset}"`.

## Remote MCP Server Considerations

Commands must be **transport-agnostic** — they run identically in stdio (local) and HTTP (remote) modes across concurrent users.

### Outgoing Authentication Strategies

| Strategy | Flag | Behavior |
|---|---|---|
| **On-Behalf-Of (OBO)** | `--outgoing-auth-strategy UseOnBehalfOf` | Exchanges the caller's delegated Entra ID bearer token for downstream Azure tokens; enforces per-user RBAC and audit trails. Requires a user token (app-only tokens throw `OnBehalfOfUserTokenRequiredException`, mapped to `403`). |
| **Hosting Environment Identity** | `--outgoing-auth-strategy UseHostingEnvironmentIdentity` | Uses the server's Managed Identity / Service Principal; all callers share the server's RBAC permissions. |

`IAzureTokenCredentialProvider` and `IAzureService` handle both strategies and stdio automatically — no transport-specific code belongs in commands.

### Transport-Agnostic Design Rules

- **Stateless & thread-safe:** Inject singleton services via primary constructors; never store per-request `CommandContext` or `TOptions` in instance fields (concurrent HTTP requests will race).
- **No transport branching:** Never inspect `ASPNETCORE_URLS` or `_httpContextAccessor.HttpContext` inside commands.
- **Tenant propagation:** Pass `options.Tenant` through service APIs to `CreateArmClientAsync(tenant, cancellationToken: cancellationToken)` or `GetCredential(tenant, cancellationToken)` so `IAzureService` enforces tenant validation in OBO mode and resolves default/requested tenant in hosting/stdio modes.
- **Testing:** Unit tests mock services; recorded `RecordedCommandTestsBase` tests exercise transport-agnostic behavior, and `LocalRequired = true` tools verify HTTP exclusion via `AssertLocalToolIsUnavailableInHttpMode`.

## Consolidated Mode Requirements

Add new commands to the appropriate category in `servers/Azure.Mcp.Server/src/Resources/consolidated-tools.json` and update its description as needed (or propose a new category if none matches). Use `azmcp tools list --name --namespace <tool_area>` from `servers/Azure.Mcp.Server/src/bin/Debug/net10.0` to find the registered tool name.

## Code Quality, AOT, and Troubleshooting

### Implicit Usings and Unused Imports

`<ImplicitUsings>enable</ImplicitUsings>` is configured in `Directory.Build.props` for .NET 10, so `System`, `System.Collections.Generic`, `System.IO`, `System.Linq`, `System.Net.Http`, `System.Threading`, and `System.Threading.Tasks` are included automatically — do not add them manually or copy-paste unused `using` blocks across files.

Clean up unused imports and formatting before committing:

```powershell
dotnet format --include="tools/Azure.Mcp.Tools.{Toolset}/**/*.cs" --verbosity normal
```

### Native AOT Compatibility

All toolsets and dependencies are validated in CI by the **(Native AOT) Build module** gate (`./eng/scripts/Build-Local.ps1 -IncludeNative`).

- Do **not** modify or add exclusions to the `#if !BUILD_NATIVE` block in `servers/Azure.Mcp.Server/src/Program.cs`, and do **not** exclude arbitrary external packages in `Azure.Mcp.Server.csproj`.
- Follow [`docs/aot-compatibility.md`](https://github.com/microsoft/mcp/blob/main/docs/aot-compatibility.md): if AOT fails due to `Azure.ResourceManager.*` packages, note it in the PR and coordinate with the team; if it fails due to your own or external packages, make the library AOT-safe or choose an AOT-compatible alternative.

### Troubleshooting Common Issues

- **Missing package version / restore error:** Add `<PackageVersion>` to `Directory.Packages.props` before adding `<PackageReference>` to the `.csproj`.
- **`"SelfContainedPostScript is not supported if there is no test-resources-post.ps1"`:** Every Azure service toolset must include `tests/test-resources-post.ps1` even if it uses only the basic template.
- **`cannot convert from 'CancellationToken' to 'string'`:** Pass named parameter `.GetAsync(resourceName, cancellationToken: cancellationToken)`.
- **Missing SDK property (`CreationDate`, `Edition`, etc.) or `Operator '?' cannot be applied to 'AzureLocation'`:** Verify actual SDK names (`CreatedOn`, `EarliestRestoreOn`, `CurrentSku?.Name`) and use `Location.ToString()` (`AzureLocation` is a struct).
- **Missing ARM method (`.GetSqlServerAsync`):** Access via collections: `.GetSqlServers().GetAsync(name, cancellationToken: cancellationToken)`.
- **`MCP server process exited unexpectedly` / `azmcp.exe not found`:** Ensure the test `.csproj` has `<OutputType>Exe</OutputType>`, `<HasLiveTests>true</HasLiveTests>`, `<HasUnitTests>true</HasUnitTests>`, and references `$(RepoRoot)servers\Azure.Mcp.Server\src\Azure.Mcp.Server.csproj`.
- **Bicep / live deployment failures (`Resource not found`, `Permission denied`):** Validate with `az bicep build`, verify `Settings.ResourceBaseName` naming and `testApplicationOid` RBAC role GUID/scope, and inspect logs from `./eng/scripts/Deploy-TestResources.ps1 -Paths {Toolset}`.
- **JSON serialization or DI failure:** Register result/model types in `{Toolset}JsonContext.cs` (and expose `ResultTypeInfo`), register services/commands as singletons in `{Toolset}Setup.cs`, and register new toolsets alphabetically in `Program.cs` `RegisterAreas()`.
- **Base command errors:** Call `HandleException(context, ex)` (not `context.Response`), match the two-generic logger type `ILogger<Base{Resource}Command<TOptions, TResult>>`, and include `using Microsoft.Mcp.Core.Commands;` for `TrimAnnotations.CommandAnnotations`.

---

Before opening a PR, complete the [PR Readiness Checklist](https://github.com/microsoft/mcp/blob/main/.github/skills/add-azure-mcp-tools/SKILL.md#phase-7-pr-readiness-checklist).
