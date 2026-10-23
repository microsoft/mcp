param(
    [string]$Suffix
)

# if $suffix wasn't provided, default to "after"
if (-not $Suffix) {
    $Suffix = "after"
}

$repoRoot = (Resolve-Path "$PSScriptRoot/../..").Path
New-Item -Path "$repoRoot/.work" -ItemType Directory -ErrorAction SilentlyContinue | Out-Null

$originalLocation = Get-Location
Set-Location $repoRoot

try
{
    $serverDirectories = Get-ChildItem -Path "$repoRoot/servers" -Directory

    foreach ($serverDirectory in $serverDirectories) {
        $name = $serverDirectory.Name.ToLower() -replace '.mcp.server', ''
        $outputFile = "$repoRoot/.work/$name-tools-$Suffix.json"
        Remove-Item -Path $outputFile -ErrorAction SilentlyContinue -ProgressAction SilentlyContinue

        dotnet build "$serverDirectory/src"
        if ($LASTEXITCODE -ne 0) {
            Write-Warning "Build failed with exit code $LASTEXITCODE"
            continue
        }

        $toolsList = dotnet run --project "$serverDirectory/src" --no-build -- tools list
        if ($LASTEXITCODE -ne 0) {
            Write-Warning "Unable to get tools list for $name, skipping. dotnet exited with code $LASTEXITCODE"
            continue
        }

        # Remove header text up to the first json object
        $pastHeader = $false
        $outLines = @()
        foreach ($line in $toolsList) {
            if (!$pastHeader) {
                if ($line -match '^\s*\{') {
                    $pastHeader = $true
                } else {
                    continue
                }
            }

            $outLines += $line
        }

        # Parse, sort options within each tool by name, and re-serialize
        $json = $outLines -join "`n" | ConvertFrom-Json
        foreach ($tool in $json.results) {
            if ($tool.option) {
                $tool.option = @($tool.option | Sort-Object -Property name)
            }
        }
        $json | ConvertTo-Json -Depth 10 | Out-File $outputFile
    }
}
finally {
    Set-Location $originalLocation
}
