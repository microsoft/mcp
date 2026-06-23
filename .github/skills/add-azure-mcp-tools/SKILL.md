---
name: add-azure-mcp-tools
description: 'Add a new tool/command to any Azure MCP toolset. Full lifecycle from scaffolding through PR submission. USE WHEN: add new command, create tool, new MCP tool, scaffold command, implement operation, add azure service tool, create new toolset.'
argument-hint: 'Describe the new tool (e.g., "add storage container delete command" or "create new KeyVault toolset with secret get command")'
---

# Add a New Tool to Azure MCP

## Purpose

Step-by-step workflow for adding a new command to any Azure MCP toolset.
Each phase has an explicit **gate** — do not proceed until the gate passes.

## Decision: New Toolset or Existing?

Before starting, determine:

- **Adding to existing toolset?** → Skip to Phase 1.
- **Creating a new toolset?** → Complete Phase 0 first.

**⚠️ CRITICAL: Does your command interact with Azure resources?**

| | Azure Service Commands | Non-Azure Commands |
|---|---|---|
| **Examples** | ACR Registry List, SQL Database List, Storage Account Get | CLI wrappers, Best Practices, Documentation tools |
| **test-resources.bicep** | ✅ Required | ❌ Skip |
| **test-resources-post.ps1** | ✅ Required (even if basic) | ❌ Skip |
| **RBAC role assignments** | ✅ Required | ❌ Skip |
| **Live tests** | ✅ Required (recorded) | ❌ Skip |
| **Unit tests** | ✅ Required | ✅ Required |

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

1. Add package version to `Directory.Packages.props` (if Azure SDK needed)
 2. Register the project in solution files by running:
    `pwsh eng/scripts/Update-Solutions.ps1 -All`
3. Register the new toolset in `servers/Azure.Mcp.Server/src/Program.cs` `RegisterAreas()` (alphabetical order)
 4. Choose the appropriate base class:
    - **Commands that need an Azure subscription** (most Azure service tools) → inherit from `SubscriptionCommand<TOptions, TResult>` and inject `ISubscriptionResolver`.
    - **Commands that do NOT need a subscription** (CLI wrappers, documentation tools, best-practice advisors) → inherit from `BaseCommand<TOptions, TResult>` directly.
 
    Only add a shared intermediate base command if you have real cross-command logic shared by multiple commands in the same toolset.
5. Register both the service **and the command** as singletons in `{Toolset}Setup.cs` `ConfigureServices`:
   ```csharp
   public void ConfigureServices(IServiceCollection services)
   {
       services.AddSingleton<I{Toolset}Service, {Toolset}Service>();
       services.AddSingleton<{Resource}{Operation}Command>();
   }
   ```

**GATE:** `dotnet build servers/Azure.Mcp.Server/src` must pass.

---

## Phase 1: Implement Command

Create these files in order:

### 1a. Options Class (with `[Option]` attributes)

File: `src/Options/{Resource}/{Resource}{Operation}Options.cs`

```csharp
using Azure.Mcp.Core.Options;
using Microsoft.Mcp.Core.Models;
using Microsoft.Mcp.Core.Options;

namespace Azure.Mcp.Tools.{Toolset}.Options.{Resource};

public class {Resource}{Operation}Options : ISubscriptionOption
{
    [Option("Description of what this option does (e.g., 'The name of the resource').")]
    public string? MyOption { get; set; }

    [Option(OptionDescriptions.ResourceGroup)]
    public string? ResourceGroup { get; set; }

    [Option(OptionDescriptions.Subscription)]
    public string? Subscription { get; set; }

    [Option(OptionDescriptions.Tenant)]
    public string? Tenant { get; set; }

    [Option(Name = "retry")]
    public RetryPolicyOptions? RetryPolicy { get; set; }
}
```

Rules:
- Implement `ISubscriptionOption` for commands that need subscription resolution
- Use `[Option("description")]` for the description — property name auto-converts to `--kebab-case`
- Use `[Option(Name = "custom")]` only when the default kebab-case conversion is wrong (e.g., `RetryPolicy` → `--retry` not `--retry-policy`)
- Use `[Option(OptionDescriptions.X)]` for shared descriptions (`Subscription`, `Tenant`, `ResourceGroup`, `AuthMethod`)
- Use `subscription` (never `subscriptionId`) — supports both IDs and names
- Use `resourceGroup` (never `resourceGroupName`)
- Use singular nouns for resources (`server` not `serverName`)
- Remove unnecessary `name` suffixes (`Account` / `--account` not `AccountName` / `--account-name`)
- Use `required` on required options; use nullable types (`?`) for optional options.
- Non-nullable value types (e.g., `public int Count { get; set; }`) are always valid without `required` — they default to `0`. Use `required` if the caller must explicitly provide a value, or use `int?` if the parameter should be truly optional.
- Order: command-specific options first, then `ResourceGroup`, `Subscription`, `Tenant`, `AuthMethod`, `RetryPolicy`
- Keep parameter names consistent with Azure SDK parameters when possible

 > **Note:** Options are defined entirely via `[Option]` attributes.
 > A static `{Toolset}OptionDefinitions` class is not needed

### 1c. Service Interface and Implementation

File: `src/Services/I{Toolset}Service.cs`

Return type depends on operation type:
- **Resource Graph queries** → `Task<ResourceQueryResults<MyModel>>` (includes `AreResultsTruncated` flag)
- **Data plane operations** → `Task<List<MyModel>>`
- **Write operations** → `Task<MyResultModel>`

```csharp
public interface I{Toolset}Service
{
    // Resource Graph read operation
    Task<ResourceQueryResults<MyModel>> GetResourcesAsync(
        string? myOption,
        string subscription,
        string? resourceGroup = null,
        string? tenant = null,
        RetryPolicyOptions? retryPolicy = null,
        CancellationToken cancellationToken = default);

    // Data plane operation (returns simple List)
    Task<List<MyDetail>> GetDetailsAsync(
        string resourceName,
        string subscription,
        string? tenant = null,
        RetryPolicyOptions? retryPolicy = null,
        CancellationToken cancellationToken = default);
}
```

File: `src/Services/{Toolset}Service.cs`

Choose base class:
 - **Operations that need Resource Graph (ARG) queries:** inherit `BaseAzureResourceService`
 - **All other operations (ARM, data plane):** inherit `BaseAzureService`
 
 > `BaseAzureResourceService` extends `BaseAzureService` — neither is
 > inherently read-only or write-only. The distinction is whether you
 > need ARG querying functionality.

```csharp
public class {Toolset}Service(ISubscriptionService subscriptionService, ITenantService tenantService)
    : BaseAzureResourceService(subscriptionService, tenantService), I{Toolset}Service
{
    public async Task<ResourceQueryResults<MyModel>> GetResourcesAsync(
        string? myOption,
        string subscription,
        string? resourceGroup = null,
        string? tenant = null,
        RetryPolicyOptions? retryPolicy = null,
        CancellationToken cancellationToken = default)
    {
        return await ExecuteResourceQueryAsync(
            "Microsoft.{Provider}/{resourceType}",
            resourceGroup,
            subscription,
            retryPolicy,
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
            Tags: data.Tags as IReadOnlyDictionary<string, string>
        );
    }
}
```

For **write operations** (using direct ARM clients):
```csharp
public class {Toolset}Service(ISubscriptionService subscriptionService, ITenantService tenantService)
    : BaseAzureService(tenantService), I{Toolset}Service
{
    private readonly ISubscriptionService _subscriptionService = subscriptionService
        ?? throw new ArgumentNullException(nameof(subscriptionService));

    public async Task<MyResource> CreateResourceAsync(
        string resourceName,
        string resourceGroup,
        string subscription,
        string? tenant = null,
        RetryPolicyOptions? retryPolicy = null,
        CancellationToken cancellationToken = default)
    {
        var subscriptionResource = await _subscriptionService.GetSubscription(subscription, tenant, retryPolicy);

        // CRITICAL: Use GetResourceGroupAsync with await
        var rgResource = await subscriptionResource.GetResourceGroupAsync(resourceGroup, cancellationToken);
        var resource = await rgResource.Value
            .GetMyResources()
            .GetAsync(resourceName, cancellationToken: cancellationToken);
        return resource.Value;
    }
}
```

Sovereign cloud rules:
- ARM/Resource Graph operations: cloud-aware automatically, no extra work
- Data plane endpoints: use `TenantService.CloudConfiguration.CloudType` switch — never hardcode URLs

**Data plane endpoint pattern** (required for services like Storage, Cosmos, Search):

```csharp
public class MyService(ISubscriptionService subscriptionService, ITenantService tenantService)
    : BaseAzureResourceService(subscriptionService, tenantService), IMyService
{

    private async Task<MyDataPlaneClient> CreateDataPlaneClientAsync(
        string resourceName,
        string? tenant = null,
        RetryPolicyOptions? retryPolicy = null,
        CancellationToken cancellationToken = default)
    {
        var endpoint = GetResourceEndpoint(resourceName);
        var options = ConfigureRetryPolicy(AddDefaultPolicies(new MyClientOptions()), retryPolicy);
        options.Transport = new HttpClientTransport(TenantService.GetClient());
        return new MyDataPlaneClient(
            new Uri(endpoint),
            await GetCredential(tenant, cancellationToken),
            options);
    }

    private string GetResourceEndpoint(string resourceName)
    {
        return TenantService.CloudConfiguration.CloudType switch
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
}
```

