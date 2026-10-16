# Copyright (c) Microsoft Corporation.
# Licensed under the MIT License.

#Requires -Version 7
#Requires -Modules Az.Accounts, Az.Resources

[CmdletBinding()]
param(
    [Parameter(Mandatory)]
    [string] $ResourceGroupName,
    [string] $DeploymentName = 'drills-ga-fixtures',
    [string] $DeleteAssetResourceGroupName = "$ResourceGroupName-delete",
    [switch] $DeleteOnly,
    [switch] $ActionsOnly
)

$ErrorActionPreference = 'Stop'
if ($DeleteOnly -and $ActionsOnly) { throw 'Choose either DeleteOnly or ActionsOnly, not both.' }
$context = Get-AzContext
$subscriptionId = $context.Subscription.Id
$tenantId = $context.Tenant.Id
$apiVersion = '2026-10-01'
$resourceGroup = Get-AzResourceGroup -Name $ResourceGroupName
if ($resourceGroup.Tags['Purpose'] -ne 'mcp-drills-ga-live-tests') {
    throw 'Refusing to configure a resource group that is not a dedicated Drills GA fixture.'
}
$deployment = Get-AzResourceGroupDeployment -ResourceGroupName $ResourceGroupName -Name $DeploymentName
if ($deployment.ProvisioningState -ne 'Succeeded') {
    throw "Fixture infrastructure is not ready: $($deployment.ProvisioningState)."
}
$outputs = $deployment.Outputs
$vmId = $outputs.vmId.Value
$automationId = $outputs.automationAccountId.Value
$healthModelId = $outputs.healthModelId.Value
$groupId = "/subscriptions/$subscriptionId/resourceGroups/$ResourceGroupName"
if (!$vmId.StartsWith("$groupId/providers/Microsoft.Compute/virtualMachines/", [StringComparison]::OrdinalIgnoreCase)) {
    throw 'The target VM must belong to the dedicated fixture resource group.'
}

function Invoke-FixtureArm {
    param(
        [string] $Path,
        [string] $Method = 'GET',
        [object] $Body,
        [switch] $AllowNotFound,
        [switch] $Action,
        [switch] $WaitForLinkedAuthorization,
        [string] $ContentType = 'application/json'
    )
    $uri = if ($Path.StartsWith('/')) { "https://management.azure.com$Path" } else { $Path }
    if (([Uri]$uri).Host -ne 'management.azure.com' -or ([Uri]$uri).Scheme -ne 'https') {
        throw 'Fixture requests and operation polling must use public Azure ARM.'
    }
    $safePath = ([Uri]$uri).AbsolutePath
    if ($Method -ne 'GET') { Write-Host "$Method $safePath" }
    $headers = @{}
    if ($Action) {
        $operationId = [Guid]::NewGuid().ToString()
        $headers['operation-id'] = $operationId
        $uri += "&operationId=$operationId"
    }
    $parameters = @{
        Uri = $uri
        Method = $Method
        Authentication = 'Bearer'
        Token = (Get-AzAccessToken -ResourceUrl 'https://management.azure.com/').Token
        Headers = $headers
        ContentType = $ContentType
        SkipHttpErrorCheck = $true
    }
    if ($null -ne $Body) {
        $parameters.Body = if ($Body -is [string]) { $Body } else { ConvertTo-Json $Body -Depth 30 -Compress }
    }
    $deadline = [DateTime]::UtcNow.AddMinutes(5)
    $transientAttempts = 0
    do {
        try {
            $response = Invoke-WebRequest @parameters
        }
        catch [System.Net.Http.HttpRequestException] {
            throw [InvalidOperationException]::new("$Method $safePath failed before receiving a complete ARM response; inspect resource state before retrying.", $_.Exception)
        }
        if ($Method -in @('GET', 'PUT') -and $response.StatusCode -in @(429, 500, 502, 503, 504) -and $transientAttempts -lt 3) {
            $transientAttempts++
            Write-Warning "$Method $safePath returned HTTP $($response.StatusCode); retrying the idempotent request."
            Start-Sleep -Seconds (15 * $transientAttempts)
            continue
        }
        if (!$WaitForLinkedAuthorization -or $response.StatusCode -ne 403 -or
            ($response.Content | ConvertFrom-Json).error.code -ne 'LinkedAuthorizationFailed' -or
            [DateTime]::UtcNow -ge $deadline) { break }
        Write-Host 'Waiting for linked service-group authorization to propagate.'
        Start-Sleep -Seconds 15
    } while ($true)
    if ($response.StatusCode -eq 404 -and $AllowNotFound) {
        return $null
    }
    if ($response.StatusCode -notin @(200, 201, 202, 204)) {
        throw "$Method $safePath failed ($($response.StatusCode)): $($response.Content)"
    }
    return $response
}

