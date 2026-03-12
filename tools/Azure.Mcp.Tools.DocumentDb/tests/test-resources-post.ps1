param(
    [string] $TenantId,
    [string] $TestApplicationId,
    [string] $ResourceGroupName,
    [string] $BaseName,
    [hashtable] $DeploymentOutputs
)

$ErrorActionPreference = "Stop"

. "$PSScriptRoot/../../../eng/common/scripts/common.ps1"
. "$PSScriptRoot/../../../eng/scripts/helpers/TestResourcesHelpers.ps1"

$testSettings = New-TestSettings @PSBoundParameters -OutputPath $PSScriptRoot

# $testSettings contains:
# - TenantId
# - TenantName
# - SubscriptionId
# - SubscriptionName
# - ResourceGroupName
# - ResourceBaseName

# $DeploymentOutputs keys are all UPPERCASE

# Save updated test settings
$testSettings | ConvertTo-Json | Out-File (Join-Path $PSScriptRoot '.testsettings.json') -Encoding UTF8

Write-Host "Test resources deployed successfully for DocumentDB"
Write-Host "Connection string saved to .testsettings.json"

Write-Host "Database seeding is handled by the C# live test setup."
Write-Host "No external mongosh dependency is required for CI."