Patterns and anti-patterns:
```csharp
// ❌ Hardcoded public-cloud endpoint
var client = new BlobServiceClient(new($"https://{account}.blob.core.windows.net"), credential, options);

// ❌ Hardcoded connection string
var connectionString = $"AccountEndpoint=https://{server}.documents.azure.com:443/;...";

// ✅ Cloud-aware endpoint via switch expression
var endpoint = GetBlobEndpoint(account);
var client = new BlobServiceClient(new(endpoint), credential, options);
```

Reference implementations: `StorageService`, `CosmosService`, `SearchService`, `ConfidentialLedgerService`.

### 1d. Command Class

File: `src/Commands/{Resource}/{Resource}{Operation}Command.cs`

**Required using statements:**
```csharp
using Azure.Mcp.Core.Commands.Subscription;
using Azure.Mcp.Core.Services.Azure.Subscription;
using Azure.Mcp.Tools.{Toolset}.Models;
using Azure.Mcp.Tools.{Toolset}.Options.{Resource};
using Azure.Mcp.Tools.{Toolset}.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Mcp.Core.Commands;
using Microsoft.Mcp.Core.Models.Command;
```

```csharp
[CommandMetadata(
    Id = "<generate-new-guid>",
    Name = "operation",
    Title = "Human Readable Title",
    Description = """
        What this command does. Include required options and return format.
        """,
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
                options.RetryPolicy,
                cancellationToken);

            context.Response.Results = ResponseResult.Create(
                new {Resource}{Operation}CommandResult(results?.Results ?? [], results?.AreResultsTruncated ?? false),
                {Toolset}JsonContext.Default.{Resource}{Operation}CommandResult);
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

**Key points (two-generic pattern from `docs/option-conversion.md`):**
- Two generic parameters: `SubscriptionCommand<TOptions, TResult>` — `TResult` is the command's result record
- `ISubscriptionResolver` injected via primary constructor and passed to base
- `ExecuteAsync` receives **pre-bound `TOptions options`** — no `ParseResult` parameter
- No `RegisterOptions()`/`BindOptions()` overrides needed — `OptionBinder` handles binding via `[Option]` attributes
- No manual `Validate()` call — framework validates based on nullability and `ValidateOptions()` override
- Result record is `public` (for JSON serialization context visibility) and declared inside the command class
- **DO NOT** log `{@Options}` — may expose sensitive information

**Custom validation** (if needed beyond nullability checks):
```csharp
public override void ValidateOptions({Resource}{Operation}Options options, ValidationResult validationResult)
{
    base.ValidateOptions(options, validationResult);  // checks --subscription

    if (string.IsNullOrEmpty(options.MyRequiredField))
    {
        validationResult.Errors.Add("--my-required-field is required.");
    }
}
```

**Intermediate base commands** (only if you have shared cross-command logic):
```csharp
// Use interface constraints for type-safe access to shared options
public abstract class Base{Toolset}Command<
    [DynamicallyAccessedMembers(TrimAnnotations.CommandAnnotations)] TOptions, TResult>(
    ISubscriptionResolver subscriptionResolver)
    : SubscriptionCommand<TOptions, TResult>(subscriptionResolver)
    where TOptions : class, ISubscriptionOption, I{Toolset}Option
{
    public override void ValidateOptions(TOptions options, ValidationResult validationResult)
    {
        base.ValidateOptions(options, validationResult);
        // Shared validation using options.SharedProperty
    }
}
```

### 1e. JSON Serialization Context

File: `src/Commands/{Toolset}JsonContext.cs`

```csharp
[JsonSerializable(typeof({Resource}{Operation}Command.{Resource}{Operation}CommandResult))]
[JsonSerializable(typeof(MyModel))]
[JsonSourceGenerationOptions(
    PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase,
    DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull)]
internal partial class {Toolset}JsonContext : JsonSerializerContext;
```

Guidelines:
- Only include types actually serialized as top-level result payloads
- Keep `[JsonSerializable]` attributes sorted by `typeof` model name
- Use one context per toolset always
- Filename must match class name (`{Toolset}JsonContext.cs`)
- Use `{Toolset}JsonContext.Default.{CommandResult}` when serializing — never `JsonSerializer.Deserialize<T>()` without a context

### 1f. Register Command

File: `src/{Toolset}Setup.cs`

```csharp
public class {Toolset}Setup : IAreaSetup
{
    public string Name => "{toolset}";

    public string Title => "Manage Azure {Toolset}";

    public void ConfigureServices(IServiceCollection services)
    {
        services.AddSingleton<I{Toolset}Service, {Toolset}Service>();

        // Register all commands as singletons
        services.AddSingleton<{Resource}{Operation}Command>();
    }

    public CommandGroup RegisterCommands(IServiceProvider serviceProvider)
    {
        var root = new CommandGroup(Name,
            """
            {Toolset} operations - description of what this toolset covers.
            """,
            Title);

        var resource = new CommandGroup("{resource}", "{Resource} operations description");
        root.AddSubGroup(resource);
        resource.AddCommand<{Resource}{Operation}Command>(serviceProvider);

        return root;
    }
}
```

Also register the toolset in `servers/Azure.Mcp.Server/src/Program.cs`:
```csharp
private static IAreaSetup[] RegisterAreas()
{
    return [
        // ... existing toolsets (alphabetical order) ...
        new Azure.Mcp.Tools.{Toolset}.{Toolset}Setup(),
        // ... more toolsets ...
    ];
}
```

The `RegisterAreas()` list **must remain alphabetically sorted** (excluding the `#if !BUILD_NATIVE` block).

Command group naming: concatenated lowercase or dash-separated. Never underscores.
- ✅ Good: `"entraadmin"`, `"resourcegroup"`, `"storageaccount"`, `"entra-admin"`
- ❌ Bad: `"entra_admin"`, `"resource_group"`, `"storage_account"`

Command hierarchy patterns and anti-patterns:
- ✅ Good: `azmcp postgres server param set` (command groups: server → param, operation: set)
- ❌ Bad: `azmcp postgres server setparam` (mixed operation `setparam` at same level)
- ✅ Good: `azmcp storage blob upload permission set`
- ❌ Bad: `azmcp storage blobupload`

This pattern improves discoverability and allows grouping related operations.

**GATE:** `dotnet build tools/Azure.Mcp.Tools.{Toolset}/src` must pass with 0 errors.

---

## Phase 2: Unit Tests

File: `tests/Azure.Mcp.Tools.{Toolset}.Tests/{Resource}/{Resource}{Operation}CommandTests.cs`

### Required test methods

```csharp
using System.Net;
using Azure.Mcp.Core.Services.Azure;
using Azure.Mcp.Tests.Commands;
using Azure.Mcp.Tools.{Toolset}.Commands;
using Azure.Mcp.Tools.{Toolset}.Commands.{Resource};
using Azure.Mcp.Tools.{Toolset}.Models;
using Azure.Mcp.Tools.{Toolset}.Services;
using Microsoft.Mcp.Core.Options;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Xunit;

namespace Azure.Mcp.Tools.{Toolset}.Tests.{Resource};

public class {Resource}{Operation}CommandTests
    : SubscriptionCommandUnitTestsBase<{Resource}{Operation}Command, I{Toolset}Service>
{
    [Fact]
    public void Constructor_InitializesCommandCorrectly()
    {
        var command = Command.GetCommand();
        Assert.Equal("operation", command.Name);
        Assert.NotNull(command.Description);
        Assert.NotEmpty(command.Description);
    }

    [Theory]
    [InlineData("--my-option val --subscription sub123", true)]
    [InlineData("--subscription sub123", true)]  // my-option is optional
    [InlineData("", false)]  // missing args
    public async Task ExecuteAsync_ValidatesInputCorrectly(string args, bool shouldSucceed)
    {
        if (shouldSucceed)
        {
            Service.GetResourcesAsync(
                Arg.Any<string?>(), Arg.Any<string>(), Arg.Any<string?>(),
                Arg.Any<string?>(), Arg.Any<RetryPolicyOptions?>(),
                Arg.Any<CancellationToken>())
                .Returns(new ResourceQueryResults<MyModel>([], false));
        }

        var response = await ExecuteCommandAsync(args);

        Assert.Equal(shouldSucceed ? HttpStatusCode.OK : HttpStatusCode.BadRequest, response.Status);
        if (!shouldSucceed)
            Assert.Contains("required", response.Message.ToLower());
    }

    [Fact]
    public async Task ExecuteAsync_DeserializationValidation()
    {
        Service.GetResourcesAsync(
            Arg.Any<string?>(), Arg.Any<string>(), Arg.Any<string?>(),
            Arg.Any<string?>(), Arg.Any<RetryPolicyOptions?>(),
            Arg.Any<CancellationToken>())
            .Returns(new ResourceQueryResults<MyModel>([], false));

        var response = await ExecuteCommandAsync("--subscription", "sub123");

        var result = ValidateAndDeserializeResponse(
            response, {Toolset}JsonContext.Default.{Resource}{Operation}CommandResult);
        Assert.Empty(result.Items);
    }

    [Fact]
    public async Task ExecuteAsync_HandlesServiceErrors()
    {
        Service.GetResourcesAsync(
            Arg.Any<string?>(), Arg.Any<string>(), Arg.Any<string?>(),
            Arg.Any<string?>(), Arg.Any<RetryPolicyOptions?>(),
            Arg.Any<CancellationToken>())
            .ThrowsAsync(new Exception("Test error"));

        var response = await ExecuteCommandAsync("--subscription", "sub123", "--my-option", "val");

        Assert.Equal(HttpStatusCode.InternalServerError, response.Status);
        Assert.Contains("Test error", response.Message);
        Assert.Contains("troubleshooting", response.Message);
    }

    [Fact]
    public async Task ExecuteAsync_HandlesNotFound()
    {
        Service.GetResourcesAsync(
            Arg.Any<string?>(), Arg.Any<string>(), Arg.Any<string?>(),
            Arg.Any<string?>(), Arg.Any<RetryPolicyOptions?>(),
            Arg.Any<CancellationToken>())
            .ThrowsAsync(new RequestFailedException((int)HttpStatusCode.NotFound, "Resource not found"));

        var response = await ExecuteCommandAsync("--subscription", "sub123", "--my-option", "val");

        Assert.Equal(HttpStatusCode.NotFound, response.Status);
        Assert.Contains("Resource not found", response.Message);
    }
}
```