function Get-FixtureResource {
    param([string] $Path, [switch] $AllowNotFound)
    $response = Invoke-FixtureArm -Path $Path -AllowNotFound:$AllowNotFound
    if ($null -ne $response -and $response.Content) {
        return $response.Content | ConvertFrom-Json -AsHashtable
    }
}

function Wait-FixtureResource {
    param([string] $Path)
    $deadline = [DateTime]::UtcNow.AddMinutes(15)
    do {
        $resource = Get-FixtureResource -Path $Path -AllowNotFound
        $state = $resource.properties.provisioningState
        if ($state -eq 'Succeeded') { return $resource }
        if ($state -in @('Failed', 'Canceled')) {
            throw "Fixture provisioning $state at ${Path}: $(ConvertTo-Json $resource.properties.errorDetails -Depth 15 -Compress)"
        }
        Start-Sleep -Seconds 10
    } while ([DateTime]::UtcNow -lt $deadline)
    throw "Timed out provisioning $Path. Last state: $state."
}

function New-FixtureResource {
    param([string] $Path, [hashtable] $Body, [switch] $WaitForLinkedAuthorization)
    $existing = Get-FixtureResource -Path $Path -AllowNotFound
    if (!$existing) {
        $response = Invoke-FixtureArm -Path $Path -Method PUT -Body $Body -WaitForLinkedAuthorization:$WaitForLinkedAuthorization
        Wait-FixtureOperation -Response $response -Path $Path
    }
    return Wait-FixtureResource -Path $Path
}

function Add-FixtureRole {
    param([string] $Scope, [string] $PrincipalId, [string] $RoleId)
    if (!$PrincipalId) { throw "A managed identity is required for the role at $Scope." }
    $path = "$Scope/providers/Microsoft.Authorization/roleAssignments"
    $assignments = Get-FixtureResource -Path "$path`?api-version=2022-04-01&`$filter=atScope()"
    $existing = $assignments.value | Where-Object {
        $_.properties.principalId -eq $PrincipalId -and $_.properties.roleDefinitionId.EndsWith("/$RoleId")
    }
    if (!$existing) {
        Invoke-FixtureArm -Method PUT -Path "$path/$([Guid]::NewGuid())?api-version=2022-04-01" -Body @{
            properties = @{
                principalId = $PrincipalId
                principalType = 'ServicePrincipal'
                roleDefinitionId = "/providers/Microsoft.Authorization/roleDefinitions/$RoleId"
            }
        } | Out-Null
    }
}

function Invoke-FixtureAction {
    param([string] $Path, [hashtable] $Body)
    $response = Invoke-FixtureArm -Method POST -Action -Path $Path -Body $Body
    Wait-FixtureOperation -Response $response -Path $Path
}

function Wait-FixtureOperation {
    param([object] $Response, [string] $Path)
    $pollUri = $response.Headers['Azure-AsyncOperation'] | Select-Object -First 1
    if (!$pollUri) { $pollUri = $response.Headers['Location'] | Select-Object -First 1 }
    if (!$pollUri) {
        if ($response.StatusCode -eq 202) { throw "Accepted operation has no polling URL: $Path." }
        return
    }
    $deadline = [DateTime]::UtcNow.AddMinutes(15)
    do {
        Start-Sleep -Seconds 10
        $poll = Invoke-FixtureArm -Path $pollUri
        $payload = if ($poll.Content) { $poll.Content | ConvertFrom-Json } else { $null }
        $status = $payload.status ?? $payload.properties.provisioningState
        if ($status -in @('Failed', 'Canceled')) { throw "Fixture operation failed: $($poll.Content)" }
        if ($status -eq 'Succeeded' -or ($poll.StatusCode -in @(200, 204) -and !$status)) { return }
    } while ([DateTime]::UtcNow -lt $deadline)
    throw "Timed out waiting for $Path. Last operation status: $status."
}

