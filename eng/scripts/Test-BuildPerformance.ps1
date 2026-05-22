#!/bin/env pwsh
#Requires -Version 7

param(
    [parameter(Mandatory)]
    [string] $ServerName,
    [int] $RunCount = 10
)

. "$PSScriptRoot/../common/scripts/common.ps1"

$combinations = @()

$combinations += @{
    Name = "plain/no-self"
}

$combinations += @{
    Name = "single/no-self"
    SingleFile = $true
}

$combinations += @{
    Name = "r2r/no-self"
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
    $csv = @("Name,Self Contained,Trimmed,Single File,Ready To Run,Native,Compilation,First Run,Average Run,Package Size")

    foreach($combination in $combinations) {
        $name = $combination.Name
        $singleFile = !!$combination.SingleFile
        $trimmed = !!$combination.Trimmed
        $selfContained = !!$combination.SelfContained
        $readyToRun = !!$combination.ReadyToRun
        $native = !!$combination.Native

        $result = $results[$name]

        $compilation = $result.Compilation
        $firstRun = $result.FirstRun
        $averageRun = $result.AverageRun
        $packageSize = $result.PackageSize

        $csv += "$name,$selfContained,$trimmed,$singleFile,$readyToRun,$native,$compilation,$firstRun,$averageRun,$packageSize"
    }

    New-Item -ItemType Directory -Path .work -Force
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

        Remove-Item -Path .work/build -Recurse -Force -ErrorAction SilentlyContinue -ProgressAction SilentlyContinue

        # Remove bin and obj directories anywhere in the repo
        Get-ChildItem . -Directory -Recurse
        | Where-Object Name -Match '^(bin|obj)$'
        | Remove-Item -Recurse -Force -ErrorAction SilentlyContinue -ProgressAction SilentlyContinue

        $start = Get-Date

        Write-Host @"
./eng/scripts/Build-Code.ps1 ``
    -ServerName '$ServerName' ``
    -SelfContained:`$$(!!$combination.SelfContained) ``
    -Trimmed:`$$(!!$combination.Trimmed) ``
    -ReadyToRun:`$$(!!$combination.ReadyToRun) ``
    -SingleFile:`$$(!!$combination.SingleFile) ``
    -Native:`$$(!!$combination.Native)
"@

        ./eng/scripts/Build-Code.ps1 `
            -ServerName $ServerName `
            -SelfContained:(!!$combination.SelfContained) `
            -Trimmed:(!!$combination.Trimmed) `
            -ReadyToRun:(!!$combination.ReadyToRun) `
            -SingleFile:(!!$combination.SingleFile) `
            -Native:(!!$combination.Native)

        $compilation = ((Get-Date) - $start).TotalMilliseconds

        # Remove pdb files
        Get-ChildItem -Path $outputPath -Filter "*.pdb"
        | Remove-Item -Force -ErrorAction SilentlyContinue -ProgressAction SilentlyContinue

        $executable = Get-ChildItem -Path $outputPath -Filter "*.exe" | Select-Object -First 1

        if(-not $executable) {
            Write-Warning "No executable found for '$($combination.Name)' in '$outputPath'"
            continue
        }

        $command = "$($executable.FullName) tools list"

        Write-Host "Running '$($combination.Name)' - Round $i"
        Write-Host "----------------------------------------"

        $start = Get-Date

        $runs = @()
        Write-Host "Running $command"
        foreach($x in 1..$RunCount) {
            $start = Get-Date
            Invoke-Expression $command | Out-Null
            if ($LASTEXITCODE -ne 0) {
                Write-Warning "$($combination.Name) failed with exit code $LASTEXITCODE"
            }
            $runs += ((Get-Date) - $start).TotalMilliseconds
        }

        $packageSize = (Get-ChildItem -Path $executable.DirectoryName -File -Recurse | Measure-Object -Property Length -Sum).Sum / 1MB

        $results[$combination.Name] = @{
            Compilation = $compilation
            FirstRun = $runs[0]
            AverageRun = ($runs | Select-Object -Skip 2 | Measure-Object -Average).Average
            PackageSize = $packageSize
        }

        SaveResults
    }
}
finally {
    Pop-Location
}