**Critical: Choose the correct test base class:**
 - Commands extending `SubscriptionCommand` → use `SubscriptionCommandUnitTestsBase<TCommand, TService>`
 - Commands extending `BaseCommand` directly (no subscription) → use `CommandUnitTestsBase<TCommand, TService>`
 
 Using the wrong base class will cause DI failures.

**Prefer string args over constructing options directly.** Using `ExecuteCommandAsync("--account", ...)` tests the full pipeline: `[Option]` attribute registration, `OptionBinder` parsing, and `SubscriptionResolver` post-processing.

Mock rules:
- Use `Arg.Any<CancellationToken>()` for CancellationToken in mocks
- Use `TestContext.Current.CancellationToken` when invoking real code
- Use `Arg.Is(value)` or the value directly for specific match assertions
- Never pass `CancellationToken.None` or `default` in test code

Deserialization rules:
- Use `{Toolset}JsonContext.Default.{Operation}CommandResult` for deserialization — never define custom test models
  - ✅ `ValidateAndDeserializeResponse(response, {Toolset}JsonContext.Default.{Operation}CommandResult)`
  - ❌ `JsonSerializer.Deserialize<TestModel>(json)`

**GATE:** `dotnet test tools/Azure.Mcp.Tools.{Toolset}/tests --filter "FullyQualifiedName~{Resource}{Operation}CommandTests"` must pass.

---

## Phase 3: Live Tests (Azure service commands only)

Skip this phase for non-Azure commands (CLI wrappers, best practices, documentation tools).

### 3a. Test Infrastructure

File: `tests/test-resources.bicep`

```bicep
targetScope = 'resourceGroup'

@minLength(3)
@maxLength(17)
param baseName string = resourceGroup().name

param testApplicationOid string = deployer().objectId
param location string = resourceGroup().location

resource myResource 'Microsoft.{Provider}/{type}@{api-version}' = {
  name: baseName
  location: location
  properties: { /* minimal config */ }
}

resource roleAssignment 'Microsoft.Authorization/roleAssignments@2022-04-01' = {
  name: guid(roleDefinition.id, testApplicationOid, myResource.id)
  scope: myResource
  properties: {
    principalId: testApplicationOid
    roleDefinitionId: roleDefinition.id
  }
}

output resourceName string = myResource.name
```

File: `tests/test-resources-post.ps1` (required even if empty logic)

```powershell
[CmdletBinding()]
param (
    [Parameter(Mandatory)] [hashtable] $DeploymentOutputs,
    [Parameter(Mandatory)] [hashtable] $AdditionalParameters
)
Write-Host "{Toolset} post-deployment setup completed."
```

Validate: `az bicep build --file tools/Azure.Mcp.Tools.{Toolset}/tests/test-resources.bicep`

### 3b. Live Test Class

File: `tests/Azure.Mcp.Tools.{Toolset}.Tests/{Toolset}CommandTests.cs`

```csharp
public class {Toolset}CommandTests(ITestOutputHelper output, TestProxyFixture fixture, LiveServerFixture liveServerFixture)
    : RecordedCommandTestsBase(output, fixture, liveServerFixture)
{
    [Fact]
    public async Task {Resource}{Operation}_ReturnsExpectedResult()
    {
        var result = await CallToolAsync(
            "{toolset}_{resource}_{operation}",
            new()
            {
                ["subscription"] = SubscriptionId,
                ["resource-group"] = ResourceGroupName,
            });

        Assert.NotNull(result);
        var items = result.Value.AssertProperty("items");
        Assert.Equal(JsonValueKind.Array, items.ValueKind);
    }
}
```

### 3c. Record and Verify

 #### Create assets.json

Create `assets.json` if it doesn't exist:

```json
{
  "AssetsRepo": "Azure/azure-sdk-assets",
  "AssetsRepoPrefixPath": "",
  "TagPrefix": "Azure.Mcp.Tools.{Toolset}.Tests",
  "Tag": ""
}
```

 #### Deploy
 ```powershell
eng/common/TestResources/New-TestResources.ps1 `
   -TestResourcesDirectory tools/Azure.Mcp.Tools.{Toolset}
 ```

 #### Record tests
 ```powershell
 dotnet test tools\Azure.Mcp.Tools.{Toolset}\tests\Azure.Mcp.Tools.{Toolset}.Tests `
   --filter "FullyQualifiedName~{Resource}{Operation}"
```

 #### Push recordings
 ```powershell
.proxy\Azure.Sdk.Tools.TestProxy push `
  -a tools\Azure.Mcp.Tools.{Toolset}\tests\Azure.Mcp.Tools.{Toolset}.Tests\assets.json
```

 #### Verify playback
Change TestMode to "Playback" in .testsettings.json, then re-run tests

### 3c-1. Recorded Test Pitfalls

**These are common causes of recorded test failures. Always verify playback passes after recording.**

#### Always pass `Settings.TenantId` in live test calls

If the test subscription lives in a non-default tenant, the command will fail with `InvalidAuthenticationTokenTenant`. Include tenant when your subscription requires it:
```csharp
var result = await CallToolAsync(
    "{toolset}_{resource}_{operation}",
    new()
    {
        { "subscription", Settings.SubscriptionId },
        { "resource-group", Settings.ResourceGroupName },
        { "tenant", Settings.TenantId }  // Always include
    });
```

#### Use `RegisterOrRetrieveVariable` for all dynamic values

Any non-deterministic value (`Guid.NewGuid()`, `DateTime.Now`) must be wrapped so the same value is used in both Record and Playback runs:
```csharp
// ✅ Value is recorded and replayed deterministically
var topicName = RegisterOrRetrieveVariable("create_topic_name", $"topic-{Guid.NewGuid():N}"[..24]);

// ❌ Different GUID each run — breaks playback request matching
var topicName = $"topic-{Guid.NewGuid():N}"[..24];
```

#### Assertions must survive sanitization

Recording sanitizers replace sensitive values (resource names, IDs, endpoints) with placeholders like `"Sanitized"`. Your assertion strategy depends on your test class sanitizer configuration:

| Approach | When to use | Example toolsets |
|----------|-------------|-----------------|
| Exact name assert | Your sanitizers do NOT replace the resource name | KeyVault, FunctionApp |
| Structural assert (`AssertProperty`) | Your sanitizers DO replace the name | EventGrid |
| `SanitizeAndRecord` helper | You need exact asserts AND have aggressive sanitizers | ManagedLustre |

**How to check:** After recording, inspect the session recording JSON (use `.proxy/Azure.Sdk.Tools.TestProxy.exe config locate -a <assets.json>`). If the `"name"` field shows `"Sanitized"`, you cannot use exact name asserts without the `SanitizeAndRecord` pattern.

```csharp
// Safe assertions that survive any sanitizer configuration:
topic.AssertProperty("name");  // Checks existence only
Assert.Equal("Succeeded", topic.GetProperty("provisioningState").GetString());  // Enum values aren't sanitized
Assert.Equal(JsonValueKind.Object, topic.ValueKind);  // Type checks
```

#### Credential type in `.testsettings.json`

`Deploy-TestResources.ps1` sets `AZURE_TOKEN_CREDENTIALS=AzurePowerShellCredential`. If the MCP server subprocess cannot access the PowerShell credential cache (common on some machines), switch to `AzureCliCredential`:
```json
"EnvironmentVariables": {
    "AZURE_TOKEN_CREDENTIALS": "AzureCliCredential"
}
```
Ensure `az login --tenant <tenant-id>` is active. If recording fails with credential errors from the subprocess, this is the likely fix.

#### Redeploy if resource group is missing

Test resource groups are auto-deleted after 12 hours. If tests fail with `ResourceGroupNotFound`, redeploy:
```powershell
./eng/scripts/Deploy-TestResources.ps1 -Paths {Toolset}
```

### 3d. Test Project Configuration (Critical)

The test `.csproj` **must** have these specific settings or tests will fail with "azmcp.exe not found":

```xml
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <TargetFramework>net10.0</TargetFramework>
    <ImplicitUsings>enable</ImplicitUsings>
    <Nullable>enable</Nullable>
    <IsPackable>false</IsPackable>
    <IsTestProject>true</IsTestProject>
    <OutputType>Exe</OutputType>
    <HasLiveTests>true</HasLiveTests>
    <HasUnitTests>true</HasUnitTests>
  </PropertyGroup>

  <ItemGroup>
    <ProjectReference Include="..\..\src\Azure.Mcp.Tools.{Toolset}.csproj" />
    <ProjectReference Include="$(RepoRoot)servers\Azure.Mcp.Server\src\Azure.Mcp.Server.csproj" />
  </ItemGroup>
