// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Core;
using Azure.Mcp.Core.Services.Azure;
using Azure.Mcp.Tools.IoTHub.Commands;
using Azure.Mcp.Tools.IoTHub.Models;
using Azure.ResourceManager.Resources;
using Microsoft.Extensions.Logging;

namespace Azure.Mcp.Tools.IoTHub.Services;

public class IoTHubService(IAzureService azureService, ILogger<IoTHubService> logger)
    : BaseAzureService(azureService), IIoTHubService
{
    private readonly ILogger<IoTHubService> _logger = logger ?? throw new ArgumentNullException(nameof(logger));

    public async Task<IoTHubDescription> GetIoTHub(
        string hubName,
        string resourceGroup,
        string subscription,
        string? tenant = null,
        CancellationToken cancellationToken = default)
    {
        ValidateRequiredParameters(
            (nameof(subscription), subscription),
            (nameof(resourceGroup), resourceGroup),
            (nameof(hubName), hubName));

        try
        {
            var subscriptionResource = await AzureService.GetSubscription(
                subscription,
                tenant,
                cancellationToken: cancellationToken);
            var armClient = await CreateArmClientAsync(
                tenant,
                cancellationToken: cancellationToken);
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
