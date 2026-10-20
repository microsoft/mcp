#!/usr/bin/env pwsh

# Copyright (c) Microsoft Corporation. All rights reserved.
# Licensed under the MIT License.

#Requires -Version 7.0
#Requires -PSEdition Core

[CmdletBinding(SupportsShouldProcess = $true)]
param (
    [Parameter(Mandatory = $true)]
    [string] $ResourceGroupName,

    [Parameter()]
    [string] $Location = '',

    [Parameter()]
    [hashtable] $AdditionalParameters = @{},

    # Captures any arguments from the deployment script
    [Parameter(ValueFromRemainingArguments = $true)]
    $RemainingArguments
)

Write-Host "Running Azure.Mcp.Tools.Kusto pre-deployment script"

# Honor explicit locations; otherwise try US regions in preference order.
$candidateRegions = @(
    'westus2', 'westus3', 'eastus2', 'eastus', 'centralus',
    'northcentralus', 'southcentralus', 'westcentralus', 'westus'
)
$explicitLocation = if ($templateFileParameters.ContainsKey('location')) {
    [string]$templateFileParameters['location']
} elseif ($Location) {
    $Location
} else {
    $null
}
$regionsToCheck = if ($explicitLocation) { @($explicitLocation) } else { $candidateRegions }

$clusterSkus = Get-AzKustoClusterSku -ErrorAction Stop
foreach ($deploymentLocation in $regionsToCheck) {
    Write-Host "Checking Kusto cluster SKUs in '$deploymentLocation'..."
    $clusterSku = $clusterSkus |
        Where-Object {
            $_.Location -contains $deploymentLocation -and
                $_.ResourceType -eq 'clusters' -and
                $_.Tier -eq 'Standard' -and
                $_.Name -match '^Standard_E2[a-z]*_v\d+$' -and
            ($null -eq $_.Restriction -or @($_.Restriction).Count -eq 0)
        } |
        Sort-Object -Property Name |
        Select-Object -ExpandProperty Name -First 1
    if ($clusterSku) {
        break
    }
}

if (!$clusterSku) {
    throw "No unrestricted Standard E2 Kusto cluster SKU is available in any of: $($regionsToCheck -join ', ')."
}

Write-Host "Selected Kusto cluster SKU: $clusterSku in '$deploymentLocation'"
$templateFileParameters['location'] = $deploymentLocation
$templateFileParameters['clusterSku'] = $clusterSku
