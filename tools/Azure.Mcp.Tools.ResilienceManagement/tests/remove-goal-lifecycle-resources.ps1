param(
    [Parameter(Mandatory)] [string] $SubscriptionId,
    [Parameter(Mandatory)] [string] $ResourceGroupName,
    [string] $DedicatedServiceGroupName,
    [string] $ParentServiceGroupName,
    [switch] $ApprovedCleanup
)

$ErrorActionPreference = 'Stop'
if (!$ApprovedCleanup) {
    throw 'Obtain explicit user approval before cleanup, then supply -ApprovedCleanup.'
}

$groupJson = az group show --subscription $SubscriptionId --name $ResourceGroupName --output json
if ($LASTEXITCODE -ne 0) {
    throw 'Could not inspect the requested resource group.'
}
$group = $groupJson | ConvertFrom-Json
if ($group.tags.purpose -ne 'mcp-goals-ga-lifecycle') {
    throw 'Cleanup is restricted to the dedicated, tagged Goals lifecycle resource group.'
}

if ($DedicatedServiceGroupName) {
    if (!$ParentServiceGroupName -or
        $DedicatedServiceGroupName -notmatch '^[a-zA-Z0-9_.()-]{1,90}$' -or
        $ParentServiceGroupName -notmatch '^[a-zA-Z0-9_.()-]{1,90}$' -or
        $DedicatedServiceGroupName -eq $ParentServiceGroupName) {
        throw 'Specify distinct, valid dedicated and parent service group names.'
    }
    $childUri = "https://management.azure.com/providers/Microsoft.Management/serviceGroups/$DedicatedServiceGroupName"
    $childJson = az rest --subscription $SubscriptionId --method GET --url "${childUri}?api-version=2024-02-01-preview" --output json
    if ($LASTEXITCODE -ne 0) {
        throw 'Could not verify the dedicated child service group.'
    }
    $child = $childJson | ConvertFrom-Json
    if ($child.properties.displayName -ne 'Dedicated MCP Goals GA lifecycle fixture' -or
        $child.properties.parent.resourceId -ne "/providers/Microsoft.Management/serviceGroups/$ParentServiceGroupName") {
        throw 'The service group does not match the dedicated child fixture identity.'
    }
    $assignmentsJson = az rest --subscription $SubscriptionId --method GET --url "$childUri/providers/Microsoft.AzureResilienceManagement/goalAssignments?api-version=2026-10-01" --output json
    if ($LASTEXITCODE -ne 0 -or @(($assignmentsJson | ConvertFrom-Json).value).Count -ne 0) {
        throw 'The dedicated service group must have no goal assignments before cleanup.'
    }
}

# The assignment lifecycle deletes only its own fresh assignment. Never delete the reused parent service group.
az group delete --subscription $SubscriptionId --name $ResourceGroupName --yes
if ($LASTEXITCODE -ne 0) {
    throw 'Dedicated Goals lifecycle resource group cleanup failed.'
}
if ($DedicatedServiceGroupName) {
    az rest --subscription $SubscriptionId --method DELETE --url "${childUri}?api-version=2024-02-01-preview" --output none
    if ($LASTEXITCODE -ne 0) {
        throw 'Dedicated child service group cleanup failed.'
    }
}
