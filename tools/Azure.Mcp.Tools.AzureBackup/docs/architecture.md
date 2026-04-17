# Azure Backup MCP Toolset — Architecture

## Overview

The Azure Backup MCP toolset (`Azure.Mcp.Tools.AzureBackup`) provides AI agents with unified access to Azure Backup operations across **two distinct vault platforms**:

| Platform | Vault Type | SDK | Flag |
|----------|-----------|-----|------|
| **Recovery Services Vault (RSV)** | `Microsoft.RecoveryServices/vaults` | `Azure.ResourceManager.RecoveryServicesBackup` | `--vault-type rsv` |
| **Data Protection / Backup Vault (DPP)** | `Microsoft.DataProtection/backupVaults` | `Azure.ResourceManager.DataProtectionBackup` | `--vault-type dpp` |

Every command exposes a single, vault-agnostic entry point (e.g., `azmcp azurebackup vault get`). Under the hood, the service layer routes the call to the correct RSV or DPP implementation based on the `--vault-type` flag—or auto-detects the vault type when omitted.

---

## Project Structure

```
Azure.Mcp.Tools.AzureBackup/
├── src/
│   ├── AzureBackupSetup.cs                      # DI registration & command tree
│   ├── Commands/
│   │   ├── AzureBackupJsonContext.cs             # AOT-safe JSON serialization
│   │   ├── BaseAzureBackupCommand.cs             # Base for vault-scoped commands
│   │   ├── BaseProtectedItemCommand.cs           # Base for protected-item commands
│   │   ├── Backup/
│   │   │   └── BackupStatusCommand.cs
│   │   ├── Dr/
│   │   │   └── DrEnableCrrCommand.cs
│   │   ├── Governance/
│   │   │   ├── GovernanceFindUnprotectedCommand.cs
│   │   │   ├── GovernanceImmutabilityCommand.cs
│   │   │   └── GovernanceSoftDeleteCommand.cs
│   │   ├── Job/
│   │   │   └── JobGetCommand.cs
│   │   ├── Policy/
│   │   │   ├── PolicyCreateCommand.cs
│   │   │   └── PolicyGetCommand.cs
│   │   ├── ProtectableItem/
│   │   │   └── ProtectableItemListCommand.cs
│   │   ├── ProtectedItem/
│   │   │   ├── ProtectedItemGetCommand.cs
│   │   │   └── ProtectedItemProtectCommand.cs
│   │   ├── RecoveryPoint/
│   │   │   └── RecoveryPointGetCommand.cs
│   │   └── Vault/
│   │       ├── VaultCreateCommand.cs
│   │       ├── VaultGetCommand.cs
│   │       └── VaultUpdateCommand.cs
│   ├── Models/                                   # Immutable DTOs (sealed records)
│   │   ├── BackupJobInfo.cs
│   │   ├── BackupPolicyInfo.cs
│   │   ├── BackupStatusResult.cs
│   │   ├── BackupVaultInfo.cs
│   │   ├── OperationResult.cs
│   │   ├── ProtectableItemInfo.cs
│   │   ├── ProtectedItemInfo.cs
│   │   ├── ProtectResult.cs
│   │   ├── RecoveryPointInfo.cs
│   │   ├── UnprotectedResourceInfo.cs
│   │   └── VaultCreateResult.cs
│   ├── Options/                                  # Option definitions & binding
│   │   ├── AzureBackupOptionDefinitions.cs
│   │   ├── BaseAzureBackupOptions.cs
│   │   ├── BaseProtectedItemOptions.cs
│   │   └── [per-command Options classes]
│   └── Services/                                 # Core service layer
│       ├── IAzureBackupService.cs                # Unified facade interface
│       ├── AzureBackupService.cs                 # Facade: routing + auto-detect
│       ├── VaultTypeResolver.cs                  # "rsv" / "dpp" validation helpers
│       ├── IDppBackupOperations.cs               # DPP operations interface
│       ├── DppBackupOperations.cs                # DPP SDK implementation
│       ├── DppDatasourceProfile.cs               # Profile record + enums
│       ├── DppDatasourceRegistry.cs              # Registry of all DPP workloads
│       ├── IRsvBackupOperations.cs               # RSV operations interface
│       ├── RsvBackupOperations.cs                # RSV SDK implementation
│       ├── RsvDatasourceProfile.cs               # Profile record + enums
│       └── RsvDatasourceRegistry.cs              # Registry of all RSV workloads
└── tests/
    ├── test-resources.bicep                      # Bicep: deploys RSV + DPP vaults
    ├── test-resources-post.ps1                   # Post-deployment settings script
    ├── Azure.Mcp.Tools.AzureBackup.LiveTests/    # Recorded integration tests
    └── Azure.Mcp.Tools.AzureBackup.UnitTests/    # Unit tests (mocked services)
```

