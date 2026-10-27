# cspell:ignore authenticationsettings discoveryrules relationshipresources resourcecontainers servicegroupmember Fmanagement RHDS RHRO RHUB DRILLSAUTOMATIONACCOUNTID DRILLSDELETEASSETRESOURCEGROUPNAME DRILLSDRILLNAME DRILLSDRILLRESOURCEID DRILLSDRILLRUNNAME DRILLSDRILLRUNRESOURCENAME DRILLSHEALTHMODELID DRILLSLIFECYCLESERVICEGROUPNAME DRILLSRECOVERYPLANNAME DRILLSRECOVERYRESOURCEID DRILLSRESOURCEGROUPNAME DRILLSSERVICEGROUPNAME DRILLSSOURCELOCATION DRILLSUSAGEPLANID DRILLSVMID
param(
    [string] $TenantId,
    [string] $TestApplicationId,
    [string] $TestApplicationOid,
    [string] $ResourceGroupName,
    [string] $BaseName,
    [hashtable] $DeploymentOutputs,
    [hashtable] $AdditionalParameters
)

$ErrorActionPreference = "Stop"

. "$PSScriptRoot/../../../eng/common/scripts/common.ps1"
. "$PSScriptRoot/../../../eng/scripts/helpers/TestResourcesHelpers.ps1"

$testSettings = New-TestSettings @PSBoundParameters -OutputPath $PSScriptRoot

# $testSettings contains:
# - TenantId
# - TenantName
# - SubscriptionId
# - SubscriptionName
# - ResourceGroupName
# - ResourceBaseName

# $DeploymentOutputs keys are all UPPERCASE

# The tenant-scoped service group and the usage plan enrollment are created here via
# direct ARM REST calls (Invoke-AzRestMethod) because:
#  - Microsoft.Management/serviceGroups is a tenant-scoped resource that cannot be created
#    in the resource-group-scoped test-resources.bicep deployment, and a direct PUT only
#    requires serviceGroups write (not tenant-level deployment write).
#  - The enrollment requires the service group to already exist; the usage plan it enrolls
#    into is created by test-resources.bicep.

$tenantId = $testSettings.TenantId
$subscriptionId = $testSettings.SubscriptionId
$serviceGroupName = $DeploymentOutputs['SERVICEGROUPNAME']
$usagePlanName = $DeploymentOutputs['USAGEPLANNAME']
$enrollmentName = $DeploymentOutputs['ENROLLMENTNAME']
$lifecycleEnrollmentName = $DeploymentOutputs['LIFECYCLEENROLLMENTNAME']
$lifecycleServiceGroupName = $DeploymentOutputs['LIFECYCLESERVICEGROUPNAME']
$planLifecycleEnrollmentName = $DeploymentOutputs['PLANLIFECYCLEENROLLMENTNAME']
$planLifecycleServiceGroupName = $DeploymentOutputs['PLANLIFECYCLESERVICEGROUPNAME']
$workflowEnrollmentName = $DeploymentOutputs['WORKFLOWENROLLMENTNAME']
$workflowServiceGroupName = $DeploymentOutputs['WORKFLOWSERVICEGROUPNAME']
$goalsServiceGroupName = $DeploymentOutputs['GOALSSERVICEGROUPNAME']
$storageAccountId = $DeploymentOutputs['STORAGEACCOUNTID']
$goalAssignmentName = $DeploymentOutputs['GOALASSIGNMENTNAME']
$recoveryPlanName = $DeploymentOutputs['RECOVERYPLANNAME']
$workflowRecoveryPlanName = $DeploymentOutputs['WORKFLOWRECOVERYPLANNAME']
$automationAccountName = $DeploymentOutputs['AUTOMATIONACCOUNTNAME']
$automationAccountId = $DeploymentOutputs['AUTOMATIONACCOUNTID']
$failoverRunbookName = $DeploymentOutputs['FAILOVERRUNBOOKNAME']
$reprotectRunbookName = $DeploymentOutputs['REPROTECTRUNBOOKNAME']

$serviceGroupApiVersion = '2024-02-01-preview'
$membershipApiVersion = '2023-09-01-preview'
$resilienceApiVersion = '2026-10-01'

$serviceGroupId = "/providers/Microsoft.Management/serviceGroups/$serviceGroupName"
$serviceGroupResilienceBase = "$serviceGroupId/providers/Microsoft.AzureResilienceManagement"
$lifecycleServiceGroupId = "/providers/Microsoft.Management/serviceGroups/$lifecycleServiceGroupName"
$planLifecycleServiceGroupId = "/providers/Microsoft.Management/serviceGroups/$planLifecycleServiceGroupName"
$workflowServiceGroupId = "/providers/Microsoft.Management/serviceGroups/$workflowServiceGroupName"
$workflowServiceGroupResilienceBase = "$workflowServiceGroupId/providers/Microsoft.AzureResilienceManagement"
$goalsServiceGroupId = "/providers/Microsoft.Management/serviceGroups/$goalsServiceGroupName"
$failoverRunbookId = "$automationAccountId/runbooks/$failoverRunbookName"
$reprotectRunbookId = "$automationAccountId/runbooks/$reprotectRunbookName"

function Invoke-ResilienceRestPut {
    param(
        [string] $Path,
        [hashtable] $Body
    )

    $payload = $Body | ConvertTo-Json -Depth 20 -Compress
    Write-Host "PUT $Path"
    $response = Invoke-AzRestMethod -Method PUT -Path $Path -Payload $payload
    if ($response.StatusCode -ge 400) {
        throw "PUT $Path failed with status $($response.StatusCode): $($response.Content)"
    }
    return $response
}

function Invoke-ResilienceRestPost {
    param(
        [string] $Path,
        [hashtable] $Body,
        [string] $OperationId
    )

    Write-Host "POST $Path"
    if ($Body) {
        $payload = $Body | ConvertTo-Json -Depth 20 -Compress
        if ($OperationId) {
            $payloadPath = [System.IO.Path]::GetTempFileName()
            try {
                [System.IO.File]::WriteAllText($payloadPath, $payload)
                $responseContent = az rest --method POST --url "https://management.azure.com$Path" --headers "operation-id=$OperationId" "Content-Type=application/json" --body "@$payloadPath" --output json 2>&1
                if ($LASTEXITCODE -ne 0) {
                    throw "POST $Path failed: $responseContent"
                }
                return $responseContent
            }
            finally {
                Remove-Item $payloadPath -Force
            }
        }

        $response = Invoke-AzRestMethod -Method POST -Path $Path -Payload $payload
    }
    else {
        $response = Invoke-AzRestMethod -Method POST -Path $Path
    }

    if ($response.StatusCode -ge 400) {
        throw "POST $Path failed with status $($response.StatusCode): $($response.Content)"
    }
    return $response
}

