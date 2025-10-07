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

$ledgerName = $DeploymentOutputs['CONFIDENTIAL_LEDGER_NAME']
$ledgerUrl = $DeploymentOutputs['CONFIDENTIAL_LEDGER_URL']

Write-Host "Setting up Confidential Ledger for testing: $ledgerName" -ForegroundColor Yellow
Write-Host "Ledger URL: $ledgerUrl" -ForegroundColor Gray

# Note: Confidential Ledger may take some time to become fully operational after deployment
# The ledger is ready to use once the deployment completes, but initial operations may need retry logic
Write-Host "Confidential Ledger setup complete" -ForegroundColor Green
Write-Host "Test settings saved to: $PSScriptRoot\.testsettings.json" -ForegroundColor Green
