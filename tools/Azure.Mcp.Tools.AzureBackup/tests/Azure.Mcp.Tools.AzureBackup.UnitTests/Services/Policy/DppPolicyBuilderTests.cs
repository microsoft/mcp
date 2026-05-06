// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using Azure.Mcp.Tools.AzureBackup.Services;
using Azure.Mcp.Tools.AzureBackup.Services.Policy;
using Azure.ResourceManager.DataProtectionBackup.Models;
using Xunit;

namespace Azure.Mcp.Tools.AzureBackup.UnitTests.Services.Policy;

public class DppPolicyBuilderTests
{
    [Fact]
    public void Build_DiskMinimal_ProducesDefaultRuleAndBackupRule()
    {
        var profile = DppDatasourceRegistry.Resolve("AzureDisk");
        var req = new PolicyCreateRequest
        {
            Policy = "p",
            WorkloadType = "AzureDisk",
        };

        var policy = DppPolicyBuilder.Build(req, profile);

        Assert.Equal(2, policy.PolicyRules.Count);
        var defaultRule = Assert.IsType<DataProtectionRetentionRule>(policy.PolicyRules[0]);
        Assert.Equal("Default", defaultRule.Name);
        Assert.True(defaultRule.IsDefault);
        Assert.Single(defaultRule.Lifecycles);
        Assert.Empty(defaultRule.Lifecycles[0].TargetDataStoreCopySettings);

        var backupRule = Assert.IsType<DataProtectionBackupRule>(policy.PolicyRules[1]);
        Assert.Equal(profile.BackupRuleName, backupRule.Name);
        var trigger = Assert.IsType<ScheduleBasedBackupTriggerContext>(backupRule.Trigger);
        Assert.Single(trigger.TaggingCriteriaList);
        Assert.True(trigger.TaggingCriteriaList[0].IsDefault);
    }

    [Fact]
    public void Build_BlobContinuous_OmitsBackupRule()
    {
        var profile = DppDatasourceRegistry.Resolve("AzureBlob");
        var req = new PolicyCreateRequest { Policy = "p", WorkloadType = "AzureBlob" };

        var policy = DppPolicyBuilder.Build(req, profile);

        Assert.Single(policy.PolicyRules);
        Assert.IsType<DataProtectionRetentionRule>(policy.PolicyRules[0]);
    }

    [Fact]
    public void Build_DiskMultiTierWithArchive_AddsTierRulesAndArchiveCopy()
    {
        var profile = DppDatasourceRegistry.Resolve("AzureDisk");
        var req = new PolicyCreateRequest
        {
            Policy = "p",
            WorkloadType = "AzureDisk",
            DailyRetentionDays = "7",
            WeeklyRetentionWeeks = 8,
            MonthlyRetentionMonths = 12,
            YearlyRetentionYears = 5,
            ArchiveTierAfterDays = "90",
            ArchiveTierMode = "TierAfter",
        };

        var policy = DppPolicyBuilder.Build(req, profile);

        // 4 retention rules (Default + Weekly + Monthly + Yearly) + 1 backup rule.
        Assert.Equal(5, policy.PolicyRules.Count);
        var ruleNames = policy.PolicyRules.OfType<DataProtectionRetentionRule>().Select(r => r.Name).ToArray();
        Assert.Contains("Default", ruleNames);
        Assert.Contains("Weekly", ruleNames);
        Assert.Contains("Monthly", ruleNames);
        Assert.Contains("Yearly", ruleNames);

        // Each retention rule has an archive copy setting (CustomCopySetting because TierAfter+days).
        foreach (var rule in policy.PolicyRules.OfType<DataProtectionRetentionRule>())
        {
            Assert.Single(rule.Lifecycles[0].TargetDataStoreCopySettings);
            var copy = rule.Lifecycles[0].TargetDataStoreCopySettings[0];
            Assert.Equal(DataStoreType.ArchiveStore, copy.DataStore.DataStoreType);
            var custom = Assert.IsType<CustomCopySetting>(copy.CopyAfter);
            Assert.Equal(TimeSpan.FromDays(90), custom.Duration);
        }

        // Backup rule has 4 tagging criteria (default + 3 tiers).
        var backupRule = (DataProtectionBackupRule)policy.PolicyRules.Last(r => r is DataProtectionBackupRule);
        var trigger = (ScheduleBasedBackupTriggerContext)backupRule.Trigger;
        Assert.Equal(4, trigger.TaggingCriteriaList.Count);
        Assert.Contains(trigger.TaggingCriteriaList, t => t.TagInfo.TagName == "Weekly");
        Assert.Contains(trigger.TaggingCriteriaList, t => t.TagInfo.TagName == "Monthly");
        Assert.Contains(trigger.TaggingCriteriaList, t => t.TagInfo.TagName == "Yearly");

        var weeklyTag = trigger.TaggingCriteriaList.First(t => t.TagInfo.TagName == "Weekly");
        var sched = Assert.IsType<ScheduleBasedBackupCriteria>(weeklyTag.Criteria[0]);
        Assert.Contains(BackupAbsoluteMarker.FirstOfWeek, sched.AbsoluteCriteria);
    }

