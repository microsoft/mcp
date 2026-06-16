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

function Get-DeploymentOutputValue {
    param(
        [hashtable] $Outputs,
        [string] $Name
    )

    $output = $Outputs[$Name]

    if ($null -eq $output) {
        return $null
    }

    if ($output -is [hashtable] -and $output.ContainsKey('value')) {
        return [string] $output['value']
    }

    if ($output.PSObject.Properties['value']) {
        return [string] $output.value
    }

    return [string] $output
}

$cleanroomEndpoint = Get-DeploymentOutputValue -Outputs $DeploymentOutputs -Name 'CLEANROOM_ENDPOINT'

if ([string]::IsNullOrWhiteSpace($cleanroomEndpoint)) {
    Write-Warning "CLEANROOM_ENDPOINT was not set. Live tests will be skipped until a Cleanroom Analytics Frontend endpoint is provisioned and provided."
} else {
    Write-Host "Cleanroom Analytics Frontend endpoint: $cleanroomEndpoint" -ForegroundColor Gray
}

Write-Host "Managed Cleanroom test settings saved to: $PSScriptRoot\.testsettings.json" -ForegroundColor Green
