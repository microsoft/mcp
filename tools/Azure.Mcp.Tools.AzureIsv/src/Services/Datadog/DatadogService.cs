// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Core;
using Azure.Mcp.Core.Services.Azure;
using Azure.Mcp.Core.Services.Azure.Tenant;
using Azure.ResourceManager.Datadog;

namespace Azure.Mcp.Tools.AzureIsv.Services.Datadog;

public partial class DatadogService : BaseAzureService, IDatadogService
{
    public DatadogService(ITenantService? tenantService = null) : base(tenantService)
    {
    }

    public async Task<List<string>> ListMonitoredResources(string resourceGroup, string subscription, string datadogResource)
    {
        try
        {
            var tenantId = await ResolveTenantIdAsync(null);
            var armClient = await CreateArmClientAsync(tenant: tenantId, retryPolicy: null);

            var resourceId = $"/subscriptions/{subscription}/resourceGroups/{resourceGroup}/providers/Microsoft.Datadog/monitors/{datadogResource}";

            ResourceIdentifier id = new ResourceIdentifier(resourceId);
            var datadogMonitorResource = armClient.GetDatadogMonitorResource(id);
            var monitoredResources = datadogMonitorResource.GetMonitoredResources();

            var resourceList = new List<string>();
            foreach (var resource in monitoredResources)
            {
                var resourceIdSegments = resource.Id.ToString().Split('/');
                var lastSegment = resourceIdSegments[^1];
                resourceList.Add(lastSegment);
            }

            return resourceList;
        }
        catch (Exception ex)
        {
            throw new Exception($"Error listing monitored resources: {ex.Message}", ex);
        }
    }
}