    [Fact]
    public void Build_DiskCopyOnExpiry_UsesCopyOnExpirySetting()
    {
        var profile = DppDatasourceRegistry.Resolve("AzureDisk");
        var req = new PolicyCreateRequest
        {
            Policy = "p",
            WorkloadType = "AzureDisk",
            ArchiveTierMode = "CopyOnExpiry",
        };

        var policy = DppPolicyBuilder.Build(req, profile);
        var defaultRule = (DataProtectionRetentionRule)policy.PolicyRules[0];
        Assert.Single(defaultRule.Lifecycles[0].TargetDataStoreCopySettings);
        Assert.IsType<CopyOnExpirySetting>(defaultRule.Lifecycles[0].TargetDataStoreCopySettings[0].CopyAfter);
    }

    [Fact]
    public void Build_DiskCustomScheduleAndTimeZone_FlowsIntoBackupRule()
    {
        var profile = DppDatasourceRegistry.Resolve("AzureDisk");
        var req = new PolicyCreateRequest
        {
            Policy = "p",
            WorkloadType = "AzureDisk",
            ScheduleFrequency = "PT4H",
            ScheduleTimes = "14:30",
            TimeZone = "UTC",
        };

        var policy = DppPolicyBuilder.Build(req, profile);
        var backupRule = (DataProtectionBackupRule)policy.PolicyRules.Last(r => r is DataProtectionBackupRule);
        var trigger = (ScheduleBasedBackupTriggerContext)backupRule.Trigger;
        var interval = trigger.Schedule.RepeatingTimeIntervals[0];
        Assert.EndsWith("/PT4H", interval);
        Assert.Contains("14:30:00", interval);
        Assert.Equal("UTC", trigger.Schedule.TimeZone);
    }

    // ===== Stage 2 tests =====

