#!/bin/env pwsh
#Requires -Version 7

<#
.SYNOPSIS
    Packs Azure MCP Server binaries into an executable Maven JAR for distribution.

.DESCRIPTION
    This script creates a single "fat" executable JAR for the Azure MCP Server that
    bundles the native build output for every supported platform under
    native/<platform>/. At runtime the bundled Java launcher detects the host
    OS/architecture, extracts the matching platform folder, and spawns the azmcp
    executable from it.

    The JAR follows Maven naming conventions:
    - azure-mcp-<version>.jar           (executable fat JAR)
    - azure-mcp-<version>-sources.jar   (launcher sources)
    - azure-mcp-<version>-javadoc.jar   (minimal javadoc placeholder)

    A pom.xml is rendered alongside the JARs from eng/maven/pom.xml.template.

    Users can run the server with:
    - java -jar azure-mcp-<version>.jar server start

.PARAMETER ArtifactsPath
    Path to the build artifacts containing the server binaries.

.PARAMETER BuildInfoPath
    Path to the build_info.json file containing server and platform details.

.PARAMETER OutputPath
    Path where the Maven packages will be created.

.EXAMPLE
    ./Pack-Maven.ps1
    Creates the Maven JAR using default local paths.

.EXAMPLE
    ./Pack-Maven.ps1 -ArtifactsPath ".work/build" -BuildInfoPath ".work/build_info.json"
    Creates the Maven JAR using specified artifact and build info paths.
#>

[CmdletBinding()]
param(
    [string] $ArtifactsPath,
    [string] $BuildInfoPath,
    [string] $OutputPath
)

