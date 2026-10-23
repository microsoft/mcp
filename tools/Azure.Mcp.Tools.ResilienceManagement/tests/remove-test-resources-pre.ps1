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
$resilienceApiVersion = '2026-10-01'

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

function Get-OptionalOutputValue {
    param([string] $Name)

    $property = $outputs.PSObject.Properties[$Name.ToUpperInvariant()]
    return $property ? $property.Value : $null
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
    [CmdletBinding(SupportsShouldProcess)]
    param([string] $ResourceId, [string] $ApiVersion)

    if ([string]::IsNullOrWhiteSpace($ResourceId)) {
        return
    }

    if (!$PSCmdlet.ShouldProcess($ResourceId, 'Delete resource')) {
        return
    }

    $path = "${ResourceId}?api-version=$ApiVersion"
    Write-Information "DELETE $path"
    $response = Invoke-ManagementRestMethod -Method DELETE -Path $path
    if ($response.StatusCode -notin @(200, 202, 204, 404)) {
        throw "DELETE $path failed with status $($response.StatusCode): $($response.Content)"
    }

    if ($response.StatusCode -ne 404) {
        Wait-ResourceDeleted -ResourceId $ResourceId -ApiVersion $ApiVersion
    }
}

function Remove-RecoveryPlanCollection {
    [CmdletBinding(SupportsShouldProcess)]
    param([string] $ServiceGroupId)

    if (!$PSCmdlet.ShouldProcess($ServiceGroupId, 'Delete recovery plans')) {
        return
    }

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
        Remove-Resource -ResourceId $recoveryPlan.id -ApiVersion $resilienceApiVersion -Confirm:$false
    }
}

function Remove-ResourceCollection {
    [CmdletBinding(SupportsShouldProcess)]
    param(
        [string] $CollectionId,
        [string] $ApiVersion
    )

    if (!$PSCmdlet.ShouldProcess($CollectionId, 'Delete resources in collection')) {
        return
    }

    $collectionPath = "${CollectionId}?api-version=$ApiVersion"
    $response = Invoke-ManagementRestMethod -Method GET -Path $collectionPath
    if ($response.StatusCode -eq 404) {
        return
    }

    if ($response.StatusCode -ne 200) {
        throw "GET $collectionPath failed with status $($response.StatusCode): $($response.Content)"
    }

    $content = $response.Content | ConvertFrom-Json
    foreach ($resource in $content.value) {
        Remove-Resource -ResourceId $resource.id -ApiVersion $ApiVersion -Confirm:$false
    }
}

function Remove-PrincipalRoleAssignment {
    [CmdletBinding(SupportsShouldProcess)]
    param(
        [string] $Scope,
        [string] $PrincipalId
    )

    if ([string]::IsNullOrWhiteSpace($PrincipalId)) {
        return
    }

    if (!$PSCmdlet.ShouldProcess($Scope, "Delete role assignments for principal $PrincipalId")) {
        return
    }

    $apiVersion = '2022-04-01'
    $collectionPath = "$Scope/providers/Microsoft.Authorization/roleAssignments?api-version=$apiVersion&`$filter=atScope()"
    $response = Invoke-ManagementRestMethod -Method GET -Path $collectionPath
    if ($response.StatusCode -eq 404) {
        return
    }

    if ($response.StatusCode -ne 200) {
        throw "GET $collectionPath failed with status $($response.StatusCode): $($response.Content)"
    }

    $content = $response.Content | ConvertFrom-Json
    foreach ($assignment in $content.value) {
        if ($assignment.properties.principalId -eq $PrincipalId) {
            Remove-Resource -ResourceId $assignment.id -ApiVersion $apiVersion -Confirm:$false
        }
    }
}

