param(
    [Parameter(Mandatory=$true, ParameterSetName="ByPath")]
    [string]$Path,
    [Parameter(Mandatory=$true, ParameterSetName="ByTool")]
    [string]$Tool,
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

switch ($PSCmdlet.ParameterSetName) {
    'ByPath' {
        if(!(Test-Path -Path $Path)) {
            Write-Error "Path '$Path' does not exist."
            exit 1
        }
    }
    'ByTool' {
        $tools = Get-ChildItem -Path "$RepoRoot/tools" -Directory

        $match = $tools | Where-Object { $_.Name -ceq $Tool }
        if ($match) {
            $Path = $match.FullName
        } else {
            $matchingTools = $tools | Where-Object { $_.Name -like "*$Tool*" }
            if ($matchingTools.Count -eq 1) {
                $match = $matchingTools[0]
                Write-Host "Found tool '$($match.Name)'."
                $Path = $match.FullName
            } elseif ($matchingTools.Count -gt 1) {
                $matchNames = $matchingTools | ForEach-Object { $_.Name }
                Write-Error "Multiple tools match '$Tool':`n  $($matchNames -join "`n  ")`nPlease specify a more specific tool name."
                exit 1
            } else {
                Write-Error "No tool matches '*$Tool*'."
                exit 1
            }
        }
    }
}

$testResourcesDirectory = Resolve-Path -Path  "$Path/tests" -ErrorAction SilentlyContinue
$bicepPath = "$testResourcesDirectory/test-resources.bicep"
if(!(Test-Path -Path $bicepPath)) {
    Write-Error "Test resources bicep template '$bicepPath' does not exist."
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
