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
$goalTemplateName = $DeploymentOutputs['GOALTEMPLATENAME']
$goalAssignmentName = $DeploymentOutputs['GOALASSIGNMENTNAME']
$recoveryPlanName = $DeploymentOutputs['RECOVERYPLANNAME']
$drillName = $DeploymentOutputs['DRILLNAME']
$deleteDrillName = $DeploymentOutputs['DELETEDRILLNAME']
$storageAccountId = $DeploymentOutputs['STORAGEACCOUNTID']
$location = $DeploymentOutputs['LOCATION']

$serviceGroupApiVersion = '2024-02-01-preview'
$membershipApiVersion = '2023-09-01-preview'
$resilienceApiVersion = '2026-04-01-preview'

$serviceGroupId = "/providers/Microsoft.Management/serviceGroups/$serviceGroupName"
$serviceGroupResilienceBase = "$serviceGroupId/providers/Microsoft.AzureResilienceManagement"
$lifecycleServiceGroupId = "/providers/Microsoft.Management/serviceGroups/$lifecycleServiceGroupName"
$lifecycleServiceGroupResilienceBase = "$lifecycleServiceGroupId/providers/Microsoft.AzureResilienceManagement"
$planLifecycleServiceGroupId = "/providers/Microsoft.Management/serviceGroups/$planLifecycleServiceGroupName"

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
            } else {
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
        [string] $Scope
    )

    $roleName = 'Azure Resilience Management Recovery Contributor'
    $assignment = Get-AzRoleAssignment -ObjectId $TestApplicationOid -Scope $Scope -RoleDefinitionName $roleName -ErrorAction SilentlyContinue
    if (!$assignment) {
        Write-Host "Assigning $roleName to test identity at $Scope"
        New-AzRoleAssignment -ObjectId $TestApplicationOid -Scope $Scope -RoleDefinitionName $roleName | Out-Null
    }
}

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

Add-RecoveryContributorRole -Scope $serviceGroupId
Add-RecoveryContributorRole -Scope $lifecycleServiceGroupId
Add-RecoveryContributorRole -Scope $planLifecycleServiceGroupId

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

# 4) Create a goal template on the service group.
$goalTemplatePath = "$serviceGroupResilienceBase/goalTemplates/$goalTemplateName`?api-version=$resilienceApiVersion"
Invoke-ResilienceRestPut -Path $goalTemplatePath -Body @{
    properties = @{
        goalType                       = 'Resiliency'
        requireHighAvailability        = 'Required'
        requireDisasterRecovery        = 'NotRequired'
        regionalRecoveryPointObjective = 'PT15M'
        regionalRecoveryTimeObjective  = 'PT30M'
    }
} | Out-Null
Wait-ResilienceProvisioning -Path $goalTemplatePath

# 5) Assign the goal template to the service group.
$goalAssignmentPath = "$serviceGroupResilienceBase/goalAssignments/$goalAssignmentName`?api-version=$resilienceApiVersion"
$goalTemplateId = "$serviceGroupResilienceBase/goalTemplates/$goalTemplateName"
$existingGoalAssignment = Invoke-AzRestMethod -Method GET -Path $goalAssignmentPath
if ($existingGoalAssignment.StatusCode -eq 404) {
    Invoke-ResilienceRestPut -Path $goalAssignmentPath -Body @{
        properties = @{
            goalAssignmentType = 'Resiliency'
            goalTemplateId     = $goalTemplateId
        }
    } | Out-Null
    Wait-ResilienceProvisioning -Path $goalAssignmentPath
} elseif ($existingGoalAssignment.StatusCode -eq 200) {
    $goalAssignment = $existingGoalAssignment.Content | ConvertFrom-Json
    if ($goalAssignment.properties.goalAssignmentType -ne 'Resiliency' -or
        $goalAssignment.properties.goalTemplateId -ne $goalTemplateId -or
        $goalAssignment.properties.provisioningState -ne 'Succeeded') {
        throw "Existing goal assignment '$goalAssignmentName' does not match the requested test configuration."
    }
    Write-Host "Goal assignment '$goalAssignmentName' already exists with the requested configuration."
} else {
    throw "GET $goalAssignmentPath failed with status $($existingGoalAssignment.StatusCode): $($existingGoalAssignment.Content)"
}

