# Copyright (c) Microsoft Corporation.
# Licensed under the MIT License.

# Invoked by New-TestResources.ps1 before test-resources.bicep is deployed. The drills module
# deploys into a dedicated resource group, which must exist before the main deployment.
param(
    [string] $ResourceGroupName
)

$ErrorActionPreference = 'Stop'

$mainGroup = Get-AzResourceGroup -Name $ResourceGroupName
$tags = @{}
if ($mainGroup.Tags) {
    foreach ($key in $mainGroup.Tags.Keys) { $tags[$key] = $mainGroup.Tags[$key] }
}
# Fixture scripts refuse to configure or delete groups without this tag.
$tags['Purpose'] = 'mcp-drills-ga-live-tests'

$drillsGroupName = "$ResourceGroupName-drills"
foreach ($name in @($drillsGroupName, "$drillsGroupName-delete")) {
    $existing = Get-AzResourceGroup -Name $name -ErrorAction SilentlyContinue
    if ($existing) {
        if ($existing.Tags['Purpose'] -ne 'mcp-drills-ga-live-tests') {
            throw "Refusing to reuse resource group '$name' without the dedicated drills fixture tag."
        }
        continue
    }

    Write-Host "Creating drills fixture resource group '$name' in '$($mainGroup.Location)'."
    New-AzResourceGroup -Name $name -Location $mainGroup.Location -Tag $tags | Out-Null
}
