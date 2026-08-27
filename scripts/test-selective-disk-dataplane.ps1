# Selective Disk Backup — DATA-PLANE E2E validation
#
# Extends control-plane E2E by triggering real backup jobs and inspecting the
# resulting recovery points to prove the disk exclusion configuration is honored
# by RSV at backup time.
#
# Layout: 3 VMs (each with 3 data disks) share one RSV.
#   sdvm1 — configured via `protect --disk-list-setting include --disks-list 0`
#   sdvm2 — configured via `update-protection --disk-list-setting exclude --disks-list 1,2`
#   sdvm3 — configured via `update-protection --exclude-all-data-disks`
# All three trigger backup-now in parallel, then poll to Completed, then dump RP JSON.
#
# Wall clock: ~60-90 min (VM deploy ~5, protect + trigger ~5, backup jobs 30-60,
# teardown ~5).
[CmdletBinding()]
param(
    [string]$Location = 'eastus',
    [string]$RgSuffix = (Get-Random -Minimum 1000 -Maximum 9999),
    [switch]$KeepResources,
    [switch]$SkipDeploy,
    [string]$Rg,
    [int]$JobTimeoutMin = 90
)

$ErrorActionPreference = 'Stop'
Set-StrictMode -Version Latest

if (-not $Rg) { $Rg = "mcp-sdb-dp-$RgSuffix" }
$RsvName   = "sdrsv$RgSuffix"
$VmNames   = @('sdvm1', 'sdvm2', 'sdvm3')
$AdminUser = 'sdtestadmin'
$AdminPass = 'Sd!' + ([Guid]::NewGuid().ToString('N').Substring(0, 16)) + 'Aa1@'
$AzMcp     = Join-Path $PSScriptRoot '..\servers\Azure.Mcp.Server\src\bin\Debug\net10.0\azmcp.exe' | Resolve-Path | Select-Object -ExpandProperty Path
$Bicep     = Join-Path $PSScriptRoot 'selective-disk-dataplane.bicep'
$Sub       = (az account show --query id -o tsv).Trim()
$Tenant    = (az account show --query tenantId -o tsv).Trim()
$LogDir    = Join-Path $PSScriptRoot ("dp-logs-" + $RgSuffix)
New-Item -ItemType Directory -Force -Path $LogDir | Out-Null

function Section($name) {
    Write-Host ''
    Write-Host ('═' * 78) -ForegroundColor DarkGray
    Write-Host " $name" -ForegroundColor Cyan
    Write-Host ('═' * 78) -ForegroundColor DarkGray
}

function Log($msg, $color = 'Gray') { Write-Host "  $msg" -ForegroundColor $color }

function Invoke-AzMcp([string[]]$Argv) {
    Log ("> azmcp " + ($Argv -join ' ')) 'DarkYellow'
    $raw = & $AzMcp @Argv 2>&1
    $text = ($raw | Out-String).Trim()
    if ($text) {
        $preview = $text.Substring(0, [Math]::Min(400, $text.Length))
        Write-Host "  --- azmcp output (first 400) ---" -ForegroundColor DarkGray
        Write-Host $preview -ForegroundColor Gray
        Write-Host "  --- end ---" -ForegroundColor DarkGray
    }
    try { return $text | ConvertFrom-Json -Depth 32 -ErrorAction Stop } catch { return $text }
}

# ─── Deploy ─────────────────────────────────────────────────────────────────────
Section 'Setup'
if (-not $SkipDeploy) {
    Log "Creating RG $Rg in $Location…"
    az group create -n $Rg -l $Location --tags Owner=azurebackup-mcp-selectivedisk-e2e ServiceName=AzureBackup Environment=Test | Out-Null

    Log "Deploying infra (VMs=$($VmNames -join ',') RSV=$RsvName)…"
    $paramsFile = Join-Path $LogDir 'deploy.parameters.json'
    @{
        '$schema' = 'https://schema.management.azure.com/schemas/2019-04-01/deploymentParameters.json#'
        contentVersion = '1.0.0.0'
        parameters = @{
            rsvName   = @{ value = $RsvName }
            vmNames   = @{ value = $VmNames }
            adminUser = @{ value = $AdminUser }
            adminPass = @{ value = $AdminPass }
        }
    } | ConvertTo-Json -Depth 10 | Out-File -FilePath $paramsFile -Encoding utf8
    az deployment group create `
        --resource-group $Rg `
        --template-file $Bicep `
        --parameters "@$paramsFile" `
        --only-show-errors -o none
    if ($LASTEXITCODE -ne 0) { throw "Bicep deployment failed" }
    Log "Deployment complete." Green
} else {
    Log "Skipping deploy; reusing RG $Rg." Yellow
    $rsvList = az backup vault list -g $Rg --query "[].name" -o tsv
    if ($rsvList) { $RsvName = ($rsvList -split "`n")[0].Trim() }
    Log "RSV=$RsvName" Green
}

