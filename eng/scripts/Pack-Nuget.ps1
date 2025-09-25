#!/bin/env pwsh
#Requires -Version 7

param(
	[string] $ArtifactsPath,
    [string] $BuildInfoPath,
	[string] $OutputPath
)

$ErrorActionPreference = "Stop"
. "$PSScriptRoot/../common/scripts/common.ps1"
$RepoRoot = $RepoRoot.Path.Replace('\', '/')

$tempFolder = "$RepoRoot/.work/temp"
$nuspecSourcePath = "$RepoRoot/eng/dnx/nuspec"
$azureIconPath = "$RepoRoot/eng/images/azureicon.png"

# When running locally, ignore missing artifacts instead of failing
$ignoreMissingArtifacts = $env:TF_BUILD -ne 'true'

$exitCode = 0

if(!$ArtifactsPath) {
	$ArtifactsPath = "$RepoRoot/.work/build"
}

if(!$BuildInfoPath) {
    $BuildInfoPath = "$RepoRoot/.work/build_info.json"
}

if (!$OutputPath) {
    $OutputPath = "$RepoRoot/.work/packages_dnx"
}

if(!(Test-Path $ArtifactsPath)) {
	LogError "Artifacts path $ArtifactsPath does not exist."
	$exitCode = 1
}

if (!(Test-Path $BuildInfoPath)) {
    LogError "Build info file $BuildInfoPath does not exist. Run eng/scripts/New-BuildInfo.ps1 to create it."
    $exitCode = 1
}

# Exit early if there were parameter errors
if($exitCode -ne 0) {
    exit $exitCode
}

$buildInfo = Get-Content $BuildInfoPath -Raw | ConvertFrom-Json -AsHashtable

Remove-Item -Path $OutputPath -Recurse -Force -ErrorAction SilentlyContinue -ProgressAction SilentlyContinue

$sharedTargetFramework = dotnet msbuild "$RepoRoot/Directory.Build.props" -getProperty:TargetFramework
if($LASTEXITCODE -ne 0 -or !$sharedTargetFramework) {
    LogError "Failed to get TargetFramework from Directory.Build.props"
    exit 1
}

function ExportDotnetToolSettings {
    param(
        [ValidateNotNullOrWhiteSpace()]
        [string] $OutputPath,
        [ValidateNotNullOrWhiteSpace()]
        [string] $CliName,
        [ValidateNotNullOrWhiteSpace()]
        [string] $RuntimeIdentifier,
        [ValidateNotNullOrWhiteSpace()]
        [string] $Id
    )

    $xml = New-Object System.Xml.XmlDocument
    $xml.AppendChild($xml.CreateXmlDeclaration("1.0", "UTF-8", $null)) | Out-Null

    $dotnetCliTool = $xml.AppendChild($xml.CreateElement("DotNetCliTool"))
    $dotnetCliTool.SetAttribute("Version", "2")

    $commands = $dotnetCliTool.AppendChild($xml.CreateElement("Commands"))
    $command = $commands.AppendChild($xml.CreateElement("Command"))
    $command.SetAttribute("Name", $CliName)

    $ridPackages = $dotnetCliTool.AppendChild($xml.CreateElement("RuntimeIdentifierPackages"))
    $ridPackage = $ridPackages.AppendChild($xml.CreateElement("RuntimeIdentifierPackage"))
    $ridPackage.SetAttribute("RuntimeIdentifier", $RuntimeIdentifier)
    $ridPackage.SetAttribute("Id", $Id)

    $xml.Save($OutputPath)
}

function ExportServerJson {
    param(
        [ValidateNotNullOrWhiteSpace()]
        [string] $OutputPath,
        [ValidateNotNullOrWhiteSpace()]
        [string] $Description,
        [ValidateNotNullOrWhiteSpace()]
        [string] $ServerName,
        [ValidateNotNullOrWhiteSpace()]
        [string] $DnxPackageId,
        [ValidateNotNullOrWhiteSpace()]
        [string] $Version,
        [ValidateNotNullOrWhiteSpace()]
        [string] $RepositoryUrl
    )

    if (!$OutputPath) {
        throw "Output path is required for ExportServerJson"
    }

    if (!$Description) {
        throw "Description is required for ExportServerJson"
    }

    if (!$ServerName) {
        throw "Server name is required for ExportServerJson"
    }

    if (!$DnxPackageId) {
        throw "DNX Package ID is required for ExportServerJson"
    }

    if (!$Version) {
        throw "Version is required for ExportServerJson"
    }

    if (!$RepositoryUrl) {
        throw "Repository URL is required for ExportServerJson"
    }

    $output = @{
        '$schema' = "https://modelcontextprotocol.io/schemas/draft/2025-07-09/server.json"
        description = $Description
        name = "io.github.microsoft/mcp/$ServerName"
        packages = @(
            @{
                registry_name = "nuget"
                name = $DnxPackageId
                version = $Version
                package_arguments = @(
                    @{ type = "positional"; value = "server"; value_hint = "server" },
                    @{ type = "positional"; value = "start"; value_hint = "start" }
                )
            }
        )
        repository = @{
            url = $RepositoryUrl
            source = "github"
        }
        version_detail = @{
            version = $Version
        }
    }

    $output | ConvertTo-Json | Out-File -Path $OutputPath -Encoding utf8
}

function ExportWrapperPackageNuspec {
    param(
        [ValidateNotNullOrWhiteSpace()]
        [string] $OutputPath,
        [ValidateNotNullOrWhiteSpace()]
        [string] $DnxPackageId,
        [ValidateNotNullOrWhiteSpace()]
        [string] $Version,
        [ValidateNotNullOrWhiteSpace()]
        [string] $Description,
        [ValidateNotNullOrWhiteSpace()]
        [string] $Tags,
        [ValidateNotNullOrWhiteSpace()]
        [string] $RepositoryUrl,
        [ValidateNotNullOrWhiteSpace()]
        [string] $Branch,
        [ValidateNotNullOrWhiteSpace()]
        [string] $CommitSha,
        [ValidateNotNullOrWhiteSpace()]
        [string] $SharedTargetFramework
    )

    $xml = New-Object System.Xml.XmlDocument
    $xml.AppendChild($xml.CreateXmlDeclaration("1.0", "utf-8", $null)) | Out-Null
    $package = $xml.AppendChild($xml.CreateElement("package"))
    $package.SetAttribute("xmlns", "http://schemas.microsoft.com/packaging/2012/06/nuspec.xsd")

    $metadata = $package.AppendChild($xml.CreateElement("metadata"))

    $id = $metadata.AppendChild($xml.CreateElement("id"))
    $id.InnerText = $DnxPackageId

    $ver = $metadata.AppendChild($xml.CreateElement("version"))
    $ver.InnerText = $Version

    $authors = $metadata.AppendChild($xml.CreateElement("authors"))
    $authors.InnerText = "Microsoft"

    $requireLicenseAcceptance = $metadata.AppendChild($xml.CreateElement("requireLicenseAcceptance"))
    $requireLicenseAcceptance.InnerText = "false"

    $license = $metadata.AppendChild($xml.CreateElement("license"))
    $license.SetAttribute("type", "expression")
    $license.InnerText = "MIT"

    $licenseUrl = $metadata.AppendChild($xml.CreateElement("licenseUrl"))
    $licenseUrl.InnerText = "https://licenses.nuget.org/MIT"

    $readme = $metadata.AppendChild($xml.CreateElement("readme"))
    $readme.InnerText = "README.md"

    $desc = $metadata.AppendChild($xml.CreateElement("description"))
    $desc.InnerText = $Description

    $relNotes = $metadata.AppendChild($xml.CreateElement("releaseNotes"))
    $relNotes.InnerText = "$RepoUrl/tree/$ReleaseTag/servers/$ServerName/CHANGELOG.md"

    $tagsElem = $metadata.AppendChild($xml.CreateElement("tags"))
    $tagsElem.InnerText = $Tags

    $copyright = $metadata.AppendChild($xml.CreateElement("copyright"))
    $copyright.InnerText = "© Microsoft Corporation. All rights reserved."

    $projectUrlElem = $metadata.AppendChild($xml.CreateElement("projectUrl"))
    $projectUrlElem.InnerText = "$RepoUrl/tree/$ReleaseTag/servers/$ServerName"

    $packageTypes = $metadata.AppendChild($xml.CreateElement("packageTypes"))
    $packageType1 = $packageTypes.AppendChild($xml.CreateElement("packageType"))
    $packageType1.SetAttribute("name", "DotnetTool")

    $packageType2 = $packageTypes.AppendChild($xml.CreateElement("packageType"))
    $packageType2.SetAttribute("name", "McpServer")


    $repository = $metadata.AppendChild($xml.CreateElement("repository"))
    $repository.SetAttribute("type", "git")
    $repository.SetAttribute("url", $RepositoryUrl)
    $repository.SetAttribute("branch", $Branch)
    $repository.SetAttribute("commit", $CommitSha)

    $frameworkReferences = $metadata.AppendChild($xml.CreateElement("frameworkReferences"))
    $group = $frameworkReferences.AppendChild($xml.CreateElement("group"))
    $group.SetAttribute("targetFramework", $SharedTargetFramework)
    $frameworkReference = $group.AppendChild($xml.CreateElement("frameworkReference"))
    $frameworkReference.SetAttribute("name", "Microsoft.AspNetCore.App")

    $icon = $metadata.AppendChild($xml.CreateElement("icon"))
    $icon.InnerText = "azureicon.png"

    $xml.Save($OutputPath)
}

function BuildServerPackages([hashtable] $server, [bool] $native) {
    $serverDirectory = "$ArtifactsPath/$($server.artifactPath)"

    if(!(Test-Path $serverDirectory)) {
        $message = "Server directory $serverDirectory does not exist."
        if ($ignoreMissingArtifacts) {
            Write-Warning $message
        } else {
            Write-Error $message
        }
        return
    }

    $filteredPlatforms = $server.platforms | Where-Object { $_.native -eq $native }
    if ($filteredPlatforms.Count -eq 0) {
        Write-Host "No platforms to build for server $($server.name) with native=$native"
        return
    }

    $serverOutputPath = "$OutputPath/$($server.artifactPath)"

    $wrapperOutputPath = "$serverOutputPath/wrapper"
    New-Item -ItemType Directory -Force -Path $wrapperOutputPath | Out-Null

    $platformOutputPath = "$serverOutputPath/platform"
    New-Item -ItemType Directory -Force -Path $platformOutputPath | Out-Null

    $packageId = $server.dnxPackageId
    $description = $server.dnxDescription ? $server.dnxDescription : $server.description

    # Create dnx wrapper nuget tool
    $wrapperToolDir = "$tempFolder/tools/$sharedTargetFramework/any"
    $wrapperToolNuspec = "$tempFolder/$packageId.nuspec"
    New-Item -ItemType Directory -Force -Path $wrapperToolDir | Out-Null
    New-Item -ItemType Directory -Force -Path "$tempFolder/.mcp" | Out-Null

    $packageVersion = $server.version
    $releaseTag = $buildInfo.releaseTag
    $repoUrl = $buildInfo.repositoryUrl

    ExportDotnetToolSettings `
        -CliName $server.cliName `
        -RuntimeIdentifier $sharedTargetFramework `
        -Id $packageId `
        -OutputPath "$tempFolder/tools/$sharedTargetFramework/any/DotnetToolSettings.xml"

    # Export ServerJson
    ExportServerJson `
        -Description $description `
        -ServerName $server.name `
        -DnxPackageId $packageId `
        -Version $server.version `
        -RepositoryUrl $buildInfo.repositoryUrl `
        -OutputPath "$tempFolder/.mcp/server.json"


    # Export WrapperPackageNuspec
    ExportWrapperPackageNuspec `
        -DnxPackageId $packageId `
        -Version $version `
        -Description $description `
        -Tags $tags `
        -RepositoryUrl $buildInfo.repositoryUrl `
        -Branch $branch `
        -CommitSha $commitSha `
        -SharedTargetFramework $sharedTargetFramework `
        -OutputPath $wrapperToolNuspec

    Copy-Item -Path "$nuspecSourcePath/README.md" -Destination $tempFolder -Force
    Copy-Item -Path "$RepoRoot/LICENSE" -Destination $tempFolder -Force
    Copy-Item -Path "$RepoRoot/NOTICE.txt" -Destination $tempFolder -Force
    Copy-Item -Path $azureIconPath -Destination $tempFolder -Force

    # Build the project
    foreach ($platformDirectory in $platformDirectories) {
        $platformOSArch = $platformDirectory.Name
        $tempPlatformDir = Join-Path $platformOutputPath $platformOSArch
        $platformToolDir = "$tempPlatformDir/tools/any/$platformOSArch"
        $platformPackageId = "$packageId.$($server.nodeOs)-$($server.architecture)"
        $platformNuspecFile = "$tempPlatformDir/$platformPackageId.nuspec"
        New-Item -ItemType Directory -Force -Path $platformToolDir | Out-Null

        Copy-Item -Path "$platformDirectory/dist/*" -Destination $platformToolDir -Recurse -Force
        Copy-Item -Path $azureIconPath -Destination $tempPlatformDir -Force
        Copy-Item -Path "$RepoRoot/LICENSE" -Destination $tempPlatformDir -Force
        Copy-Item -Path "$RepoRoot/NOTICE.txt" -Destination $tempPlatformDir -Force

        $platformToolEntryPoint = (
            Get-ChildItem -Path $platformToolDir -Filter "$($serverProjectProperties.CliName)*" -Recurse |
            Where-Object { $_.PSIsContainer -eq $false -and ($_.Extension -eq ".exe" -or $_.Extension -eq "") } |
            Select-Object -First 1
        ).Name

        ExportPlatformPackageNuspec `
            -PackageId $platformPackageId `
            -Version $packageVersion `
            -Description ($serverProjectProperties.PackageDescription -replace '\$\(RuntimeIdentifier\)', $platformOSArch) `
            -Tags ($serverProjectProperties.PackageTags -replace ';', ' ').Trim() `
            -RepositoryUrl $repoUrl `
            -Branch $branch `
            -CommitSha $commitSha `
            -SharedTargetFramework $sharedTargetFramework `
            -ToolEntryPoint $platformToolEntryPoint `
            -OutputPath $platformNuspecFile

        (Get-Content -Path "$nuspecSourcePath/RuntimeSpecificTemplate.nuspec") `
            -replace "__Id__", $platformPackageId `
            -replace "__Version__", $packageVersion `
            -replace "__Authors__", $wrapperPackageJson.author `
            -replace "__Description__", ($serverProjectProperties.PackageDescription -replace '\$\(RuntimeIdentifier\)', $platformOSArch) `
            -replace "__Tags__", ($serverProjectProperties.PackageTags -replace ';', ' ').Trim() `
            -replace "__RepositoryUrl__", $RepoUrl `
            -replace "__ProjectUrl__", "$RepoUrl/tree/$releaseTag/servers/$serverName" `
            -replace "__ReleaseNotes__", "$RepoUrl/tree/$releaseTag/servers/$serverName/CHANGELOG.md" `
            -replace "__RepositoryBranch__", $Branch `
            -replace "__CommitSHA__", $CommitSha `
            -replace "__TargetFramework__", $sharedTargetFramework |
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

        if ((Get-Content -Raw -Path $platformNuspecFile) + (Get-Content -Raw -Path "$platformToolDir/DotnetToolSettings.xml") -match '__') {
            Write-Error "Placeholder(s) with '__' still found in $platformNuspecFile or DotnetToolSettings.xml. Please check your replacements."
            exit 1
        }

        LogInfo "Creating Nuget Symbol Package from $platformNuspecFile"
        Invoke-LoggedCommand "nuget pack '$platformNuspecFile' -OutputDirectory '$platformOutputPath'" -GroupOutput
        $generatedNupkg = Get-ChildItem -Path $platformOutputPath -Filter "*.nupkg" | Sort-Object LastWriteTime -Descending | Select-Object -First 1
        $symbolPkgName = $generatedNupkg.Name -replace ".nupkg$", ".symbols.nupkg"
        Rename-Item -Path $generatedNupkg.FullName -NewName $symbolPkgName -Force

        Get-ChildItem -Path $platformToolDir -Recurse -Include "*.pdb", "*.dSYM", "*.dbg" | Remove-Item -Force -Recurse -ErrorAction SilentlyContinue
        LogInfo "Creating Nuget Package from $platformNuspecFile"
        Invoke-LoggedCommand "nuget pack '$platformNuspecFile' -OutputDirectory '$platformOutputPath'" -GroupOutput
        Remove-Item -Path $tempPlatformDir -Recurse -Force -ErrorAction SilentlyContinue -ProgressAction SilentlyContinue
    }

    LogInfo "Creating Nuget Package from $wrapperToolNuspec"
    Invoke-LoggedCommand "nuget pack '$wrapperToolNuspec' -OutputDirectory '$wrapperOutputPath'" -GroupOutput
    Remove-Item -Path $tempFolder -Recurse -Force -ErrorAction SilentlyContinue -ProgressAction SilentlyContinue

}

Push-Location $RepoRoot
try {
	# Clear and recreate the output directory
	Remove-Item -Path $OutputPath -Recurse -Force -ErrorAction SilentlyContinue -ProgressAction SilentlyContinue

	$wrapperJsonFiles = Get-ChildItem -Path $ArtifactsPath -Filter "wrapper.json" -Recurse
	foreach($wrapperJsonFile in $wrapperJsonFiles) {
		$serverDirectory = $wrapperJsonFile.Directory

	}
	LogSuccess "`nNuGet packaging completed successfully!" -ForegroundColor Green
}
finally {
	Pop-Location
}
