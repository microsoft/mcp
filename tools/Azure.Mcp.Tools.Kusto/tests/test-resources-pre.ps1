#!/usr/bin/env pwsh

# Copyright (c) Microsoft Corporation. All rights reserved.
# Licensed under the MIT License.

#Requires -Version 6.0
#Requires -PSEdition Core

[CmdletBinding(SupportsShouldProcess = $true)]
param (
    [Parameter(Mandatory = $true)]
    [string] $ResourceGroupName,

    [Parameter()]
    [hashtable] $AdditionalParameters = @{},

    # Captures any arguments from the deployment script
    [Parameter(ValueFromRemainingArguments = $true)]
    $RemainingArguments
)

Write-Host "Running Azure.Mcp.Tools.Kusto pre-deployment script"

# westus can restrict the Kusto SKUs used by these tests. Keep an explicit
# location override, but otherwise deploy the cluster in westus2.
$deploymentLocation = if ($templateFileParameters.ContainsKey('location')) {
    [string]$templateFileParameters['location']
} else {
    'westus2'
}

$clusterSku = Get-AzKustoSku -Location $deploymentLocation |
    Where-Object {
        $_.ResourceType -eq 'clusters' -and
            $_.Tier -eq 'Standard' -and
            $_.Name -match '^Standard_E2[a-z]*_v\d+$' -and
        ($null -eq $_.Restriction -or @($_.Restriction).Count -eq 0)
    } |
    Sort-Object -Property Name |
    Select-Object -ExpandProperty Name -First 1

if (!$clusterSku) {
    throw "No unrestricted Standard E2 Kusto cluster SKU is available in '$deploymentLocation'."
}

Write-Host "Selected Kusto cluster SKU: $clusterSku"
$templateFileParameters['location'] = $deploymentLocation
$templateFileParameters['clusterSku'] = $clusterSku
