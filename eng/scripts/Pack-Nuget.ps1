#!/bin/env pwsh
#Requires -Version 7

param(
	[Parameter(Mandatory=$false)]
	[string] $ArtifactsPath,
	[Parameter(Mandatory=$false)]
	[string] $VersionSuffix,
	[Parameter(Mandatory=$false)]
	[string] $OutputPath,
	[Parameter(Mandatory=$false)]
	[string] $RepoUrl,
	[Parameter(Mandatory=$false)]
	[string] $CommitSha,
	[Parameter(Mandatory=$false)]
	[string] $Branch
)

. "$PSScriptRoot/../common/scripts/common.ps1"
$RepoRoot = $RepoRoot.Path.Replace('\', '/')

$mcpServerjson = "$RepoRoot/eng/dnx/.mcp/server.json"
$nuspecSourcePath = "$RepoRoot/eng/dnx/nuspec"
$azureIconPath = "$RepoRoot/eng/images/azureicon.png"
$projectPropertiesScript = "$RepoRoot/eng/scripts/Get-ProjectProperties.ps1"

if(!$ArtifactsPath) {
	$ArtifactsPath = "$RepoRoot/.work/build"
}

if(!$OutputPath) {
	$OutputPath = "$RepoRoot/.work/package"
}

if(!(Test-Path $ArtifactsPath)) {
	LogError "Artifacts path $ArtifactsPath does not exist."
	return
}

$sharedProjectProperties = & "$projectPropertiesScript" -ProjectName "Directory.Build.props"

Push-Location $RepoRoot
try {
	# Clear and recreate the output directory
	Remove-Item -Path $OutputPath -Recurse -Force -ErrorAction SilentlyContinue -ProgressAction SilentlyContinue

	$wrapperJsonFiles = Get-ChildItem -Path $ArtifactsPath -Filter "wrapper.json" -Recurse
	foreach($wrapperJsonFile in $wrapperJsonFiles) {
		$serverDirectory = $wrapperJsonFile.Directory
		$serverName = $serverDirectory.Name -replace '-native', ''
		$serverProjectProperties = & "$projectPropertiesScript" -ProjectName "$serverName.csproj"
		$platformOutputPath = "$OutputPath/nuget/$($serverDirectory.Name)/platform"
		$wrapperOutputPath = "$OutputPath/nuget/$($serverDirectory.Name)/wrapper"

		New-Item -ItemType Directory -Force -Path $platformOutputPath | Out-Null
		New-Item -ItemType Directory -Force -Path $wrapperOutputPath | Out-Null

		$wrapperPackageJson = Get-Content $wrapperJsonFile -Raw | ConvertFrom-Json -AsHashtable

		$platformDirectories = Get-ChildItem -Path $serverDirectory -Directory

		# Create dnx wrapper nuget tool
        $tempNugetWrapperDir = Join-Path $wrapperOutputPath "temp"
        $wrapperToolDir = "$tempNugetWrapperDir/tools/$($sharedProjectProperties.TargetFramework)/any"
        $wrapperToolNuspec = "$tempNugetWrapperDir/$($serverProjectProperties.PackageId).nuspec"
        New-Item -ItemType Directory -Force -Path $wrapperToolDir | Out-Null
        New-Item -ItemType Directory -Force -Path "$tempNugetWrapperDir/.mcp" | Out-Null

		$packageVersion = "$($serverProjectProperties.Version)$VersionSuffix"
		$releaseTag = "$serverName-$packageVersion"
        
        (Get-Content -Path "$nuspecSourcePath/RuntimeAgnosticToolSettingsTemplate.xml") `
            -replace "__CommandName__", $serverProjectProperties.CliName |
            Set-Content -Path "$wrapperToolDir/DotnetToolSettings.xml"

        (Get-Content -Path $mcpServerjson -Raw) `
            -replace '\$\(PackageDescription\)', $serverProjectProperties.Description `
            -replace '\$\(PackageId\)', $serverProjectProperties.PackageId `
            -replace '\$\(PackageVersion\)', $packageVersion `
            -replace '\$\(ServerName\)', $serverProjectProperties.CliName `
            -replace '\$\(RepositoryUrl\)', $RepoUrl |
            Set-Content -Path "$tempNugetWrapperDir/.mcp/server.json"
        
        (Get-Content -Path "$nuspecSourcePath/RuntimeAgnosticTemplate.nuspec") `
            -replace "__Id__", $serverProjectProperties.PackageId `
            -replace "__Version__", $packageVersion `
            -replace "__Authors__", $wrapperPackageJson.author `
            -replace "__Description__", $wrapperPackageJson.description `
            -replace "__Tags__", ($serverProjectProperties.PackageTags -replace ';', ' ').Trim() `
            -replace "__ProjectUrl__", "$RepoUrl/tree/$releaseTag/servers/$serverName" `
			-replace "__ReleaseNotes__", "$RepoUrl/tree/$releaseTag/servers/$serverName/CHANGELOG.md" `
            -replace "__RepositoryUrl__", $RepoUrl `
            -replace "__RepositoryBranch__", $Branch `
            -replace "__CommitSHA__", $CommitSha `
            -replace "__TargetFramework__", $sharedProjectProperties.TargetFramework |
            Set-Content -Path $wrapperToolNuspec
        Copy-Item -Path "$nuspecSourcePath/README.md" -Destination $tempNugetWrapperDir -Force
		Copy-Item -Path $azureIconPath -Destination $tempNugetWrapperDir -Force

		# Build the project
		foreach ($platformDirectory in $platformDirectories) {
			$platformOSArch = $platformDirectory.Name
			$tempPlatformDir = Join-Path $platformOutputPath $platformOSArch
			$platformToolDir = "$tempPlatformDir/tools/any/$platformOSArch"
			$platformPackageId = "$($serverProjectProperties.PackageId).$platformOSArch"
			$platformNuspecFile = "$tempPlatformDir/$platformPackageId.nuspec"
			New-Item -ItemType Directory -Force -Path $platformToolDir | Out-Null

			Copy-Item -Path "$platformDirectory/dist/*" -Destination $platformToolDir -Recurse -Force
			Copy-Item -Path $azureIconPath -Destination $tempPlatformDir -Force
			$platformToolEntryPoint = (
				Get-ChildItem -Path $platformToolDir -Filter "$($serverProjectProperties.CliName)*" -Recurse |
				Where-Object { $_.PSIsContainer -eq $false -and ($_.Extension -eq ".exe" -or $_.Extension -eq "") } |
				Select-Object -First 1
			).Name
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

		if ((Get-Content -Raw -Path $wrapperToolNuspec) + (Get-Content -Raw -Path "$wrapperToolDir/DotnetToolSettings.xml") -match '__') {
			Write-Error "Placeholder(s) with '__' still found in $wrapperToolNuspec or DotnetToolSettings.xml. Please check your replacements."
			exit 1
		}

		LogInfo "Creating Nuget Package from $wrapperToolNuspec"
        Invoke-LoggedCommand "nuget pack '$wrapperToolNuspec' -OutputDirectory '$wrapperOutputPath'" -GroupOutput
        Remove-Item -Path $tempNugetWrapperDir -Recurse -Force -ErrorAction SilentlyContinue -ProgressAction SilentlyContinue
	}
	LogSuccess "`nNuGet packaging completed successfully!" -ForegroundColor Green
}
finally {
	Pop-Location
}