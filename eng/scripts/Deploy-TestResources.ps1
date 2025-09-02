param(
    [Parameter(Mandatory=$true)]
    [string]$Area,
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

$testResourcesDirectory = Resolve-Path -Path  "$RepoRoot/tools/Azure.Mcp.Tools.$($Area.ToLower())/tests" -ErrorAction SilentlyContinue
$bicepPath = "$testResourcesDirectory/test-resources.bicep"
if(!(Test-Path -Path $bicepPath)) {
    Write-Error "Test resources bicep template '$bicepPath' does not exist."
    return
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

# Base the user hash on the user's account ID and subscription being deployed to
if($Unique) {
    $hash = [guid]::NewGuid().ToString()
} else {
    $hash = (New-StringHash $account.Id, $SubscriptionId, $Area)
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
