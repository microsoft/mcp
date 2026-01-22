// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Core.Options;
using Azure.Mcp.Core.Services.Azure;
using Azure.Mcp.Core.Services.Azure.Subscription;
using Azure.Mcp.Core.Services.Azure.Tenant;
using Azure.Mcp.Tools.Compute.Models;
using Azure.ResourceManager;
using Azure.ResourceManager.Compute;

namespace Azure.Mcp.Tools.Compute.Services;

/// <summary>
/// Service implementation for Azure Compute operations.
/// </summary>
public class ComputeService(ISubscriptionService subscriptionService, ITenantService tenantService)
    : BaseAzureService(tenantService), IComputeService
{
    private readonly ISubscriptionService _subscriptionService = subscriptionService ?? throw new ArgumentNullException(nameof(subscriptionService));

    /// <inheritdoc/>
    public async Task<Disk> GetDiskAsync(
        string diskName,
        string resourceGroup,
        string subscription,
        string? tenant = null,
        RetryPolicyOptions? retryPolicy = null,
        CancellationToken cancellationToken = default)
    {
        var subscriptionResource = await _subscriptionService.GetSubscription(subscription, tenant, retryPolicy, cancellationToken);
        var resourceGroupResource = await subscriptionResource.GetResourceGroups().GetAsync(resourceGroup, cancellationToken);
        var diskResource = await resourceGroupResource.Value.GetManagedDisks().GetAsync(diskName, cancellationToken);

        var disk = diskResource.Value.Data;

        return ConvertToDiskModel(diskResource.Value, resourceGroup);
    }

    /// <inheritdoc/>
    public async Task<List<Disk>> ListDisksAsync(
        string subscription,
        string? resourceGroup = null,
        string? tenant = null,
        RetryPolicyOptions? retryPolicy = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var subscriptionResource = await _subscriptionService.GetSubscription(subscription, tenant, retryPolicy, cancellationToken);
            var disks = new List<Disk>();

            if (!string.IsNullOrEmpty(resourceGroup))
            {
                // List disks in specific resource group
                var rg = await subscriptionResource.GetResourceGroupAsync(resourceGroup, cancellationToken);
                if (rg?.Value == null)
                {
                    throw new InvalidOperationException($"Resource group '{resourceGroup}' not found.");
                }

                await foreach (var diskResource in rg.Value.GetManagedDisks().GetAllAsync(cancellationToken))
                {
                    disks.Add(ConvertToDiskModel(diskResource, resourceGroup));
                }
            }
            else
            {
                // List all disks in subscription
                await foreach (var diskResource in subscriptionResource.GetManagedDisksAsync(cancellationToken))
                {
                    var rgName = ExtractResourceGroupFromId(diskResource.Id.ToString());
                    disks.Add(ConvertToDiskModel(diskResource, rgName));
                }
            }

            return disks;
        }
        catch (Azure.RequestFailedException)
        {
            throw;
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Failed to list disks: {ex.Message}", ex);
        }
    }

    private static Disk ConvertToDiskModel(ManagedDiskResource diskResource, string resourceGroup)
    {
        var disk = diskResource.Data;
        return new Disk
        {
            Name = disk.Name,
            Id = disk.Id?.ToString(),
            ResourceGroup = resourceGroup,
            Location = disk.Location.ToString(),
            SkuName = disk.Sku?.Name.ToString(),
            SkuTier = disk.Sku?.Tier,
            DiskSizeGB = disk.DiskSizeGB,
            DiskState = disk.DiskState?.ToString(),
            TimeCreated = disk.TimeCreated,
            OSType = disk.OSType?.ToString(),
            ProvisioningState = disk.ProvisioningState,
            Tags = disk.Tags?.ToDictionary(kvp => kvp.Key, kvp => kvp.Value)
        };
    }

    private static string ExtractResourceGroupFromId(string resourceId)
    {
        // Resource ID format: /subscriptions/{guid}/resourceGroups/{rgName}/providers/...
        var parts = resourceId.Split('/', StringSplitOptions.RemoveEmptyEntries);
        var rgIndex = Array.IndexOf(parts, "resourceGroups");
        return rgIndex >= 0 && rgIndex + 1 < parts.Length ? parts[rgIndex + 1] : string.Empty;
    }
}