</Project>
```

⚠️ Common mistake: Referencing only the toolset project. Live tests must also reference `Azure.Mcp.Server.csproj`.

### 3e. IAsyncLifetime Pattern

If your live test class needs `IAsyncLifetime` or overrides `Dispose`, you **must** call `base.Dispose()`:
```csharp
public class MyCommandTests(ITestOutputHelper output, TestProxyFixture fixture, LiveServerFixture liveServerFixture)
    : RecordedCommandTestsBase(output, fixture, liveServerFixture), IAsyncLifetime
{
    public ValueTask DisposeAsync()
    {
        base.Dispose();
        return ValueTask.CompletedTask;
    }
}
```
Failure to call `base.Dispose()` prevents request/response data from being written to failing test results.

**GATE:** Tests pass in both Record and Playback modes.

---

## Phase 4: Validation

Run all checks in order. All must pass.

```powershell
# 1. Build
dotnet build tools/Azure.Mcp.Tools.{Toolset}/src

# 2. Format
dotnet format Microsoft.Mcp.slnx --verify-no-changes --include "tools/Azure.Mcp.Tools.{Toolset}/**"

# 3. All unit tests (including existing — no regressions)
dotnet test tools/Azure.Mcp.Tools.{Toolset}/tests

# 4. Spell check
.\eng\common\spelling\Invoke-Cspell.ps1

# 5. Full verification
./eng/scripts/Build-Local.ps1 -UsePaths -VerifyNpx

# 6. AOT/Native build (required for AOT-compatible toolsets)
./eng/scripts/Build-Local.ps1 -BuildNative
```

If AOT fails (common for new Azure SDK dependencies):
1. Move toolset setup in `Program.cs` under `#if !BUILD_NATIVE`
2. Add `ProjectReference-Remove` condition in `Azure.Mcp.Server.csproj`

**GATE:** All 6 checks green.

---

## Phase 5: Documentation

### 5a. Command Reference

File: `servers/Azure.Mcp.Server/docs/azmcp-commands.md`

Add command in alphabetical order within service section. Then regenerate metadata:

```powershell
./eng/scripts/Update-AzCommandsMetadata.ps1
```

### 5b. Test Prompts

File: `servers/Azure.Mcp.Server/docs/e2eTestPrompts.md`

Add 2-3 natural language prompts in alphabetical order:

```markdown
| {toolset}_{resource}_{operation} | Natural language prompt |
```

### 5c. Changelog Entry

Follow `docs/changelog-entries.md`. Create entry using `./eng/scripts/New-ChangelogEntry.ps1` or manually. Use `-ChangelogPath servers/Azure.Mcp.Server/CHANGELOG.md`.

### 5d. README Updates

- **`servers/Azure.Mcp.Server/README.md`**: Update the supported services table (line ~1189) and add example prompts in the "What can you do" section (line ~898). This file is processed by `eng/scripts/Process-PackageReadMe.ps1` into package-specific outputs (NuGet, VSIX, npm, PyPI) so a single update covers all distribution channels.

### 5e. CODEOWNERS

File: `.github/CODEOWNERS`

Add your new toolset path with appropriate team ownership:
```
/tools/Azure.Mcp.Tools.{Toolset}/ @your-team
```

### 5f. Consolidated Tools Registration

File: `servers/Azure.Mcp.Server/src/Resources/consolidated-tools.json`

Add your new tool(s) to the consolidated tools JSON. Use the following command to find the correct tool name:
```powershell
cd servers/Azure.Mcp.Server/src/bin/Debug/net10.0
./azmcp[.exe] tools list --name --namespace <tool_area>
```

**Documentation Standards:**
- Use consistent command paths in all documentation (e.g., `azmcp sql db show`)
- Always run `.\eng\scripts\Update-AzCommandsMetadata.ps1` after updating azmcp-commands.md (CI will fail if skipped)
- Organize example prompts by service in README.md under service-specific sections
- Maintain **alphabetical sorting** in e2eTestPrompts.md (service sections AND tool names within each table)
- Include parameter descriptions and required vs optional indicators in azmcp-commands.md

**GATE:** `./eng/scripts/Update-AzCommandsMetadata.ps1` succeeds.

---

## Phase 6: Tool Description Evaluation

Now that test prompts are written (Phase 5b), validate your command description against them using the **ToolDescriptionEvaluator**.

> **Full documentation:** See `eng/tools/ToolDescriptionEvaluator/Quickstart.md` for setup details.

### Prerequisites

Set your Azure OpenAI endpoint and API key as environment variables:

```powershell
$env:AOAI_ENDPOINT = "https://<your-resource>.openai.azure.com/openai/deployments/<embeddings-deployment-name>/embeddings?api-version=<api-version>"
$env:TEXT_EMBEDDING_API_KEY = "your_api_key_here"
```

