. "$PSScriptRoot/../common/scripts/common.ps1"
$RepoRoot = $RepoRoot.Path.Replace('\', '/')

# Set of files that don't require build or test when changed
$skipFiles = @(
    'CHANGELOG.md',
    'README.md',
    'SUPPORT.md',
    'TROUBLESHOOTING.md',
    'CONTRIBUTING.md',
    'CODE_OF_CONDUCT.md',
    'SECURITY.md',
    'NOTICE.txt',
    'LICENSE'
)

# Set of directories that don't require build or test when changed
$skipDirectories = @(
    '.github/',
    'eng/images/',
    'docs',
    'Resources'
)

$skipBuildAndTest = $true

$changedFilesScriptPath = "$RepoRoot/eng/common/scripts/get-changedfiles.ps1"
$changedFiles = & $changedFilesScriptPath -DiffFilterType ''

if ($changedFiles) {
    foreach ($file in $changedFiles) {
        Write-Host "Checking file: $file"
        $fileName = [System.IO.Path]::GetFileName($file)
        $isSkipFile = $skipFiles -contains $fileName

        $isInSkipDirectory = $false
        foreach ($dir in $skipDirectories) {
            if ($file.StartsWith($dir, [System.StringComparison]::OrdinalIgnoreCase)) {
                $isInSkipDirectory = $true
                break
            }
        }

        Write-Host "  -> File: $fileName, IsSkipFile: $isSkipFile, IsInSkipDir: $isInSkipDirectory"
        if (-not $isSkipFile -and -not $isInSkipDirectory) {
            $skipBuildAndTest = $false
            Write-Host "  -> File: $file requires build and test!"
            return
        }
    }
}

if ($skipBuildAndTest) {
    Write-Host "##vso[task.setvariable variable=SkipBuildJob;isOutput=true]true"
    Write-Host "##vso[task.setvariable variable=SkipLiveTestJob;isOutput=true]true"
    Write-Host "SkipBuildJob set to: true"
    Write-Host "SkipLiveTestJob set to: true"
} else {
    Write-Host "This PR contains files that require build and/or test."
}