# Copyright (c) Microsoft Corporation.
# Licensed under the MIT License.

# Post-deployment script for Azure IoT Operations test resources.
# This script runs after the Bicep template has been deployed.
# Script requires that all the standard parameters exist.

param(
    [string] $TenantId,
    [string] $TestApplicationId,
    [string] $ResourceGroupName,
    [string] $BaseName,
    [hashtable] $DeploymentOutputs,
    [hashtable] $AdditionalParameters
)

$ErrorActionPreference = "Stop"

. "$PSScriptRoot/../../../eng/common/scripts/common.ps1"
. "$PSScriptRoot/../../../eng/scripts/helpers/TestResourcesHelpers.ps1"

New-TestSettings @PSBoundParameters -OutputPath $PSScriptRoot | Out-Null

Write-Host "IoT Operations test resources deployed successfully."

if ($DeploymentOutputs) {
    Write-Host "Resource Group: $($DeploymentOutputs['IOTOPERATIONS_RESOURCE_GROUP'])"
    Write-Host "Location: $($DeploymentOutputs['IOTOPERATIONS_LOCATION'])"
}
