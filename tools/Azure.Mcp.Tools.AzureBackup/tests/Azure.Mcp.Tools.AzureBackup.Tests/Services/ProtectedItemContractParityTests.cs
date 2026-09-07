// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.ResourceManager.DataProtectionBackup.Models;
using Azure.ResourceManager.RecoveryServicesBackup.Models;
using Xunit;

namespace Azure.Mcp.Tools.AzureBackup.Tests.Services;

public class ProtectedItemContractParityTests
{
    [Fact]
    public void DppProtectedItemContract_CoversBackupInstanceProperties()
    {
        var sdkProperties = typeof(DataProtectionBackupInstanceProperties)
            .GetProperties()
            .Select(p => p.Name)
            .ToHashSet(StringComparer.Ordinal);

        var covered = new HashSet<string>(StringComparer.Ordinal)
        {
            nameof(DataProtectionBackupInstanceProperties.FriendlyName),
            nameof(DataProtectionBackupInstanceProperties.DataSourceInfo),
            nameof(DataProtectionBackupInstanceProperties.DataSourceSetInfo),
            nameof(DataProtectionBackupInstanceProperties.PolicyInfo),
            nameof(DataProtectionBackupInstanceProperties.ResourceGuardOperationRequests),
            nameof(DataProtectionBackupInstanceProperties.ProtectionStatus),
            nameof(DataProtectionBackupInstanceProperties.CurrentProtectionState),
            nameof(DataProtectionBackupInstanceProperties.ResourceProtectionErrorDetails),
            nameof(DataProtectionBackupInstanceProperties.ProvisioningState),
            nameof(DataProtectionBackupInstanceProperties.DataSourceAuthCredentials),
            nameof(DataProtectionBackupInstanceProperties.ValidationType),
            nameof(DataProtectionBackupInstanceProperties.IdentityDetails),
            nameof(DataProtectionBackupInstanceProperties.ObjectType),
        };

        var intentionallyExcluded = new HashSet<string>(StringComparer.Ordinal)
        {
            // Deprecated by SDK; use ResourceProtectionErrorDetails and ProtectionStatusErrorDetails.
            "ProtectionErrorDetails",
        };

        var missing = sdkProperties.Except(covered).Except(intentionallyExcluded).OrderBy(s => s).ToArray();
        Assert.True(
            missing.Length == 0,
            $"DPP protected-item property coverage drift detected. Missing: {string.Join(", ", missing)}");
    }

    [Fact]
    public void RsvVmProtectedItemContract_CoversVmProperties()
    {
        var sdkProperties = typeof(IaasVmProtectedItem)
            .GetProperties()
            .Select(p => p.Name)
            .ToHashSet(StringComparer.Ordinal);

        var covered = new HashSet<string>(StringComparer.Ordinal)
        {
            nameof(IaasVmProtectedItem.BackupManagementType),
            nameof(IaasVmProtectedItem.WorkloadType),
            nameof(IaasVmProtectedItem.ContainerName),
            nameof(IaasVmProtectedItem.SourceResourceId),
            nameof(IaasVmProtectedItem.PolicyId),
            nameof(IaasVmProtectedItem.LastRecoverOn),
            nameof(IaasVmProtectedItem.BackupSetName),
            nameof(IaasVmProtectedItem.CreateMode),
            nameof(IaasVmProtectedItem.DeferredDeletedOn),
            nameof(IaasVmProtectedItem.IsScheduledForDeferredDelete),
            nameof(IaasVmProtectedItem.DeferredDeleteTimeRemaining),
            nameof(IaasVmProtectedItem.IsDeferredDeleteScheduleUpcoming),
            nameof(IaasVmProtectedItem.IsRehydrate),
            nameof(IaasVmProtectedItem.ResourceGuardOperationRequests),
            nameof(IaasVmProtectedItem.IsArchiveEnabled),
            nameof(IaasVmProtectedItem.PolicyName),
            nameof(IaasVmProtectedItem.SoftDeleteRetentionPeriodInDays),
            nameof(IaasVmProtectedItem.SoftDeleteRetentionPeriod),
            nameof(IaasVmProtectedItem.VaultId),
            nameof(IaasVmProtectedItem.FriendlyName),
            nameof(IaasVmProtectedItem.VirtualMachineId),
            nameof(IaasVmProtectedItem.ProtectionStatus),
            nameof(IaasVmProtectedItem.ProtectionState),
            nameof(IaasVmProtectedItem.HealthStatus),
            nameof(IaasVmProtectedItem.HealthDetails),
            nameof(IaasVmProtectedItem.KpisHealths),
            nameof(IaasVmProtectedItem.LastBackupStatus),
            nameof(IaasVmProtectedItem.LastBackupOn),
            nameof(IaasVmProtectedItem.ProtectedItemDataId),
            nameof(IaasVmProtectedItem.ExtendedInfo),
            nameof(IaasVmProtectedItem.ExtendedProperties),
            nameof(IaasVmProtectedItem.PolicyType),
        };

        var missing = sdkProperties.Except(covered).OrderBy(s => s).ToArray();
        Assert.True(
            missing.Length == 0,
            $"RSV VM protected-item property coverage drift detected. Missing: {string.Join(", ", missing)}");
    }
}