# 6) Create or validate the recoveryplan on the service group. Do not PUT an
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
    Wait-ResilienceProvisioning -Path $recoveryPlanPath
} elseif ($existingRecoveryPlan.StatusCode -eq 200) {
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
} else {
    throw "GET $recoveryPlanPath failed with status $($existingRecoveryPlan.StatusCode): $($existingRecoveryPlan.Content)"
}

# 7) Run a readiness check on the recoveryplan so it has a recorded validation status.
$checkReadinessPath = "$serviceGroupResilienceBase/recoveryPlans/$recoveryPlanName/checkReadiness`?api-version=$resilienceApiVersion"
Invoke-ResilienceRestPost -Path $checkReadinessPath -OperationId (New-Guid).Guid | Out-Null
Wait-ResilienceProvisioning -Path $recoveryPlanPath

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

# 8) Create a drill. The service creates its drillResources from the service-group membership.
$drillPath = "$serviceGroupResilienceBase/drills/$drillName`?api-version=$resilienceApiVersion"
$recoveryPlanId = "$serviceGroupResilienceBase/recoveryPlans/$recoveryPlanName"
$existingDrill = Invoke-AzRestMethod -Method GET -Path $drillPath
if ($existingDrill.StatusCode -eq 404) {
    Invoke-ResilienceRestPut -Path $drillPath -Body @{
        identity   = @{
            type = 'SystemAssigned'
        }
        properties = @{
            drillType               = 'Zonal'
            rbacSetupMode           = 'AutomatedBuiltinRoles'
            recoveryPlanProperties  = @{
                recoveryPlanId = $recoveryPlanId
                identity       = @{ type = 'SystemAssigned' }
            }
            drillAssetProperties    = @{
                subscription  = $subscriptionId
                region        = $location
                resourceGroup = $ResourceGroupName
            }
            chaosResourceProperties = @{
                identity                       = @{ type = 'SystemAssigned' }
                chaosResourceIdentityForFaults = @{ type = 'SystemAssigned' }
            }
            monitoringProperties    = @{
                identity = @{ type = 'SystemAssigned' }
            }
        }
    } | Out-Null
    Wait-ResilienceProvisioning -Path $drillPath
}
elseif ($existingDrill.StatusCode -eq 200) {
    $drill = $existingDrill.Content | ConvertFrom-Json
    if ($drill.properties.drillType -ne 'Zonal' -or
        $drill.properties.rbacSetupMode -ne 'AutomatedBuiltinRoles' -or
        $drill.properties.drillAssetProperties.subscription -ne $subscriptionId -or
        $drill.properties.drillAssetProperties.region -ne $location -or
        $drill.properties.drillAssetProperties.resourceGroup -ne $ResourceGroupName -or
        $drill.properties.recoveryPlanProperties.recoveryPlanId -ne $recoveryPlanId -or
        $drill.properties.monitoringProperties.identity.type -ne 'SystemAssigned' -or
        $drill.properties.provisioningState -ne 'Succeeded') {
        throw "Existing drill '$drillName' does not match the requested test configuration."
    }
    Write-Host "Drill '$drillName' already exists with the requested configuration."
}
else {
    throw "GET $drillPath failed with status $($existingDrill.StatusCode): $($existingDrill.Content)"
}

