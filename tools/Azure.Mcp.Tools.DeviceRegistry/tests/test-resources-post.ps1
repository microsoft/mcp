# Copyright (c) Microsoft Corporation.
# Licensed under the MIT License.

# Post-deployment script for Device Registry test resources
# This script runs after the Bicep template has been deployed

param(
    [Parameter()]
    [hashtable] $DeploymentOutputs
)

Write-Host "Device Registry test resources deployed successfully."

if ($DeploymentOutputs) {
    Write-Host "Namespace Name: $($DeploymentOutputs['DEVICEREGISTRY_NAMESPACE_NAME'].Value)"
    Write-Host "Namespace ID: $($DeploymentOutputs['DEVICEREGISTRY_NAMESPACE_ID'].Value)"
}
