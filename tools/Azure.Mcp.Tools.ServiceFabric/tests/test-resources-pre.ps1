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
    [string] $DefaultVmSku = "Standard_D2s_v3",

    [Parameter()]
    [hashtable] $AdditionalParameters = @{},

    # Captures any arguments from the deployment script
    [Parameter(ValueFromRemainingArguments = $true)]
    $RemainingArguments
)

Write-Host "Running Azure.Mcp.Tools.ServiceFabric pre-deployment script"

$resourceGroup = Get-AzResourceGroup -Name $ResourceGroupName

$vmSku = Get-AzComputeResourceSku -Location $resourceGroup.Location |
    Where-Object {
        $sku = $_
        $vCpuCapability = $sku.Capabilities | Where-Object { $_.Name -eq 'vCPUs' }
        $memoryCapability = $sku.Capabilities | Where-Object { $_.Name -eq 'MemoryGB' }
        $sku.ResourceType -eq 'virtualMachines' -and
            $sku.Name -like 'Standard_D*' -and
            [int]$vCpuCapability.Value -eq 2 -and
            $null -ne $memoryCapability -and
            [double]$memoryCapability.Value -le 8
    } |
    Sort-Object -Descending -Property {
        [double]($_.Capabilities | Where-Object { $_.Name -eq 'MemoryGB' }).Value
    } |
    Select-Object -ExpandProperty Name -First 1

if (!$vmSku) {
    Write-Host "No suitable VM SKU found, using default VM SKU: $DefaultVmSku"
    $vmSku = $DefaultVmSku
}

Write-Host "Selected VM SKU: $vmSku"
$templateFileParameters['vmSku'] = $vmSku