function Wait-FixtureServiceGroupIndex {
    param([string] $Id)
    $deadline = [DateTime]::UtcNow.AddMinutes(15)
    do {
        $response = Invoke-FixtureArm -Method POST -Path '/providers/Microsoft.ResourceGraph/resources?api-version=2024-04-01' -Body @{
            query = "resourcecontainers | where id =~ '$Id' | project id"
        }
        if (($response.Content | ConvertFrom-Json).count -eq 1) { return }
        Start-Sleep -Seconds 15
    } while ([DateTime]::UtcNow -lt $deadline)
    throw "Service group $Id exists in ARM but is not indexed in tenant Resource Graph. Dependent provisioning cannot proceed."
}

$serviceGroupName = $outputs.serviceGroupName.Value
$lifecycleServiceGroupName = $outputs.lifecycleServiceGroupName.Value
$creationServiceGroupName = $outputs.creationServiceGroupName.Value
if (!$creationServiceGroupName) {
    throw 'Redeploy the current fixture template to obtain the isolated creation service group name.'
}
$serviceGroupId = "/providers/Microsoft.Management/serviceGroups/$serviceGroupName"
foreach ($name in @($serviceGroupName, $lifecycleServiceGroupName, $creationServiceGroupName)) {
    Write-Host "Preparing isolated service group $name."
    $id = "/providers/Microsoft.Management/serviceGroups/$name"
    New-FixtureResource -Path "$id`?api-version=2024-02-01-preview" -Body @{
        properties = @{ displayName = $name; parent = @{ resourceId = "/providers/Microsoft.Management/serviceGroups/$tenantId" } }
    } | Out-Null
    Wait-FixtureServiceGroupIndex -Id $id
}
$settingsOutputs = @{}
foreach ($key in $outputs.Keys) {
    if ($null -ne $outputs[$key].Value) { $settingsOutputs[$key.ToUpperInvariant()] = [string]$outputs[$key].Value }
}
$deleteGroupId = "/subscriptions/$subscriptionId/resourceGroups/$DeleteAssetResourceGroupName"
if ($DeleteAssetResourceGroupName -eq $ResourceGroupName) {
    throw 'The deletion fixture must use a separate asset resource group.'
}
$deleteGroup = Get-FixtureResource -Path "$deleteGroupId`?api-version=2024-03-01" -AllowNotFound
if ($deleteGroup -and $deleteGroup.tags.Purpose -ne 'mcp-drills-ga-live-tests') {
    throw 'Refusing to reuse a deletion asset group without the dedicated fixture tag.'
}
if (!$deleteGroup) {
    Invoke-FixtureArm -Method PUT -Path "$deleteGroupId`?api-version=2024-03-01" -Body @{
        location = $outputs.location.Value
        tags = $resourceGroup.Tags
    } | Out-Null
}
$lifecycleId = "/providers/Microsoft.Management/serviceGroups/$lifecycleServiceGroupName"
New-FixtureResource -WaitForLinkedAuthorization -Path "$($outputs.usagePlanId.Value)/enrollments/$lifecycleServiceGroupName`?api-version=$apiVersion" -Body @{
    properties = @{ serviceGroupId = $lifecycleId }
} | Out-Null
# Drill creation requires a nonempty group; this disk cannot affect the lifecycle VM.
New-FixtureResource -Path "$deleteGroupId/providers/Microsoft.Compute/disks/delete-fixture-disk?api-version=2024-03-02" -Body @{
    location = $outputs.location.Value
    zones = @('1')
    tags = @{ purpose = 'mcp-drills-ga-live-tests' }
    sku = @{ name = 'Standard_LRS' }
    properties = @{ creationData = @{ createOption = 'Empty' }; diskSizeGB = 4 }
} | Out-Null
Invoke-FixtureArm -Method PUT -WaitForLinkedAuthorization -Path "$deleteGroupId/providers/Microsoft.Relationships/serviceGroupMember/delete-fixture?api-version=2023-09-01-preview" -Body @{
    properties = @{ targetId = $lifecycleId }
} | Out-Null
$settingsOutputs['DELETEASSETRESOURCEGROUPNAME'] = $DeleteAssetResourceGroupName
. "$PSScriptRoot\..\..\..\eng\scripts\helpers\TestResourcesHelpers.ps1"
if ($DeleteOnly) {
    New-TestSettings -ResourceGroupName $ResourceGroupName -BaseName 'mcp-drills-ga' -DeploymentOutputs $settingsOutputs -OutputPath $PSScriptRoot | Out-Null
    Write-Host 'Isolated delete fixture configured. The recorded test creates and deletes its own drill.'
    return
}

