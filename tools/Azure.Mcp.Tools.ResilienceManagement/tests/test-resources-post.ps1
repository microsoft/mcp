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
$goalTemplateName = $DeploymentOutputs['GOALTEMPLATENAME']
$goalAssignmentName = $DeploymentOutputs['GOALASSIGNMENTNAME']
$recoveryPlanName = $DeploymentOutputs['RECOVERYPLANNAME']
$drillName = $DeploymentOutputs['DRILLNAME']
$storageAccountId = $DeploymentOutputs['STORAGEACCOUNTID']
$location = $DeploymentOutputs['LOCATION']

$serviceGroupApiVersion = '2024-02-01-preview'
$membershipApiVersion = '2023-09-01-preview'
$resilienceApiVersion = '2026-04-01-preview'

$serviceGroupId = "/providers/Microsoft.Management/serviceGroups/$serviceGroupName"
$serviceGroupResilienceBase = "$serviceGroupId/providers/Microsoft.AzureResilienceManagement"
$lifecycleServiceGroupId = "/providers/Microsoft.Management/serviceGroups/$lifecycleServiceGroupName"

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
        [hashtable] $Body
    )

    Write-Host "POST $Path"
    if ($Body) {
        $payload = $Body | ConvertTo-Json -Depth 20 -Compress
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
        [int] $TimeoutSeconds = 900
    )

    $deadline = (Get-Date).AddSeconds($TimeoutSeconds)
    while ((Get-Date) -lt $deadline) {
        $response = Invoke-AzRestMethod -Method GET -Path $Path

        # Creation of these resources is asynchronous and eventually consistent, so a
        # 404 immediately after the PUT is expected. Treat it as "not ready yet" and keep
        # polling until the resource appears or we hit the timeout.
        if ($response.StatusCode -eq 404) {
            Write-Host "  not found yet (still provisioning)"
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
            throw "Provisioning of $Path ended in state '$state'."
        }

        Start-Sleep -Seconds 15
    }

    throw "Timed out waiting for $Path to finish provisioning."
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
Wait-ResilienceProvisioning -Path $serviceGroupPath

# Create a second enrolled service group without a recovery plan. Lifecycle tests use it
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
Wait-ResilienceProvisioning -Path $lifecycleServiceGroupPath

# 2) Add the resource group as a member of the service group so its resources
#    (e.g. the storage account) surface as goal/recovery resource targets.
$membershipPath = "/subscriptions/$subscriptionId/resourceGroups/$ResourceGroupName/providers/Microsoft.Relationships/serviceGroupMember/rhub-rg-member`?api-version=$membershipApiVersion"
Invoke-ResilienceRestPut -Path $membershipPath -Body @{
    properties = @{
        targetId = $serviceGroupId
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
Invoke-ResilienceRestPut -Path $goalAssignmentPath -Body @{
    properties = @{
        goalAssignmentType = 'Resiliency'
        goalTemplateId     = "$serviceGroupResilienceBase/goalTemplates/$goalTemplateName"
    }
} | Out-Null
Wait-ResilienceProvisioning -Path $goalAssignmentPath

# 6) Create a recovery plan on the service group.
$recoveryPlanPath = "$serviceGroupResilienceBase/recoveryPlans/$recoveryPlanName`?api-version=$resilienceApiVersion"
Invoke-ResilienceRestPut -Path $recoveryPlanPath -Body @{
    identity   = @{
        type = 'SystemAssigned'
    }
    properties = @{
        planDescription       = 'Recovery plan for live testing.'
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

# 7) Run a readiness check on the recovery plan so it has a recorded validation status.
$checkReadinessPath = "$serviceGroupResilienceBase/recoveryPlans/$recoveryPlanName/checkReadiness`?api-version=$resilienceApiVersion"
Invoke-ResilienceRestPost -Path $checkReadinessPath | Out-Null
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
Invoke-ResilienceRestPut -Path $drillPath -Body @{
    identity   = @{
        type = 'SystemAssigned'
    }
    properties = @{
        drillType             = 'Zonal'
        rbacSetupMode         = 'AutomatedCustomRole'
        metricsProperties     = @{
            identity       = @{ type = 'SystemAssigned' }
            metricsToTrack = @()
        }
        recoveryPlanProperties = @{
            identity = @{ type = 'SystemAssigned' }
        }
        drillAssetProperties = @{
            subscription = $subscriptionId
            region       = $location
        }
        chaosResourceProperties = @{
            identity                       = @{ type = 'SystemAssigned' }
            chaosResourceIdentityForFaults = @{ type = 'SystemAssigned' }
        }
    }
} | Out-Null
Wait-ResilienceProvisioning -Path $drillPath

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
        $_.properties.targetResourceId -eq $storageAccountId
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

$addResourcesPath = "$serviceGroupResilienceBase/drills/$drillName/addOrUpdateResources`?api-version=$resilienceApiVersion&operationId=$((New-Guid).Guid)"
Invoke-ResilienceRestPost -Path $addResourcesPath -Body @{
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

$startPath = "$serviceGroupResilienceBase/drills/$drillName/start`?api-version=$resilienceApiVersion&operationId=$((New-Guid).Guid)"
Invoke-ResilienceRestPost -Path $startPath -Body @{
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

Write-Host "Resilience test resources are ready (service group: $serviceGroupName, usage plan: $usagePlanName, enrollment: $enrollmentName, goal template: $goalTemplateName, goal assignment: $goalAssignmentName, recovery plan: $recoveryPlanName, drill: $drillName, drill run: $($drillRun.name))."
