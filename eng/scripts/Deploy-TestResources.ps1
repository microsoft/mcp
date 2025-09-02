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

$toolPaths = Get-ChildItem -Path "$RepoRoot/tools" -Directory
| Where-Object { Test-Path "$_/tests/test-resources.bicep" }

$serverPaths = Get-ChildItem -Path "$RepoRoot/servers" -Directory
| Where-Object { Test-Path "$_/tests/test-resources.bicep" }

$corePaths = Get-ChildItem -Path "$RepoRoot/core" -Directory
| Where-Object { Test-Path "$_/tests/test-resources.bicep" }

$allPaths = $toolPaths + $serverPaths + $corePaths

$filteredPaths = @()
foreach ($path in $allPaths) {
    foreach ($filter in $Paths) {
        if ($path -like $filter) {
            $filteredPaths += $path
            break
        }
    }
}

if ($filteredPaths.Count -eq 0) {
    Write-Error "No paths with test resources match the specified filters: $($Areas -join ', ')"
    exit 1
}

foreach ($area in $filteredAreas) {
    $testResourcesDirectory = Resolve-Path -Path  "$Path/tests" -ErrorAction SilentlyContinue
    $bicepPath = "$testResourcesDirectory/test-resources.bicep"

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

    # Base the user hash on the user's account ID and subscription being deployed to
    if($Unique) {
        $hash = [guid]::NewGuid().ToString()
    } else {
        $hash = (New-StringHash $account.Id, $SubscriptionId, $Path)
    }

    $suffix = $hash.ToLower().Substring(0, 8)

    if(!$BaseName) {
        $BaseName = "mcp$($suffix)"
    }

    if(!$ResourceGroupName) {
        $username = $account.Id.Split('@')[0].ToLower()
        $ResourceGroupName = "$username-mcp$($suffix)"
    }

    Push-Location $RepoRoot
    try {
        Write-Host @"
Deploying:
    SubscriptionId: '$SubscriptionId'
    SubscriptionName: '$subscriptionName'
    ResourceGroupName: '$ResourceGroupName'
    BaseName: '$BaseName'
    DeleteAfterHours: $DeleteAfterHours
    TestResourcesDirectory: '$testResourcesDirectory'
"@
        ./eng/common/TestResources/New-TestResources.ps1 `
            -SubscriptionId $SubscriptionId `
            -ResourceGroupName $ResourceGroupName `
            -BaseName $BaseName `
            -TestResourcesDirectory $testResourcesDirectory `
            -DeleteAfterHours $DeleteAfterHours `
            -Force
    }
    finally {
        Pop-Location
    }
}
