param(
    [Parameter(Mandatory=$true)]
    [string[]]$Paths,
    [string]$SubscriptionId,
    [string]$ResourceGroupName,
    [string]$BaseName,
    [int]$DeleteAfterHours = 12,
    [switch]$Unique
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
| ForEach-Object {
    $relative = Resolve-Path -Path $_ -Relative -RelativeBasePath $RepoRoot
    $relative.Replace('\', '/').TrimStart('./')
 }

$filteredPaths = @()
foreach ($path in $testablePaths) {
    foreach ($filter in $Paths) {
        if ($path -like $filter.Replace('\', '/')) {
            $filteredPaths += $path
            break
        }
    }
}

if ($filteredPaths.Count -eq 0) {
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
Deploying:
    Path: '$Path'
    SubscriptionId: '$SubscriptionId'
    SubscriptionName: '$SubscriptionName'
    ResourceGroupName: '$ResourceGroupName'
    BaseName: '$BaseName'
    DeleteAfterHours: $DeleteAfterHours
    TestResourcesDirectory: '$TestResourcesDirectory'`n
"@ -ForegroundColor Yellow
    if($AsJob) {
       Start-ThreadJob -ScriptBlock {
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

$jobInputs = $filteredPaths | ForEach-Object {
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

if ($jobInputs.Count -eq 1) {
    $jobInput = $jobInputs[0]
    Deploy-TestResources @jobInput
} else {
    $jobs = @()
    $stopwatch = [System.Diagnostics.Stopwatch]::StartNew()
    foreach ($jobInput in $jobInputs) {
        $job = Deploy-TestResources @jobInput -AsJob
        $jobs += @{ Id = $job.Id; Path = $jobInput.Path }
    }

    $ErrorActionPreference = 'Continue'
    while($true) {
        $elapsed = $stopwatch.Elapsed
        Write-Host "`n($($elapsed)) Checking status of deployment jobs..." -ForegroundColor Cyan

        foreach ($job in $jobs) {
            $jobState = Get-Job -Id $job.Id | Select-Object -ExpandProperty State
            if ($jobState -in 'Failed', 'Stopped') {
                Write-Warning "  Deployment job $($job.Path) $jobState. Job ID: $($job.Id)"
                Receive-Job -Id $job.Id
                Remove-Job -Id $job.Id
                $jobs = @($jobs | Where-Object { $_.Id -ne $job.Id })
            } elseif ($jobState -eq 'Completed') {
                Write-Host "  Deployment job $($job.Path) completed. Job ID: $($job.Id)"
                Receive-Job -Id $job.Id
                Remove-Job -Id $job.Id
                $jobs = @($jobs | Where-Object { $_.Id -ne $job.Id })
            } else {
                Write-Host "  Deployment job $($job.Path) $jobState. Job ID: $($job.Id)"
            }
        }

        if ($jobs.Count -eq 0) {
            break
        }

        Start-Sleep -Seconds 15
    }
}