New-FixtureResource -WaitForLinkedAuthorization -Path "$($outputs.usagePlanId.Value)/enrollments/$serviceGroupName`?api-version=$apiVersion" -Body @{
    properties = @{ serviceGroupId = $serviceGroupId }
} | Out-Null

# Only the primary group discovers the VM's RG; deletion owns a separate disk-only RG.
Invoke-FixtureArm -Method PUT -Path "$groupId/providers/Microsoft.Relationships/serviceGroupMember/drills-ga?api-version=2023-09-01-preview" -Body @{
    properties = @{ targetId = $serviceGroupId }
} | Out-Null

$base = "$serviceGroupId/providers/Microsoft.AzureResilienceManagement"
$drillId = "$base/drills/$($outputs.drillName.Value)"
$existingDrill = Get-FixtureResource -Path "$drillId`?api-version=$apiVersion" -AllowNotFound
if ($existingDrill -and $existingDrill.properties.executionState -ne 'NotRunning') {
    throw 'An existing drill is running. Resolve its lifecycle before reconfiguring this fixture.'
}
$goalId = "$base/goalAssignments/ga-fixture"
$planId = "$base/recoveryPlans/$($outputs.recoveryPlanName.Value)"
$plan = New-FixtureResource -Path "$planId`?api-version=$apiVersion" -Body @{
    identity = @{ type = 'SystemAssigned' }
    properties = @{
        planType = 'Zonal'
        planDescription = 'Isolated MCP Drills GA lifecycle fixture'
        recoveryGroupsSetting = @{
            defaultGroup = @{
                properties = @{
                    description = 'Dedicated test VM only'
                    groupUniqueId = [Guid]::NewGuid().ToString()
                    orderId = 0
                    preActions = @()
                    postActions = @()
                }
            }
            additionalGroups = @()
        }
    }
}
Add-FixtureRole -Scope $groupId -PrincipalId $plan.identity.principalId -RoleId 'acdd72a7-3385-48ef-bd42-f606fba81ae7'
Add-FixtureRole -Scope $automationId -PrincipalId $plan.identity.principalId -RoleId '4fe576fe-1146-4730-92eb-48519fa6bf9f'

# These custom recovery actions exercise orchestration, not cross-zone disaster recovery.
# They start the faulted test VM and leave an observable phase marker for assertions.
$runbookTemplate = @'
$ErrorActionPreference = 'Stop'
$vmId = '__VM_ID__'
$phase = '__PHASE__'
if (!$env:IDENTITY_ENDPOINT -or !$env:IDENTITY_HEADER) {
    throw 'The fixture runbook requires the Automation account managed identity.'
}
$tokenUri = "$($env:IDENTITY_ENDPOINT)?resource=https%3A%2F%2Fmanagement.azure.com%2F&api-version=2019-08-01"
$token = Invoke-RestMethod -Uri $tokenUri -Headers @{ 'X-IDENTITY-HEADER' = $env:IDENTITY_HEADER; Metadata = 'True' }
$headers = @{ Authorization = "Bearer $($token.access_token)" }
$base = "https://management.azure.com$vmId"
$vm = Invoke-RestMethod -Uri "$base`?api-version=2024-07-01" -Headers $headers
if ($vm.tags.purpose -ne 'mcp-drills-ga-live-tests') {
    throw 'Refusing to operate on a VM without the dedicated fixture tag.'
}
Invoke-RestMethod -Method POST -Uri "$base/start?api-version=2024-07-01" -Headers $headers | Out-Null
$deadline = [DateTime]::UtcNow.AddMinutes(10)
do {
    $view = Invoke-RestMethod -Uri "$base/instanceView?api-version=2024-07-01" -Headers $headers
    if ($view.statuses.code -contains 'PowerState/running') { break }
    Start-Sleep -Seconds 10
} while ([DateTime]::UtcNow -lt $deadline)
if ($view.statuses.code -notcontains 'PowerState/running') { throw 'The dedicated test VM did not restart.' }
$vm.tags | Add-Member -NotePropertyName 'mcp-drill-phase' -NotePropertyValue $phase -Force
$body = @{ tags = $vm.tags } | ConvertTo-Json -Depth 10
Invoke-RestMethod -Method PATCH -Uri "$base`?api-version=2024-07-01" -Headers $headers -ContentType 'application/json' -Body $body | Out-Null
Write-Output "Dedicated fixture VM is running; phase=$phase."
'@

