param([switch]$Evaluate)

$ErrorActionPreference = 'Stop'
$repoRoot = (Resolve-Path (Join-Path $PSScriptRoot '..\..\..')).Path
$output = Join-Path $PSScriptRoot 'comparison'
$serverProject = Join-Path $repoRoot 'servers\Azure.Mcp.Server\src\Azure.Mcp.Server.csproj'
$server = Join-Path $repoRoot 'servers\Azure.Mcp.Server\src\bin\Debug\net10.0\azmcp.dll'
$evaluator = Join-Path $repoRoot 'eng\tools\ToolDescriptionEvaluator\src'
$prompts = Join-Path $repoRoot 'servers\Azure.Mcp.Server\docs\e2eTestPrompts.md'
New-Item -ItemType Directory -Path $output -Force | Out-Null
$catalogs = @{}

foreach ($variant in @('baseline', 'generated')) {
    $enabled = ($variant -eq 'generated').ToString().ToLowerInvariant()
    dotnet build $serverProject "-p:UseGeneratedStorageTools=$enabled" --nologo --verbosity quiet
    if ($LASTEXITCODE -ne 0) { throw "$variant server build failed." }
    $raw = (& dotnet $server tools list) -join "`n"
    if ($LASTEXITCODE -ne 0) { throw "$variant catalog extraction failed." }
    $response = $raw | ConvertFrom-Json -AsHashtable
    if ($response.status -ne 200 -or !$response.results -or $response.results.Count -eq 0) {
        throw "$variant returned no successful tool catalog."
    }
    $raw | Set-Content -LiteralPath (Join-Path $output "$variant.json") -Encoding utf8
    $catalogs[$variant] = $response.results
}

$baseline = ConvertTo-Json -InputObject $catalogs.baseline -Depth 100 -Compress
$generated = ConvertTo-Json -InputObject $catalogs.generated -Depth 100 -Compress
if ($baseline -cne $generated) {
    $manifest = Get-Content -LiteralPath (Join-Path $PSScriptRoot 'storage.tools.json') -Raw | ConvertFrom-Json
    if ($manifest.operations[0].useDescriptionOverride) {
        throw "Catalog/schema parity failed. Compare baseline.json and generated.json in $output."
    }
    foreach ($variant in @('baseline', 'generated')) {
        $selected = @($catalogs[$variant] | Where-Object { $_.command -eq 'storage blob container create' })
        if ($selected.Count -ne 1) { throw "$variant does not contain exactly one curated create tool." }
        $selected[0].description = '<intentional generated draft; compare with evaluator>'
    }
    if ((ConvertTo-Json -InputObject $catalogs.baseline -Depth 100 -Compress) -cne
        (ConvertTo-Json -InputObject $catalogs.generated -Depth 100 -Compress)) {
        throw "Unexpected catalog/schema difference beyond the selected draft description. Compare snapshots in $output."
    }
    Write-Host 'Only the manifest-selected draft description differs; all other catalog/schema metadata match.'
}
else {
    Write-Host 'Baseline and generated tool catalogs match, including descriptions, required options and safety metadata.'
}

if ($Evaluate) {
    foreach ($name in @('AOAI_ENDPOINT', 'TEXT_EMBEDDING_API_KEY')) {
        if ([string]::IsNullOrWhiteSpace([Environment]::GetEnvironmentVariable($name))) {
            throw "$name is required for the real ToolDescriptionEvaluator comparison. No model evaluation was performed."
        }
    }
    foreach ($variant in @('baseline', 'generated')) {
        $resultName = "storage-$variant"
        dotnet run --project $evaluator -- --area storage --tools-file (Join-Path $output "$variant.json") `
            --prompts-file $prompts --output-file-name $resultName
        if ($LASTEXITCODE -ne 0) { throw "$variant description evaluation failed." }
        $reports = @('.md', '.txt') | ForEach-Object { Join-Path $evaluator "$resultName$_" } |
            Where-Object { Test-Path -LiteralPath $_ }
        if (!$reports) { throw "$variant evaluator returned without producing a report." }
        foreach ($report in $reports) { Move-Item -LiteralPath $report -Destination $output -Force }
    }
}
