# Adding New Tools to the Azure Backup MCP Toolset

This guide walks through adding a new command (tool) to the `Azure.Mcp.Tools.AzureBackup` toolset, as well as adding support for a new workload type.

---

## Table of Contents

1. [Adding a New Command](#1-adding-a-new-command)
2. [Adding a New DPP Workload](#2-adding-a-new-dpp-workload)
3. [Adding a New RSV Workload](#3-adding-a-new-rsv-workload)
4. [Updating Live Tests](#4-updating-live-tests)
5. [Checklist](#5-checklist)

---

## 1. Adding a New Command

Follow these steps to add a new command to an existing command group (e.g., adding `delete` to `vault`) or a new command group.

### Step 1: Create the Options Class

Create a new file under `src/Options/{Group}/`:

```
src/Options/Vault/VaultDeleteOptions.cs
```

```csharp
// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Azure.Mcp.Tools.AzureBackup.Options.Vault;

public class VaultDeleteOptions : BaseAzureBackupOptions
{
    // Add command-specific option properties here.
    // Inherited from BaseAzureBackupOptions: Vault, ResourceGroup, VaultType
    // Inherited from SubscriptionOptions: Subscription, Tenant, RetryPolicy
}
```

If the command operates on a protected item, inherit from `BaseProtectedItemOptions` instead.

### Step 2: Add the Service Interface Method

Add the method signature to `IAzureBackupService`:

```csharp
// In Services/IAzureBackupService.cs
Task<OperationResult> DeleteVaultAsync(
    string vaultName, string resourceGroup, string subscription,
    string? vaultType = null, string? tenant = null,
    RetryPolicyOptions? retryPolicy = null,
    CancellationToken cancellationToken = default);
```

### Step 3: Add the Platform Operations Interface Methods

Add to both `IRsvBackupOperations` and `IDppBackupOperations`:

```csharp
// In Services/IRsvBackupOperations.cs
Task<OperationResult> DeleteVaultAsync(
    string vaultName, string resourceGroup, string subscription,
    string? tenant, RetryPolicyOptions? retryPolicy,
    CancellationToken cancellationToken);

// In Services/IDppBackupOperations.cs
Task<OperationResult> DeleteVaultAsync(
    string vaultName, string resourceGroup, string subscription,
    string? tenant, RetryPolicyOptions? retryPolicy,
    CancellationToken cancellationToken);
```

### Step 4: Implement the Platform Operations

Add implementations to `RsvBackupOperations.cs` and `DppBackupOperations.cs`:

```csharp
// In RsvBackupOperations.cs
public async Task<OperationResult> DeleteVaultAsync(
    string vaultName, string resourceGroup, string subscription,
    string? tenant, RetryPolicyOptions? retryPolicy,
    CancellationToken cancellationToken)
{
    var armClient = await CreateArmClientAsync(tenant, retryPolicy, cancellationToken: cancellationToken);
    // ... RSV-specific SDK calls
    return new OperationResult("Succeeded", null, $"Vault '{vaultName}' deleted.");
}
```

```csharp
// In DppBackupOperations.cs
public async Task<OperationResult> DeleteVaultAsync(
    string vaultName, string resourceGroup, string subscription,
    string? tenant, RetryPolicyOptions? retryPolicy,
    CancellationToken cancellationToken)
{
    var armClient = await CreateArmClientAsync(tenant, retryPolicy, cancellationToken: cancellationToken);
    // ... DPP-specific SDK calls
    return new OperationResult("Succeeded", null, $"Vault '{vaultName}' deleted.");
}
```

### Step 5: Implement the Facade Method

Add routing logic to `AzureBackupService.cs`:

```csharp
public async Task<OperationResult> DeleteVaultAsync(
    string vaultName, string resourceGroup, string subscription,
    string? vaultType, string? tenant,
    RetryPolicyOptions? retryPolicy, CancellationToken cancellationToken)
{
    var resolved = await ResolveVaultTypeAsync(
        vaultName, resourceGroup, subscription, vaultType, tenant, retryPolicy, cancellationToken);

    return VaultTypeResolver.IsRsv(resolved)
        ? await rsvOps.DeleteVaultAsync(vaultName, resourceGroup, subscription, tenant, retryPolicy, cancellationToken)
        : await dppOps.DeleteVaultAsync(vaultName, resourceGroup, subscription, tenant, retryPolicy, cancellationToken);
}
```

### Step 6: Create the Command Class

Create `src/Commands/Vault/VaultDeleteCommand.cs`:

```csharp
// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net;
using System.Text.Json.Serialization;
using Azure.Mcp.Tools.AzureBackup.Models;
using Azure.Mcp.Tools.AzureBackup.Options.Vault;
using Azure.Mcp.Tools.AzureBackup.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Mcp.Core.Commands;
using Microsoft.Mcp.Core.Models.Command;

namespace Azure.Mcp.Tools.AzureBackup.Commands.Vault;

public sealed class VaultDeleteCommand(
    ILogger<VaultDeleteCommand> logger,
    IAzureBackupService azureBackupService) : BaseAzureBackupCommand<VaultDeleteOptions>()
{
    private const string CommandTitle = "Delete Backup Vault";
    private readonly ILogger<VaultDeleteCommand> _logger = logger;
    private readonly IAzureBackupService _azureBackupService = azureBackupService;

    // Generate a new GUID for the command ID
    public override string Id => "xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx";
    public override string Name => "delete";
    public override string Description => "Deletes a backup vault (RSV or DPP).";
    public override string Title => CommandTitle;
    public override ToolMetadata Metadata => new()
    {
        Destructive = true,
        Idempotent = false,
        OpenWorld = false,
        ReadOnly = false,
        LocalRequired = false,
        Secret = false
    };

    public override async Task<CommandResponse> ExecuteAsync(
        CommandContext context, ParseResult parseResult, CancellationToken cancellationToken)
    {
        if (!Validate(parseResult.CommandResult, context.Response).IsValid)
        {
            return context.Response;
        }

        var options = BindOptions(parseResult);

        try
        {
            var result = await _azureBackupService.DeleteVaultAsync(
                options.Vault!,
                options.ResourceGroup!,
                options.Subscription!,
                options.VaultType,
                options.Tenant,
                options.RetryPolicy,
                cancellationToken);

            context.Response.Results = ResponseResult.Create(
                new VaultDeleteCommandResult(result),
                AzureBackupJsonContext.Default.VaultDeleteCommandResult);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting vault. Vault: {Vault}", options.Vault);
            HandleException(context, ex);
        }

        return context.Response;
    }

    protected override string GetErrorMessage(Exception ex) => ex switch
    {
        RequestFailedException { Status: 404 } => "Vault not found.",
        RequestFailedException { Status: 403 } => $"Authorization failed: {ex.Message}",
        KeyNotFoundException knfEx => knfEx.Message,
        ArgumentException argEx => argEx.Message,
        _ => base.GetErrorMessage(ex)
    };

    protected override int GetStatusCode(Exception ex) => ex switch
    {
        RequestFailedException reqEx => reqEx.Status,
        KeyNotFoundException => (int)HttpStatusCode.NotFound,
        ArgumentException => (int)HttpStatusCode.BadRequest,
        _ => base.GetStatusCode(ex)
    };

    public sealed record VaultDeleteCommandResult(
        [property: JsonPropertyName("result")] OperationResult Result);
}
```

### Step 7: Register in JSON Serialization Context

Add to `AzureBackupJsonContext.cs`:

```csharp
[JsonSerializable(typeof(VaultDeleteCommand.VaultDeleteCommandResult))]
```

### Step 8: Register in Setup

Add to `AzureBackupSetup.cs`:

```csharp
// In ConfigureServices:
services.AddSingleton<VaultDeleteCommand>();

// In RegisterCommands (under the existing vault group):
RegisterCommand<VaultDeleteCommand>(serviceProvider, vault);
```

### Step 9: Build and Verify

```powershell
dotnet build tools/Azure.Mcp.Tools.AzureBackup/src
```

---

## Creating a New Command Group

If you need a completely new group (e.g., `restore`):

1. Follow all steps above for the command itself.
2. In `AzureBackupSetup.RegisterCommands`, add a new `CommandGroup`:

```csharp
var restore = new CommandGroup("restore", "Restore operations – Trigger and monitor restore jobs.");
azureBackup.AddSubGroup(restore);
RegisterCommand<RestoreTriggerCommand>(serviceProvider, restore);
```

---

## 2. Adding a New DPP Workload

DPP workloads are **data-driven**. To add support for a new workload:

### Step 1: Define a Profile in `DppDatasourceRegistry.cs`

Add a new `DppDatasourceProfile` static instance:

```csharp
public static readonly DppDatasourceProfile MySqlFlexible = new()
{
    FriendlyName = "MySQLFlexible",
    ArmResourceType = "Microsoft.DBforMySQL/flexibleServers",
    Aliases = ["mysqlflexible", "mysql"],
    UsesOperationalStore = false,          // true = snapshot-based; false = vault store
    IsContinuousBackup = false,            // true for Blob/ADLS/CosmosDB
    ScheduleInterval = "P1D",              // ISO 8601 interval
    BackupType = "Full",                   // "Full" or "Incremental"
    BackupRuleName = "BackupDaily",
    DefaultRetentionDays = 30,
    RequiresSnapshotResourceGroup = false,
    DataSourceSetMode = DppDataSourceSetMode.None,       // None, Self, or Parent
    BackupParametersMode = DppBackupParametersMode.None, // None or KubernetesCluster
    InstanceNamingMode = DppInstanceNamingMode.Standard,  // Standard or ParentChild
    DefaultRestoreMode = DppRestoreMode.RestoreAsFiles,
    SupportsPolicyUpdate = false,
};
```

### Step 2: Register in `AllProfiles`

Add to the `AllProfiles` array:

```csharp
public static readonly DppDatasourceProfile[] AllProfiles =
[
    AzureDisk,
    AzureBlob,
    Aks,
    ElasticSan,
    PostgreSqlFlexible,
    AzureDataLakeStorage,
    CosmosDb,
    MySqlFlexible,   // ← new entry
];
```

### Step 3: (Optional) Add Auto-Detection

If the ARM resource type in the datasource ID differs from user-facing type (like Blob → `blobServices`), set:

```csharp
AutoDetectFromBaseResourceType = "Microsoft.DBforMySQL/flexibleServers",
```

### Step 4: Add to Governance Scan

If the resource should appear in the `FindUnprotectedResourcesAsync` scan, add its ARM type to `s_protectableResourceTypes` in `AzureBackupService.cs`:

```csharp
private static readonly string[] s_protectableResourceTypes =
[
    // ... existing types
    "Microsoft.DBforMySQL/flexibleServers",  // ← add here
];
```

**That's it.** No changes needed in `DppBackupOperations` — the profile data drives all behavior.

---

## 3. Adding a New RSV Workload

RSV workloads also use a registry pattern, but require SDK type mapping:

### Step 1: Define a Profile in `RsvDatasourceRegistry.cs`

```csharp
public static readonly RsvDatasourceProfile NewWorkload = new()
{
    FriendlyName = "NewWorkload",
    Aliases = ["newworkload", "nw"],
    IsWorkloadType = true,                      // true for in-VM workloads (SQL, HANA, ASE)
    ProtectedItemType = RsvProtectedItemType.SqlDatabase,  // which SDK ProtectedItem to construct
    PolicyType = RsvPolicyType.VmWorkload,      // which SDK Policy type to use
    BackupContentType = RsvBackupContentType.Workload,
    RestoreContentType = RsvRestoreContentType.SqlRestore,
    ApiWorkloadType = "NewWorkloadType",        // Azure API workload string
    ContainerNamePrefix = "VMAppContainer;Compute",
    RequiresContainerRegistration = true,
    RequiresContainerInquiry = true,
    SupportsPolicyUpdate = true,
};
```

### Step 2: Register in `AllProfiles`

```csharp
public static readonly RsvDatasourceProfile[] AllProfiles =
[
    IaasVm,
    SqlDatabase,
    SapHanaDatabase,
    SapAse,
    AzureFileShare,
    NewWorkload,   // ← add here
];
```

### Step 3: (Optional) Add New Enum Values

If the workload uses a new SDK type pattern not covered by existing enums, add values to the enums in `RsvDatasourceProfile.cs`:

```csharp
public enum RsvProtectedItemType
{
    IaasVm,
    SqlDatabase,
    SapHanaDatabase,
    AzureFileShare,
    NewWorkloadItem,  // ← if needed
}
```

Then handle the new enum value in `RsvBackupOperations` where the switch dispatches on the profile's type.

---

## 4. Updating Live Tests

### Adding Tests for a New Command

Add test methods to `AzureBackupCommandTests.cs`:

```csharp
[Fact]
public async Task VaultDelete_DeletesVault_Successfully()
{
    var vaultName = RegisterOrRetrieveVariable("deletedVaultName", $"test-del-{Random.Shared.NextInt64()}");

    // Create the vault first
    await CallToolAsync("azurebackup_vault_create", new()
    {
        { "subscription", Settings.SubscriptionId },
        { "resource-group", Settings.ResourceGroupName },
        { "vault", vaultName },
        { "vault-type", "rsv" },
        { "location", "eastus" }
    });

    // Delete it
    var result = await CallToolAsync("azurebackup_vault_delete", new()
    {
        { "subscription", Settings.SubscriptionId },
        { "resource-group", Settings.ResourceGroupName },
        { "vault", vaultName }
    });

    Assert.NotNull(result);
}
```

### Updating Test Infrastructure

If the new command needs additional Azure resources (e.g., a VM to protect), update `test-resources.bicep`:

```bicep
// Add to test-resources.bicep:
resource testVm 'Microsoft.Compute/virtualMachines@2024-03-01' = {
  name: '${baseName}-vm'
  location: location
  // ... VM configuration
}
```

### Adding Unit Tests

Create a test class in `tests/Azure.Mcp.Tools.AzureBackup.UnitTests/{Group}/`:

```csharp
public class VaultDeleteCommandTests
{
    [Fact]
    public void Constructor_InitializesCommandCorrectly()
    {
        var logger = Substitute.For<ILogger<VaultDeleteCommand>>();
        var service = Substitute.For<IAzureBackupService>();
        var command = new VaultDeleteCommand(logger, service);

        Assert.Equal("delete", command.Name);
        Assert.True(command.Metadata.Destructive);
    }

    [Fact]
    public async Task ExecuteAsync_DeletesVault_Successfully()
    {
        var logger = Substitute.For<ILogger<VaultDeleteCommand>>();
        var service = Substitute.For<IAzureBackupService>();
        service.DeleteVaultAsync(Arg.Any<string>(), /* ... */)
            .Returns(new OperationResult("Succeeded", null, "Deleted."));

        var command = new VaultDeleteCommand(logger, service);
        // ... invoke and assert
    }

    [Fact]
    public async Task ExecuteAsync_HandlesNotFound()
    {
        // ... test 404 handling
    }

    [Fact]
    public async Task ExecuteAsync_ValidatesInput()
    {
        // ... test missing required parameters
    }
}
```

### Required Unit Test Patterns

Every command must have these test categories:
- `Constructor_InitializesCommandCorrectly` — name, metadata, description
- `ExecuteAsync_SuccessPath` — mock service returns valid data
- `ExecuteAsync_HandlesException` — generic exception handling
- `ExecuteAsync_HandlesNotFound` (404) — for get/update/delete commands
- `ExecuteAsync_ValidatesInput` — missing required parameters

---

## 5. Checklist

Before submitting a PR for a new tool:

- [ ] **Options class** created under `Options/{Group}/`
- [ ] **Service interface** updated (`IAzureBackupService`)
- [ ] **Platform interfaces** updated (`IRsvBackupOperations`, `IDppBackupOperations`)
- [ ] **Platform implementations** added (`RsvBackupOperations`, `DppBackupOperations`)
- [ ] **Facade method** added to `AzureBackupService` with vault-type routing
- [ ] **Command class** created as `sealed` with primary constructor
- [ ] **Command ID** is a unique GUID
- [ ] **ToolMetadata** correctly set (Destructive, ReadOnly, Idempotent, etc.)
- [ ] **JSON context** updated (`AzureBackupJsonContext.cs`)
- [ ] **Setup file** updated (`AzureBackupSetup.cs`) — both DI and command registration
- [ ] **Unit tests** added with full coverage (constructor, success, errors, validation)
- [ ] **Live tests** added for both RSV and DPP scenarios
- [ ] **Build passes**: `dotnet build tools/Azure.Mcp.Tools.AzureBackup/src`
- [ ] **Format check**: `dotnet format --include="tools/Azure.Mcp.Tools.AzureBackup/**/*.cs"`
- [ ] **Spelling check**: `.\eng\common\spelling\Invoke-Cspell.ps1`
- [ ] **Documentation updated**: `servers/Azure.Mcp.Server/docs/azmcp-commands.md`
- [ ] **Test prompts added**: `servers/Azure.Mcp.Server/docs/e2eTestPrompts.md`
- [ ] **Error handling** includes `GetErrorMessage` and `GetStatusCode` overrides
- [ ] **No per-request state** stored in command instance fields (thread-safety)
- [ ] **CancellationToken** passed through all async call chains

---

## Quick Reference: File Locations

| What | Where |
|------|-------|
| Option definitions | `src/Options/AzureBackupOptionDefinitions.cs` |
| Base options | `src/Options/BaseAzureBackupOptions.cs` |
| Base command | `src/Commands/BaseAzureBackupCommand.cs` |
| Service interface | `src/Services/IAzureBackupService.cs` |
| Service facade | `src/Services/AzureBackupService.cs` |
| RSV operations | `src/Services/RsvBackupOperations.cs` |
| DPP operations | `src/Services/DppBackupOperations.cs` |
| DPP workload registry | `src/Services/DppDatasourceRegistry.cs` |
| RSV workload registry | `src/Services/RsvDatasourceRegistry.cs` |
| JSON context (AOT) | `src/Commands/AzureBackupJsonContext.cs` |
| Setup & DI | `src/AzureBackupSetup.cs` |
| Live tests | `tests/Azure.Mcp.Tools.AzureBackup.LiveTests/` |
| Unit tests | `tests/Azure.Mcp.Tools.AzureBackup.UnitTests/` |
| Test infrastructure | `tests/test-resources.bicep` |
