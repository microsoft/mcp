$ErrorActionPreference = 'Stop'
$repoRoot = Join-Path $PSScriptRoot '..' '..' -Resolve

function Invoke($command) {
    Invoke-Expression $command
    if($LASTEXITCODE -ne 0) {
        Write-Host "'$command' exited with code: $LASTEXITCODE." -ForegroundColor Red
        exit $LASTEXITCODE
    }
}

$remotes = Invoke 'git remote show'
if ($remotes -contains 'tools') {
    Invoke 'git remote remove tools'
}

Invoke 'git remote add tools https://github.com/azure/azure-sdk-tools.git'
Invoke 'git fetch tools'

$engCommon = Join-Path $repoRoot 'eng' 'common'

$toolsMain = Invoke 'git rev-parse remotes/tools/main'
$commitDate = Invoke "git show -s --format=%cs $toolsMain"

$currentDirectory = Get-Location
Set-Location $repoRoot
try {
    Remove-Item -Path $engCommon -Recurse -Force -ErrorAction SilentlyContinue
    Invoke "git checkout $toolsMain -- eng/common"
    Invoke 'git add eng/common'

    $partialSha = Invoke "git rev-parse --short $toolsMain"
    Invoke "git commit -m 'Update eng/common from tools repo`n`nUpdated to [$partialSha @ $commitDate](https://github.com/azure/azure-sdk-tools/tree/$toolsMain)'"
    Invoke 'git remote remove tools'
}
finally {
    Set-Location $currentDirectory
}
