# Azure Backup MCP — Manual Test Cases (All 15 Tools × RSV + DPP)

> Run these MCP tool calls via an MCP client (VS Code Copilot Chat, Claude Desktop, etc.) connected to the local Azure MCP Server.
>
> **Replace placeholders** before running:
> - `<SUBSCRIPTION>` — Your Azure subscription ID
> - `<RSV_RG>` — Resource group containing your RSV vault
> - `<RSV_VAULT>` — Your Recovery Services vault name
> - `<DPP_RG>` — Resource group containing your DPP vault
> - `<DPP_VAULT>` — Your Backup vault (Data Protection) name
> - `<VM_ARM_ID>` — ARM ID of a VM: `/subscriptions/<SUBSCRIPTION>/resourceGroups/<RSV_RG>/providers/Microsoft.Compute/virtualMachines/<VM_NAME>`
> - `<STORAGE_ARM_ID>` — ARM ID of a storage account: `/subscriptions/<SUBSCRIPTION>/resourceGroups/<DPP_RG>/providers/Microsoft.Storage/storageAccounts/<STORAGE_NAME>`
> - `<RSV_VM_CONTAINER>` — RSV container, e.g. `iaasvmcontainerv2;<RSV_RG>;<VM_NAME>`
> - `<RSV_VM_ITEM>` — RSV protected item name, e.g. `VM;iaasvmcontainerv2;<RSV_RG>;<VM_NAME>`
> - `<DPP_INSTANCE>` — DPP backup instance name (from protecteditem list)
> - `<RSV_POLICY>` — Existing RSV policy name (e.g., `DefaultPolicy`)
> - `<DPP_POLICY>` — Existing DPP policy name
> - `<RSV_JOB_ID>` — A completed RSV job ID (GUID)
> - `<DPP_JOB_ID>` — A completed DPP job ID (GUID or Base64)
> - `<RSV_RP_ID>` — An RSV recovery point ID
> - `<DPP_RP_ID>` — A DPP recovery point ID
> - `<LOCATION>` — Azure region (e.g., `eastus`, `eastasia`)

---

## 1. `azurebackup_vault_get` — List/Get Vaults

### 1a. List all vaults (RSV + DPP combined)
```json
{
  "name": "azurebackup_vault_get",
  "arguments": {
    "subscription": "<SUBSCRIPTION>"
  }
}
```
**Expected**: Returns list of both RSV and DPP vaults.

### 1b. List only RSV vaults
```json
{
  "name": "azurebackup_vault_get",
  "arguments": {
    "subscription": "<SUBSCRIPTION>",
    "vault-type": "rsv"
  }
}
```
**Expected**: Returns only Recovery Services vaults.

### 1c. List only DPP vaults
```json
{
  "name": "azurebackup_vault_get",
  "arguments": {
    "subscription": "<SUBSCRIPTION>",
    "vault-type": "dpp"
  }
}
```
**Expected**: Returns only Backup vaults (Data Protection).

### 1d. Get single RSV vault
```json
{
  "name": "azurebackup_vault_get",
  "arguments": {
    "subscription": "<SUBSCRIPTION>",
    "resource-group": "<RSV_RG>",
    "vault": "<RSV_VAULT>"
  }
}
```
**Expected**: Returns single vault with SKU, location, storageType.

### 1e. Get single DPP vault
```json
{
  "name": "azurebackup_vault_get",
  "arguments": {
    "subscription": "<SUBSCRIPTION>",
    "resource-group": "<DPP_RG>",
    "vault": "<DPP_VAULT>"
  }
}
```

### 1f. Get nonexistent vault (negative)
```json
{
  "name": "azurebackup_vault_get",
  "arguments": {
    "subscription": "<SUBSCRIPTION>",
    "resource-group": "<RSV_RG>",
    "vault": "nonexistent-vault-12345"
  }
}
```
**Expected**: Error — vault not found.

---

## 2. `azurebackup_vault_create` — Create Vault

