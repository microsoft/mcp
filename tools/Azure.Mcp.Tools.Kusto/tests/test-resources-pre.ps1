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
    [string] $DefaultClusterSku = "Standard_E2a_v4",

    [Parameter()]
    [hashtable] $AdditionalParameters = @{},

    # Captures any arguments from the deployment script
    [Parameter(ValueFromRemainingArguments = $true)]
    $RemainingArguments
)

Write-Host "Running Azure.Mcp.Tools.Kusto pre-deployment script"

$resourceGroup = Get-AzResourceGroup -Name $ResourceGroupName

$clusterSku = Get-AzKustoSku -Location $resourceGroup.Location |
    Where-Object {
        $_.ResourceType -eq 'clusters' -and
            $_.Tier -eq 'Standard' -and
            $_.Name -match '^Standard_E2[a-z]*_v\d+$' -and
        ($null -eq $_.Restriction -or @($_.Restriction).Count -eq 0)
    } |
    Sort-Object -Property Name |
    Select-Object -ExpandProperty Name -First 1

if (!$clusterSku) {
    Write-Host "No suitable Kusto cluster SKU found, using default SKU: $DefaultClusterSku"
    $clusterSku = $DefaultClusterSku
}

Write-Host "Selected Kusto cluster SKU: $clusterSku"
$templateFileParameters['clusterSku'] = $clusterSku
