[CmdletBinding()]
param(
    [Parameter(Mandatory)]
    [string] $ArtifactsPath
)

$ErrorActionPreference = "Stop"
. "$PSScriptRoot/../common/scripts/common.ps1"

$RepoRoot = $RepoRoot.Path.Replace('\', '/')
$exitCode = 0

if (!(Test-Path $ArtifactsPath)) {
    LogError "Artifacts path $ArtifactsPath does not exist."
    exit 1
}

Write-Host "Verifying VSIX signing using the vsce npm module..."

$vsixToolsDirectory = "$RepoRoot/eng/vsix-tools"

$originalLocation = Get-Location
Set-Location $vsixToolsDirectory
try {
    Write-Host "Installing npm packages"
    Invoke-LoggedCommand 'npm ci --omit=optional'

    $vsixPaths = Get-ChildItem -Path $ArtifactsPath -Filter *.vsix -Recurse | Select-Object -ExpandProperty FullName
    foreach ($vsixPath in $vsixPaths) {
        Write-Host "Verifying VSIX signature for $vsixPath"

        $manifestPath = [System.IO.Path]::ChangeExtension($vsixPath, ".manifest")
        $p7sPath = [System.IO.Path]::ChangeExtension($vsixPath, ".signature.p7s")
        
        Invoke-LoggedCommand "npx --no @vscode/vsce verify-signature --packagePath '$vsixPath' --manifestPath '$manifestPath' --signaturePath '$p7sPath'" -DoNotExitOnFailedExitCode | Tee-Object -Variable output
        # $output is an array of output lines; check if any line indicates success
        $succeeded = $output -contains 'Signature verification result: Success'

        if ($LASTEXITCODE -ne 0 -or !$succeeded) {
            LogError "VSIX signature verification failed for $vsixPath"
            $exitCode = $LASTEXITCODE
        } else {
            Write-Host "VSIX signature verification succeeded for $vsixPath"
        }
    }
}
finally {
    Set-Location $originalLocation
}

exit $exitCode
