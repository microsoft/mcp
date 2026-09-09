// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.ResourceManager.DataProtectionBackup.Models;
using Azure.ResourceManager.DataProtectionBackup;
using Azure.ResourceManager.RecoveryServicesBackup;
using Azure.ResourceManager.RecoveryServicesBackup.Models;
using Azure.ResourceManager;
using Azure.ResourceManager.Models;
using Azure;
using Azure.Core;
using Azure.Mcp.Tools.AzureBackup.Models;
using Azure.Mcp.Tools.AzureBackup.Services;
using System.Reflection;
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

    [Fact]
    public void RsvVmProtectedItemMapper_UsesSdkProtectionStatusForTopLevelAndDetails()
    {
        var vmItem = new IaasComputeVmProtectedItem
        {
            ProtectionStatus = "ProtectionStatusFromSdk"
        };

        var mapped = InvokeRsvMapper(vmItem);

        Assert.Equal("ProtectionStatusFromSdk", mapped.ProtectionStatus);
        Assert.NotNull(mapped.Details);
        Assert.Equal("ProtectionStatusFromSdk", mapped.Details!.ProtectionStatus);
    }

    [Fact]
    public void RsvVmWorkloadMapper_DoesNotShiftProtectionStateIntoHealthStatus()
    {
        var workloadItem = new VmWorkloadSqlDatabaseProtectedItem();
        SetProperty(workloadItem, "ProtectionState", "Protected");

        var mapped = InvokeRsvMapper(workloadItem);

        Assert.NotNull(mapped.Details);
        Assert.Equal(mapped.ProtectionStatus, mapped.Details!.ProtectionStatus);
        Assert.Null(mapped.Details.HealthStatus);
    }

    [Fact]
    public void RsvFileShareMapper_PopulatesDetailsForFileShareItems()
    {
        var fileShareItem = new FileshareProtectedItem();
        SetProperty(fileShareItem, "ProtectionState", "Protected");

        var mapped = InvokeRsvMapper(fileShareItem);

        Assert.NotNull(mapped.Details);
        Assert.Equal(mapped.ProtectionStatus, mapped.Details!.ProtectionStatus);
        Assert.Equal(mapped.DatasourceType, mapped.Details.WorkloadType);
    }

    [Fact]
    public void DppProtectedItemMapper_MapsNestedDatasourceAndPolicyPayloads()
    {
        var datasource = new DataSourceInfo(new ResourceIdentifier("/subscriptions/00000000-0000-0000-0000-000000000000/resourceGroups/rg/providers/Microsoft.Compute/disks/disk01"))
        {
            DataSourceType = "Microsoft.Compute/disks"
        };
        SetProperty(datasource, "ResourceUriString", "https://example.local/resource");

        var policyInfo = new BackupInstancePolicyInfo(new ResourceIdentifier("/subscriptions/00000000-0000-0000-0000-000000000000/resourceGroups/rg/providers/Microsoft.DataProtection/backupVaults/v1/backupPolicies/p1"));

        var properties = new DataProtectionBackupInstanceProperties(datasource, policyInfo, "friendly-item")
        {
            ObjectType = "BackupInstance"
        };

        var data = new DataProtectionBackupInstanceData
        {
            Properties = properties
        };

        var mapped = InvokeDppMapper(data);

        Assert.NotNull(mapped.DppDetails);
        Assert.NotNull(mapped.DppDetails!.DataSourceInfo);
        Assert.Equal("https://example.local/resource", mapped.DppDetails.DataSourceInfo!.ResourceUriString);
        Assert.NotNull(mapped.DppDetails.PolicyInfo);
        Assert.Equal(policyInfo.PolicyId?.ToString(), mapped.DppDetails.PolicyInfo!.PolicyId);
    }

    [Fact]
    public void DppErrorMapper_MapsNestedDetailsAndInnerError()
    {
        var child = new DataProtectionBackupUserFacingError
        {
            Code = "ChildCode",
            Message = "child"
        };

        var parent = new DataProtectionBackupUserFacingError
        {
            Code = "ParentCode",
            Message = "parent",
            InnerError = new DataProtectionBackupInnerError { Code = "InnerCode" },
        };
        parent.Details.Add(child);
        parent.Properties.Add("key", "value");

        var method = typeof(DppBackupOperations).GetMethod(
            "MapToDppError",
            BindingFlags.NonPublic | BindingFlags.Static,
            binder: null,
            [typeof(DataProtectionBackupUserFacingError)],
            modifiers: null);
        Assert.NotNull(method);

        var mapped = Assert.IsType<ProtectedItemDppError>(method!.Invoke(null, [parent]));

        Assert.Equal("ParentCode", mapped.Code);
        Assert.NotNull(mapped.Details);
        Assert.Single(mapped.Details!);
        Assert.Equal("ChildCode", mapped.Details[0].Code);
        Assert.NotNull(mapped.InnerError);
        Assert.Equal("InnerCode", mapped.InnerError!.Code);
        Assert.NotNull(mapped.Properties);
        Assert.Equal("value", mapped.Properties!["key"]);
    }

    private static ProtectedItemInfo InvokeRsvMapper(BackupGenericProtectedItem item)
    {
        var data = new BackupProtectedItemData(new AzureLocation("eastus"))
        {
            Properties = item
        };

        var method = typeof(RsvBackupOperations).GetMethod("MapToProtectedItemInfo", BindingFlags.NonPublic | BindingFlags.Static);
        Assert.NotNull(method);

        var result = method!.Invoke(null, [data]);
        return Assert.IsType<ProtectedItemInfo>(result);
    }

    private static ProtectedItemInfo InvokeDppMapper(DataProtectionBackupInstanceData data)
    {
        var method = typeof(DppBackupOperations).GetMethod("MapToProtectedItemInfo", BindingFlags.NonPublic | BindingFlags.Static);
        Assert.NotNull(method);

        var result = method!.Invoke(null, [data]);
        return Assert.IsType<ProtectedItemInfo>(result);
    }

    private static void SetProperty(object target, string propertyName, object? value)
    {
        var property = target.GetType().GetProperty(propertyName, BindingFlags.Public | BindingFlags.Instance);
        Assert.NotNull(property);
        Assert.True(property!.CanWrite, $"Property '{propertyName}' on '{target.GetType().Name}' is not writable.");

        var converted = ConvertValueForProperty(property.PropertyType, value);
        property.SetValue(target, converted);
    }

    private static void SetEnumProperty(object target, string propertyName, string preferredName)
    {
        var property = target.GetType().GetProperty(propertyName, BindingFlags.Public | BindingFlags.Instance);
        Assert.NotNull(property);
        Assert.True(property!.CanWrite, $"Property '{propertyName}' on '{target.GetType().Name}' is not writable.");

        var enumType = Nullable.GetUnderlyingType(property.PropertyType) ?? property.PropertyType;
        Assert.True(enumType.IsEnum, $"Property '{propertyName}' on '{target.GetType().Name}' is not an enum.");

        var enumNames = Enum.GetNames(enumType);
        var selectedName = enumNames.FirstOrDefault(n => n.Equals(preferredName, StringComparison.OrdinalIgnoreCase)) ?? enumNames.First();
        var enumValue = Enum.Parse(enumType, selectedName, ignoreCase: true);
        property.SetValue(target, enumValue);
    }

    private static object? ConvertValueForProperty(Type propertyType, object? value)
    {
        if (value is null)
        {
            return null;
        }

        var targetType = Nullable.GetUnderlyingType(propertyType) ?? propertyType;

        if (targetType.IsInstanceOfType(value))
        {
            return value;
        }

        if (targetType == typeof(string))
        {
            return value.ToString();
        }

        if (value is string text)
        {
            if (targetType.IsEnum)
            {
                return Enum.Parse(targetType, text, ignoreCase: true);
            }

            var stringCtor = targetType.GetConstructor([typeof(string)]);
            if (stringCtor is not null)
            {
                return stringCtor.Invoke([text]);
            }

            var parseMethod = targetType.GetMethod(
                "Parse",
                BindingFlags.Public | BindingFlags.Static,
                binder: null,
                [typeof(string)],
                modifiers: null);
            if (parseMethod is not null)
            {
                return parseMethod.Invoke(null, [text]);
            }
        }

        if (value is not IConvertible)
        {
            return value;
        }

        return Convert.ChangeType(value, targetType);
    }
}
