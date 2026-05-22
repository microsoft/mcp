#!/bin/env pwsh
#Requires -Version 7

param(
    [parameter(Mandatory)]
    [string] $ServerName,
    [int] $RunCount = 3,
    [int] $InvocationsPerRun = 10
)

. "$PSScriptRoot/../common/scripts/common.ps1"

$combinations = @()

$combinations += @{
    Name = "plain/no-self"
}

$combinations += @{
    Name = "trimmed/no-self"
}

$combinations += @{
    Name = "trimmed r2r/no-self"
    Trimmed = $true
    ReadyToRun = $true
}

$combinations += @{
    Name = "native"
    Native = $true
}

$combinations += @{
    Name = "native r2r"
    Native = $true
    ReadyToRun = $true
}

@($false, $true) | ForEach-Object {
    $SingleFile = $_
    @($false, $true) | ForEach-Object {
        $Trimmed = $_
        @($false, $true) | ForEach-Object {
            $ReadyToRun = $_
            $combinations += @{
                Name = "$($Trimmed ? 'trim' : 'no-trim')/$($ReadyToRun ? 'r2r' : 'no-r2r')/$($SingleFile ? 'single' : 'no-single')"
                SelfContained = $true
                Trimmed = $Trimmed
                ReadyToRun = $ReadyToRun
                SingleFile = $SingleFile
            }
        }
    }
}

function SaveResults() {
    $csv = @("Name,Compilation,First Run,Average Run,Package Size")

    foreach($combination in $combinations) {
        $result = $results[$combination.Name]

        $compilation = ($result | Measure-Object -Property Compilation -Average).Average
        $firstRun = ($result | Measure-Object -Property FirstRun -Average).Average
        $averageRun = ($result | Measure-Object -Property AverageRun -Average).Average
        $packageSize = ($result | Measure-Object -Property PackageSize -Average).Average

        $csv += "$($combination.Name),$compilation,$firstRun,$averageRun,$packageSize"
    }

    $csv | Out-File -FilePath .work/results.csv -Encoding utf8 -Force
}

$results = @{};

Push-Location $RepoRoot
try {
    $rid = [System.Runtime.InteropServices.RuntimeInformation]::RuntimeIdentifier
    $platformName = $rid.Replace('win', 'windows').Replace('osx', 'macos')

    foreach($combination in $combinations) {
        $results[$combination.Name] = @()
        $outputPath = ".work/build/$ServerName/$platformName"
        Write-Host "Building '$($combination.Name)' to '$outputPath' ..."
        Write-Host "-----------------------------------------------------------------------------------"

        Remove-Item -Path .work -Recurse -Force -ErrorAction SilentlyContinue -ProgressAction SilentlyContinue
        Remove-Item -Path .dist -Recurse -Force -ErrorAction SilentlyContinue -ProgressAction SilentlyContinue
        Remove-Item -Path ./src/bin -Recurse -Force -ErrorAction SilentlyContinue -ProgressAction SilentlyContinue
        Remove-Item -Path ./src/obj -Recurse -Force -ErrorAction SilentlyContinue -ProgressAction SilentlyContinue

        $start = Get-Date

        ./eng/scripts/Build-Code.ps1 `
            -ServerName $ServerName `
            -SelfContained:(!!$combination.SelfContained) `
            -Trimmed:(!!$combination.Trimmed) `
            -ReadyToRun:(!!$combination.ReadyToRun) `
            -SingleFile:(!!$combination.SingleFile) `
            -Native:(!!$combination.Native)

        $compilation = ((Get-Date) - $start).TotalMilliseconds

        $executable = Get-ChildItem -Path $outputPath -Filter "*.exe" | Select-Object -First 1

        if(-not $executable) {
            Write-Warning "No executable found for '$($combination.Name)' in '$outputPath'"
            continue
        }

        $command = "$($executable.FullName) tools list"

        for($i = 1; $i -le $RunCount; $i++) {
            Write-Host "Running '$($combination.Name)' - Round $i"
            Write-Host "----------------------------------------"

            $start = Get-Date

            $runs = @()
            Write-Host "Running $command"
            foreach($x in 1..$InvocationsPerRun) {
                $start = Get-Date
                Invoke-Expression $command | Out-Null
                if ($LASTEXITCODE -ne 0) {
                    Write-Warning "$($combination.Name) failed with exit code $LASTEXITCODE"
                }
                $runs += ((Get-Date) - $start).TotalMilliseconds
            }

            $packageSize = (Get-ChildItem -Path $executable.DirectoryName -File -Recurse | Measure-Object -Property Length -Sum).Sum / 1MB

            $results[$combination.Name] += @{
                Compilation = $compilation
                FirstRun = $runs[0]
                AverageRun = ($runs | Select-Object -Skip 2 | Measure-Object -Average).Average
                PackageSize = $packageSize
            }
        }

        SaveResults
    }
}
finally {
    Pop-Location
}
