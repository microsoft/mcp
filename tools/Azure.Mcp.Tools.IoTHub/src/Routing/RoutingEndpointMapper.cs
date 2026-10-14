// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Tools.IoTHub.Models;

namespace Azure.Mcp.Tools.IoTHub.Routing;

internal static class RoutingEndpointMapper
{
    internal static List<RoutingEndpointDetails> ConvertToRoutingEndpointDetailsList(
        RoutingEndpoints? endpoints)
    {
        if (endpoints is null)
        {
            return [];
        }

        var results = new List<RoutingEndpointDetails>();
        AppendEndpoints(results, endpoints.EventHubs, "EventHub", isContainer: false);
        AppendEndpoints(results, endpoints.ServiceBusQueues, "ServiceBusQueue", isContainer: false);
        AppendEndpoints(results, endpoints.ServiceBusTopics, "ServiceBusTopic", isContainer: false);
        AppendEndpoints(results, endpoints.StorageContainers, "StorageContainer", isContainer: true);
        AppendEndpoints(results, endpoints.CosmosDBSqlContainers, "CosmosDBSqlContainer", isContainer: true);
        return results;
    }

    private static void AppendEndpoints(
        List<RoutingEndpointDetails> results,
        List<RoutingEndpointProperties>? endpoints,
        string endpointType,
        bool isContainer)
    {
        foreach (var endpoint in endpoints ?? [])
        {
            results.Add(new RoutingEndpointDetails(
                Name: endpoint.Name ?? string.Empty,
                EndpointType: endpointType,
                EndpointResourceName: GetEndpointResourceName(endpoint.EndpointUri),
                SubscriptionId: endpoint.SubscriptionId,
                ResourceGroup: endpoint.ResourceGroup,
                EndpointUri: endpoint.EndpointUri,
                EntityPath: isContainer ? null : endpoint.EntityPath,
                ContainerName: isContainer ? endpoint.ContainerName : null,
                DatabaseName: endpointType == "CosmosDBSqlContainer" ? endpoint.DatabaseName : null,
                AuthenticationType: endpoint.AuthenticationType,
                BatchFrequencyInSeconds: endpoint.BatchFrequencyInSeconds,
                EndpointId: endpoint.Id));
        }
    }

    // IoT Hub endpoint ids are opaque and key-based endpoints have no endpointUri, so only the endpoint URI host
    // names the target namespace or account. Without it the target stays unresolved instead of being guessed.
    private static string? GetEndpointResourceName(string? endpointUri)
    {
        if (!Uri.TryCreate(endpointUri, UriKind.Absolute, out var uri) ||
            string.IsNullOrWhiteSpace(uri.Host))
        {
            return null;
        }
        var endpointName = uri.Host.Split('.')[0];
        return string.IsNullOrWhiteSpace(endpointName) ? null : endpointName;
    }
}
