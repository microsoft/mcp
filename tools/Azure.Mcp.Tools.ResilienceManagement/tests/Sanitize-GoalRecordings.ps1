param(
    [Parameter(Mandatory)] [string] $RecordingDirectory,
    [Parameter(Mandatory)] [ValidateNotNullOrWhiteSpace()] [string] $SubscriptionId,
    [Parameter(Mandatory)] [ValidateNotNullOrWhiteSpace()] [string] $TenantId,
    [Parameter(Mandatory)] [ValidateNotNullOrWhiteSpace()] [string] $ServiceGroup,
    [Parameter(Mandatory)] [ValidateNotNullOrWhiteSpace()] [string] $ResourceGroup,
    [Parameter(Mandatory)] [ValidateNotNullOrWhiteSpace()] [string] $StorageAccount
)

$ErrorActionPreference = 'Stop'
$directory = (Resolve-Path -LiteralPath $RecordingDirectory).Path
$files = @(Get-ChildItem -LiteralPath $directory -File -Filter '*ResilienceManagementCommandTests.*_ga*.json')
if ($files.Count -eq 0) {
    throw 'No Goals GA recordings were found. Supply the exact SessionRecords directory.'
}

# Proxy entry sanitizers do not sanitize Variables. Apply consistent substitutions to both.
$replacements = [ordered]@{
    $SubscriptionId = '00000000-0000-0000-0000-000000000000'
    $TenantId = '00000000-0000-0000-0000-000000000000'
    $ServiceGroup = 'goals-service-group'
    $ResourceGroup = 'Sanitized'
    $StorageAccount = 'goalsstorage' # cspell:ignore goalsstorage
}

foreach ($file in $files) {
    $content = [System.IO.File]::ReadAllText($file.FullName)
    foreach ($replacement in $replacements.GetEnumerator()) {
        $content = [regex]::Replace($content, [regex]::Escape($replacement.Key), $replacement.Value, 'IgnoreCase')
    }
    $null = $content | ConvertFrom-Json
    [System.IO.File]::WriteAllText($file.FullName, $content, [System.Text.UTF8Encoding]::new($false))
}
Write-Host "Sanitized $($files.Count) local Goals recordings. Existing non-Goals recordings were not modified."
