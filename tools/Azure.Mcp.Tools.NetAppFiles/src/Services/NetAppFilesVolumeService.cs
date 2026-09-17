// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure;
using Azure.Core;
using Azure.Mcp.Core.Services.Azure;
using Azure.Mcp.Tools.NetAppFiles.Models;
using Azure.ResourceManager.NetApp;
using Azure.ResourceManager.NetApp.Models;

namespace Azure.Mcp.Tools.NetAppFiles.Services;

public class NetAppFilesVolumeService(IAzureService azureService) : BaseAzureService(azureService), INetAppFilesVolumeService
{
    private const long BytesPerGib = 1024L * 1024L * 1024L;

    public async Task<NetAppFilesVolume> CreateVolumeAsync(
        string account,
        string pool,
        string volume,
        string location,
        string subnetId,
        long quotaGib,
        string serviceLevel,
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
        var volumeData = new NetAppVolumeData(
            new AzureLocation(location),
            volume,
            checked(quotaGib * BytesPerGib),
            new ResourceIdentifier(subnetId))
        {
            ServiceLevel = ParseServiceLevel(serviceLevel)
        };

        var operation = await poolResource
            .GetNetAppVolumes()
            .CreateOrUpdateAsync(WaitUntil.Started, volume, volumeData, cancellationToken);

        await WaitForLroCompletionAsync(operation, cancellationToken);

        return Map(operation.Value);
    }

    private static NetAppFileServiceLevel ParseServiceLevel(string serviceLevel) => serviceLevel.ToLowerInvariant() switch
    {
        "standard" => NetAppFileServiceLevel.Standard,
        "premium" => NetAppFileServiceLevel.Premium,
        "ultra" => NetAppFileServiceLevel.Ultra,
        _ => throw new ArgumentOutOfRangeException(nameof(serviceLevel))
    };

    private static NetAppFilesVolume Map(NetAppVolumeResource volume) => new(
        volume.Data.Name,
        volume.Data.Id.ToString(),
        volume.Data.Location.ToString(),
        volume.Data.ProvisioningState,
        volume.Data.UsageThreshold / BytesPerGib,
        volume.Data.ServiceLevel?.ToString());
}