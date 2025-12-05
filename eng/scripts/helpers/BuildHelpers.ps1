function Get-OperatingSystems {
    return @(
        @{ Name = 'linux'; NodeName = 'linux'; DotNetName = 'linux'; Extension = '' }
        @{ Name = 'macos'; NodeName = 'darwin'; DotNetName = 'osx'; Extension = '' }
        @{ Name = 'windows'; NodeName = 'win32'; DotNetName = 'win'; Extension = '.exe' }
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
