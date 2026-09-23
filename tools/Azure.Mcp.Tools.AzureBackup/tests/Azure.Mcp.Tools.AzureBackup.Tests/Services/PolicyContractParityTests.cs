// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Reflection;

using Azure.Core;
using Azure.Mcp.Tools.AzureBackup.Models;
using Azure.Mcp.Tools.AzureBackup.Services;
using Azure.ResourceManager.DataProtectionBackup;
using Azure.ResourceManager.DataProtectionBackup.Models;
using Azure.ResourceManager.RecoveryServicesBackup;
using Azure.ResourceManager.RecoveryServicesBackup.Models;
using Xunit;

namespace Azure.Mcp.Tools.AzureBackup.Tests.Services;

public class PolicyContractParityTests
{
    [Fact]
    public void RsvIaasVmPolicyContract_CoversPolicyProperties()
    {
        var sdkProperties = typeof(IaasVmProtectionPolicy)
            .GetProperties()
            .Select(p => p.Name)
            .ToHashSet(StringComparer.Ordinal);

        var covered = new HashSet<string>(StringComparer.Ordinal)
        {
            nameof(IaasVmProtectionPolicy.ProtectedItemsCount),
            nameof(IaasVmProtectionPolicy.ResourceGuardOperationRequests),
            nameof(IaasVmProtectionPolicy.TimeZone),
            nameof(IaasVmProtectionPolicy.PolicyType),
            nameof(IaasVmProtectionPolicy.SnapshotConsistencyType),
            nameof(IaasVmProtectionPolicy.InstantRPRetentionRangeInDays),
            nameof(IaasVmProtectionPolicy.InstantRPDetails),
            nameof(IaasVmProtectionPolicy.SchedulePolicy),
            nameof(IaasVmProtectionPolicy.RetentionPolicy),
            nameof(IaasVmProtectionPolicy.TieringPolicy),
        };

        AssertNoDrift("RSV IaasVM policy", sdkProperties, covered);
    }

    [Fact]
    public void RsvFileSharePolicyContract_CoversPolicyProperties()
    {
        var sdkProperties = typeof(FileShareProtectionPolicy)
            .GetProperties()
            .Select(p => p.Name)
            .ToHashSet(StringComparer.Ordinal);

        var covered = new HashSet<string>(StringComparer.Ordinal)
        {
            nameof(FileShareProtectionPolicy.ProtectedItemsCount),
            nameof(FileShareProtectionPolicy.ResourceGuardOperationRequests),
            nameof(FileShareProtectionPolicy.WorkLoadType),
            nameof(FileShareProtectionPolicy.TimeZone),
            nameof(FileShareProtectionPolicy.SchedulePolicy),
            nameof(FileShareProtectionPolicy.RetentionPolicy),
        };

        var intentionallyExcluded = new HashSet<string>(StringComparer.Ordinal)
        {
            // Vault-tier retention is not surfaced by the policy GET contract today.
            nameof(FileShareProtectionPolicy.VaultRetentionPolicy),
        };

        AssertNoDrift("RSV file share policy", sdkProperties, covered, intentionallyExcluded);
    }

    [Fact]
    public void RsvVmWorkloadPolicyContract_CoversPolicyProperties()
    {
        var sdkProperties = typeof(VmWorkloadProtectionPolicy)
            .GetProperties()
            .Select(p => p.Name)
            .ToHashSet(StringComparer.Ordinal);

        var covered = new HashSet<string>(StringComparer.Ordinal)
        {
            nameof(VmWorkloadProtectionPolicy.ProtectedItemsCount),
            nameof(VmWorkloadProtectionPolicy.ResourceGuardOperationRequests),
            nameof(VmWorkloadProtectionPolicy.WorkLoadType),
            nameof(VmWorkloadProtectionPolicy.Settings),
            nameof(VmWorkloadProtectionPolicy.SubProtectionPolicy),
            nameof(VmWorkloadProtectionPolicy.DoesMakePolicyConsistent),
        };

        AssertNoDrift("RSV workload policy", sdkProperties, covered);
    }

    [Fact]
    public void RsvSubProtectionPolicyContract_CoversProperties()
    {
        var sdkProperties = typeof(SubProtectionPolicy)
            .GetProperties()
            .Select(p => p.Name)
            .ToHashSet(StringComparer.Ordinal);

        var covered = new HashSet<string>(StringComparer.Ordinal)
        {
            nameof(SubProtectionPolicy.PolicyType),
            nameof(SubProtectionPolicy.SchedulePolicy),
            nameof(SubProtectionPolicy.RetentionPolicy),
            nameof(SubProtectionPolicy.TieringPolicy),
            nameof(SubProtectionPolicy.SnapshotBackupAdditionalDetails),
        };

        AssertNoDrift("RSV sub-protection policy", sdkProperties, covered);
    }

    [Fact]
    public void DppRuleBasedPolicyContract_CoversProperties()
    {
        var sdkProperties = typeof(RuleBasedBackupPolicy)
            .GetProperties()
            .Select(p => p.Name)
            .ToHashSet(StringComparer.Ordinal);

        var covered = new HashSet<string>(StringComparer.Ordinal)
        {
            nameof(RuleBasedBackupPolicy.DataSourceTypes),
            nameof(RuleBasedBackupPolicy.PolicyRules),
        };

        AssertNoDrift("DPP rule-based policy", sdkProperties, covered);
    }

