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

# $DeploymentOutputs keys are from the Bicep outputs and are all UPPERCASE
# Add DeploymentOutputs as a nested dictionary to match LiveTestSettings structure
$testSettings["DeploymentOutputs"] = @{
    "OpenAIAccount" = $DeploymentOutputs.OPENAIOACCOUNT
    "OpenAIDeploymentName" = $DeploymentOutputs.OPENAIDEPLOYMENTNAME
    "OpenAIAccountResourceGroup" = $DeploymentOutputs.OPENAIACCOUNTRESOURCEGROUP
}