### 2a. Create RSV vault
```json
{
  "name": "azurebackup_vault_create",
  "arguments": {
    "subscription": "<SUBSCRIPTION>",
    "resource-group": "<RSV_RG>",
    "vault": "test-rsv-manual",
    "vault-type": "rsv",
    "location": "<LOCATION>"
  }
}
```
**Expected**: Succeeded, vaultType=rsv.

### 2b. Create DPP vault
```json
{
  "name": "azurebackup_vault_create",
  "arguments": {
    "subscription": "<SUBSCRIPTION>",
    "resource-group": "<DPP_RG>",
    "vault": "test-dpp-manual",
    "vault-type": "dpp",
    "location": "<LOCATION>"
  }
}
```
**Expected**: Succeeded, vaultType=dpp.

### 2c. Create vault without vault-type (negative)
```json
{
  "name": "azurebackup_vault_create",
  "arguments": {
    "subscription": "<SUBSCRIPTION>",
    "resource-group": "<RSV_RG>",
    "vault": "test-novaulttype",
    "location": "<LOCATION>"
  }
}
```
**Expected**: Error — `--vault-type` is required for create.

---

## 3. `azurebackup_vault_update` — Update Vault

### 3a. Update RSV vault redundancy
```json
{
  "name": "azurebackup_vault_update",
  "arguments": {
    "subscription": "<SUBSCRIPTION>",
    "resource-group": "<RSV_RG>",
    "vault": "<RSV_VAULT>",
    "redundancy": "GeoRedundant"
  }
}
```

### 3b. Update DPP vault tags
```json
{
  "name": "azurebackup_vault_update",
  "arguments": {
    "subscription": "<SUBSCRIPTION>",
    "resource-group": "<DPP_RG>",
    "vault": "<DPP_VAULT>",
    "tags": "{\"env\":\"test\"}"
  }
}
```

### 3c. Update RSV vault soft-delete
```json
{
  "name": "azurebackup_vault_update",
  "arguments": {
    "subscription": "<SUBSCRIPTION>",
    "resource-group": "<RSV_RG>",
    "vault": "<RSV_VAULT>",
    "soft-delete": "On",
    "soft-delete-retention-days": "14"
  }
}
```

---

## 4. `azurebackup_policy_get` — List/Get Policies

### 4a. List all RSV policies
```json
{
  "name": "azurebackup_policy_get",
  "arguments": {
    "subscription": "<SUBSCRIPTION>",
    "resource-group": "<RSV_RG>",
    "vault": "<RSV_VAULT>"
  }
}
```
**Expected**: Returns list of policies with datasourceTypes.

### 4b. List all DPP policies
```json
{
  "name": "azurebackup_policy_get",
  "arguments": {
    "subscription": "<SUBSCRIPTION>",
    "resource-group": "<DPP_RG>",
    "vault": "<DPP_VAULT>"
  }
}
```

### 4c. Get single RSV policy
```json
{
  "name": "azurebackup_policy_get",
  "arguments": {
    "subscription": "<SUBSCRIPTION>",
    "resource-group": "<RSV_RG>",
    "vault": "<RSV_VAULT>",
    "policy": "<RSV_POLICY>"
  }
}
```

### 4d. Get single DPP policy
```json
{
  "name": "azurebackup_policy_get",
  "arguments": {
    "subscription": "<SUBSCRIPTION>",
    "resource-group": "<DPP_RG>",
    "vault": "<DPP_VAULT>",
    "policy": "<DPP_POLICY>"
  }
}
```

### 4e. Get nonexistent policy (negative)
```json
{
  "name": "azurebackup_policy_get",
  "arguments": {
    "subscription": "<SUBSCRIPTION>",
    "resource-group": "<RSV_RG>",
    "vault": "<RSV_VAULT>",
    "policy": "NonexistentPolicy999"
  }
}
```
**Expected**: Error — policy not found.

---

## 5. `azurebackup_policy_create` — Create Policy

### 5a. Create RSV VM policy
```json
{
  "name": "azurebackup_policy_create",
  "arguments": {
    "subscription": "<SUBSCRIPTION>",
    "resource-group": "<RSV_RG>",
    "vault": "<RSV_VAULT>",
    "policy": "TestVMPolicy",
    "workload-type": "AzureVM",
    "schedule-frequency": "Daily",
    "schedule-time": "02:00",
    "daily-retention-days": "30"
  }
}
```