# 8b) Create an isolated recoveryplan and drill used exclusively by the delete live test.
$deleteRecoveryPlanPath = "$lifecycleServiceGroupResilienceBase/recoveryPlans/$recoveryPlanName`?api-version=$resilienceApiVersion"
if ((Invoke-AzRestMethod -Method GET -Path $deleteRecoveryPlanPath).StatusCode -eq 404) {
    Invoke-ResilienceRestPut -Path $deleteRecoveryPlanPath -Body @{
        identity   = @{
            type = 'SystemAssigned'
        }
        properties = @{
            planDescription       = 'Recoveryplan for the drill delete live test.'
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
    Wait-ResilienceProvisioning -Path $deleteRecoveryPlanPath
}

$deleteDrillPath = "$lifecycleServiceGroupResilienceBase/drills/$deleteDrillName`?api-version=$resilienceApiVersion"
$deleteRecoveryPlanId = "$lifecycleServiceGroupResilienceBase/recoveryPlans/$recoveryPlanName"
if ((Invoke-AzRestMethod -Method GET -Path $deleteDrillPath).StatusCode -eq 404) {
    Invoke-ResilienceRestPut -Path $deleteDrillPath -Body @{
        identity   = @{
            type = 'SystemAssigned'
        }
        properties = @{
            drillType               = 'Zonal'
            rbacSetupMode           = 'AutomatedBuiltinRoles'
            recoveryPlanProperties  = @{
                recoveryPlanId = $deleteRecoveryPlanId
                identity       = @{ type = 'SystemAssigned' }
            }
            drillAssetProperties    = @{
                subscription  = $subscriptionId
                region        = $location
                resourceGroup = $ResourceGroupName
            }
            chaosResourceProperties = @{
                identity                       = @{ type = 'SystemAssigned' }
                chaosResourceIdentityForFaults = @{ type = 'SystemAssigned' }
            }
            monitoringProperties    = @{
                identity = @{ type = 'SystemAssigned' }
            }
        }
    } | Out-Null
    Wait-ResilienceProvisioning -Path $deleteDrillPath
}

# Wait for the drill resource created from the storage-account service-group member.
$drillResourcesPath = "$serviceGroupResilienceBase/drills/$drillName/drillResources`?api-version=$resilienceApiVersion"
$drillResource = $null
$deadline = (Get-Date).AddSeconds(600)
while (-not $drillResource -and (Get-Date) -lt $deadline) {
    $response = Invoke-AzRestMethod -Method GET -Path $drillResourcesPath
    if ($response.StatusCode -ge 400) {
        throw "GET $drillResourcesPath failed with status $($response.StatusCode): $($response.Content)"
    }

    $drillResources = ($response.Content | ConvertFrom-Json).value
    $drillResource = $drillResources | Where-Object {
        $_.properties.resourceId -eq $storageAccountId
    } | Select-Object -First 1

    if (-not $drillResource) {
        Write-Host "  waiting for the storage account drill resource to appear..."
        Start-Sleep -Seconds 15
    }
}

if (-not $drillResource) {
    throw "No drill resource was created for storage account $storageAccountId."
}

$DeploymentOutputs['DRILLRESOURCENAME'] = $drillResource.name

# 9) Include the storage account in the drill, preserving the faults discovered by the service.
$includeResource = @{
    id = $drillResource.id
}
if ($drillResource.properties.faultProperties) {
    $includeResource['faultProperties'] = $drillResource.properties.faultProperties
}

$addResourcesPath = "$serviceGroupResilienceBase/drills/$drillName/addOrUpdateResources`?api-version=$resilienceApiVersion"
Invoke-ResilienceRestPost -Path $addResourcesPath -OperationId (New-Guid).Guid -Body @{
    faultDurationInMin = 1
    forceInclusionAndUpdate = 'Enable'
    resourceLists = @{
        includeResources = @($includeResource)
        excludeResources = @()
        updateResources  = @()
    }
} | Out-Null

$drillResourcePath = "$($drillResource.id)`?api-version=$resilienceApiVersion"
$deadline = (Get-Date).AddSeconds(900)
do {
    $response = Invoke-AzRestMethod -Method GET -Path $drillResourcePath
    if ($response.StatusCode -ge 400) {
        throw "GET $drillResourcePath failed with status $($response.StatusCode): $($response.Content)"
    }

    $includedResource = $response.Content | ConvertFrom-Json
    if ($includedResource.properties.inclusionState -eq 'Included') {
        break
    }

    Write-Host "  drill resource inclusionState = $($includedResource.properties.inclusionState)"
    Start-Sleep -Seconds 15
} while ((Get-Date) -lt $deadline)

if ($includedResource.properties.inclusionState -ne 'Included') {
    throw "Drill resource $($drillResource.name) was not included before the timeout."
}

# 10) Start the drill and discover the server-generated drill run.
$drillRunsPath = "$serviceGroupResilienceBase/drills/$drillName/drillRuns`?api-version=$resilienceApiVersion"
$existingRunIds = @()
$existingRunsResponse = Invoke-AzRestMethod -Method GET -Path $drillRunsPath
if ($existingRunsResponse.StatusCode -lt 400) {
    $existingRunIds = @((($existingRunsResponse.Content | ConvertFrom-Json).value).id)
}

$startPath = "$serviceGroupResilienceBase/drills/$drillName/start`?api-version=$resilienceApiVersion"
Invoke-ResilienceRestPost -Path $startPath -OperationId (New-Guid).Guid -Body @{
    mode = 'Failover'
} | Out-Null

$drillRun = $null
$deadline = (Get-Date).AddSeconds(900)
while (-not $drillRun -and (Get-Date) -lt $deadline) {
    $response = Invoke-AzRestMethod -Method GET -Path $drillRunsPath
    if ($response.StatusCode -ge 400) {
        throw "GET $drillRunsPath failed with status $($response.StatusCode): $($response.Content)"
    }

    $runs = ($response.Content | ConvertFrom-Json).value
    $drillRun = $runs | Where-Object { $_.id -notin $existingRunIds } | Select-Object -First 1
    if (-not $drillRun) {
        Write-Host "  waiting for the drill run to appear..."
        Start-Sleep -Seconds 15
    }
}

if (-not $drillRun) {
    throw "No drill run appeared after starting drill $drillName."
}

$DeploymentOutputs['DRILLRUNNAME'] = $drillRun.name

# Capture a run target for the drill run resource live tests.
$drillRunResourcesPath = "$serviceGroupResilienceBase/drills/$drillName/drillRuns/$($drillRun.name)/drillRunTargets`?api-version=$resilienceApiVersion"
$drillRunResource = $null
$deadline = (Get-Date).AddSeconds(900)
while (-not $drillRunResource -and (Get-Date) -lt $deadline) {
    $response = Invoke-AzRestMethod -Method GET -Path $drillRunResourcesPath
    if ($response.StatusCode -ge 400) {
        throw "GET $drillRunResourcesPath failed with status $($response.StatusCode): $($response.Content)"
    }

    $drillRunResources = ($response.Content | ConvertFrom-Json).value
    $drillRunResource = $drillRunResources | Select-Object -First 1
    if (-not $drillRunResource) {
        Write-Host "  waiting for the drill run target to appear..."
        Start-Sleep -Seconds 15
    }
}

if (-not $drillRunResource) {
    throw "No drill run target appeared for drill run $($drillRun.name)."
}

$DeploymentOutputs['DRILLRUNRESOURCENAME'] = $drillRunResource.name

New-TestSettings @PSBoundParameters -OutputPath $PSScriptRoot | Out-Null

Write-Host "Resilience test resources are ready (service group: $serviceGroupName, usage plan: $usagePlanName, enrollment: $enrollmentName, goal template: $goalTemplateName, goal assignment: $goalAssignmentName, recoveryplan: $recoveryPlanName, drill: $drillName, drill run: $($drillRun.name))."
