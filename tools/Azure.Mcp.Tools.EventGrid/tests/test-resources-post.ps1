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

Write-Host "Running Event Grid post-deployment setup..." -ForegroundColor Yellow

try {
    # Extract outputs from deployment (note: keys are UPPERCASE)
    $eventGridTopicName = $DeploymentOutputs['EVENTGRIDTOPICNAME']
    $eventGridTopicEndpoint = $DeploymentOutputs['EVENTGRIDTOPICENDPOINT']

    Write-Host "Event Grid Topic created: $eventGridTopicName" -ForegroundColor Green
    Write-Host "Event Grid Topic Endpoint: $eventGridTopicEndpoint" -ForegroundColor Green

    # Add any EventGrid-specific post-deployment steps here
    # For example, you might want to:
    # - Publish test events to validate the topic
    # - Set up additional configurations
    # - Create test subscriptions

    Write-Host "Event Grid post-deployment setup completed successfully." -ForegroundColor Green
}
catch {
    Write-Error "Failed to complete Event Grid post-deployment setup: $_"
    throw
}