### 5b. Create DPP Blob policy
```json
{
  "name": "azurebackup_policy_create",
  "arguments": {
    "subscription": "<SUBSCRIPTION>",
    "resource-group": "<DPP_RG>",
    "vault": "<DPP_VAULT>",
    "policy": "TestBlobPolicy",
    "workload-type": "AzureBlob",
    "daily-retention-days": "30"
  }
}
```

### 5c. Create DPP Disk policy
```json
{
  "name": "azurebackup_policy_create",
  "arguments": {
    "subscription": "<SUBSCRIPTION>",
    "resource-group": "<DPP_RG>",
    "vault": "<DPP_VAULT>",
    "policy": "TestDiskPolicy",
    "workload-type": "AzureDisk",
    "schedule-frequency": "Daily",
    "schedule-time": "02:00",
    "daily-retention-days": "7"
  }
}
```

### 5d. Create RSV FileShare policy
```json
{
  "name": "azurebackup_policy_create",
  "arguments": {
    "subscription": "<SUBSCRIPTION>",
    "resource-group": "<RSV_RG>",
    "vault": "<RSV_VAULT>",
    "policy": "TestFSPolicy",
    "workload-type": "AzureFileShare",
    "schedule-frequency": "Daily",
    "schedule-time": "03:00",
    "daily-retention-days": "14"
  }
}
```

---

## 6. `azurebackup_protecteditem_get` — List/Get Protected Items

### 6a. List all RSV protected items
```json
{
  "name": "azurebackup_protecteditem_get",
  "arguments": {
    "subscription": "<SUBSCRIPTION>",
    "resource-group": "<RSV_RG>",
    "vault": "<RSV_VAULT>"
  }
}
```
**Expected**: List of VMs, SQL databases, etc. with protectionStatus.

### 6b. List all DPP backup instances
```json
{
  "name": "azurebackup_protecteditem_get",
  "arguments": {
    "subscription": "<SUBSCRIPTION>",
    "resource-group": "<DPP_RG>",
    "vault": "<DPP_VAULT>"
  }
}
```

### 6c. Get single RSV protected item
```json
{
  "name": "azurebackup_protecteditem_get",
  "arguments": {
    "subscription": "<SUBSCRIPTION>",
    "resource-group": "<RSV_RG>",
    "vault": "<RSV_VAULT>",
    "protected-item": "<RSV_VM_ITEM>",
    "container": "<RSV_VM_CONTAINER>"
  }
}
```

### 6d. Get single DPP backup instance
```json
{
  "name": "azurebackup_protecteditem_get",
  "arguments": {
    "subscription": "<SUBSCRIPTION>",
    "resource-group": "<DPP_RG>",
    "vault": "<DPP_VAULT>",
    "protected-item": "<DPP_INSTANCE>"
  }
}
```

---

## 7. `azurebackup_protecteditem_protect` — Enable Protection

### 7a. Protect VM with RSV
```json
{
  "name": "azurebackup_protecteditem_protect",
  "arguments": {
    "subscription": "<SUBSCRIPTION>",
    "resource-group": "<RSV_RG>",
    "vault": "<RSV_VAULT>",
    "datasource-id": "<VM_ARM_ID>",
    "policy": "<RSV_POLICY>"
  }
}
```
**Expected**: Returns jobId, status=Accepted/InProgress.

### 7b. Protect Blob storage with DPP
```json
{
  "name": "azurebackup_protecteditem_protect",
  "arguments": {
    "subscription": "<SUBSCRIPTION>",
    "resource-group": "<DPP_RG>",
    "vault": "<DPP_VAULT>",
    "datasource-id": "<STORAGE_ARM_ID>",
    "policy": "<DPP_POLICY>",
    "datasource-type": "AzureBlob"
  }
}
```

### 7c. Protect without policy (negative)
```json
{
  "name": "azurebackup_protecteditem_protect",
  "arguments": {
    "subscription": "<SUBSCRIPTION>",
    "resource-group": "<RSV_RG>",
    "vault": "<RSV_VAULT>",
    "datasource-id": "<VM_ARM_ID>"
  }
}
```
**Expected**: Error — `--policy` is required.

