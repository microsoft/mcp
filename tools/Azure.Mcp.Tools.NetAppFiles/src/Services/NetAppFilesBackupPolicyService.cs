// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure;
using Azure.Core;
using Azure.Mcp.Core.Services.Azure;
using Azure.Mcp.Tools.NetAppFiles.Models;
using Azure.ResourceManager.NetApp;
using Azure.ResourceManager.NetApp.Models;

namespace Azure.Mcp.Tools.NetAppFiles.Services;

public class NetAppFilesBackupPolicyService(IAzureService azureService)
    : BaseAzureService(azureService), INetAppFilesBackupPolicyService
{
    public async Task<NetAppFilesBackupPolicy> CreateBackupPolicyAsync(
        string account,
        string backupPolicy,
        string location,
        int dailyBackupsToKeep,
        int weeklyBackupsToKeep,
        int monthlyBackupsToKeep,
        bool enabled,
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
        var backupPolicyData = new NetAppBackupPolicyData(new AzureLocation(location))
        {
            DailyBackupsToKeep = dailyBackupsToKeep,
            WeeklyBackupsToKeep = weeklyBackupsToKeep,
            MonthlyBackupsToKeep = monthlyBackupsToKeep,
            IsEnabled = enabled
        };
        var operation = await accountResource
            .GetNetAppBackupPolicies()
            .CreateOrUpdateAsync(WaitUntil.Started, backupPolicy, backupPolicyData, cancellationToken);

        await WaitForLroCompletionAsync(operation, cancellationToken);

        return Map(operation.Value);
    }

    public async Task<NetAppFilesBackupPolicy> GetBackupPolicyAsync(
        string account,
        string backupPolicy,
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
        var backupPolicyResource = accountResource.GetNetAppBackupPolicy(backupPolicy, cancellationToken).Value;

        return Map(backupPolicyResource);
    }

    public async Task<NetAppFilesBackupPolicy> UpdateBackupPolicyAsync(
        string account,
        string backupPolicy,
        int? dailyBackupsToKeep,
        int? weeklyBackupsToKeep,
        int? monthlyBackupsToKeep,
        bool? enabled,
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
        var backupPolicyResource = accountResource.GetNetAppBackupPolicy(backupPolicy, cancellationToken).Value;
        var patch = new NetAppBackupPolicyPatch(backupPolicyResource.Data.Location)
        {
            DailyBackupsToKeep = dailyBackupsToKeep,
            WeeklyBackupsToKeep = weeklyBackupsToKeep,
            MonthlyBackupsToKeep = monthlyBackupsToKeep,
            IsEnabled = enabled
        };

        var operation = await backupPolicyResource.UpdateAsync(WaitUntil.Started, patch, cancellationToken);
        await WaitForLroCompletionAsync(operation, cancellationToken);

        return Map(operation.Value);
    }

    private static NetAppFilesBackupPolicy Map(NetAppBackupPolicyResource backupPolicy) => new(
        backupPolicy.Data.Name,
        backupPolicy.Data.Id.ToString(),
        backupPolicy.Data.Location.ToString(),
        backupPolicy.Data.ProvisioningState,
        backupPolicy.Data.DailyBackupsToKeep,
        backupPolicy.Data.WeeklyBackupsToKeep,
        backupPolicy.Data.MonthlyBackupsToKeep,
        backupPolicy.Data.IsEnabled);
}
