# param(
#     [Parameter(Mandatory=$true, ParameterSetName="Path")]
#     [string]$Path,
    
#     [Parameter(Mandatory=$true, ParameterSetName="Tool")]
#     [string]$Tool,
    
#     [Parameter(Mandatory=$true, ParameterSetName="Area")]
#     [string]$Area,
    
#     [string]$SubscriptionId,
#     [string]$ResourceGroupName,
#     [string]$BaseName,
#     [int]$DeleteAfterHours = 12,
#     [switch]$Unique
# )

# $ErrorActionPreference = 'Stop'
# . "$PSScriptRoot/../common/scripts/common.ps1"

# function New-StringHash([string[]]$strings) {
#     $string = $strings -join ' '
#     $hash = [System.Security.Cryptography.SHA1]::Create()
#     $bytes = [System.Text.Encoding]::UTF8.GetBytes($string)
#     $hashBytes = $hash.ComputeHash($bytes)
#     return [BitConverter]::ToString($hashBytes) -replace '-', ''
# }

# function Find-TestResourcesDirectory {
#     param(
#         [string]$SearchPath,
#         [string]$Context
#     )
    
#     $bicepPath = "$SearchPath/test-resources.bicep"
#     if (Test-Path -Path $bicepPath) {
#         return $SearchPath
#     }
    
#     # Search recursively for test-resources.bicep
#     $testDirs = Get-ChildItem -Path $SearchPath -Recurse -Directory -Name "tests" -ErrorAction SilentlyContinue
#     foreach ($testDir in $testDirs) {
#         $fullTestPath = Join-Path $SearchPath $testDir
#         $bicepPath = "$fullTestPath/test-resources.bicep"
#         if (Test-Path -Path $bicepPath) {
#             return $fullTestPath
#         }
#     }
    
#     Write-Error "No test-resources.bicep found in $Context path: '$SearchPath'"
#     return $null
# }

# # Determine the test resources directory based on parameter set
# if ($PSCmdlet.ParameterSetName -eq "Path") {
#     $searchPath = Resolve-Path -Path $Path -ErrorAction SilentlyContinue
#     if (-not $searchPath) {
#         Write-Error "Path '$Path' does not exist."
#         return
#     }
#     $testResourcesDirectory = Find-TestResourcesDirectory -SearchPath $searchPath -Context "specified path"
#     $projectName = Split-Path $searchPath -Leaf
# }
# elseif ($PSCmdlet.ParameterSetName -eq "Tool") {
#     # Search for tools matching the pattern
#     $toolsPath = "$RepoRoot/tools"
#     $matchingTools = Get-ChildItem -Path $toolsPath -Directory | Where-Object { 
#         $_.Name -like "*$Tool*" -or $_.Name -like $Tool 
#     }
    
#     if ($matchingTools.Count -eq 0) {
#         Write-Error "No tools found matching pattern '$Tool' in $toolsPath"
#         return
#     }
#     elseif ($matchingTools.Count -gt 1) {
#         Write-Error "Multiple tools found matching pattern '$Tool': $($matchingTools.Name -join ', '). Please be more specific."
#         return
#     }
    
#     $toolPath = $matchingTools[0].FullName
#     $testResourcesDirectory = Find-TestResourcesDirectory -SearchPath $toolPath -Context "tool '$Tool'"
#     $projectName = $matchingTools[0].Name
# }
# elseif ($PSCmdlet.ParameterSetName -eq "Area") {
#     # Original area-based logic
#     $testResourcesDirectory = Resolve-Path -Path "$RepoRoot/tools/Azure.Mcp.Tools.$Area/tests" -ErrorAction SilentlyContinue
#     $bicepPath = "$testResourcesDirectory/test-resources.bicep"
#     if(!(Test-Path -Path $bicepPath)) {
#         Write-Error "Test resources bicep template '$bicepPath' does not exist."
#         return
#     }
#     $projectName = $Area
# }

# if (-not $testResourcesDirectory) {
#     return
# }

# # Ensure bicep file exists (for Path and Tool parameter sets)
# if ($PSCmdlet.ParameterSetName -ne "Area") {
#     $bicepPath = "$testResourcesDirectory/test-resources.bicep"
#     if(!(Test-Path -Path $bicepPath)) {
#         Write-Error "Test resources bicep template '$bicepPath' does not exist."
#         return
#     }
# }

# if($SubscriptionId) {
#     Select-AzSubscription -Subscription $SubscriptionId | Out-Null
#     $context = Get-AzContext
# } else {
#     # We don't want New-TestResources to conditionally pick a subscription for us
#     # If the user didn't specify a subscription, we explicitly use the current context's subscription
#     $context = Get-AzContext
#     $SubscriptionId = $context.Subscription.Id
# }
# $subscriptionName = $context.Subscription.Name
# $account = $context.Account

# # Base the user hash on the user's account ID and subscription being deployed to
# if($Unique) {
#     $hash = [guid]::NewGuid().ToString()
# } else {
#     $hash = (New-StringHash $account.Id, $SubscriptionId, $projectName)
# }

# $suffix = $hash.ToLower().Substring(0, 8)

# if(!$BaseName) {
#     $BaseName = "mcp$($suffix)"
# }

# if(!$ResourceGroupName) {
#     $username = $account.Id.Split('@')[0].ToLower()
#     $ResourceGroupName = "$username-mcp$($suffix)"
# }

# Push-Location $RepoRoot
# try {
#     Write-Host @"
# Deploying:
#     Project: '$projectName'
#     ParameterSet: '$($PSCmdlet.ParameterSetName)'
#     SubscriptionId: '$SubscriptionId'
#     SubscriptionName: '$subscriptionName'
#     ResourceGroupName: '$ResourceGroupName'
#     BaseName: '$BaseName'
#     DeleteAfterHours: $DeleteAfterHours
#     TestResourcesDirectory: '$testResourcesDirectory'
# "@
#     ./eng/common/TestResources/New-TestResources.ps1 `
#         -SubscriptionId $SubscriptionId `
#         -ResourceGroupName $ResourceGroupName `
#         -BaseName $BaseName `
#         -TestResourcesDirectory $testResourcesDirectory `
#         -DeleteAfterHours $DeleteAfterHours `
#         -Force
# }
# finally {
#     Pop-Location
# }

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

$testResourcesDirectory = Resolve-Path -Path  "$RepoRoot/tools/Azure.Mcp.Tools.$Area/tests" -ErrorAction SilentlyContinue
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