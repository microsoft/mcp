#!/bin/env pwsh
#Requires -Version 7

param(
    [string] $Command = 'azmcp tools list'
)

. "$PSScriptRoot/../common/scripts/common.ps1"

$combinations = @()
@($false, $true) | ForEach-Object {
    $SingleFile = $_
    @($false, $true) | ForEach-Object {
        $Trimmed = $_
        @($false, $true) | ForEach-Object {
            $ReadyToRun = $_
            $combinations += @{
                Name = "$($Trimmed ? 'aot' : 'no-aot')/$($ReadyToRun ? 'r2r' : 'no-r2r')"
                Trimmed = $Trimmed
                ReadyToRun = $ReadyToRun
                SingleFile = $SingleFile
            }
        }
    }
}

function SaveResults() {
    $csv = @("Trimmed,ReadyToRun,Compilation,Installation,First Run,Average Run,Tgz Size,Package Size")

    foreach($combination in $combinations) {
        $result = $results[$combination.Name]

        $compilation = ($result | Measure-Object -Property Compilation -Average).Average
        $installation = ($result | Measure-Object -Property Installation -Average).Average
        $firstRun = ($result | Measure-Object -Property FirstRun -Average).Average
        $averageRun = ($result | Measure-Object -Property AverageRun -Average).Average
        $tgzSize = ($result | Measure-Object -Property TgzSize -Average).Average
        $packageSize = ($result | Measure-Object -Property PackageSize -Average).Average

        $csv += "$($combination.Trimmed),$($combination.ReadyToRun),$compilation,$installation,$firstRun,$averageRun,$tgzSize,$packageSize"
    }

    $csv | Out-File -FilePath .work/results.csv -Encoding utf8 -Force
}

$results = @{};

Push-Location $RepoRoot
try {
    $version = & "./eng/scripts/Get-Version.ps1"
    $rid = [System.Runtime.InteropServices.RuntimeInformation]::RuntimeIdentifier
    $nodeRid = $rid.Replace('win', 'win32').Replace('osx', 'darwin')

    foreach($combination in $combinations) {
        $results[$combination.Name] = @()

        Write-Host "Building '$($combination.Name)'"
        Write-Host "-------------------------------"

        Remove-Item -Path .work -Recurse -Force -ErrorAction SilentlyContinue -ProgressAction SilentlyContinue
        Remove-Item -Path .dist -Recurse -Force -ErrorAction SilentlyContinue -ProgressAction SilentlyContinue
        Remove-Item -Path ./src/bin -Recurse -Force -ErrorAction SilentlyContinue -ProgressAction SilentlyContinue
        Remove-Item -Path ./src/obj -Recurse -Force -ErrorAction SilentlyContinue -ProgressAction SilentlyContinue

        $start = Get-Date
        ./eng/scripts/Build-Local.ps1 -SelfContained:$true -Trimmed:$combination.Trimmed -ReadyToRun:$combination.ReadyToRun -UsePaths:$true -AllPlatforms:$false
        $compilation = ((Get-Date) - $start).TotalMilliseconds

        for($i = 1; $i -le 3; $i++) {
            Write-Host "Running '$($combination.Name)' - Round $i"
            Write-Host "----------------------------------------"
            npm uninstall -g azmcp | Out-Null

            $start = Get-Date
            Write-Host "> npm install -g .dist/wrapper/azure-mcp-$version.tgz"
            npm install -g ".dist/wrapper/azure-mcp-$version.tgz" | Out-Null
            $installation = ((Get-Date) - $start).TotalMilliseconds

            $runs = @()
            Write-Host "Running ..."
            foreach($x in 1..6) {
                $start = Get-Date
                Invoke-Expression $Command | Out-Null
                if ($LASTEXITCODE -ne 0) {
                    Write-Warning "$($combination.Name) failed with exit code $LASTEXITCODE"
                }
                $runs += ((Get-Date) - $start).TotalMilliseconds
            }

            $tgzSize = (Get-Item -Path ".dist/platform/azure-mcp-$nodeRid-$version.tgz").Length / 1MB
            $packageSize = (Get-ChildItem -Path ".work/platform/$rid" -File -Recurse | Measure-Object -Property Length -Sum).Sum / 1MB

            $results[$combination.Name] += @{
                Compilation = $compilation
                Installation = $installation
                FirstRun = $runs[0]
                AverageRun = ($runs | Select-Object -Skip 3 | Measure-Object -Average).Average
                TgzSize = $tgzSize
                PackageSize = $packageSize
            }
        }

        SaveResults
    }
}
finally {
    Pop-Location
}