function Remove-DrillsFixture {
    [CmdletBinding(SupportsShouldProcess)]
    param()

    $drillsResourceGroupName = Get-OptionalOutputValue -Name 'drillsResourceGroupName'
    if ([string]::IsNullOrWhiteSpace($drillsResourceGroupName) -or
        !$PSCmdlet.ShouldProcess($drillsResourceGroupName, 'Delete drills fixture resources')) {
        return
    }

    $serviceGroupName = Get-OutputValue -Name 'drillsServiceGroupName'
    $lifecycleServiceGroupName = Get-OutputValue -Name 'drillsLifecycleServiceGroupName'
    $usagePlanId = Get-OutputValue -Name 'drillsUsagePlanId'
    $drillName = Get-OutputValue -Name 'drillsDrillName'
    $deleteAssetResourceGroupName = Get-OutputValue -Name 'drillsDeleteAssetResourceGroupName'
    $requiredOutputs = @{
        drillsServiceGroupName             = $serviceGroupName
        drillsLifecycleServiceGroupName    = $lifecycleServiceGroupName
        drillsUsagePlanId                  = $usagePlanId
        drillsDrillName                    = $drillName
        drillsDeleteAssetResourceGroupName = $deleteAssetResourceGroupName
    }
    foreach ($requiredOutput in $requiredOutputs.GetEnumerator()) {
        if ([string]::IsNullOrWhiteSpace($requiredOutput.Value)) {
            throw "Deployment output '$($requiredOutput.Key)' is required for safe drills fixture teardown."
        }
    }

    $serviceGroupId = "/providers/Microsoft.Management/serviceGroups/$serviceGroupName"
    $lifecycleServiceGroupId = "/providers/Microsoft.Management/serviceGroups/$lifecycleServiceGroupName"
    $resilienceBase = "$serviceGroupId/providers/Microsoft.AzureResilienceManagement"
    $lifecycleResilienceBase = "$lifecycleServiceGroupId/providers/Microsoft.AzureResilienceManagement"
    $drillsResourceGroupId = "/subscriptions/$subscriptionId/resourceGroups/$drillsResourceGroupName"
    $deleteAssetResourceGroupId = "/subscriptions/$subscriptionId/resourceGroups/$deleteAssetResourceGroupName"
    $drillId = "$resilienceBase/drills/$drillName"

    $drillResponse = Invoke-ManagementRestMethod -Method GET -Path "$drillId`?api-version=$resilienceApiVersion"
    if ($drillResponse.StatusCode -eq 200) {
        $drillPrincipalId = ($drillResponse.Content | ConvertFrom-Json).identity.principalId
        Remove-PrincipalRoleAssignment -Scope $serviceGroupId -PrincipalId $drillPrincipalId -Confirm:$false
        Remove-PrincipalRoleAssignment -Scope "/subscriptions/$subscriptionId" -PrincipalId $drillPrincipalId -Confirm:$false
    }
    elseif ($drillResponse.StatusCode -ne 404) {
        throw "GET $drillId failed with status $($drillResponse.StatusCode): $($drillResponse.Content)"
    }

    Remove-ResourceCollection -CollectionId "$resilienceBase/drills" -ApiVersion $resilienceApiVersion -Confirm:$false
    Remove-ResourceCollection -CollectionId "$lifecycleResilienceBase/drills" -ApiVersion $resilienceApiVersion -Confirm:$false
    Remove-RecoveryPlanCollection -ServiceGroupId $serviceGroupId -Confirm:$false
    Remove-RecoveryPlanCollection -ServiceGroupId $lifecycleServiceGroupId -Confirm:$false
    Remove-Resource -ResourceId "$usagePlanId/enrollments/$serviceGroupName" -ApiVersion $resilienceApiVersion
    Remove-Resource -ResourceId "$usagePlanId/enrollments/$lifecycleServiceGroupName" -ApiVersion $resilienceApiVersion
    Remove-Resource -ResourceId "$drillsResourceGroupId/providers/Microsoft.Relationships/serviceGroupMember/drills-ga" -ApiVersion $membershipApiVersion
    Remove-Resource -ResourceId "$deleteAssetResourceGroupId/providers/Microsoft.Relationships/serviceGroupMember/delete-fixture" -ApiVersion $membershipApiVersion
    Remove-Resource -ResourceId $lifecycleServiceGroupId -ApiVersion $serviceGroupApiVersion
    Remove-Resource -ResourceId $serviceGroupId -ApiVersion $serviceGroupApiVersion
}

# Drills teardown can be blocked by service-side drill state; still remove the general fixture and
# start deleting the dedicated drills resource groups so billable resources do not leak.
try {
    Remove-DrillsFixture -Confirm:$false
}
catch {
    Write-Warning "Drills fixture teardown did not complete: $($_.Exception.Message)"
}
$drillsResourceGroupName = Get-OptionalOutputValue -Name 'drillsResourceGroupName'
$drillsDeleteAssetResourceGroupName = Get-OptionalOutputValue -Name 'drillsDeleteAssetResourceGroupName'
foreach ($groupName in @($drillsDeleteAssetResourceGroupName, $drillsResourceGroupName)) {
    if ([string]::IsNullOrWhiteSpace($groupName)) {
        continue
    }
    $path = "/subscriptions/$subscriptionId/resourceGroups/$groupName`?api-version=2024-03-01"
    $groupResponse = Invoke-ManagementRestMethod -Method GET -Path $path
    if ($groupResponse.StatusCode -eq 404) {
        continue
    }
    if ($groupResponse.StatusCode -ne 200) {
        Write-Warning "Skipping '$groupName': GET failed with status $($groupResponse.StatusCode), so fixture ownership cannot be verified."
        continue
    }
    # Setup refuses untagged groups; teardown must not delete a group it cannot prove it owns.
    $tags = ($groupResponse.Content | ConvertFrom-Json).tags
    $purpose = $tags ? ($tags.PSObject.Properties | Where-Object { $_.Name -ieq 'Purpose' } | Select-Object -First 1).Value : $null
    if ($purpose -ne 'mcp-drills-ga-live-tests') {
        Write-Warning "Skipping '$groupName': it is not tagged Purpose=mcp-drills-ga-live-tests."
        continue
    }

    Write-Information "DELETE $path"
    $response = Invoke-ManagementRestMethod -Method DELETE -Path $path
    if ($response.StatusCode -notin @(200, 202, 204, 404)) {
        Write-Warning "DELETE $path failed with status $($response.StatusCode): $($response.Content)"
    }
}