# ─── Configure each VM's protection ────────────────────────────────────────────
Section 'Configure protection'
function Get-VmArmId([string]$name) { "/subscriptions/$Sub/resourceGroups/$Rg/providers/Microsoft.Compute/virtualMachines/$name" }

$vmConfigs = @(
    @{ Vm='sdvm1'; Label='include-lun-0'
       Cmd=@('azurebackup','protecteditem','protect',
             '--tenant',$Tenant,'--subscription',$Sub,'--resource-group',$Rg,
             '--vault',$RsvName,'--datasource-type','VM','--datasource-id',(Get-VmArmId 'sdvm1'),
             '--policy','DefaultPolicy','--disk-list-setting','include','--disks-list','0') }
    @{ Vm='sdvm2'; Label='exclude-lun-1-2'
       Cmd=@('azurebackup','protecteditem','protect',
             '--tenant',$Tenant,'--subscription',$Sub,'--resource-group',$Rg,
             '--vault',$RsvName,'--datasource-type','VM','--datasource-id',(Get-VmArmId 'sdvm2'),
             '--policy','DefaultPolicy') }
    @{ Vm='sdvm3'; Label='exclude-all'
       Cmd=@('azurebackup','protecteditem','protect',
             '--tenant',$Tenant,'--subscription',$Sub,'--resource-group',$Rg,
             '--vault',$RsvName,'--datasource-type','VM','--datasource-id',(Get-VmArmId 'sdvm3'),
             '--policy','DefaultPolicy') }
)

foreach ($c in $vmConfigs) {
    Log "→ $($c.Vm): $($c.Label) (initial protect)"
    $r = Invoke-AzMcp $c.Cmd
    Start-Sleep -Seconds 5
}

# Poll until protect jobs complete (protect is async). Give up after 10 min.
Log "Waiting for initial protect jobs to complete (~5 min each)…"
$protectDeadline = (Get-Date).AddMinutes(15)
$protectDone = @{}
foreach ($v in $VmNames) { $protectDone[$v] = $false }
while (((Get-Date) -lt $protectDeadline) -and ($protectDone.Values -contains $false)) {
    foreach ($v in $VmNames) {
        if ($protectDone[$v]) { continue }
        $exists = az backup item show -g $Rg -v $RsvName --backup-management-type AzureIaasVM `
            --workload-type VM --container-name $v --name $v --query 'properties.protectionState' -o tsv 2>$null
        $exists = ($exists | Out-String).Trim()
        if ($exists) {
            Log "  $v protectionState=$exists" Green
            $protectDone[$v] = $true
        } else {
            Log "  $v not yet registered…"
        }
    }
    if ($protectDone.Values -contains $false) { Start-Sleep -Seconds 30 }
}

# For sdvm2 and sdvm3 we then update-protection with the actual exclusion config
Log "→ sdvm2: update-protection exclude 1,2"
Invoke-AzMcp @('azurebackup','protecteditem','update-protection',
    '--tenant',$Tenant,'--subscription',$Sub,'--resource-group',$Rg,
    '--vault',$RsvName,'--datasource-id',(Get-VmArmId 'sdvm2'),
    '--disk-list-setting','exclude','--disks-list','1,2') | Out-Null

Log "→ sdvm3: update-protection exclude-all-data-disks"
Invoke-AzMcp @('azurebackup','protecteditem','update-protection',
    '--tenant',$Tenant,'--subscription',$Sub,'--resource-group',$Rg,
    '--vault',$RsvName,'--datasource-id',(Get-VmArmId 'sdvm3'),
    '--exclude-all-data-disks') | Out-Null

Start-Sleep -Seconds 20

# ─── Verify ARM state before firing backups ─────────────────────────────────────
Section 'ARM state (pre-backup)'
foreach ($v in $VmNames) {
    $props = az backup item show -g $Rg -v $RsvName --backup-management-type AzureIaasVM `
        --workload-type VM --container-name $v --name $v --query 'properties.extendedProperties.diskExclusionProperties' -o json 2>$null
    Log "  $v → $props" Yellow
}

# ─── Trigger backup-now for each VM ─────────────────────────────────────────────
Section 'Trigger backup-now'
$jobs = @{}
$retainUntil = (Get-Date).ToUniversalTime().AddDays(2).ToString('dd-MM-yyyy')
foreach ($v in $VmNames) {
    Log "→ $v : az backup protection backup-now (retain until $retainUntil)"
    $out = az backup protection backup-now -g $Rg -v $RsvName `
        --container-name $v --item-name $v `
        --backup-management-type AzureIaasVM --retain-until $retainUntil `
        --only-show-errors -o json 2>$null
    if ($LASTEXITCODE -ne 0) {
        Log "  backup-now failed for $v :`n$out" Red
        continue
    }
    try {
        $jobObj = ($out | Out-String) | ConvertFrom-Json -ErrorAction Stop
        $jobId = ($jobObj.name -as [string])
        if (-not $jobId -and $jobObj.id) { $jobId = ([string]$jobObj.id).Split('/')[-1] }
        Log "  jobId=$jobId" Green
        $jobs[$v] = $jobId
    } catch {
        Log "  Could not parse job id: $out" Red
    }
}

