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

# Add your post deployment steps here
# For example, you might want to configure resources or run additional scripts.

# $DeploymentOutputs keys are from the Bicep outputs
# Add DeploymentOutputs as a nested dictionary to match LiveTestSettings structure
$deploymentOutputs = @{
    "OpenAIAccount" = "azmcp-test"
    "OpenAIDeploymentName" = "gpt-4o-mini"
    "OpenAIAccountResourceGroup" = "static-test-resources"
}

$testSettings.Add("DeploymentOutputs", $deploymentOutputs)

# Update the test settings file with the additional properties
$testSettingsPath = Join-Path -Path $PSScriptRoot -ChildPath ".testsettings.json"
$testSettingsJson = $testSettings | ConvertTo-Json -Depth 3
Write-Host "Updating test settings file at $testSettingsPath with OpenAI configuration"
$testSettingsJson | Set-Content -Path $testSettingsPath -Force -NoNewLine