---

## Layered Architecture

The toolset follows a strict three-layer architecture:

```
┌──────────────────────────────────────────────────────────────────┐
│                        MCP Command Layer                         │
│  (VaultGetCommand, PolicyCreateCommand, JobGetCommand, etc.)     │
│  Sealed command classes → option binding → call service → return │
└──────────────────────────┬───────────────────────────────────────┘
                           │
                           ▼
┌──────────────────────────────────────────────────────────────────┐
│                    Unified Service Facade                         │
│                      AzureBackupService                          │
│   • Routes to RSV or DPP based on --vault-type                   │
│   • Auto-detects vault type when not specified                   │
│   • Merges results from both platforms (e.g., ListVaults)        │
└──────────┬───────────────────────────────────┬───────────────────┘
           │                                   │
           ▼                                   ▼
┌────────────────────────┐     ┌────────────────────────────┐
│   RsvBackupOperations  │     │   DppBackupOperations      │
│ (IRsvBackupOperations) │     │ (IDppBackupOperations)     │
│                        │     │                            │
│ Azure.ResourceManager  │     │ Azure.ResourceManager      │
│ .RecoveryServicesBackup│     │ .DataProtectionBackup      │
│                        │     │                            │
│ ┌────────────────────┐ │     │ ┌──────────────────────┐   │
│ │ RsvDatasourceReg.  │ │     │ │ DppDatasourceReg.    │   │
│ │ RsvDatasourceProf. │ │     │ │ DppDatasourceProf.   │   │
│ └────────────────────┘ │     │ └──────────────────────┘   │
└────────────────────────┘     └────────────────────────────┘
```

### Layer 1: Commands

Each command is a **sealed** class that:
1. Inherits from `BaseAzureBackupCommand<TOptions>` (vault-scoped) or `BaseProtectedItemCommand<TOptions>` (adds `--protected-item` and `--container`).
2. Registers required/optional options via `RegisterOptions`.
3. Binds parsed CLI values to a typed options record via `BindOptions`.
4. Calls the unified `IAzureBackupService` method.
5. Wraps the result in a `ResponseResult` using the AOT-safe `AzureBackupJsonContext`.

Base command hierarchy:

```
SubscriptionCommand<T>           ← from MCP Core (handles --subscription, --tenant)
  └── BaseAzureBackupCommand<T>  ← adds --vault, --resource-group, --vault-type
        └── BaseProtectedItemCommand<T>  ← adds --protected-item, --container
```

### Layer 2: Unified Service Facade (`AzureBackupService`)

`AzureBackupService` implements `IAzureBackupService` and acts as a **strategy router**:

- **Explicit vault type**: When `--vault-type` is `rsv` or `dpp`, the call is dispatched directly to the corresponding operations object.
- **Auto-detection**: When `--vault-type` is omitted, the service first tries RSV, then DPP (catching 404s). For `ListVaults`, both platforms are queried in parallel and results are merged.
- **Vault-type resolution** (`ResolveVaultTypeAsync`): Used by most methods — attempts RSV first, falls back to DPP, caches nothing (stateless for multi-user safety).

Key methods:
| Method | Behavior |
|--------|----------|
| `ListVaultsAsync` | Parallel RSV + DPP when no vault type specified; merges results |
| `GetVaultAsync` | Auto-detect via try-RSV-then-DPP when vault type omitted |
| `FindUnprotectedResourcesAsync` | Cross-platform governance: enumerates all RSV+DPP protected items, lists all protectable ARM resources, returns the unprotected delta |
| `GetBackupStatusAsync` | RSV-only API (checks datasource protection status) |
| `ListProtectableItemsAsync` | RSV-only (routed directly to RSV operations) |

### Layer 3: Platform Operations & Datasource Registries

Each platform has a dedicated operations class and a **datasource registry**:

#### RSV Operations (`RsvBackupOperations`)

Uses `Azure.ResourceManager.RecoveryServicesBackup` SDK. The `RsvDatasourceRegistry` maps workload types to `RsvDatasourceProfile` records containing:

- SDK polymorphic type selectors (`RsvProtectedItemType`, `RsvPolicyType`, `RsvBackupContentType`, `RsvRestoreContentType`)
- Container naming prefixes
- Discovery requirements (container registration, inquiry, discovery)
- Auto-protection support

#### DPP Operations (`DppBackupOperations`)

Uses `Azure.ResourceManager.DataProtectionBackup` SDK. The `DppDatasourceRegistry` maps workload types to `DppDatasourceProfile` records containing:

