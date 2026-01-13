$ErrorActionPreference = "Stop"

$testName = 'Should_create_azure_managed_lustre_with_storage_and_cmk'
$assetsJsonPath = "$PSScriptRoot/Azure.Mcp.Tools.ManagedLustre.LiveTests/assets.json"

if ( -not (Test-Path $assetsJsonPath)) {
    Write-Error "Could not find assets.json at path: $assetsJsonPath"
}

Write-Host "Locating assets path using 'test-proxy config locate'..." -ForegroundColor Cyan
$assetsPath = test-proxy config locate --assets-json-path $assetsJsonPath | Select-Object -Last 1
if ($LastExitCode -ne 0) {
    Write-Error "Could not locate assets path using test-proxy."
}
Write-Host "Assets path located at: $assetsPath" -ForegroundColor Green

Write-Host "`nLocating recording file for test: $testName" -ForegroundColor Cyan
$recordingPath = Get-ChildItem $assetsPath -Filter "*.$testName.json" -Recurse | Select-Object -First 1
if (-not $recordingPath) {
    Write-Error "Could not find recording file for $testName in assets path: $assetsPath"
}
Write-Host "Recording file located at: $recordingPath" -ForegroundColor Green


Write-Host "`nProcessing recording to remove consecutive 'InProgress' responses..." -ForegroundColor Cyan
$recording = Get-Content $recordingPath | ConvertFrom-Json

$newEntries = @()
$lastStatus = ''

Write-Host "Original number of entries: $($recording.Entries.Count)" -ForegroundColor Yellow
# Remove consecutive 'InProgress' responses
foreach ($entry in $recording.Entries) {                                                                                                                              
  $currentStatus = $entry.ResponseBody.status
  if ($currentStatus -ne 'InProgress' -or $lastStatus -ne 'InProgress') {
    $newEntries += $entry
  }
  $lastStatus = $currentStatus
}

$recording.Entries = $newEntries
Write-Host "Updated number of entries: $($recording.Entries.Count)" -ForegroundColor Yellow
$recording | ConvertTo-Json -Depth 100 | Set-Content $recordingPath
Write-Host "Recording file updated successfully." -ForegroundColor Green

Write-Host "`nPushing updated recording back to assets using 'test-proxy push'..." -ForegroundColor Cyan
test-proxy push --assets-json-path $assetsJsonPath
if ($LastExitCode -ne 0) {
    Write-Error "Could not push updated recording using test-proxy."
}
Write-Host "Updated recording pushed successfully." -ForegroundColor Green
