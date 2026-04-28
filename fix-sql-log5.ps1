$file = 'tools/Azure.Mcp.Tools.AzureBackup/tests/Azure.Mcp.Tools.AzureBackup.LiveTests/AzureBackupCommandTests.cs'
$content = [System.IO.File]::ReadAllText($file)

$idx = $content.IndexOf('PolicyCreate_RsvSql_FullLogDiff_E2E')
if ($idx -gt 0) {
    $old = '                { "log-retention-days", "15" }'
    $targetIdx = $content.IndexOf($old, $idx)
    if ($targetIdx -gt 0) {
        $new = '                { "log-retention-days", "5" }'
        $content = $content.Substring(0, $targetIdx) + $new + $content.Substring($targetIdx + $old.Length)
        [System.IO.File]::WriteAllText($file, $content)
        Write-Host "Replaced log retention 15->5"
    } else { Write-Host "ERROR: log retention not found" }
} else { Write-Host "ERROR: test not found" }