- ARM resource type strings
- User-friendly aliases
- Store type (Operational vs Vault)
- Schedule and retention defaults
- Snapshot RG requirements
- `DataSourceSetMode`, `BackupParametersMode`, `InstanceNamingMode`
- Continuous backup flag (for Blob, ADLS, CosmosDB)
- Auto-detect base resource type remapping

##### Adding a new workload is purely data-driven

For **DPP**, you just add a new `DppDatasourceProfile` instance to `DppDatasourceRegistry.AllProfiles`.
For **RSV**, you add a new `RsvDatasourceProfile` instance to `RsvDatasourceRegistry.AllProfiles`.
No if/else branching changes are needed in the operations classes.

---

## Vault Type Resolution

The `VaultTypeResolver` static class provides:

| Method | Purpose |
|--------|---------|
| `IsRsv(vaultType)` | Case-insensitive check for "rsv" |
| `IsDpp(vaultType)` | Case-insensitive check for "dpp" |
| `ValidateVaultType(vaultType)` | Throws if null/empty or invalid (used by create) |
| `IsVaultTypeSpecified(vaultType)` | Returns false for null/empty, throws for invalid values |

Auto-detection flow in `AzureBackupService`:

```
User omits --vault-type
       │
       ▼
ResolveVaultTypeAsync()
  ├── Try rsvOps.GetVaultAsync()  →  Success? Return "rsv"
  │        └── 404? Continue
  └── Try dppOps.GetVaultAsync()  →  Success? Return "dpp"
           └── 404? Throw KeyNotFoundException
```

---

## Tools (Commands) Reference

The toolset exposes **15 commands** organized in **9 command groups**:

### Vault Operations (`azurebackup vault`)

| Command | MCP Tool Name | Description |
|---------|--------------|-------------|
| `get` | `azurebackup_vault_get` | Get single vault details or list all vaults (RSV + DPP) |
| `create` | `azurebackup_vault_create` | Create a new RSV or DPP vault |
| `update` | `azurebackup_vault_update` | Update vault properties (tags, redundancy, soft-delete, immutability, identity) |

### Policy Operations (`azurebackup policy`)

| Command | MCP Tool Name | Description |
|---------|--------------|-------------|
| `get` | `azurebackup_policy_get` | Get single policy or list all policies in a vault |
| `create` | `azurebackup_policy_create` | Create a backup policy for a workload type with schedule and retention rules |

### Protected Item Operations (`azurebackup protecteditem`)

| Command | MCP Tool Name | Description |
|---------|--------------|-------------|
| `get` | `azurebackup_protecteditem_get` | Get protected item details or list all protected items |
| `protect` | `azurebackup_protecteditem_protect` | Enable backup protection on a datasource with a policy |

### Protectable Item Operations (`azurebackup protectableitem`)

| Command | MCP Tool Name | Description |
|---------|--------------|-------------|
| `list` | `azurebackup_protectableitem_list` | List discovered databases/items available for protection (RSV only) |

### Backup Operations (`azurebackup backup`)

| Command | MCP Tool Name | Description |
|---------|--------------|-------------|
| `status` | `azurebackup_backup_status` | Check backup protection status for a datasource by ARM ID |

### Job Operations (`azurebackup job`)

| Command | MCP Tool Name | Description |
|---------|--------------|-------------|
| `get` | `azurebackup_job_get` | Get job details by ID or list all jobs in a vault |

### Recovery Point Operations (`azurebackup recoverypoint`)

| Command | MCP Tool Name | Description |
|---------|--------------|-------------|
| `get` | `azurebackup_recoverypoint_get` | Get recovery point details or list all recovery points for a protected item |

### Governance Operations (`azurebackup governance`)

| Command | MCP Tool Name | Description |
|---------|--------------|-------------|
| `find-unprotected` | `azurebackup_governance_find-unprotected` | Scan subscription for resources lacking backup (cross-platform) |
| `immutability` | `azurebackup_governance_immutability` | Configure vault immutability (Disabled / Enabled / Locked) |
| `soft-delete` | `azurebackup_governance_soft-delete` | Configure vault soft-delete (AlwaysOn / On / Off) |

### Disaster Recovery Operations (`azurebackup dr`)

| Command | MCP Tool Name | Description |
|---------|--------------|-------------|
| `enablecrr` | `azurebackup_dr_enablecrr` | Enable Cross-Region Restore on a GRS vault |

---

## Workloads Supported

### Recovery Services Vault (RSV) Workloads

