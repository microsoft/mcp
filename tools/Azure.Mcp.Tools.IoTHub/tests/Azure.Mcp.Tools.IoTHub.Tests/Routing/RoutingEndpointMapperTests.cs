// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Tools.IoTHub.Models;
using Azure.Mcp.Tools.IoTHub.Routing;
using Xunit;

namespace Azure.Mcp.Tools.IoTHub.Tests.Routing;

public class RoutingEndpointMapperTests()
{
    [Fact]
    public void ConvertToRoutingEndpointDetailsList_HandlesAbsentEndpoints()
    {
        Assert.Empty(RoutingEndpointMapper.ConvertToRoutingEndpointDetailsList(null));
    }

    [Fact]
    public void ConvertToRoutingEndpointDetailsList_PreservesAllEndpointKinds()
    {
        var endpoint = new RoutingEndpointProperties
        {
            Id = "dbc3cb4d-0119-4d46-9824-e8e3e83fb04a",
            Name = "endpoint1",
            SubscriptionId = "11111111-1111-1111-1111-111111111111",
            ResourceGroup = "rg1",
            EndpointUri = "https://account1.blob.core.windows.net",
            EntityPath = "entity1",
            ContainerName = "container1",
            DatabaseName = "database1",
            AuthenticationType = "identityBased",
            BatchFrequencyInSeconds = 60
        };
        var endpoints = new RoutingEndpoints
        {
            EventHubs = [endpoint],
            ServiceBusQueues = [endpoint],
            ServiceBusTopics = [endpoint],
            StorageContainers = [endpoint],
            CosmosDBSqlContainers = [endpoint]
        };

        var result = RoutingEndpointMapper.ConvertToRoutingEndpointDetailsList(endpoints);

        Assert.Equal(
            ["EventHub", "ServiceBusQueue", "ServiceBusTopic", "StorageContainer", "CosmosDBSqlContainer"],
            result.Select(value => value.EndpointType));
        Assert.All(result, value =>
        {
            Assert.Equal(endpoint.Id, value.EndpointId);
            Assert.Equal(endpoint.Name, value.Name);
            Assert.Equal(endpoint.SubscriptionId, value.SubscriptionId);
            Assert.Equal(endpoint.ResourceGroup, value.ResourceGroup);
            Assert.Equal(endpoint.EndpointUri, value.EndpointUri);
            Assert.Equal(endpoint.AuthenticationType, value.AuthenticationType);
            Assert.Equal(60, value.BatchFrequencyInSeconds);
            Assert.Equal("account1", value.EndpointResourceName);
        });
        Assert.All(result.Take(3), value =>
        {
            Assert.Equal("entity1", value.EntityPath);
            Assert.Null(value.ContainerName);
            Assert.Null(value.DatabaseName);
        });
        Assert.All(result.Skip(3), value =>
        {
            Assert.Equal("container1", value.ContainerName);
            Assert.Null(value.EntityPath);
        });
        Assert.Null(result[3].DatabaseName);
        Assert.Equal("database1", result[4].DatabaseName);
    }

    [Fact]
    public void ConvertToRoutingEndpointDetailsList_LeavesKeyBasedTargetNameUnresolved()
    {
        // IoT Hub returns an opaque endpoint id and no endpointUri for key-based endpoints.
        static RoutingEndpointProperties KeyBased(string? entityPath = null, string? containerName = null) => new()
        {
            Id = "dbc3cb4d-0119-4d46-9824-e8e3e83fb04a",
            Name = "endpoint1",
            SubscriptionId = "11111111-1111-1111-1111-111111111111",
            ResourceGroup = "rg1",
            EntityPath = entityPath,
            ContainerName = containerName,
            AuthenticationType = "keyBased"
        };
        var endpoints = new RoutingEndpoints
        {
            EventHubs = [KeyBased(entityPath: "telemetry-hub")],
            ServiceBusQueues = [KeyBased(entityPath: "telemetry-queue")],
            ServiceBusTopics = [KeyBased(entityPath: "telemetry-topic")],
            StorageContainers = [KeyBased(containerName: "telemetry")]
        };

        var result = RoutingEndpointMapper.ConvertToRoutingEndpointDetailsList(endpoints);

        Assert.Equal(4, result.Count);
        Assert.All(result, value =>
        {
            Assert.Null(value.EndpointResourceName);
            Assert.Equal("dbc3cb4d-0119-4d46-9824-e8e3e83fb04a", value.EndpointId);
        });
        Assert.Equal(["telemetry-hub", "telemetry-queue", "telemetry-topic"], result.Take(3).Select(value => value.EntityPath));
        Assert.Equal("telemetry", result[3].ContainerName);
    }
}