foreach ($phase in @('Failover', 'Reprotect')) {
    $runbookId = "$automationId/runbooks/Drills-$phase"
    Invoke-FixtureArm -Method PUT -Path "$runbookId`?api-version=2023-11-01" -Body @{
        location = $outputs.location.Value
        properties = @{ runbookType = 'PowerShell'; logProgress = $false; logVerbose = $false; description = 'Isolated Drills lifecycle fixture' }
    } | Out-Null
    $content = $runbookTemplate.Replace('__VM_ID__', $vmId).Replace('__PHASE__', $phase)
    Invoke-FixtureArm -Method PUT -Path "$runbookId/draft/content?api-version=2023-11-01" -ContentType 'text/powershell' -Body $content | Out-Null
    Invoke-FixtureArm -Method POST -Path "$runbookId/publish?api-version=2023-11-01" | Out-Null
}

Invoke-FixtureAction -Path "$planId/checkReadiness?api-version=$apiVersion"
Write-Host 'Waiting for the dedicated VM to appear in recovery-resource discovery.'
$deadline = [DateTime]::UtcNow.AddMinutes(15)
do {
    $resources = Get-FixtureResource -Path "$planId/recoveryResources?api-version=$apiVersion"
    $target = $resources.value | Where-Object { $_.properties.resourceId -eq $vmId }
    if ($target) { break }
    $validation = (Get-FixtureResource -Path "$planId`?api-version=$apiVersion").properties.latestValidationStatus
    if ($validation.errorDetails.code -eq 'RHRORecoveryResourceSyncFailed') {
        throw "Recovery-resource discovery failed: $(ConvertTo-Json $validation -Depth 10 -Compress)"
    }
    Start-Sleep -Seconds 15
} while ([DateTime]::UtcNow -lt $deadline)
if (@($target).Count -ne 1) { throw 'Expected exactly one recovery resource for the dedicated test VM.' }

$updates = @($resources.value | ForEach-Object {
    $properties = @{ recoveryResourceUniqueId = $_.name; inclusionState = 'Excluded' }
    if ($_.id -eq $target.id) {
        $properties.inclusionState = 'Included'
        $properties.recoveryGroupId = $plan.properties.recoveryGroupsSetting.defaultGroup.properties.groupUniqueId
        $properties.selectedProtectionSolutionType = 'CustomRunbook'
        $properties.selectedProtectionSolutionSetting = @{
            protectionSolutionType = 'CustomRunbook'
            failoverAction = @{ resourceId = "$automationId/runbooks/Drills-Failover" }
            reprotectAction = @{ resourceId = "$automationId/runbooks/Drills-Reprotect" }
        }
    }
    @{ id = $_.id; properties = $properties }
})
Invoke-FixtureAction -Path "$planId/updateResources?api-version=$apiVersion" -Body @{
    resourcesToUpdate = $updates
    resourcesToRemove = @()
} | Out-Null
Wait-FixtureResource -Path "$planId`?api-version=$apiVersion" | Out-Null
Invoke-FixtureAction -Path "$planId/finalize?api-version=$apiVersion"
$included = @((Get-FixtureResource -Path "$planId/recoveryResources?api-version=$apiVersion").value |
    Where-Object { $_.properties.inclusionState -eq 'Included' })
if ($included.Count -ne 1 -or $included[0].properties.resourceId -ne $vmId) {
    throw 'Only the dedicated fixture VM may be included in the recovery plan.'
}

