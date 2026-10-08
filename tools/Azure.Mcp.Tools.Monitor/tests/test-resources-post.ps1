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

# Add static deployment outputs
$staticDeploymentOutputs = @{
    "staticStorageAccountName" = "azuresdktrainingdatatme"
    "staticResourceGroup" = "static-test-resources"
    "staticWorkspace" = "monitor-query-ws"
}

if (!$DeploymentOutputs) {
    $DeploymentOutputs = @{}
}

foreach ($key in $staticDeploymentOutputs.Keys) {
    $DeploymentOutputs[$key] = $staticDeploymentOutputs[$key]
}

$PSBoundParameters['DeploymentOutputs'] = $DeploymentOutputs
New-TestSettings @PSBoundParameters -OutputPath $PSScriptRoot | Out-Null

$storageAccountName = "$($BaseName)mon"
$containerName = 'foo'
$context = New-AzStorageContext -StorageAccountName $storageAccountName -UseConnectedAccount
Write-Host "Uploading sample files to blob storage: $storageAccountName/$containerName" -ForegroundColor Yellow
$files = Get-ChildItem -Path "$PSScriptRoot/samples" -Filter '*.md'
foreach ($file in $files) {
    Set-AzStorageBlobContent -File $file.FullName -Container $containerName -Blob $file.Name -Context $context -Force -ProgressAction SilentlyContinue | Out-Null
}

function Get-PlainTextAccessToken {
    param([Parameter(Mandatory)] [string] $ResourceUrl)

    $token = (Get-AzAccessToken -ResourceUrl $ResourceUrl).Token
    if ($token -is [System.Security.SecureString]) {
        return [System.Net.NetworkCredential]::new('', $token).Password
    }

    return $token
}

function Send-LogSearchFixture {
    param(
        [Parameter(Mandatory)] [string] $Endpoint,
        [Parameter(Mandatory)] [string] $DcrImmutableId,
        [Parameter(Mandatory)] [string] $Stream,
        [Parameter(Mandatory)] [object[]] $Records,
        [Parameter(Mandatory)] [string] $AccessToken
    )

    $uri = "$($Endpoint.TrimEnd('/'))/dataCollectionRules/$DcrImmutableId/streams/$Stream`?api-version=2023-01-01"
    $headers = @{ Authorization = "Bearer $AccessToken" }
    $payload = ConvertTo-Json -InputObject $Records -Depth 10 -Compress

    for ($attempt = 1; $attempt -le 12; $attempt++) {
        try {
            Invoke-RestMethod -Method Post -Uri $uri -Headers $headers -ContentType 'application/json' -Body $payload | Out-Null
            return
        } catch {
            if ($_.Exception -isnot [Microsoft.PowerShell.Commands.HttpResponseException] -or $null -eq $_.Exception.Response) {
                throw
            }

            $statusCode = [int]$_.Exception.Response.StatusCode
            $isTransient = $statusCode -eq 403 -or $statusCode -eq 429 -or $statusCode -ge 500
            if (!$isTransient -or $attempt -eq 12) {
                throw
            }

            Write-Host "Fixture ingestion is not ready yet (HTTP $statusCode); retrying." -ForegroundColor DarkYellow
            Start-Sleep -Seconds ([Math]::Min(30, $attempt * 5))
        }
    }
}

function Test-LogSearchFixtureAvailable {
    param(
        [Parameter(Mandatory)] [string] $WorkspaceCustomerId,
        [Parameter(Mandatory)] [string] $Table,
        [Parameter(Mandatory)] [string] $FixtureId,
        [Parameter(Mandatory)] [datetime] $StartTime,
        [Parameter(Mandatory)] [string] $AccessToken
    )

    $endTime = [DateTime]::UtcNow.AddMinutes(1)
    $timespan = [Uri]::EscapeDataString("$($StartTime.ToString('o'))/$($endTime.ToString('o'))")
    $uri = "https://api.loganalytics.io/v1/workspaces/$WorkspaceCustomerId/search?timespan=$timespan"
    $headers = @{
        Authorization = "Bearer $AccessToken"
        Prefer = 'wait=10'
    }
    $payload = ConvertTo-Json -Compress -InputObject @{
        query = "$Table | where FixtureId == '$FixtureId' | take 1"
    }

    try {
        $response = Invoke-RestMethod -Method Post -Uri $uri -Headers $headers -ContentType 'application/json' -Body $payload
        return $response.tables -and $response.tables[0].rows.Count -gt 0
    } catch {
        Write-Host "Log fixture for $Table is not queryable yet: $($_.Exception.Message)" -ForegroundColor DarkYellow
        return $false
    }
}

