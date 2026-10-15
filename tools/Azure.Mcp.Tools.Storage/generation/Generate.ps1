param([switch]$Verify)

$ErrorActionPreference = 'Stop'
$toolRoot = Split-Path $PSScriptRoot -Parent
$generated = Join-Path $toolRoot 'src\Generated'
$before = @{}
if ($Verify -and (Test-Path $generated)) {
    Get-ChildItem $generated -File | ForEach-Object { $before[$_.Name] = (Get-FileHash $_.FullName).Hash }
}

Push-Location $PSScriptRoot
try {
    dotnet build '..\src\Azure.Mcp.Tools.Storage.csproj' --nologo --verbosity quiet
    if ($LASTEXITCODE -ne 0) { throw 'Storage prerequisite build failed.' }
    dotnet build 'Generator\StorageMcp.Generator.csproj' --configuration Release --nologo --verbosity quiet
    if ($LASTEXITCODE -ne 0) { throw 'MCP generator build failed.' }
    npx --no-install tsp compile 'storage.tsp'
    if ($LASTEXITCODE -ne 0) { throw 'TypeSpec-to-MCP generation failed.' }
    if ($Verify) {
        $after = @(Get-ChildItem $generated -File)
        if ($after.Count -ne $before.Count) { throw 'Generated file inventory changed.' }
        foreach ($file in $after) {
            if ($before[$file.Name] -ne (Get-FileHash $file.FullName).Hash) {
                throw "Generated output drifted: $($file.Name)"
            }
        }
    }
}
finally {
    Pop-Location
}
