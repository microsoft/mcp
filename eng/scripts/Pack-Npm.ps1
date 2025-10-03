#!/bin/env pwsh
#Requires -Version 7

[CmdletBinding()]
param(
    [string] $ArtifactsPath,
    [string] $OutputPath,
    [switch] $UsePaths
)

. "$PSScriptRoot/../common/scripts/common.ps1"
$RepoRoot = $RepoRoot.Path.Replace('\', '/')

. "$RepoRoot/eng/scripts/Process-PackageReadMe.ps1"

$wrapperSourcePath = "$RepoRoot/eng/npm/wrapper"
$platformSourcePath = "$RepoRoot/eng/npm/platform"

if(!$ArtifactsPath) {
    $ArtifactsPath = "$RepoRoot/.work/build"
}

if(!$OutputPath) {
    $OutputPath = "$RepoRoot/.work/package"
}

if(!(Test-Path $ArtifactsPath)) {
    Write-Error "Artifacts path $ArtifactsPath does not exist."
    return
}

$tempFolder = "$RepoRoot/.work/temp"

Push-Location $RepoRoot
try {
    # Clear and recreate the output directory
    Remove-Item -Path $OutputPath -Recurse -Force -ErrorAction SilentlyContinue -ProgressAction SilentlyContinue

    $wrapperJsonFiles = Get-ChildItem -Path $ArtifactsPath -Filter "wrapper.json" -Recurse
    foreach($wrapperJsonFile in $wrapperJsonFiles) {
        $serverDirectory = $wrapperJsonFile.Directory
        $serverName = $serverDirectory.Name
        $platformOutputPath = "$OutputPath/npm/$serverName/platform"
        $wrapperOutputPath = "$OutputPath/npm/$serverName/wrapper"

        New-Item -ItemType Directory -Force -Path $platformOutputPath | Out-Null
        New-Item -ItemType Directory -Force -Path $wrapperOutputPath | Out-Null

        $wrapperPackageJson = Get-Content $wrapperJsonFile -Raw | ConvertFrom-Json -AsHashtable

        $platformDirectories = Get-ChildItem -Path $serverDirectory -Directory

        # Build the project
        foreach ($platformDirectory in $platformDirectories) {
            Remove-Item -Path $tempFolder -Recurse -Force -ErrorAction SilentlyContinue -ProgressAction SilentlyContinue
            Copy-Item -Path $platformDirectory -Destination $tempFolder -Recurse -Force
            Copy-Item -Path "$platformSourcePath/*" -Destination $tempFolder -Force
            Write-Host "Copied platform script files into $tempFolder"

            # Remove symbols files before packing
            Get-ChildItem -Path $tempFolder -Recurse -Include "*.pdb", "*.dSYM", "*.dbg" | Remove-Item -Force -Recurse -ErrorAction SilentlyContinue

            $platformFile = "$tempFolder/package.json"
            $platformPackageJson = Get-Content $platformFile -Raw | ConvertFrom-Json -AsHashtable

            if ($platformPackageJson.version -ne $wrapperPackageJson.version) {
                Write-Error "Version mismatch in $platformFile. Expected $($wrapperPackageJson.version), found $($platformPackageJson.version)"
                return
            }

            $os = $platformPackageJson.os[0]
            $cpu = $platformPackageJson.cpu[0]
            $binPath = $platformPackageJson.bin.Values[0]

            if($wrapperPackageJson.os -notcontains $os) {
                $wrapperPackageJson.os += $os
            }

            if($wrapperPackageJson.cpu -notcontains $cpu) {
                $wrapperPackageJson.cpu += $cpu
            }

            if (!$IsWindows) {
                Write-Host "Setting executable permissions for $tempFolder/index.js" -ForegroundColor Yellow
                Invoke-LoggedCommand "chmod +x `"$tempFolder/index.js`""

                if ($os -ne 'win32') {
                    Write-Host "Setting executable permissions for $tempFolder/$binPath" -ForegroundColor Yellow
                    Invoke-LoggedCommand "chmod +x `"$tempFolder/$binPath`""
                }
            }
            else {
                Write-Warning "Executable permissions are not set when packing on a Windows agent."
            }

            Extract-PackageSpecificReadMe -InputReadMePath "$serverDirectory/README.md" `
                -OutputDirectory $tempFolder -PackageType "npm" -InsertPayload @{ ToolTitle = 'NPM Package' }

            Copy-Item -Path "$RepoRoot/LICENSE" -Destination $tempFolder -Force
            Copy-Item -Path "$RepoRoot/NOTICE.txt" -Destination $tempFolder -Force
            Write-Host "Copied README.md, NOTICE.txt and LICENSE to $tempFolder"

            Write-Host "Packaging $tempFolder into $platformOutputPath"
            Invoke-LoggedCommand "npm pack $tempFolder --pack-destination '$platformOutputPath'" -GroupOutput | Tee-Object -Variable fileName
            Write-Host "Package location: $platformOutputPath/$fileName" -ForegroundColor Yellow

            if ($UsePaths) {
                $wrapperPackageJson.optionalDependencies[$platformPackageJson.name] = "file://$((Resolve-Path "$platformOutputPath/$fileName").Path.Replace('\', '/'))"
            } else {
                $wrapperPackageJson.optionalDependencies[$platformPackageJson.name] = $platformPackageJson.version
            }
        }

        Remove-Item -Path $tempFolder -Recurse -Force -ErrorAction SilentlyContinue -ProgressAction SilentlyContinue
        Copy-Item -Path $wrapperSourcePath -Destination $tempFolder -Recurse -Force
        Write-Host "Copied wrapper script files into $tempFolder"

        if (!$IsWindows) {
            Write-Host "Setting executable permissions for $tempFolder/index.js" -ForegroundColor Yellow
            Invoke-LoggedCommand "chmod +x `"$tempFolder/index.js`""
        }

        $wrapperPackageJson | ConvertTo-Json -Depth 10 | Out-File -FilePath "$tempFolder/package.json" -Encoding utf8
        Write-Host "Created package.json in $tempFolder"

        Extract-PackageSpecificReadMe -InputReadMePath "$serverDirectory/README.md" `
            -OutputDirectory $tempFolder -PackageType "npm" -InsertPayload @{ ToolTitle = 'NPM Package' }
            
        Copy-Item -Path "$RepoRoot/LICENSE" -Destination $tempFolder -Force
        Write-Host "Copied README.md and LICENSE to $tempFolder"

        Write-Host "Packaging $tempFolder into $wrapperOutputPath"
        Invoke-LoggedCommand "npm pack $tempFolder --pack-destination '$wrapperOutputPath'" -GroupOutput | Tee-Object -Variable fileName
        Write-Host "Package location: $wrapperOutputPath/$fileName" -ForegroundColor Yellow
    }

    Remove-Item -Path $tempFolder -Recurse -Force -ErrorAction SilentlyContinue -ProgressAction SilentlyContinue
    Write-Host "`nPackaging completed successfully!" -ForegroundColor Green
}
finally {
    Pop-Location
}

