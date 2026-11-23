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
   - Commands inheriting from `SubscriptionCommand` handle `--subscription` resolution automatically via `ISubscriptionResolver`.
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

Options classes are flat POCOs with `[Option]` attributes. Registration and binding are handled automatically by `OptionBinder` — no manual `RegisterOptions` or `BindOptions` overrides needed.

### Option Attribute Conventions

- **Flat POCOs:** No class inheritance; implement `ISubscriptionOption` when the command accepts `--subscription` (note: `ISubscriptionOption` provides `Subscription` only — add `Tenant` separately if accepted).
- **Automatic binding:** `OptionBinder` registers and binds public writable properties marked with `[Option]` or `[OptionContainer<TContainer>]`; unattributed properties are ignored.
- **Name:** Derived from the property name in kebab-case (`LocalFilePath` → `--local-file-path`). Specify `Name` only when kebab-case produces the wrong name (e.g., `[Option(Name = "foobar", Description = "...")]` on `FooBar` gives `--foobar`).
- **Required vs. optional:** Driven by the `required` keyword (`RequiredMemberAttribute`), not nullability. Use `required` when callers must supply the option; use nullable types (`?`) for optional values when omission must be distinguishable from a default. Non-nullable value types (e.g., `int`) default to `0` and are valid without `required`.
- **Descriptions:** Always required (`[Option(Description = "...")]`). Use `OptionDescriptions.X` for shared options (`Subscription`, `Tenant`, `ResourceGroup`, `AuthMethod`), or a toolset `{Toolset}OptionDescriptions` constants class for options reused across commands.
- **Nested objects:** Use `[OptionContainer<TContainer>(Prefix = "prefix")]` on a complex property so attributed child properties become `--prefix-child-name` (e.g., prefix `hello` + child `World` → `--hello-world`).
- **Parameter naming:**
  - Always use `subscription` (never `subscriptionId`) so callers can pass either IDs or names resolved by `ISubscriptionResolver`.
  - Use `resourceGroup` (never `resourceGroupName`), singular resource nouns (`server` not `serverName`), and omit unnecessary `-name` suffixes (`--account`, `--container`) unless disambiguation is needed (e.g., `--subscription-name`).
  - Align parameter names and order with Azure SDK parameters and sibling commands.
- **Property ordering:** Command-specific options first, then `ResourceGroup`, `Subscription`, `Tenant`, `AuthMethod`.
- **No `RegisterOptions`/`BindOptions` or `OptionDefinitions`:** Validation belongs in `ValidateOptions(TOptions, ValidationResult)` — never `Command.Validators.Add`.

### Option Usage Patterns

**Pattern 1: Standard command with required and optional options**

```csharp
public class {Resource}{Operation}Options : ISubscriptionOption
{
    [Option(Description = "The name of the Azure Storage account.")]
    public required string Account { get; set; }

    [Option(Description = "The name of the container within the storage account.")]
    public required string Container { get; set; }

    [Option(Description = "Optional filter expression.")]
    public string? Filter { get; set; }

    [Option(Description = OptionDescriptions.ResourceGroup)]
    public string? ResourceGroup { get; set; }

    [Option(Description = OptionDescriptions.Subscription)]
    public string? Subscription { get; set; }

    [Option(Description = OptionDescriptions.Tenant)]
    public string? Tenant { get; set; }
}
```

**Pattern 2: Mutually exclusive options**

Make both options optional in the POCO and validate in `ValidateOptions`:

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

// In the command class:
public override void ValidateOptions(MyCommandOptions options, ValidationResult validationResult)
{
    base.ValidateOptions(options, validationResult);

    var hasEitherThis = !string.IsNullOrWhiteSpace(options.EitherThis);
    var hasOrThat = !string.IsNullOrWhiteSpace(options.OrThat);

    if (!hasEitherThis && !hasOrThat)
    {
        const string message = "Either --either-this or --or-that must be provided.";
        validationResult.AddError(message, message);
    }

    if (hasEitherThis && hasOrThat)
    {
        const string message = "Cannot specify both --either-this and --or-that. Use only one.";
        validationResult.AddError(message, message);
    }
}
```

**Pattern 3: Enum or constrained options**

```csharp
public class MyCommandOptions : ISubscriptionOption
{
    [Option(Description = "The output format.")]
    public required string Format { get; set; }