---

## 8. `azurebackup_protectableitem_list` — List Protectable Items (RSV only)

### 8a. List all protectable items
```json
{
  "name": "azurebackup_protectableitem_list",
  "arguments": {
    "subscription": "<SUBSCRIPTION>",
    "resource-group": "<RSV_RG>",
    "vault": "<RSV_VAULT>"
  }
}
```
**Expected**: SQL/SAP HANA databases in registered containers (may be empty if no SQL VMs registered).

### 8b. List with SQL workload filter
```json
{
  "name": "azurebackup_protectableitem_list",
  "arguments": {
    "subscription": "<SUBSCRIPTION>",
    "resource-group": "<RSV_RG>",
    "vault": "<RSV_VAULT>",
    "workload-type": "SQLDataBase"
  }
}
```

### 8c. List protectable items against DPP vault (negative)
```json
{
  "name": "azurebackup_protectableitem_list",
  "arguments": {
    "subscription": "<SUBSCRIPTION>",
    "resource-group": "<DPP_RG>",
    "vault": "<DPP_VAULT>"
  }
}
```
**Expected**: Empty list or error (RSV-only feature).

---

## 9. `azurebackup_backup_status` — Check Backup Status

### 9a. Check status of a protected VM
```json
{
  "name": "azurebackup_backup_status",
  "arguments": {
    "subscription": "<SUBSCRIPTION>",
    "datasource-id": "<VM_ARM_ID>",
    "location": "<LOCATION>"
  }
}
```
**Expected**: protectionStatus=Protected, vaultId, policyName.

### 9b. Check status of a protected storage account (DPP)
```json
{
  "name": "azurebackup_backup_status",
  "arguments": {
    "subscription": "<SUBSCRIPTION>",
    "datasource-id": "<STORAGE_ARM_ID>",
    "location": "<LOCATION>"
  }
}
```

### 9c. Check status of an unprotected resource
```json
{
  "name": "azurebackup_backup_status",
  "arguments": {
    "subscription": "<SUBSCRIPTION>",
    "datasource-id": "/subscriptions/<SUBSCRIPTION>/resourceGroups/<RSV_RG>/providers/Microsoft.Compute/virtualMachines/unprotected-vm-12345",
    "location": "<LOCATION>"
  }
}
```
**Expected**: protectionStatus=NotProtected or null vaultId.

---

## 10. `azurebackup_job_get` — List/Get Jobs

### 10a. List all RSV jobs
```json
{
  "name": "azurebackup_job_get",
  "arguments": {
    "subscription": "<SUBSCRIPTION>",
    "resource-group": "<RSV_RG>",
    "vault": "<RSV_VAULT>"
  }
}
```
**Expected**: List of jobs with operation, status, startTime.

### 10b. List all DPP jobs
```json
{
  "name": "azurebackup_job_get",
  "arguments": {
    "subscription": "<SUBSCRIPTION>",
    "resource-group": "<DPP_RG>",
    "vault": "<DPP_VAULT>"
  }
}
```

### 10c. Get single RSV job
```json
{
  "name": "azurebackup_job_get",
  "arguments": {
    "subscription": "<SUBSCRIPTION>",
    "resource-group": "<RSV_RG>",
    "vault": "<RSV_VAULT>",
    "job": "<RSV_JOB_ID>"
  }
}
```

### 10d. Get single DPP job
```json
{
  "name": "azurebackup_job_get",
  "arguments": {
    "subscription": "<SUBSCRIPTION>",
    "resource-group": "<DPP_RG>",
    "vault": "<DPP_VAULT>",
    "job": "<DPP_JOB_ID>"
  }
}
```

### 10e. Get nonexistent job (negative)
```json
{
  "name": "azurebackup_job_get",
  "arguments": {
    "subscription": "<SUBSCRIPTION>",
    "resource-group": "<RSV_RG>",
    "vault": "<RSV_VAULT>",
    "job": "00000000-0000-0000-0000-000000000000"
  }
}
```
**Expected**: Error — job not found (404).

