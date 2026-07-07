// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Core;
using Azure.Mcp.Core.Services.Azure;
using Azure.Mcp.Core.Services.Azure.Tenant;
using Azure.ResourceManager.Datadog;
using Microsoft.Extensions.Logging;

namespace Azure.Mcp.Tools.AzureIsv.Services.Datadog;

public partial class DatadogService(ITenantService tenantService, ILogger<DatadogService> logger)
    : BaseAzureService(tenantService), IDatadogService
{
    private readonly ILogger<DatadogService> _logger = logger ?? throw new ArgumentNullException(nameof(logger));

    public async Task<List<string>> ListMonitoredResources(string resourceGroup, string subscription, string datadogResource, CancellationToken cancellationToken = default)
    {
        var tenantId = await ResolveTenantIdAsync(null, cancellationToken);
        var armClient = await CreateArmClientAsync(tenantIdOrName: tenantId, retryPolicy: null, armClientOptions: null, cancellationToken);

        var resourceId = $"/subscriptions/{subscription}/resourceGroups/{resourceGroup}/providers/Microsoft.Datadog/monitors/{datadogResource}";

        ResourceIdentifier id = new(resourceId);
        var datadogMonitorResource = armClient.GetDatadogMonitorResource(id);
        var monitoredResources = datadogMonitorResource.GetMonitoredResources(cancellationToken);

        var resourceList = new List<string>();
        foreach (var resource in monitoredResources)
        {
            var resourceIdSegments = resource.Id.ToString().Split('/');
            var lastSegment = resourceIdSegments[^1];
            resourceList.Add(lastSegment);
        }

        return resourceList;
    }
}
