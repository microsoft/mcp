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
. "$RepoRoot/eng/scripts/Process-PackageReadMe.ps1"

$tempFolder = "$RepoRoot/.work/temp"
$nuspecSourcePath = "$RepoRoot/eng/dnx/nuspec"

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

$buildInfo = Get-Content $BuildInfoPath -Raw | ConvertFrom-Json -AsHashtable

Remove-Item -Path $OutputPath -Recurse -Force -ErrorAction SilentlyContinue -ProgressAction SilentlyContinue

$tempDirectory = "$RepoRoot/.work/temp"
Remove-Item -Path $tempDirectory -Recurse -Force -ErrorAction SilentlyContinue -ProgressAction SilentlyContinue

Write-Host "Getting TargetFramework from Directory.Build.props"
$sharedTargetFramework = dotnet msbuild "$RepoRoot/Directory.Build.props" -getProperty:TargetFramework

if($LASTEXITCODE -ne 0 -or !$sharedTargetFramework) {
    LogError "Failed to get TargetFramework from Directory.Build.props"
    $exitCode = 1
}

$buildInfo = Get-Content $BuildInfoPath -Raw | ConvertFrom-Json -AsHashtable

# Exit early if there were parameter errors
if($exitCode -ne 0) {
    exit $exitCode
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
        [string] $PackageId,
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

    if (!$PackageId) {
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
                name = $PackageId
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

    $output | ConvertTo-Json -Depth 10 | Out-File -Path $OutputPath -Encoding utf8

    Write-Host "`n== Generated $OutputPath` =="
    Get-Content $OutputPath | Out-Host
    Write-Host ""
}

function ExportWrapperToolSettings {
    param(
        [ValidateNotNullOrWhiteSpace()]
        [string] $OutputPath,
        [ValidateNotNullOrWhiteSpace()]
        [string] $CommandName,
        [ValidateNotNullOrWhiteSpace()]
        [string] $RuntimeIdentifier,
        [ValidateNotNullOrWhiteSpace()]
        [string] $Id,
        [hashtable] $PlatformReferences
    )

    $xml = New-Object System.Xml.XmlDocument
    $xml.AppendChild($xml.CreateXmlDeclaration("1.0", "UTF-8", $null)) | Out-Null

    $dotnetCliTool = $xml.AppendChild($xml.CreateElement("DotNetCliTool"))
    $dotnetCliTool.SetAttribute("Version", "2")

    $commands = $dotnetCliTool.AppendChild($xml.CreateElement("Commands"))
    $command = $commands.AppendChild($xml.CreateElement("Command"))
    $command.SetAttribute("Name", $CommandName)

    $ridPackages = $dotnetCliTool.AppendChild($xml.CreateElement("RuntimeIdentifierPackages"))
    $ridPackage = $ridPackages.AppendChild($xml.CreateElement("RuntimeIdentifierPackage"))
    $ridPackage.SetAttribute("RuntimeIdentifier", $RuntimeIdentifier)
    $ridPackage.SetAttribute("Id", $Id)

    foreach ($key in $PlatformReferences.Keys) {
        $platformRef = $ridPackages.AppendChild($xml.CreateElement("RuntimeIdentifierPackage"))
        $platformRef.SetAttribute("RuntimeIdentifier", $key)
        $platformRef.SetAttribute("Id", $PlatformReferences[$key])
    }

    $xml.Save($OutputPath)

    Write-Host "`n== Generated $OutputPath` =="
    Get-Content $OutputPath | Out-Host
    Write-Host ""

}

function ExportWrapperPackageNuspec {
    param(
        [ValidateNotNullOrWhiteSpace()]
        [string] $OutputPath,
        [ValidateNotNullOrWhiteSpace()]
        [string] $PackageId,
        [ValidateNotNullOrWhiteSpace()]
        [string] $Version,
        [ValidateNotNullOrWhiteSpace()]
        [string] $Description,
        [ValidateNotNullOrWhiteSpace()]
        [string[]] $Tags,
        [ValidateNotNullOrWhiteSpace()]
        [string] $RepositoryUrl,
        [ValidateNotNullOrWhiteSpace()]
        [string] $Branch,
        [ValidateNotNullOrWhiteSpace()]
        [string] $CommitSha,
        [ValidateNotNullOrWhiteSpace()]
        [string] $SharedTargetFramework,
        [ValidateNotNullOrWhiteSpace()]
        [string] $PackageIcon
    )

    $xml = New-Object System.Xml.XmlDocument
    $xml.AppendChild($xml.CreateXmlDeclaration("1.0", "utf-8", $null)) | Out-Null
    $package = $xml.AppendChild($xml.CreateElement("package"))
    $package.SetAttribute("xmlns", "http://schemas.microsoft.com/packaging/2012/06/nuspec.xsd")

    $metadata = $package.AppendChild($xml.CreateElement("metadata"))

    $id = $metadata.AppendChild($xml.CreateElement("id"))
    $id.InnerText = $PackageId

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
    $tagsElem.InnerText = $Tags -join ' '

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
    $icon.InnerText = Split-Path $PackageIcon -Leaf

    $xml.Save($OutputPath)

    Write-Host "`n== Generated $OutputPath` =="
    Get-Content $OutputPath | Out-Host
    Write-Host ""
}

function ExportPlatformToolSettings {
    param(
        [ValidateNotNullOrWhiteSpace()]
        [string] $OutputPath,
        [ValidateNotNullOrWhiteSpace()]
        [string] $CommandName,
        [ValidateNotNullOrWhiteSpace()]
        [string] $EntryPoint
    )

    $xml = New-Object System.Xml.XmlDocument
    $xml.AppendChild($xml.CreateXmlDeclaration("1.0", "UTF-8", $null)) | Out-Null

    $dotnetCliTool = $xml.AppendChild($xml.CreateElement("DotNetCliTool"))
    $dotnetCliTool.SetAttribute("Version", "2")

    $commands = $dotnetCliTool.AppendChild($xml.CreateElement("Commands"))
    $command = $commands.AppendChild($xml.CreateElement("Command"))
    $command.SetAttribute("Name", $CliName)
    $command.SetAttribute("EntryPoint", $EntryPoint)
    $command.SetAttribute("Runner", "executable")

    $xml.Save($OutputPath)

    Write-Host "`n== Generated $OutputPath` =="
    Get-Content $OutputPath | Out-Host
    Write-Host ""
}

function ExportPlatformPackageNuspec {
    param(
        [ValidateNotNullOrWhiteSpace()]
        [string] $OutputPath,
        [ValidateNotNullOrWhiteSpace()]
        [string] $PackageId,
        [ValidateNotNullOrWhiteSpace()]
        [string] $Version,
        [ValidateNotNullOrWhiteSpace()]
        [string] $Description,
        [ValidateNotNullOrWhiteSpace()]
        [string[]] $Tags,
        [ValidateNotNullOrWhiteSpace()]
        [string] $RepositoryUrl,
        [ValidateNotNullOrWhiteSpace()]
        [string] $ReleaseTag,
        [ValidateNotNullOrWhiteSpace()]
        [string] $Branch,
        [ValidateNotNullOrWhiteSpace()]
        [string] $CommitSha,
        [ValidateNotNullOrWhiteSpace()]
        [string] $SharedTargetFramework,
        [ValidateNotNullOrWhiteSpace()]
        [string] $PackageIcon
    )

    $xml = New-Object System.Xml.XmlDocument
    $xml.AppendChild($xml.CreateXmlDeclaration("1.0", "utf-8", $null)) | Out-Null
    $package = $xml.AppendChild($xml.CreateElement("package"))
    $package.SetAttribute("xmlns", "http://schemas.microsoft.com/packaging/2012/06/nuspec.xsd")

    $metadata = $package.AppendChild($xml.CreateElement("metadata"))

    $id = $metadata.AppendChild($xml.CreateElement("id"))
    $id.InnerText = $PackageId

    $ver = $metadata.AppendChild($xml.CreateElement("version"))
    $ver.InnerText = $Version

    $authors = $metadata.AppendChild($xml.CreateElement("authors"))
    $authors.InnerText = "Microsoft"

    $license = $metadata.AppendChild($xml.CreateElement("license"))
    $license.SetAttribute("type", "expression")
    $license.InnerText = "MIT"

    $licenseUrl = $metadata.AppendChild($xml.CreateElement("licenseUrl"))
    $licenseUrl.InnerText = "https://licenses.nuget.org/MIT"

    $desc = $metadata.AppendChild($xml.CreateElement("description"))
    $desc.InnerText = $Description

    $relNotes = $metadata.AppendChild($xml.CreateElement("releaseNotes"))
    $relNotes.InnerText = "$RepoUrl/tree/$ReleaseTag/servers/$ServerName/CHANGELOG.md"

    $tagsElem = $metadata.AppendChild($xml.CreateElement("tags"))
    $tagsElem.InnerText = $Tags -join ' '

    $copyright = $metadata.AppendChild($xml.CreateElement("copyright"))
    $copyright.InnerText = "© Microsoft Corporation. All rights reserved."

    $projectUrlElem = $metadata.AppendChild($xml.CreateElement("projectUrl"))
    $projectUrlElem.InnerText = "$RepoUrl/tree/$ReleaseTag/servers/$ServerName"

    $packageTypes = $metadata.AppendChild($xml.CreateElement("packageTypes"))
    $packageType1 = $packageTypes.AppendChild($xml.CreateElement("packageType"))
    $packageType1.SetAttribute("name", "DotnetToolRidPackage")

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
    $icon.InnerText = Split-Path $PackageIcon -Leaf

    $xml.Save($OutputPath)

    Write-Host "`n== Generated $OutputPath` =="
    Get-Content $OutputPath | Out-Host
    Write-Host ""
}

function BuildServerPackages([hashtable] $server, [bool] $native) {
    LogInfo "## Packing $($native ? 'native' : 'non-native') NuGet packages for server $($server.name)"
    $repoUrl = $buildInfo.repositoryUrl
    $packageId = $server.dnxPackageId
    $description = $server.dnxDescription ? $server.dnxDescription : $server.description
    $iconFileName = Split-Path $server.packageIcon -Leaf

    $filteredPlatforms = $server.platforms | Where-Object { $_.native -eq $native }
    if ($filteredPlatforms.Count -eq 0) {
        LogInfo "No platforms to build for server $($server.name) with native=$native"
        return
    }

    $serverOutputPath = "$OutputPath/$($server.artifactPath)"

    $platformOutputPath = "$serverOutputPath/platform"
    New-Item -ItemType Directory -Force -Path $platformOutputPath | Out-Null

    # Process all platform packages before the wrapper package
    $platformRefs = @{}

    # Build the project
    foreach ($platform in $filteredPlatforms) {
        LogInfo "## Packing platform $($platform.name)"
        $platformDirectory = "$ArtifactsPath/$($platform.artifactPath)"

        if(!(Test-Path $platformDirectory)) {
            $message = "Platform directory $platformDirectory does not exist."
            if ($ignoreMissingArtifacts) {
                LogWarning $message
            } else {
                LogError $message
                $script:exitCode = 1
            }

            continue
        }

        $os = $platform.dotnetOs
        $arch = $platform.architecture
        $platformOsArch = "$os-$arch"

        $platformToolDir = "$tempDirectory/tools/any/$platformOsArch"
        $platformPackageId = "$packageId.$platformOsArch"
        $platformDescription = "$description. Internal implementation package for $platformOsArch."
        $platformNuspecFile = "$tempDirectory/$platformPackageId.nuspec"
        New-Item -ItemType Directory -Force -Path $platformToolDir | Out-Null

        Copy-Item -Path "$platformDirectory/*" -Destination $platformToolDir -Recurse -Force -ProgressAction SilentlyContinue
        Copy-Item -Path $server.packageIcon -Destination $tempDirectory -Force
        Copy-Item -Path "$RepoRoot/LICENSE" -Destination $tempDirectory -Force
        Copy-Item -Path "$RepoRoot/NOTICE.txt" -Destination $tempDirectory -Force

        $platformToolEntryPoint = (
            Get-ChildItem -Path $platformToolDir -Filter "$($server.cliName)*" -Recurse |
            Where-Object { $_.PSIsContainer -eq $false -and ($_.Extension -eq ".exe" -or $_.Extension -eq "") } |
            Select-Object -First 1
        ).Name

        ExportPlatformPackageNuspec `
            -PackageId $platformPackageId `
            -Version $server.version `
            -Description $platformDescription `
            -Tags $server.dnxPackageTags `
            -RepositoryUrl $repoUrl `
            -ReleaseTag $server.releaseTag `
            -Branch $buildInfo.branch `
            -CommitSha $buildInfo.commitSha `
            -SharedTargetFramework $sharedTargetFramework `
            -ToolEntryPoint $platformToolEntryPoint `
            -PackageIcon $iconFileName `
            -OutputPath $platformNuspecFile

        ExportPlatformToolSettings `
            -CommandName $server.cliName `
            -EntryPoint $platformToolEntryPoint `
            -OutputPath "$platformToolDir/DotnetToolSettings.xml"

        $platformRefs[$platformOsArch] = $platformPackageId

        LogInfo "Creating Nuget Symbol Package from $platformNuspecFile"
        Invoke-LoggedCommand "nuget pack '$platformNuspecFile' -OutputDirectory '$platformOutputPath'" -GroupOutput
        $generatedNupkg = Get-ChildItem -Path $platformOutputPath -Filter "*.nupkg" | Sort-Object LastWriteTime -Descending | Select-Object -First 1
        $symbolPkgName = $generatedNupkg.Name -replace ".nupkg$", ".symbols.nupkg"
        Rename-Item -Path $generatedNupkg.FullName -NewName $symbolPkgName -Force

        Get-ChildItem -Path $platformToolDir -Recurse -Include "*.pdb", "*.dSYM", "*.dbg" | Remove-Item -Force -Recurse -ErrorAction SilentlyContinue
        LogInfo "Creating Nuget Package from $platformNuspecFile"
        Invoke-LoggedCommand "nuget pack '$platformNuspecFile' -OutputDirectory '$platformOutputPath'" -GroupOutput
        Remove-Item -Path $tempDirectory -Recurse -Force -ErrorAction SilentlyContinue -ProgressAction SilentlyContinue
    }

    $wrapperOutputPath = "$serverOutputPath/wrapper"
    New-Item -ItemType Directory -Force -Path $wrapperOutputPath | Out-Null

    # Create dnx wrapper nuget tool
    $wrapperToolDir = "$tempFolder/tools/$sharedTargetFramework/any"
    $wrapperToolNuspec = "$tempFolder/$packageId.nuspec"
    New-Item -ItemType Directory -Force -Path $wrapperToolDir | Out-Null
    New-Item -ItemType Directory -Force -Path "$tempFolder/.mcp" | Out-Null

    Copy-Item -Path "$nuspecSourcePath/README.md" -Destination $tempFolder -Force
    Copy-Item -Path "$RepoRoot/LICENSE" -Destination $tempFolder -Force
    Copy-Item -Path "$RepoRoot/NOTICE.txt" -Destination $tempFolder -Force
    Copy-Item -Path $server.packageIcon -Destination $tempFolder -Force

    # Export ServerJson
    ExportServerJson `
        -Description $description `
        -ServerName $server.name `
        -PackageId $packageId `
        -Version $server.version `
        -RepositoryUrl $buildInfo.repositoryUrl `
        -OutputPath "$tempFolder/.mcp/server.json"

    # Export WrapperPackageNuspec
    ExportWrapperPackageNuspec `
        -PackageId $packageId `
        -Version $server.version `
        -Description $description `
        -Tags $server.dnxPackageTags `
        -RepositoryUrl $buildInfo.repositoryUrl `
        -ReleaseTag $server.releaseTag `
        -Branch $buildInfo.branch `
        -CommitSha $buildInfo.commitSha `
        -SharedTargetFramework $sharedTargetFramework `
        -PackageIcon $iconFileName `
        -OutputPath $wrapperToolNuspec

    ExportWrapperToolSettings `
        -CliName $server.cliName `
        -RuntimeIdentifier $sharedTargetFramework `
        -Id $packageId `
        -PlatformReferences $platformRefs `
        -OutputPath "$tempFolder/tools/$sharedTargetFramework/any/DotnetToolSettings.xml"

    Extract-PackageSpecificReadMe `
        -InputReadMePath "$RepoRoot/$($server.readmePath)" `
        -OutputDirectory $tempFolder `
        -PackageType "nuget" `
        -InsertPayload @{ ToolTitle = '.NET Tool' }

    LogInfo "Creating Nuget Package from $wrapperToolNuspec"
    Invoke-LoggedCommand "nuget pack '$wrapperToolNuspec' -OutputDirectory '$wrapperOutputPath'" -GroupOutput
    Remove-Item -Path $tempFolder -Recurse -Force -ErrorAction SilentlyContinue -ProgressAction SilentlyContinue
}

Push-Location $RepoRoot
try {
	# Clear and recreate the output directory
	Remove-Item -Path $OutputPath -Recurse -Force -ErrorAction SilentlyContinue -ProgressAction SilentlyContinue

    foreach($server in $buildInfo.servers) {
        BuildServerPackages -server $server -native $false

        if ($buildInfo.includeNative) {
            BuildServerPackages -server $server -native $true
        }
    }

	LogSuccess "`nNuGet packaging completed successfully!"
}
finally {
	Pop-Location
}

exit $exitCode
