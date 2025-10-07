param(
    [string] $TenantId,
    [string] $TestApplicationId,
    [string] $ResourceGroupName,
    [string] $BaseName,
    [hashtable] $DeploymentOutputs
)

$ErrorActionPreference = "Stop"

. "$PSScriptRoot/../../../eng/common/scripts/common.ps1"
. "$PSScriptRoot/../../../eng/scripts/helpers/TestResourcesHelpers.ps1"

$testSettings = New-TestSettings @PSBoundParameters -OutputPath $PSScriptRoot

$serverName = $DeploymentOutputs['POSTGRES_SERVER_NAME']
if (-not $serverName) {
    $serverName = $testSettings.ResourceBaseName
}

$serverFqdn = $DeploymentOutputs['POSTGRES_SERVER_FQDN']
if (-not $serverFqdn) {
    $serverFqdn = "$serverName.postgres.database.azure.com"
}

$databaseName = $DeploymentOutputs['POSTGRES_DATABASE_NAME']
if (-not $databaseName) {
    $databaseName = 'sampledb'
}

$tableName = $DeploymentOutputs['POSTGRES_TABLE_NAME']
if (-not $tableName) {
    $tableName = 'inventory'
}

$userPrincipal = if ($DeploymentOutputs.ContainsKey('POSTGRES_AAD_PRINCIPAL') -and $DeploymentOutputs['POSTGRES_AAD_PRINCIPAL']) {
    $DeploymentOutputs['POSTGRES_AAD_PRINCIPAL']
} elseif ($TestApplicationId) {
    $TestApplicationId
} else {
    (Get-AzContext).Account.Id
}

Write-Host "Configuring PostgreSQL dataset on $serverFqdn/$databaseName as '$userPrincipal'" -ForegroundColor Yellow

$accessToken = (Get-AzAccessToken -ResourceUrl "https://ossrdbms-aad.database.windows.net").Token
if (-not $accessToken) {
    throw "Failed to acquire access token for PostgreSQL."
}

function Get-GlobalPackagesPath {
    if ($env:NUGET_PACKAGES) {
        return $env:NUGET_PACKAGES
    }

    $profilePath = $env:USERPROFILE
    if ([string]::IsNullOrWhiteSpace($profilePath)) {
        $profilePath = $env:HOME
    }

    if ([string]::IsNullOrWhiteSpace($profilePath)) {
        $profilePath = [Environment]::GetFolderPath([Environment+SpecialFolder]::UserProfile)
    }

    if ([string]::IsNullOrWhiteSpace($profilePath)) {
        return $null
    }

    return Join-Path $profilePath '.nuget/packages'
}

function Get-NuGetAssemblyPath {
    param(
        [Parameter(Mandatory = $true)][string] $PackageName,
        [string] $AssemblyFileName
    )

    $packagesRoot = Get-GlobalPackagesPath
    if ([string]::IsNullOrWhiteSpace($packagesRoot)) {
        return $null
    }

    if ([string]::IsNullOrWhiteSpace($AssemblyFileName)) {
        $AssemblyFileName = "${PackageName}.dll"
    }

    $packageFolder = Join-Path $packagesRoot ($PackageName.ToLowerInvariant())
    if (-not (Test-Path $packageFolder)) {
        return $null
    }

    $tfmPreference = @('net9.0', 'net8.0', 'net7.0', 'net6.0', 'net5.0', 'netcoreapp3.1', 'netstandard2.1', 'netstandard2.0')

    $versions = Get-ChildItem -Path $packageFolder -Directory | Sort-Object Name -Descending
    foreach ($version in $versions) {
        $libFolder = Join-Path $version.FullName 'lib'
        if (-not (Test-Path $libFolder)) {
            continue
        }

        foreach ($tfm in $tfmPreference) {
            $candidatePath = Join-Path $libFolder $tfm
            if (-not (Test-Path $candidatePath)) {
                continue
            }

            $assemblyCandidate = Join-Path $candidatePath $AssemblyFileName
            if (Test-Path $assemblyCandidate) {
                return $assemblyCandidate
            }
        }
    }

    return $null
}

function Get-NpgsqlAssemblyPath {
    return Get-NuGetAssemblyPath -PackageName 'Npgsql' -AssemblyFileName 'Npgsql.dll'
}

$loggingAssemblyPath = Get-NuGetAssemblyPath -PackageName 'Microsoft.Extensions.Logging.Abstractions' -AssemblyFileName 'Microsoft.Extensions.Logging.Abstractions.dll'
if (-not $loggingAssemblyPath) {
    throw "Unable to locate Microsoft.Extensions.Logging.Abstractions.dll in the NuGet package cache. Run 'dotnet restore' to populate dependencies before invoking this script."
}
[System.Reflection.Assembly]::LoadFrom($loggingAssemblyPath) | Out-Null

$assemblyPath = Get-NpgsqlAssemblyPath
if (-not $assemblyPath) {
    throw "Unable to locate Npgsql.dll in the NuGet package cache. Run 'dotnet restore' to populate dependencies before invoking this script."
}

[System.Reflection.Assembly]::LoadFrom($assemblyPath) | Out-Null

$connectionString = "Host=$serverFqdn;Database=$databaseName;Username=$userPrincipal;Password=$accessToken;Ssl Mode=Require;Trust Server Certificate=true;Timeout=30;Command Timeout=30;"

$connection = [Npgsql.NpgsqlConnection]::new($connectionString)
try {
    $connection.Open()

    $commands = @(
        "CREATE TABLE IF NOT EXISTS public.$tableName (id SERIAL PRIMARY KEY, name TEXT NOT NULL, quantity INTEGER NOT NULL, last_updated TIMESTAMPTZ DEFAULT CURRENT_TIMESTAMP);",
        "DELETE FROM public.$tableName;",
        "INSERT INTO public.$tableName (name, quantity) VALUES ('alpha', 5), ('beta', 12), ('gamma', 27);"
    )

    foreach ($commandText in $commands) {
        $command = $connection.CreateCommand()
        try {
            $command.CommandText = $commandText
            $null = $command.ExecuteNonQuery()
        } finally {
            $command.Dispose()
        }
    }

    Write-Host "Seeded table '$tableName' with sample data." -ForegroundColor Green
} catch {
    Write-Error "Failed to seed PostgreSQL data: $($_.Exception.Message)"
    throw
} finally {
    if ($connection) {
        $connection.Dispose()
    }
}