# ─── Poll all jobs to completion ────────────────────────────────────────────────
Section "Poll backup jobs (timeout ${JobTimeoutMin}m)"
$deadline = (Get-Date).AddMinutes($JobTimeoutMin)
$done = @{}
foreach ($v in $VmNames) { $done[$v] = $false }

while (((Get-Date) -lt $deadline) -and ($done.Values -contains $false)) {
    foreach ($v in $VmNames) {
        if ($done[$v]) { continue }
        if (-not $jobs.ContainsKey($v)) { $done[$v] = $true; continue }
        $status = az backup job show -g $Rg -v $RsvName --name $jobs[$v] --query 'properties.status' -o tsv 2>$null
        $status = ($status | Out-String).Trim()
        Log "  $v [$($jobs[$v].Substring(0,8))…] → $status"
        if ($status -in @('Completed', 'CompletedWithWarnings', 'Failed', 'Cancelled')) {
            $done[$v] = $true
        }
    }
    if ($done.Values -contains $false) { Start-Sleep -Seconds 60 }
}

# ─── Fetch RP details ───────────────────────────────────────────────────────────
Section 'Recovery point details'
foreach ($v in $VmNames) {
    if (-not $jobs.ContainsKey($v)) { continue }
    $jobStatus = az backup job show -g $Rg -v $RsvName --name $jobs[$v] --query 'properties.status' -o tsv 2>$null
    Log "$v final status: $jobStatus" ($jobStatus -eq 'Completed' ? 'Green' : 'Yellow')

    $rp = az backup recoverypoint list -g $Rg -v $RsvName `
        --container-name $v --item-name $v `
        --backup-management-type AzureIaasVM --workload-type VM `
        --query "[?properties.recoveryPointType!='Log'] | [0]" -o json 2>$null
    if ($rp) {
        $rpFile = Join-Path $LogDir "$v.rp.json"
        $rp | Out-File -FilePath $rpFile -Encoding utf8
        Log "  → RP dumped to $rpFile" DarkGray
        try {
            $rpObj = $rp | ConvertFrom-Json -Depth 32
            $rpName = $rpObj.name
            $rpTime = $rpObj.properties.recoveryPointTime
            Log "  $v RP $rpName @ $rpTime" Green

            # Try to fetch disk detail
            $rpDetail = az backup recoverypoint show -g $Rg -v $RsvName `
                --container-name $v --item-name $v `
                --backup-management-type AzureIaasVM --workload-type VM `
                --name $rpName -o json 2>$null
            if ($rpDetail) {
                $rpDetailFile = Join-Path $LogDir "$v.rp.detail.json"
                $rpDetail | Out-File -FilePath $rpDetailFile -Encoding utf8
                Log "    detail dumped to $rpDetailFile" DarkGray
            }
        } catch {
            Log "  $v RP parse failed: $_" Red
        }
    } else {
        Log "  $v : no RP found" Red
    }
}

# ─── Teardown ───────────────────────────────────────────────────────────────────
if (-not $KeepResources -and $jobs.Count -gt 0) {
    Section 'Teardown'
    Log "Disabling protection (delete data) on all 3 VMs…"
    foreach ($v in $VmNames) {
        az backup protection disable -g $Rg -v $RsvName `
            --container-name $v --item-name $v `
            --backup-management-type AzureIaasVM --delete-backup-data true --yes 2>&1 | Out-Null
    }
    Log "Deleting RG $Rg (async)…"
    az group delete -n $Rg --yes --no-wait | Out-Null
} else {
    Section 'Teardown skipped'
    Log "RG $Rg kept for inspection. Delete manually with: az group delete -n $Rg --yes --no-wait" Yellow
    Log "Logs in $LogDir." Yellow
}

Section 'Done'
Log "Log dir: $LogDir" Cyan