| Workload | Profile Name | Aliases | ARM Type | Notes |
|----------|-------------|---------|----------|-------|
| Azure Virtual Machines | `VM` | vm, iaasvm, virtualmachine | `Microsoft.Compute/virtualMachines` | Default workload; requires container discovery |
| SQL Database on VM | `SQL` | sql, sqldatabase, mssql, sqldb | In-guest workload | Requires container registration + inquiry; supports auto-protect |
| SAP HANA on VM | `SAPHANA` | saphana, saphanadatabase, hana | In-guest workload | Requires container registration + inquiry |
| SAP ASE on VM | `SAPASE` | sapase, ase, sybase | In-guest workload | Requires container registration + inquiry |
| Azure File Share | `AzureFileShare` | azurefileshare, fileshare, afs | `Microsoft.Storage/storageAccounts` | StorageContainer naming |

### Backup Vault (DPP) Workloads

| Workload | Profile Name | Aliases | ARM Type | Store | Backup Mode |
|----------|-------------|---------|----------|-------|-------------|
| Azure Disk | `AzureDisk` | azuredisk, disk | `Microsoft.Compute/disks` | Operational | Incremental (4-hourly) |
| Azure Blob Storage | `AzureBlob` | azureblob, blob | `.../storageAccounts/blobServices` | Operational | Continuous |
| Azure Kubernetes Service | `AKS` | aks, kubernetes | `Microsoft.ContainerService/managedClusters` | Operational | Incremental (4-hourly) |
| Elastic SAN | `ElasticSAN` | elasticsan, esan | `.../elasticSans/volumeGroups` | Operational | Incremental (daily) |
| PostgreSQL Flexible Server | `PostgreSQLFlexible` | pgflex, postgresql | `Microsoft.DBforPostgreSQL/flexibleServers` | Vault | Full (daily) |
| Azure Data Lake Storage | `AzureDataLakeStorage` | adls, datalake | `.../storageAccounts/blobServices` | Operational | Continuous |
| Azure Cosmos DB | `CosmosDB` | cosmosdb, cosmos | `Microsoft.DocumentDB/databaseAccounts` | Operational | Continuous |

### Cross-Platform Protectable Resource Types

The `FindUnprotectedResourcesAsync` governance tool scans for all of these ARM resource types:

- `Microsoft.Compute/virtualMachines`
- `Microsoft.Storage/storageAccounts`
- `Microsoft.DBforPostgreSQL/flexibleServers`
- `Microsoft.ContainerService/managedClusters`
- `Microsoft.Compute/disks`
- `Microsoft.ElasticSan/elasticSans`
- `Microsoft.DocumentDB/databaseAccounts`

---

## Data Models

All models are **sealed records** (immutable, AOT-safe):

| Model | Description |
|-------|-------------|
| `BackupVaultInfo` | Vault metadata: name, type, location, SKU, storage type, tags |
| `VaultCreateResult` | Vault creation result: ID, name, type, location, provisioning state |
| `BackupPolicyInfo` | Policy metadata: name, vault type, datasource types, protected items count |
| `ProtectedItemInfo` | Protected item: name, vault type, status, datasource info, policy, last backup |
| `ProtectableItemInfo` | Discoverable items (RSV): type, workload, server, container |
| `ProtectResult` | Protection operation result: status, item name, job ID |
| `BackupJobInfo` | Job details: operation, status, times, datasource info |
| `RecoveryPointInfo` | Recovery point: time, type |
| `BackupStatusResult` | Datasource protection status: policy, health, last backup |
| `UnprotectedResourceInfo` | Unprotected resource: ID, type, RG, location, tags |
| `OperationResult` | Generic operation result: status, job ID, message |

---

## AOT Safety & JSON Serialization

All response types are registered in `AzureBackupJsonContext`:

```csharp
[JsonSerializable(typeof(VaultGetCommand.VaultGetCommandResult))]
[JsonSerializable(typeof(PolicyCreateCommand.PolicyCreateCommandResult))]
// ... all 15 command result types + 11 model types
[JsonSourceGenerationOptions(
    PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase,
    DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingDefault)]
internal sealed partial class AzureBackupJsonContext : JsonSerializerContext { }
```

Each command defines a nested result record type (e.g., `VaultGetCommandResult`) that wraps the model data.

---

## Dependency Injection & Setup

`AzureBackupSetup` implements `IAreaSetup` and:
1. Registers services as singletons: `IRsvBackupOperations`, `IDppBackupOperations`, `IAzureBackupService`
2. Registers all 15 command classes as singletons
3. Builds the command group tree under the `azurebackup` namespace:

