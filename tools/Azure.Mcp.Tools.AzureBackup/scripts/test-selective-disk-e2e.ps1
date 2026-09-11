# Selective Disk Backup — E2E validation script
#
# Purpose: prove the local azmcp.exe selective disk backup implementation works against
# a real Azure IaaS VM + RSV. Spins up a throwaway RG (VM with 3 data disks + RSV LRS),
# runs a series of protect / update-protection scenarios via the LOCAL binary, verifies
# each via ARM state (`az backup item show`), then tears the RG down.
#
# Scenarios covered (all against RSV IaaS VM):
#   S1 protect --disk-list-setting include --disks-list 0                 → LUN 0 + OS
#   S2 update-protection --disk-list-setting exclude --disks-list 1,2     → LUN 0 + OS (equivalent)
#   S3 update-protection --exclude-all-data-disks                         → OS disk only
#   S4 update-protection --disk-list-setting resetexclusionsettings       → all disks
#   S5 update-protection (no selective disk args) → validation error       (client-side guard)
#
# Not covered here (already covered by unit tests / would need extra setup):
#   * DPP + disk-exclusion → rejected  (unit + service tests)
#   * SQL / SAPHANA / SAPASE / AFS + disk-exclusion → rejected (registry tests)
#
# Usage:
#   pwsh tools/Azure.Mcp.Tools.AzureBackup/scripts/test-selective-disk-e2e.ps1
#   pwsh tools/Azure.Mcp.Tools.AzureBackup/scripts/test-selective-disk-e2e.ps1 -KeepResources -Location eastus
[CmdletBinding()]
param(
    [string]$Location = 'southeastasia',
    [string]$RgSuffix = (Get-Random -Minimum 1000 -Maximum 9999),
    [switch]$KeepResources,
    [switch]$SkipDeploy   # reuse an existing RG (name via -Rg) — useful for re-runs
    ,
    [string]$Rg
)

$ErrorActionPreference = 'Stop'
Set-StrictMode -Version Latest

# ─── Configuration ──────────────────────────────────────────────────────────────
if (-not $Rg) { $Rg = "mcp-sdb-e2e-$RgSuffix" }
$VmName    = 'sdvm'
$RsvName   = "sdrsv$RgSuffix"
$AdminUser = 'sdtestadmin'
# Deterministic-but-strong password
$AdminPass = 'Sd!' + ([Guid]::NewGuid().ToString('N').Substring(0, 16)) + 'Aa1@'
$AzMcp     = Join-Path $PSScriptRoot '..\..\..\servers\Azure.Mcp.Server\src\bin\Debug\net10.0\azmcp.exe' | Resolve-Path | Select-Object -ExpandProperty Path
$Bicep     = Join-Path $PSScriptRoot 'selective-disk-e2e.bicep'
$Sub       = (az account show --query id -o tsv).Trim()
$Tenant    = (az account show --query tenantId -o tsv).Trim()
$Results   = New-Object 'System.Collections.Generic.List[psobject]'

function Section($name) {
    Write-Host ''
    Write-Host ('═' * 78) -ForegroundColor DarkGray
    Write-Host " $name" -ForegroundColor Cyan
    Write-Host ('═' * 78) -ForegroundColor DarkGray
}

function Log($msg, $color = 'Gray') { Write-Host "  $msg" -ForegroundColor $color }

function Record([string]$name, [bool]$pass, [string]$detail = '') {
    $status = if ($pass) { 'PASS' } else { 'FAIL' }
    $color  = if ($pass) { 'Green' } else { 'Red' }
    Write-Host "  [$status] $name" -ForegroundColor $color
    if ($detail) { Write-Host "        $detail" -ForegroundColor DarkGray }
    $Results.Add([pscustomobject]@{ Name = $name; Pass = $pass; Detail = $detail })
}

function Invoke-AzMcp([string[]]$Argv) {
    Log ("> azmcp " + ($Argv -join ' ')) 'DarkYellow'
    $raw = & $AzMcp @Argv 2>&1
    $text = ($raw | Out-String).Trim()
    if ($text) {
        $preview = $text.Substring(0, [Math]::Min(600, $text.Length))
        Write-Host "  --- azmcp output ---" -ForegroundColor DarkGray
        Write-Host $preview -ForegroundColor Gray
        Write-Host "  --- end ---" -ForegroundColor DarkGray
    }
    try { return $text | ConvertFrom-Json -Depth 32 -ErrorAction Stop } catch { return $text }
}

function Get-VmProtectedItem {
    param([string]$Container, [string]$Item)
    # az backup item show needs friendly names, not fabric-encoded IDs
    $json = az backup item show `
        --resource-group $Rg --vault-name $RsvName `
        --container-name $Container --name $Item `
        --backup-management-type AzureIaasVM --workload-type VM `
        -o json 2>$null
    if (-not $json) { return $null }
    return $json | ConvertFrom-Json -Depth 32
}

function Get-DiskExclusion {
    param($Item)
    if (-not $Item) { return $null }
    $p = $Item.properties
    if ($null -eq $p) { return $null }
    $ext = $p.extendedProperties
    if ($null -eq $ext) { return $null }
    return $ext.diskExclusionProperties
}