---

## 11. `azurebackup_recoverypoint_get` — List/Get Recovery Points

### 11a. List RSV recovery points
```json
{
  "name": "azurebackup_recoverypoint_get",
  "arguments": {
    "subscription": "<SUBSCRIPTION>",
    "resource-group": "<RSV_RG>",
    "vault": "<RSV_VAULT>",
    "protected-item": "<RSV_VM_ITEM>",
    "container": "<RSV_VM_CONTAINER>"
  }
}
```
**Expected**: List of recovery points with time and type.

### 11b. List DPP recovery points
```json
{
  "name": "azurebackup_recoverypoint_get",
  "arguments": {
    "subscription": "<SUBSCRIPTION>",
    "resource-group": "<DPP_RG>",
    "vault": "<DPP_VAULT>",
    "protected-item": "<DPP_INSTANCE>"
  }
}
```

### 11c. Get single RSV recovery point
```json
{
  "name": "azurebackup_recoverypoint_get",
  "arguments": {
    "subscription": "<SUBSCRIPTION>",
    "resource-group": "<RSV_RG>",
    "vault": "<RSV_VAULT>",
    "protected-item": "<RSV_VM_ITEM>",
    "container": "<RSV_VM_CONTAINER>",
    "recovery-point": "<RSV_RP_ID>"
  }
}
```

### 11d. Get single DPP recovery point
```json
{
  "name": "azurebackup_recoverypoint_get",
  "arguments": {
    "subscription": "<SUBSCRIPTION>",
    "resource-group": "<DPP_RG>",
    "vault": "<DPP_VAULT>",
    "protected-item": "<DPP_INSTANCE>",
    "recovery-point": "<DPP_RP_ID>"
  }
}
```

---

## 12. `azurebackup_governance_find-unprotected` — Find Unprotected Resources

### 12a. All unprotected resources
```json
{
  "name": "azurebackup_governance_find-unprotected",
  "arguments": {
    "subscription": "<SUBSCRIPTION>"
  }
}
```

### 12b. Filter by resource type (VMs only)
```json
{
  "name": "azurebackup_governance_find-unprotected",
  "arguments": {
    "subscription": "<SUBSCRIPTION>",
    "resource-type-filter": "Microsoft.Compute/virtualMachines"
  }
}
```

### 12c. Filter by resource group
```json
{
  "name": "azurebackup_governance_find-unprotected",
  "arguments": {
    "subscription": "<SUBSCRIPTION>",
    "resource-group-filter": "<RSV_RG>"
  }
}
```

---

## 13. `azurebackup_governance_immutability` — Configure Immutability

### 13a. Enable immutability on RSV
```json
{
  "name": "azurebackup_governance_immutability",
  "arguments": {
    "subscription": "<SUBSCRIPTION>",
    "resource-group": "<RSV_RG>",
    "vault": "<RSV_VAULT>",
    "immutability-state": "Unlocked"
  }
}
```
**Expected**: Succeeded. (RSV uses Unlocked/Locked, not Enabled/Disabled.)

### 13b. Enable immutability on DPP
```json
{
  "name": "azurebackup_governance_immutability",
  "arguments": {
    "subscription": "<SUBSCRIPTION>",
    "resource-group": "<DPP_RG>",
    "vault": "<DPP_VAULT>",
    "immutability-state": "Unlocked"
  }
}
```

> ⚠️ **Do NOT test** `immutability-state: "Locked"` — this is **irreversible**.

---

## 14. `azurebackup_governance_soft-delete` — Configure Soft Delete

### 14a. Set soft delete on RSV (AlwaysOn)
```json
{
  "name": "azurebackup_governance_soft-delete",
  "arguments": {
    "subscription": "<SUBSCRIPTION>",
    "resource-group": "<RSV_RG>",
    "vault": "<RSV_VAULT>",
    "soft-delete": "AlwaysOn",
    "soft-delete-retention-days": "14"
  }
}
```

