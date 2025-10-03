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

Write-Host "Communication Services resource '$BaseName' has been created." -ForegroundColor Green
Write-Host "Endpoint URL: $($DeploymentOutputs.COMMUNICATION_SERVICES_ENDPOINT)" -ForegroundColor Yellow

Write-Host "" -ForegroundColor Yellow
Write-Host "⚠️  MANUAL STEPS REQUIRED FOR SMS TESTING:" -ForegroundColor Yellow
Write-Host "" -ForegroundColor Yellow
Write-Host "1. To enable SMS functionality, you need to purchase phone number(s):" -ForegroundColor Yellow
Write-Host "   - Go to Azure portal > Communication Services > Phone numbers" -ForegroundColor Yellow
Write-Host "   - Purchase a phone number with SMS capabilities" -ForegroundColor Yellow
Write-Host "   - Set the COMMUNICATION_SERVICES_FROM_PHONE environment variable" -ForegroundColor Yellow
Write-Host "" -ForegroundColor Yellow
Write-Host "2. For testing, set the COMMUNICATION_SERVICES_TO_PHONE environment variable" -ForegroundColor Yellow
Write-Host "   to a phone number you can verify (e.g., your own phone)" -ForegroundColor Yellow
Write-Host "" -ForegroundColor Yellow
Write-Host "3. Environment variables for live tests:" -ForegroundColor Yellow
Write-Host "   COMMUNICATION_SERVICES_ENDPOINT (already set)" -ForegroundColor Yellow
Write-Host "   COMMUNICATION_SERVICES_FROM_PHONE (requires phone number purchase)" -ForegroundColor Yellow
Write-Host "   COMMUNICATION_SERVICES_TO_PHONE (your test phone number)" -ForegroundColor Yellow
Write-Host "" -ForegroundColor Yellow
Write-Host "Note: Phone number purchase and SMS charges may apply." -ForegroundColor Yellow
