$file = 'tools/Azure.Mcp.Tools.AzureBackup/tests/Azure.Mcp.Tools.AzureBackup.LiveTests/AzureBackupCommandTests.cs'
$content = [System.IO.File]::ReadAllText($file)

$old = '                { "weekly-retention-weeks", "4" },
                { "weekly-retention-days-of-week", "Sunday" },'

$new = '                { "weekly-retention-weeks", "4" },
                { "weekly-retention-days-of-week", "Sunday,Monday,Tuesday,Thursday,Friday,Saturday" },'

# Only replace inside the FullLogDiff test (the first occurrence with this exact surrounding)
if ($content.Contains($old)) {
    # Find the occurrence that's near "FullLogDiff" - replace only the first one
    $idx = $content.IndexOf('PolicyCreate_RsvSql_FullLogDiff_E2E')
    if ($idx -gt 0) {
        $searchStart = $idx
        $targetIdx = $content.IndexOf($old, $searchStart)
        if ($targetIdx -gt 0) {
            $content = $content.Substring(0, $targetIdx) + $new + $content.Substring($targetIdx + $old.Length)
            [System.IO.File]::WriteAllText($file, $content)
            Write-Host "Replaced successfully at position $targetIdx"
        } else {
            Write-Host "ERROR: Pattern not found after FullLogDiff"
        }
    }
} else {
    Write-Host "ERROR: Pattern not found"
}
