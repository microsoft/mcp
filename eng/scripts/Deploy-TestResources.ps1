param(
    [Parameter(Mandatory=$true)]
    [string[]]$Paths,
    [string]$SubscriptionId,
    [string]$ResourceGroupName,
    [string]$BaseName,
    [int]$DeleteAfterHours = 12,
    [switch]$Unique,
    [switch]$Parallel
)

$ErrorActionPreference = 'Stop'
. "$PSScriptRoot/../common/scripts/common.ps1"

function New-StringHash([string[]]$strings) {
    $string = $strings -join ' '
    $hash = [System.Security.Cryptography.SHA1]::Create()
    $bytes = [System.Text.Encoding]::UTF8.GetBytes($string)
    $hashBytes = $hash.ComputeHash($bytes)
    return [BitConverter]::ToString($hashBytes) -replace '-', ''
}

$toolDirectories = Get-ChildItem -Path "$RepoRoot/tools" -Directory
$serverDirectories = Get-ChildItem -Path "$RepoRoot/servers" -Directory
$coreDirectories = Get-ChildItem -Path "$RepoRoot/core" -Directory

$testablePaths = @($toolDirectories + $serverDirectories + $coreDirectories)
| Where-Object { Test-Path "$_/tests/test-resources.bicep" }
| ForEach-Object { (Resolve-Path -Path $_ -Relative -RelativeBasePath $RepoRoot).Replace('\', '/').TrimStart('./')}

$normalizedPathFilters = $Paths | ForEach-Object { "*$($_.Replace('\', '/'))*" }

if($normalizedPathFilters) {
    $testablePaths = $testablePaths | Where-Object {
        foreach($filter in $normalizedPathFilters) {
            if ($_ -like $filter) {
                return $true
            }
        }
        return $false
    }
}

if ($testablePaths.Count -eq 0) {
    Write-Error "No paths with test resources match the specified filters: $($Paths -join ', ')"
    exit 1
}

if($SubscriptionId) {
    Select-AzSubscription -Subscription $SubscriptionId | Out-Null
    $context = Get-AzContext
} else {
    # We don't want New-TestResources to conditionally pick a subscription for us
    # If the user didn't specify a subscription, we explicitly use the current context's subscription
    $context = Get-AzContext
    $SubscriptionId = $context.Subscription.Id
}

$subscriptionName = $context.Subscription.Name
$account = $context.Account
$accountName = $account.Id.Split('@')[0].ToLower()

function Deploy-TestResources
{
    param(
        [string]$Path,
        [string]$SubscriptionId,
        [string]$SubscriptionName,
        [string]$ResourceGroupName,
        [string]$BaseName,
        [int]$DeleteAfterHours,
        [string]$TestResourcesDirectory,
        [switch]$AsJob
    )

    Write-Host @"
Deploying$($AsJob ? ' in background job' : ''):
    Path: '$Path'
    SubscriptionId: '$SubscriptionId'
    SubscriptionName: '$SubscriptionName'
    ResourceGroupName: '$ResourceGroupName'
    BaseName: '$BaseName'
    DeleteAfterHours: $DeleteAfterHours
    TestResourcesDirectory: '$TestResourcesDirectory'`n
"@ -ForegroundColor Yellow

    if($AsJob) {
        Start-Job -ScriptBlock {
            param($RepoRoot, $SubscriptionId, $ResourceGroupName, $BaseName, $testResourcesDirectory, $DeleteAfterHours)

            & "$RepoRoot/eng/common/TestResources/New-TestResources.ps1" `
                -SubscriptionId $SubscriptionId `
                -ResourceGroupName $ResourceGroupName `
                -BaseName $BaseName `
                -TestResourcesDirectory $testResourcesDirectory `
                -DeleteAfterHours $DeleteAfterHours `
                -Force

        } -ArgumentList $RepoRoot, $SubscriptionId, $ResourceGroupName, $BaseName, $TestResourcesDirectory, $DeleteAfterHours
    } else {
        & "$RepoRoot/eng/common/TestResources/New-TestResources.ps1" `
            -SubscriptionId $SubscriptionId `
            -ResourceGroupName $ResourceGroupName `
            -BaseName $BaseName `
            -TestResourcesDirectory $testResourcesDirectory `
            -DeleteAfterHours $DeleteAfterHours `
            -Force
    }
}

$jobInputs = $testablePaths | ForEach-Object {
    # the suffix is a unique-looking string appended to the resource group and resource base name
    if($Unique) {
        # actually unique suffix
        $hash = [guid]::NewGuid().ToString()
    } else {
        # Base the suffix on the path being deployed, the user's account ID, and the subscription being deployed to
        $hash = (New-StringHash $account.Id, $SubscriptionId, $_)
    }

    $suffix = $hash.ToLower().Substring(0, 8)

    return @{
        Path = $_
        SubscriptionId = $SubscriptionId
        SubscriptionName = $subscriptionName
        ResourceGroupName = $ResourceGroupName ? $ResourceGroupName : "$accountName-mcp$($suffix)"
        BaseName = $BaseName ? $BaseName : "mcp$($suffix)"
        DeleteAfterHours = $DeleteAfterHours
        TestResourcesDirectory = Resolve-Path -Path "$RepoRoot/$_/tests"
    }
}

$failedPaths = @()
if ($jobInputs.Count -eq 1 -or !$Parallel) {
    foreach ($jobInput in $jobInputs) {
        try {
            Deploy-TestResources @jobInput
        } catch {
            $failedPaths += $jobInput.Path
        }
    }
} else {
    Write-Host "Deploying resources in parallel for $($jobInputs.Count) paths..." -ForegroundColor Yellow
    Write-Host "Cancelling this script (Ctrl-C) will not stop its background jobs. To stop the jobs, you must run:" -ForegroundColor Yellow
    Write-Host "  Get-Job | Stop-Job -PassThrough | Remove-Job`n" -ForegroundColor Yellow

    Start-Sleep -Seconds 2

    $jobs = @()
    $stopwatch = [System.Diagnostics.Stopwatch]::StartNew()
    foreach ($jobInput in $jobInputs) {
        $job = Deploy-TestResources @jobInput -AsJob
        $jobs += @{ Id = $job.Id; Path = $jobInput.Path }
    }

    $ErrorActionPreference = 'Continue'
    $runningJobs = @() + $jobs
    $delay = 0
    while($true) {
        $elapsed = $stopwatch.Elapsed
        Write-Host "($($elapsed)) Checking status of deployment jobs..." -ForegroundColor Cyan

        foreach ($job in $runningJobs) {
            $job.State = Get-Job -Id $job.Id | Select-Object -ExpandProperty State
            if ($job.State -in 'Running', 'NotStarted') {
                continue
            }

            if ($job.State -in 'Failed', 'Stopped') {
                Write-Host "## Deployment job '$($job.Path)' $($job.State). ##" -ForegroundColor Red
                Receive-Job -Id $job.Id
                Write-Host ''
            } else {
                Write-Host "## Deployment job '$($job.Path)' $($job.State). ##" -ForegroundColor Green
            }

            Remove-Job -Id $job.Id
            $runningJobs = $runningJobs | Where-Object { $_.Id -ne $job.Id }
        }

        $jobs | Select-Object -Property Path, State | Format-Table -AutoSize

        if (!$runningJobs) {
            break
        }

        $delay = [Math]::Min(15, $delay + 5)
        Start-Sleep -Seconds $delay
    }

    $failedJobs = $jobs | Where-Object { $_.State -in 'Failed', 'Stopped' }
    $failedPaths = $failedJobs.Path
}

if($failedPaths) {
    $relativePath = Resolve-Path -Path "$RepoRoot/eng/scripts/Deploy-TestResources.ps1" -Relative
    Write-Host "`nTo retry failed deployments, run:" -ForegroundColor Yellow
    foreach ($path in $failedPaths) {
        Write-Host "  $relativePath -Path $path" -ForegroundColor Yellow
    }
}