    [Fact]
    public void Build_DiskWithVaultTierCopy_EmitsSeparateNamedRetentionRuleAndDailyTag()
    {
        var profile = DppDatasourceRegistry.Resolve("AzureDisk");
        var req = new PolicyCreateRequest
        {
            Policy = "p",
            WorkloadType = "AzureDisk",
            EnableVaultTierCopy = true,
            VaultTierCopyAfterDays = 7,
        };

        var policy = DppPolicyBuilder.Build(req, profile);

        // Default operational retention rule (rule[0]) should have NO TargetDataStoreCopySettings.
        var defaultRule = (DataProtectionRetentionRule)policy.PolicyRules[0];
        Assert.Empty(defaultRule.Lifecycles[0].TargetDataStoreCopySettings);

        // A separate "Daily" retention rule sourced from VaultStore must follow.
        var vaultRule = (DataProtectionRetentionRule)policy.PolicyRules[1];
        Assert.Equal("Daily", vaultRule.Name);
        Assert.False(vaultRule.IsDefault);
        Assert.Equal(DataStoreType.VaultStore, vaultRule.Lifecycles[0].SourceDataStore.DataStoreType);
        var deleteAfter = (DataProtectionBackupAbsoluteDeleteSetting)vaultRule.Lifecycles[0].DeleteAfter;
        Assert.Equal(TimeSpan.FromDays(7), deleteAfter.Duration);

        // BackupRule's tagging criteria must contain a "Daily" tag with FirstOfDay marker.
        var backupRule = (DataProtectionBackupRule)policy.PolicyRules[^1];
        var trigger = (ScheduleBasedBackupTriggerContext)backupRule.Trigger;
        Assert.Contains(trigger.TaggingCriteriaList, t => t.TagInfo.TagName == "Daily" &&
            t.Criteria.OfType<ScheduleBasedBackupCriteria>().Any(c => c.AbsoluteCriteria.Contains(BackupAbsoluteMarker.FirstOfDay)));
    }

    [Fact]
    public void Build_BlobVaultedMode_EmitsRetentionOnlyAndUsesVaultStore()
    {
        var profile = DppDatasourceRegistry.Resolve("AzureBlob");
        var req = new PolicyCreateRequest
        {
            Policy = "p",
            WorkloadType = "AzureBlob",
            BackupMode = "Vaulted",
            DailyRetentionDays = "30",
        };

        var policy = DppPolicyBuilder.Build(req, profile);

        // Vaulted Blob: only a single VaultStore retention rule (no AzureBackupRule).
        var rule = Assert.Single(policy.PolicyRules);
        var retention = Assert.IsType<DataProtectionRetentionRule>(rule);
        Assert.Equal(DataStoreType.VaultStore, retention.Lifecycles[0].SourceDataStore.DataStoreType);
    }

    [Fact]
    public void Build_BlobContinuousWithPitrRetentionDays_OverridesDefault()
    {
        var profile = DppDatasourceRegistry.Resolve("AzureBlob");
        var req = new PolicyCreateRequest
        {
            Policy = "p",
            WorkloadType = "AzureBlob",
            PitrRetentionDays = 60,
        };

        var policy = DppPolicyBuilder.Build(req, profile);
        var defaultRule = (DataProtectionRetentionRule)policy.PolicyRules[0];
        var deleteSetting = (DataProtectionBackupAbsoluteDeleteSetting)defaultRule.Lifecycles[0].DeleteAfter;

        Assert.Equal(TimeSpan.FromDays(60), deleteSetting.Duration);
    }

    [Fact]
    public void Build_AdlsVaultedMode_EmitsRetentionOnly()
    {
        var profile = DppDatasourceRegistry.Resolve("ADLS");
        var req = new PolicyCreateRequest
        {
            Policy = "p",
            WorkloadType = "AzureDataLakeStorage",
            BackupMode = "Vaulted",
            DailyRetentionDays = "30",
        };

        var policy = DppPolicyBuilder.Build(req, profile);

        // Vaulted ADLS: only a single VaultStore retention rule (no AzureBackupRule).
        var rule = Assert.Single(policy.PolicyRules);
        var retention = Assert.IsType<DataProtectionRetentionRule>(rule);
        Assert.Equal(DataStoreType.VaultStore, retention.Lifecycles[0].SourceDataStore.DataStoreType);
    }

    [Fact]
    public void Build_AksMinimal_ProducesPolicyForKubernetesArmType()
    {
        var profile = DppDatasourceRegistry.Resolve("AKS");
        var req = new PolicyCreateRequest
        {
            Policy = "p",
            WorkloadType = "AKS",
        };

        var policy = DppPolicyBuilder.Build(req, profile);

        Assert.Contains("Microsoft.ContainerService", policy.DataSourceTypes[0]);
        Assert.NotEmpty(policy.PolicyRules);
    }
}
