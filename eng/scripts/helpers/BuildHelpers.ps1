function Get-OperatingSystems {
    return @(
        @{ name = 'linux'; nodeName = 'linux'; dotnetName = 'linux'; extension = '' }
        @{ name = 'macos'; nodeName = 'darwin'; dotnetName = 'osx'; extension = '' }
        @{ name = 'windows'; nodeName = 'win32'; dotnetName = 'win'; extension = '.exe' }
    )
}

function Get-RepoRelativePath {
    [CmdletBinding()]
    param(
        [parameter(Mandatory, ValueFromPipeline)]
        [string] $Path,
        [switch] $NormalizeSeparators
    )

    process {
        $root = Resolve-Path (Join-Path $PSScriptRoot ".." ".." "..")
        $relativePath = Resolve-Path -LiteralPath $Path -Relative -RelativeBasePath $root

        # trim the leading ./
        if ($relativePath.StartsWith('./') -or $relativePath.StartsWith('.\')) {
            $relativePath = $relativePath.Substring(2)
        }

        $NormalizeSeparators ? $relativePath.Replace('\', '/') : $relativePath
    }
}

<#
.SYNOPSIS
Gets the repository platform name for the current operating system and architecture.

.DESCRIPTION
Combines PowerShell's current operating-system indicator with the architecture
from the .NET runtime identifier. The result uses the same `<os>-<architecture>`
format as platform names in `build_info.json`, such as `windows-x64` or
`linux-arm64`.

.OUTPUTS
System.String
The current platform name.

.EXAMPLE
$platformName = Get-PlatformName

Returns a value such as `windows-x64`, `linux-x64`, or `macos-arm64`.

.NOTES
Throws when the operating system is not Windows, Linux, or macOS.
#>
function Get-PlatformName {
    [string]$fullPlatform = ""

    if ($IsWindows) {
        $fullPlatform = "windows"
    } elseif ($IsLinux) {
        $fullPlatform = "linux"
    } elseif ($IsMacOS) {
        $fullPlatform = "macos"
    } else {
        throw "Unsupported platform"
    }

    $currentArch = [System.Runtime.InteropServices.RuntimeInformation]::RuntimeIdentifier.Split('-')[1]
    $fullPlatform += "-$currentArch"

    return $fullPlatform
}
