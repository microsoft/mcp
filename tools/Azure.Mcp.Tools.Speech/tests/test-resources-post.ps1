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

Write-Host "Running Speech post-deployment setup..."

try {
    # Extract outputs from deployment
    $aiServicesName = $DeploymentOutputs['aiServicesName']
    $aiServicesEndpoint = $DeploymentOutputs['aiServicesEndpoint']
    $aiServicesLocation = $DeploymentOutputs['aiServicesLocation']

    Write-Host "Azure AI Services '$aiServicesName' deployed successfully."
    Write-Host "Endpoint: $aiServicesEndpoint"
    Write-Host "Location: $aiServicesLocation"

    Write-Host "Azure AI Services post-deployment setup completed successfully."
}
catch {
    Write-Error "Failed to complete Azure AI Services post-deployment setup: $_"
    throw
}
