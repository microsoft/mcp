#!/usr/bin/env pwsh

# Copyright (c) Microsoft Corporation.
# Licensed under the MIT License.

#Requires -Version 6.0
#Requires -PSEdition Core

[CmdletBinding()]
param (
    [Parameter(Mandatory)]
    [string]
    $ResourceGroupName,

    [Parameter(Mandatory)]
    [hashtable]
    $DeploymentOutputs,

    [Parameter(Mandatory)]
    [hashtable]
    $AdditionalParameters
)

Write-Host "Running FileShares post-deployment setup..."

try {
    # Extract outputs from deployment
    $storageAccountName = $DeploymentOutputs['storageAccountName'].Value
    $fileShareName = $DeploymentOutputs['fileShareName'].Value

    Write-Host "Storage Account: $storageAccountName"
    Write-Host "File Share: $fileShareName"

    Write-Host "FileShares post-deployment setup completed successfully."
}
catch {
    Write-Error "Failed to complete FileShares post-deployment setup: $_"
    throw
}
