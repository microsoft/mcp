# cSpell:ignore nologo

function Get-OperatingSystems {
    return @(
        @{ name = 'linux'; nodeName = 'linux'; dotnetName = 'linux'; extension = '' }
        @{ name = 'macos'; nodeName = 'darwin'; dotnetName = 'osx'; extension = '' }
        @{ name = 'windows'; nodeName = 'win32'; dotnetName = 'win'; extension = '.exe' }
    )
}

function Get-RepoRelativePath {
    [CmdletBinding()]
    param(
        [parameter(Mandatory, ValueFromPipeline)]
        [string] $Path,
        [switch] $NormalizeSeparators,
        [string] $RepositoryRoot = (Resolve-Path (Join-Path $PSScriptRoot '..' '..' '..')).Path
    )

    process {
        $relativePath = Resolve-Path -LiteralPath $Path -Relative -RelativeBasePath $RepositoryRoot

        # trim the leading ./
        if ($relativePath.StartsWith('./') -or $relativePath.StartsWith('.\')) {
            $relativePath = $relativePath.Substring(2)
        }

        $NormalizeSeparators ? $relativePath.Replace('\', '/') : $relativePath
    }
}

function Get-CanonicalProjectPath {
    param([string] $Path)

    $normalizedPath = $Path.Replace('\', '/')
    if ($normalizedPath -match '^(tools|servers|core)/[^/]+') {
        return $Matches[0]
    }

    return $null
}

function Select-ServersForChangedProjectPaths {
    param(
        [object[]] $Servers,
        [string[]] $ChangedProjectPaths,
        [hashtable] $DependencyProjectPathsByServer
    )

    return @($Servers | Where-Object {
            $serverName = $_.name
            $serverDependencyPaths = @($DependencyProjectPathsByServer[$serverName])
            @($ChangedProjectPaths | Where-Object { $serverDependencyPaths -contains $_ }).Count -gt 0
        } | ForEach-Object { $_.name })
}

function Get-ServersToBuild {
    param(
        [object[]] $Servers,
        [string[]] $ChangedFiles,
        [switch] $IsPullRequest,
        [string] $RepositoryRoot = (Resolve-Path (Join-Path $PSScriptRoot '..' '..' '..')).Path
    )

    if (!$IsPullRequest) {
        return @($Servers | ForEach-Object { $_.name })
    }

    $changedProjectPaths = @()
    foreach ($changedFile in $ChangedFiles) {
        $projectPath = Get-CanonicalProjectPath $changedFile
        if (!$projectPath) {
            Write-Host "Change outside a project root detected at '$changedFile'. Building all servers." -ForegroundColor Yellow
            return @($Servers | ForEach-Object { $_.name })
        }

        $changedProjectPaths += $projectPath
    }

    $changedProjectPaths = @($changedProjectPaths | Sort-Object -Unique)
    if ($changedProjectPaths.Count -eq 0) {
        Write-Host "No changed project paths detected. Building all servers." -ForegroundColor Yellow
        return @($Servers | ForEach-Object { $_.name })
    }

    $missingProjectPaths = @($changedProjectPaths | Where-Object {
        -not (Test-Path (Join-Path $RepositoryRoot $_) -PathType Container)
    })
    if ($missingProjectPaths.Count -gt 0) {
        Write-Host "Changed project roots no longer exist: $($missingProjectPaths -join ', '). Building all servers." -ForegroundColor Yellow
        return @($Servers | ForEach-Object { $_.name })
    }

    $dependencyProjectPathsByServer = @{}
    foreach ($server in $Servers) {
        $graphPath = Join-Path ([IO.Path]::GetTempPath()) "mcp-$($server.name)-$([guid]::NewGuid().ToString('N')).json"
        $escapedServerPath = $server.path.Replace("'", "''")
        $escapedGraphPath = $graphPath.Replace("'", "''")
        try {
            Invoke-LoggedMsBuildCommand "dotnet msbuild '$escapedServerPath' /t:GenerateRestoreGraphFile /p:RestoreGraphOutputPath='$escapedGraphPath' /nologo" -GroupOutput | Out-Host
            $dependencyGraph = Get-Content $graphPath -Raw | ConvertFrom-Json -AsHashtable
            $dependencyProjectPathsByServer[$server.name] = @($dependencyGraph.projects.Keys |
                Get-RepoRelativePath -NormalizeSeparators -RepositoryRoot $RepositoryRoot |
                ForEach-Object { Get-CanonicalProjectPath $_ } |
                Where-Object { $_ } |
                Sort-Object -Unique)
        }
        finally {
            Remove-Item $graphPath -Force -ErrorAction SilentlyContinue
        }
    }

    $serversToBuild = @(Select-ServersForChangedProjectPaths $Servers $changedProjectPaths $dependencyProjectPathsByServer)
    Write-Host "Servers affected by changed project dependencies: $($serversToBuild -join ', ')"
    return $serversToBuild
}