# ─── Step 1: Deploy infra ───────────────────────────────────────────────────────
Section "Step 1 · Provision test infrastructure ($Rg, $Location)"

if ($SkipDeploy) {
    Log "SkipDeploy set — reusing $Rg" 'Yellow'
    # Look up the actual RSV name from the RG
    $vaults = az backup vault list -g $Rg -o json 2>$null | ConvertFrom-Json
    if ($vaults -and $vaults.Count -ge 1) {
        $RsvName = $vaults[0].name
        Log "Discovered RSV: $RsvName" 'Green'
    } else {
        throw "No RSV found in $Rg"
    }
} else {
    Log "Creating resource group..."
    az group create -n $Rg -l $Location --tags Owner=azurebackup-mcp-selectivedisk-e2e Purpose=selective-disk-e2e -o none

    Log "Deploying VM (3 data disks) + RSV via Bicep (~2-3 min)..."
    $deployName = "sdb-e2e-$(Get-Date -Format yyyyMMddHHmmss)"
    az deployment group create `
        -g $Rg -n $deployName -f $Bicep `
        --parameters vmName=$VmName rsvName=$RsvName adminUser=$AdminUser adminPass=$AdminPass `
        -o none
    if ($LASTEXITCODE -ne 0) { throw "Bicep deployment failed" }
    Log "Deployment complete." 'Green'
}

$VmId = "/subscriptions/$Sub/resourceGroups/$Rg/providers/Microsoft.Compute/virtualMachines/$VmName"

# ─── Step 2: Discover default policy ────────────────────────────────────────────
Section "Step 2 · Discover default IaaS VM policy"

$policyRes = Invoke-AzMcp @('azurebackup', 'policy', 'get',
    '--tenant', $Tenant, '--subscription', $Sub, '--resource-group', $Rg,
    '--vault', $RsvName, '--vault-type', 'rsv')

# Attempt to extract "DefaultPolicy" if returned; else fall back to az CLI listing
$policyName = 'DefaultPolicy'
$existingPolicies = az backup policy list --vault-name $RsvName -g $Rg -o json 2>$null | ConvertFrom-Json -Depth 8
if ($existingPolicies) {
    $iaasPolicy = $existingPolicies | Where-Object { $_.properties.backupManagementType -eq 'AzureIaasVM' } | Select-Object -First 1
    if ($iaasPolicy) { $policyName = $iaasPolicy.name }
}
Log "Using policy: $policyName" 'Green'

# ─── Step 3: Scenario 1 — protect --disk-list-setting include --disks-list 0 ────
Section "Step 3 · S1 · protect (include LUN 0)"

$s1 = Invoke-AzMcp @('azurebackup', 'protecteditem', 'protect',
    '--tenant', $Tenant, '--subscription', $Sub, '--resource-group', $Rg,
    '--vault', $RsvName, '--vault-type', 'rsv',
    '--datasource-type', 'VM',
    '--datasource-id', $VmId,
    '--policy', $policyName,
    '--disk-list-setting', 'include',
    '--disks-list', '0')

Log "Waiting up to 4 min for backup enable to propagate..." 'Yellow'
$container = $VmName        # friendly container name (az CLI convention for IaaS VM)
$itemName  = $VmName        # friendly item name
$found = $false
for ($i = 0; $i -lt 24; $i++) {
    Start-Sleep -Seconds 10
    $it = Get-VmProtectedItem -Container $container -Item $itemName
    if ($it -and $it.properties.protectionState -and $it.properties.protectionState -ne 'ProtectionStopped') { $found = $true; break }
    Write-Host "." -NoNewline
}
Write-Host ""

if (-not $found) {
    Record "S1 protect (include LUN 0) — protection enabled" $false "Item did not appear in ARM within 4 min"
} else {
    $de = Get-DiskExclusion -Item $it
    $ok = ($null -ne $de) -and ($de.isInclusionList -eq $true) -and ($de.diskLunList -contains 0) -and ($de.diskLunList.Count -eq 1)
    Record "S1 protect (include LUN 0)" $ok ("diskExclusionProperties = " + ($de | ConvertTo-Json -Compress -Depth 4))
}

# ─── Step 4: Scenario 2 — update-protection --exclude 1,2 ───────────────────────
Section "Step 4 · S2 · update-protection (exclude LUN 1,2)"

$s2 = Invoke-AzMcp @('azurebackup', 'protecteditem', 'update-protection',
    '--tenant', $Tenant, '--subscription', $Sub, '--resource-group', $Rg,
    '--vault', $RsvName, '--vault-type', 'rsv',
    '--datasource-id', $VmId,
    '--disk-list-setting', 'exclude',
    '--disks-list', '1,2')

Start-Sleep -Seconds 30
$it2 = Get-VmProtectedItem -Container $container -Item $itemName
$de2 = Get-DiskExclusion -Item $it2
if ($null -eq $de2) {
    Record "S2 update-protection (exclude LUN 1,2)" $false "No diskExclusionProperties on ARM item"
} else {
    $lunSet2 = @(@($de2.diskLunList) | Sort-Object)
    $ok2 = ($de2.isInclusionList -eq $false) -and ($lunSet2.Count -eq 2) -and ($lunSet2[0] -eq 1) -and ($lunSet2[1] -eq 2)
    Record "S2 update-protection (exclude LUN 1,2)" $ok2 ("diskExclusionProperties = " + ($de2 | ConvertTo-Json -Compress -Depth 4))
}

# ─── Step 5: Scenario 3 — update-protection --exclude-all-data-disks ────────────
Section "Step 5 · S3 · update-protection (exclude-all-data-disks)"

$s3 = Invoke-AzMcp @('azurebackup', 'protecteditem', 'update-protection',
    '--tenant', $Tenant, '--subscription', $Sub, '--resource-group', $Rg,
    '--vault', $RsvName, '--vault-type', 'rsv',
    '--datasource-id', $VmId,
    '--exclude-all-data-disks')

Start-Sleep -Seconds 30
$it3 = Get-VmProtectedItem -Container $container -Item $itemName
$de3 = Get-DiskExclusion -Item $it3
# exclude-all-data-disks ⇒ inclusion=true with empty LUN list (back up nothing, i.e., OS only)
if ($null -eq $de3) {
    Record "S3 update-protection (exclude-all-data-disks)" $false "No diskExclusionProperties on ARM item"
} else {
    $lunSet3 = @($de3.diskLunList)
    $ok3 = ($de3.isInclusionList -eq $true) -and ($lunSet3.Count -eq 0)
    Record "S3 update-protection (exclude-all-data-disks)" $ok3 ("diskExclusionProperties = " + ($de3 | ConvertTo-Json -Compress -Depth 4))
}

# ─── Step 6: Scenario 4 — reset ─────────────────────────────────────────────────
Section "Step 6 · S4 · update-protection (resetexclusionsettings)"

$s4 = Invoke-AzMcp @('azurebackup', 'protecteditem', 'update-protection',
    '--tenant', $Tenant, '--subscription', $Sub, '--resource-group', $Rg,
    '--vault', $RsvName, '--vault-type', 'rsv',
    '--datasource-id', $VmId,
    '--disk-list-setting', 'resetexclusionsettings')

Start-Sleep -Seconds 30
$it4 = Get-VmProtectedItem -Container $container -Item $itemName
$de4 = Get-DiskExclusion -Item $it4
# reset ⇒ no diskExclusionProperties (or empty)
if ($null -eq $de4) {
    Record "S4 update-protection (reset)" $true "diskExclusionProperties = null (as expected)"
} else {
    $lunSet4 = @($de4.diskLunList)
    # A "reset" may still leave the property in ARM but with no LUNs and inclusion=false, or the property may be dropped entirely.
    $ok4 = ($lunSet4.Count -eq 0)
    Record "S4 update-protection (reset)" $ok4 ("diskExclusionProperties = " + ($de4 | ConvertTo-Json -Compress -Depth 4))
}

# ─── Step 7: Scenario 5 — client-side validation guard ──────────────────────────
Section "Step 7 · S5 · update-protection (no args) — should error"

$rawS5 = & $AzMcp azurebackup protecteditem update-protection `
    --tenant $Tenant --subscription $Sub --resource-group $Rg `
    --vault $RsvName --vault-type rsv `
    --datasource-id $VmId 2>&1
$textS5 = ($rawS5 | Out-String)
$ok5 = ($textS5 -match 'At least one of' -or $textS5 -match 'nothing to update' -or $textS5 -match 'ArgumentException')
Record "S5 update-protection (no args) — rejects with validation error" $ok5 ("Server text: " + ($textS5.Substring(0, [Math]::Min(200, $textS5.Length)) -replace "`r?`n", ' | '))

# ─── Summary ────────────────────────────────────────────────────────────────────
Section "Summary"
$pass = @($Results | Where-Object { $_.Pass }).Count
$fail = @($Results | Where-Object { -not $_.Pass }).Count
Write-Host ("  Total: {0}   Pass: {1}   Fail: {2}" -f $Results.Count, $pass, $fail) -ForegroundColor $(if ($fail -eq 0) { 'Green' } else { 'Red' })
$Results | Format-Table -AutoSize Name, Pass, Detail

# ─── Cleanup ────────────────────────────────────────────────────────────────────
if ($KeepResources) {
    Log "-KeepResources set — leaving RG '$Rg' in place" 'Yellow'
} else {
    Section "Cleanup"
    Log "Deleting RG '$Rg' (fire-and-forget)..." 'Yellow'
    # Try to disable+stop backup first so the RSV delete doesn't block
    az backup protection disable `
        --resource-group $Rg --vault-name $RsvName `
        --container-name $container --item-name $itemName `
        --backup-management-type AzureIaasVM `
        --delete-backup-data true --yes 2>$null | Out-Null
    az group delete -n $Rg --yes --no-wait -o none
}

if ($fail -gt 0) { exit 1 }
