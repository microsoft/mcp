// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Azure.Mcp.Tools.IoTHub.Models;

public class RoutingEndpoints
{
    public List<RoutingEndpointProperties>? EventHubs { get; set; }

    public List<RoutingEndpointProperties>? ServiceBusQueues { get; set; }

    public List<RoutingEndpointProperties>? ServiceBusTopics { get; set; }

    public List<RoutingEndpointProperties>? StorageContainers { get; set; }

    public List<RoutingEndpointProperties>? CosmosDBSqlContainers { get; set; }
}