$drillProperties = @{
    drillType = 'Zonal'
    rbacSetupMode = 'AutomatedBuiltinRoles'
    drillAssetProperties = @{ subscription = $subscriptionId; region = $outputs.location.Value; resourceGroup = $ResourceGroupName }
    recoveryPlanProperties = @{ recoveryPlanId = $planId; identity = @{ type = 'SystemAssigned' } }
    chaosResourceProperties = @{
        identity = @{ type = 'SystemAssigned' }
        chaosResourceIdentityForFaults = @{ type = 'SystemAssigned' }
    }
    monitoringProperties = @{ identity = @{ type = 'SystemAssigned' } }
}
if (!$ActionsOnly) {
    $healthApi = '2026-05-01-preview'
    Invoke-FixtureArm -Method PUT -Path "$healthModelId/authenticationsettings/default-auth?api-version=$healthApi" -Body @{
        properties = @{
            displayName = 'Fixture managed identity'
            authenticationKind = 'ManagedIdentity'
            managedIdentityName = 'SystemAssigned'
        }
    } | Out-Null
    $healthModelName = $healthModelId.Split('/')[-1]
    Invoke-FixtureArm -Method PUT -Path "$healthModelId/discoveryrules/$healthModelName`?api-version=$healthApi" -Body @{
        properties = @{
            displayName = 'Dedicated fixture service group'
            authenticationSetting = 'default-auth'
            discoverRelationships = 'Enabled'
            addRecommendedSignals = 'Enabled'
            addResourceHealthSignal = 'Enabled'
            specification = @{
                kind = 'ResourceGraphQuery'
                resourceGraphQuery = "relationshipresources | where type =~ 'microsoft.relationships/servicegroupmember' | where tostring(properties.TargetId) =~ '$serviceGroupId' | project id=tostring(properties.SourceId)"
            }
        }
    } | Out-Null
    $drillProperties.goalAssignmentProperties = @{ identity = @{ type = 'SystemAssigned' } }
    $drillProperties.healthModelMonitoringProperties = @{ identity = @{ type = 'SystemAssigned' }; healthModelId = $healthModelId }
}

$drill = New-FixtureResource -Path "$drillId`?api-version=$apiVersion" -Body @{
    identity = @{ type = 'SystemAssigned' }
    properties = $drillProperties
}
if ($drill.properties.executionState -ne 'NotRunning') {
    throw 'An existing drill is running. Resolve its lifecycle before reconfiguring this fixture.'
}
Add-FixtureRole -Scope $serviceGroupId -PrincipalId $drill.identity.principalId -RoleId '517781b0-5ad4-4418-94d5-f2421834b586'
Add-FixtureRole -Scope "/subscriptions/$subscriptionId" -PrincipalId $drill.identity.principalId -RoleId '344b740c-5e7c-4d32-89b2-ee3b51cb89bc'
Start-Sleep -Seconds 60
$response = Invoke-FixtureArm -Method PUT -Path "$drillId`?api-version=$apiVersion" -Body @{
    identity = @{ type = 'SystemAssigned' }
    properties = $drillProperties
}
Wait-FixtureOperation -Response $response -Path $drillId
$drill = Wait-FixtureResource -Path "$drillId`?api-version=$apiVersion"
if ($ActionsOnly) {
    $zones = @($target.properties.resourcePhysicalZones)
    if ($zones.Count -ne 1) { throw 'Expected exactly one physical source zone for the dedicated VM.' }
    $settingsOutputs['DRILLRUNSOURCELOCATION'] = $zones[0]
    $settingsOutputs['RECOVERYRESOURCEID'] = $target.id
    New-TestSettings -ResourceGroupName $ResourceGroupName -BaseName 'mcp-drills-ga' -DeploymentOutputs $settingsOutputs -OutputPath $PSScriptRoot | Out-Null
    Write-Host 'Action fixture configured without GA monitoring configuration. Record resource add/update and resync before starting a run.'
    Write-Host 'Set DRILLRESOURCEID from the dedicated VM drill-resource discovery result before recording resource add/update; VMID is not interchangeable.'
    Write-Host 'No drill has been started and no fault injected.'
    return
}
if ($drill.properties.goalAssignmentProperties.identity.type -ne 'SystemAssigned' -or
    $drill.properties.healthModelMonitoringProperties.healthModelId -ne $healthModelId) {
    throw 'The GA drill response did not preserve the requested goal/Health Model configuration. Resolve the service response-contract mismatch before lifecycle testing.'
}
Invoke-FixtureAction -Path "$drillId/resyncReadinessCheck?api-version=$apiVersion"

