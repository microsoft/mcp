$file = 'tools/Azure.Mcp.Tools.AzureBackup/tests/Azure.Mcp.Tools.AzureBackup.LiveTests/AzureBackupCommandTests.cs'
$content = [System.IO.File]::ReadAllText($file)

$old = '    [Fact(Skip = "Deferred: AzureDisk operational tier does not accept multi-tier (weekly/monthly/yearly) retention rules. Sourcing the multi-tier rules from VaultStore (when --enable-vault-tier-copy is set) still produces BMSUserErrorInvalidInput from the DPP API; additional shape work is required for the per-tier taggingCriteria/policyRules combination on AzureDisk vault tier. Tracked as a follow-up builder enhancement.")]
    public async Task PolicyCreate_DppDisk_VaultTierMultiTierArchive_E2E()
    {
        var vaultName = $"{Settings.ResourceBaseName}-dpp";
        var policyName = RegisterOrRetrieveVariable("createdDppDiskMultiTierPolicyName", $"test-disk-mt-{Random.Shared.NextInt64()}");

        var result = await CallToolAsync(
            "azurebackup_policy_create",
            new()
            {
                { "subscription", Settings.SubscriptionId },
                { "resource-group", Settings.ResourceGroupName },
                { "vault", vaultName },
                { "policy", policyName },
                { "workload-type", "AzureDisk" },
                { "schedule-times", "02:00" },
                { "daily-retention-days", "7" },
                { "weekly-retention-weeks", "4" },
                { "weekly-retention-days-of-week", "Sunday" },
                { "monthly-retention-months", "12" },
                { "monthly-retention-days-of-month", "1" },
                { "yearly-retention-years", "5" },
                { "yearly-retention-months", "January" },
                { "yearly-retention-week-of-month", "First" },
                { "yearly-retention-days-of-week", "Sunday" },
                { "archive-tier-mode", "TierAfter" },
                { "archive-tier-after-days", "180" }
            });'

$new = '    [Fact]
    public async Task PolicyCreate_DppDisk_VaultTierMultiTier_E2E()
    {
        // AzureDisk with vault-tier copy + Weekly/Monthly retention (no Yearly, no Archive per manifest).
        var vaultName = $"{Settings.ResourceBaseName}-dpp";
        var policyName = RegisterOrRetrieveVariable("createdDppDiskMultiTierPolicyName", $"test-disk-mt-{Random.Shared.NextInt64()}");

        var result = await CallToolAsync(
            "azurebackup_policy_create",
            new()
            {
                { "subscription", Settings.SubscriptionId },
                { "resource-group", Settings.ResourceGroupName },
                { "vault", vaultName },
                { "policy", policyName },
                { "workload-type", "AzureDisk" },
                { "schedule-times", "02:00" },
                { "daily-retention-days", "7" },
                { "enable-vault-tier-copy", "true" },
                { "vault-tier-copy-after-days", "7" },
                { "weekly-retention-weeks", "12" },
                { "monthly-retention-months", "12" }
            });'

if ($content.Contains('VaultTierMultiTierArchive_E2E')) {
    $content = $content.Replace($old, $new)
    [System.IO.File]::WriteAllText($file, $content)
    Write-Host "Replaced successfully"
} else {
    Write-Host "ERROR: Pattern not found"
}
