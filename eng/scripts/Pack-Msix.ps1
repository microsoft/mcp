#!/bin/env pwsh
#Requires -Version 7

<#
.SYNOPSIS
    Creates MSIX packages from server binaries for Windows on-device agent registry.

.DESCRIPTION
    This script packages MCP servers into the MSIX format for Windows ODR (on-device registry)
    integration. MSIX packages provide:
    - Package identity for secure containment
    - Automatic registration/unregistration with Windows ODR
    - Enterprise management via MDM/Intune
    - Microsoft Store ready distribution (future)

    The script reads server and platform information from build_info.json and creates unsigned
    .msix files for Windows x64 platforms. The packages can then be signed using ESRP or a
    local test certificate.

.PARAMETER ArtifactsPath
    Path to the build artifacts directory containing the server binaries.
    Defaults to ".work/build" in the repo root.

.PARAMETER BuildInfoPath
    Path to the build_info.json file containing server and platform details.
    Defaults to ".work/build_info.json" in the repo root.

.PARAMETER OutputPath
    Output directory for the .msix files.
    Defaults to ".work/packages_msix" in the repo root.

.PARAMETER ServerName
    Name of a specific server to package (e.g., "Azure.Mcp.Server").
    If not specified, all servers with MSIX configuration will be packaged.

.PARAMETER CertificatePath
    Path to a .pfx certificate file for local signing (optional).
    If provided, the package will be signed after creation.

.PARAMETER CertificatePassword
    Password for the certificate file (optional, used with -CertificatePath).

.PARAMETER KeepStagingDirectory
    If specified, keeps the staging directory after packaging for debugging purposes.

.PARAMETER McpbStagingPath
    Path to the MCPB staging directory (output from Pack-Mcpb.ps1 -KeepStagingDirectory).
    If specified, MSIX packaging will use the manifest.json from this directory which
    already contains the _meta.com.microsoft.windows.static_responses section with
    all tools auto-discovered by 'mcpb pack --update'.

.PARAMETER McpbPackagePath
    Path to an existing .mcpb package file. If specified, the package will be unpacked
    and its manifest.json will be used for MSIX packaging. This is an alternative to
    -McpbStagingPath when the staging directory is not available.

.EXAMPLE
    ./Pack-Msix.ps1

.EXAMPLE
    ./Pack-Msix.ps1 -ServerName "Azure.Mcp.Server"

.EXAMPLE
    ./Pack-Msix.ps1 -CertificatePath "test.pfx" -CertificatePassword "password"

.EXAMPLE
    # Use MCPB staging directory (recommended - avoids regenerating manifest)
    ./Pack-Mcpb.ps1 -KeepStagingDirectory
    ./Pack-Msix.ps1 -McpbStagingPath ".work/temp_mcpb"

.EXAMPLE
    # Use existing .mcpb package
    ./Pack-Msix.ps1 -McpbPackagePath ".work/packages_mcpb/Azure.Mcp.Server/Azure.Mcp.Server-win-x64.mcpb"
#>

[CmdletBinding()]
param(
    [string] $ArtifactsPath,
    [string] $BuildInfoPath,
    [string] $OutputPath,
    [string] $ServerName,
    [string] $CertificatePath,
    [string] $CertificatePassword,
    [string] $McpbStagingPath,
    [string] $McpbPackagePath,
    [switch] $KeepStagingDirectory
)

$ErrorActionPreference = "Stop"
. "$PSScriptRoot/../common/scripts/common.ps1"

