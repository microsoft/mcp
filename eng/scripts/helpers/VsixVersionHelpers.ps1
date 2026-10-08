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
    # The gallery API only accepts preview api-versions (GA forms such as 3.0 fail with HTTP 400). 3.0-preview.1 is
    # the version @vscode/vsce, which publishes this VSIX, uses for the same query.
    $marketplaceUrl = "https://marketplace.visualstudio.com/_apis/public/gallery/extensionquery?api-version=3.0-preview.1"
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

    $publishedVersions = @(
        foreach ($result in $response.results) {
            foreach ($extension in $result.extensions) {
                foreach ($extensionVersion in $extension.versions) {
                    $extensionVersion.version
                }
            }
        }
    )

    # List order is not guaranteed, so take the numeric maximum instead of the first match.
    $matchingPatches = @(
        foreach ($publishedVersion in $publishedVersions) {
            if ($publishedVersion -match "^$MajorVersion\.0\.(\d+)$") {
                [int]$Matches[1]
            }
        }
    )

    $maxPatch = $null
    if ($matchingPatches.Count -gt 0) {
        $maxPatch = [int]($matchingPatches | Measure-Object -Maximum).Maximum
    }

    return [PSCustomObject]@{
        ExtensionPublished = $publishedVersions.Count -gt 0
        LatestVersion = $null -eq $maxPatch ? $null : "$MajorVersion.0.$maxPatch"
        MaxPatch = $maxPatch
        NextPatch = $null -eq $maxPatch ? $null : $maxPatch + 1
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
    foreach ($requiredProperty in 'ExtensionPublished', 'NextPatch') {
        if ($null -eq $marketplaceInfo -or -not $marketplaceInfo.PSObject.Properties[$requiredProperty]) {
            throw "Marketplace version provider returned an invalid result for '$publisherId.$extensionName': '$requiredProperty' is missing."
        }
    }

    if ($null -ne $marketplaceInfo.NextPatch) {
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

    # Only an extension that has never been published may start at 1.0.0. An extension with other published
    # versions but no history for this series indicates a gap that must be investigated, not guessed.
    if ($isFirstGaRelease -and -not $marketplaceInfo.ExtensionPublished) {
        return [PSCustomObject]@{
            Version = '1.0.0'
            IsPrerelease = $false
            Source = 'FirstGaRelease'
            MarketplaceLatestVersion = $null
        }
    }

    throw "Cannot determine VSIX version for $ServerName $ServerVersion. No marketplace versions were found for the $($version.Major).0.X series. The first-GA exception (server version 1.0.0) applies only when the extension has no published versions."
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
