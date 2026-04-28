$file = 'tools/Azure.Mcp.Tools.AzureBackup/tests/Azure.Mcp.Tools.AzureBackup.LiveTests/AzureBackupCommandTests.cs'
$content = [System.IO.File]::ReadAllText($file)

$old = '    [Fact(Skip = "Deferred: SQL Full (Weekly multi-day) + Diff (single day) + Log shape still rejected by RSV API with BMSUserErrorInvalidPolicyInput. Differential can only run once a week. The builder shape for this combination needs canonical sample from SQL workload PM. Other SQL policy shapes succeed against the same vault.")]'

$new = '    [Fact]'

if ($content.Contains($old)) {
    $content = $content.Replace($old, $new)
    [System.IO.File]::WriteAllText($file, $content)
    Write-Host "Replaced successfully"
} else {
    Write-Host "ERROR: Pattern not found"
}
