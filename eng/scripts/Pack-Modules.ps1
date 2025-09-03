#!/bin/env pwsh
#Requires -Version 7

[CmdletBinding()]
param(
    [string] $ArtifactsPath,
    [string] $OutputPath,
    [string] $RepoUrl,
    [string] $CommitSha,
    [string] $Branch,
    [switch] $UsePaths
)

. "$PSScriptRoot/../common/scripts/common.ps1"
$RepoRoot = $RepoRoot.Path.Replace('\', '/')

$wrapperSourcePath = "$RepoRoot/eng/npm/wrapper"
$platformSourcePath = "$RepoRoot/eng/npm/platform"
$mcpServerjson = "$RepoRoot/eng/dnx/.mcp/server.json"
$nuspecSourcePath = "$RepoRoot/eng/dnx/nuspec"
$projectPropertiesScript = "$RepoRoot/eng/scripts/Get-ProjectProperties.ps1"

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
$sharedProjectProperties = & "$projectPropertiesScript" -ProjectName "Directory.Build.props"

Push-Location $RepoRoot
try {
    # Clear and recreate the output directory
    Remove-Item -Path $OutputPath -Recurse -Force -ErrorAction SilentlyContinue -ProgressAction SilentlyContinue

    $wrapperJsonFiles = Get-ChildItem -Path $ArtifactsPath -Filter "wrapper.json" -Recurse
    foreach($wrapperJsonFile in $wrapperJsonFiles) {
        $serverDirectory = $wrapperJsonFile.Directory
        $serverName = $serverDirectory.Name
        $serverProjectProperties = & "$projectPropertiesScript" -ProjectName "$($serverName -replace '-native', '').csproj"
        $npmPlatformOutputPath = "$OutputPath/npm/$serverName/platform"
        $npmWrapperOutputPath = "$OutputPath/npm/$serverName/wrapper"
        $nugetPlatformOutputPath = "$OutputPath/nuget/$serverName/platform"
        $nugetWrapperOutputPath = "$OutputPath/nuget/$serverName/wrapper"

        New-Item -ItemType Directory -Force -Path $npmPlatformOutputPath | Out-Null
        New-Item -ItemType Directory -Force -Path $npmWrapperOutputPath | Out-Null
        New-Item -ItemType Directory -Force -Path $nugetPlatformOutputPath | Out-Null
        New-Item -ItemType Directory -Force -Path $nugetWrapperOutputPath | Out-Null

        $wrapperPackageJson = Get-Content $wrapperJsonFile -Raw | ConvertFrom-Json -AsHashtable

        $platformDirectories = Get-ChildItem -Path $serverDirectory -Directory
        
        # Create dnx wrapper nuget tool
        $tempNugetWrapperDir = Join-Path $nugetWrapperOutputPath "temp"
        $wrapperToolDir = "$tempNugetWrapperDir/tools/$($sharedProjectProperties.TargetFramework)/any"
        $wrapperToolNuspec = "$tempNugetWrapperDir/$($serverProjectProperties.PackageId).nuspec"
        New-Item -ItemType Directory -Force -Path $wrapperToolDir | Out-Null
        New-Item -ItemType Directory -Force -Path "$tempNugetWrapperDir/.mcp" | Out-Null
        
        (Get-Content -Path "$nuspecSourcePath/RuntimeAgnosticToolSettingsTemplate.xml") `
            -replace "__CommandName__", $serverProjectProperties.CliName |
            Set-Content -Path "$wrapperToolDir/DotnetToolSettings.xml"

        (Get-Content -Path $mcpServerjson -Raw) `
            -replace '\$\(PackageDescription\)', $serverProjectProperties.Description `
            -replace '\$\(PackageId\)', $serverProjectProperties.PackageId `
            -replace '\$\(PackageVersion\)', $serverProjectProperties.Version `
            -replace '\$\(RepositoryUrl\)', $RepoUrl |
            Set-Content -Path "$tempNugetWrapperDir/.mcp/server.json"
        
        (Get-Content -Path "$nuspecSourcePath/RuntimeAgnosticTemplate.nuspec") `
            -replace "__Id__", $serverProjectProperties.PackageId `
            -replace "__Version__", $serverProjectProperties.Version `
            -replace "__Authors__", $wrapperPackageJson.author `
            -replace "__Description__", $wrapperPackageJson.description `
            -replace "__Tags__", $serverProjectProperties.PackageTags `
            -replace "__RepositoryUrl__", $RepoUrl `
            -replace "__RepositoryBranch__", $Branch `
            -replace "__CommitSHA__", $CommitSha `
            -replace "__TargetFramework__", $sharedProjectProperties.TargetFramework |
            Set-Content -Path $wrapperToolNuspec
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
            $platformOSArch = $platformDirectory.Name
            $tempPlatformDir = Join-Path $nugetPlatformOutputPath $platformOSArch
            $platformToolDir = "$tempPlatformDir/tools/any/$platformOSArch"
            $platformPackageId = "$($serverProjectProperties.PackageId).$platformOSArch"
            $platformNuspecFile = "$tempPlatformDir/$platformPackageId.nuspec"
            New-Item -ItemType Directory -Force -Path $platformToolDir | Out-Null

            Copy-Item -Path "$tempFolder/dist/*" -Destination $platformToolDir -Recurse -Force
            $platformToolEntryPoint = (
                Get-ChildItem -Path $platformToolDir -Filter "$($serverProjectProperties.CliName)*" -Recurse |
                Where-Object { $_.PSIsContainer -eq $false -and ($_.Extension -eq ".exe" -or $_.Extension -eq "") } |
                Select-Object -First 1
            ).Name
            (Get-Content -Path "$nuspecSourcePath/RuntimeSpecificTemplate.nuspec") `
                -replace "__Id__", $platformPackageId `
                -replace "__Version__", $serverProjectProperties.version `
                -replace "__Authors__", $wrapperPackageJson.author `
                -replace "__Description__", $platformPackageJson.description `
                -replace "__RepositoryUrl__", $RepoUrl `
                -replace "__RepositoryBranch__", $Branch `
                -replace "__CommitSHA__", $CommitSha `
                -replace "__TargetFramework__", $sharedProjectProperties.TargetFramework |
                Set-Content -Path $platformNuspecFile

            (Get-Content -Path "$nuspecSourcePath/RuntimeSpecificToolSettingsTemplate.xml") `
                -replace "__CommandName__", $serverProjectProperties.CliName `
                -replace "__CommandEntryPoint__", $platformToolEntryPoint |
                Set-Content -Path "$platformToolDir/DotnetToolSettings.xml"

            [xml]$wrapperToolSettings = Get-Content -Path "$wrapperToolDir/DotnetToolSettings.xml"
            $ridNode = $wrapperToolSettings.DotNetCliTool.RuntimeIdentifierPackages
            if ($ridNode.Count -eq 1 -and $ridNode.RuntimeIdentifierPackage.RuntimeIdentifier -eq "__RuntimeIdentifier__") {
                $ridNode.RemoveAll()
            }
            $newRid = $wrapperToolSettings.CreateElement("RuntimeIdentifierPackage")
            $newRid.SetAttribute("RuntimeIdentifier", $platformOSArch)
            $newRid.SetAttribute("Id", $platformPackageId)
            $ridNode.AppendChild($newRid) | Out-Null
            $wrapperToolSettings.Save("$wrapperToolDir/DotnetToolSettings.xml")

            Write-Host "Creating Nuget Package from $platformNuspecFile"
            Invoke-LoggedCommand "nuget pack '$platformNuspecFile' -OutputDirectory '$nugetPlatformOutputPath'" -GroupOutput
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

        Write-Host "Creating Nuget Package from $wrapperToolNuspec"
        Invoke-LoggedCommand "nuget pack '$wrapperToolNuspec' -OutputDirectory '$nugetWrapperOutputPath'" -GroupOutput
        Remove-Item -Path $tempNugetWrapperDir -Recurse -Force -ErrorAction SilentlyContinue -ProgressAction SilentlyContinue
    }

    Remove-Item -Path $tempFolder -Recurse -Force -ErrorAction SilentlyContinue -ProgressAction SilentlyContinue
    Write-Host "`nPackaging completed successfully!" -ForegroundColor Green
}
finally {
    Pop-Location
}
