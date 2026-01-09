#!/usr/bin/env pwsh

# Copyright (c) Microsoft Corporation.
# Licensed under the MIT License.

#Requires -Version 6.0
#Requires -PSEdition Core

[CmdletBinding()]
param (
    [string]
    $TenantId,

    [string]
    $TestApplicationId,

    [string]
    $ResourceGroupName,

    [string]
    $BaseName,

    [hashtable]
    $DeploymentOutputs,

    [string]
    $SubscriptionId,

    [string]
    $TestResourcesDirectory,

    [int]
    $DeleteAfterHours,

    [switch]
    $Force,

    [string]
    $TestApplicationSecret
)

Write-Host "Running FileShares post-deployment setup..."

try {
    # Extract outputs from deployment
    $fileShare1Name = $DeploymentOutputs['fileShare1Name'].Value
    $fileShare1Id = $DeploymentOutputs['fileShare1Id'].Value
    $fileShare2Name = $DeploymentOutputs['fileShare2Name'].Value
    $fileShare2Id = $DeploymentOutputs['fileShare2Id'].Value

    $fileShareSnapshot1Name = $DeploymentOutputs['fileShareSnapshot1Name'].Value
    $fileShareSnapshot1Id = $DeploymentOutputs['fileShareSnapshot1Id'].Value
    $fileShareSnapshot2Name = $DeploymentOutputs['fileShareSnapshot2Name'].Value
    $fileShareSnapshot2Id = $DeploymentOutputs['fileShareSnapshot2Id'].Value

    $privateEndpointName = $DeploymentOutputs['privateEndpointName'].Value
    $privateEndpointId = $DeploymentOutputs['privateEndpointId'].Value

    $testApplicationOid = $DeploymentOutputs['testApplicationOid'].Value

    Write-Host "FileShare 1: $fileShare1Name (ID: $fileShare1Id)"
    Write-Host "FileShare 2: $fileShare2Name (ID: $fileShare2Id)"
    Write-Host "FileShareSnapshot 1: $fileShareSnapshot1Name (ID: $fileShareSnapshot1Id)"
    Write-Host "FileShareSnapshot 2: $fileShareSnapshot2Name (ID: $fileShareSnapshot2Id)"
    Write-Host "Private Endpoint: $privateEndpointName (ID: $privateEndpointId)"
    Write-Host "Test Application OID: $testApplicationOid"

    Write-Host ""
    Write-Host "FileShares test resources have been successfully created:"
    Write-Host "  ✓ FileShare resources (2x Microsoft.FileShares/fileShares)"
    Write-Host "  ✓ FileShare Snapshots (2x Microsoft.FileShares/fileShares/fileShareSnapshots)"
    Write-Host "  ✓ Private Endpoint (Microsoft.Network/privateEndpoints)"
    Write-Host "  ✓ Role Assignments (Microsoft.Authorization/roleAssignments)"
    Write-Host ""
    Write-Host "Ready for MFS (Microsoft FileShares) testing and exercises."
}
catch {
    Write-Error "Failed to complete FileShares post-deployment setup: $_"
    throw
}
