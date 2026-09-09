param(
    [string] $ResourceGroupName,
    [string] $TestSettingsPath = (Join-Path $PSScriptRoot '.testsettings.json')
)

# cspell:ignore rhub

$ErrorActionPreference = 'Stop'

if (!(Test-Path $TestSettingsPath)) {
    throw "Test settings file '$TestSettingsPath' was not found."
}

$testSettings = Get-Content $TestSettingsPath -Raw | ConvertFrom-Json
$outputs = $testSettings.DeploymentOutputs
$subscriptionId = $testSettings.SubscriptionId
$tenantId = $testSettings.TenantId
$serviceGroupApiVersion = '2024-02-01-preview'
$membershipApiVersion = '2023-09-01-preview'
$resilienceApiVersion = '2026-04-01-preview'

$accessToken = az account get-access-token --tenant $tenantId --resource 'https://management.azure.com/' --query accessToken --output tsv
if ($LASTEXITCODE -ne 0 -or [string]::IsNullOrWhiteSpace($accessToken)) {
    throw "Azure CLI could not acquire an Azure Resource Manager token for tenant '$tenantId'."
}

$requestHeaders = @{ Authorization = "Bearer $accessToken" }

function Invoke-ManagementRestMethod {
    param([string] $Method, [string] $Path)

    $response = Invoke-WebRequest `
        -Method $Method `
        -Uri "https://management.azure.com$Path" `
        -Headers $requestHeaders `
        -SkipHttpErrorCheck

    return [pscustomobject]@{
        StatusCode = [int]$response.StatusCode
        Content    = $response.Content
    }
}

function Get-OutputValue {
    param([string] $Name)

    return $outputs.PSObject.Properties[$Name.ToUpperInvariant()].Value
}

function Wait-ResourceDeleted {
    param([string] $ResourceId, [string] $ApiVersion)

    $path = "${ResourceId}?api-version=$ApiVersion"
    for ($attempt = 0; $attempt -lt 60; $attempt++) {
        $response = Invoke-ManagementRestMethod -Method GET -Path $path
        if ($response.StatusCode -eq 404) {
            return
        }

        if ($response.StatusCode -ne 200) {
            throw "GET $path failed with status $($response.StatusCode): $($response.Content)"
        }

        Start-Sleep -Seconds 5
    }

    throw "Timed out waiting for '$ResourceId' to be deleted."
}

function Remove-Resource {
    param([string] $ResourceId, [string] $ApiVersion)

    if ([string]::IsNullOrWhiteSpace($ResourceId)) {
        return
    }

    $path = "${ResourceId}?api-version=$ApiVersion"
    Write-Host "DELETE $path"
    $response = Invoke-ManagementRestMethod -Method DELETE -Path $path
    if ($response.StatusCode -notin @(200, 202, 204, 404)) {
        throw "DELETE $path failed with status $($response.StatusCode): $($response.Content)"
    }

    if ($response.StatusCode -ne 404) {
        Wait-ResourceDeleted -ResourceId $ResourceId -ApiVersion $ApiVersion
    }
}

function Remove-RecoveryPlans {
    param([string] $ServiceGroupId)

    $collectionPath = "$ServiceGroupId/providers/Microsoft.AzureResilienceManagement/recoveryPlans?api-version=$resilienceApiVersion"
    $response = Invoke-ManagementRestMethod -Method GET -Path $collectionPath
    if ($response.StatusCode -eq 404) {
        return
    }

    if ($response.StatusCode -ne 200) {
        throw "GET $collectionPath failed with status $($response.StatusCode): $($response.Content)"
    }

    $content = $response.Content | ConvertFrom-Json
    foreach ($recoveryPlan in $content.value) {
        Remove-Resource -ResourceId $recoveryPlan.id -ApiVersion $resilienceApiVersion
    }
}

$serviceGroupName = Get-OutputValue -Name 'serviceGroupName'
$lifecycleServiceGroupName = Get-OutputValue -Name 'lifecycleServiceGroupName'
$planLifecycleServiceGroupName = Get-OutputValue -Name 'planLifecycleServiceGroupName'
$workflowServiceGroupName = Get-OutputValue -Name 'workflowServiceGroupName'
$usagePlanName = Get-OutputValue -Name 'usagePlanName'
$enrollmentName = Get-OutputValue -Name 'enrollmentName'
$lifecycleEnrollmentName = Get-OutputValue -Name 'lifecycleEnrollmentName'
$planLifecycleEnrollmentName = Get-OutputValue -Name 'planLifecycleEnrollmentName'
$workflowEnrollmentName = Get-OutputValue -Name 'workflowEnrollmentName'
$goalAssignmentName = Get-OutputValue -Name 'goalAssignmentName'
$goalTemplateName = Get-OutputValue -Name 'goalTemplateName'
$drillName = Get-OutputValue -Name 'drillName'

