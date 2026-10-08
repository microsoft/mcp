#Requires -Version 7

function Get-LatestMarketplaceVersion {
    [CmdletBinding()]
    param(
        [Parameter(Mandatory = $true)]
        [ValidateNotNullOrEmpty()]
        [string]$PublisherId,

        [Parameter(Mandatory = $true)]
        [ValidateNotNullOrEmpty()]
        [string]$ExtensionId,

        [Parameter(Mandatory = $true)]
        [ValidateRange(0, [int]::MaxValue)]
        [int]$MajorVersion
    )

    # IncludeVersions (1) returns the whole history in one unpaged response (~100 KB for ~145 versions across 6
    # platforms). Do not add IncludeLatestVersionOnly (512), which hides every older major series, or
    # IncludeVersionProperties (16), which inflates the payload roughly 12x.
    $marketplaceUrl = "https://marketplace.visualstudio.com/_apis/public/gallery/extensionquery?api-version=7.1-preview.1"
    $body = @{
        filters = @(
            @{
                criteria = @(
                    @{ filterType = 7; value = "$PublisherId.$ExtensionId" }
                )
            }
        )
        flags = 1
    } | ConvertTo-Json -Depth 10

    try {
        $response = Invoke-RestMethod -Uri $marketplaceUrl -Method Post -Body $body -ContentType "application/json" -ErrorAction Stop
    }
    catch {
        throw [InvalidOperationException]::new(
            "Unable to query VS Code Marketplace versions for '$PublisherId.$ExtensionId': $($_.Exception.Message)",
            $_.Exception)
    }

    if ($null -eq $response -or $null -eq $response.PSObject.Properties['results']) {
        throw "VS Code Marketplace returned an invalid response for '$PublisherId.$ExtensionId'."
    }

    $matchingPatches = @(
        foreach ($result in $response.results) {
            foreach ($extension in $result.extensions) {
                foreach ($extensionVersion in $extension.versions) {
                    if ($extensionVersion.version -match "^$MajorVersion\.0\.(\d+)$") {
                        [int]$Matches[1]
                    }
                }
            }
        }
    )

    if ($matchingPatches.Count -eq 0) {
        return $null
    }

    $maxPatch = [int]($matchingPatches | Measure-Object -Maximum).Maximum
    return [PSCustomObject]@{
        LatestVersion = "$MajorVersion.0.$maxPatch"
        MaxPatch = $maxPatch
        NextPatch = $maxPatch + 1
    }
}