$RepoRoot = $RepoRoot.Path.Replace('\', '/')
$ignoreMissingArtifacts = $env:TF_BUILD -ne 'true'
$exitCode = 0

if (!$ArtifactsPath) {
    $ArtifactsPath = "$RepoRoot/.work/build"
}

if (!$BuildInfoPath) {
    $BuildInfoPath = "$RepoRoot/.work/build_info.json"
}

if (!$OutputPath) {
    $OutputPath = "$RepoRoot/.work/packages_msix"
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

# Check for WinAppCli (preferred) or MakeAppx.exe (fallback)
# Use SDK 10.0.22621.0 - the 10.0.26100.0 SDK forces TrustedLaunch for ODR extensions,
# which requires Store-level signing. ESRP Authenticode signing does not satisfy
# TrustedLaunch CI policy, so we use the older SDK which does not enforce it.
$useWinAppCli = $false

if (Get-Command winapp -ErrorAction SilentlyContinue) {
    $useWinAppCli = $true
    Write-Host "Using WinAppCli for MSIX packaging"
} else {
    # Use MakeAppx.exe from Windows SDK 10.0.22621.0 (10.0.26100.0 forces TrustedLaunch for ODR)
    $makeAppxPath = "${env:ProgramFiles(x86)}\Windows Kits\10\bin\10.0.22621.0\x64\makeappx.exe"

    if (-not (Test-Path $makeAppxPath)) {
        LogError "MakeAppx.exe not found. Please install Windows SDK 10.0.22621.0: winget install Microsoft.WindowsSDK.10.0.22621"
        exit 1
    }

    Write-Host "Using MakeAppx.exe from: $makeAppxPath"
}

# Always use SignTool for signing (WinAppCli has issues with complex Publisher DNs)
$signToolPath = $null
if ($CertificatePath) {
    $signToolPath = "${env:ProgramFiles(x86)}\Windows Kits\10\bin\10.0.22621.0\x64\signtool.exe"

    if (-not (Test-Path $signToolPath)) {
        LogError "SignTool.exe not found. Please install the Windows SDK: winget install Microsoft.WindowsSDK.10.0.22621"
        exit 1
    }

    Write-Host "Using SignTool.exe from: $signToolPath"
}

if ($CertificatePath -and !(Test-Path $CertificatePath)) {
    LogError "Certificate file not found: $CertificatePath"
    exit 1
}

$buildInfo = Get-Content $BuildInfoPath -Raw | ConvertFrom-Json

if (Test-Path $OutputPath) {
    Write-Host "Cleaning existing output path $OutputPath"
    Remove-Item -Path $OutputPath -Recurse -Force -ErrorAction SilentlyContinue
}

New-Item -ItemType Directory -Force -Path $OutputPath | Out-Null

$tempPath = "$RepoRoot/.work/temp_msix"

foreach ($server in $buildInfo.servers) {
    # Skip if ServerName is specified and doesn't match
    if ($ServerName -and $server.name -ne $ServerName) {
        continue
    }

    $msixDirectory = "$RepoRoot/servers/$($server.name)/msix"
    $mcpbDirectory = "$RepoRoot/servers/$($server.name)/mcpb"
    $manifestTemplatePath = "$msixDirectory/AppxManifest.template.xml"
    
    # Determine the source for MCP manifest.json
    # Priority: 1) MCPB staging dir, 2) Unpacked .mcpb, 3) Source mcpb/manifest.json
    # MCPB staging/packed manifests include _meta.com.microsoft.windows.static_responses with all tools
    $mcpManifestPath = $null
    $mcpManifestSource = $null
    
    # Option 1: Use MCPB staging directory if provided
    if ($McpbStagingPath) {
        # Platform naming in MCPB staging is "windows-x64", not "win-x64"
        $stagingManifestPath = "$McpbStagingPath/$($server.name)/windows-x64/manifest.json"
        if (Test-Path $stagingManifestPath) {
            $mcpManifestPath = $stagingManifestPath
            $mcpManifestSource = "MCPB staging directory"
        }
    }
    
    # Option 2: Unpack existing .mcpb package if provided
    if (-not $mcpManifestPath -and $McpbPackagePath) {
        # The McpbPackagePath could be a template with server name, or direct path
        $actualMcpbPath = $McpbPackagePath -replace '\{SERVER_NAME\}', $server.name
        if (Test-Path $actualMcpbPath) {
            $unpackDir = "$tempPath/unpacked_mcpb/$($server.name)"
            Remove-Item -Path $unpackDir -Recurse -Force -ErrorAction SilentlyContinue
            New-Item -ItemType Directory -Force -Path $unpackDir | Out-Null
            
            Write-Host "Unpacking MCPB from $actualMcpbPath..."
            & mcpb unpack $actualMcpbPath --output $unpackDir 2>&1 | Out-Null
            if ($LASTEXITCODE -eq 0 -and (Test-Path "$unpackDir/manifest.json")) {
                $mcpManifestPath = "$unpackDir/manifest.json"
                $mcpManifestSource = "unpacked .mcpb file"
            } else {
                LogWarning "Failed to unpack MCPB from $actualMcpbPath"
            }
        }
    }
    
    # Option 3: Fall back to source manifest (will need to regenerate _meta)
    if (-not $mcpManifestPath) {
        $sourceManifestPath = "$mcpbDirectory/manifest.json"
        if (Test-Path $sourceManifestPath) {
            $mcpManifestPath = $sourceManifestPath
            $mcpManifestSource = "source manifest (no _meta, will generate empty tools list)"
        }
    }

    if (!(Test-Path $manifestTemplatePath)) {
        LogWarning "MSIX manifest template not found at $manifestTemplatePath. Skipping server $($server.name)."
        continue
    }

    if (-not $mcpManifestPath) {
        LogWarning "No MCP manifest found for $($server.name). Skipping."
        continue
    }

    Write-Host "`n========================================" -ForegroundColor Cyan
    Write-Host "Packing MSIX for server: $($server.name)" -ForegroundColor Cyan
    Write-Host "Version: $($server.version)" -ForegroundColor Cyan
    Write-Host "MCP Manifest: $mcpManifestSource" -ForegroundColor Cyan
    Write-Host "========================================`n" -ForegroundColor Cyan

    # Filter platforms: only Windows x64 for now (MSIX is Windows-only)
    $windowsPlatform = $server.platforms | Where-Object { 
        $_.operatingSystem -eq "windows" -and 
        $_.architecture -eq "x64" -and
        $_.trimmed -eq $true -and 
        -not $_.native -and 
        -not $_.specialPurpose 
    } | Select-Object -First 1

    if (-not $windowsPlatform) {
        LogWarning "No suitable Windows x64 platform found for MSIX packaging for server $($server.name)"
        continue
    }

    $platformDirectory = "$ArtifactsPath/$($windowsPlatform.artifactPath)"

    if (!(Test-Path $platformDirectory)) {
        $message = "Platform directory $platformDirectory does not exist."
        if ($ignoreMissingArtifacts) {
            LogWarning $message
            continue
        } else {
            LogError $message
            $exitCode = 1
            continue
        }
    }

    Write-Host @"

------------------------------------------------------------------------------------
Processing MSIX packaging:
  Server: $($server.name)
  Platform: $($windowsPlatform.name)
  Version: $($server.version)
  CLI Name: $($server.cliName)
  Binaries Path: $platformDirectory
------------------------------------------------------------------------------------

"@

    # Create clean staging directory
    $stagingDir = "$tempPath/$($server.name)"
    Remove-Item -Path $stagingDir -Recurse -Force -ErrorAction SilentlyContinue
    New-Item -ItemType Directory -Force -Path $stagingDir | Out-Null
    New-Item -ItemType Directory -Force -Path "$stagingDir/server" | Out-Null
    New-Item -ItemType Directory -Force -Path "$stagingDir/Assets" | Out-Null

    # Copy server binaries
    Write-Host "Copying server binaries from $platformDirectory..."
    Copy-Item -Path "$platformDirectory/*" -Destination "$stagingDir/server" -Recurse

    # Process AppxManifest.xml template
    Write-Host "Processing AppxManifest.xml template..."
    $manifestContent = Get-Content $manifestTemplatePath -Raw

    # Convert version to Windows format (must be X.Y.Z.W with only numbers)
    # Strip prerelease suffix and ensure 4-part version
    $versionParts = $server.version -split '[-+]' | Select-Object -First 1
    $versionSegments = $versionParts -split '\.'
    while ($versionSegments.Count -lt 4) {
        $versionSegments += "0"
    }
    $windowsVersion = ($versionSegments[0..3] -join '.')

    # Replace template placeholders
    $manifestContent = $manifestContent -replace '\{\{VERSION\}\}', $windowsVersion
    $manifestContent = $manifestContent -replace '\{\{DISPLAY_NAME\}\}', $server.assemblyTitle
    $manifestContent = $manifestContent -replace '\{\{DESCRIPTION\}\}', $server.description
    $manifestContent = $manifestContent -replace '\{\{CLI_NAME\}\}', $server.cliName
    $manifestContent = $manifestContent -replace '\{\{SERVER_NAME\}\}', $server.name

    $manifestContent | Set-Content "$stagingDir/AppxManifest.xml" -NoNewline

    # Copy MCP manifest.json (for ODR registration) - reuse MCPB manifest with Windows-specific modifications
    Write-Host "Processing MCP manifest from $mcpManifestPath..."
    $mcpManifest = Get-Content $mcpManifestPath -Raw | ConvertFrom-Json

    # Update version to match package
    $mcpManifest.version = $server.version

    # Update entry_point for MSIX structure (Windows-only, no ${__dirname} needed)
    $executableName = $server.cliName + $windowsPlatform.extension
    $mcpManifest.server.entry_point = "server/$executableName"
    $mcpManifest.server.mcp_config.command = "server/$executableName"

    # Remove platform_overrides since MSIX is Windows-only
    if ($mcpManifest.server.mcp_config.PSObject.Properties.Name -contains 'platform_overrides') {
        $mcpManifest.server.mcp_config.PSObject.Properties.Remove('platform_overrides')
    }

    # Update compatibility to Windows-only
    if ($mcpManifest.PSObject.Properties.Name -contains 'compatibility') {
        $mcpManifest.compatibility.platforms = @("win32")
        # Remove claude_desktop since this is for Windows ODR
        if ($mcpManifest.compatibility.PSObject.Properties.Name -contains 'claude_desktop') {
            $mcpManifest.compatibility.PSObject.Properties.Remove('claude_desktop')
        }
    }

    # Handle _meta.com.microsoft.windows.static_responses section for Windows ODR containment
    # This allows Windows to validate server responses without launching the server
    # If manifest already has _meta with tools (from MCPB staging), preserve it; otherwise generate minimal
    $hasExistingMeta = $false
    if ($mcpManifest.PSObject.Properties.Name -contains '_meta') {
        $meta = $mcpManifest._meta
        if ($meta.'com.microsoft.windows'.static_responses.'tools/list'.tools.Count -gt 0) {
            $hasExistingMeta = $true
            $toolCount = $meta.'com.microsoft.windows'.static_responses.'tools/list'.tools.Count
            Write-Host "Using existing _meta with $toolCount tools from $mcpManifestSource" -ForegroundColor Green
            
            # Just update the version in serverInfo to match package version
            if ($meta.'com.microsoft.windows'.static_responses.initialize.serverInfo) {
                $mcpManifest._meta.'com.microsoft.windows'.static_responses.initialize.serverInfo.version = $server.version
            }
        }
    }
    
    if (-not $hasExistingMeta) {
        Write-Host "Generating minimal _meta (no tools discovered - consider using -McpbStagingPath)" -ForegroundColor Yellow
        $mcpManifest | Add-Member -NotePropertyName '_meta' -NotePropertyValue @{} -Force
        $mcpManifest._meta = @{
            'com.microsoft.windows' = @{
                'static_responses' = @{
                    'initialize' = @{
                        'protocolVersion' = '2024-11-05'
                        'capabilities' = @{
                            'logging' = @{}
                            'tools' = @{
                                'listChanged' = $true
                            }
                        }
                        'serverInfo' = @{
                            'name' = $mcpManifest.display_name
                            'version' = $server.version
                        }
                    }
                    'tools/list' = @{
                        'tools' = @()
                    }
                }
            }
        }
    }

    $mcpManifest | ConvertTo-Json -Depth 100 | Set-Content "$stagingDir/Assets/manifest.json" -NoNewline

    # Copy asset images - prefer MSIX-specific assets, fall back to MCPB assets
    $assetsDirectory = "$msixDirectory/Assets"
    if (Test-Path $assetsDirectory) {
        Write-Host "Copying asset images from $assetsDirectory..."
        Copy-Item -Path "$assetsDirectory/*" -Destination "$stagingDir/Assets" -Recurse
    } else {
        LogWarning "MSIX Assets directory not found at $assetsDirectory. Using MCPB assets as fallback."
    }

    # Copy servericon from MCPB if not already present
    $serverIconPath = "$stagingDir/Assets/servericon.png"
    if (!(Test-Path $serverIconPath)) {
        $mcpbIconPath = "$mcpbDirectory/servericon.png"
        if (Test-Path $mcpbIconPath) {
            Write-Host "Copying servericon from MCPB..."
            Copy-Item $mcpbIconPath $serverIconPath
        }
    }

    # Use servericon as StoreLogo if StoreLogo doesn't exist
    if (!(Test-Path "$stagingDir/Assets/StoreLogo.png") -and (Test-Path $serverIconPath)) {
        Write-Host "Using servericon as StoreLogo..."
        Copy-Item $serverIconPath "$stagingDir/Assets/StoreLogo.png"
    }

    # Use servericon for other required logos if they don't exist
    $requiredLogos = @("Square44x44Logo.png", "Square150x150Logo.png", "Wide310x150Logo.png")
    foreach ($logo in $requiredLogos) {
        $logoPath = "$stagingDir/Assets/$logo"
        if (!(Test-Path $logoPath) -and (Test-Path $serverIconPath)) {
            Write-Host "Using servericon as $logo..."
            Copy-Item $serverIconPath $logoPath
        }
    }

    # Also check for package icon and use it if no assets exist
    if ($server.packageIcon -and !(Test-Path "$stagingDir/Assets/StoreLogo.png")) {
        $packageIconPath = "$RepoRoot/$($server.packageIcon)"
        if (Test-Path $packageIconPath) {
            Write-Host "Using package icon as StoreLogo..."
            Copy-Item $packageIconPath "$stagingDir/Assets/StoreLogo.png"
        }
    }

    # Create output directory for this server
    $serverOutputPath = "$OutputPath/$($server.artifactPath)"
    New-Item -ItemType Directory -Force -Path $serverOutputPath | Out-Null

    # Pack the MSIX
    $msixFileName = "$($server.name)-$($windowsPlatform.architecture).msix"
    $msixFilePath = "$serverOutputPath/$msixFileName"

    Write-Host "Packing MSIX to $msixFilePath..."
    
    if ($useWinAppCli) {
        # Use WinAppCli for packaging (handles TrustedLaunch and other modern requirements)
        # Note: We don't use WinAppCli's built-in signing because it has issues with complex Publisher DNs
        $winappArgs = @("pack", $stagingDir, "--output", $msixFilePath, "--manifest", "$stagingDir/AppxManifest.xml")
        
        & winapp @winappArgs
        if ($LASTEXITCODE -ne 0) {
            LogError "MSIX packing failed for $($server.name)"
            $exitCode = 1
            continue
        }
    } else {
        # Fallback to MakeAppx.exe
        & $makeAppxPath pack /d $stagingDir /p $msixFilePath /o
        if ($LASTEXITCODE -ne 0) {
            LogError "MSIX packing failed for $($server.name)"
            $exitCode = 1
            continue
        }
    }

    # Sign with SignTool if certificate is provided
    # We always use SignTool (not WinAppCli's built-in signing) because WinAppCli
    # has issues with complex Publisher DNs that include O=, L=, S=, C= components
    if ($CertificatePath -and $signToolPath) {
        Write-Host "Signing MSIX with certificate..."
        $signArgs = @("sign", "/fd", "SHA256", "/a", "/f", $CertificatePath)
        if ($CertificatePassword) {
            $signArgs += @("/p", $CertificatePassword)
        }
        $signArgs += $msixFilePath

        & $signToolPath @signArgs
        if ($LASTEXITCODE -ne 0) {
            LogError "MSIX signing failed for $($server.name)"
            $exitCode = 1
            continue
        }
        Write-Host "MSIX signed successfully" -ForegroundColor Green
    }

    # Get file size for reporting
    $fileSize = (Get-Item $msixFilePath).Length
    $fileSizeMB = [math]::Round($fileSize / 1MB, 2)

    Write-Host "Created: $msixFilePath ($fileSizeMB MB)" -ForegroundColor Green
}

# Cleanup temp directory (unless -KeepStagingDirectory is specified)
if ($KeepStagingDirectory) {
    Write-Host "`nStaging directory preserved at: $tempPath" -ForegroundColor Yellow
} else {
    Remove-Item -Path $tempPath -Recurse -Force -ErrorAction SilentlyContinue
}

if ($exitCode -eq 0) {
    Write-Host "`n========================================" -ForegroundColor Green
    Write-Host "MSIX packaging completed successfully!" -ForegroundColor Green
    Write-Host "Output: $OutputPath" -ForegroundColor Green
    Write-Host "========================================`n" -ForegroundColor Green
} else {
    Write-Host "`n========================================" -ForegroundColor Red
    Write-Host "MSIX packaging completed with errors." -ForegroundColor Red
    Write-Host "========================================`n" -ForegroundColor Red
}

exit $exitCode