### 14b. Set soft delete on DPP (On)
```json
{
  "name": "azurebackup_governance_soft-delete",
  "arguments": {
    "subscription": "<SUBSCRIPTION>",
    "resource-group": "<DPP_RG>",
    "vault": "<DPP_VAULT>",
    "soft-delete": "On",
    "soft-delete-retention-days": "30"
  }
}
```

### 14c. Disable soft delete on RSV
```json
{
  "name": "azurebackup_governance_soft-delete",
  "arguments": {
    "subscription": "<SUBSCRIPTION>",
    "resource-group": "<RSV_RG>",
    "vault": "<RSV_VAULT>",
    "soft-delete": "Off"
  }
}
```

---

## 15. `azurebackup_dr_enablecrr` — Enable Cross-Region Restore

### 15a. Enable CRR on RSV (requires GRS vault)
```json
{
  "name": "azurebackup_dr_enablecrr",
  "arguments": {
    "subscription": "<SUBSCRIPTION>",
    "resource-group": "<RSV_RG>",
    "vault": "<RSV_VAULT>"
  }
}
```
**Expected**: Succeeded (vault must have GRS redundancy).

### 15b. Enable CRR on DPP vault
```json
{
  "name": "azurebackup_dr_enablecrr",
  "arguments": {
    "subscription": "<SUBSCRIPTION>",
    "resource-group": "<DPP_RG>",
    "vault": "<DPP_VAULT>"
  }
}
```
**Expected**: Error/NotSupported (CRR is RSV-only for most regions).

### 15c. Enable CRR on non-GRS vault (negative)
```json
{
  "name": "azurebackup_dr_enablecrr",
  "arguments": {
    "subscription": "<SUBSCRIPTION>",
    "resource-group": "<RSV_RG>",
    "vault": "some-lrs-vault"
  }
}
```
**Expected**: Error — CRR requires GRS redundancy.

---

## Test Execution Order (Recommended)

| Phase | Tests | Prerequisites |
|-------|-------|---------------|
| 1 | 1a–1f | Existing vaults |
| 2 | 2a–2c | — (creates new vaults) |
| 3 | 3a–3c | Vault from phase 1 or 2 |
| 4 | 4a–4e | Vault with policies |
| 5 | 5a–5d | Vault from phase 1 |
| 6 | 14a–14c | Vault from phase 1 |
| 7 | 13a–13b | Vault from phase 1 |
| 8 | 7a–7c | Vault + policy + unprotected resource |
| 9 | 6a–6d | Protected items from phase 8 |
| 10 | 9a–9c | Protected + unprotected resources |
| 11 | 10a–10e | Jobs from protection in phase 8 |
| 12 | 11a–11d | Protected items with at least 1 backup |
| 13 | 8a–8c | RSV vault with SQL container (optional) |
| 14 | 12a–12c | Subscription with mixed protected/unprotected |
| 15 | 15a–15c | GRS-enabled RSV vault |

---

## Summary

| # | Tool | RSV Tests | DPP Tests | Negative | Total |
|---|------|-----------|-----------|----------|-------|
| 1 | `vault_get` | 2 | 2 | 1 | 6 |
| 2 | `vault_create` | 1 | 1 | 1 | 3 |
| 3 | `vault_update` | 2 | 1 | 0 | 3 |
| 4 | `policy_get` | 2 | 2 | 1 | 5 |
| 5 | `policy_create` | 2 | 2 | 0 | 4 |
| 6 | `protecteditem_get` | 2 | 2 | 0 | 4 |
| 7 | `protecteditem_protect` | 1 | 1 | 1 | 3 |
| 8 | `protectableitem_list` | 2 | 0 | 1 | 3 |
| 9 | `backup_status` | 1 | 1 | 1 | 3 |
| 10 | `job_get` | 2 | 2 | 1 | 5 |
| 11 | `recoverypoint_get` | 2 | 2 | 0 | 4 |
| 12 | `find-unprotected` | — | — | 0 | 3 |
| 13 | `immutability` | 1 | 1 | 0 | 2 |
| 14 | `soft-delete` | 2 | 1 | 0 | 3 |
| 15 | `dr_enablecrr` | 1 | 0 | 2 | 3 |
| | **Total** | **20** | **15** | **9** | **54** |