    [Option(Description = OptionDescriptions.Subscription)]
    public string? Subscription { get; set; }

    [Option(Description = OptionDescriptions.Tenant)]
    public string? Tenant { get; set; }
}

// In the command class:
public override void ValidateOptions(MyCommandOptions options, ValidationResult validationResult)
{
    base.ValidateOptions(options, validationResult);

    if (!new[] { "json", "table", "csv" }.Contains(options.Format, StringComparer.OrdinalIgnoreCase))
    {
        const string message = "--format must be one of: json, table, or csv.";
        validationResult.AddError(message, message);
    }
}
```

**Pattern 4: Interface constraints for shared base command behavior**

```csharp
public interface IStorageAccountOption
{
    string Account { get; }
}

public interface IContainerOption : IStorageAccountOption
{
    string Container { get; }
}

public class BlobUploadOptions : ISubscriptionOption, IContainerOption
{
    [Option(Description = "The name of the Azure Storage account.")]
    public required string Account { get; set; }

    [Option(Description = "The name of the container within the storage account.")]
    public required string Container { get; set; }

    [Option(Description = "The blob name/path within the container.")]
    public required string Blob { get; set; }

    [Option(Description = "The local file path to read content from.")]
    public required string LocalFilePath { get; set; }

    [Option(Description = OptionDescriptions.Subscription)]
    public string? Subscription { get; set; }

    [Option(Description = OptionDescriptions.Tenant)]
    public string? Tenant { get; set; }
}
```

**Pattern 5: List/query commands with optional filtering**

```csharp
public class StorageAccountListOptions : ISubscriptionOption
{
    [Option(Description = OptionDescriptions.ResourceGroup)]
    public string? ResourceGroup { get; set; }

    [Option(Description = OptionDescriptions.Subscription)]
    public string? Subscription { get; set; }

