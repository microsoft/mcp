#!/usr/bin/env pwsh

# Copyright (c) Microsoft Corporation.
# Licensed under the MIT License.

#Requires -Version 6.0
#Requires -PSEdition Core

[CmdletBinding()]
param (
    [Parameter(Mandatory)]
    [hashtable] $DeploymentOutputs,

    [Parameter(Mandatory)]
    [hashtable] $AdditionalParameters
)

Write-Host "Running DPS post-deployment setup..."

try {
    # Extract outputs from deployment
    $dpsName = $DeploymentOutputs['Dps']['dpsInstanceName']['value']
    $resourceGroup = $AdditionalParameters['ResourceGroupName']

    Write-Host "DPS instance '$dpsName' deployed successfully in resource group '$resourceGroup'."
    Write-Host "DPS post-deployment setup completed successfully."
}
catch {
    Write-Error "Failed to complete DPS post-deployment setup: $_"
    throw
}