function Wait-ResilienceProvisioning {
    param(
        [string] $Path,
        [int] $TimeoutSeconds = 900,
        [switch] $WaitForAuthorization
    )

    $deadline = (Get-Date).AddSeconds($TimeoutSeconds)
    while ((Get-Date) -lt $deadline) {
        $response = Invoke-AzRestMethod -Method GET -Path $Path

        # Resource creation is eventually consistent. Service groups also create an
        # automatic administrator assignment that can take time to become effective.
        if ($response.StatusCode -eq 404 -or ($WaitForAuthorization -and $response.StatusCode -eq 403)) {
            Write-Host "  not accessible yet (still provisioning)"
            Start-Sleep -Seconds 15
            continue
        }

        if ($response.StatusCode -ge 400) {
            throw "GET $Path failed with status $($response.StatusCode): $($response.Content)"
        }

        $state = ($response.Content | ConvertFrom-Json).properties.provisioningState
        Write-Host "  provisioningState = $state"
        if ($state -eq 'Succeeded') {
            return
        }
        if ($state -in @('Failed', 'Canceled')) {
            $errorDetails = ($response.Content | ConvertFrom-Json).properties.errorDetails
            $errorMessage = if ($errorDetails) {
                " ErrorCode: $($errorDetails.code). Message: $($errorDetails.message)"
            }
            else {
                ''
            }
            throw "Provisioning of $Path ended in state '$state'.$errorMessage"
        }

        Start-Sleep -Seconds 15
    }

    throw "Timed out waiting for $Path to finish provisioning."
}

function Add-RecoveryContributorRole {
    param(
        [string] $Scope,
        [int] $TimeoutSeconds = 900
    )

    $roleName = 'Azure Resilience Management Recovery Contributor'
    $deadline = (Get-Date).AddSeconds($TimeoutSeconds)
    while ((Get-Date) -lt $deadline) {
        try {
            $assignment = Get-AzRoleAssignment -ObjectId $TestApplicationOid -Scope $Scope -RoleDefinitionName $roleName -ErrorAction Stop
            if ($assignment) {
                return
            }

            Write-Host "Assigning $roleName to test identity at $Scope"
            New-AzRoleAssignment -ObjectId $TestApplicationOid -Scope $Scope -RoleDefinitionName $roleName -ErrorAction Stop | Out-Null
            return
        }
        catch {
            if ($_.Exception.Response.StatusCode -notin @(403, 404)) {
                throw
            }

            Write-Host "  role assignment authorization is still propagating"
            Start-Sleep -Seconds 15
        }
    }

    throw "Timed out assigning $roleName to test identity at $Scope."
}

function Add-DrillsAdministratorRole {
    param(
        [string] $Scope,
        [int] $TimeoutSeconds = 900
    )

    $roleDefinitionId = 'c914561b-1575-4601-af9c-a1356bf59818'
    $deadline = (Get-Date).AddSeconds($TimeoutSeconds)
    while ((Get-Date) -lt $deadline) {
        try {
            $assignment = Get-AzRoleAssignment -ObjectId $TestApplicationOid -Scope $Scope -RoleDefinitionId $roleDefinitionId -ErrorAction Stop
            if ($assignment) {
                return
            }

            Write-Host "Assigning Azure Resilience Management Drills Administrator to test identity at $Scope"
            New-AzRoleAssignment -ObjectId $TestApplicationOid -Scope $Scope -RoleDefinitionId $roleDefinitionId -ErrorAction Stop | Out-Null
            return
        }
        catch {
            if ($_.Exception.Response.StatusCode -notin @(403, 404)) {
                throw
            }

            Write-Host "  role assignment authorization is still propagating"
            Start-Sleep -Seconds 15
        }
    }

    throw "Timed out assigning the Drills Administrator role to test identity at $Scope."
}

