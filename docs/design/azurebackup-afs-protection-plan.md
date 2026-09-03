# Azure Files (AFS) Backup — MCP Enablement Plan

Author: Azure Backup MCP • Baseline: `upstream/main` @ latest (as of this document)
Toolset: `tools/Azure.Mcp.Tools.AzureBackup`

## 1. Problem statement

End-to-end enablement of Azure Files (AFS) backup on a Recovery Services Vault (RSV) requires four ARM operations that the Azure Backup MCP toolset does not yet expose. Today, `azmcp azurebackup protecteditem protect` implicitly relies on the storage account already being registered as a `StorageContainer` in the RSV and only calls `InquireAsync` before configuring protection. Because there is no register / refresh / protectable-share discovery, protection of a "fresh" storage account fails with a 404 on the container (`BackupProtectionContainerResource`) and the user has no way to remediate through MCP.

Per the [Azure Files backup overview](https://learn.microsoft.com/azure/backup/azure-file-share-backup-overview) the mandatory ARM flow is:

1. Vault discovers storage accounts that can be registered — `POST .../backupFabrics/Azure/refreshContainers`.
2. Vault lists candidate storage accounts (protectable containers, not-yet-registered) — `GET .../backupFabrics/Azure/protectableContainers`.
3. Storage account is registered as a `StorageContainer` — `PUT .../backupFabrics/Azure/protectionContainers/{containerName}`.
4. File shares under the registered container are inquired — `POST .../protectionContainers/{containerName}/inquire`.
5. File shares are listed as `AzureFileShare` protectable items — `GET .../backupProtectableItems?$filter=backupManagementType eq 'AzureStorage'`.
6. `protecteditem protect` PUT with `FileshareProtectedItem` (this step exists today).

Steps 1–5 are the gap. Step 4 is a partial implementation inside `protecteditem protect`, but it is not idempotent and lacks a preflight check for container registration.

## 2. Existing coverage vs. gap

| Stage | Purpose | Existing MCP tool | Gap |
|-------|---------|-------------------|-----|
| Refresh containers | Trigger vault-side discovery of storage accounts across the subscription | none | **New tool** |
| List available (protectable) containers | Show storage accounts the vault can register | none | **New tool** |
| Get container registration status | Verify whether a storage account is already registered (idempotency) | none | **New tool** (returned by container list, but a scoped `get` is needed for a single account) |
| Register storage account container | Register a storage account with the RSV as `AzureStorage/StorageContainer` | none | **New tool** |
| Inquire container | Discover file shares under a registered container | inline inside `protecteditem protect` (fire-and-forget with `Task.Delay(5000)`) | **Promote to a dedicated tool** so it is observable and re-runnable |
| List protectable file shares | List `AzureFileShare` protectable items after inquire | `protectableitem list` supports `--workload-type AzureFileShare` via `NormalizeWorkloadTypeForFilter`, but only when a container is already registered and inquired | **Extend, not new tool** (see §5) |
| Enable AFS protection | Configure backup on a specific file share | `protecteditem protect` with `--datasource-type AzureFileShare` | Already implemented; will be updated once new tools land (remove `Task.Delay`, reuse dedicated inquire) |

The user's gap list maps 1:1 to the "New tool" rows.

## 3. Proposed tools (one PR each)

Each tool ships as its own PR per the repo's "one tool per PR" contract. Commands live under the existing `azurebackup container` group (new group) except for the promoted inquire, which extends the existing `protectableitem` group.

### PR 1 — `azmcp azurebackup container refresh` (BLOCKING)

- **Purpose**: Trigger the RSV to (re)discover storage accounts eligible for registration. Underlying ARM: `POST /backupFabrics/{fabric}/refreshContainers`.
- **SDK**: `ResourceGroupResource.RefreshProtectionContainerAsync(vaultName, fabricName, filter, ct)`.
- **Options**:
  - `--vault` (required), `--resource-group` (required), `--subscription` (required), `--tenant`, `--vault-type` (only `rsv` supported — see §4 error contract).
  - `--filter` (optional): passed through as the ARM `$filter` (default `backupManagementType eq 'AzureStorage'` to scope to file share flows).
- **Result shape**:
  ```json
  { "status": "Accepted", "vault": "…", "fabric": "Azure", "message": "…" }
  ```
- **Behavior notes**:
  - The API returns 202 with no body; treat any 2xx as success.
  - LRO polling is intentionally NOT surfaced (Azure PowerShell parity); message tells the caller to run `container list-available` next.
  - Metadata: `Destructive=false, Idempotent=true, ReadOnly=false, LocalRequired=false, OpenWorld=false, Secret=false`.

### PR 2 — `azmcp azurebackup container list-available` (BLOCKING)

- **Purpose**: List storage accounts the vault can register but has not yet registered. Underlying ARM: `GET /backupFabrics/{fabric}/protectableContainers`.
- **SDK**: `ResourceGroupResource.GetProtectableContainersAsync(vaultName, fabricName, filter, ct)`.
- **Options**:
  - Base options; `--vault-type` restricted to `rsv`.
  - `--filter` (optional): default `backupManagementType eq 'AzureStorage'`.
  - `--storage-account` (optional): client-side filter that limits results to a single storage account name or ARM ID.
- **Result shape**: array of `ProtectableContainerInfo { name, friendlyName, containerType, backupManagementType, sourceResourceId, healthStatus }`.
- **Metadata**: `ReadOnly=true, Idempotent=true, Destructive=false`.

### PR 3 — `azmcp azurebackup container get` (BLOCKING — required for idempotency)

- **Purpose**: Report whether a specific storage account is already registered with the RSV so callers (and agents) can skip PR 4 when unnecessary.
- **SDK**:
  - Single lookup: `ResourceGroupResource.GetBackupProtectionContainerAsync(vaultName, fabricName, containerName, ct)` — returns 404 when not registered.
  - Listing: `ResourceGroupResource.GetBackupProtectionContainersAsync(vaultName, filter, ct)`.
- **Options**:
  - Base options.
  - Either `--container` (fabric container name, e.g. `StorageContainer;Storage;{rg};{account}`) OR `--storage-account` (ARM ID or account name — MCP derives the container name via existing `RsvNamingHelper.DeriveContainerName`).
  - `--filter` (optional): default `backupManagementType eq 'AzureStorage'` when listing.
- **Result shape**:
  ```json
  {
    "registered": true,
    "container": {
      "name": "StorageContainer;Storage;my-rg;mysa",
      "friendlyName": "mysa",
      "sourceResourceId": "/subscriptions/…/storageAccounts/mysa",
      "registrationStatus": "Registered",
      "healthStatus": "Healthy",
      "protectedItemCount": 3
    }
  }
  ```
- **404 handling**: return `{ "registered": false, "container": null }` with HTTP status 200 so the tool never surfaces an "error" for the not-registered case — this is the key idempotency signal.

### PR 4 — `azmcp azurebackup container register` (BLOCKING)

- **Purpose**: Register a storage account as an `AzureStorage/StorageContainer`. Underlying ARM: `PUT /backupFabrics/{fabric}/protectionContainers/{containerName}` with body:
  ```json
  {
    "properties": {
      "backupManagementType": "AzureStorage",
      "containerType": "StorageContainer",
      "sourceResourceId": "/subscriptions/…/storageAccounts/{account}",
      "acquireStorageAccountLock": "Acquire"
    }
  }
  ```
- **SDK**: `BackupProtectionContainerCollection.CreateOrUpdateAsync(WaitUntil.Started, containerName, BackupProtectionContainerData)` where properties are an `AzureStorageContainer` model.
- **Options**:
  - Base options.
  - `--storage-account` (required): ARM ID or bare account name. If bare, MCP resolves it under the same `--resource-group` as the vault (documented explicitly).
  - `--acquire-lock` (optional bool, default `true`) — maps to `AcquireStorageAccountLock` (`Acquire`/`NotAcquire`).
- **Two-call LRO** (per `AGENTS.md`): `CreateOrUpdateAsync(WaitUntil.Started, …)` + `WaitForLroCompletionAsync`.
- **Idempotency**: If `GetBackupProtectionContainerAsync` succeeds before the PUT, return early with `{ status: "Succeeded", alreadyRegistered: true }`.
- **Result shape**:
  ```json
  {
    "status": "Succeeded",
    "container": { "name": "…", "friendlyName": "…", "sourceResourceId": "…", "registrationStatus": "Registered" },
    "alreadyRegistered": false,
    "message": "Storage account 'mysa' registered with vault 'my-rsv'. Run 'azurebackup protectableitem inquire' next."
  }
  ```
- **Unsupported-scenario preflight** — reject with `ArgumentException` mapped to HTTP 400 (see §4):
  - Storage account has hierarchical namespace enabled (ADLS Gen2) — resolve via `Microsoft.Storage/storageAccounts` GET, check `isHnsEnabled`.
  - Storage account is not GPv2 or FileStorage (snapshot tier accepts GPv1 too; validate against tier only if we can determine tier from context — otherwise defer to the service and pass through its error).
  - Storage account key access is disabled (`allowSharedKeyAccess=false`) — required for both tiers per the support matrix (source SA must have "Allow storage account key access" enabled).
  - Attempted registration when the account is already registered with a different vault (409/`RegistrationFailedException`) — surface as HTTP 409 with actionable message pointing the user to the existing vault.
- **Metadata**: `Destructive=true` (creates state on the vault), `Idempotent=true`, `ReadOnly=false`.

### PR 5 — `azmcp azurebackup protectableitem inquire` (BLOCKING for AFS discovery UX; also useful for SQL/HANA)

- **Purpose**: Trigger container inquiry so file shares (or workload databases) become listable via `protectableitem list`. Underlying ARM: `POST /protectionContainers/{containerName}/inquire`.
- **SDK**: `BackupProtectionContainerResource.InquireAsync(filter, ct)` (already used inline in `RsvBackupOperations.ProtectItemAsync`).
- **Rationale for a dedicated tool**: The current `Task.Delay(5000)` embedded in `protecteditem protect` is not observable and races on cold containers. A dedicated tool makes the two-step flow explicit and lets agents retry inquire without re-attempting protection.
- **Options**:
  - Base options.
  - Either `--container` OR `--storage-account` (mutually exclusive with `--container`); when `--storage-account` is provided, MCP derives the container name.
  - `--workload-type` (optional): passed through as `$filter=workloadType eq '…'`. Defaults to unfiltered.
- **Behavior**:
  - Returns 202. Treat 202/2xx as success.
  - 404 → surface as `KeyNotFoundException` with hint to run `container register` first.
  - 409 → return `Accepted` with message `"Inquiry already in progress"` (matches existing swallow behavior but observable now).
- **Result shape**:
  ```json
  { "status": "Accepted", "container": "…", "message": "Inquiry started. Run 'azurebackup protectableitem list --workload-type AzureFileShare' to enumerate discovered items." }
  ```
- **Follow-up cleanup (same PR)**: Remove the `Task.Delay(5000)` from `RsvBackupOperations.ProtectItemAsync` and call the new `InquireAsync` service method directly, then poll `GetBackupProtectableItemsAsync` up to 30s for the target share to become listable before submitting the protect PUT. This closes the race without a fixed sleep.

### PR 6 (optional, follow-up) — `protectableitem list` enhancement

Not a "new tool" but a follow-up to make the AFS flow feel complete: extend `ProtectableItemListCommand` to accept `--vault-type rsv` explicitly and document that `--workload-type AzureFileShare` requires a prior `container register` + `protectableitem inquire`. No new options; only doc/description update. Bundled in PR 5's PR body or shipped as its own micro-PR.

## 4. Unsupported scenarios & error contract

All new tools MUST follow the toolset's existing `GetErrorMessage`/`GetStatusCode` conventions (see `azurebackup-detailed-analysis.md` / `AzureBackupServiceExceptions`). Specific mappings:

| Scenario | ARM signal | MCP response |
|----------|-----------|--------------|
| Vault type is DPP | detected in `Options.VaultType` | 400 `ArgumentException` — "AFS backup uses RSV. Pass `--vault-type rsv`. For DPP-based vaulted AFS backup, use the RSV workload family; DPP does not host AFS today." |
| NFS file share | The share list will show NFS type when inquire runs; MCP does not preflight file share protocol. Any `protecteditem protect` on an NFS share will fail server-side. | 400 (from service) surfaced verbatim with hint: "Azure Backup does not support NFS file shares. See Azure Files backup support matrix." |
| Storage account with HNS enabled (ADLS Gen2) | preflight in PR 4 via `Microsoft.Storage/storageAccounts` GET | 400 `ArgumentException` — "Storage account '{name}' has hierarchical namespace enabled (ADLS Gen2). AFS backup requires a standard storage account." |
| `allowSharedKeyAccess=false` | preflight in PR 4 | 400 — "Storage account '{name}' must have shared key access enabled for Azure Files backup. Enable 'Allow storage account key access' and retry." |
| Storage account already registered to a different vault | 409 `ProtectionContainerAlreadyRegistered` / `AzureFileShareContainerAlreadyRegistered` | 409 — actionable message including the owning vault ID from the error payload when present. |
| Storage account registered to same vault | preflight via `container get` | HTTP 200 with `alreadyRegistered=true` (never an error). |
| Container not registered when inquire is called | 404 | 404 — "Container '{name}' is not registered. Run `azurebackup container register --storage-account …` first." |
| Vault tier region not supported | preflight best-effort: check vault location against the vault-tier region allowlist ONLY when `--policy` maps to a vault-tier AFS policy; otherwise pass through service error | 400 with the allowlist URL from the support matrix. |
| Region disallowed for snapshot tier | Germany Central (Sovereign), Germany Northeast (Sovereign), China East, China North, France South, US Gov Iowa | 400 preflight in `container register` when vault region is in this list. |
| Cross-subscription AFS backup for snapshot tier | Not supported for snapshot; supported for vaulted only | Deferred: MCP does not currently model tier at registration time. Document in tool description; no preflight. |
| Rate limits (50 registrations/vault/day, 25 inquiries/day, 50 discoveries/day, 200 file shares/vault/day) | 429 / `TooManyRequests` from service | 429 passed through with per-limit guidance from the support matrix. |
| Vault redundancy = LRS + CRR request downstream | not this PR family | out of scope. |

Preflight validation for storage account properties in PR 4 uses `Azure.ResourceManager.Storage` (already a transitive dependency via `Azure.ResourceManager`). No new NuGet references.

## 5. Files to add / modify (per PR)

### Common (PR 4 introduces, later PRs consume)

- `src/Commands/Container/` — new folder.
- `src/Options/Container/` — new folder.
- `src/Models/ProtectableContainerInfo.cs`, `ProtectionContainerInfo.cs`, `ContainerRegisterResult.cs`, `ContainerRefreshResult.cs`, `InquireResult.cs`.
- Extend `AzureBackupOptionDefinitions.cs`: `--storage-account`, `--filter`, `--acquire-lock`.
- Extend `AzureBackupJsonContext.cs` with the new result records.
- Extend `IAzureBackupService` / `AzureBackupService` with `RefreshContainersAsync`, `ListAvailableContainersAsync`, `GetContainerAsync`, `RegisterContainerAsync`, `InquireContainerAsync`.
- Extend `IRsvBackupOperations` / `RsvBackupOperations` with the corresponding SDK-facing methods.
- Register commands in `AzureBackupSetup.cs` under a new `container` sub-group and add `protectableitem inquire` under the existing `protectableitem` sub-group.

### Per-PR summary

| PR | New command file | New options file | New model | Service method | Setup change |
|----|------------------|------------------|-----------|----------------|--------------|
| 1  | `Container/ContainerRefreshCommand.cs` | `Container/ContainerRefreshOptions.cs` | `ContainerRefreshResult` | `RefreshContainersAsync` | new `container` group + `refresh` |
| 2  | `Container/ContainerListAvailableCommand.cs` | `Container/ContainerListAvailableOptions.cs` | `ProtectableContainerInfo` | `ListAvailableContainersAsync` | add `list-available` |
| 3  | `Container/ContainerGetCommand.cs` | `Container/ContainerGetOptions.cs` | `ProtectionContainerInfo` | `GetContainerAsync` | add `get` |
| 4  | `Container/ContainerRegisterCommand.cs` | `Container/ContainerRegisterOptions.cs` | `ContainerRegisterResult` | `RegisterContainerAsync` + storage-account preflight helper | add `register` |
| 5  | `ProtectableItem/ProtectableItemInquireCommand.cs` | `ProtectableItem/ProtectableItemInquireOptions.cs` | `InquireResult` | `InquireContainerAsync`; remove `Task.Delay` in `ProtectItemAsync` | add `inquire` to `protectableitem` group |

## 6. Documentation & registration (per PR checklist)

Every PR MUST update:

- `servers/Azure.Mcp.Server/Resources/consolidated-tools.json` — add the new tool ID to `get_azure_backup_details` (PRs 2, 3), `create_azure_backup_resources` (PR 4), `update_azure_backup_settings` (PRs 1, 5).
- `servers/Azure.Mcp.Server/docs/e2eTestPrompts.md` — add 2–3 alphabetical prompts per new tool (e.g. *"refresh backup containers in vault my-rsv"*, *"register storage account mysa with backup vault my-rsv"*, *"list file shares I can back up in vault my-rsv"*).
- `servers/Azure.Mcp.Server/docs/azmcp-commands.md` — add command syntax and options table for each new command.
- `tools/Azure.Mcp.Tools.AzureBackup/README.md` — add "Azure Files backup" section covering the end-to-end flow.
- `servers/Azure.Mcp.Server/changelog-entries/*.yaml` — one entry per PR under `Added` section, referencing the PR number.
- ToolDescriptionEvaluator score ≥ `0.4` for each new tool description; recorded in the PR body per `.github/copilot-instructions.md`.

## 7. Unit tests (per PR)

Extend `tools/Azure.Mcp.Tools.AzureBackup/tests/Azure.Mcp.Tools.AzureBackup.UnitTests/` with a folder mirroring the command layout.

| PR | Test class | Coverage |
|----|------------|----------|
| 1 | `Container/ContainerRefreshCommandTests` | success, RSV-only enforcement, filter passthrough, error mapping (403 → 403, 404 vault → 404), option binding, JSON round-trip. |
| 2 | `Container/ContainerListAvailableCommandTests` | success (empty + populated), client-side `--storage-account` filter, error mapping, AOT-safe JSON. |
| 3 | `Container/ContainerGetCommandTests` | 404 → `{ registered: false }`, 200 → `{ registered: true, container: … }`, both `--container` and `--storage-account` inputs, mutually-exclusive validation. |
| 4 | `Container/ContainerRegisterCommandTests` | happy path, `alreadyRegistered=true` short-circuit, HNS/allowSharedKeyAccess preflight rejections, 409 `ProtectionContainerAlreadyRegistered` mapping, `--acquire-lock` toggling, storage-account name resolution under vault RG, LRO two-call pattern (`WaitForLroCompletionAsync` invoked). |
| 5 | `ProtectableItem/ProtectableItemInquireCommandTests` | 404 → hint to register, 409 → `Accepted` + "already in progress", options mutual-exclusivity, `--workload-type` filter passthrough. Also update `ProtectedItemProtectCommandTests` to assert `Task.Delay` no longer runs (via injected time provider) and inquire is called through the service. |

All test classes extend the appropriate `CommandUnitTestsBase` and follow the existing `NSubstitute` patterns already used in the toolset. Reminder from repo memory: use `Arg.Any<CancellationToken>()` (not `default`) in `.DidNotReceiveWithAnyArgs()` calls to avoid xUnit1051 build breaks.

## 8. Live tests (recorded, per `docs/recorded-tests.md`)

Live tests extend `tools/Azure.Mcp.Tools.AzureBackup/tests/Azure.Mcp.Tools.AzureBackup.LiveTests/AzureBackupCommandTests.cs` (single class, ~50 methods today). Each PR adds recorded tests scoped to the new tool and updates `assets.json` with the new `Tag` after `test-proxy push`.

### 8.1 Bicep infrastructure additions (`tests/test-resources.bicep`)

- Add a **Standard GPv2** storage account (name derived from `${baseName}sa`) with:
  - `kind = 'StorageV2'`, `sku = 'Standard_LRS'`
  - `properties.allowSharedKeyAccess = true`
  - `properties.isHnsEnabled = false`
  - `properties.publicNetworkAccess = 'Enabled'`
  - `properties.networkAcls.bypass = 'AzureServices'` (required per support matrix)
- Add one Azure File Share child resource: `${storageAccount.name}/default/testshare` (Standard tier, 100 GiB quota).
- Add a **negative** storage account `${baseName}saHns` with `isHnsEnabled = true` to exercise the HNS preflight failure test (PR 4).
- Grant the test app the `Storage Account Contributor` role on the storage accounts (needed for registration).
- Emit the storage account ARM IDs, the file share name, and the RG in Bicep outputs so tests can retrieve them via `Settings.DeploymentOutputs`.

### 8.2 Post-deployment script (`tests/test-resources-post.ps1`)

No new steps required — Azure Backup registration is exercised by the tests themselves. Ensure any existing script does not pre-register the SA (would break the "cold" flow tests).

### 8.3 Live test methods per PR

Every test starts with `AssertLocalToolIsUnavailableInHttpMode(toolName)` where applicable (all these tools are transport-agnostic and are NOT `LocalRequired`, so this helper returns false and the test runs). Class-level `CustomDefaultMatcher` and sanitizers already handle correlation IDs and RG name replacement — no new sanitizers needed except confirming storage account names are sanitized (add a `GeneralRegexSanitizer` for the deployed SA name → `sanitizedsa`).

| PR | Test method(s) |
|----|----------------|
| 1 | `ContainerRefresh_RsvVault_TriggersDiscovery_Successfully` — call `container refresh`, assert 200/Accepted. |
| 2 | `ContainerListAvailable_RsvVault_ListsProtectableContainers_Successfully` — assert the deployed SA appears in the list after a preceding refresh; `ContainerListAvailable_RsvVault_FiltersByStorageAccount_Successfully` — client-side filter works. |
| 3 | `ContainerGet_UnregisteredStorageAccount_ReturnsRegisteredFalse` — happy path when no registration exists; `ContainerGet_RegisteredStorageAccount_ReturnsMetadata` — after PR 4's register test runs and populates the vault, this one records the "already registered" path. |
| 4 | `ContainerRegister_RsvVault_RegistersStorageAccount_Successfully` — main flow: refresh → list-available → register → get returns `registered=true`. `ContainerRegister_AlreadyRegistered_ReturnsAlreadyRegistered` — second call to register short-circuits. `ContainerRegister_HnsAccount_Rejected` — uses the negative SA from Bicep, asserts 400 with the HNS message. `ContainerRegister_SharedKeyDisabled_Rejected` — flip `allowSharedKeyAccess=false` on the SA via `test-resources-post.ps1` conditional branch (or `az storage account update` inside the test using the deployment outputs) then attempt to register. |
| 5 | `ProtectableItemInquire_RegisteredContainer_ReturnsAccepted` — happy path against the SA registered in PR 4's recording. `ProtectableItemInquire_UnregisteredContainer_Returns404WithHint`. `ProtectableItemList_AfterInquire_ListsFileShare` — asserts the deployed `testshare` becomes visible when `--workload-type AzureFileShare` is passed. `ProtectedItemProtect_ColdStorageAccount_EndToEnd` — regression test: `container register` → `protectableitem inquire` → `protecteditem protect` succeeds without the old `Task.Delay`. |

### 8.4 Recording workflow (per PR)

Follow `docs/recorded-tests.md` and the repo-memory checklist in `/memories/repo/azurebackup-recording-workflow.md`:

1. `az login` with the test tenant/subscription (recording mode requires live Azure).
2. Deploy test resources: `eng/common/TestResources/New-TestResources.ps1 -TestResourcesDirectory tools/Azure.Mcp.Tools.AzureBackup`.
3. Set env: `TEST_MODE=Record`; run only the newly added test method(s) with `dotnet test --filter "FullyQualifiedName~<method>"`.
4. Inspect the produced JSON under the assets repo working directory; verify sanitizers scrubbed the storage account name and RG.
5. `test-proxy push -a tools/Azure.Mcp.Tools.AzureBackup/tests/Azure.Mcp.Tools.AzureBackup.LiveTests/assets.json` — captures a new `Tag` in `assets.json`.
6. Commit the updated `assets.json` in the same PR.
7. Re-run in `Playback` mode locally and in CI to confirm the recording is stable.

### 8.5 Skip guidance

- Do NOT skip any of the new tests in Playback mode; recordings must be complete.
- If a preflight test (e.g. `SharedKeyDisabled_Rejected`) proves flaky against a real SA due to eventual consistency, re-record only that method and pin a note in `AzureBackupCommandTests.cs` describing why.

## 9. Rollout order & branching

Recommended merge order (each PR is independently green):

1. **PR 1** `container refresh` — no dependencies.
2. **PR 3** `container get` — no dependencies; unlocks idempotency for PR 4 tests.
3. **PR 2** `container list-available` — no dependencies.
4. **PR 4** `container register` — depends on PRs 1 & 3 for the end-to-end live-test recording (needs list-available + get pre/post).
5. **PR 5** `protectableitem inquire` + `protecteditem protect` cleanup — depends on PR 4 (needs a registered container for its main live test).

Branch naming: `user/azurebackup-afs-<step>` (e.g. `user/azurebackup-afs-container-refresh`). Each branch cuts from `upstream/main` after the previous PR merges to avoid overlapping `AzureBackupSetup.cs` changes.

## 10. Out of scope (explicit non-goals)

- **DPP-based AFS**: DPP vaulted Azure Files is not exposed by Azure Backup MCP today; when it becomes GA in the SDK, a follow-up plan will cover it.
- **Unregister container / stop protection with `deleteBackupData=true` on containers**: covered by existing `protecteditem` operations plus a future `container unregister` PR (not part of this batch).
- **AFS restore**: `restore` operations for AFS require a separate design; not in scope here.
- **Vaulted AFS policy variants beyond what `PolicyCreate` already supports**: policy creation for AFS is already implemented — this plan strictly enables the *pre-protect* discovery/registration flow.

## 11. Acceptance criteria

Each PR is merge-ready when:

- `./eng/scripts/Build-Local.ps1 -VerifyNpx` passes.
- `dotnet format` produces zero diff.
- `.\eng\common\spelling\Invoke-Cspell.ps1` passes.
- New/updated unit tests all pass locally; `TreatWarningsAsErrors=true` clean.
- Live tests pass in Playback mode with the new `assets.json` `Tag` committed.
- `consolidated-tools.json`, `e2eTestPrompts.md`, `azmcp-commands.md`, README, and changelog entry are updated in the same PR.
- ToolDescriptionEvaluator score ≥ 0.4 for the new tool, reported in the PR body.
- Copilot-authored PR body includes the standard *"Invoking Livetests"* footer per `.github/copilot-instructions.md`.
