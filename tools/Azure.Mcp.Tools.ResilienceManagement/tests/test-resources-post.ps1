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
$goalTemplateName = $DeploymentOutputs['GOALTEMPLATENAME']
$goalAssignmentName = $DeploymentOutputs['GOALASSIGNMENTNAME']
$recoveryPlanName = $DeploymentOutputs['RECOVERYPLANNAME']

$serviceGroupApiVersion = '2024-02-01-preview'
$membershipApiVersion = '2023-09-01-preview'
$resilienceApiVersion = '2026-04-01-preview'

$serviceGroupId = "/providers/Microsoft.Management/serviceGroups/$serviceGroupName"
$serviceGroupResilienceBase = "$serviceGroupId/providers/Microsoft.AzureResilienceManagement"

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
        [string] $Path
    )

    Write-Host "POST $Path"
    $response = Invoke-AzRestMethod -Method POST -Path $Path
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

# 2) Add the resource group as a member of the service group so its resources
#    (e.g. the storage account) surface as goal/recovery/drill resource targets.
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

Write-Host "Resilience test resources are ready (service group: $serviceGroupName, usage plan: $usagePlanName, enrollment: $enrollmentName, goal template: $goalTemplateName, goal assignment: $goalAssignmentName, recovery plan: $recoveryPlanName)."
