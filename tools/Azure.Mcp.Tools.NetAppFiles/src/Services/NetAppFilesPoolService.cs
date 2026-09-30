// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure;
using Azure.Core;
using Azure.Mcp.Core.Services.Azure;
using Azure.Mcp.Tools.NetAppFiles.Models;
using Azure.ResourceManager.NetApp;
using Azure.ResourceManager.NetApp.Models;

namespace Azure.Mcp.Tools.NetAppFiles.Services;

public class NetAppFilesPoolService(IAzureService azureService)
    : BaseAzureService(azureService), INetAppFilesPoolService
{
    public async Task<NetAppFilesPool> GetPoolAsync(
        string account,
        string pool,
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
        var poolResource = accountResource.GetCapacityPool(pool, cancellationToken);

        return Map(poolResource.Value);
    }

    public async Task<NetAppFilesPool> CreatePoolAsync(
        string account,
        string pool,
        long sizeInBytes,
        string serviceLevel,
        string resourceGroup,
        string subscription,
        string? location = null,
        string? qosType = null,
        bool? coolAccess = null,
        string? encryptionType = null,
        int? customThroughputMibps = null,
        IReadOnlyDictionary<string, string>? tags = null,
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
        var poolLocation = location is null ? accountResource.Data.Location : new AzureLocation(location);
        var poolData = new CapacityPoolData(poolLocation, sizeInBytes, new NetAppFileServiceLevel(serviceLevel))
        {
            IsCoolAccessEnabled = coolAccess,
            CustomThroughputMibpsInt = customThroughputMibps
        };

        if (qosType is not null)
        {
            poolData.QosType = new CapacityPoolQosType(qosType);
        }

        if (encryptionType is not null)
        {
            poolData.EncryptionType = new CapacityPoolEncryptionType(encryptionType);
        }

        if (tags is not null)
        {
            foreach (var tag in tags)
            {
                poolData.Tags[tag.Key] = tag.Value;
            }
        }

        var operation = await accountResource
            .GetCapacityPools()
            .CreateOrUpdateAsync(WaitUntil.Started, pool, poolData, cancellationToken);

        await WaitForLroCompletionAsync(operation, cancellationToken);

        return Map(operation.Value);
    }

    public async Task<NetAppFilesPool> UpdatePoolAsync(
        string account,
        string pool,
        string resourceGroup,
        string subscription,
        long? sizeInBytes = null,
        string? qosType = null,
        bool? coolAccess = null,
        int? customThroughputMibps = null,
        IReadOnlyDictionary<string, string>? tags = null,
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
        var patch = new CapacityPoolPatch(poolResource.Data.Location)
        {
            Size = sizeInBytes,
            IsCoolAccessEnabled = coolAccess,
            CustomThroughputMibpsInt = customThroughputMibps
        };

        if (qosType is not null)
        {
            patch.QosType = new CapacityPoolQosType(qosType);
        }

        if (tags is not null)
        {
            foreach (var tag in tags)
            {
                patch.Tags[tag.Key] = tag.Value;
            }
        }

        var operation = await poolResource.UpdateAsync(WaitUntil.Started, patch, cancellationToken);
        await WaitForLroCompletionAsync(operation, cancellationToken);

        return Map(operation.Value);
    }

    private static NetAppFilesPool Map(CapacityPoolResource pool) => new(
        pool.Data.Name,
        pool.Data.Id.ToString(),
        pool.Data.Location.ToString(),
        pool.Data.Size,
        pool.Data.ServiceLevel.ToString(),
        pool.Data.QosType?.ToString(),
        pool.Data.IsCoolAccessEnabled,
        pool.Data.EncryptionType?.ToString(),
        pool.Data.CustomThroughputMibpsInt,
        new Dictionary<string, string>(pool.Data.Tags),
        pool.Data.ProvisioningState?.ToString());
}
