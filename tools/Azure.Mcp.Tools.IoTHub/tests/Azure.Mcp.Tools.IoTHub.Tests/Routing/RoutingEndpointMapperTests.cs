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
            Id = "/subscriptions/11111111-1111-1111-1111-111111111111/resourceGroups/rg1/providers/Microsoft.EventHub/namespaces/namespace1",
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
        });
        Assert.All(result.Take(3), value =>
        {
            Assert.Equal("namespace1", value.EndpointResourceName);
            Assert.Equal("entity1", value.EntityPath);
            Assert.Null(value.ContainerName);
            Assert.Null(value.DatabaseName);
        });
        Assert.All(result.Skip(3), value =>
        {
            Assert.Equal("account1", value.EndpointResourceName);
            Assert.Equal("container1", value.ContainerName);
            Assert.Null(value.EntityPath);
        });
        Assert.Null(result[3].DatabaseName);
        Assert.Equal("database1", result[4].DatabaseName);
    }
}