$ErrorActionPreference = "Stop"
. "$PSScriptRoot/../common/scripts/common.ps1"
$RepoRoot = $RepoRoot.Path.Replace('\', '/')

$mavenSourcePath = "$RepoRoot/eng/maven"

# When running locally, ignore missing artifacts instead of failing
$ignoreMissingArtifacts = $env:TF_BUILD -ne 'true'
$exitCode = 0

if (!$ArtifactsPath) {
    $ArtifactsPath = "$RepoRoot/.work/build"
}

if (!$BuildInfoPath) {
    $BuildInfoPath = "$RepoRoot/.work/build_info.json"
}

if (!$OutputPath) {
    $OutputPath = "$RepoRoot/.work/packages_maven"
    Remove-Item -Path $OutputPath -Recurse -Force -ErrorAction SilentlyContinue -ProgressAction SilentlyContinue
}

if (!(Test-Path $ArtifactsPath)) {
    LogError "Artifacts path $ArtifactsPath does not exist."
    $exitCode = 1
}

if (!(Test-Path $BuildInfoPath)) {
    LogError "Build info file $BuildInfoPath does not exist. Run eng/scripts/New-BuildInfo.ps1 to create it."
    $exitCode = 1
}

if ($exitCode -ne 0) {
    exit $exitCode
}

$buildInfo = Get-Content $BuildInfoPath -Raw | ConvertFrom-Json -AsHashtable

$tempFolder = "$RepoRoot/.work/temp_maven"

# Map build_info nodeOs-architecture to bundled Java platform keys.
# These keys mirror the os.name/os.arch detection in Launcher.java.
$mavenPlatformMap = @{
    'win32-x64'    = 'windows-x86_64'
    'win32-arm64'  = 'windows-aarch64'
    'darwin-x64'   = 'macos-x86_64'
    'darwin-arm64' = 'macos-aarch64'
    'linux-x64'    = 'linux-x86_64'
    'linux-arm64'  = 'linux-aarch64'
}

$mainClass = 'com.microsoft.mcp.azure.Launcher'

function Get-KeywordsString($keywords) {
    return ($keywords -join ', ')
}

function Get-JdkCommand([string] $name) {
    # Prefer JAVA_HOME/bin when available, otherwise rely on PATH.
    $javaHome = $env:JAVA_HOME
    if ($javaHome) {
        $candidate = Join-Path $javaHome "bin/$name"
        if ($IsWindows) { $candidate += '.exe' }
        if (Test-Path $candidate) {
            return $candidate
        }
    }

    $command = Get-Command $name -ErrorAction SilentlyContinue
    if ($command) {
        return $command.Source
    }

    throw "$name is not installed or not in PATH. Please install a JDK (Java 11+) and ensure '$name' is available (set JAVA_HOME or add it to PATH)."
}

function BuildServerPackage([hashtable] $server) {
    $serverDirectory = "$ArtifactsPath/$($server.artifactPath)"

    if (!(Test-Path $serverDirectory)) {
        $message = "Server directory $serverDirectory does not exist."
        if ($ignoreMissingArtifacts) {
            Write-Warning $message
        }
        else {
            Write-Error $message
        }
        return
    }

    # Skip servers without Maven configuration
    $groupId = if ([string]::IsNullOrWhiteSpace($server.mavenGroupId)) { $null } else { $server.mavenGroupId }
    $artifactId = if ([string]::IsNullOrWhiteSpace($server.mavenArtifactId)) { $null } else { $server.mavenArtifactId }
    if (!$groupId -or !$artifactId) {
        Write-Host "Skipping $($server.name) - no Maven coordinates configured" -ForegroundColor Yellow
        return
    }

    $filteredPlatforms = $server.platforms | Where-Object { -not $_.native -and -not $_.specialPurpose }
    if ($filteredPlatforms.Count -eq 0) {
        Write-Host "No platforms to bundle for server $($server.name)"
        return
    }

    $version = $server.version
    $description = if ([string]::IsNullOrWhiteSpace($server.mavenDescription)) { $server.description } else { $server.mavenDescription }
    $cliName = $server.cliName
    $keywords = @(if ($server.mavenPackageKeywords) { $server.mavenPackageKeywords } else { $server.npmPackageKeywords })

    $serverOutputPath = "$OutputPath/$($server.artifactPath)"
    New-Item -ItemType Directory -Force -Path $serverOutputPath | Out-Null

    # Clean temp folder
    Remove-Item -Path $tempFolder -Recurse -Force -ErrorAction SilentlyContinue -ProgressAction SilentlyContinue
    $classesFolder = "$tempFolder/classes"
    $sourcesFolder = "$tempFolder/sources"
    $nativeFolder = "$classesFolder/native"
    New-Item -ItemType Directory -Force -Path $classesFolder | Out-Null
    New-Item -ItemType Directory -Force -Path $nativeFolder | Out-Null

    Write-Host "`nBundling native binaries for $artifactId $version" -ForegroundColor Cyan

    $bundledPlatforms = @()

    foreach ($platform in $filteredPlatforms) {
        $platformKey = "$($platform.nodeOs)-$($platform.architecture)"
        $mavenPlatform = $mavenPlatformMap[$platformKey]

        if (!$mavenPlatform) {
            Write-Warning "  Unknown platform: $platformKey, skipping"
            continue
        }

        $platformDirectory = "$ArtifactsPath/$($platform.artifactPath)"
        if (!(Test-Path $platformDirectory)) {
            $errorMessage = "Platform directory $platformDirectory does not exist."
            if ($ignoreMissingArtifacts) {
                Write-Warning $errorMessage
                continue
            }

            Write-Error $errorMessage
            return
        }

        $binaryName = "$cliName$($platform.extension)"
        $sourceBinary = "$platformDirectory/$binaryName"
        if (!(Test-Path $sourceBinary)) {
            $errorMessage = "Binary $sourceBinary does not exist."
            if ($ignoreMissingArtifacts) {
                Write-Warning $errorMessage
                continue
            }

            Write-Error $errorMessage
            return
        }

        $destDirectory = "$nativeFolder/$mavenPlatform"
        New-Item -ItemType Directory -Force -Path $destDirectory | Out-Null
        $fileCount = (Get-ChildItem -Path $platformDirectory -Recurse -File).Count
        Write-Host "  Adding $mavenPlatform ($fileCount files)"
        # Copy the entire platform output, not just the launcher binary. The published
        # build is framework-dependent, so azmcp[.exe] is an apphost that requires its
        # companion azmcp.dll, runtimeconfig.json, and dependency assemblies alongside it.
        Copy-Item -Path "$platformDirectory/*" -Destination $destDirectory -Recurse -Force

        $bundledPlatforms += $mavenPlatform
    }

    if ($bundledPlatforms.Count -eq 0) {
        Write-Warning "No platform binaries were bundled for $($server.name); skipping JAR creation."
        return
    }

    # Remove symbol files before packing
    Write-Host "  Removing symbol files"
    Get-ChildItem -Path $nativeFolder -Recurse -Include "*.pdb", "*.dSYM", "*.dbg" | Remove-Item -Force -Recurse -ErrorAction SilentlyContinue -ProgressAction SilentlyContinue

    # Locate JDK tools
    $javac = Get-JdkCommand 'javac'
    $jar = Get-JdkCommand 'jar'

    # Copy launcher sources for compilation and the sources JAR
    Write-Host "  Copying launcher sources"
    Copy-Item -Path "$mavenSourcePath/src/main/java" -Destination "$sourcesFolder" -Recurse -Force
    $javaFiles = Get-ChildItem -Path "$sourcesFolder" -Recurse -Filter "*.java" | ForEach-Object { $_.FullName }

    # Compile the launcher
    Write-Host "  Compiling launcher (Java 11 target)" -ForegroundColor Green
    $quotedJavaFiles = ($javaFiles | ForEach-Object { '"' + $_ + '"' }) -join ' '
    Invoke-LoggedCommand "& `"$javac`" --release 11 -d `"$classesFolder`" $quotedJavaFiles"

    # Add license/notice to the JAR metadata
    Copy-Item -Path "$RepoRoot/LICENSE" -Destination "$classesFolder/LICENSE" -Force
    Copy-Item -Path "$RepoRoot/NOTICE.txt" -Destination "$classesFolder/NOTICE.txt" -Force

    # Build a manifest with the main class and implementation version (used by the launcher
    # to resolve the per-version cache directory).
    $manifestPath = "$tempFolder/MANIFEST.MF"
    @(
        "Manifest-Version: 1.0"
        "Main-Class: $mainClass"
        "Implementation-Title: $artifactId"
        "Implementation-Version: $version"
        "Implementation-Vendor: Microsoft"
        ""
    ) -join "`n" | Out-File -FilePath $manifestPath -Encoding ascii -Force

    # Create the executable fat JAR
    $jarName = "$artifactId-$version.jar"
    Write-Host "  Creating $jarName" -ForegroundColor Green
    Invoke-LoggedCommand "& `"$jar`" --create --file `"$serverOutputPath/$jarName`" --manifest `"$manifestPath`" -C `"$classesFolder`" ."

    # Create the sources JAR
    $sourcesJarName = "$artifactId-$version-sources.jar"
    Write-Host "  Creating $sourcesJarName"
    Invoke-LoggedCommand "& `"$jar`" --create --file `"$serverOutputPath/$sourcesJarName`" -C `"$sourcesFolder`" ."

    # Create a minimal javadoc JAR (placeholder for future Maven Central publishing)
    $javadocFolder = "$tempFolder/javadoc"
    New-Item -ItemType Directory -Force -Path $javadocFolder | Out-Null
    "Javadoc for $artifactId $version. See $($server.readmeUrl)." | Out-File -FilePath "$javadocFolder/README.txt" -Encoding utf8 -Force
    $javadocJarName = "$artifactId-$version-javadoc.jar"
    Write-Host "  Creating $javadocJarName"
    Invoke-LoggedCommand "& `"$jar`" --create --file `"$serverOutputPath/$javadocJarName`" -C `"$javadocFolder`" ."

    # Render the POM from the template
    $pomTemplate = Get-Content "$mavenSourcePath/pom.xml.template" -Raw
    $pomContent = $pomTemplate `
        -replace '{{GROUP_ID}}', $groupId `
        -replace '{{ARTIFACT_ID}}', $artifactId `
        -replace '{{VERSION}}', $version `
        -replace '{{DESCRIPTION}}', $description `
        -replace '{{HOMEPAGE}}', $server.readmeUrl `
        -replace '{{KEYWORDS}}', (Get-KeywordsString $keywords)

    $pomPath = "$serverOutputPath/$artifactId-$version.pom"
    Write-Host "  Writing $artifactId-$version.pom"
    $pomContent | Out-File -FilePath $pomPath -Encoding utf8 -Force

    Write-Host "`n✅ Maven package built successfully for $($server.name)" -ForegroundColor Green
    Write-Host "   Coordinates: ${groupId}:${artifactId}:${version}"
    Write-Host "   Platforms: $(($bundledPlatforms | Sort-Object -Unique) -join ', ')"
}

# Main execution
foreach ($server in $buildInfo.servers) {
    Write-Host "`n========================================" -ForegroundColor Cyan
    Write-Host "Building Maven package for $($server.name)" -ForegroundColor Cyan
    Write-Host "========================================" -ForegroundColor Cyan

    BuildServerPackage $server
}

# Cleanup temp folder
Remove-Item -Path $tempFolder -Recurse -Force -ErrorAction SilentlyContinue -ProgressAction SilentlyContinue

Write-Host "`n========================================" -ForegroundColor Green
Write-Host "Maven packaging complete!" -ForegroundColor Green
Write-Host "Output: $OutputPath" -ForegroundColor Green
Write-Host "========================================" -ForegroundColor Green

exit $exitCode
