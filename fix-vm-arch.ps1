$file = 'tools/Azure.Mcp.Tools.AzureBackup/tests/Azure.Mcp.Tools.AzureBackup.LiveTests/AzureBackupCommandTests.cs'
$content = [System.IO.File]::ReadAllText($file)

$old = '    [Fact(Skip = "Deferred: VM Enhanced V2 + Weekly schedule + multi-tier (weekly/monthly/yearly) retention + archive tier shape still rejected by RSV API with BMSUserErrorInvalidPolicyInput after dropping the redundant DailySchedule alongside WeeklySchedule. The remaining shape work (likely involving WeeklyRetentionFormat days alignment or TieringPolicy placement on the SubProtectionPolicy) requires further reverse-engineering. Other VM policy shapes succeed against the same vault. Tracked as a follow-up.")]
    public async Task PolicyCreate_RsvVm_WeeklyMultiTierWithArchive_E2E()
    {
        var vaultName = $"{Settings.ResourceBaseName}-rsv";
        var policyName = RegisterOrRetrieveVariable("createdRsvVmWeeklyArchivePolicyName", $"test-vm-weekly-arch-{Random.Shared.NextInt64()}");

        var result = await CallToolAsync(
            "azurebackup_policy_create",
            new()
            {
                { "subscription", Settings.SubscriptionId },
                { "resource-group", Settings.ResourceGroupName },
                { "vault", vaultName },
                { "policy", policyName },
                { "workload-type", "AzureVM" },
                { "schedule-frequency", "Weekly" },
                { "schedule-days-of-week", "Monday" },
                { "schedule-times", "03:00" },
                { "weekly-retention-weeks", "8" },
                { "weekly-retention-days-of-week", "Monday" },
                { "monthly-retention-months", "12" },
                { "monthly-retention-days-of-month", "1" },
                { "yearly-retention-years", "5" },
                { "yearly-retention-months", "January" },
                { "yearly-retention-week-of-month", "First" },
                { "yearly-retention-days-of-week", "Sunday" },
                { "archive-tier-mode", "TierAfter" },
                { "archive-tier-after-days", "90" }
            });'

$new = '    [Fact]
    public async Task PolicyCreate_RsvVm_WeeklyMultiTierWithArchive_E2E()
    {
        var vaultName = $"{Settings.ResourceBaseName}-rsv";
        var policyName = RegisterOrRetrieveVariable("createdRsvVmWeeklyArchivePolicyName", $"test-vm-weekly-arch-{Random.Shared.NextInt64()}");

        // VM Enhanced V2 + Weekly + multi-tier + archive. All retention days-of-week must match schedule.
        // Monthly/Yearly use relative format (week-of-month + days-of-week) for Weekly schedule.
        var result = await CallToolAsync(
            "azurebackup_policy_create",
            new()
            {
                { "subscription", Settings.SubscriptionId },
                { "resource-group", Settings.ResourceGroupName },
                { "vault", vaultName },
                { "policy", policyName },
                { "workload-type", "AzureVM" },
                { "policy-sub-type", "Enhanced" },
                { "schedule-frequency", "Weekly" },
                { "schedule-days-of-week", "Monday" },
                { "schedule-times", "03:00" },
                { "weekly-retention-weeks", "8" },
                { "weekly-retention-days-of-week", "Monday" },
                { "monthly-retention-months", "12" },
                { "monthly-retention-week-of-month", "First" },
                { "monthly-retention-days-of-week", "Monday" },
                { "yearly-retention-years", "5" },
                { "yearly-retention-months", "January" },
                { "yearly-retention-week-of-month", "First" },
                { "yearly-retention-days-of-week", "Monday" },
                { "archive-tier-mode", "TierAfter" },
                { "archive-tier-after-days", "90" }
            });'

if ($content.Contains($old)) {
    $content = $content.Replace($old, $new)
    [System.IO.File]::WriteAllText($file, $content)
    Write-Host "Replaced successfully"
} else {
    Write-Host "ERROR: Pattern not found"
}
