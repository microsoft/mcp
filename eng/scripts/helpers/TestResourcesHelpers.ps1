#Requires -Version 7

function New-TestSettings {
    <#
    .SYNOPSIS
        Retrieves test settings from the current Azure context and optionally writes them to a file.

    .DESCRIPTION
        This function retrieves the tenant, subscription, and resource group information from the current Azure context.

    .OUTPUTS
        A hashtable containing the tenant ID, subscription ID, and resource group name.
    #>
    param(
        [string] $TenantId,
        [string] $TestApplicationId,
        [string] $ResourceGroupName,
        [string] $BaseName,
        [hashtable] $DeploymentOutputs,
        [string] $OutputPath
    )

    if($env:ARM_OIDC_TOKEN -and $TenantId -and $TestApplicationId) {
        Write-Host "Using OIDC token for Azure Powershell authentication"
        Connect-AzAccount -ServicePrincipal `
            -TenantId $TenantId `
            -ApplicationId $TestApplicationId `
            -FederatedToken $env:ARM_OIDC_TOKEN
    }

    $context = Get-AzContext
    # Get current subscription ID
    $subscriptionId = $context.Subscription.Id
    $subscriptionName = $context.Subscription.Name
    $tenantId = $context.Tenant.Id
    $tenantName = $context.Tenant.Name

    # When using TME in CI, $context.Tenant.Name is empty so we use a map
    # $context.Tenant.Name still works for local dev
    if(!$tenantName) {
        $tenantName = switch($tenantId) {
            '70a036f6-8e4d-4615-bad6-149c02e7720d' { 'TME Organization' }
            '72f988bf-86f1-41af-91ab-2d7cd011db47' { 'Microsoft' }
        }
    }

    $testSettings = [ordered]@{
        TenantId = $tenantId
        TenantName = $tenantName
        SubscriptionId = $subscriptionId
        SubscriptionName = $subscriptionName
        ResourceGroupName = $ResourceGroupName
        ResourceBaseName = $BaseName
    }

    # Add DeploymentOutputs if provided
    if($DeploymentOutputs -and $DeploymentOutputs.Count -gt 0) {
        $testSettings.DeploymentOutputs = $DeploymentOutputs
    }

    if($OutputPath) {
        if(Test-Path -Path $OutputPath -PathType Container) {
            $OutputPath = Join-Path -Path $OutputPath -ChildPath ".testsettings.json"
        }

        # Create our test settings file
        $testSettingsJson = $testSettings | ConvertTo-Json
        Write-Host "Creating test settings file at $OutputPath`:`n$testSettingsJson"
        $testSettingsJson | Set-Content -Path $OutputPath -Force -NoNewLine
    }

    return $testSettings
}