    [Fact]
    public void DppBackupRuleContract_CoversProperties()
    {
        var sdkProperties = typeof(DataProtectionBackupRule)
            .GetProperties()
            .Select(p => p.Name)
            .ToHashSet(StringComparer.Ordinal);

        var covered = new HashSet<string>(StringComparer.Ordinal)
        {
            nameof(DataProtectionBackupRule.Name),
            nameof(DataProtectionBackupRule.BackupParameters),
            nameof(DataProtectionBackupRule.DataStore),
            nameof(DataProtectionBackupRule.Trigger),
        };

        AssertNoDrift("DPP backup rule", sdkProperties, covered);
    }

    [Fact]
    public void DppRetentionRuleContract_CoversProperties()
    {
        var sdkProperties = typeof(DataProtectionRetentionRule)
            .GetProperties()
            .Select(p => p.Name)
            .ToHashSet(StringComparer.Ordinal);

        var covered = new HashSet<string>(StringComparer.Ordinal)
        {
            nameof(DataProtectionRetentionRule.Name),
            nameof(DataProtectionRetentionRule.IsDefault),
            nameof(DataProtectionRetentionRule.Lifecycles),
        };

        AssertNoDrift("DPP retention rule", sdkProperties, covered);
    }

    [Fact]
    public void RsvVmPolicyMapper_PopulatesDetails()
    {
        var policy = new IaasVmProtectionPolicy
        {
            TimeZone = "UTC",
            InstantRPRetentionRangeInDays = 2,
        };

        var mapped = InvokeRsvPolicyMapper(policy);

        Assert.NotNull(mapped.Details);
        Assert.Equal("AzureIaasVM", mapped.Details!.WorkloadType);
        Assert.Equal("UTC", mapped.Details.TimeZone);
        Assert.Equal(2, mapped.Details.InstantRPRetentionRangeInDays);
        Assert.Null(mapped.DppDetails);
    }

    [Fact]
    public void DppPolicyMapper_PopulatesDppDetailsWithRules()
    {
        var schedule = new DataProtectionBackupSchedule(new[] { "R/2024-01-01T02:00:00+00:00/P1D" });
        var trigger = new ScheduleBasedBackupTriggerContext(schedule, Array.Empty<DataProtectionBackupTaggingCriteria>());
        var backupRule = new DataProtectionBackupRule(
            "BackupDaily",
            new DataStoreInfoBase(DataStoreType.VaultStore, "DataStoreInfoBase"),
            trigger);

        var lifecycle = new SourceLifeCycle(
            new DataProtectionBackupAbsoluteDeleteSetting(TimeSpan.FromDays(30)),
            new DataStoreInfoBase(DataStoreType.VaultStore, "DataStoreInfoBase"));
        var retentionRule = new DataProtectionRetentionRule("Default", new[] { lifecycle });

        var policy = new RuleBasedBackupPolicy(
            new[] { "Microsoft.Compute/disks" },
            new DataProtectionBasePolicyRule[] { backupRule, retentionRule });
        var data = new DataProtectionBackupPolicyData
        {
            Properties = policy,
        };

        var mapped = InvokeDppPolicyMapper(data);

        Assert.NotNull(mapped.DppDetails);
        Assert.Contains("Microsoft.Compute/disks", mapped.DppDetails!.DataSourceTypes!);
        Assert.NotNull(mapped.DppDetails.Rules);
        Assert.Equal(2, mapped.DppDetails.Rules!.Count);

        var mappedBackupRule = mapped.DppDetails.Rules.Single(r => r.Name == "BackupDaily");
        Assert.NotNull(mappedBackupRule.RepeatingTimeIntervals);
        Assert.Contains("R/2024-01-01T02:00:00+00:00/P1D", mappedBackupRule.RepeatingTimeIntervals!);

        var mappedRetentionRule = mapped.DppDetails.Rules.Single(r => r.Name == "Default");
        Assert.NotNull(mappedRetentionRule.Lifecycles);
        Assert.Single(mappedRetentionRule.Lifecycles!);
        Assert.Equal("VaultStore", mappedRetentionRule.Lifecycles![0].SourceDataStoreType);
        Assert.Null(mapped.Details);
    }

    private static void AssertNoDrift(
        string label,
        HashSet<string> sdkProperties,
        HashSet<string> covered,
        HashSet<string>? intentionallyExcluded = null)
    {
        var missing = sdkProperties
            .Except(covered)
            .Except(intentionallyExcluded ?? new HashSet<string>(StringComparer.Ordinal))
            .OrderBy(s => s)
            .ToArray();

        Assert.True(
            missing.Length == 0,
            $"{label} property coverage drift detected. Missing: {string.Join(", ", missing)}");
    }

    private static BackupPolicyInfo InvokeRsvPolicyMapper(BackupGenericProtectionPolicy policy)
    {
        var data = new BackupProtectionPolicyData(new AzureLocation("eastus"))
        {
            Properties = policy,
        };

        var method = typeof(RsvBackupOperations).GetMethod("MapToPolicyInfo", BindingFlags.NonPublic | BindingFlags.Static);
        Assert.NotNull(method);

        return Assert.IsType<BackupPolicyInfo>(method!.Invoke(null, [data]));
    }

    private static BackupPolicyInfo InvokeDppPolicyMapper(DataProtectionBackupPolicyData data)
    {
        var method = typeof(DppBackupOperations).GetMethod("MapToPolicyInfo", BindingFlags.NonPublic | BindingFlags.Static);
        Assert.NotNull(method);

        return Assert.IsType<BackupPolicyInfo>(method!.Invoke(null, [data]));
    }
}
