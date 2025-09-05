[CmdletBinding()]
param(
    [Parameter(Mandatory=$false)]
    [string] $PackagesDirectory,
    [Parameter(Mandatory=$false)]
    [string] $ServerName,
    [string] $VersionSuffix,
    [string] $FeedUrl
)
. "$PSScriptRoot/../common/scripts/common.ps1"
$projectPropertiesScript = "$RepoRoot/eng/scripts/Get-ProjectProperties.ps1"

$serverProjectProperties = & "$projectPropertiesScript" -ProjectName "$($ServerName -replace '-native', '').csproj"
$packagesVersion = "$($serverProjectProperties.Version)$VersionSuffix"

$ErrorActionPreference = "Stop"
$nupkgFiles = Get-ChildItem -Path $PackagesDirectory -Filter *.nupkg | Where-Object { $_.Name -notlike '*.symbols.nupkg' }
if (-not $nupkgFiles) {
  LogWarning "No NuGet packages found in $PackagesDirectory"
  exit 1
}
if ($FeedUrl) {
    dotnet nuget add source $FeedUrl
}
$maxRetries = 12
$delaySeconds = 5
foreach ($nupkg in $nupkgFiles) {
  $packageName = [System.IO.Path]::GetFileNameWithoutExtension($nupkg.Name) -replace "\.$packagesVersion$",""
  $found = $false
  for ($i = 1; $i -le $maxRetries; $i++) {
    LogInfo "Checking for package $packageName in feed (attempt $i of $maxRetries)..."
    try {
      $search = nuget list $packageName -Prerelease
      if ($search -match $packageName -and $search -match $packagesVersion) {
        LogSuccess "Package $packageName found in feed."
        $found = $true
        break
      }
    } catch {
      LogError "Error querying feed: $_"
    }
    LogInfo "Package $packageName not found yet. Waiting $delaySeconds seconds before retrying..."
    Start-Sleep -Seconds $delaySeconds
  }
  if (-not $found) {
    LogError "Package $packageName was not found in the feed after $($maxRetries * $delaySeconds) seconds."
    exit 1
  }
}