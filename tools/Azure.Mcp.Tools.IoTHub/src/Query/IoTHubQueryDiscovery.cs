// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Tools.IoTHub.Models;
using Azure.Mcp.Tools.IoTHub.Services;

namespace Azure.Mcp.Tools.IoTHub.Query;

public static class IoTHubQueryDiscovery
{
    // Samples the twin registry for <paramref name="source"/> and returns the discovered queryable fields.
    public static async Task<QueryDiscoveredFields> DiscoverFieldsAsync(
        IIoTHubDeviceService service,
        string source,
        string hubName,
        string resourceGroup,
        string subscription,
        string? tenant,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(service);

        var samplePage = await service.RunQuery(
            $"SELECT * FROM {source}",
            hubName,
            resourceGroup,
            subscription,
            IoTHubQueryLimits.MaxPageSize,
            null,
            tenant,
            cancellationToken);

        return IoTHubQueryFieldDiscoverer.Discover(samplePage.Items);
    }
}
