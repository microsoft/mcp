param(
    $os,
    $Version,
    $PackageArguments
)

# Map .NET runtimes to VSIX target names
$target = (($os -replace "win", "win32") -replace "osx", "darwin")
$vsixName = "vscode-azure-mcp-extension-$target-$Version.vsix"
$ignoreFile = ".vscodeignore"

# Update VSIX version in eng/vscode/package.json
$vsixPackageJsonPath = "./package.json"
if (Test-Path $vsixPackageJsonPath) {
    $packageJson = Get-Content $vsixPackageJsonPath -Raw | ConvertFrom-Json
    $packageJson.version = $Version
    $packageJson | ConvertTo-Json -Depth 100 | Set-Content $vsixPackageJsonPath -NoNewline
    Write-Host "Updated VSIX version in $vsixPackageJsonPath to $Version"
} else {
    Write-Warning "VSIX package.json not found at $vsixPackageJsonPath"
}

# Build the vsce package command
$vsceCmd = "vsce package --target $target --out $vsixName --ignoreFile $ignoreFile"
if ($PackageArguments) {
    $vsceCmd += " $PackageArguments"
}

# Run the packaging step
Invoke-Expression $vsceCmd

# Clean up node_modules
Remove-Item -Recurse -Force node_modules