```
azurebackup/
├── vault/          (get, create, update)
├── policy/         (get, create)
├── protecteditem/  (get, protect)
├── protectableitem/ (list)
├── backup/         (status)
├── job/            (get)
├── recoverypoint/  (get)
├── governance/     (find-unprotected, immutability, soft-delete)
└── dr/             (enablecrr)
```

---

## Option Definitions

All option names are centralized in `AzureBackupOptionDefinitions` as `const string` fields, with corresponding `Option<string>` static instances. This ensures:
- No hardcoded option strings in commands
- Consistent descriptions across all commands
- Reusable options via `.AsRequired()` / `.AsOptional()` extension methods

Key shared options:
- `--vault` — Vault name
- `--vault-type` — `rsv` or `dpp` (optional with auto-detect)
- `--resource-group` — Azure resource group
- `--subscription` — Subscription (inherited from base)
- `--protected-item` — Protected item / backup instance name
- `--container` — RSV container name (RSV-only)

---

## Error Handling

Commands implement `GetErrorMessage` and `GetStatusCode` overrides:

```csharp
protected override string GetErrorMessage(Exception ex) => ex switch
{
    RequestFailedException { Status: 404 } => "Resource not found...",
    RequestFailedException { Status: 403 } => "Authorization failed...",
    KeyNotFoundException knfEx => knfEx.Message,  // from auto-detect failure
    ArgumentException argEx => argEx.Message,     // from VaultTypeResolver
    _ => base.GetErrorMessage(ex)
};
```

All catch blocks call `HandleException(context, ex)` which uses these overrides to populate the response.

---

## Live Tests

### Infrastructure

Live tests use the **recorded test** pattern via `RecordedCommandTestsBase`. Test infrastructure is defined in:

- **`test-resources.bicep`** — Deploys both an RSV vault (`{baseName}-rsv`) and a DPP vault (`{baseName}-dpp`) with:
  - Backup Contributor role assignments for the test application
  - Soft-delete disabled on DPP vault (for clean teardown)
- **`test-resources-post.ps1`** — Generates test settings file

### Test Coverage

The live test class `AzureBackupCommandTests` contains **20 recorded tests** covering both RSV and DPP:

| Category | Tests | What's Validated |
|----------|-------|------------------|
| **Vault (RSV)** | List vaults, get single RSV, create vault, update tags | Vault CRUD operations on RSV |
| **Vault (DPP)** | Get single DPP, update DPP tags | Vault read/update on DPP |
| **Policy (RSV)** | List RSV policies, create AzureVM policy | Policy CRUD on RSV |
| **Policy (DPP)** | List DPP policies, create AzureDisk policy | Policy CRUD on DPP |
| **Protected Items (RSV)** | List protected items | Read-only protected item listing on RSV |
| **Protected Items (DPP)** | List protected items | Read-only protected item listing on DPP |
| **Protectable Items** | List protectable items (RSV) | RSV-only discovery feature |
| **Governance (RSV)** | Soft-delete config, immutability config | RSV vault governance settings |
| **Governance (DPP)** | Soft-delete config, immutability config | DPP vault governance settings |
| **Governance (Subscription)** | Find unprotected resources | Cross-platform subscription scan |
| **Jobs (RSV)** | List jobs | Job listing on RSV vault |
| **Jobs (DPP)** | List jobs | Job listing on DPP vault |
| **DR** | Enable CRR (RSV), enable CRR (DPP) | Cross-region restore on both vault types |

### Recording Configuration

Tests use custom matchers and sanitizers to handle non-deterministic values:
- **Excluded headers**: `Authorization`, `Content-Type`, `x-ms-client-request-id`
- **Body comparison**: Disabled (ARM requests include timestamps, correlation IDs)
- **Body regex sanitizers**: Hostname masking in response URLs

### Unit Tests

**16 unit test classes** with comprehensive coverage:
- Constructor initialization
- Success path for all operations
- Exception handling (generic, 404, 403, 400, 409)
- Input validation (missing required parameters)
- Deserialization validation
- Options binding
- Service method parameter passing
- Registry resolution (DppDatasourceRegistryTests)

---

## Design Principles

1. **Vault-agnostic commands**: Users work with a single command set; vault platform differences are abstracted away.
2. **Data-driven workload support**: Datasource profiles are pure data records — adding a new workload requires only a profile instance, not code changes in operations classes.
3. **AOT safety**: All JSON serialization uses source-generated contexts; no reflection.
4. **Stateless & thread-safe**: Commands store no per-request state; safe for multi-user remote HTTP mode.
5. **Parallel execution**: List operations query both platforms concurrently via `Task.WhenAll`.
6. **Consistent error messages**: Context-aware error messages guide users to fix issues (wrong vault type, missing permissions, etc.).
