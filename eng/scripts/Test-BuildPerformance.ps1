#!/bin/env pwsh
#Requires -Version 7

param(
    [parameter(Mandatory)]
    [string] $ServerName
)

. "$PSScriptRoot/../common/scripts/common.ps1"

$combinations = @()

$combinations += @{
    Name = "dependent/no-r2r/no-single"
}

$combinations += @{
    Name = "dependent/no-r2r/single"
    SingleFile = $true
}

$combinations += @{
    Name = "dependent/r2r/no-single"
    ReadyToRun = $true
}

$combinations += @{
    Name = "dependent/r2r/single"
    ReadyToRun = $true
    SingleFile = $true
}

$combinations += @{
    Name = "native/no-r2r"
    Native = $true
}

$combinations += @{
    Name = "native/r2r"
    Native = $true
    ReadyToRun = $true
}

@($false, $true) | ForEach-Object {
    $Trimmed = $_
    @($false, $true) | ForEach-Object {
        $ReadyToRun = $_
        @($false, $true) | ForEach-Object {
            $SingleFile = $_
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
    $csv = @("Name,Self Contained,Trimmed,Single File,Ready To Run,Native,Compilation,Cold Run,Warm Run,Package Size")

    foreach($combination in $combinations) {
        $name = $combination.Name
        $singleFile = !!$combination.SingleFile
        $trimmed = !!$combination.Trimmed
        $selfContained = !!$combination.SelfContained
        $readyToRun = !!$combination.ReadyToRun
        $native = !!$combination.Native

        $result = $results[$name]

        $compilation = $result.Compilation
        $coldRun = $result.ColdRun
        $warmRun = $result.WarmRun
        $packageSize = $result.PackageSize

        $csv += "$name,$selfContained,$trimmed,$singleFile,$readyToRun,$native,$compilation,$coldRun,$warmRun,$packageSize"
    }

    New-Item -ItemType Directory -Path .work -Force | Out-Null
    $csv | Out-File -FilePath .work/results.csv -Encoding utf8 -Force
}

$results = @{};

$ballastSize = 1gb
$ballastFile = ".work/ballast.txt"
function Write-ballastFile {
    New-Item -ItemType Directory -Path .work -Force | Out-Null
    Remove-Item -Path $ballastFile -Force -ErrorAction SilentlyContinue
    # Create a large ballast file to clear the io cache
    $ballastRemaining = $ballastSize
    $gitPackFiles = Get-ChildItem .git/objects/pack -File
    while ($ballastRemaining -gt 0) {
        foreach ($file in $gitPackFiles) {
            $chunk = [IO.File]::ReadAllBytes($file.FullName)
            # trim the chunk if necessary
            if ($ballastRemaining -lt $chunk.Length) {
                [Array]::Resize([ref]$chunk, [int]$ballastRemaining)
            }
            [IO.File]::AppendAllBytes($ballastFile, $chunk)
            $ballastRemaining -= $chunk.Length
            if ($ballastRemaining -le 0) {
                break
            }
        }
    }
}

Push-Location $RepoRoot
try {
    $rid = [System.Runtime.InteropServices.RuntimeInformation]::RuntimeIdentifier
    $platformName = $rid.Replace('win', 'windows').Replace('osx', 'macos')

    Write-ballastFile

    foreach($combination in $combinations) {
        $results[$combination.Name] = @()
        $outputPath = ".work/build/$ServerName/$platformName"
        Write-Host "Building '$($combination.Name)' to '$outputPath' ..."
        Write-Host "-----------------------------------------------------------------------------------"

        Remove-Item -Path $outputPath -Recurse -Force -ErrorAction SilentlyContinue -ProgressAction SilentlyContinue

        # Remove bin and obj directories anywhere in the repo
        Get-ChildItem . -Directory -Recurse
        | Where-Object Name -Match '^(bin|obj)$'
        | Remove-Item -Recurse -Force -ErrorAction SilentlyContinue -ProgressAction SilentlyContinue

        $start = Get-Date

        Write-Host @"
./eng/scripts/Build-Code.ps1 ``
    -ReleaseBuild ``
    -ServerName '$ServerName' ``
    -SelfContained:`$$(!!$combination.SelfContained) ``
    -Trimmed:`$$(!!$combination.Trimmed) ``
    -ReadyToRun:`$$(!!$combination.ReadyToRun) ``
    -SingleFile:`$$(!!$combination.SingleFile) ``
    -Native:`$$(!!$combination.Native)
"@

        ./eng/scripts/Build-Code.ps1 `
            -ReleaseBuild `
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

        Write-Host "Running '$($combination.Name)'"
        Write-Host "----------------------------------------"

        $start = Get-Date

        function measureRun {
            $start = Get-Date
            & $executable 'tools' 'list' | Out-Null
            if ($LASTEXITCODE -ne 0) {
                Write-Warning "$($combination.Name) failed with exit code $LASTEXITCODE"
                return $null
            }
            return ((Get-Date) - $start).TotalMilliseconds
        }

        $coldRuns = @()
        $warmRuns = @()
        foreach($i in 1..3) {
            # Read the ballast file to clear the io cache
            Write-Host "Reading ballast file..."
            [IO.File]::ReadAllBytes($ballastFile) | Out-Null
            Write-Host "Cold run $i.0`: $command"
            $coldRun = measureRun
            if (!$coldRun) {
                Write-Warning "Failed to measure first run for $($combination.Name)"
                break
            }
            $coldRuns += $coldRun

            foreach($x in 1..5) {
                Write-Host "Warm run $i.$x`: $command"
                $warmRun = measureRun
                if (!$warmRun) {
                    Write-Warning "Failed to measure warm run for $($combination.Name)"
                    break
                }
                $warmRuns += $warmRun
            }
        }

        $packageSize = (Get-ChildItem -Path $executable.DirectoryName -File -Recurse | Measure-Object -Property Length -Sum).Sum / 1MB

        $results[$combination.Name] = @{
            Compilation = $compilation
            ColdRun = ($coldRuns | Measure-Object -Average).Average
            WarmRun = ($warmRuns | Measure-Object -Average).Average
            PackageSize = $packageSize
        }

        SaveResults

        # rename the output path to preserve it
        $newOutputPath = ".work/build-old/$($combination.Name -replace '\W', '_')"
        Remove-Item $newOutputPath -Recurse -Force -ErrorAction SilentlyContinue -ProgressAction SilentlyContinue
        New-Item -ItemType Directory -Path $newOutputPath -Force | Out-Null
        Move-Item -Path $outputPath/* -Destination $newOutputPath
        Remove-Item $outputPath -Recurse -Force -ErrorAction SilentlyContinue -ProgressAction SilentlyContinue
    }
}
finally {
    Pop-Location
}
