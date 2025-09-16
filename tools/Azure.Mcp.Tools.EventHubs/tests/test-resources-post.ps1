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

# Create EventHub namespace and EventHub
$eventHubNamespaceName = $BaseName
$eventHubName = "test-hub"

# Add EventHub details to test settings
$testSettings["EVENTHUB_NAMESPACE_NAME"] = $eventHubNamespaceName
$testSettings["EVENTHUB_NAME"] = $eventHubName
$testSettings["EVENTHUB_CONNECTION_STRING"] = (Get-AzEventHubKey -ResourceGroupName $ResourceGroupName -NamespaceName $eventHubNamespaceName -AuthorizationRuleName "RootManageSharedAccessKey").PrimaryConnectionString

Write-TestSettings -TestSettings $testSettings -OutputPath $PSScriptRoot