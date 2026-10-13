# Copyright (c) Microsoft Corporation.
# Licensed under the MIT License.

# Post-deployment script for Azure IoT Operations test resources.
# This script runs after the Bicep template has been deployed.

param(
    [Parameter()]
    [hashtable] $DeploymentOutputs
)

Write-Host "IoT Operations test resources deployed successfully."

if ($DeploymentOutputs) {
    Write-Host "Resource Group: $($DeploymentOutputs['IOTOPERATIONS_RESOURCE_GROUP'].Value)"
    Write-Host "Location: $($DeploymentOutputs['IOTOPERATIONS_LOCATION'].Value)"
}
