// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure;
using Azure.Core;
using Azure.Mcp.Core.Services.Azure;
using Azure.Mcp.Tools.NetAppFiles.Models;
using Azure.ResourceManager.NetApp;
using Azure.ResourceManager.NetApp.Models;

namespace Azure.Mcp.Tools.NetAppFiles.Services;

public class NetAppFilesBackupService(IAzureService azureService) : BaseAzureService(azureService), INetAppFilesBackupService
{
    public async Task<NetAppFilesBackup> CreateBackupAsync(
        string account,
        string backupVault,
        string backup,
        ResourceIdentifier volumeResourceId,
        string? label,
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
        var backupVaultResource = accountResource.GetNetAppBackupVault(backupVault, cancellationToken).Value;
        var backupData = new NetAppBackupData(volumeResourceId)
        {
            Label = label
        };

        var operation = await backupVaultResource
            .GetNetAppBackupVaultBackups()
            .CreateOrUpdateAsync(WaitUntil.Started, backup, backupData, cancellationToken);

        await WaitForLroCompletionAsync(operation, cancellationToken);

        return Map(operation.Value);
    }

    public async Task<NetAppFilesBackup> GetBackupAsync(
        string account,
        string backupVault,
        string backup,
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
        var backupVaultResource = accountResource.GetNetAppBackupVault(backupVault, cancellationToken).Value;
        var backupResource = backupVaultResource
            .GetNetAppBackupVaultBackup(backup, cancellationToken);

        return Map(backupResource.Value);
    }

    public async Task<NetAppFilesBackup> UpdateBackupAsync(
        string account,
        string backupVault,
        string backup,
        string label,
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
        var backupVaultResource = accountResource.GetNetAppBackupVault(backupVault, cancellationToken).Value;
        var backupResource = backupVaultResource.GetNetAppBackupVaultBackup(backup, cancellationToken).Value;
        var patch = new NetAppBackupVaultBackupPatch
        {
            Label = label
        };

        var operation = await backupResource.UpdateAsync(WaitUntil.Started, patch, cancellationToken);
        await WaitForLroCompletionAsync(operation, cancellationToken);

        return Map(operation.Value);
    }

    private static NetAppFilesBackup Map(NetAppBackupVaultBackupResource backup) => new(
        backup.Data.Name,
        backup.Data.Id.ToString(),
        backup.Data.ProvisioningState,
        backup.Data.BackupType?.ToString(),
        backup.Data.Label,
        backup.Data.Size,
        backup.Data.VolumeResourceId?.ToString());
}