$requiredOutputs = @{
    serviceGroupName          = $serviceGroupName
    lifecycleServiceGroupName = $lifecycleServiceGroupName
    usagePlanName             = $usagePlanName
    enrollmentName            = $enrollmentName
    lifecycleEnrollmentName   = $lifecycleEnrollmentName
    goalAssignmentName        = $goalAssignmentName
    goalTemplateName          = $goalTemplateName
    drillName                 = $drillName
}

foreach ($requiredOutput in $requiredOutputs.GetEnumerator()) {
    if ([string]::IsNullOrWhiteSpace($requiredOutput.Value)) {
        throw "Deployment output '$($requiredOutput.Key)' is required for safe ResilienceManagement teardown."
    }
}

$serviceGroupId = "/providers/Microsoft.Management/serviceGroups/$serviceGroupName"
$lifecycleServiceGroupId = "/providers/Microsoft.Management/serviceGroups/$lifecycleServiceGroupName"
$planLifecycleServiceGroupId = "/providers/Microsoft.Management/serviceGroups/$planLifecycleServiceGroupName"
$workflowServiceGroupId = "/providers/Microsoft.Management/serviceGroups/$workflowServiceGroupName"
$resilienceBase = "$serviceGroupId/providers/Microsoft.AzureResilienceManagement"
$usagePlanId = "/subscriptions/$subscriptionId/resourceGroups/$ResourceGroupName/providers/Microsoft.AzureResilienceManagement/usagePlans/$usagePlanName"

Remove-Resource -ResourceId "$resilienceBase/drills/$drillName" -ApiVersion $resilienceApiVersion
Remove-RecoveryPlans -ServiceGroupId $serviceGroupId
Remove-RecoveryPlans -ServiceGroupId $lifecycleServiceGroupId
if (![string]::IsNullOrWhiteSpace($planLifecycleServiceGroupName)) {
    Remove-RecoveryPlans -ServiceGroupId $planLifecycleServiceGroupId
}
if (![string]::IsNullOrWhiteSpace($workflowServiceGroupName)) {
    Remove-RecoveryPlans -ServiceGroupId $workflowServiceGroupId
}
Remove-Resource -ResourceId "$resilienceBase/goalAssignments/$goalAssignmentName" -ApiVersion $resilienceApiVersion
Remove-Resource -ResourceId "$resilienceBase/goalTemplates/$goalTemplateName" -ApiVersion $resilienceApiVersion
Remove-Resource -ResourceId "$usagePlanId/enrollments/$enrollmentName" -ApiVersion $resilienceApiVersion
Remove-Resource -ResourceId "$usagePlanId/enrollments/$lifecycleEnrollmentName" -ApiVersion $resilienceApiVersion
if (![string]::IsNullOrWhiteSpace($planLifecycleEnrollmentName)) {
    Remove-Resource -ResourceId "$usagePlanId/enrollments/$planLifecycleEnrollmentName" -ApiVersion $resilienceApiVersion
}
if (![string]::IsNullOrWhiteSpace($workflowEnrollmentName)) {
    Remove-Resource -ResourceId "$usagePlanId/enrollments/$workflowEnrollmentName" -ApiVersion $resilienceApiVersion
}
Remove-Resource -ResourceId "/subscriptions/$subscriptionId/resourceGroups/$ResourceGroupName/providers/Microsoft.Relationships/serviceGroupMember/rhub-rg-member" -ApiVersion $membershipApiVersion
Remove-Resource -ResourceId "/subscriptions/$subscriptionId/resourceGroups/$ResourceGroupName/providers/Microsoft.Relationships/serviceGroupMember/rhub-lifecycle-rg-member" -ApiVersion $membershipApiVersion
Remove-Resource -ResourceId "/subscriptions/$subscriptionId/resourceGroups/$ResourceGroupName/providers/Microsoft.Relationships/serviceGroupMember/rhub-plan-lifecycle-rg-member" -ApiVersion $membershipApiVersion
Remove-Resource -ResourceId "/subscriptions/$subscriptionId/resourceGroups/$ResourceGroupName/providers/Microsoft.Relationships/serviceGroupMember/rhub-workflow-rg-member" -ApiVersion $membershipApiVersion

foreach ($serviceGroupIdToDelete in @($workflowServiceGroupName ? $workflowServiceGroupId : $null, $planLifecycleServiceGroupName ? $planLifecycleServiceGroupId : $null, $lifecycleServiceGroupId, $serviceGroupId)) {
    if ($serviceGroupIdToDelete -notmatch '/$') {
        Remove-Resource -ResourceId $serviceGroupIdToDelete -ApiVersion $serviceGroupApiVersion
    }
}