$serviceGroupName = Get-OutputValue -Name 'serviceGroupName'
$lifecycleServiceGroupName = Get-OutputValue -Name 'lifecycleServiceGroupName'
$planLifecycleServiceGroupName = Get-OutputValue -Name 'planLifecycleServiceGroupName'
$workflowServiceGroupName = Get-OutputValue -Name 'workflowServiceGroupName'
$goalsServiceGroupName = Get-OptionalOutputValue -Name 'goalsServiceGroupName'
$storageAccountId = Get-OptionalOutputValue -Name 'storageAccountId'
$usagePlanName = Get-OutputValue -Name 'usagePlanName'
$enrollmentName = Get-OutputValue -Name 'enrollmentName'
$lifecycleEnrollmentName = Get-OutputValue -Name 'lifecycleEnrollmentName'
$planLifecycleEnrollmentName = Get-OutputValue -Name 'planLifecycleEnrollmentName'
$workflowEnrollmentName = Get-OutputValue -Name 'workflowEnrollmentName'
$goalAssignmentName = Get-OutputValue -Name 'goalAssignmentName'

$requiredOutputs = @{
    serviceGroupName          = $serviceGroupName
    lifecycleServiceGroupName = $lifecycleServiceGroupName
    usagePlanName             = $usagePlanName
    enrollmentName            = $enrollmentName
    lifecycleEnrollmentName   = $lifecycleEnrollmentName
    goalAssignmentName        = $goalAssignmentName
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
$goalsServiceGroupId = "/providers/Microsoft.Management/serviceGroups/$goalsServiceGroupName"
$resilienceBase = "$serviceGroupId/providers/Microsoft.AzureResilienceManagement"
$usagePlanId = "/subscriptions/$subscriptionId/resourceGroups/$ResourceGroupName/providers/Microsoft.AzureResilienceManagement/usagePlans/$usagePlanName"

# Deployments created before drills moved to the dedicated fixture may still hold general drills.
Remove-ResourceCollection -CollectionId "$resilienceBase/drills" -ApiVersion $resilienceApiVersion -Confirm:$false
Remove-ResourceCollection -CollectionId "$lifecycleServiceGroupId/providers/Microsoft.AzureResilienceManagement/drills" -ApiVersion $resilienceApiVersion -Confirm:$false
Remove-RecoveryPlanCollection -ServiceGroupId $serviceGroupId -Confirm:$false
Remove-RecoveryPlanCollection -ServiceGroupId $lifecycleServiceGroupId -Confirm:$false
if (![string]::IsNullOrWhiteSpace($planLifecycleServiceGroupName)) {
    Remove-RecoveryPlanCollection -ServiceGroupId $planLifecycleServiceGroupId -Confirm:$false
}
if (![string]::IsNullOrWhiteSpace($workflowServiceGroupName)) {
    Remove-RecoveryPlanCollection -ServiceGroupId $workflowServiceGroupId -Confirm:$false
}
Remove-Resource -ResourceId "$resilienceBase/goalAssignments/$goalAssignmentName" -ApiVersion $resilienceApiVersion
if (![string]::IsNullOrWhiteSpace($goalsServiceGroupName)) {
    # Interrupted goal lifecycle tests can leave their assignment behind.
    Remove-ResourceCollection -CollectionId "$goalsServiceGroupId/providers/Microsoft.AzureResilienceManagement/goalAssignments" -ApiVersion $resilienceApiVersion -Confirm:$false
    if (![string]::IsNullOrWhiteSpace($storageAccountId)) {
        Remove-Resource -ResourceId "$storageAccountId/providers/Microsoft.Relationships/serviceGroupMember/rhub-goals-member" -ApiVersion $membershipApiVersion
    }
}
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

foreach ($serviceGroupIdToDelete in @($goalsServiceGroupName ? $goalsServiceGroupId : $null, $workflowServiceGroupName ? $workflowServiceGroupId : $null, $planLifecycleServiceGroupName ? $planLifecycleServiceGroupId : $null, $lifecycleServiceGroupId, $serviceGroupId)) {
    if ($serviceGroupIdToDelete -notmatch '/$') {
        Remove-Resource -ResourceId $serviceGroupIdToDelete -ApiVersion $serviceGroupApiVersion
    }
}
