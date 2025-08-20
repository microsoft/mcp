param(
    [string] $TenantId,
    [string] $TestApplicationId,
    [string] $ResourceGroupName,
    [string] $BaseName,
    [hashtable] $DeploymentOutputs
)

# cspell: words GRAFANAWORKSPACENAME

$ErrorActionPreference = "Stop"

. "$PSScriptRoot/../../../eng/common/scripts/common.ps1"
. "$PSScriptRoot/../../../eng/scripts/helpers/TestResourcesHelpers.ps1"

$testSettings = New-TestSettings @PSBoundParameters -OutputPath $PSScriptRoot

$grafanaWorkspaceName = $DeploymentOutputs.GRAFANAWORKSPACENAME

Write-Host "Getting Grafana workspace information: $grafanaWorkspaceName" -ForegroundColor Yellow

try {
    $grafanaWorkspace = Get-AzGrafana -ResourceGroupName $ResourceGroupName -Name $grafanaWorkspaceName

    if ($grafanaWorkspace) {
        Write-Host "Grafana workspace '$grafanaWorkspaceName' is ready for testing" -ForegroundColor Green
        Write-Host "  - Workspace ID: $($grafanaWorkspace.Id)" -ForegroundColor Gray
        Write-Host "  - Endpoint: $($grafanaWorkspace.Endpoint)" -ForegroundColor Gray
        Write-Host "  - Location: $($grafanaWorkspace.Location)" -ForegroundColor Gray
    }
    else {
        Write-Warning "Grafana workspace '$grafanaWorkspaceName' not found"
    }
}
catch {
    Write-Warning "Failed to retrieve Grafana workspace information: $($_.Exception.Message)"
}
