#!/usr/bin/env pwsh

# Copyright (c) Microsoft Corporation.
# Licensed under the MIT License.

#Requires -Version 6.0
#Requires -PSEdition Core

[CmdletBinding()]
param (
    [Parameter(Mandatory)]
    [string] $ResourceGroupName,
    [Parameter(Mandatory)]
    [hashtable] $DeploymentOutputs,
    [Parameter(Mandatory)]
    [hashtable] $AdditionalParameters
)

Write-Host "Running EventHubs post-deployment setup..."

try {
    # Extract outputs from deployment
    $eventHubsNamespaceName = $DeploymentOutputs['eventHubsNamespaceName']
    $eventHubName = $DeploymentOutputs['eventHubName']
    
    Write-Host "EventHubs namespace: $eventHubsNamespaceName"
    Write-Host "EventHub: $eventHubName"
    
    # Create test settings file for integration tests
    . "$PSScriptRoot/../../../eng/common/scripts/common.ps1"
    . "$PSScriptRoot/../../../eng/scripts/helpers/TestResourcesHelpers.ps1"
    
    $testSettings = New-TestSettings @PSBoundParameters -OutputPath $PSScriptRoot
    
    Write-Host "EventHubs post-deployment setup completed successfully."
}
catch {
    Write-Error "Failed to complete EventHubs post-deployment setup: $_"
    exit 1
    throw
}