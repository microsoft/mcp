// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure;
using Azure.Core;
using Azure.Mcp.Core.Services.Azure;
using Azure.Mcp.Tools.NetAppFiles.Models;
using Azure.ResourceManager.NetApp;
using Azure.ResourceManager.NetApp.Models;

namespace Azure.Mcp.Tools.NetAppFiles.Services;

public class NetAppFilesSnapshotPolicyService(IAzureService azureService)
    : BaseAzureService(azureService), INetAppFilesSnapshotPolicyService
{
    public async Task<NetAppFilesSnapshotPolicy> CreateSnapshotPolicyAsync(
        SnapshotPolicyCreateRequest request,
        string subscription,
        string? tenant = null,
        CancellationToken cancellationToken = default)
    {
        var resourceGroupResource = await AzureService.GetResourceGroupResource(
            subscription,
            request.ResourceGroup,
            tenant,
            cancellationToken: cancellationToken)
            ?? throw new KeyNotFoundException($"Resource group '{request.ResourceGroup}' was not found.");

        var accountResource = resourceGroupResource.GetNetAppAccount(request.Account, cancellationToken).Value;
        var policyData = new SnapshotPolicyData(new AzureLocation(request.Location))
        {
            IsEnabled = request.Enabled
        };

        if (request.HourlySnapshotsToKeep.HasValue)
        {
            policyData.HourlySchedule = new SnapshotPolicyHourlySchedule
            {
                Minute = request.HourlyMinute,
                SnapshotsToKeep = request.HourlySnapshotsToKeep
            };
        }

        if (request.DailySnapshotsToKeep.HasValue)
        {
            policyData.DailySchedule = new SnapshotPolicyDailySchedule
            {
                Hour = request.DailyHour,
                Minute = request.DailyMinute,
                SnapshotsToKeep = request.DailySnapshotsToKeep
            };
        }

        if (request.WeeklySnapshotsToKeep.HasValue)
        {
            policyData.WeeklySchedule = new SnapshotPolicyWeeklySchedule
            {
                Day = request.WeeklyDay,
                Hour = request.WeeklyHour,
                Minute = request.WeeklyMinute,
                SnapshotsToKeep = request.WeeklySnapshotsToKeep
            };
        }

        if (request.MonthlySnapshotsToKeep.HasValue)
        {
            policyData.MonthlySchedule = new SnapshotPolicyMonthlySchedule
            {
                DaysOfMonth = request.MonthlyDaysOfMonth,
                Hour = request.MonthlyHour,
                Minute = request.MonthlyMinute,
                SnapshotsToKeep = request.MonthlySnapshotsToKeep
            };
        }

        if (request.Tags is not null)
        {
            foreach (var tag in request.Tags)
            {
                policyData.Tags[tag.Key] = tag.Value;
            }
        }

        var operation = await accountResource
            .GetSnapshotPolicies()
            .CreateOrUpdateAsync(WaitUntil.Started, request.SnapshotPolicy, policyData, cancellationToken);
        await WaitForLroCompletionAsync(operation, cancellationToken);

        return Map(operation.Value);
    }

    public async Task<NetAppFilesSnapshotPolicy> GetSnapshotPolicyAsync(
        string account,
        string snapshotPolicy,
        string resourceGroup,
        string subscription,
        string? tenant = null,
        CancellationToken cancellationToken = default)
    {
        var resourceGroupResource = await AzureService.GetResourceGroupResource(
            subscription,
            resourceGroup,
            tenant,
            cancellationToken: cancellationToken)
            ?? throw new KeyNotFoundException($"Resource group '{resourceGroup}' was not found.");

        var accountResource = resourceGroupResource.GetNetAppAccount(account, cancellationToken).Value;
        var policy = await accountResource.GetSnapshotPolicies().GetAsync(snapshotPolicy, cancellationToken);

        return Map(policy.Value);
    }

    public async Task<NetAppFilesSnapshotPolicy> UpdateSnapshotPolicyAsync(
        SnapshotPolicyUpdateRequest request,
        string subscription,
        string? tenant = null,
        CancellationToken cancellationToken = default)
    {
        var resourceGroupResource = await AzureService.GetResourceGroupResource(
            subscription,
            request.ResourceGroup,
            tenant,
            cancellationToken: cancellationToken)
            ?? throw new KeyNotFoundException($"Resource group '{request.ResourceGroup}' was not found.");

        var accountResource = resourceGroupResource.GetNetAppAccount(request.Account, cancellationToken).Value;
        var policyResource = (await accountResource
            .GetSnapshotPolicies()
            .GetAsync(request.SnapshotPolicy, cancellationToken)).Value;
        var patch = new SnapshotPolicyPatch(new AzureLocation(request.Location ?? policyResource.Data.Location.ToString()));

        if (request.Enabled.HasValue)
        {
            patch.IsEnabled = request.Enabled;
        }

        if (request.HourlyMinute.HasValue || request.HourlySnapshotsToKeep.HasValue)
        {
            patch.HourlySchedule = new SnapshotPolicyHourlySchedule
            {
                Minute = request.HourlyMinute,
                SnapshotsToKeep = request.HourlySnapshotsToKeep
            };
        }

        if (request.DailyHour.HasValue || request.DailyMinute.HasValue || request.DailySnapshotsToKeep.HasValue)
        {
            patch.DailySchedule = new SnapshotPolicyDailySchedule
            {
                Hour = request.DailyHour,
                Minute = request.DailyMinute,
                SnapshotsToKeep = request.DailySnapshotsToKeep
            };
        }

        if (request.WeeklyDay is not null || request.WeeklyHour.HasValue || request.WeeklyMinute.HasValue || request.WeeklySnapshotsToKeep.HasValue)
        {
            patch.WeeklySchedule = new SnapshotPolicyWeeklySchedule
            {
                Day = request.WeeklyDay,
                Hour = request.WeeklyHour,
                Minute = request.WeeklyMinute,
                SnapshotsToKeep = request.WeeklySnapshotsToKeep
            };
        }

        if (request.MonthlyDaysOfMonth is not null || request.MonthlyHour.HasValue || request.MonthlyMinute.HasValue || request.MonthlySnapshotsToKeep.HasValue)
        {
            patch.MonthlySchedule = new SnapshotPolicyMonthlySchedule
            {
                DaysOfMonth = request.MonthlyDaysOfMonth,
                Hour = request.MonthlyHour,
                Minute = request.MonthlyMinute,
                SnapshotsToKeep = request.MonthlySnapshotsToKeep
            };
        }

        if (request.Tags is not null)
        {
            patch.Tags.Clear();
            foreach (var tag in request.Tags)
            {
                patch.Tags[tag.Key] = tag.Value;
            }
        }

        var operation = await policyResource.UpdateAsync(WaitUntil.Started, patch, cancellationToken);
        await WaitForLroCompletionAsync(operation, cancellationToken);

        return Map(operation.Value);
    }

    private static NetAppFilesSnapshotPolicy Map(SnapshotPolicyResource policy) => new(
        policy.Data.Name,
        policy.Data.Id.ToString(),
        policy.Data.Location.ToString(),
        policy.Data.ProvisioningState,
        policy.Data.IsEnabled,
        policy.Data.HourlySchedule?.Minute,
        policy.Data.HourlySchedule?.SnapshotsToKeep,
        policy.Data.DailySchedule?.Hour,
        policy.Data.DailySchedule?.Minute,
        policy.Data.DailySchedule?.SnapshotsToKeep,
        policy.Data.WeeklySchedule?.Day,
        policy.Data.WeeklySchedule?.Hour,
        policy.Data.WeeklySchedule?.Minute,
        policy.Data.WeeklySchedule?.SnapshotsToKeep,
        policy.Data.MonthlySchedule?.DaysOfMonth,
        policy.Data.MonthlySchedule?.Hour,
        policy.Data.MonthlySchedule?.Minute,
        policy.Data.MonthlySchedule?.SnapshotsToKeep,
        new Dictionary<string, string>(policy.Data.Tags));
}