function Resolve-PublicVsixVersion {
    [CmdletBinding()]
    param(
        [Parameter(Mandatory = $true)]
        [ValidateNotNullOrEmpty()]
        [string]$ServerName,

        [Parameter(Mandatory = $true)]
        [ValidateNotNullOrEmpty()]
        [string]$ServerVersion,

        [Parameter(Mandatory = $true)]
        [ValidateNotNullOrEmpty()]
        [string]$PackageJsonPath,

        [Parameter(Mandatory = $false)]
        [scriptblock]$MarketplaceVersionProvider = {
            param($PublisherId, $ExtensionId, $MajorVersion)
            Get-LatestMarketplaceVersion -PublisherId $PublisherId -ExtensionId $ExtensionId -MajorVersion $MajorVersion
        }
    )

    $version = [AzureEngSemanticVersion]::ParseVersionString($ServerVersion)
    if (-not $version) {
        throw "Server version '$ServerVersion' is not a supported semantic version."
    }

    $isBetaSeries = $version.Minor -eq 0 -and
        $version.Patch -eq 0 -and
        $version.PrereleaseLabel -eq 'beta'

    if ($isBetaSeries) {
        return [PSCustomObject]@{
            Version = "$($version.Major).$($version.Minor).$($version.PrereleaseNumber)"
            IsPrerelease = $true
            Source = 'BetaMapping'
            MarketplaceLatestVersion = $null
        }
    }

    if ($ServerName -eq 'Fabric.Mcp.Server') {
        return [PSCustomObject]@{
            Version = "$($version.Major).$($version.Minor).$($version.Patch)"
            IsPrerelease = $false
            Source = 'ServerVersion'
            MarketplaceLatestVersion = $null
        }
    }

    if (-not (Test-Path -LiteralPath $PackageJsonPath -PathType Leaf)) {
        throw "VS Code package manifest '$PackageJsonPath' was not found for $ServerName."
    }

    try {
        $packageJson = Get-Content -LiteralPath $PackageJsonPath -Raw | ConvertFrom-Json -AsHashtable
    }
    catch {
        throw [InvalidOperationException]::new(
            "Unable to read VS Code package manifest '$PackageJsonPath': $($_.Exception.Message)",
            $_.Exception)
    }

    $publisherId = $packageJson['publisher']
    $extensionName = $packageJson['name']
    if ([string]::IsNullOrWhiteSpace($publisherId) -or [string]::IsNullOrWhiteSpace($extensionName)) {
        throw "Publisher or extension name was not found in '$PackageJsonPath' for $ServerName."
    }

    $marketplaceInfo = & $MarketplaceVersionProvider $publisherId $extensionName $version.Major
    if ($marketplaceInfo) {
        $nextPatch = 0
        if (-not [int]::TryParse("$($marketplaceInfo.NextPatch)", [ref]$nextPatch) -or $nextPatch -lt 0) {
            throw "Marketplace version provider returned an invalid next patch for '$publisherId.$extensionName'."
        }

        return [PSCustomObject]@{
            Version = "$($version.Major).0.$nextPatch"
            IsPrerelease = $false
            Source = 'Marketplace'
            MarketplaceLatestVersion = $marketplaceInfo.LatestVersion
        }
    }

    $isFirstGaRelease = [string]::IsNullOrEmpty($version.PrereleaseLabel) -and
        $version.Major -eq 1 -and
        $version.Minor -eq 0 -and
        $version.Patch -eq 0

    if ($isFirstGaRelease) {
        return [PSCustomObject]@{
            Version = '1.0.0'
            IsPrerelease = $false
            Source = 'FirstGaRelease'
            MarketplaceLatestVersion = $null
        }
    }

    throw "Cannot determine VSIX version for $ServerName $ServerVersion. No marketplace versions were found for the $($version.Major).0.X series. The 1.0.0 first-GA exception does not apply."
}

function Assert-VsixChangelogVersion {
    [CmdletBinding()]
    param(
        [Parameter(Mandatory = $true)]
        [ValidateNotNullOrEmpty()]
        [string]$ChangelogPath,

        [Parameter(Mandatory = $true)]
        [ValidatePattern('^\d+\.\d+\.\d+$')]
        [string]$VsixVersion,

        [Parameter(Mandatory = $true)]
        [bool]$IsPrerelease
    )

    $content = Get-Content -LiteralPath $ChangelogPath -Raw -ErrorAction Stop
    $latestHeader = [regex]::Match($content, '(?m)^##[ \t]+(?<header>[^\r\n]+)')
    if (-not $latestHeader.Success) {
        throw "VS Code changelog '$ChangelogPath' has no release heading."
    }

    $release = [regex]::Match(
        $latestHeader.Groups['header'].Value,
        '^(?<version>\d+\.\d+\.\d+)[ \t]+\(\d{4}-\d{2}-\d{2}\)(?<prerelease>[ \t]+\(pre-release\))?[ \t]*$')
    if (-not $release.Success) {
        throw "VS Code changelog '$ChangelogPath' must start with a finalized '<major>.<minor>.<patch> (yyyy-MM-dd)' release heading."
    }

    $changelogVersion = $release.Groups['version'].Value
    if ($changelogVersion -ne $VsixVersion) {
        throw "VS Code changelog version '$changelogVersion' does not match VSIX version '$VsixVersion'. Update the latest extension changelog heading to the version in build_info.json before publishing."
    }

    if ($release.Groups['prerelease'].Success -ne $IsPrerelease) {
        throw "VS Code changelog '$ChangelogPath' does not match VSIX release channel (pre-release: $IsPrerelease). Update the latest heading's '(pre-release)' suffix before publishing."
    }
}
