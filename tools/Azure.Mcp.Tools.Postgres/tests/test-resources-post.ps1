
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

function Resolve-DeploymentValue {
    param(
        [string] $Key,
        [string] $Fallback
    )

    if ($DeploymentOutputs -and $DeploymentOutputs.ContainsKey($Key) -and -not [string]::IsNullOrWhiteSpace($DeploymentOutputs[$Key])) {
        return $DeploymentOutputs[$Key]
    }

    return $Fallback
}

$serverName = Resolve-DeploymentValue -Key 'POSTGRES_SERVER_NAME' -Fallback "$($testSettings.ResourceBaseName)-postgres"
$serverFqdn = Resolve-DeploymentValue -Key 'POSTGRES_SERVER_FQDN' -Fallback "$serverName.postgres.database.azure.com"
$databaseName = Resolve-DeploymentValue -Key 'POSTGRES_DATABASE_NAME' -Fallback 'sampledb'
$tableName = Resolve-DeploymentValue -Key 'POSTGRES_TABLE_NAME' -Fallback 'inventory'

if ($DeploymentOutputs -and $DeploymentOutputs.ContainsKey('POSTGRES_AAD_PRINCIPAL') -and -not [string]::IsNullOrWhiteSpace($DeploymentOutputs['POSTGRES_AAD_PRINCIPAL'])) {
    $userPrincipal = $DeploymentOutputs['POSTGRES_AAD_PRINCIPAL']
} elseif ($TestApplicationId) {
    $userPrincipal = $TestApplicationId
} else {
    $userPrincipal = (Get-AzContext).Account.Id
}

Write-Host "Configuring PostgreSQL dataset on $serverFqdn/$databaseName as '$userPrincipal'" -ForegroundColor Yellow

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

function Load-NpgsqlDependencies {
    $loggingAssemblyPath = Get-NuGetAssemblyPath -PackageName 'Microsoft.Extensions.Logging.Abstractions' -AssemblyFileName 'Microsoft.Extensions.Logging.Abstractions.dll'
    if (-not $loggingAssemblyPath) {
        throw "Unable to locate Microsoft.Extensions.Logging.Abstractions.dll in the NuGet package cache. Run 'dotnet restore' to populate dependencies before invoking this script."
    }
    [System.Reflection.Assembly]::LoadFrom($loggingAssemblyPath) | Out-Null

    $npgsqlAssemblyPath = Get-NuGetAssemblyPath -PackageName 'Npgsql' -AssemblyFileName 'Npgsql.dll'
    if (-not $npgsqlAssemblyPath) {
        throw "Unable to locate Npgsql.dll in the NuGet package cache. Run 'dotnet restore' to populate dependencies before invoking this script."
    }
    [System.Reflection.Assembly]::LoadFrom($npgsqlAssemblyPath) | Out-Null
}

function Wait-ForPostgresReady {
    param(
        [string] $ResourceGroup,
        [string] $ServerName,
        [int] $TimeoutSeconds = 300,
        [int] $PollingIntervalSeconds = 15
    )

    $elapsed = 0
    do {
        $server = Get-AzPostgreSqlFlexibleServer -ResourceGroupName $ResourceGroup -Name $ServerName -ErrorAction Stop
        Write-Host "  Server state: $($server.State)" -ForegroundColor Gray
        if ($server.State -eq 'Ready') {
            return
        }

        Start-Sleep -Seconds $PollingIntervalSeconds
        $elapsed += $PollingIntervalSeconds
    } while ($elapsed -lt $TimeoutSeconds)

    Write-Warning "Timeout waiting for PostgreSQL server '$ServerName' to reach Ready state."
}

