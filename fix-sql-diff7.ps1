$file = 'tools/Azure.Mcp.Tools.AzureBackup/tests/Azure.Mcp.Tools.AzureBackup.LiveTests/AzureBackupCommandTests.cs'
$content = [System.IO.File]::ReadAllText($file)

$old = '                { "differential-retention-days", "30" },'
$new = '                { "differential-retention-days", "7" },'

$idx = $content.IndexOf('PolicyCreate_RsvSql_FullLogDiff_E2E')
if ($idx -gt 0) {
    $targetIdx = $content.IndexOf($old, $idx)
    if ($targetIdx -gt 0) {
        $content = $content.Substring(0, $targetIdx) + $new + $content.Substring($targetIdx + $old.Length)
        [System.IO.File]::WriteAllText($file, $content)
        Write-Host "Replaced diff retention 30->7 at position $targetIdx"
    } else {
        Write-Host "ERROR: diff retention pattern not found after FullLogDiff"
    }
} else {
    Write-Host "ERROR: FullLogDiff test not found"
}
