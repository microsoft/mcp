// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Core;
using Azure.Mcp.Core.Services.Azure;
using Azure.Mcp.Core.Services.Azure.Subscription;
using Azure.Mcp.Core.Services.Azure.Tenant;
using Azure.Mcp.Tools.IoTHub.Commands;
using Azure.Mcp.Tools.IoTHub.Models;
using Azure.ResourceManager.Resources;
using Microsoft.Extensions.Logging;
using Microsoft.Mcp.Core.Options;

namespace Azure.Mcp.Tools.IoTHub.Services;

public class IoTHubService(
    ISubscriptionService subscriptionService,
    ITenantService tenantService,
    ILogger<IoTHubService> logger)
    : BaseAzureService(tenantService), IIoTHubService
{
    private readonly ISubscriptionService _subscriptionService = subscriptionService;
    private readonly ILogger<IoTHubService> _logger = logger ?? throw new ArgumentNullException(nameof(logger));

    public async Task<IoTHubDescription> GetIoTHub(
        string hubName,
        string resourceGroup,
        string subscription,
        string? tenant = null,
        RetryPolicyOptions? retryPolicy = null,
        CancellationToken cancellationToken = default)
    {
        ValidateRequiredParameters(
            (nameof(subscription), subscription),
            (nameof(resourceGroup), resourceGroup),
            (nameof(hubName), hubName));

        try
        {
            var subscriptionResource = await _subscriptionService.GetSubscription(subscription, tenant, retryPolicy, cancellationToken);
            var armClient = await CreateArmClientAsync(tenant, retryPolicy, cancellationToken: cancellationToken);
            var iotHubResourceId = new ResourceIdentifier(
                $"/subscriptions/{subscriptionResource.Data.SubscriptionId}/resourceGroups/{resourceGroup}/providers/Microsoft.Devices/IotHubs/{hubName}");
            var hub = await armClient.GetGenericResource(iotHubResourceId).GetAsync(cancellationToken);

            return ConvertToIoTHubDescription(hub.Value.Data);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving IoT Hub '{HubName}' in resource group '{ResourceGroup}' and subscription '{Subscription}'", hubName, resourceGroup, subscription);
            throw;
        }
    }

    private static IoTHubDescription ConvertToIoTHubDescription(GenericResourceData hub)
    {
        var properties = hub.Properties?.ToObjectFromJson(IoTHubJsonContext.Default.IoTHubProperties);

        return new IoTHubDescription(
            hub.Id.ToString(),
            hub.Name,
            hub.Location.ToString(),
            hub.Id?.ResourceGroupName ?? string.Empty,
            hub.Id?.SubscriptionId ?? string.Empty,
            hub.Sku?.Name ?? string.Empty,
            hub.Sku?.Capacity ?? 0,
            properties?.State ?? string.Empty,
            properties?.HostName ?? string.Empty
        );
    }
}
