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

Write-Host "Running Azure.Mcp.Tools.ServiceFabric pre-deployment script"

# westus has no VM SKUs enabled for some test subscriptions. Keep an explicit
# location override, but otherwise deploy the cluster in westus2.
$deploymentLocation = if ($templateFileParameters.ContainsKey('location')) {
    [string]$templateFileParameters['location']
} else {
    'westus2'
}

$vmSku = Get-AzComputeResourceSku -Location $deploymentLocation |
    Where-Object {
        $sku = $_
        $vCpuCapability = $sku.Capabilities | Where-Object { $_.Name -eq 'vCPUs' }
        $memoryCapability = $sku.Capabilities | Where-Object { $_.Name -eq 'MemoryGB' }
        $hasLocationRestriction = $sku.Restrictions | Where-Object { $_.Type -eq 'Location' }
        $sku.ResourceType -eq 'virtualMachines' -and
            $sku.Name -like 'Standard_D*' -and
            [int]$vCpuCapability.Value -eq 2 -and
            $null -ne $memoryCapability -and
            [double]$memoryCapability.Value -le 8 -and
            -not $hasLocationRestriction
    } |
    Sort-Object -Descending -Property {
        [double]($_.Capabilities | Where-Object { $_.Name -eq 'MemoryGB' }).Value
    } |
    Select-Object -ExpandProperty Name -First 1

if (!$vmSku) {
    throw "No unrestricted 2-vCPU Standard_D VM SKU with at most 8 GB RAM is available in '$deploymentLocation'."
}

Write-Host "Selected VM SKU: $vmSku"
$templateFileParameters['location'] = $deploymentLocation
$templateFileParameters['vmSku'] = $vmSku