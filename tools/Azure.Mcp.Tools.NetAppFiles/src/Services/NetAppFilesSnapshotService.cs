// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure;
using Azure.Mcp.Core.Services.Azure;
using Azure.Mcp.Tools.NetAppFiles.Models;
using Azure.ResourceManager.NetApp;
using Azure.ResourceManager.NetApp.Models;

namespace Azure.Mcp.Tools.NetAppFiles.Services;

public class NetAppFilesSnapshotService(IAzureService azureService) : BaseAzureService(azureService), INetAppFilesSnapshotService
{
    public async Task<NetAppFilesSnapshot> GetSnapshotAsync(
        string account,
        string pool,
        string volume,
        string snapshot,
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
        var poolResource = accountResource.GetCapacityPool(pool, cancellationToken).Value;
        var volumeResource = poolResource.GetNetAppVolume(volume, cancellationToken).Value;
        var snapshotResource = volumeResource.GetNetAppVolumeSnapshot(snapshot, cancellationToken).Value;

        return Map(snapshotResource);
    }

    public async Task<NetAppFilesSnapshot> CreateSnapshotAsync(
        string account,
        string pool,
        string volume,
        string snapshot,
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
        var poolResource = accountResource.GetCapacityPool(pool, cancellationToken).Value;
        var volumeResource = poolResource.GetNetAppVolume(volume, cancellationToken).Value;
        var snapshotData = new NetAppVolumeSnapshotData(volumeResource.Data.Location);

        var operation = await volumeResource
            .GetNetAppVolumeSnapshots()
            .CreateOrUpdateAsync(WaitUntil.Started, snapshot, snapshotData, cancellationToken);

        await WaitForLroCompletionAsync(operation, cancellationToken);

        return Map(operation.Value);
    }

    public async Task<NetAppFilesSnapshot> UpdateSnapshotAsync(
        string account,
        string pool,
        string volume,
        string snapshot,
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
        var poolResource = accountResource.GetCapacityPool(pool, cancellationToken).Value;
        var volumeResource = poolResource.GetNetAppVolume(volume, cancellationToken).Value;
        var snapshotResource = volumeResource.GetNetAppVolumeSnapshot(snapshot, cancellationToken).Value;

        var operation = await snapshotResource.UpdateAsync(
            WaitUntil.Started,
            new NetAppVolumeSnapshotPatch(),
            cancellationToken);
        await WaitForLroCompletionAsync(operation, cancellationToken);

        return Map(operation.Value);
    }

    private static NetAppFilesSnapshot Map(NetAppVolumeSnapshotResource snapshot) => new(
        snapshot.Data.Name,
        snapshot.Data.Id.ToString(),
        snapshot.Data.Location.ToString(),
        snapshot.Data.ProvisioningState,
        snapshot.Data.SnapshotId,
        snapshot.Data.Created);
}