function Publish-TestRunbook {
    param(
        [string] $Name
    )

    $runbookPath = Join-Path ([System.IO.Path]::GetTempPath()) "$Name.ps1"
    try {
        [System.IO.File]::WriteAllText($runbookPath, "Write-Output 'Completed'`r`n")
        Import-AzAutomationRunbook `
            -ResourceGroupName $ResourceGroupName `
            -AutomationAccountName $automationAccountName `
            -Name $Name `
            -Type PowerShell `
            -Path $runbookPath `
            -Published `
            -Force | Out-Null
    }
    finally {
        Remove-Item $runbookPath -Force -ErrorAction SilentlyContinue
    }
}

Publish-TestRunbook -Name $failoverRunbookName
Publish-TestRunbook -Name $reprotectRunbookName

# 1) Create the tenant-scoped service group.
$serviceGroupPath = "$serviceGroupId`?api-version=$serviceGroupApiVersion"
Invoke-ResilienceRestPut -Path $serviceGroupPath -Body @{
    properties = @{
        displayName = $serviceGroupName
        parent      = @{
            resourceId = "/providers/Microsoft.Management/serviceGroups/$tenantId"
        }
    }
} | Out-Null
Wait-ResilienceProvisioning -Path $serviceGroupPath -WaitForAuthorization

# Create a second enrolled service group without a recoveryplan. Lifecycle tests use it
# to exercise create and delete without disturbing the shared plan used by other tests.
$lifecycleServiceGroupPath = "$lifecycleServiceGroupId`?api-version=$serviceGroupApiVersion"
Invoke-ResilienceRestPut -Path $lifecycleServiceGroupPath -Body @{
    properties = @{
        displayName = $lifecycleServiceGroupName
        parent      = @{
            resourceId = "/providers/Microsoft.Management/serviceGroups/$tenantId"
        }
    }
} | Out-Null
Wait-ResilienceProvisioning -Path $lifecycleServiceGroupPath -WaitForAuthorization

# Recoveryplan lifecycle tests use a separate service group because only one plan of each
# type can exist in a service group and the drill delete fixture reserves the lifecycle group.
$planLifecycleServiceGroupPath = "$planLifecycleServiceGroupId`?api-version=$serviceGroupApiVersion"
Invoke-ResilienceRestPut -Path $planLifecycleServiceGroupPath -Body @{
    properties = @{
        displayName = $planLifecycleServiceGroupName
        parent      = @{
            resourceId = "/providers/Microsoft.Management/serviceGroups/$tenantId"
        }
    }
} | Out-Null
Wait-ResilienceProvisioning -Path $planLifecycleServiceGroupPath -WaitForAuthorization

$workflowServiceGroupPath = "$workflowServiceGroupId`?api-version=$serviceGroupApiVersion"
Invoke-ResilienceRestPut -Path $workflowServiceGroupPath -Body @{
    properties = @{
        displayName = $workflowServiceGroupName
        parent      = @{
            resourceId = "/providers/Microsoft.Management/serviceGroups/$tenantId"
        }
    }
} | Out-Null
Wait-ResilienceProvisioning -Path $workflowServiceGroupPath -WaitForAuthorization

# Goals allow only one assignment per service group, and the shared group already holds the fixture
# assignment. Goal lifecycle tests create and delete their own assignment in this group instead.
$goalsServiceGroupPath = "$goalsServiceGroupId`?api-version=$serviceGroupApiVersion"
Invoke-ResilienceRestPut -Path $goalsServiceGroupPath -Body @{
    properties = @{
        displayName = $goalsServiceGroupName
        parent      = @{
            resourceId = "/providers/Microsoft.Management/serviceGroups/$tenantId"
        }
    }
} | Out-Null
Wait-ResilienceProvisioning -Path $goalsServiceGroupPath -WaitForAuthorization

Add-RecoveryContributorRole -Scope $serviceGroupId
Add-RecoveryContributorRole -Scope $lifecycleServiceGroupId
Add-RecoveryContributorRole -Scope $planLifecycleServiceGroupId
Add-RecoveryContributorRole -Scope $workflowServiceGroupId
Add-DrillsAdministratorRole -Scope "/subscriptions/$subscriptionId"

# 2) Add the resource group as a member of the service group so its resources
#    (e.g. the storage account) surface as goal/recovery resource targets.
$membershipPath = "/subscriptions/$subscriptionId/resourceGroups/$ResourceGroupName/providers/Microsoft.Relationships/serviceGroupMember/rhub-rg-member`?api-version=$membershipApiVersion"
Invoke-ResilienceRestPut -Path $membershipPath -Body @{
    properties = @{
        targetId = $serviceGroupId
    }
} | Out-Null

$lifecycleMembershipPath = "/subscriptions/$subscriptionId/resourceGroups/$ResourceGroupName/providers/Microsoft.Relationships/serviceGroupMember/rhub-lifecycle-rg-member`?api-version=$membershipApiVersion"
Invoke-ResilienceRestPut -Path $lifecycleMembershipPath -Body @{
    properties = @{
        targetId = $lifecycleServiceGroupId
    }
} | Out-Null

$planLifecycleMembershipPath = "/subscriptions/$subscriptionId/resourceGroups/$ResourceGroupName/providers/Microsoft.Relationships/serviceGroupMember/rhub-plan-lifecycle-rg-member`?api-version=$membershipApiVersion"
Invoke-ResilienceRestPut -Path $planLifecycleMembershipPath -Body @{
    properties = @{
        targetId = $planLifecycleServiceGroupId
    }
} | Out-Null

# Only the storage account joins the goals group, so goal refresh discovers a single member.
$goalsMembershipPath = "$storageAccountId/providers/Microsoft.Relationships/serviceGroupMember/rhub-goals-member`?api-version=$membershipApiVersion"
Invoke-ResilienceRestPut -Path $goalsMembershipPath -Body @{
    properties = @{
        targetId = $goalsServiceGroupId
    }
} | Out-Null

$workflowMembershipPath = "/subscriptions/$subscriptionId/resourceGroups/$ResourceGroupName/providers/Microsoft.Relationships/serviceGroupMember/rhub-workflow-rg-member`?api-version=$membershipApiVersion"
Invoke-ResilienceRestPut -Path $workflowMembershipPath -Body @{
    properties = @{
        targetId = $workflowServiceGroupId
    }
} | Out-Null

# 3) Enroll the service group into the usage plan (the usage plan is created by the bicep template).
$enrollmentPath = "/subscriptions/$subscriptionId/resourceGroups/$ResourceGroupName/providers/Microsoft.AzureResilienceManagement/usagePlans/$usagePlanName/enrollments/$enrollmentName`?api-version=$resilienceApiVersion"
Invoke-ResilienceRestPut -Path $enrollmentPath -Body @{
    properties = @{
        serviceGroupId = $serviceGroupId
    }
} | Out-Null
Wait-ResilienceProvisioning -Path $enrollmentPath

$lifecycleEnrollmentPath = "/subscriptions/$subscriptionId/resourceGroups/$ResourceGroupName/providers/Microsoft.AzureResilienceManagement/usagePlans/$usagePlanName/enrollments/$lifecycleEnrollmentName`?api-version=$resilienceApiVersion"
Invoke-ResilienceRestPut -Path $lifecycleEnrollmentPath -Body @{
    properties = @{
        serviceGroupId = $lifecycleServiceGroupId
    }
} | Out-Null
Wait-ResilienceProvisioning -Path $lifecycleEnrollmentPath

$planLifecycleEnrollmentPath = "/subscriptions/$subscriptionId/resourceGroups/$ResourceGroupName/providers/Microsoft.AzureResilienceManagement/usagePlans/$usagePlanName/enrollments/$planLifecycleEnrollmentName`?api-version=$resilienceApiVersion"
Invoke-ResilienceRestPut -Path $planLifecycleEnrollmentPath -Body @{
    properties = @{
        serviceGroupId = $planLifecycleServiceGroupId
    }
} | Out-Null
Wait-ResilienceProvisioning -Path $planLifecycleEnrollmentPath

$workflowEnrollmentPath = "/subscriptions/$subscriptionId/resourceGroups/$ResourceGroupName/providers/Microsoft.AzureResilienceManagement/usagePlans/$usagePlanName/enrollments/$workflowEnrollmentName`?api-version=$resilienceApiVersion"
Invoke-ResilienceRestPut -Path $workflowEnrollmentPath -Body @{
    properties = @{
        serviceGroupId = $workflowServiceGroupId
    }
} | Out-Null
Wait-ResilienceProvisioning -Path $workflowEnrollmentPath

# 4) Create a GA goal assignment directly on the service group.
$goalAssignmentPath = "$serviceGroupResilienceBase/goalAssignments/$goalAssignmentName`?api-version=$resilienceApiVersion"
$existingGoalAssignment = Invoke-AzRestMethod -Method GET -Path $goalAssignmentPath
if ($existingGoalAssignment.StatusCode -eq 404) {
    Invoke-ResilienceRestPut -Path $goalAssignmentPath -Body @{
        properties = @{
            requireZonalResiliency = $true
            serviceLevelResources  = @()
        }
    } | Out-Null
    Wait-ResilienceProvisioning -Path $goalAssignmentPath -WaitForAuthorization
}
elseif ($existingGoalAssignment.StatusCode -eq 200) {
    $goalAssignment = $existingGoalAssignment.Content | ConvertFrom-Json
    if ($goalAssignment.properties.requireZonalResiliency -ne $true -or
        $goalAssignment.properties.provisioningState -ne 'Succeeded') {
        throw "Existing goal assignment '$goalAssignmentName' does not match the requested test configuration."
    }
    Write-Host "Goal assignment '$goalAssignmentName' already exists with the requested configuration."
}
else {
    throw "GET $goalAssignmentPath failed with status $($existingGoalAssignment.StatusCode): $($existingGoalAssignment.Content)"
}

# 5) Create or validate the recoveryplan on the service group. Do not PUT an
# existing plan because recovery group IDs are referenced by its recovery resources.
$recoveryPlanPath = "$serviceGroupResilienceBase/recoveryPlans/$recoveryPlanName`?api-version=$resilienceApiVersion"
$existingRecoveryPlan = Invoke-AzRestMethod -Method GET -Path $recoveryPlanPath
if ($existingRecoveryPlan.StatusCode -eq 404) {
    Invoke-ResilienceRestPut -Path $recoveryPlanPath -Body @{
        identity   = @{
            type = 'SystemAssigned'
        }
        properties = @{
            planDescription       = 'Recoveryplan for live testing.'
            planType              = 'Zonal'
            recoveryGroupsSetting = @{
                defaultGroup     = @{
                    properties = @{
                        description   = 'Default recovery group'
                        groupUniqueId = (New-Guid).Guid
                        orderId       = 0
                        preActions    = @()
                        postActions   = @()
                    }
                }
                additionalGroups = @()
            }
        }
    } | Out-Null
    Wait-ResilienceProvisioning -Path $recoveryPlanPath -WaitForAuthorization
}
elseif ($existingRecoveryPlan.StatusCode -eq 200) {
    $recoveryPlan = $existingRecoveryPlan.Content | ConvertFrom-Json
    if ($recoveryPlan.properties.provisioningState -ne 'Succeeded') {
        $errorDetails = $recoveryPlan.properties.errorDetails
        throw "Existing recoveryplan '$recoveryPlanName' is in provisioning state '$($recoveryPlan.properties.provisioningState)'. ErrorCode: $($errorDetails.code). Message: $($errorDetails.message)"
    }
    if ($recoveryPlan.properties.planType -ne 'Zonal') {
        throw "Existing recoveryplan '$recoveryPlanName' does not match the requested Zonal test configuration."
    }
    if ([string]::IsNullOrWhiteSpace($recoveryPlan.properties.recoveryGroupsSetting.defaultGroup.properties.groupUniqueId)) {
        throw "Existing recoveryplan '$recoveryPlanName' does not have a valid default recovery group."
    }
    Write-Host "Recoveryplan '$recoveryPlanName' already exists with the requested configuration."
}
else {
    throw "GET $recoveryPlanPath failed with status $($existingRecoveryPlan.StatusCode): $($existingRecoveryPlan.Content)"
}

# 7) Run a readiness check on the recoveryplan so it has a recorded validation status.
$checkReadinessPath = "$serviceGroupResilienceBase/recoveryPlans/$recoveryPlanName/checkReadiness`?api-version=$resilienceApiVersion"
Invoke-ResilienceRestPost -Path $checkReadinessPath -OperationId (New-Guid).Guid | Out-Null
Wait-ResilienceProvisioning -Path $recoveryPlanPath -WaitForAuthorization

# Capture the recovery job created by the readiness check (and its first resource, if any) so the
# recovery job/resource live tests can read them from deployment outputs. The job appears
# asynchronously, so poll the list until one shows up.
$recoveryJobsPath = "$serviceGroupResilienceBase/recoveryPlans/$recoveryPlanName/recoveryJobs`?api-version=$resilienceApiVersion"
$recoveryJobName = $null
$deadline = (Get-Date).AddSeconds(300)
while (-not $recoveryJobName -and (Get-Date) -lt $deadline) {
    $recoveryJobs = (Invoke-AzRestMethod -Method GET -Path $recoveryJobsPath).Content | ConvertFrom-Json
    $recoveryJobName = $recoveryJobs.value | Select-Object -First 1 -ExpandProperty name
    if (-not $recoveryJobName) {
        Write-Host "  waiting for recovery job to appear..."
        Start-Sleep -Seconds 15
    }
}

if ($recoveryJobName) {
    $DeploymentOutputs['RECOVERYJOBNAME'] = $recoveryJobName

    $recoveryJobResourcesPath = "$serviceGroupResilienceBase/recoveryPlans/$recoveryPlanName/recoveryJobs/$recoveryJobName/recoveryJobResources`?api-version=$resilienceApiVersion"
    $recoveryJobResources = (Invoke-AzRestMethod -Method GET -Path $recoveryJobResourcesPath).Content | ConvertFrom-Json
    $recoveryJobResourceName = $recoveryJobResources.value | Select-Object -First 1 -ExpandProperty name
    if ($recoveryJobResourceName) {
        $DeploymentOutputs['RECOVERYJOBRESOURCENAME'] = $recoveryJobResourceName
    }

    # Re-write the test settings so the newly created recovery job names are available to tests.
    New-TestSettings @PSBoundParameters -OutputPath $PSScriptRoot | Out-Null
}
else {
    Write-Warning "No recovery job appeared after the readiness check; RECOVERYJOBNAME was not set."
}

# Prepare an isolated CustomRunbook plan for successful failover, resume, and reprotect recordings.
$workflowRecoveryPlanPath = "$workflowServiceGroupResilienceBase/recoveryPlans/$workflowRecoveryPlanName`?api-version=$resilienceApiVersion"
if ((Invoke-AzRestMethod -Method GET -Path $workflowRecoveryPlanPath).StatusCode -eq 404) {
    Invoke-ResilienceRestPut -Path $workflowRecoveryPlanPath -Body @{
        identity   = @{
            type = 'SystemAssigned'
        }
        properties = @{
            planDescription       = 'Recovery action workflow live test.'
            planType              = 'Zonal'
            recoveryGroupsSetting = @{
                defaultGroup     = @{
                    properties = @{
                        description   = 'Workflow recovery group'
                        groupUniqueId = (New-Guid).Guid
                        orderId       = 0
                        preActions    = @()
                        postActions   = @(
                            @{
                                type             = 'ManualAction'
                                name             = 'Confirmfailover'
                                description      = 'Approve the recorded failover workflow.'
                                timeoutInMinutes = 30
                            }
                        )
                    }
                }
                additionalGroups = @()
            }
        }
    } | Out-Null
    Wait-ResilienceProvisioning -Path $workflowRecoveryPlanPath -WaitForAuthorization
}

$workflowRecoveryPlan = (Invoke-AzRestMethod -Method GET -Path $workflowRecoveryPlanPath).Content | ConvertFrom-Json
$workflowPlanPrincipalId = $workflowRecoveryPlan.identity.principalId
$automationJobOperatorRoleId = '4fe576fe-1146-4730-92eb-48519fa6bf9f'
$automationRole = Get-AzRoleAssignment -ObjectId $workflowPlanPrincipalId -Scope $automationAccountId -RoleDefinitionId $automationJobOperatorRoleId -ErrorAction SilentlyContinue
if (!$automationRole) {
    New-AzRoleAssignment -ObjectId $workflowPlanPrincipalId -Scope $automationAccountId -RoleDefinitionId $automationJobOperatorRoleId | Out-Null
}

$workflowResourcesPath = "$workflowServiceGroupResilienceBase/recoveryPlans/$workflowRecoveryPlanName/recoveryResources`?api-version=$resilienceApiVersion"
$workflowResource = $null
$deadline = (Get-Date).AddSeconds(600)
while (-not $workflowResource -and (Get-Date) -lt $deadline) {
    $workflowResources = ((Invoke-AzRestMethod -Method GET -Path $workflowResourcesPath).Content | ConvertFrom-Json).value
    $workflowResource = $workflowResources | Where-Object { $_.properties.resourceId -eq $automationAccountId } | Select-Object -First 1
    if (-not $workflowResource) {
        Write-Host "  waiting for the Automation account recovery resource to appear..."
        Start-Sleep -Seconds 15
    }
}
if (-not $workflowResource) {
    throw "No recovery resource was created for Automation account $automationAccountId."
}

$workflowGroupId = $workflowRecoveryPlan.properties.recoveryGroupsSetting.defaultGroup.properties.groupUniqueId
$workflowResourceUpdates = @(
    @{
        id         = $workflowResource.id
        properties = @{
            recoveryResourceUniqueId          = $workflowResource.name
            inclusionState                    = 'Included'
            recoveryGroupId                   = $workflowGroupId
            selectedProtectionSolutionType    = 'CustomRunbook'
            selectedProtectionSolutionSetting = @{
                protectionSolutionType = 'CustomRunbook'
                failoverAction         = @{ resourceId = $failoverRunbookId }
                reprotectAction        = @{ resourceId = $reprotectRunbookId }
            }
        }
    }
)
foreach ($resource in $workflowResources | Where-Object { $_.id -ne $workflowResource.id }) {
    $workflowResourceUpdates += @{
        id         = $resource.id
        properties = @{
            recoveryResourceUniqueId = $resource.name
            inclusionState           = 'Excluded'
        }
    }
}

$workflowUpdatePath = "$workflowServiceGroupResilienceBase/recoveryPlans/$workflowRecoveryPlanName/updateResources`?api-version=$resilienceApiVersion"
Invoke-ResilienceRestPost -Path $workflowUpdatePath -OperationId (New-Guid).Guid -Body @{
    resourcesToUpdate = $workflowResourceUpdates
    resourcesToRemove = @()
} | Out-Null
Wait-ResilienceProvisioning -Path $workflowRecoveryPlanPath -WaitForAuthorization

$workflowFinalizePath = "$workflowServiceGroupResilienceBase/recoveryPlans/$workflowRecoveryPlanName/finalize`?api-version=$resilienceApiVersion"
$workflowFinalizeResponse = $null
$workflowFinalizeOperationId = (New-Guid).Guid
for ($attempt = 0; $attempt -lt 40; $attempt++) {
    $workflowFinalizeResponse = az rest --method POST --url "https://management.azure.com$workflowFinalizePath" --headers "operation-id=$workflowFinalizeOperationId" --output json 2>&1
    if ($LASTEXITCODE -eq 0) {
        break
    }
    if ($workflowFinalizeResponse -notmatch '\b409\b|Conflict') {
        throw "POST $workflowFinalizePath failed: $workflowFinalizeResponse"
    }

    Write-Host "  recoveryplan update is still active; waiting to finalize..."
    Start-Sleep -Seconds 15
}
if ($LASTEXITCODE -ne 0) {
    throw "Recoveryplan $workflowRecoveryPlanName could not be finalized after waiting for the resource update."
}
Wait-ResilienceProvisioning -Path $workflowRecoveryPlanPath -WaitForAuthorization

$DeploymentOutputs['WORKFLOWRECOVERYRESOURCEID'] = $workflowResource.id
New-TestSettings @PSBoundParameters -OutputPath $PSScriptRoot | Out-Null

# Exclude any other newly discovered member resources before readiness-dependent workflow tests run.
$workflowResources = ((Invoke-AzRestMethod -Method GET -Path $workflowResourcesPath).Content | ConvertFrom-Json).value
$unresolvedWorkflowResources = @(
    $workflowResources | Where-Object {
        $_.id -ne $workflowResource.id -and $_.properties.inclusionState -ne 'Excluded'
    }
)
if ($unresolvedWorkflowResources.Count -gt 0) {
    $workflowResourceUpdates = @(
        $unresolvedWorkflowResources | ForEach-Object {
            @{
                id         = $_.id
                properties = @{
                    recoveryResourceUniqueId = $_.name
                    inclusionState           = 'Excluded'
                }
            }
        }
    )

    Invoke-ResilienceRestPost -Path $workflowUpdatePath -OperationId (New-Guid).Guid -Body @{
        resourcesToUpdate = $workflowResourceUpdates
        resourcesToRemove = @()
    } | Out-Null
    Wait-ResilienceProvisioning -Path $workflowRecoveryPlanPath -WaitForAuthorization

    $workflowFinalizeOperationId = (New-Guid).Guid
    for ($attempt = 0; $attempt -lt 40; $attempt++) {
        $workflowFinalizeResponse = az rest --method POST --url "https://management.azure.com$workflowFinalizePath" --headers "operation-id=$workflowFinalizeOperationId" --output json 2>&1
        if ($LASTEXITCODE -eq 0) {
            break
        }
        if ($workflowFinalizeResponse -notmatch 'InvalidResourceOperation') {
            throw "POST $workflowFinalizePath failed: $workflowFinalizeResponse"
        }
        Start-Sleep -Seconds 15
    }
    if ($LASTEXITCODE -ne 0) {
        throw "POST $workflowFinalizePath did not acquire the recoveryplan operation lock: $workflowFinalizeResponse"
    }
}

$workflowJobsPath = "$workflowServiceGroupResilienceBase/recoveryPlans/$workflowRecoveryPlanName/recoveryJobs`?api-version=$resilienceApiVersion"
$existingWorkflowJobNames = @(
    ((Invoke-AzRestMethod -Method GET -Path $workflowJobsPath).Content | ConvertFrom-Json).value.name
)
$workflowReadinessPath = "$workflowServiceGroupResilienceBase/recoveryPlans/$workflowRecoveryPlanName/checkReadiness`?api-version=$resilienceApiVersion"
$workflowReadinessOperationId = (New-Guid).Guid
for ($attempt = 0; $attempt -lt 40; $attempt++) {
    $workflowReadinessResponse = az rest --method POST --url "https://management.azure.com$workflowReadinessPath" --headers "operation-id=$workflowReadinessOperationId" --output json 2>&1
    if ($LASTEXITCODE -eq 0) {
        break
    }
    if ($workflowReadinessResponse -notmatch 'InvalidResourceOperation') {
        throw "POST $workflowReadinessPath failed: $workflowReadinessResponse"
    }
    Start-Sleep -Seconds 15
}
if ($LASTEXITCODE -ne 0) {
    throw "POST $workflowReadinessPath did not acquire the recoveryplan operation lock: $workflowReadinessResponse"
}

$deadline = (Get-Date).AddSeconds(600)
while ((Get-Date) -lt $deadline) {
    $workflowJobs = ((Invoke-AzRestMethod -Method GET -Path $workflowJobsPath).Content | ConvertFrom-Json).value
    $workflowReadinessJob = $workflowJobs |
        Where-Object { $_.name -notin $existingWorkflowJobNames } |
        Sort-Object { [DateTime]$_.properties.startTime } -Descending |
        Select-Object -First 1
    if (-not $workflowReadinessJob -or $workflowReadinessJob.properties.status -notin @('Completed', 'Succeeded', 'Failed', 'Canceled')) {
        Start-Sleep -Seconds 15
        continue
    }
    if ($workflowReadinessJob.properties.status -notin @('Completed', 'Succeeded')) {
        $errorDetails = $workflowReadinessJob.properties.errorDetails
        throw "Workflow readiness job '$($workflowReadinessJob.name)' ended in state '$($workflowReadinessJob.properties.status)'. ErrorCode: $($errorDetails.errorCode). Message: $($errorDetails.errorMessage)"
    }
    break
}
if (-not $workflowReadinessJob -or $workflowReadinessJob.properties.status -notin @('Completed', 'Succeeded')) {
    throw "Workflow readiness did not complete before the timeout."
}

# Configures the VM-backed drills fixture that test-resources.bicep deploys into its dedicated
# resource group, then adds DRILLS* entries to $DeploymentOutputs so the final New-TestSettings
# call writes them with the general outputs. Used by the destructive drill lifecycle tests.
function Initialize-DrillsFixture {
    param(
        [Parameter(Mandatory)]
        [hashtable] $DeploymentOutputs,
        [Parameter(Mandatory)]
        [string] $SubscriptionId,
        [Parameter(Mandatory)]
        [string] $TenantId
    )

    $apiVersion = '2026-10-01'
    $fixturePurpose = 'mcp-drills-ga-live-tests'

    $resourceGroupName = $DeploymentOutputs['DRILLSRESOURCEGROUPNAME']
    $deleteAssetResourceGroupName = $DeploymentOutputs['DRILLSDELETEASSETRESOURCEGROUPNAME']
    $usagePlanId = $DeploymentOutputs['DRILLSUSAGEPLANID']
    $serviceGroupName = $DeploymentOutputs['DRILLSSERVICEGROUPNAME']
    $lifecycleServiceGroupName = $DeploymentOutputs['DRILLSLIFECYCLESERVICEGROUPNAME']
    $recoveryPlanName = $DeploymentOutputs['DRILLSRECOVERYPLANNAME']
    $drillName = $DeploymentOutputs['DRILLSDRILLNAME']
    $vmId = $DeploymentOutputs['DRILLSVMID']
    $automationId = $DeploymentOutputs['DRILLSAUTOMATIONACCOUNTID']
    $healthModelId = $DeploymentOutputs['DRILLSHEALTHMODELID']
    $location = $DeploymentOutputs['LOCATION']

    foreach ($name in @($resourceGroupName, $deleteAssetResourceGroupName)) {
        $group = Get-AzResourceGroup -Name $name
        if ($group.Tags['Purpose'] -ne $fixturePurpose) {
            throw "Refusing to configure '$name'; it is not a dedicated drills fixture resource group."
        }
    }
    $groupId = "/subscriptions/$SubscriptionId/resourceGroups/$resourceGroupName"
    $deleteGroupId = "/subscriptions/$SubscriptionId/resourceGroups/$deleteAssetResourceGroupName"
    if (!$vmId.StartsWith("$groupId/providers/Microsoft.Compute/virtualMachines/", [StringComparison]::OrdinalIgnoreCase)) {
        throw 'The target VM must belong to the dedicated drills resource group.'
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
        $deadline = [DateTime]::UtcNow.AddMinutes(30)
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
            if ($response.StatusCode -ne 403 -or [DateTime]::UtcNow -ge $deadline) { break }
            $errorCode = try { ($response.Content | ConvertFrom-Json).error.code } catch { $null }
            # A new service group grants its creator access asynchronously, and enrollments need the
            # service-group link to propagate. A rejected request was not executed, so it is safe to retry.
            $isNewServiceGroupAccess = $errorCode -eq 'AuthorizationFailed' -and $safePath.StartsWith('/providers/Microsoft.Management/serviceGroups/', [StringComparison]::OrdinalIgnoreCase)
            $isLinkedAuthorization = $WaitForLinkedAuthorization -and $errorCode -eq 'LinkedAuthorizationFailed'
            if (!$isNewServiceGroupAccess -and !$isLinkedAuthorization) { break }
            Write-Host "Waiting for service-group authorization to propagate for $safePath."
            Start-Sleep -Seconds 20
            $parameters.Token = (Get-AzAccessToken -ResourceUrl 'https://management.azure.com/').Token
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
        [CmdletBinding(SupportsShouldProcess)]
        param([string] $Path, [hashtable] $Body, [switch] $WaitForLinkedAuthorization)
        $existing = Get-FixtureResource -Path $Path -AllowNotFound
        if (!$existing) {
            if (!$PSCmdlet.ShouldProcess($Path, 'Create fixture resource')) { return $null }
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

    function Wait-FixtureOperation {
        param([object] $Response, [string] $Path)
        $pollUri = $Response.Headers['Azure-AsyncOperation'] | Select-Object -First 1
        if (!$pollUri) { $pollUri = $Response.Headers['Location'] | Select-Object -First 1 }
        if (!$pollUri) {
            if ($Response.StatusCode -eq 202) { throw "Accepted operation has no polling URL: $Path." }
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

    function Invoke-FixtureAction {
        param([string] $Path, [hashtable] $Body, [string[]] $RetryOnErrorCodes = @(), [int] $MaxAttempts = 1)
        for ($attempt = 1; ; $attempt++) {
            try {
                $response = Invoke-FixtureArm -Method POST -Action -Path $Path -Body $Body
                Wait-FixtureOperation -Response $response -Path $Path
                return
            }
            catch {
                $message = $_.Exception.Message
                $isRetryable = @($RetryOnErrorCodes | Where-Object { $message.Contains($_) }).Count -gt 0
                if ($attempt -ge $MaxAttempts -or !$isRetryable) { throw }
                Write-Warning "Retrying $Path after a transient service condition (attempt $attempt of $MaxAttempts)."
                Start-Sleep -Seconds 120
            }
        }
    }

    # Drill-run stage operations can remain in progress for the whole stage, so callers poll the run's
    # supported verbs instead of the operation status.
    function Invoke-DrillRunVerb {
        param([string] $Path, [hashtable] $Body)
        for ($attempt = 1; ; $attempt++) {
            try {
                Invoke-FixtureArm -Method POST -Action -Path $Path -Body $Body | Out-Null
                return
            }
            catch {
                # The previous stage operation releases the run lock asynchronously.
                if ($attempt -ge 40 -or !$_.Exception.Message.Contains('(409)')) { throw }
                Start-Sleep -Seconds 15
            }
        }
    }

    function Assert-NoFailedDrillTask {
        param([object] $Task)
        if ($Task.status -in @('Failed', 'Cancelled')) {
            throw "Drill run task '$($Task.taskName)' $($Task.status): $(ConvertTo-Json $Task.errorDetails -Depth 15 -Compress)"
        }
        foreach ($child in @($Task.jobExtendedInfo.tasksList) + @($Task.subTasksList)) {
            if ($null -ne $child) { Assert-NoFailedDrillTask -Task $child }
        }
    }

    function Wait-DrillRunVerb {
        param([string] $RunId, [string] $Stage, [string[]] $Verbs)
        Write-Host "Waiting for drill run stage $Stage to offer $($Verbs -join ' or ')."
        $deadline = [DateTime]::UtcNow.AddMinutes(30)
        do {
            $run = Get-FixtureResource -Path "$RunId`?api-version=$apiVersion"
            Assert-NoFailedDrillTask -Task $run.properties
            $stageVerbs = @($run.properties.supportedVerbsForStage | Where-Object { $_.drillRunStage -eq $Stage } | ForEach-Object { $_.supportedVerbs })
            if (@($stageVerbs | Where-Object { $_ -in $Verbs }).Count -gt 0) { return }
            Start-Sleep -Seconds 15
        } while ([DateTime]::UtcNow -lt $deadline)
        throw "Drill run stage $Stage did not offer $($Verbs -join ' or ') within the timeout."
    }

    # Recovery Orchestration rejects failover unless the plan is Ready, and discovering new service-group
    # members (for example, resources created by the fixture or the drill) moves it to Warning. Explicitly
    # exclude every member except the target VM, then finalize and re-run readiness until the plan is Ready.
    function Wait-FixturePlanReady {
        for ($attempt = 1; ; $attempt++) {
            $members = @((Get-FixtureResource -Path "$planId/recoveryResources?api-version=$apiVersion").value |
                Where-Object { $_.properties.resourceId -ne $vmId -and $_.properties.inclusionState -ne 'Included' })
            if ($members.Count -gt 0) {
                Invoke-FixtureAction -Path "$planId/updateResources?api-version=$apiVersion" -Body @{
                    resourcesToUpdate = @($members | ForEach-Object {
                        @{ id = $_.id; properties = @{ recoveryResourceUniqueId = $_.name; inclusionState = 'Excluded' } }
                    })
                    resourcesToRemove = @()
                }
                Wait-FixtureResource -Path "$planId`?api-version=$apiVersion" | Out-Null
                Invoke-FixtureAction -Path "$planId/finalize?api-version=$apiVersion" -RetryOnErrorCodes @('RHUBUserErrorAzureResourceAddedToServiceGroup') -MaxAttempts 3
            }
            Invoke-FixtureAction -Path "$planId/checkReadiness?api-version=$apiVersion" -RetryOnErrorCodes @('RHUBUserErrorAzureResourceAddedToServiceGroup') -MaxAttempts 3
            $planState = (Get-FixtureResource -Path "$planId`?api-version=$apiVersion").properties.planState
            if ($planState -eq 'Ready') { return }
            if ($attempt -ge 4) { throw "Recovery plan $recoveryPlanName did not return to Ready (state: $planState)." }
            Write-Warning "Recovery plan state is $planState; excluding newly discovered members and retrying."
            Start-Sleep -Seconds 60
        }
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

    # 1) Isolated service groups: the primary group owns the VM drill; the lifecycle group owns
    #    a disk-only member so creation tests can create and delete their own drills.
    $serviceGroupId = "/providers/Microsoft.Management/serviceGroups/$serviceGroupName"
    $lifecycleId = "/providers/Microsoft.Management/serviceGroups/$lifecycleServiceGroupName"
    foreach ($id in @($serviceGroupId, $lifecycleId)) {
        Write-Host "Preparing isolated drills service group $id."
        $name = $id.Split('/')[-1]
        New-FixtureResource -Path "$id`?api-version=2024-02-01-preview" -Body @{
            properties = @{ displayName = $name; parent = @{ resourceId = "/providers/Microsoft.Management/serviceGroups/$TenantId" } }
        } | Out-Null
        Wait-FixtureServiceGroupIndex -Id $id
    }

    New-FixtureResource -WaitForLinkedAuthorization -Path "$usagePlanId/enrollments/$lifecycleServiceGroupName`?api-version=$apiVersion" -Body @{
        properties = @{ serviceGroupId = $lifecycleId }
    } | Out-Null
    # Drill creation requires a nonempty group; this disk cannot affect the fault-injection VM.
    New-FixtureResource -Path "$deleteGroupId/providers/Microsoft.Compute/disks/delete-fixture-disk?api-version=2024-03-02" -Body @{
        location = $location
        zones = @('1')
        tags = @{ purpose = $fixturePurpose }
        sku = @{ name = 'Standard_LRS' }
        properties = @{ creationData = @{ createOption = 'Empty' }; diskSizeGB = 4 }
    } | Out-Null
    Invoke-FixtureArm -Method PUT -WaitForLinkedAuthorization -Path "$deleteGroupId/providers/Microsoft.Relationships/serviceGroupMember/delete-fixture?api-version=2023-09-01-preview" -Body @{
        properties = @{ targetId = $lifecycleId }
    } | Out-Null

    New-FixtureResource -WaitForLinkedAuthorization -Path "$usagePlanId/enrollments/$serviceGroupName`?api-version=$apiVersion" -Body @{
        properties = @{ serviceGroupId = $serviceGroupId }
    } | Out-Null
    Invoke-FixtureArm -Method PUT -Path "$groupId/providers/Microsoft.Relationships/serviceGroupMember/drills-ga?api-version=2023-09-01-preview" -Body @{
        properties = @{ targetId = $serviceGroupId }
    } | Out-Null

    $base = "$serviceGroupId/providers/Microsoft.AzureResilienceManagement"
    $drillId = "$base/drills/$drillName"
    $existingDrill = Get-FixtureResource -Path "$drillId`?api-version=$apiVersion" -AllowNotFound
    if ($existingDrill -and $existingDrill.properties.executionState -ne 'NotRunning') {
        throw 'An existing drill is running. Resolve its lifecycle before reconfiguring this fixture.'
    }

    # 2) Recovery plan that protects only the dedicated VM through custom runbooks.
    $planId = "$base/recoveryPlans/$recoveryPlanName"
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
            location = $location
            properties = @{ runbookType = 'PowerShell'; logProgress = $false; logVerbose = $false; description = 'Isolated Drills lifecycle fixture' }
        } | Out-Null
        $content = $runbookTemplate.Replace('__VM_ID__', $vmId).Replace('__PHASE__', $phase)
        Invoke-FixtureArm -Method PUT -Path "$runbookId/draft/content?api-version=2023-11-01" -ContentType 'text/powershell' -Body $content | Out-Null
        Invoke-FixtureArm -Method POST -Path "$runbookId/publish?api-version=2023-11-01" | Out-Null
    }

    # Newly discovered service-group members briefly fail readiness until discovery settles.
    Invoke-FixtureAction -Path "$planId/checkReadiness?api-version=$apiVersion" -RetryOnErrorCodes @('RHUBUserErrorAzureResourceAddedToServiceGroup') -MaxAttempts 6
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

    # Drill setup creates Chaos resources in this group, so new members can appear between the
    # update and finalize calls. Re-read the members and retry until finalize sees a stable set.
    for ($attempt = 1; ; $attempt++) {
        $resources = Get-FixtureResource -Path "$planId/recoveryResources?api-version=$apiVersion"
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
        }
        Wait-FixtureResource -Path "$planId`?api-version=$apiVersion" | Out-Null
        try {
            Invoke-FixtureAction -Path "$planId/finalize?api-version=$apiVersion"
            break
        }
        catch {
            if ($attempt -ge 4 -or !$_.Exception.Message.Contains('RHUBUserErrorAzureResourceAddedToServiceGroup')) { throw }
            Write-Warning 'New service-group members were discovered before finalize; excluding them and retrying.'
            Start-Sleep -Seconds 60
        }
    }
    $included = @((Get-FixtureResource -Path "$planId/recoveryResources?api-version=$apiVersion").value |
        Where-Object { $_.properties.inclusionState -eq 'Included' })
    if ($included.Count -ne 1 -or $included[0].properties.resourceId -ne $vmId) {
        throw 'Only the dedicated fixture VM may be included in the recovery plan.'
    }

    # 3) Health Model monitoring for the GA drill configuration tests.
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

    # 4) The drill. The second PUT runs after the drill identity holds its roles.
    $drillProperties = @{
        drillType = 'Zonal'
        rbacSetupMode = 'AutomatedBuiltinRoles'
        drillAssetProperties = @{ subscription = $SubscriptionId; region = $location; resourceGroup = $resourceGroupName }
        recoveryPlanProperties = @{ recoveryPlanId = $planId; identity = @{ type = 'SystemAssigned' } }
        chaosResourceProperties = @{
            identity = @{ type = 'SystemAssigned' }
            chaosResourceIdentityForFaults = @{ type = 'SystemAssigned' }
        }
        monitoringProperties = @{ identity = @{ type = 'SystemAssigned' } }
        goalAssignmentProperties = @{ identity = @{ type = 'SystemAssigned' } }
        healthModelMonitoringProperties = @{ identity = @{ type = 'SystemAssigned' }; healthModelId = $healthModelId }
    }
    $drill = New-FixtureResource -Path "$drillId`?api-version=$apiVersion" -Body @{
        identity = @{ type = 'SystemAssigned' }
        properties = $drillProperties
    }
    Add-FixtureRole -Scope $serviceGroupId -PrincipalId $drill.identity.principalId -RoleId '517781b0-5ad4-4418-94d5-f2421834b586'
    Add-FixtureRole -Scope "/subscriptions/$SubscriptionId" -PrincipalId $drill.identity.principalId -RoleId '344b740c-5e7c-4d32-89b2-ee3b51cb89bc'
    Start-Sleep -Seconds 60
    $response = Invoke-FixtureArm -Method PUT -Path "$drillId`?api-version=$apiVersion" -Body @{
        identity = @{ type = 'SystemAssigned' }
        properties = $drillProperties
    }
    Wait-FixtureOperation -Response $response -Path $drillId
    $drill = Wait-FixtureResource -Path "$drillId`?api-version=$apiVersion"
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

    $addOrUpdateBody = @{
        faultDurationInMin = 1
        forceInclusionAndUpdate = 'Disable'
        resourceLists = @{
            includeResources = @(@{ id = $drillTarget.id })
            excludeResources = @($drillResources.value | Where-Object { $_.properties.resourceId -ne $vmId } | ForEach-Object { $_.id })
            updateResources = @()
        }
    }
    try {
        Invoke-FixtureAction -Path "$drillId/addOrUpdateResources?api-version=$apiVersion" -Body $addOrUpdateBody
    }
    catch {
        if (!$_.Exception.Message.Contains('RHDSChaosResourceValidationFailed')) { throw }
        # The service asks for its supported fixup when the first Chaos scenario configuration fails.
        Write-Warning 'Chaos scenario configuration failed; running the supported resync fixup and retrying once.'
        Invoke-FixtureAction -Path "$drillId/resyncReadinessCheck?api-version=$apiVersion"
        Invoke-FixtureAction -Path "$drillId/addOrUpdateResources?api-version=$apiVersion" -Body $addOrUpdateBody
    }
    $drillTarget = Get-FixtureResource -Path "$($drillTarget.id)?api-version=$apiVersion"
    if ($drillTarget.properties.inclusionState -ne 'Included') {
        throw "The dedicated VM was not included: $(ConvertTo-Json $drillTarget.properties -Depth 15 -Compress)"
    }
    $sourceLocation = @($target.properties.resourcePhysicalZones)
    if ($sourceLocation.Count -ne 1) { throw 'Expected exactly one physical source zone for the dedicated VM.' }

    # 5) A completed drill run for the read-only drill-run tests. Drill-run resources exist only after
    #    failover, so the run completes a full manual failover and reprotect cycle on the dedicated VM
    #    and leaves the drill NotRunning for the destructive lifecycle tests.
    Wait-FixturePlanReady
    $runsPath = "$drillId/drillRuns?api-version=$apiVersion"
    $existingRunIds = @((Get-FixtureResource -Path $runsPath).value | ForEach-Object { $_.id })
    Invoke-FixtureAction -Path "$drillId/start?api-version=$apiVersion" -Body @{ mode = 'Failover' }
    $deadline = [DateTime]::UtcNow.AddMinutes(15)
    do {
        $drillRun = (Get-FixtureResource -Path $runsPath).value | Where-Object { $_.id -notin $existingRunIds } | Select-Object -First 1
        if ($drillRun) { break }
        Start-Sleep -Seconds 15
    } while ([DateTime]::UtcNow -lt $deadline)
    if (!$drillRun) { throw "No drill run appeared after starting drill $drillName." }
    $runId = "$drillId/drillRuns/$($drillRun.name)"

    Invoke-DrillRunVerb -Path "$runId/failOver?api-version=$apiVersion" -Body @{
        autoFailover = 'Disable'
        failoverProperties = @{
            failoverDirection = 'FromSpecificLocations'
            failoverRequestProperties = @{ sourceLocations = @($sourceLocation[0]) }
        }
    }
    Wait-DrillRunVerb -RunId $runId -Stage 'FaultInjection' -Verbs 'MarkAsComplete'
    Invoke-DrillRunVerb -Path "$runId/markAsComplete?api-version=$apiVersion" -Body @{ drillRunStage = 'FaultInjection' }
    Wait-DrillRunVerb -RunId $runId -Stage 'Failover' -Verbs 'Start'
    Invoke-DrillRunVerb -Path "$runId/resume?api-version=$apiVersion"
    Wait-DrillRunVerb -RunId $runId -Stage 'Failover' -Verbs 'MarkAsComplete'
    Invoke-DrillRunVerb -Path "$runId/markAsComplete?api-version=$apiVersion" -Body @{ drillRunStage = 'Failover' }
    Wait-DrillRunVerb -RunId $runId -Stage 'Reprotect' -Verbs 'Start', 'Retry'
    # The service accepts a reprotect without a body but never processes it.
    Invoke-DrillRunVerb -Path "$runId/reprotect?api-version=$apiVersion" -Body @{ reprotectProperties = @{} }
    Wait-DrillRunVerb -RunId $runId -Stage 'Reprotect' -Verbs 'MarkAsComplete'
    $drillRunTarget = (Get-FixtureResource -Path "$runId/drillRunResources?api-version=$apiVersion").value | Select-Object -First 1
    if (!$drillRunTarget) { throw "No drill run resource appeared for drill run $($drillRun.name)." }
    Invoke-DrillRunVerb -Path "$runId/markAsComplete?api-version=$apiVersion" -Body @{ drillRunStage = 'Reprotect' }

    $endBody = @{ attestation = 'Success'; attestationNotes = 'Azure MCP live-test fixture run.' }
    for ($attempt = 1; ; $attempt++) {
        try {
            Invoke-FixtureAction -Path "$drillId/end?api-version=$apiVersion" -Body $endBody
            break
        }
        catch {
            # End conflicts while the start operation is still settling.
            if ($attempt -ge 20 -or !$_.Exception.Message.Contains('(409)')) { throw }
            Start-Sleep -Seconds 15
        }
    }
    $deadline = [DateTime]::UtcNow.AddMinutes(15)
    do {
        $drill = Get-FixtureResource -Path "$drillId`?api-version=$apiVersion"
        if ($drill.properties.executionState -eq 'NotRunning') { break }
        Start-Sleep -Seconds 15
    } while ([DateTime]::UtcNow -lt $deadline)
    if ($drill.properties.executionState -ne 'NotRunning') { throw "Drill $drillName did not return to NotRunning after the fixture run." }
    # The lifecycle tests run further forward failover drills, which require a Ready plan.
    Wait-FixturePlanReady

    $DeploymentOutputs['DRILLSDRILLRESOURCEID'] = $drillTarget.id
    $DeploymentOutputs['DRILLSSOURCELOCATION'] = $sourceLocation[0]
    $DeploymentOutputs['DRILLSRECOVERYRESOURCEID'] = $target.id
    $DeploymentOutputs['DRILLSDRILLRUNNAME'] = $drillRun.name
    $DeploymentOutputs['DRILLSDRILLRUNRESOURCENAME'] = $drillRunTarget.name
    Write-Host "Drills fixture ready (service group: $serviceGroupName, drill: $drillName, completed run: $($drillRun.name))."
}
Initialize-DrillsFixture -DeploymentOutputs $DeploymentOutputs -SubscriptionId $subscriptionId -TenantId $tenantId

New-TestSettings @PSBoundParameters -OutputPath $PSScriptRoot | Out-Null

Write-Host "Resilience test resources are ready (service group: $serviceGroupName, usage plan: $usagePlanName, enrollment: $enrollmentName, goal assignment: $goalAssignmentName, recoveryplan: $recoveryPlanName, drills service group: $($DeploymentOutputs['DRILLSSERVICEGROUPNAME']))."
