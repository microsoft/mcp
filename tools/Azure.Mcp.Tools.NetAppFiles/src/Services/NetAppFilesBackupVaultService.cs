// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure;
using Azure.Core;
using Azure.Mcp.Core.Services.Azure;
using Azure.Mcp.Tools.NetAppFiles.Models;
using Azure.ResourceManager.NetApp;
using Azure.ResourceManager.NetApp.Models;

namespace Azure.Mcp.Tools.NetAppFiles.Services;

public class NetAppFilesBackupVaultService(IAzureService azureService)
    : BaseAzureService(azureService), INetAppFilesBackupVaultService
{
    public async Task<NetAppFilesBackupVault> CreateBackupVaultAsync(
        string account,
        string backupVault,
        string location,
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
        var backupVaultData = new NetAppBackupVaultData(new AzureLocation(location));
        var operation = await accountResource
            .GetNetAppBackupVaults()
            .CreateOrUpdateAsync(WaitUntil.Started, backupVault, backupVaultData, cancellationToken);

        await WaitForLroCompletionAsync(operation, cancellationToken);

        return Map(operation.Value);
    }

    public async Task<NetAppFilesBackupVault> GetBackupVaultAsync(
        string account,
        string backupVault,
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

        return Map(backupVaultResource);
    }

    public async Task<NetAppFilesBackupVault> UpdateBackupVaultAsync(
        string account,
        string backupVault,
        IDictionary<string, string> tags,
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
        var patch = new NetAppBackupVaultPatch();

        foreach (var tag in tags)
        {
            patch.Tags.Add(tag);
        }

        var operation = await backupVaultResource.UpdateAsync(WaitUntil.Started, patch, cancellationToken);
        await WaitForLroCompletionAsync(operation, cancellationToken);

        return Map(operation.Value);
    }

    private static NetAppFilesBackupVault Map(NetAppBackupVaultResource backupVault) => new(
        backupVault.Data.Name,
        backupVault.Data.Id.ToString(),
        backupVault.Data.Location.ToString(),
        backupVault.Data.ProvisioningState);
}
