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
