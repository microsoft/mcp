param(
    [Parameter(Mandatory)][string] $Subscription,
    [Parameter(Mandatory)][string] $Tenant,
    [Parameter(Mandatory)][string] $ServiceGroup,
    [Parameter(Mandatory)][string] $Template,
    [Parameter(Mandatory)][string] $Assignment,
    [string] $OutputDirectory = "$PSScriptRoot\bin\GoalResourceFixture"
)

$ErrorActionPreference = 'Stop'
$account = az account show --subscription $Subscription -o json | ConvertFrom-Json
if ($LASTEXITCODE -ne 0 -or $account.tenantId -ne $Tenant) {
    throw 'The selected subscription is not authenticated in the requested tenant.'
}
foreach ($name in @($ServiceGroup, $Template, $Assignment)) {
    if ($name -notmatch '^[a-zA-Z0-9-]+$') { throw 'Fixture names must contain only letters, numbers, and hyphens.' }
}
New-Item -ItemType Directory -Path $OutputDirectory -Force | Out-Null
$base = "https://management.azure.com/providers/Microsoft.Management/serviceGroups/$ServiceGroup/providers/Microsoft.AzureResilienceManagement"
$api = '2026-04-01-preview'

function New-FixtureResource([string] $Path, [hashtable] $Body, [string] $FileName) {
    # Never overwrite an existing goal fixture, including an earlier failed assignment.
    $existing = az rest --method get --url "$Path`?api-version=$api" --subscription $Subscription -o json 2>&1
    if ($LASTEXITCODE -eq 0) { throw "Refusing to overwrite existing fixture: $Path" }
    if ("$existing" -notmatch 'NotFound|ResourceNotFound|404') { throw "Cannot verify fixture absence: $existing" }
    $payload = Join-Path $OutputDirectory $FileName
    $Body | ConvertTo-Json -Depth 20 | Set-Content -Path $payload
    az rest --method put --url "$Path`?api-version=$api" --subscription $Subscription --body "@$payload" --query '{name:name,state:properties.provisioningState}' -o json
    if ($LASTEXITCODE -ne 0) { throw "Fixture creation failed: $Path" }
    for ($attempt = 0; $attempt -lt 24; $attempt++) {
        $resource = az rest --method get --url "$Path`?api-version=$api" --subscription $Subscription -o json | ConvertFrom-Json
        if ($LASTEXITCODE -ne 0) { throw "Fixture read failed: $Path" }
        $state = $resource.properties.provisioningState
        if ($state -eq 'Succeeded') { return }
        if ($state -eq 'Failed') { throw "Fixture provisioning failed: $Path. Inspect its service error and operation status." }
        Start-Sleep -Seconds 10
    }
    throw "Fixture provisioning did not complete within four minutes: $Path"
}

New-FixtureResource "$base/goalTemplates/$Template" @{
    properties = @{
        goalType = 'Resiliency'
        requireHighAvailability = 'Required'
        requireDisasterRecovery = 'NotRequired'
        regionalRecoveryPointObjective = 'PT15M'
        regionalRecoveryTimeObjective = 'PT30M'
    }
} 'template.json'
New-FixtureResource "$base/goalAssignments/$Assignment" @{
    properties = @{
        goalAssignmentType = 'Resiliency'
        goalTemplateId = "/providers/Microsoft.Management/serviceGroups/$ServiceGroup/providers/Microsoft.AzureResilienceManagement/goalTemplates/$Template"
    }
} 'assignment.json'
@{ ServiceGroup = $ServiceGroup; Template = $Template; Assignment = $Assignment } |
    ConvertTo-Json | Set-Content (Join-Path $OutputDirectory 'fixture.json')