    [Option(Description = OptionDescriptions.Tenant)]
    public string? Tenant { get; set; }
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

### Command Class Pattern

```csharp
using System.Net;
using System.Text.Json.Serialization.Metadata;
using Azure.Mcp.Core.Commands.Subscription;       // For SubscriptionCommand<TOptions, TResult>
using Azure.Mcp.Core.Services.Azure.Subscription;  // For ISubscriptionResolver
using Azure.Mcp.Tools.{Toolset}.Models;
using Azure.Mcp.Tools.{Toolset}.Options;
using Azure.Mcp.Tools.{Toolset}.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Mcp.Core.Commands;
using Microsoft.Mcp.Core.Models.Command;

[CommandMetadata(
    Id = "<GUID>",
    Name = "operation",
    Title = "Human Readable Title",
    Description = """
        Detailed description of what the command does.
        Returns description of return format.
          Required options:
        - list required options
        """,
    OperationPlane = ToolOperationPlane.Control,
    Destructive = false,    // Set to true for tools that modify resources
    OpenWorld = false,      // Set to true only for unpredictable external systems
    Idempotent = true,      // Set to false for tools that are not idempotent
    ReadOnly = true,        // Set to false for tools that modify resources
    Secret = false,         // Set to true for tools that may return sensitive information
    LocalRequired = false)] // Set to true for tools requiring local execution/resources
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

    public override void ValidateOptions({Resource}{Operation}Options options, ValidationResult validationResult)
    {
        base.ValidateOptions(options, validationResult);  // checks --subscription
        // Add custom validation if needed using validationResult.AddError(...)
    }

    public override async Task<CommandResponse> ExecuteAsync(
        CommandContext context, {Resource}{Operation}Options options, CancellationToken cancellationToken)
    {
        try
        {
            context.Activity?.WithSubscriptionTag(options);

            var results = await _service.{Operation}(
                options.RequiredOption,
                options.OptionalOption,
                options.Subscription!,
                cancellationToken);

            // For enumerable returns, coalesce null into an empty enumerable.
            SetResult(context, new(results ?? []));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in {Operation}. Subscription: {Subscription}",
                Name, options.Subscription);
            HandleException(context, ex);
        }

        return context.Response;
    }

    public record {Resource}{Operation}CommandResult(List<ResultType> Results);
}
```

Key requirements:
- **Two-generic base:** `SubscriptionCommand<TOptions, TResult>` (or `BaseCommand<TOptions, TResult>` when no subscription is needed). Do not override `RegisterOptions`/`BindOptions`, take `ParseResult` in `ExecuteAsync`, or call `Validate()` manually.
- **Structured output:** Declare the result record `public` inside the command, override `ResultTypeInfo`, and call `SetResult(context, ...)` for success. When `--structured-output-mode` enables structured output, the same source-generated type supplies `outputSchema` and `structuredContent`; see [`docs/output-schema-migration.md`](https://github.com/microsoft/mcp/blob/main/docs/output-schema-migration.md).
- **Empty arrays:** Commands returning array payloads return `[]` if the service returned `null` or empty.
- **Error overrides:** Only override `GetErrorMessage` and `GetStatusCode` when behavior differs from the base class.

### Tool ID and CommandMetadata Properties

All `[CommandMetadata]` properties are required; declare every property explicitly even when using defaults, and choose `OperationPlane` to match the APIs the tool acts against.

- **`Id`:** A unique GUID generated per command to uniquely identify the tool across the system.
- **`OpenWorld`:** `false` for Azure Resource Manager, Microsoft Graph, and well-defined Microsoft APIs (including storage accounts, databases, VMs, Bicep schemas, best practices). Set `true` only for unpredictable external systems, arbitrary URLs, or unstructured third-party sources.
- **`Destructive`:** `true` if the command deletes, purges, resets, revokes, or modifies critical state/replaces resources; `false` for reads, queries, and non-destructive operations.
- **`Idempotent`:** `true` if repeated calls with identical inputs produce the same result or converge to the same state; `false` if repeated calls rotate keys, generate new names, or accumulate side effects.
- **`ReadOnly`:** `true` if the command only retrieves data; `false` if it creates, updates, deletes, writes, or uploads.
- **`Secret`:** `true` if the command returns access keys, connection strings, passwords, certificates, tokens, or credential metadata; `false` for public or non-sensitive configuration. **When in doubt, set `true`.**
- **`LocalRequired`:** `true` if the command requires local files, CLI tools (Azure CLI, Docker, npm), or locally installed software; `false` for pure cloud API commands.

### Metadata Validation Checklist

Cross-check `[CommandMetadata]` values before proceeding:

- **`Destructive`:** `true` if name contains `delete`, `remove`, `purge`, `reset`, `revoke`, or `update`, or replaces existing resources; `false` if `list`, `get`, `show`, `query`, or `describe`.
- **`Idempotent`:** `false` if repeated calls produce different outputs, rotate secrets, or auto-generate names; `true` if outputs or target state are repeatable.
- **`OpenWorld`:** `false` for Azure Resource Manager, Microsoft Graph, and well-defined Microsoft APIs; `true` only for user-controlled/external unpredictable systems.
- **`ReadOnly`:** `false` for create/update/delete/write/upload; `true` for retrieval-only commands (usually the inverse of `Destructive`, except both are `false` for creates).
- **`Secret`:** Default to `true` if the command/resource mentions `credential`, `secret`, `key`, `password`, `certificate`, `token`, or `connectionstring`, unless it provably cannot expose sensitive data.
- **`LocalRequired`:** `false` for remote API-only commands; `true` only when local filesystem/CLI/software is required.

### JSON Serialization Context

All models and command result record types returned in `Response.Results` must be registered in a source-generated JSON context for AOT safety and performance (`src/Commands/{Toolset}JsonContext.cs`, matching class name):

```csharp
using System.Text.Json.Serialization;
using Azure.Mcp.Tools.{Toolset}.Commands.{Resource};
using Azure.Mcp.Tools.{Toolset}.Models;

[JsonSerializable(typeof({Resource}{Operation}Command.{Resource}{Operation}CommandResult))]
[JsonSerializable(typeof(YourModelType))]
[JsonSourceGenerationOptions(
    PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase,
    DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
)]
internal partial class {Toolset}JsonContext : JsonSerializerContext;
```

Guidelines:
- Include only types actually serialized as top-level result payloads, sorted alphabetically by `typeof` model name.
- Use one context per toolset unless size forces logical grouping.
- Expose `{Toolset}JsonContext.Default.{Resource}{Operation}CommandResult` via `ResultTypeInfo` and assign via `SetResult(context, ...)`.

### Toolset Registration Reference

Register the new toolset alphabetically in `servers/Azure.Mcp.Server/src/Program.cs`:

```csharp
private static IAreaSetup[] RegisterAreas()
{
    return [
        // Core toolsets (alphabetical)
        new Azure.Mcp.Tools.AzureBestPractices.AzureBestPracticesSetup(),
        new Azure.Mcp.Tools.Extension.ExtensionSetup(),

        // Azure service toolsets (alphabetical)
        new Azure.Mcp.Tools.{Toolset}.{Toolset}Setup(),
        new Azure.Mcp.Tools.Storage.StorageSetup(),
    ];
}
```

Keep `RegisterAreas()` alphabetically sorted (do **not** modify the `#if !BUILD_NATIVE` block; see [Native AOT Compatibility](https://github.com/microsoft/mcp/blob/main/servers/Azure.Mcp.Server/docs/new-command.md#native-aot-compatibility)).

## Error Handling

Commands in Azure MCP follow a standardized error handling approach using the base `HandleException` method inherited from `BaseCommand`. Here are the key aspects:

### 1. Status Code Mapping
The base implementation returns InternalServerError for all exceptions by default:
```csharp
protected virtual HttpStatusCode GetStatusCode(Exception ex) => HttpStatusCode.InternalServerError;
```

Commands should override this to provide appropriate status codes:
```csharp
protected override HttpStatusCode GetStatusCode(Exception ex) => ex switch
{
    Azure.RequestFailedException reqEx => (HttpStatusCode)reqEx.Status,  // Use Azure-reported status
    Azure.Identity.AuthenticationFailedException => HttpStatusCode.Unauthorized,   // Unauthorized
    ValidationException => HttpStatusCode.BadRequest,    // Bad request
    _ => base.GetStatusCode(ex) // Fall back to InternalServerError
};
```

### 2. Error Message Formatting
The base implementation returns the exception message:
```csharp
protected virtual string GetErrorMessage(Exception ex) => ex.Message;
```

Commands should override this to provide user-actionable messages:
```csharp
protected override string GetErrorMessage(Exception ex) => ex switch
{
    Azure.Identity.AuthenticationFailedException authEx =>
        $"Authentication failed. Please run 'az login' to sign in. Details: {authEx.Message}",
    Azure.RequestFailedException reqEx when reqEx.Status == (int)HttpStatusCode.NotFound =>
        "Resource not found. Verify the resource name and that you have access.",
    Azure.RequestFailedException reqEx when reqEx.Status == (int)HttpStatusCode.Forbidden =>
        $"Access denied. Ensure you have appropriate RBAC permissions. Details: {reqEx.Message}",
    Azure.RequestFailedException reqEx => reqEx.Message,
    _ => base.GetErrorMessage(ex)
};
```

### 3. Response Format
The base `HandleException` method in BaseCommand handles the response formatting:
```csharp
protected virtual void HandleException(CommandContext context, Exception ex)
{
    context.Activity?.SetStatus(ActivityStatusCode.Error);

    var response = context.Response;
    var result = new ExceptionResult(
        Message: ex.Message,
        StackTrace: ex.StackTrace,
        Type: ex.GetType().Name);

    response.Status = GetStatusCode(ex);
    response.Message = GetErrorMessage(ex) + ". To mitigate this issue, please refer to the troubleshooting guidelines here at https://aka.ms/azmcp/troubleshooting.";
    response.Results = ResponseResult.Create(result, JsonSourceGenerationContext.Default.ExceptionResult);
}
```

Commands should call `HandleException(context, ex)` in their catch blocks.

### 4. Service-Specific Errors
Commands should override error handlers to add service-specific mappings:
```csharp
protected override string GetErrorMessage(Exception ex) => ex switch
{
    // Add service-specific cases
    ResourceNotFoundException =>
        "Resource not found. Verify name and permissions.",
    ServiceQuotaExceededException =>
        "Service quota exceeded. Request quota increase.",
    _ => base.GetErrorMessage(ex) // Fall back to base implementation
};
```

### 5. Error Context Logging
Always log errors with relevant context information:
```csharp
catch (Exception ex)
{
    _logger.LogError(ex, "Error in {Operation}. Subscription: {Subscription}", Name, options.Subscription);
    HandleException(context, ex);
}
```

**DO NOT** log `{@Options}` as this may log sensitive information. Only log parameters that are known to be safe.

### 6. Common Error Scenarios to Handle

1. **Authentication/Authorization**
   - Azure credential expiry
   - Missing RBAC permissions
   - Invalid connection strings

2. **Validation**
   - Missing required parameters
   - Invalid parameter formats
   - Conflicting options

3. **Resource State**
   - Resource not found
   - Resource locked/in use
   - Invalid resource state

4. **Service Limits**
   - Throttling/rate limits
   - Quota exceeded
   - Service capacity

5. **Network/Connectivity**
   - Service unavailable
   - Request timeouts
   - Network failures

## Azure Service Integration and Sovereign Clouds

### Package Management and Base Class Selection

**Package Management:**
- **Resource Graph reads:** No extra Azure SDK package needed (`Azure.ResourceManager.ResourceGraph` is in core). Add toolset-specific `Azure.ResourceManager.*` packages only for specialized ARM reads or writes.
- **Add to `Directory.Packages.props` first** (`<PackageVersion Include="Azure.ResourceManager.{Service}" Version="..." />`), then add `<PackageReference Include="Azure.ResourceManager.{Service}" />` (without version) in `Azure.Mcp.Tools.{Toolset}.csproj`. Keep versions consistent across projects.

**Service Base Class Selection:**
1. **Azure Resource Graph (ARG) queries (`BaseAzureResourceService`):** Provides `ExecuteResourceQueryAsync<T>()` and `ExecuteSingleResourceQueryAsync<T>()` with subscription resolution and tenant lookup.
   ```csharp
   public class MyService(IAzureService azureService)
       : BaseAzureResourceService(azureService), IMyService
   {
       public async Task<ResourceQueryResults<MyResource>> ListResourcesAsync(
           string resourceGroup,
           string subscription,
           string? tenant = null,
           CancellationToken cancellationToken = default)
       {
           return await ExecuteResourceQueryAsync(
               "Microsoft.MyService/resources",
               resourceGroup,
               subscription,
               null,
               ConvertToMyResourceModel,
               tenant: tenant,
               cancellationToken: cancellationToken);
       }

       public async Task<MyResource?> GetResourceAsync(
           string resourceName,
           string resourceGroup,
           string subscription,
           string? tenant = null,
           CancellationToken cancellationToken = default)
       {
           return await ExecuteSingleResourceQueryAsync(
               "Microsoft.MyService/resources",
               resourceGroup,
               subscription,
               null,
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
            _ => $"https://{resourceName}.service.core.windows.net"
        };
}
```

| Scenario | Requirement |
|---|---|
| Resource Graph or ARM via `BaseAzureResourceService` / `CreateArmClientWithApiVersionAsync` | ✅ Cloud-aware automatically |
| Data-plane SDK requiring an explicit URL | ✅ `AzureService.CloudConfiguration.CloudType` switch + `ValidateAzureServiceEndpoint` |
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

- **Base class:** Inherit `SubscriptionCommandUnitTestsBase<TCommand, TService>` for subscription commands (registers a mock `ISubscriptionResolver` in DI — using `CommandUnitTestsBase` causes DI failures), or `CommandUnitTestsBase<TCommand, TService>` for non-subscription commands.
- **Argument-based execution:** Prefer `ExecuteCommandAsync("--param", "value")` over constructing options directly so `[Option]` registration, `OptionBinder`, `ValidateOptions`, and `ISubscriptionResolver` run end-to-end.
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

Commands must be **transport-agnostic** — they work identically in stdio (local) and HTTP (remote) modes with multiple concurrent users.

### Authentication Strategies

Azure MCP Server supports two outgoing authentication strategies in remote HTTP mode:

1. **On-Behalf-Of (OBO) Flow (`--outgoing-auth-strategy UseOnBehalfOf`):** Client sends an Entra ID bearer token; the server exchanges it for downstream Azure tokens so each API call enforces the user's RBAC permissions and records user identity in audit logs. Test with multiple users having different RBAC levels.
2. **Hosting Environment Identity (`--outgoing-auth-strategy UseHostingEnvironmentIdentity`):** Server uses its own identity (Managed Identity / Service Principal), sharing one RBAC permission level across all callers (behaves like `DefaultAzureCredential` in stdio mode).

No command-level changes are needed for either strategy; `IAzureTokenCredentialProvider` / `IAzureService` handles credential acquisition automatically across all modes.

### Transport-Agnostic Design and Concurrency

- **Stateless & thread-safe:** Inject singleton services via primary constructors; never store per-request `CommandContext` or `TOptions` in instance fields (concurrent HTTP requests will race).
- **No transport branching:** Never inspect `ASPNETCORE_URLS` or `_httpContextAccessor.HttpContext` inside commands.
- **Tenant & credential handling:** Pass `options.Tenant` through service APIs to `CreateArmClientAsync(tenant, cancellationToken: cancellationToken)` or `GetCredential(tenant, cancellationToken)` (`IAzureService` validates tenant match in OBO mode and resolves default/requested tenant in hosting/stdio modes).

```csharp
public sealed class StorageAccountGetCommand(
    IStorageService storageService,
    ILogger<StorageAccountGetCommand> logger,
    ISubscriptionResolver subscriptionResolver)
    : SubscriptionCommand<StorageAccountGetOptions, StorageAccountGetCommand.StorageAccountGetResult>(subscriptionResolver)
{
    private readonly IStorageService _storageService = storageService;
    private readonly ILogger<StorageAccountGetCommand> _logger = logger;

    public override JsonTypeInfo<StorageAccountGetResult>? ResultTypeInfo =>
        StorageJsonContext.Default.StorageAccountGetResult;

    public override async Task<CommandResponse> ExecuteAsync(
        CommandContext context,
        StorageAccountGetOptions options,
        CancellationToken cancellationToken)
    {
        var accounts = await _storageService.GetStorageAccountsAsync(
            options.Subscription!,
            options.ResourceGroup,
            options.Tenant,
            cancellationToken);

        SetResult(context, new(accounts ?? []));
        return context.Response;
    }

    public record StorageAccountGetResult(List<StorageAccount> Accounts);
}
```

### Error Handling and Documentation for Remote Scenarios

Override `GetErrorMessage` when remote auth/tenant errors benefit from clearer caller guidance (without exposing secrets):

```csharp
protected override string GetErrorMessage(Exception ex) => ex switch
{
    RequestFailedException reqEx when reqEx.Status == 401 =>
        "Authentication failed. In remote mode, ensure your token has the required " +
        "Mcp.Tools.ReadWrite scope and sufficient RBAC permissions on Azure resources.",
    RequestFailedException reqEx when reqEx.Status == 403 =>
        "Authorization failed. Your user account lacks the required RBAC permissions. " +
        "In remote mode with On Behalf Of flow, permissions come from the authenticated user's identity. Learn more at https://learn.microsoft.com/entra/identity-platform/v2-oauth2-on-behalf-of-flow",
    InvalidOperationException invEx when invEx.Message.Contains("tenant") =>
        "Tenant mismatch. In remote OBO mode, the requested tenant must match your " +
        "authenticated user's tenant ID.",
    _ => base.GetErrorMessage(ex)
};
```

- **Testing remote behavior:** Unit tests mock services; recorded `RecordedCommandTestsBase` tests inherently exercise transport-agnostic behavior, and `LocalRequired = true` tools verify HTTP exclusion via `AssertLocalToolIsUnavailableInHttpMode`.
- **Permissions docs:** When authentication behavior differs by outgoing identity strategy, document a `### Permissions` section in `azmcp-commands.md`:
  ```markdown
  ### Permissions

  **Stdio Mode:** Requires authenticated Azure identity (Azure CLI, VS Code, Managed Identity) using local RBAC permissions.
  **Remote HTTP Mode (OBO):** Requires `Mcp.Tools.ReadWrite` scope + authenticated user's RBAC permissions.
  **Remote HTTP Mode (Hosting Environment):** Requires `Mcp.Tools.ReadWrite` scope + server's Managed Identity RBAC permissions.
  ```

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

#### Project Setup and Integration
- **Missing package references:** Always add `<PackageVersion>` to `Directory.Packages.props` before adding `<PackageReference>` to the `.csproj`.
- **Missing `test-resources-post.ps1` (`"SelfContainedPostScript is not supported..."`):** Every Azure service toolset must include `tests/test-resources-post.ps1` even if it uses only the basic template.
- **Missing test imports:** Include `using System.Text.Json;`, `using Xunit;`, `using NSubstitute;`, and `using Azure.Mcp.Tests;`.

#### ARM Compilation Errors
- **Wrong subscription resolution:** Inject `IAzureService` and call `AzureService.GetSubscription(subscription, tenant, cancellationToken: cancellationToken)` instead of constructing `ArmClient` manually.
- **`cannot convert from 'CancellationToken' to 'string'`:** Pass named parameter `.GetAsync(resourceName, cancellationToken: cancellationToken)`.
- **Missing SDK properties (`CreationDate`, `Edition`, etc.):** Verify actual SDK names (`CreatedOn`, `EarliestRestoreOn`, `CurrentSku?.Name`).
- **`Operator '?' cannot be applied to operand of type 'AzureLocation'`:** `AzureLocation` is a struct — use `Location.ToString()`.
- **Wrong resource access method (`.GetSqlServerAsync`):** Always access via collections: `.GetSqlServers().GetAsync(name, cancellationToken: cancellationToken)`.

#### Live Test Infrastructure & Project Configuration
- **Bicep validation / deployment failures:** Run `az bicep build --file tools/Azure.Mcp.Tools.{Toolset}/tests/test-resources.bicep`, check `@minLength`/`@maxLength`, supported API versions/locations, and re-run `./eng/scripts/Deploy-TestResources.ps1 -Paths {Toolset}` to inspect deployment logs.
- **`Resource not found` / `Permission denied`:** Verify `Settings.ResourceBaseName` naming and ensure `testApplicationOid` has the right role assignment GUID on the resource scope.
- **`MCP server process exited unexpectedly` / `azmcp.exe not found`:** Ensure the test `.csproj` has `<OutputType>Exe</OutputType>`, `<HasLiveTests>true</HasLiveTests>`, `<HasUnitTests>true</HasUnitTests>`, and references `$(RepoRoot)servers\Azure.Mcp.Server\src\Azure.Mcp.Server.csproj`.

#### Service & Base Command Issues
- **JSON serialization failure:** Register every top-level result record and model type in `{Toolset}JsonContext.cs` and expose `ResultTypeInfo`.
- **Toolset not discovered / DI errors:** Verify services and commands are registered as singletons in `{Toolset}Setup.cs` and the setup is in `Program.cs` `RegisterAreas()` alphabetically.
- **`HandleException` signature:** Call `HandleException(context, ex)`, not `HandleException(context.Response, ex)`.
- **Missing subscription telemetry tag:** Call `context.Activity?.WithSubscriptionTag(options);`.
- **Wrong base command logger generic / `TrimAnnotations`:** Match the two-generic logger type `ILogger<Base{Resource}Command<TOptions, TResult>>`, and include `using Microsoft.Mcp.Core.Commands;` for `TrimAnnotations.CommandAnnotations`.

---

Before opening a PR, complete the [PR Readiness Checklist](https://github.com/microsoft/mcp/blob/main/.github/skills/add-azure-mcp-tools/SKILL.md#phase-7-pr-readiness-checklist).
