# Manual Test Cases: `azurebackup security configure-mua`

## Prerequisites

1. **Two test vaults** deployed (one RSV, one DPP/Backup Vault) in the same region
2. **A Resource Guard** deployed in the same region (ideally in a separate subscription)
3. **Reader role** on the Resource Guard assigned to the test principal
4. **Backup Contributor** on both vaults
5. Local MCP server running: `azmcp server start`

### Environment Variables
```powershell
$sub = "<your-subscription-id>"
$rg = "<vault-resource-group>"
$rsvVault = "<rsv-vault-name>"
$dppVault = "<dpp-vault-name>"
$guardId = "/subscriptions/<guard-sub>/resourceGroups/<guard-rg>/providers/Microsoft.DataProtection/resourceGuards/<guard-name>"
```

---

## Test Matrix

| # | Dimension | Vault Type | Operation | Expected |
|---|-----------|-----------|-----------|----------|
| 1 | Enable MUA | RSV | with `--resource-guard-id` | Succeeded |
| 2 | Enable MUA | DPP | with `--resource-guard-id` + `--vault-type dpp` | Succeeded |
| 3 | Disable MUA | RSV | without `--resource-guard-id` | Succeeded (if MUA was enabled) |
| 4 | Disable MUA | DPP | without `--resource-guard-id` + `--vault-type dpp` | Succeeded (if MUA was enabled) |
| 5 | Auto-detect | RSV | no `--vault-type` | Auto-detects RSV, succeeds |
| 6 | Auto-detect | DPP | no `--vault-type` | Auto-detects DPP, succeeds |
| 7 | Invalid vault-type | N/A | `--vault-type invalid` | 400 Bad Request |
| 8 | Missing vault | RSV | non-existent vault name | 404 Not Found |
| 9 | No Reader role | RSV | enable without Reader on guard | 403 Forbidden |
| 10 | Disable without MUA | RSV | disable when not enabled | 404 (no VaultProxy) |
| 11 | Re-enable (idempotent) | RSV | enable when already enabled | Succeeded (idempotent CreateOrUpdate) |
| 12 | Enable MUA | DPP | without `--vault-type` (auto-detect) | Succeeded |

---

## Detailed Test Steps

### TC-1: Enable MUA on RSV vault
```
Tool: azurebackup_security_configure-mua
Parameters:
  --subscription <sub>
  --resource-group <rg>
  --vault <rsvVault>
  --resource-guard-id <guardId>

Expected: status = "Succeeded", message contains "Multi-User Authorization enabled"
Verify in Portal: Vault > Properties > Multi-User Authorization shows Resource Guard linked
```

### TC-2: Enable MUA on DPP vault (explicit vault-type)
```
Tool: azurebackup_security_configure-mua
Parameters:
  --subscription <sub>
  --resource-group <rg>
  --vault <dppVault>
  --vault-type dpp
  --resource-guard-id <guardId>

Expected: status = "Succeeded"
Verify in Portal: Backup vault > Properties > Multi-User Authorization shows linked
```

### TC-3: Disable MUA on RSV vault (requires Backup MUA Operator role)
```
Pre-condition: TC-1 passed, MUA is enabled on RSV vault
Pre-condition: Test principal has Backup MUA Operator role on Resource Guard

Tool: azurebackup_security_configure-mua
Parameters:
  --subscription <sub>
  --resource-group <rg>
  --vault <rsvVault>

Expected: status = "Succeeded", message contains "disabled"
Verify in Portal: Vault > Properties > Multi-User Authorization shows "Not configured"
```

### TC-4: Disable MUA on DPP vault
```
Pre-condition: TC-2 passed, MUA is enabled on DPP vault
Pre-condition: Test principal has Backup MUA Operator role on Resource Guard

Tool: azurebackup_security_configure-mua
Parameters:
  --subscription <sub>
  --resource-group <rg>
  --vault <dppVault>
  --vault-type dpp

Expected: status = "Succeeded"
```

### TC-5: Auto-detect RSV vault type
```
Tool: azurebackup_security_configure-mua
Parameters:
  --subscription <sub>
  --resource-group <rg>
  --vault <rsvVault>
  --resource-guard-id <guardId>
  (no --vault-type)

Expected: Auto-detects RSV, status = "Succeeded"
```

### TC-6: Auto-detect DPP vault type
```
Tool: azurebackup_security_configure-mua
Parameters:
  --subscription <sub>
  --resource-group <rg>
  --vault <dppVault>
  --resource-guard-id <guardId>
  (no --vault-type)

Expected: Auto-detects DPP, status = "Succeeded"
```

### TC-7: Invalid vault-type
```
Tool: azurebackup_security_configure-mua
Parameters:
  --subscription <sub>
  --resource-group <rg>
  --vault <rsvVault>
  --vault-type invalid

Expected: 400 Bad Request, message contains "--vault-type must be 'rsv' or 'dpp'"
```

### TC-8: Non-existent vault
```
Tool: azurebackup_security_configure-mua
Parameters:
  --subscription <sub>
  --resource-group <rg>
  --vault nonexistent-vault-12345
  --vault-type rsv
  --resource-guard-id <guardId>

Expected: 404 Not Found, message contains "not found"
```

### TC-9: Missing Reader role on Resource Guard
```
Pre-condition: Test principal does NOT have Reader on the Resource Guard

Tool: azurebackup_security_configure-mua
Parameters:
  --subscription <sub>
  --resource-group <rg>
  --vault <rsvVault>
  --resource-guard-id <guardId>

Expected: 403 Forbidden, message contains "Authorization failed" and mentions Reader role
```

### TC-10: Disable MUA when not enabled
```
Pre-condition: MUA is NOT enabled on the vault (no VaultProxy exists)

Tool: azurebackup_security_configure-mua
Parameters:
  --subscription <sub>
  --resource-group <rg>
  --vault <rsvVault>
  --vault-type rsv

Expected: 404 Not Found, message mentions vault or Resource Guard not found
```

### TC-11: Re-enable MUA (idempotent)
```
Pre-condition: TC-1 passed, MUA is already enabled

Tool: azurebackup_security_configure-mua
Parameters:
  --subscription <sub>
  --resource-group <rg>
  --vault <rsvVault>
  --resource-guard-id <guardId>

Expected: status = "Succeeded" (CreateOrUpdate is idempotent)
```

### TC-12: Enable MUA on DPP with auto-detect
```
Tool: azurebackup_security_configure-mua
Parameters:
  --subscription <sub>
  --resource-group <rg>
  --vault <dppVault>
  --resource-guard-id <guardId>
  (no --vault-type)

Expected: Auto-detects DPP, status = "Succeeded"
```

---

## Cross-Region Validation

| Scenario | Expected |
|----------|----------|
| Resource Guard and vault in **same region** | Succeeds |
| Resource Guard and vault in **different regions** | 400 Bad Request — region mismatch |

---

## Response Schema Validation

Every successful response must match:
```json
{
  "result": {
    "status": "Succeeded",
    "jobId": null,
    "message": "Multi-User Authorization enabled on vault '...' with Resource Guard '...'."
  }
}
```

For disable:
```json
{
  "result": {
    "status": "Succeeded",
    "jobId": null,
    "message": "Multi-User Authorization disabled on vault '...'."
  }
}
```

---

## Portal Verification Steps

After enabling MUA:
1. Go to **Recovery Services vault > Properties > Multi-User Authorization**
2. Verify it shows the linked Resource Guard name and ARM ID
3. Try a protected operation (e.g., disable soft delete) — it should require approval

After disabling MUA:
1. Verify Multi-User Authorization shows "Not configured"
2. Protected operations should no longer require approval
