param(
    [string] $TenantId,
    [string] $TestApplicationId,
    [string] $ResourceGroupName,
    [string] $BaseName,
    [hashtable] $DeploymentOutputs,
    [hashtable] $AdditionalParameters
)

$ErrorActionPreference = "Stop"

. "$PSScriptRoot/../../../eng/common/scripts/common.ps1"
. "$PSScriptRoot/../../../eng/scripts/helpers/TestResourcesHelpers.ps1"

# Writes the .testsettings.json consumed by the recorded test harness (subscription, resource group,
# base resource name, etc.).
$testSettings = New-TestSettings @PSBoundParameters -OutputPath $PSScriptRoot

Write-Host "Resilience Management test resources deployed. Usage plan: $($testSettings.ResourceBaseName)" -ForegroundColor Green