> For internal contributors, refer to the **Before creating a pull request** section of [this document](https://eng.ms/docs/products/azure-developer-experience/mcp/mcp-getting-started) to use our team's deployment and credentials.

### Option A: Test a single tool description (fastest for development)

Use `--test-single-tool` mode to validate your description without building the full server:

```powershell
# Test a single tool description against one prompt
dotnet run --project eng/tools/ToolDescriptionEvaluator/src -- --test-single-tool `
  --tool-description "Your command description" `
  --prompt "user query"

# Test against multiple prompts (recommended — test 2-3 phrasings)
dotnet run --project eng/tools/ToolDescriptionEvaluator/src -- --test-single-tool `
  --tool-description "Lists all user-assigned managed identities in a subscription" `
  --prompt "show me my managed identities" `
  --prompt "list managed identities in my subscription" `
  --prompt "what identities do I have"
```

### Option B: Run the full evaluator against your service area

This builds the server and tests all tools in your area against the e2eTestPrompts.md file:

```powershell
# Run evaluator for your specific service area
pushd eng/tools/ToolDescriptionEvaluator
./scripts/Run-ToolDescriptionEvaluator.ps1 -Area "{Toolset}"

# Build the Azure.Mcp.Server as part of the run
./scripts/Run-ToolDescriptionEvaluator.ps1 -Area "{Toolset}" -BuildAzureMcp

# Run for all Azure MCP Server tools (slower)
./scripts/Run-ToolDescriptionEvaluator.ps1
popd
```

### Interpreting Results

Target: **Top 3 ranking** and confidence score **≥ 0.4**.

- Score `>= 0.6`: Excellent — tool will be reliably selected
- Score `0.4 - 0.6`: Acceptable — tool should be selected in most cases
- Score `< 0.4`: Poor — description needs improvement

### Improving Low Scores

If score is low, improve the `Description` in `[CommandMetadata]`:
- Include verbs users would say ("list", "get", "show", "configure")
- Mention specific resource types and Azure service names
- Describe what the output contains
- Consider common synonyms and alternative phrasings
- Avoid overly generic descriptions that could match many tools

Custom prompts file formats:
- **Markdown**: Same table format as `servers/Azure.Mcp.Server/docs/e2eTestPrompts.md`
- **JSON**: `{ "azmcp-your-command": ["prompt1", "prompt2"] }`

**GATE:** Score meets threshold (≥ 0.4, top 3 ranking). If the evaluator is not available (no Azure OpenAI credentials), manually verify the description is specific and action-oriented.

---

## Phase 7: PR Checklist

Before creating the PR, verify all of these:

### Core Implementation
- [ ] Options class is flat POCO with `[Option]` attributes implementing `ISubscriptionOption`
- [ ] Command inherits `SubscriptionCommand<TOptions, TResult>` with `ISubscriptionResolver`
- [ ] `ExecuteAsync` takes `(CommandContext, TOptions, CancellationToken)` — no `ParseResult`
- [ ] Service interface and implementation complete
- [ ] All async methods include `CancellationToken` parameter as final argument
- [ ] Unit tests cover all paths (using `SubscriptionCommandUnitTestsBase`)
- [ ] Integration/live tests added
- [ ] Command registered as singleton in `{Toolset}Setup.cs` `ConfigureServices`
- [ ] Command added to group in `{Toolset}Setup.cs` `RegisterCommands`
- [ ] Follows file structure exactly
- [ ] Error handling implemented with `HandleException(context, ex)`
- [ ] New tools added to `consolidated-tools.json`
- [ ] Documentation complete

### Package and Project Setup
- [ ] Azure SDK package added to both `Directory.Packages.props` AND `.csproj`
- [ ] Package version consistency (same version in both files)
- [ ] Projects added to `Microsoft.Mcp.slnx` and `Azure.Mcp.Server.slnx`
- [ ] Toolset registered in `Program.cs` `RegisterAreas()` (alphabetical)
- [ ] JSON serialization context includes all new model types

### Build and Code Quality
- [ ] Build passes (`dotnet build`)
- [ ] No compiler warnings
- [ ] Format clean (`dotnet format --verify-no-changes`)
- [ ] All unit tests pass (no regressions)
- [ ] All live tests pass in Playback mode
- [ ] Recordings pushed and `assets.json` committed
- [ ] Spell check passes (`.\eng\common\spelling\Invoke-Cspell.ps1`)
- [ ] ToolDescriptionEvaluator score ≥ 0.4
- [ ] AOT compilation verified (`./eng/scripts/Build-Local.ps1 -BuildNative`)
- [ ] One tool per PR

### Azure SDK Integration
- [ ] All Azure SDK property names verified and correct
- [ ] Resource access patterns use collections (e.g., `.GetSqlServers().GetAsync()`)
- [ ] `CancellationToken` passed to all async SDK calls
- [ ] Subscription resolution uses `ISubscriptionResolver` (injected in constructor)
- [ ] Service constructor includes `ISubscriptionService` injection

### Documentation
- [ ] `azmcp-commands.md` updated with command documentation
- [ ] `Update-AzCommandsMetadata.ps1` executed (CI will fail if skipped)
- [ ] `e2eTestPrompts.md` updated (alphabetical order maintained)
- [ ] Changelog entry created (use `-ChangelogPath`)
- [ ] `servers/Azure.Mcp.Server/README.md` updated with example prompts and service listing
- [ ] `.github/CODEOWNERS` entry added for new toolset

### Transport-Agnostic Requirements (Remote MCP Server Compatibility)
- [ ] Commands are stateless — no per-request state in instance fields
- [ ] Commands are thread-safe for multi-user concurrency
- [ ] No transport checks (`Environment.GetEnvironmentVariable("ASPNETCORE_URLS")`, `HttpContext`)
- [ ] Error messages are context-aware (include OBO-specific guidance where applicable)
- [ ] Uses `IAzureTokenCredentialProvider` for all authentication (not direct `DefaultAzureCredential`)

### Required Files Checklist

Verify all files exist for your command:

1. `src/Options/{Resource}/{Resource}{Operation}Options.cs` (flat POCO with `[Option]` attributes)
2. `src/Commands/{Resource}/{Resource}{Operation}Command.cs`
3. `src/Services/I{Toolset}Service.cs`
4. `src/Services/{Toolset}Service.cs`
5. `src/Commands/{Toolset}JsonContext.cs`
6. `src/{Toolset}Setup.cs` (implements `IAreaSetup`, registers commands + services)
7. `tests/Azure.Mcp.Tools.{Toolset}.Tests/{Resource}/{Resource}{Operation}CommandTests.cs`
8. `tests/Azure.Mcp.Tools.{Toolset}.Tests/{Toolset}CommandTests.cs` (live tests, Azure only)
9. `tests/test-resources.bicep` (Azure service commands only)
10. `tests/test-resources-post.ps1` (Azure service commands only)

---

## Quick Reference: Naming Conventions

| Element | Pattern | Example |
|---------|---------|---------|
| Command class | `{Resource}{SubResource?}{Operation}Command` | `StorageAccountGetCommand` |
| Options class | `{Resource}{Operation}Options` | `StorageAccountGetOptions` |
| Test class | `{Resource}{Operation}CommandTests` | `StorageAccountGetCommandTests` |
| CLI command | `azmcp {service} {resource} {operation}` | `azmcp storage account get` |
| Command group | Concatenated lowercase | `"resourcegroup"`, `"storageaccount"` |
| Option flag | `--kebab-case` | `--resource-group`, `--account` |

**Naming rules:**
- Resource = top-level domain entity (`Server`, `Database`, `FileSystem`)
- SubResource (optional) = nested concept (`Config`, `Param`, `SubnetSize`)
- Operation = action or computed intent (`List`, `Get`, `Set`, `Show`, `Delete`, `Calculate`)
- ✅ `ServerListCommand`, `ServerConfigGetCommand`, `FileSystemSubnetSizeCommand`
- ❌ `GetConfigCommand` (missing resource), `ListServerCommand` (verb precedes resource)

## Quick Reference: ToolMetadata Properties

| Property | `true` | `false` |
|----------|--------|---------|
| `Destructive` | Deletes/modifies resources | Read-only or safe operations |
| `Idempotent` | Same result on repeated calls | Accumulates effects |
| `OpenWorld` | Unpredictable external systems | Well-defined Azure APIs |
| `ReadOnly` | Only queries data | Creates/updates/deletes |
| `Secret` | Returns credentials/keys | Returns non-sensitive data |
| `LocalRequired` | Needs local tools/files | Remote API calls only |

**Detailed ToolMetadata guidance:**

- **OpenWorld**: Most Azure resource commands use `false` because they operate within the well-defined domain of Azure Resource Manager APIs. Only use `true` for commands interacting with truly unpredictable external systems outside Azure's control.
  - `false`: Storage accounts, databases, VMs, schema definitions, best practices guides
  - `true`: External web scraping, unstructured third-party data sources (rare)

- **Destructive**: Set `true` for commands that delete, modify, or could cause data loss.
  - `true`: Delete database, reset keys, purge storage, modify critical settings
  - `false`: List resources, show configuration, query data, get status

- **Idempotent**: Can it be safely called multiple times with same params?
  - `true`: Set config to specific value, create named resource (with "already exists" handled)
  - `false`: Generate new keys, create resources with auto-generated names, append logs

- **Secret**: Does it return sensitive data?
  - `true`: Get storage account keys, show connection strings, retrieve certificates
  - `false`: List public resources, show non-sensitive config

- **LocalRequired**: Does it need local tools/files?
  - `true`: Azure CLI wrappers, local file operations, tools requiring local installation
  - `false`: Pure cloud API commands (most Azure resource commands)

### ⚠️ Metadata Validation Checklist

After setting `[CommandMetadata]` properties, cross-check each value against these heuristics. **Do not proceed if any check fails — correct the metadata first.**

**Destructive:**
- If the command name contains `delete`, `remove`, `purge`, `reset`, `revoke`, or `update` → must be `true`
- If the command name contains `list`, `get`, `show`, `query`, or `describe` → must be `false`
- If the command creates resources that replace existing ones → should be `true`

**Idempotent:**
- If calling the command twice with the same inputs produces different results → must be `false`
- If the command generates new keys, rotates secrets, or creates auto-named resources → must be `false`
- If the command returns the same data or sets the same state regardless of repetition → must be `true`

**OpenWorld:**
- If the command only calls Azure Resource Manager, Microsoft Graph, or other well-defined Microsoft APIs → must be `false`
- Only set `true` if the command interacts with user-controlled external systems, arbitrary URLs, or unpredictable third-party services

**ReadOnly:**
- If the command can modify state (create, update, delete, write, upload) → must be `false`
- If the command only retrieves information → must be `true`
- Must be the logical inverse of `Destructive` for most commands (both can be `false` for create operations)

**Secret:**
- If the command name or resource contains `credential`, `secret`, `key`, `password`, `certificate`, `token`, or `connectionstring` → default to `true` unless the command provably cannot expose any sensitive information
- If the command returns access keys, connection strings, secret values, or credential metadata (IDs, expiry, types) → must be `true`
- If the command only lists resource names or non-sensitive configuration → `false`
- **When in doubt, set `true`** — it is safer to over-classify than to expose credentials without the Secret flag

**LocalRequired:**
- If the command makes only remote API calls → must be `false`
- Only set `true` if the command requires local file system access, local CLI tools, or locally installed software

Guidelines:
- Fully declare all `ToolMetadata` properties even if using defaults
- Only override `GetErrorMessage` and `GetStatusCode` if logic differs from base class
- Commands returning arrays return empty array `[]` for null/empty service results

## Quick Reference: Common Pitfalls

**Never do (new pattern):**
- ❌ `subscriptionId` → ✅ `subscription`
- ❌ Options without `[Option]` attribute → ✅ Always add `[Option("description")]` or `[Option(OptionDescriptions.X)]`
- ❌ Inherit options from base class → ✅ Flat POCO implementing `ISubscriptionOption`
- ❌ Manual `RegisterOptions`/`BindOptions` in new commands → ✅ Use `[Option]` attributes (automatic)
- ❌ `ExecuteAsync(context, parseResult, ct)` → ✅ `ExecuteAsync(context, options, ct)`
- ❌ Call `Validate(parseResult.CommandResult, ...)` → ✅ Override `ValidateOptions(options, result)` if needed
- ❌ Hardcoded cloud URLs → ✅ `CloudConfiguration.CloudType` switch
- ❌ Logging `{@Options}` → ✅ Log only safe parameters individually
- ❌ Underscores in group names → ✅ Concatenated lowercase or dash-separated
- ❌ Missing `CancellationToken` → ✅ Always the final parameter
- ❌ `CancellationToken.None` in tests → ✅ `TestContext.Current.CancellationToken`
- ❌ Skip `base.Dispose()` in tests → ✅ Always call when overriding
- ❌ Skip live test infrastructure for Azure commands → ✅ Create `test-resources.bicep` early
- ❌ `CommandUnitTestsBase` for subscription commands → ✅ Use `SubscriptionCommandUnitTestsBase`
- ❌ `[Option(Name = "my-option")]` when default matches → ✅ Only use `Name =` when kebab-case conversion is wrong
- ❌ Forget to register command as singleton → ✅ `services.AddSingleton<MyCommand>()` in `ConfigureServices`

**Always do:**
- Use `[Option]` attributes on flat options POCO (implements `ISubscriptionOption`)
- Use `SubscriptionCommand<TOptions, TResult>` with `ISubscriptionResolver` injection
- Inherit `SubscriptionCommandUnitTestsBase<TCommand, TService>` for unit tests
- Create test infrastructure before implementing live tests
- Follow exact file structure and naming
- Make command classes `sealed`
- Use primary constructors
- Register commands AND services in `{Toolset}Setup.cs` `ConfigureServices`
- Register toolset in `Program.cs` `RegisterAreas()`
- Handle all error cases with `HandleException`
- Use consistent resource naming patterns
- Reference `docs/option-conversion.md` when working with legacy one-generic commands

---

## Reference: SDK Property Name Discovery

Azure SDK property names frequently differ from documentation or expected names. Always verify actual property names before implementation.

### Verification Steps: **CRITICAL: Verify SDK Property Names Before Implementation**

1. **Use IntelliSense First**: Let the IDE show you what's actually available
2. **Inspect Assemblies When Needed**: If you get compilation errors about missing properties:
   ```powershell
   $dll = Get-ChildItem -Path "." -Recurse -Filter "Azure.ResourceManager.*.dll" | Select-Object -First 1 -ExpandProperty FullName
   Add-Type -Path $dll
   [Azure.ResourceManager.Compute.Models.VirtualMachineExtensionInstanceView].GetProperties() | Select-Object Name, PropertyType
   ```

### Common Property Name Patterns

- Extension types: `VirtualMachineExtensionInstanceViewType` (not `TypeHandlerType`)
- Time properties: Often use `StartOn`/`LastActionOn` (not `StartTime`/`LastActionTime`)
- Date properties: May use `CreatedOn` (not `CreationDate` or `CreateDate`)
- Location: Usually `Location.Name` or `Location.ToString()` (Location is an object, not a string)

### Properties That May Not Exist

- Some properties shown in REST API may not exist in .NET SDK models
- Set values to `null` if the property truly doesn't exist in the data model
- Don't try to derive missing data from other sources unless explicitly required
- Document why a property is set to null in comments

### Dictionary Type Casting for Tags

```csharp
// ✅ Correct: Cast to IReadOnlyDictionary
Tags: data.Tags as IReadOnlyDictionary<string, string>

// ❌ Wrong: Direct assignment causes CS1503
Tags: data.Tags
```

---

## Reference: ARM Compilation Error Troubleshooting

### Common Errors and Fixes

**`cannot convert from 'CancellationToken' to 'string'`**
- Check method parameter order. Many Azure SDK methods use named parameters.
- Fix: `.GetAsync(resourceName, cancellationToken: cancellationToken)`

**`'SqlDatabaseData' does not contain a definition for 'X'`**
- Property names differ from expected. Common fixes:
  - `CreationDate` → `CreatedOn`
  - `EarliestRestoreDate` → `EarliestRestoreOn`
  - `Edition` → `CurrentSku?.Name`

**`Operator '?' cannot be applied to operand of type 'AzureLocation'`**
- `AzureLocation` is a struct. Use `Location.ToString()` instead of `Location?.Name`

**Wrong resource access pattern**
- ❌ `.GetSqlServerAsync(name, cancellationToken)`
- ✅ `.GetSqlServers().GetAsync(name, cancellationToken: cancellationToken)`
- Pattern: Always access through collections, not direct async methods

**Missing package references**
1. Add `<PackageVersion Include="Azure.ResourceManager.{Service}" Version="{version}" />` to `Directory.Packages.props`
2. Then add `<PackageReference Include="Azure.ResourceManager.{Service}" />` to project `.csproj`
- Always add to `Directory.Packages.props` first

### Specialized Resource Collection Patterns

```csharp
// ✅ Rolling upgrade status for VMSS
var upgradeStatus = await vmssResource.Value
    .GetVirtualMachineScaleSetRollingUpgrade()
    .GetAsync(cancellationToken);

// ✅ VMSS instances
var vms = await vmssResource.Value
    .GetVirtualMachineScaleSetVms()
    .GetAllAsync(cancellationToken: cancellationToken);

// Pattern: Get{ResourceType}() returns collection,
// then .GetAsync(name, CancellationToken) or .GetAllAsync(CancellationToken)
```

### Subscription Resolution

```csharp
// ✅ Correct: use ISubscriptionService
var subscriptionResource = await _subscriptionService.GetSubscription(subscription, tenant, retryPolicy);

// ❌ Wrong: manual ARM client creation
var armClient = await CreateArmClientAsync(tenant, retryPolicy);
var subscriptionResource = armClient.GetSubscriptionResource(new ResourceIdentifier($"/subscriptions/{subscription}"));
```

---

## Reference: Implicit Usings and Code Quality

### Implicit Usings (already included via `Directory.Build.props`)

The project has `<ImplicitUsings>enable</ImplicitUsings>`, so these are automatically available — do NOT add them manually:

- `System`
- `System.Collections.Generic`
- `System.IO`
- `System.Linq`
- `System.Net.Http`
- `System.Threading`
- `System.Threading.Tasks`

### Preventing Unused Usings

- Start with minimal using statements — add as needed
- Don't copy using blocks from other files
- Run `dotnet format --include="tools/Azure.Mcp.Tools.{Toolset}/**/*.cs"` before committing

### Detection and Cleanup

```powershell
# Format specific toolset
dotnet format --include="tools/Azure.Mcp.Tools.{Toolset}/**/*.cs" --verbosity normal

# Format entire solution
dotnet format ./Microsoft.Mcp.slnx --verbosity normal

# Check for warnings
dotnet build --verbosity normal | Select-String "warning"
```

### Environment Variable Tests

If any test mutates environment variables, the test project must:
- Reference `core/Azure.Mcp.Core/tests/Azure.Mcp.Tests/Azure.Mcp.Tests.csproj`
- Include `AssemblyAttributes.cs`:
  ```csharp
  [assembly: Azure.Mcp.Tests.Helpers.ClearEnvironmentVariablesBeforeTest]
  [assembly: Xunit.CollectionBehavior(Xunit.CollectionBehavior.CollectionPerAssembly)]
  ```

---

## Reference: Command Architecture

### Command Hierarchy (Two-Generic Pattern — Current)

```
IBaseCommand
└── BaseCommand<TOptions, TResult>
    └── AuthenticatedCommand<TOptions, TResult>
        └── SubscriptionCommand<TOptions, TResult>  (where TOptions : ISubscriptionOption)
            └── {Resource}{Operation}Command  (sealed, concrete)
```

For toolsets with shared cross-command logic, add an intermediate base:
```
SubscriptionCommand<TOptions, TResult>
    └── Base{Toolset}Command<TOptions, TResult>  (where TOptions : I{Toolset}Option)
        └── Base{Resource}Command<TOptions, TResult>  (where TOptions : I{Resource}Option)
            └── {Resource}{Operation}Command  (sealed, concrete)
```

`IBaseCommand` provides:
- `Name`: Command name for CLI display
- `Description`: Detailed command description
- `Title`: Human-readable command title
- `Metadata`: Behavioral characteristics (ToolMetadata)
- `GetCommand()`: Retrieves System.CommandLine command definition
- `ExecuteAsync()`: Executes command logic

Important rules:
- Commands use primary constructors with `ILogger`, service interface, and `ISubscriptionResolver` injection
- Classes are always `sealed` unless explicitly intended for inheritance
- `SubscriptionCommand` handles subscription validation and resolution via `ISubscriptionResolver`
- Options binding is automatic via `[Option]` attributes — no manual `RegisterOptions`/`BindOptions`

### Intermediate Base Command (only when needed)

Use interface constraints for type-safe shared behavior (see `docs/option-conversion.md` Step 5):

```csharp
// Define interface for shared option access
public interface I{Toolset}Option
{
    string Account { get; }
}

// Base command constrains TOptions to the interface
public abstract class Base{Toolset}Command<
    [DynamicallyAccessedMembers(TrimAnnotations.CommandAnnotations)] TOptions, TResult>(
    ISubscriptionResolver subscriptionResolver)
    : SubscriptionCommand<TOptions, TResult>(subscriptionResolver)
    where TOptions : class, ISubscriptionOption, I{Toolset}Option
{
    public override void ValidateOptions(TOptions options, ValidationResult validationResult)
    {
        base.ValidateOptions(options, validationResult);
        // Shared validation using options.Account
    }
}

// Options class implements the interface (stays flat, no inheritance)
public class MyOptions : ISubscriptionOption, I{Toolset}Option
{
    [Option("The account name.")]
    public required string Account { get; set; }

    [Option(OptionDescriptions.Subscription)]
    public string? Subscription { get; set; }
    // ...
}
```

### Tool ID

The `Id` in `[CommandMetadata]` is a unique GUID for each tool. Generate a new one for every command — it uniquely identifies the tool across the entire system.

---

## Reference: Option Extension Methods (Legacy Pattern)

> **⚠️ LEGACY:** This section documents the **one-generic** pattern used by unconverted toolsets (e.g., KeyVault, some older tools). New commands should use the **two-generic pattern** with `[Option]` attributes as shown in Phase 1. Only reference this when maintaining or converting existing one-generic commands. See `docs/option-conversion.md` for the full migration guide.

### Available Extension Methods

```csharp
// For OptionDefinition<T> instances
.AsRequired()    // Creates a required option instance
.AsOptional()    // Creates an optional option instance

// For existing Option<T> instances
.AsRequired()    // Creates a new required version
.AsOptional()    // Creates a new optional version
```

### Key Principles (Legacy Pattern)

- Commands explicitly register options in `RegisterOptions`
- Each command controls whether each option is required or optional
- Binding is explicit using `parseResult.GetValueOrDefault(Option<T>)`
- No shared state between commands — each gets its own option instance
- Only use `.AsRequired()` / `.AsOptional()` if changing the default `Required` setting

### Usage Patterns (Legacy)

**Commands requiring specific options:**
```csharp
protected override void RegisterOptions(Command command)
{
    base.RegisterOptions(command);
    command.Options.Add(OptionDefinitions.Common.ResourceGroup.AsRequired());
    command.Options.Add(ServiceOptionDefinitions.Account.AsRequired());
    command.Options.Add(ServiceOptionDefinitions.Database); // uses default
}

protected override MyCommandOptions BindOptions(ParseResult parseResult)
{
    var options = base.BindOptions(parseResult);
    options.ResourceGroup ??= parseResult.GetValueOrDefault(OptionDefinitions.Common.ResourceGroup);
    options.Account = parseResult.GetValueOrDefault(ServiceOptionDefinitions.Account);
    options.Database = parseResult.GetValueOrDefault(ServiceOptionDefinitions.Database);
    return options;
}
```

**Commands using options optionally:**
```csharp
protected override void RegisterOptions(Command command)
{
    base.RegisterOptions(command);
    command.Options.Add(ServiceOptionDefinitions.Account.AsOptional());
    command.Options.Add(OptionDefinitions.Common.ResourceGroup.AsOptional());
}
```

**Commands with exclusive/validation options (legacy approach):**
```csharp
protected override void RegisterOptions(Command command)
{
    base.RegisterOptions(command);
    command.Options.Add(ServiceOptionDefinitions.EitherThis);
    command.Options.Add(ServiceOptionDefinitions.OrThat);
    command.Validators.Add(commandResult =>
    {
        var eitherThis = commandResult.GetOrDefaultValue(ServiceOptionDefinitions.EitherThis);
        var orThat = commandResult.GetOrDefaultValue(ServiceOptionDefinitions.OrThat);

        if (string.IsNullOrWhiteSpace(eitherThis) && string.IsNullOrWhiteSpace(orThat))
            commandResult.AddError("Either --either-this or --or-that must be provided.");

        if (!string.IsNullOrWhiteSpace(eitherThis) && !string.IsNullOrWhiteSpace(orThat))
            commandResult.AddError("Cannot specify both --either-this and --or-that.");
    });
}
```

**New pattern equivalent** for exclusive validation:
```csharp
public override void ValidateOptions(MyOptions options, ValidationResult validationResult)
{
    base.ValidateOptions(options, validationResult);

    if (string.IsNullOrWhiteSpace(options.EitherThis) && string.IsNullOrWhiteSpace(options.OrThat))
        validationResult.Errors.Add("Either --either-this or --or-that must be provided.");

    if (!string.IsNullOrWhiteSpace(options.EitherThis) && !string.IsNullOrWhiteSpace(options.OrThat))
        validationResult.Errors.Add("Cannot specify both --either-this and --or-that.");
}
```

**Custom option (making required option optional for specific command):**
```csharp
protected override void RegisterOptions(Command command)
{
    base.RegisterOptions(command);
    command.Options.Remove(ComputeOptionDefinitions.ResourceGroup);

    // ✅ Correct: Use string parameters for Option constructor
    var optionalRg = new Option<string>("--resource-group", "-g")
    {
        Description = "The name of the resource group (optional)"
    };
    command.Options.Add(optionalRg);

    // ❌ Wrong: Don't use array for aliases in constructor
    // var wrongOption = new Option<string>(aliases.ToArray(), "Description");
}
```

### Important Binding Patterns (Legacy)

- Use `??=` for options that might be set by base classes (global options)
- Use direct assignment for command-specific options
- Use `parseResult.GetValueOrDefault(Option<T>)` always
- Extension methods create new option instances — no shared state

---

## Reference: Error Handling

### Status Code Mapping

Base implementation returns `InternalServerError` for all exceptions. Override for service-specific codes:

```csharp
protected override HttpStatusCode GetStatusCode(Exception ex) => ex switch
{
    Azure.RequestFailedException reqEx => (HttpStatusCode)reqEx.Status,
    Azure.Identity.AuthenticationFailedException => HttpStatusCode.Unauthorized,
    ValidationException => HttpStatusCode.BadRequest,
    _ => base.GetStatusCode(ex)
};
```

### Error Message Formatting

Base returns `ex.Message`. Override for user-actionable messages:

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

### HandleException Response Format

The base `HandleException` in `BaseCommand`:
```csharp
protected virtual void HandleException(CommandContext context, Exception ex)
{
    context.Activity?.SetStatus(ActivityStatusCode.Error);
    var result = new ExceptionResult(Message: ex.Message, StackTrace: ex.StackTrace, Type: ex.GetType().Name);
    response.Status = GetStatusCode(ex);
    response.Message = GetErrorMessage(ex) + ". To mitigate this issue, please refer to the troubleshooting guidelines here at https://aka.ms/azmcp/troubleshooting.";
    response.Results = ResponseResult.Create(result, JsonSourceGenerationContext.Default.ExceptionResult);
}
```

Always call `HandleException(context, ex)` in catch blocks.

### Error Context Logging

```csharp
catch (Exception ex)
{
    _logger.LogError(ex, "Error in {Operation}. Subscription: {Subscription}", Name, options.Subscription);
    HandleException(context, ex);
}
```

**DO NOT** log `{@Options}` — may expose sensitive information. Only log known-safe parameters.

### Common Error Scenarios to Handle

1. **Authentication/Authorization**: Credential expiry, missing RBAC, invalid connection strings
2. **Validation**: Missing required params, invalid formats, conflicting options
3. **Resource State**: Not found, locked/in use, invalid state
4. **Service Limits**: Throttling/rate limits, quota exceeded, capacity
5. **Network/Connectivity**: Service unavailable, timeouts, network failures

---

## Reference: Method Signature Consistency

### Service Interface Formatting Rules

```csharp
// ✅ Correct: parameters aligned with line breaks
Task<List<string>> GetStorageAccounts(
    string subscription,
    string? tenant = null,
    RetryPolicyOptions? retryPolicy = null,
    CancellationToken cancellationToken = default);

// ❌ Incorrect: all on single line
Task<List<string>> GetStorageAccounts(string subscription, string? tenant = null, RetryPolicyOptions? retryPolicy = null);

// ❌ Incorrect: missing CancellationToken
Task<List<string>> GetStorageAccounts(
    string subscription,
    string? tenant = null,
    RetryPolicyOptions? retryPolicy = null);
```

Rules:
- Parameters indented and aligned
- Blank lines between method declarations
- `CancellationToken` always the final parameter
- Only use default value `= default` in the signature if other parameters also have defaults
- At call sites, always pass the `CancellationToken` explicitly — never rely on `= default` to omit it

### CancellationToken in Service Implementations

- Pass to all async calls: `cancellationToken: cancellationToken`
- Use `.WithCancellation(cancellationToken)` for `await foreach`:
  ```csharp
  // ✅ Correct
  await foreach (var rg in subscription.GetResourceGroups().WithCancellation(cancellationToken))

  // ❌ Wrong: missing .WithCancellation()
  await foreach (var rg in subscription.GetResourceGroups())
  ```
- Never pass `CancellationToken.None` or `default` as a value

### API Pattern Discovery

Study existing services for resource access patterns:
- ✅ `.GetSqlServers().GetAsync(serverName, cancellationToken: cancellationToken)`
- ❌ `.GetSqlServerAsync(serverName, cancellationToken)` — methods like this don't exist

---

## Reference: Live Test Details

### JSON Validation in Live Tests

```csharp
// Use AssertProperty when the property MUST exist
var items = result.AssertProperty("items");
Assert.Equal(JsonValueKind.Array, items.ValueKind);

// Use TryGetProperty for optional/conditional properties
if (item.TryGetProperty("optional", out var optionalProp))
{
    Assert.Equal(JsonValueKind.String, optionalProp.ValueKind);
}
```

### Key Bicep Template Requirements

- Use `baseName` parameter with appropriate length restrictions
- Include `testApplicationOid` for RBAC assignments
- Deploy test resources (databases, containers) needed for integration tests
- Assign appropriate built-in roles to the test application
- Output resource names and identifiers for test consumption
- Use minimal SKUs (Basic, Standard S0) for cost efficiency
- Deploy only resources needed for command testing
- Use resource naming that identifies test purposes

Common resource naming patterns:
- Main service: `baseName` (most common) or `{baseName}{suffix}` if disambiguation needed
- Child resources: `test{resource}` (e.g., `testdb`, `testcontainer`)
- Deployments are per-toolset — name collisions across toolsets should not occur

### Using Deployed Resources in Tests

```csharp
public class {Toolset}CommandTests(ITestOutputHelper output, TestProxyFixture fixture, LiveServerFixture liveServerFixture)
    : RecordedCommandTestsBase(output, fixture, liveServerFixture)
{
    [Fact]
    public async Task Should_Get{Resource}_Successfully()
    {
        var serviceName = Settings.ResourceBaseName;
        var resourceName = "test{resource}";

        var result = await CallToolAsync(
            "{toolset}_{resource}_show",
            new()
            {
                { "subscription", Settings.SubscriptionId },
                { "resource-group", Settings.ResourceGroupName },
                { "service-name", serviceName },
                { "resource-name", resourceName }
            });

        Assert.NotNull(result);
        var resource = result.Value.AssertProperty("{resource}");
        Assert.Equal(JsonValueKind.Object, resource.ValueKind);
        Assert.Equal(resourceName, resource.GetProperty("name").GetString());
    }
}
```

### Deploy and Run Live Tests

```powershell
# Deploy test resources
./eng/scripts/Deploy-TestResources.ps1 -Paths "{Toolset}"

# Run live tests
pushd 'tools/Azure.Mcp.Tools.{Toolset}/tests/Azure.Mcp.Tools.{Toolset}.Tests'
dotnet test --filter "Category=Live"
```

### IAsyncLifetime and base.Dispose()

If your live test class implements `IAsyncLifetime` or overrides `Dispose`, you **must** call `base.Dispose()`:

```csharp
public class MyCommandTests(ITestOutputHelper output, TestProxyFixture fixture, LiveServerFixture liveServerFixture)
    : RecordedCommandTestsBase(output, fixture, liveServerFixture), IAsyncLifetime
{
    public ValueTask DisposeAsync()
    {
        base.Dispose();
        return ValueTask.CompletedTask;
    }
}
```

Failure to call `base.Dispose()` prevents request/response data from being written to failing test results.

### Live Test Project Configuration (.csproj)

Live test projects must reference the server project and include specific properties:

```xml
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <TargetFramework>net10.0</TargetFramework>
    <ImplicitUsings>enable</ImplicitUsings>
    <Nullable>enable</Nullable>
    <IsPackable>false</IsPackable>
    <IsTestProject>true</IsTestProject>
    <OutputType>Exe</OutputType>
  </PropertyGroup>

  <ItemGroup>
    <ProjectReference Include="..\..\src\Azure.Mcp.Tools.{Toolset}.csproj" />
    <ProjectReference Include="..\..\..\..\servers\Azure.Mcp.Server\src\Azure.Mcp.Server.csproj" />
  </ItemGroup>
</Project>
```

**Common issue:** Referencing only the toolset project instead of the server project causes "MCP server process exited unexpectedly" / "azmcp.exe not found" errors.

---

## Reference: Best Practices Summary

### Command Structure

- Make command classes `sealed`
- Use primary constructors with `ILogger`, service, and `ISubscriptionResolver`
- Inherit `SubscriptionCommand<TOptions, TResult>`
- Use flat options POCO with `[Option]` attributes (no hierarchy)
- Handle all exceptions with `HandleException`
- Include `CancellationToken` as final argument in all async methods
- Register commands as singletons in `ConfigureServices`

### Error Handling

- Return `HttpStatusCode.BadRequest` for validation errors
- Return `HttpStatusCode.Unauthorized` for authentication failures
- Return `HttpStatusCode.InternalServerError` for unexpected errors
- Return service-specific status codes from `RequestFailedException`
- Add troubleshooting URL to error messages
- Log errors with context, override `GetErrorMessage`/`GetStatusCode` for custom handling

### Response Format

- Always set `Results` property for success
- Set `Status` and `Message` for errors
- Use consistent JSON property names
- Commands returning arrays return `[]` if service returned null/empty

### Documentation

- Clear command description without repeating service name
  - ✅ "List and manage clusters"
  - ❌ "AKS operations - List and manage AKS clusters"
- List all required options in description
- Describe return format
- Maintain alphabetical sorting in `e2eTestPrompts.md`

### Live Test Infrastructure

- Use minimal resource configurations for cost efficiency
- Follow naming conventions: `baseName` (most common)
- Include proper RBAC assignments for test application
- Output all necessary identifiers for test consumption
- Use appropriate Azure service API versions
- Consider resource location constraints and availability

---

## Reference: Remote MCP Server Considerations

Commands must be **transport-agnostic** — they work identically in stdio (local) and HTTP (remote) modes with multiple concurrent users.

### Authentication Strategies

Azure MCP Server supports two outgoing auth strategies in remote HTTP mode:

**1. On-Behalf-Of (OBO) Flow:**
- Client authenticates user with Entra ID, sends bearer token
- Server exchanges user's token for downstream Azure service tokens
- Each API call uses user's identity and RBAC permissions
- Use for: per-user authorization, multi-tenant, audit trails

**2. Hosting Environment Identity:**
- Server uses its own identity (Managed Identity, Service Principal)
- All downstream calls use server's credentials
- All users share server's permission level
- Use for: simplified deployment, single-tenant, service-level permissions

**Command Implementation:** No command code changes needed! `IAzureTokenCredentialProvider` handles the strategy automatically:
```csharp
// Works in ALL modes — OBO, hosting identity, and stdio
var credential = await _tokenCredentialProvider.GetTokenCredentialAsync(tenant, cancellationToken);
```

### Transport-Agnostic Design Rules

**✅ DO:**
```csharp
// Authentication provider handles all transport scenarios
var armClient = await CreateArmClientAsync(tenant, retryPolicy, cancellationToken: cancellationToken);

// Options are pre-bound by framework, no shared state
// (In ExecuteAsync, 'options' parameter is already bound)

// Service calls are async and don't store request state
var results = await _service.ListAsync(options.Subscription!, cancellationToken: cancellationToken);
```

**❌ DON'T:**
```csharp
// ❌ Don't check environment for transport type
if (Environment.GetEnvironmentVariable("ASPNETCORE_URLS") != null) { }

// ❌ Don't access HttpContext directly in commands
var httpContext = _httpContextAccessor.HttpContext;

// ❌ Don't store per-request state in instance fields
private CommandContext? _currentContext;  // Race condition!
private MyOptions? _currentOptions;      // Race condition!
```

### Multi-User Concurrency Requirements

- All commands must be **stateless** and **thread-safe**
- Don't store per-request state in command instance fields
- Use constructor injection for **singleton services only**
- Per-request data flows through `CommandContext` and options binding

### Tenant Context Handling

```csharp
public async Task<List<Resource>> GetResourcesAsync(
    string subscription, string? tenant, RetryPolicyOptions? retryPolicy,
    CancellationToken cancellationToken)
{
    // ITenantService handles tenant resolution for all modes:
    // - OBO mode: Validates tenant matches user's token
    // - Hosting environment: Uses provided tenant or default
    // - Stdio mode: Uses Azure CLI/VS Code default tenant
    var credential = await GetCredential(tenant, cancellationToken);
    var armClient = new ArmClient(credential);
    // ...
}
```

### Error Handling for Remote Scenarios

```csharp
protected override string GetErrorMessage(Exception ex) => ex switch
{
    RequestFailedException reqEx when reqEx.Status == 401 =>
        "Authentication failed. In remote mode, ensure your token has the required " +
        "Mcp.Tools.ReadWrite scope and sufficient RBAC permissions on Azure resources.",

    RequestFailedException reqEx when reqEx.Status == 403 =>
        "Authorization failed. Your user account lacks the required RBAC permissions. " +
        "In remote mode with OBO flow, permissions come from the authenticated user's identity.",

    InvalidOperationException invEx when invEx.Message.Contains("tenant") =>
        "Tenant mismatch. In remote OBO mode, the requested tenant must match your " +
        "authenticated user's tenant ID.",

    _ => base.GetErrorMessage(ex)
};
```

### Consolidated Mode Requirements

Every new command must be added to consolidated mode:

1. File: `servers/Azure.Mcp.Server/src/Resources/consolidated-tools.json`
2. Add new commands to the best matching category and exact matching `toolMetadata`
3. Update existing consolidated tool descriptions where newly mapped tools are added
4. If no matching category exists, suggest a new consolidated tool
5. Find correct tool name:
   ```powershell
   cd servers/Azure.Mcp.Server/src/bin/Debug/net10.0
   ./azmcp[.exe] tools list --name --namespace <tool_area>
   ```

### Testing Commands for Remote Mode

When writing tests, consider both transport modes:

**Unit Tests** (Always Required):
- Mock all external dependencies
- Test command logic in isolation
- No Azure resources required
- Fast execution

**Live Tests** (Required for Azure Service Commands):
- Test against real Azure resources
- Verify Azure SDK integration
- Validate RBAC permissions
- Test works identically in both stdio and HTTP modes (commands are transport-agnostic)

Live tests inherently cover both modes because commands use `IAzureTokenCredentialProvider` which handles credential acquisition differently per mode. No separate "remote mode" test is needed — the same test validates both.

### Documentation for Remote Mode

When documenting commands in `azmcp-commands.md`, include permissions section:

```markdown
### Permissions

**Stdio Mode:** Requires authenticated Azure identity (Azure CLI, VS Code, Managed Identity)
**Remote HTTP Mode (OBO):** Requires `Mcp.Tools.ReadWrite` scope + user's RBAC
**Remote HTTP Mode (Hosting Environment):** Requires `Mcp.Tools.ReadWrite` scope + server's RBAC
```