$fixtureTimestamp = [DateTime]::UtcNow.AddSeconds(1)
$fixtureTimestampText = $fixtureTimestamp.ToString('yyyy-MM-ddTHH:mm:ss.ffffffZ')
$fixtureStartTime = $fixtureTimestamp.ToString('o')
$fixtureEndTime = $fixtureTimestamp.AddMinutes(30).ToString('o')
$basicFixtureId = 'mcp-log-search-basic-1'
$auxiliaryFixtureId = 'mcp-log-search-auxiliary-1'
$basicTableName = $DeploymentOutputs['logSearchBasicTableName']
$auxiliaryTableName = $DeploymentOutputs['logSearchAuxiliaryTableName']
$ingestionEndpoint = $DeploymentOutputs['logSearchIngestionEndpoint']
$dcrImmutableId = $DeploymentOutputs['logSearchDcrImmutableId']
$workspaceCustomerId = $DeploymentOutputs['logSearchWorkspaceCustomerId']

$ingestionToken = Get-PlainTextAccessToken -ResourceUrl 'https://monitor.azure.com'
$basicRecords = @(
    [ordered]@{
        TimeGenerated = $fixtureTimestampText
        FixtureId = $basicFixtureId
        Message = 'basic fixture'
        Count = 7
        Enabled = $true
        OptionalValue = $null
    }
    [ordered]@{
        TimeGenerated = $fixtureTimestamp.AddSeconds(1).ToString('yyyy-MM-ddTHH:mm:ss.ffffffZ')
        FixtureId = 'mcp-log-search-basic-2'
        Message = 'second basic fixture'
        Count = 8
        Enabled = $false
        OptionalValue = 'present'
    }
)
$auxiliaryRecords = @(
    [ordered]@{
        TimeGenerated = $fixtureTimestampText
        FixtureId = $auxiliaryFixtureId
        Message = 'auxiliary fixture'
        Count = 11
        Enabled = $false
        OptionalValue = $null
    }
    [ordered]@{
        TimeGenerated = $fixtureTimestamp.AddSeconds(1).ToString('yyyy-MM-ddTHH:mm:ss.ffffffZ')
        FixtureId = 'mcp-log-search-auxiliary-2'
        Message = 'second auxiliary fixture'
        Count = 12
        Enabled = $true
        OptionalValue = 'present'
    }
)

Write-Host 'Sending deterministic Basic and Auxiliary Log Analytics fixtures' -ForegroundColor Yellow
Send-LogSearchFixture `
    -Endpoint $ingestionEndpoint `
    -DcrImmutableId $dcrImmutableId `
    -Stream "Custom-$basicTableName" `
    -Records $basicRecords `
    -AccessToken $ingestionToken
Send-LogSearchFixture `
    -Endpoint $ingestionEndpoint `
    -DcrImmutableId $dcrImmutableId `
    -Stream "Custom-$auxiliaryTableName" `
    -Records $auxiliaryRecords `
    -AccessToken $ingestionToken

$queryToken = Get-PlainTextAccessToken -ResourceUrl 'https://api.loganalytics.io'
$pendingTables = @{
    $basicTableName = $basicFixtureId
    $auxiliaryTableName = $auxiliaryFixtureId
}

$fixtureDeadline = [DateTime]::UtcNow.AddMinutes(20)
while ($pendingTables.Count -gt 0 -and [DateTime]::UtcNow -lt $fixtureDeadline) {
    foreach ($tableName in @($pendingTables.Keys)) {
        if (Test-LogSearchFixtureAvailable `
            -WorkspaceCustomerId $workspaceCustomerId `
            -Table $tableName `
            -FixtureId $pendingTables[$tableName] `
            -StartTime $fixtureTimestamp `
            -AccessToken $queryToken) {
            Write-Host "Log fixture for $tableName is available." -ForegroundColor Green
            $pendingTables.Remove($tableName)
        }
    }

    if ($pendingTables.Count -gt 0) {
        Start-Sleep -Seconds 10
    }
}

if ($pendingTables.Count -gt 0) {
    throw "Timed out waiting for Log Analytics fixtures in: $($pendingTables.Keys -join ', ')"
}

$DeploymentOutputs['logSearchBasicFixtureId'] = $basicFixtureId
$DeploymentOutputs['logSearchAuxiliaryFixtureId'] = $auxiliaryFixtureId
$DeploymentOutputs['logSearchFixtureStartTime'] = $fixtureStartTime
$DeploymentOutputs['logSearchFixtureEndTime'] = $fixtureEndTime

New-TestSettings @PSBoundParameters -OutputPath $PSScriptRoot | Out-Null
