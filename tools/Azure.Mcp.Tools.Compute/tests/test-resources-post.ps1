#!/usr/bin/env pwsh

# Copyright (c) Microsoft Corporation.
# Licensed under the MIT License.

#Requires -Version 6.0
#Requires -PSEdition Core

param(
    [string] $TenantId,
    [string] $TestApplicationId,
    [string] $ResourceGroupName,
    [string] $BaseName,
    [hashtable] $DeploymentOutputs
)

$ErrorActionPreference = "Stop"

. "$PSScriptRoot/../../../eng/common/scripts/common.ps1"
. "$PSScriptRoot/../../../eng/scripts/helpers/TestResourcesHelpers.ps1"

Write-Host "Running Compute post-deployment setup..."

try {
    # Create test settings file
    $testSettings = New-TestSettings @PSBoundParameters -OutputPath $PSScriptRoot

    # Extract outputs from deployment
    $diskName = $DeploymentOutputs['diskName']
    $location = $DeploymentOutputs['location']

    Write-Host "Disk '$diskName' created at location '$location'"
    Write-Host "Test settings file created: $($testSettings.SettingsFile)"
    Write-Host "Compute post-deployment setup completed successfully."
}
catch {
    Write-Error "Failed to complete Compute post-deployment setup: $_"
    throw
}
