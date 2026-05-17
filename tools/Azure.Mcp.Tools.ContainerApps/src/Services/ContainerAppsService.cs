// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json;
using Azure.Mcp.Core.Services.Azure;
using Azure.Mcp.Core.Services.Azure.Subscription;
using Azure.Mcp.Core.Services.Azure.Tenant;
using Azure.Mcp.Tools.ContainerApps.Models;
using Microsoft.Mcp.Core.Options;

namespace Azure.Mcp.Tools.ContainerApps.Services;

public sealed class ContainerAppsService(ISubscriptionService subscriptionService, ITenantService tenantService)
    : BaseAzureResourceService(subscriptionService, tenantService), IContainerAppsService
{
    public async Task<ResourceQueryResults<ContainerAppInfo>> ListContainerApps(
        string subscription,
        string? resourceGroup = null,
        RetryPolicyOptions? retryPolicy = null,
        CancellationToken cancellationToken = default)
    {
        ValidateRequiredParameters((nameof(subscription), subscription));

        var containerApps = await ExecuteResourceQueryAsync(
            "Microsoft.App/containerApps",
            resourceGroup,
            subscription,
            retryPolicy,
            ConvertToContainerAppInfoModel,
            cancellationToken: cancellationToken);

        return containerApps;
    }

    private static ContainerAppInfo ConvertToContainerAppInfoModel(JsonElement item)
    {
        var name = item.TryGetProperty("name", out var nameElement) ? nameElement.GetString() ?? string.Empty : string.Empty;
        var location = item.TryGetProperty("location", out var locationElement) ? locationElement.GetString() : null;
        var resourceGroup = item.TryGetProperty("resourceGroup", out var rgElement) ? rgElement.GetString() : null;

        string? managedEnvironmentId = null;
        string? provisioningState = null;

        if (item.TryGetProperty("properties", out var properties))
        {
            managedEnvironmentId = properties.TryGetProperty("managedEnvironmentId", out var envElement) ? envElement.GetString() : null;
            provisioningState = properties.TryGetProperty("provisioningState", out var stateElement) ? stateElement.GetString() : null;
        }

        return new ContainerAppInfo(name, location, resourceGroup, managedEnvironmentId, provisioningState);
    }
}