function New-PostgresConnection {
    param(
        [string] $Host,
        [string] $Database,
        [string] $Username,
        [string] $AccessToken
    )

    $builder = [Npgsql.NpgsqlConnectionStringBuilder]::new()
    $builder.Host = $Host
    $builder.Database = $Database
    $builder.Username = $Username
    $builder.Password = $AccessToken
    $builder.SslMode = [Npgsql.SslMode]::Require
    $builder.TrustServerCertificate = $true
    $builder.Timeout = 30
    $builder.CommandTimeout = 30
    $builder.KeepAlive = 15
    return [Npgsql.NpgsqlConnection]::new($builder.ConnectionString)
}

function Invoke-PostgresNonQuery {
    param(
        [string] $Host,
        [string] $Database,
        [string] $Username,
        [string] $AccessToken,
        [string[]] $Statements
    )

    if (-not $Statements -or $Statements.Count -eq 0) {
        return
    }

    $connection = New-PostgresConnection -Host $Host -Database $Database -Username $Username -AccessToken $AccessToken
    try {
        $connection.Open()
        foreach ($statement in $Statements) {
            $command = $connection.CreateCommand()
            try {
                $command.CommandText = $statement
                $null = $command.ExecuteNonQuery()
            }
            finally {
                $command.Dispose()
            }
        }
    }
    finally {
        $connection.Dispose()
    }
}

try {
    $postgresServer = Get-AzPostgreSqlFlexibleServer -ResourceGroupName $ResourceGroupName -Name $serverName -ErrorAction Stop
    Write-Host "PostgreSQL Server '$($postgresServer.Name)' located in $($postgresServer.Location)" -ForegroundColor Green
    Write-Host "  FQDN: $serverFqdn" -ForegroundColor Gray
    Write-Host "  Version: $($postgresServer.Version)" -ForegroundColor Gray
    Write-Host "  State: $($postgresServer.State)" -ForegroundColor Gray

    Write-Host "Waiting for PostgreSQL server to become ready..." -ForegroundColor Yellow
    Wait-ForPostgresReady -ResourceGroup $ResourceGroupName -ServerName $serverName

    Load-NpgsqlDependencies

    $accessToken = (Get-AzAccessToken -ResourceUrl 'https://ossrdbms-aad.database.windows.net').Token
    if (-not $accessToken) {
        throw "Failed to acquire access token for PostgreSQL."
    }

    Write-Host "Ensuring database '$databaseName' exists." -ForegroundColor Yellow
    try {
        $createDatabaseStatement = [string]::Format('CREATE DATABASE "{0}";', $databaseName)
        Invoke-PostgresNonQuery -Host $serverFqdn -Database 'postgres' -Username $userPrincipal -AccessToken $accessToken -Statements @(
            $createDatabaseStatement
        )
        Write-Host "Database '$databaseName' created." -ForegroundColor Green
    }
    catch {
        if ($_.Exception.InnerException -and $_.Exception.InnerException.Message -match 'already exists') {
            Write-Host "Database '$databaseName' already exists." -ForegroundColor Gray
        }
        elseif ($_.Exception.Message -match 'already exists') {
            Write-Host "Database '$databaseName' already exists." -ForegroundColor Gray
        }
        else {
            throw
        }
    }

    Write-Host "Seeding table '$tableName' in database '$databaseName'." -ForegroundColor Yellow
    $qualifiedTableName = [string]::Format('public."{0}"', $tableName)
    Invoke-PostgresNonQuery -Host $serverFqdn -Database $databaseName -Username $userPrincipal -AccessToken $accessToken -Statements @(
        "CREATE TABLE IF NOT EXISTS $qualifiedTableName (id SERIAL PRIMARY KEY, name TEXT NOT NULL, quantity INTEGER NOT NULL, last_updated TIMESTAMPTZ DEFAULT CURRENT_TIMESTAMP);",
        "TRUNCATE TABLE $qualifiedTableName;",
        "INSERT INTO $qualifiedTableName (name, quantity) VALUES ('alpha', 5), ('beta', 12), ('gamma', 27);"
    )

    Write-Host "PostgreSQL test resources setup completed successfully!" -ForegroundColor Green
}
catch {
    Write-Error "Error configuring PostgreSQL resources: $($_.Exception.Message)"
    throw
}