$deadline = [DateTime]::UtcNow.AddMinutes(15)
do {
    $drillResources = Get-FixtureResource -Path "$drillId/drillResources?api-version=$apiVersion"
    $drillTarget = $drillResources.value | Where-Object { $_.properties.resourceId -eq $vmId }
    if ($drillTarget) { break }
    Start-Sleep -Seconds 15
} while ([DateTime]::UtcNow -lt $deadline)
if (@($drillTarget).Count -ne 1) { throw 'Expected exactly one drill resource for the dedicated test VM.' }
Invoke-FixtureAction -Path "$drillId/addOrUpdateResources?api-version=$apiVersion" -Body @{
    faultDurationInMin = 1
    forceInclusionAndUpdate = 'Disable'
    resourceLists = @{
        includeResources = @(@{ id = $drillTarget.id })
        excludeResources = @($drillResources.value | Where-Object { $_.properties.resourceId -ne $vmId } | ForEach-Object { $_.id })
        updateResources = @()
    }
}
$drillTarget = Get-FixtureResource -Path "$($drillTarget.id)?api-version=$apiVersion"
if ($drillTarget.properties.inclusionState -ne 'Included') {
    throw "The dedicated VM was not included: $(ConvertTo-Json $drillTarget.properties -Depth 15 -Compress)"
}
$sourceLocation = @($target.properties.resourcePhysicalZones)
if ($sourceLocation.Count -ne 1) { throw 'Expected exactly one physical source zone for the dedicated VM.' }

$settingsOutputs['DRILLRESOURCENAME'] = $drillTarget.name
$settingsOutputs['DRILLRESOURCEID'] = $drillTarget.id
$settingsOutputs['DRILLRUNSOURCELOCATION'] = $sourceLocation[0]
$settingsOutputs['RECOVERYRESOURCEID'] = $target.id
$settingsOutputs['GOALASSIGNMENTNAME'] = 'ga-fixture'

Write-Host 'Primary drill resources configured. No drill has been started and no fault injected.'
Write-Host "Target VM: $vmId"

$creationId = "/providers/Microsoft.Management/serviceGroups/$creationServiceGroupName"
New-FixtureResource -WaitForLinkedAuthorization -Path "$($outputs.usagePlanId.Value)/enrollments/$creationServiceGroupName`?api-version=$apiVersion" -Body @{
    properties = @{ serviceGroupId = $creationId }
} | Out-Null
Invoke-FixtureArm -Method PUT -WaitForLinkedAuthorization -Path "$deleteGroupId/providers/Microsoft.Relationships/serviceGroupMember/create-fixture?api-version=2023-09-01-preview" -Body @{
    properties = @{ targetId = $creationId }
} | Out-Null

$creationRecoveryPlanName = "create-$($outputs.recoveryPlanName.Value)"
$creationRecoveryPlanId = "$creationId/providers/Microsoft.AzureResilienceManagement/recoveryPlans/$creationRecoveryPlanName"
New-FixtureResource -Path "$creationRecoveryPlanId`?api-version=$apiVersion" -Body @{
    identity = @{ type = 'SystemAssigned' }
    properties = @{
        planType = 'Zonal'
        planDescription = 'Isolated Drill creation fixture'
        recoveryGroupsSetting = @{
            defaultGroup = @{
                properties = @{
                    description = 'Creation fixture default group'
                    groupUniqueId = [Guid]::NewGuid().ToString()
                    orderId = 0
                    preActions = @()
                    postActions = @()
                }
            }
            additionalGroups = @()
        }
    }
} | Out-Null

$settingsOutputs['CREATEASSETRESOURCEGROUPNAME'] = $DeleteAssetResourceGroupName
$settingsOutputs['CREATESERVICEGROUPNAME'] = $creationServiceGroupName
$settingsOutputs['CREATERECOVERYPLANNAME'] = $creationRecoveryPlanName
$settingsOutputs['CREATEDRILLNAME'] = "create-$($outputs.drillName.Value)"
$settingsOutputs['CREATELOCATION'] = $outputs.location.Value

New-FixtureResource -Path "$goalId`?api-version=$apiVersion" -Body @{
    properties = @{ requireZonalResiliency = $true }
} | Out-Null
New-TestSettings -ResourceGroupName $ResourceGroupName -BaseName 'mcp-drills-ga' -DeploymentOutputs $settingsOutputs -OutputPath $PSScriptRoot | Out-Null
Write-Host 'GA goal assignment fixture is ready.'
