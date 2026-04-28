$file = 'tools/Azure.Mcp.Tools.AzureBackup/tests/Azure.Mcp.Tools.AzureBackup.LiveTests/AzureBackupCommandTests.cs'
$content = [System.IO.File]::ReadAllText($file)

$idx = $content.IndexOf('PolicyCreate_RsvSql_FullLogDiff_E2E')

# Fix diff retention: 7 -> 15
$old1 = '                { "differential-retention-days", "7" },'
$t1 = $content.IndexOf($old1, $idx)
if ($t1 -gt 0) {
    $content = $content.Substring(0, $t1) + '                { "differential-retention-days", "15" },' + $content.Substring($t1 + $old1.Length)
    Write-Host "Fixed diff retention 7->15"
}

# Fix log retention: 5 -> 7
$old2 = '                { "log-retention-days", "5" }'
$t2 = $content.IndexOf($old2, $idx)
if ($t2 -gt 0) {
    $content = $content.Substring(0, $t2) + '                { "log-retention-days", "7" }' + $content.Substring($t2 + $old2.Length)
    Write-Host "Fixed log retention 5->7"
}

[System.IO.File]::WriteAllText($file, $content)
Write-Host "Done"
