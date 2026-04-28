$file = 'tools/Azure.Mcp.Tools.AzureBackup/tests/Azure.Mcp.Tools.AzureBackup.LiveTests/AzureBackupCommandTests.cs'
$content = [System.IO.File]::ReadAllText($file)

$old = '    [Fact]
    public async Task PolicyCreate_RsvSql_FullLogDiff_E2E()
    {
        var vaultName = $"{Settings.ResourceBaseName}-rsv";
        var policyName = RegisterOrRetrieveVariable("createdRsvSqlFullLogDiffPolicyName", $"test-sql-fld-{Random.Shared.NextInt64()}");

        // SQL Full=Daily (runs every day except the Diff day), Diff=once a week (Wednesday).
        // Differential can only run once a week.
        var result = await CallToolAsync(
            "azurebackup_policy_create",
            new()
            {
                { "subscription", Settings.SubscriptionId },
                { "resource-group", Settings.ResourceGroupName },
                { "vault", vaultName },
                { "policy", policyName },
                { "workload-type", "SQL" },
                { "schedule-times", "02:00" },
                { "daily-retention-days", "7" },
                { "differential-schedule-days-of-week", "Wednesday" },
                { "differential-retention-days", "30" },
                { "log-frequency-minutes", "60" },
                { "log-retention-days", "15" }
            });'

$new = '    [Fact]
    public async Task PolicyCreate_RsvSql_FullLogDiff_E2E()
    {
        var vaultName = $"{Settings.ResourceBaseName}-rsv";
        var policyName = RegisterOrRetrieveVariable("createdRsvSqlFullLogDiffPolicyName", $"test-sql-fld-{Random.Shared.NextInt64()}");

        // SQL Full = Weekly multi-day (every day except Wednesday), Diff = once a week (Wednesday).
        // Differential can only run once a week and must not overlap with Full days.
        var result = await CallToolAsync(
            "azurebackup_policy_create",
            new()
            {
                { "subscription", Settings.SubscriptionId },
                { "resource-group", Settings.ResourceGroupName },
                { "vault", vaultName },
                { "policy", policyName },
                { "workload-type", "SQL" },
                { "full-schedule-frequency", "Weekly" },
                { "full-schedule-days-of-week", "Sunday,Monday,Tuesday,Thursday,Friday,Saturday" },
                { "schedule-times", "02:00" },
                { "weekly-retention-weeks", "4" },
                { "weekly-retention-days-of-week", "Sunday" },
                { "differential-schedule-days-of-week", "Wednesday" },
                { "differential-retention-days", "30" },
                { "log-frequency-minutes", "60" },
                { "log-retention-days", "15" }
            });'

if ($content.Contains($old)) {
    $content = $content.Replace($old, $new)
    [System.IO.File]::WriteAllText($file, $content)
    Write-Host "Replaced successfully"
} else {
    Write-Host "ERROR: Pattern not found"
}
