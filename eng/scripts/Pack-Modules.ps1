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

$wrapperSourcePath = "$RepoRoot/eng/npm/wrapper"
$platformSourcePath = "$RepoRoot/eng/npm/platform"
$nuspecSourcePath = "$RepoRoot/eng/dnx/nuspec"

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
        $npmPlatformOutputPath = "$OutputPath/$serverName/npm/platform"
        $npmWrapperOutputPath = "$OutputPath/$serverName/npm/wrapper"
        $nugetPlatformOutputPath = "$OutputPath/$serverName/nuget/platform"
        $nugetWrapperOutputPath = "$OutputPath/$serverName/nuget/wrapper"

        New-Item -ItemType Directory -Force -Path $npmPlatformOutputPath | Out-Null
        New-Item -ItemType Directory -Force -Path $npmWrapperOutputPath | Out-Null
        New-Item -ItemType Directory -Force -Path $nugetPlatformOutputPath | Out-Null
        New-Item -ItemType Directory -Force -Path $nugetWrapperOutputPath | Out-Null

        $wrapperPackageJson = Get-Content $wrapperJsonFile -Raw | ConvertFrom-Json -AsHashtable

        $platformDirectories = Get-ChildItem -Path $serverDirectory -Directory

        $tempNugetWrapperDir = Join-Path $nugetWrapperOutputPath "temp"
        $wrapperToolDir = "$tempNugetWrapperDir/tools/$($platformPackageJson.targetFramework)/any"
        New-Item -ItemType Directory -Force -Path $wrapperToolDir | Out-Null

        (Get-Content -Path "$nuspecSourcePath/RuntimeAgnosticToolSettingsTemplate.xml") `
            -replace "__CommandName__", $platformPackageJson.commandName `
            Set-Content -Path "$wrapperToolDir/DotnetToolSettings.xml"
        Copy-Item -Path "$serverDirectory/README.md" -Destination $tempNugetWrapperDir -Force

        # Build the project
        foreach ($platformDirectory in $platformDirectories) {
            Remove-Item -Path $tempFolder -Recurse -Force -ErrorAction SilentlyContinue -ProgressAction SilentlyContinue
            Copy-Item -Path $platformDirectory -Destination $tempFolder -Recurse -Force
            Copy-Item -Path "$platformSourcePath/*" -Destination $tempFolder -Force
            Write-Host "Copied platform script files into $tempFolder"

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

            Copy-Item -Path "$serverDirectory/README.md" -Destination $tempFolder -Force
            Copy-Item -Path "$RepoRoot/LICENSE" -Destination $tempFolder -Force
            Copy-Item -Path "$RepoRoot/NOTICE.txt" -Destination $tempFolder -Force
            Write-Host "Copied README.md, NOTICE.txt and LICENSE to $tempFolder"

            Write-Host "Packaging $tempFolder into $npmPlatformOutputPath"
            Invoke-LoggedCommand "npm pack $tempFolder --pack-destination '$npmPlatformOutputPath'" -GroupOutput | Tee-Object -Variable fileName
            Write-Host "Package location: $npmPlatformOutputPath/$fileName" -ForegroundColor Yellow

            if ($UsePaths) {
                $wrapperPackageJson.optionalDependencies[$platformPackageJson.name] = "file://$((Resolve-Path "$npmPlatformOutputPath/$fileName").Path.Replace('\', '/'))"
            } else {
                $wrapperPackageJson.optionalDependencies[$platformPackageJson.name] = $platformPackageJson.version
            }

            # Create Nuget Packages
            $tempPlatformDir = Join-Path $nugetPlatformOutputPath $platformDirectory
            $platformToolDir = "$tempPlatformDir/tools/any/$platformDirectory"
            $platformNuspecFile = "$tempPlatformDir/$($platformPackageJson.packageId).nuspec"
            New-Item -ItemType Directory -Force -Path $platformToolDir | Out-Null

            Copy-Item -Path "$tempFolder/dist" -Destination $platformToolDir -Force
            (Get-Content -Path "$nuspecSourcePath/RuntimeSpecificTemplate.nuspec") `
                -replace "__Id__", $platformPackageJson.packageId `
                -replace "__Version__", $platformPackageJson.version `
                -replace "__Authors__", $platformPackageJson.author `
                -replace "__Description__", $platformPackageJson.description `
                -replace "__RepositoryUrl__", $platformPackageJson.repository.url `
                -replace "__RepositoryBranch__", $platformPackageJson.repository.branch `
                -replace "__CommitSHA__", $platformPackageJson.repository.commitSHA `
                -replace "__TargetFramework__", $platformPackageJson.targetFramework |
                Set-Content -Path $platformNuspecFile

            (Get-Content -Path "$nuspecSourcePath/RuntimeSpecificToolSettingsTemplate.xml") `
                -replace "__CommandName__", $platformPackageJson.commandName `
                -replace "__CommandEntryPoint__", $platformPackageJson.entryPoint |
                Set-Content -Path "$platformToolDir/DotnetToolSettings.xml"

            Write-Host "Creating Nuget Package from $platformNuspecFile"
            Invoke-LoggedCommand "nuget pack $platformNuspecFile -OutputDirectory '$nugetPlatformOutputPath'" -GroupOutput
            Remove-Item -Path $tempPlatformDir -Recurse -Force -ErrorAction SilentlyContinue -ProgressAction SilentlyContinue
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

        Copy-Item -Path "$serverDirectory/README.md" -Destination $tempFolder -Force
        Copy-Item -Path "$RepoRoot/LICENSE" -Destination $tempFolder -Force
        Write-Host "Copied README.md and LICENSE to $tempFolder"

        Write-Host "Packaging $tempFolder into $npmWrapperOutputPath"
        Invoke-LoggedCommand "npm pack $tempFolder --pack-destination '$npmWrapperOutputPath'" -GroupOutput | Tee-Object -Variable fileName
        Write-Host "Package location: $npmWrapperOutputPath/$fileName" -ForegroundColor Yellow
    }

    Remove-Item -Path $tempFolder -Recurse -Force -ErrorAction SilentlyContinue -ProgressAction SilentlyContinue
    Write-Host "`nPackaging completed successfully!" -ForegroundColor Green
}
finally {
    Pop-Location
}
