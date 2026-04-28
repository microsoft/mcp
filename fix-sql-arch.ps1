$file = 'tools/Azure.Mcp.Tools.AzureBackup/tests/Azure.Mcp.Tools.AzureBackup.LiveTests/AzureBackupCommandTests.cs'
$content = [System.IO.File]::ReadAllText($file)

$old = '    [Fact(Skip = "Deferred: SQL workload + Weekly Full + LongTerm retention + TieringPolicy still rejected by RSV API with BMSUserErrorInvalidPolicyInput after dropping the redundant DailySchedule alongside WeeklySchedule. The SQL sub-policy shape for archive tier copy needs further reverse-engineering (likely affects the Full sub-policy''s RetentionPolicy/TieringPolicy combination). Other SQL policy shapes succeed against the same vault. Tracked as a follow-up.")]
    public async Task PolicyCreate_RsvSql_WithArchiveTier_E2E()
    {
        var vaultName = $"{Settings.ResourceBaseName}-rsv";
        var policyName = RegisterOrRetrieveVariable("createdSqlArchivePolicyName", $"test-sql-archive-{Random.Shared.NextInt64()}");

        // SQL Full Weekly + monthly retention + archive on Full sub-policy. Drop daily retention
        // (not valid alongside Weekly schedule per RSV shape rules).
        var result = await CallToolAsync(
            "azurebackup_policy_create",
            new()
            {
                { "subscription", Settings.SubscriptionId },
                { "resource-group", Settings.ResourceGroupName },
                { "vault", vaultName },
                { "policy", policyName },
                { "workload-type", "MSSQL" },
                { "full-schedule-frequency", "Weekly" },
                { "full-schedule-days-of-week", "Sunday" },
                { "schedule-times", "02:00" },
                { "weekly-retention-weeks", "4" },
                { "weekly-retention-days-of-week", "Sunday" },
                { "monthly-retention-months", "12" },
                { "monthly-retention-days-of-month", "1" },
                { "archive-tier-mode", "TierAfter" },
                { "archive-tier-after-days", "90" },
                { "log-frequency-minutes", "60" }
            });'

$new = '    [Fact]
    public async Task PolicyCreate_RsvSql_WithArchiveTier_E2E()
    {
        var vaultName = $"{Settings.ResourceBaseName}-rsv";
        var policyName = RegisterOrRetrieveVariable("createdSqlArchivePolicyName", $"test-sql-archive-{Random.Shared.NextInt64()}");

        // SQL Full Weekly + weekly/monthly retention + archive on Full sub-policy + Log.
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
                { "full-schedule-days-of-week", "Sunday" },
                { "schedule-times", "02:00" },
                { "weekly-retention-weeks", "4" },
                { "weekly-retention-days-of-week", "Sunday" },
                { "monthly-retention-months", "12" },
                { "monthly-retention-week-of-month", "First" },
                { "monthly-retention-days-of-week", "Sunday" },
                { "archive-tier-mode", "TierAfter" },
                { "archive-tier-after-days", "90" },
                { "log-frequency-minutes", "60" },
                { "log-retention-days", "7" }
            });'

if ($content.Contains($old)) {
    $content = $content.Replace($old, $new)
    [System.IO.File]::WriteAllText($file, $content)
    Write-Host "Replaced successfully"
} else {
    Write-Host "ERROR: Pattern not found"
}
