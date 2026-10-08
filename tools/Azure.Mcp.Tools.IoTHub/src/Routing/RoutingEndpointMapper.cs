// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Core;
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
                EndpointResourceName: isContainer
                    ? GetEndpointResourceName(
                        endpoint.Id,
                        endpoint.EndpointUri,
                        endpoint.ContainerName,
                        preferEndpointUri: true)
                    : GetEndpointResourceName(
                        endpoint.Id,
                        endpoint.EndpointUri,
                        endpoint.EntityPath),
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

    private static string? GetEndpointResourceName(
        string? id,
        string? endpointUri,
        string? fallback,
        bool preferEndpointUri = false)
    {
        if (preferEndpointUri && TryGetEndpointHostName(endpointUri, out var preferredName))
        {
            return preferredName;
        }
        if (!string.IsNullOrWhiteSpace(id))
        {
            try
            {
                return new ResourceIdentifier(id).Name;
            }
            catch (Exception ex) when (ex is ArgumentException or FormatException)
            {
            }
        }
        return TryGetEndpointHostName(endpointUri, out var endpointName) ? endpointName : fallback;
    }

    private static bool TryGetEndpointHostName(string? endpointUri, out string? endpointName)
    {
        endpointName = null;
        if (!Uri.TryCreate(endpointUri, UriKind.Absolute, out var uri) ||
            string.IsNullOrWhiteSpace(uri.Host))
        {
            return false;
        }
        endpointName = uri.Host.Split('.')[0];
        return !string.IsNullOrWhiteSpace(endpointName);
    }
}
