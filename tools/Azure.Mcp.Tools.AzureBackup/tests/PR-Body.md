## Summary

Adds the **Azure Backup** MCP toolset with 15 commands covering Recovery Services vaults, Data Protection backup vaults, backup policies, protected items, jobs, recovery points, governance, and disaster recovery operations.

## Changes

### New Toolset: `Azure.Mcp.Tools.AzureBackup`

**15 commands across 7 command groups:**

| Group | Command | Description |
|-------|---------|-------------|
| Vault | `azurebackup_vault_get` | List/get Recovery Services and Backup Vaults |
| Vault | `azurebackup_vault_create` | Create a Recovery Services vault |
| Vault | `azurebackup_vault_update` | Update vault properties (redundancy, soft delete, immutability) |
| Policy | `azurebackup_policy_get` | List/get backup policies for a vault |
| Policy | `azurebackup_policy_create` | Create backup policies for VMs, SQL, SAP HANA, Blobs, Disks, PostgreSQL |
| Protected Item | `azurebackup_protecteditem_get` | List/get protected (backed up) items |
| Protected Item | `azurebackup_protecteditem_protect` | Enable backup protection for a resource |
| Protectable Item | `azurebackup_protectableitem_list` | List resources available for backup |
| Backup | `azurebackup_backup_status` | Check backup status for an Azure resource |
| Job | `azurebackup_job_get` | List/get backup and restore jobs |
| Recovery Point | `azurebackup_recoverypoint_get` | List/get recovery points for a protected item |
| Governance | `azurebackup_governance_find-unprotected` | Find unprotected Azure resources |
| Governance | `azurebackup_governance_immutability` | Check vault immutability configuration |
| Governance | `azurebackup_governance_soft-delete` | Check vault soft delete configuration |
| DR | `azurebackup_dr_enablecrr` | Enable cross-region restore on a vault |

### Architecture

- **Dual-stack support**: Both Recovery Services (RSV) and Data Protection (DPP) vault types
- **Datasource registry pattern**: Extensible `RsvDatasourceRegistry` and `DppDatasourceRegistry` for workload-specific operations
- **Service layer**: `AzureBackupService` orchestrates RSV and DPP operations through `RsvBackupOperations` and `DppBackupOperations`
- **AOT-safe**: All models registered in `AzureBackupJsonContext`

### Test Coverage

- **99 unit tests** covering all 15 commands (success, error, input validation, deserialization)
- **6 live integration tests** (vault get/list, policy list, governance checks, job list)
- **30 e2e test prompts** in e2eTestPrompts.md

### Documentation & Compliance

- Updated `azmcp-commands.md` with full Azure Backup command reference
- Updated `e2eTestPrompts.md` with 30 test prompts
- Updated `README.md` with Azure Backup in services list
- Added CODEOWNERS entry
- Added 7 consolidated tool groups to `consolidated-tools.json`
- Created changelog entry
- Created `test-resources.bicep` and `test-resources-post.ps1` for live test infrastructure
- Created LiveTests project with `assets.json` for recording support
- Spelling check passes
- Tool metadata verified via `Update-AzCommandsMetadata.ps1`

### Files Changed

- **105 files changed**, 10,801 insertions
- Source: 18 files under `tools/Azure.Mcp.Tools.AzureBackup/src/`
- Unit Tests: 15 test files under `tests/Azure.Mcp.Tools.AzureBackup.UnitTests/`
- Live Tests: `tests/Azure.Mcp.Tools.AzureBackup.LiveTests/`
- Test Infrastructure: `test-resources.bicep`, `test-resources-post.ps1`
- Docs: `azmcp-commands.md`, `e2eTestPrompts.md`, `README.md`
- Config: `CODEOWNERS`, `consolidated-tools.json`, `Microsoft.Mcp.slnx`, `Azure.Mcp.Server.slnx`

## Validation

- [x] `dotnet build` - 0 errors, 0 warnings
- [x] `dotnet test` - 99/99 unit tests pass
- [x] Live tests - 6/6 pass against deployed `mcplive-rsv` vault
- [x] `Invoke-Cspell.ps1` - Spelling check passes
- [x] `Update-AzCommandsMetadata.ps1` - Tool metadata up to date
- [x] Changelog entry created
- [x] CODEOWNERS updated
- [x] Consolidated tools updated

## Invoking Livetests

Copilot submitted PRs are not trustworthy by default. Users with `write` access to the repo need to validate the contents of this PR before leaving a comment with the text `/azp run mcp - pullrequest - live`. This will trigger the necessary livetest workflows to complete required validation.
