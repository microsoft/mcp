// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json;
using Azure.Mcp.Tests;
using Azure.Mcp.Tests.Client;
using Xunit;

namespace Azure.Mcp.Tools.EventHubs.LiveTests;

public class EventHubsCommandTests(ITestOutputHelper output)
    : CommandTestsBase(output)
{
    [Fact]
    public async Task Should_ListNamespaces_Successfully()
    {
        var result = await CallToolAsync(
            "azmcp_eventhubs_namespace_get",
            new()
            {
                { "subscription", Settings.SubscriptionId },
                { "resource-group", Settings.ResourceGroupName }
            });

        // Should successfully retrieve the list of namespaces
        var namespaces = result.AssertProperty("namespaces");
        Assert.Equal(JsonValueKind.Array, namespaces.ValueKind);

        // Should contain at least our test namespace
        var namespaceArray = namespaces.EnumerateArray().ToList();
        Assert.True(namespaceArray.Count >= 1, "Should contain at least our test Event Hubs namespace");

        // Verify that our test namespace exists
        var testNamespace = namespaceArray.FirstOrDefault(ns =>
            ns.GetProperty("name").GetString() == Settings.ResourceBaseName);
        Assert.NotEqual(default, testNamespace);

        // Verify namespace properties
        if (testNamespace.ValueKind != JsonValueKind.Undefined)
        {
            var nsName = testNamespace.GetProperty("name").GetString();
            Assert.Equal(Settings.ResourceBaseName, nsName);

            var nsId = testNamespace.GetProperty("id").GetString();
            Assert.Contains($"/subscriptions/{Settings.SubscriptionId}", nsId);
            Assert.Contains($"/resourceGroups/{Settings.ResourceGroupName}", nsId);
            Assert.Contains("/providers/Microsoft.EventHub/namespaces/", nsId);
            Assert.Contains(Settings.ResourceBaseName, nsId);

            var nsResourceGroup = testNamespace.GetProperty("resourceGroup").GetString();
            Assert.Equal(Settings.ResourceGroupName, nsResourceGroup);
        }
    }

    [Fact]
    public async Task Should_GetSingleNamespace_Successfully()
    {
        // Test getting a single namespace by name and resource group
        var result = await CallToolAsync(
            "azmcp_eventhubs_namespace_get",
            new()
            {
                { "subscription", Settings.SubscriptionId },
                { "resource-group", Settings.ResourceGroupName },
                { "namespace", Settings.ResourceBaseName }
            });

        // Should successfully retrieve the single namespace with detailed metadata
        var namespaceData = result.AssertProperty("namespace");
        Assert.Equal(JsonValueKind.Object, namespaceData.ValueKind);

        // Verify basic properties
        var name = namespaceData.GetProperty("name").GetString();
        Assert.Equal(Settings.ResourceBaseName, name);

        var id = namespaceData.GetProperty("id").GetString();
        Assert.Contains($"/subscriptions/{Settings.SubscriptionId}", id);
        Assert.Contains($"/resourceGroups/{Settings.ResourceGroupName}", id);
        Assert.Contains("/providers/Microsoft.EventHub/namespaces/", id);
        Assert.Contains(Settings.ResourceBaseName, id);

        var resourceGroup = namespaceData.GetProperty("resourceGroup").GetString();
        Assert.Equal(Settings.ResourceGroupName, resourceGroup);

        // Verify comprehensive metadata fields are present
        var location = namespaceData.AssertProperty("location");
        Assert.False(string.IsNullOrEmpty(location.GetString()));

        Assert.True(namespaceData.TryGetProperty("status", out var status));
        Assert.False(string.IsNullOrEmpty(status.GetString()));

        Assert.True(namespaceData.TryGetProperty("provisioningState", out var provisioningState));
        Assert.False(string.IsNullOrEmpty(provisioningState.GetString()));

        // Verify SKU information is present and detailed
        var sku = namespaceData.AssertProperty("sku");
        Assert.Equal(JsonValueKind.Object, sku.ValueKind);

        var skuName = sku.AssertProperty("name");
        Assert.False(string.IsNullOrEmpty(skuName.GetString()));

        var skuTier = sku.AssertProperty("tier");
        Assert.False(string.IsNullOrEmpty(skuTier.GetString()));

        // Verify timestamps are present
        var creationTime = namespaceData.AssertProperty("creationTime");
        Assert.NotEqual(JsonValueKind.Null, creationTime.ValueKind);

        // Verify service endpoint is present
        var serviceBusEndpoint = namespaceData.AssertProperty("serviceBusEndpoint");
        Assert.False(string.IsNullOrEmpty(serviceBusEndpoint.GetString()));
        Assert.Contains(".servicebus.windows.net", serviceBusEndpoint.GetString());

        // Verify metric ID is present
        var metricId = namespaceData.AssertProperty("metricId");
        Assert.False(string.IsNullOrEmpty(metricId.GetString()));
        Assert.Contains(Settings.SubscriptionId, metricId.GetString());
        Assert.Contains(Settings.ResourceBaseName, metricId.GetString());

        // Verify feature flags are present (even if false/null)
        Assert.True(namespaceData.TryGetProperty("isAutoInflateEnabled", out _));
        Assert.True(namespaceData.TryGetProperty("kafkaEnabled", out _));
        Assert.True(namespaceData.TryGetProperty("zoneRedundant", out _));
    }

    [Fact]
    public async Task Should_CreateNamespace_Successfully()
    {
        var testNamespaceName = $"{Settings.ResourceBaseName}-test-create";
        
        // First, ensure the namespace doesn't exist by trying to delete it (ignore failures)
        try
        {
            await CallToolAsync(
                "azmcp_eventhubs_namespace_delete",
                new()
                {
                    { "subscription", Settings.SubscriptionId },
                    { "resource-group", Settings.ResourceGroupName },
                    { "namespace", testNamespaceName }
                });
        }
        catch
        {
            // Ignore deletion errors - namespace might not exist
        }

        // Create the namespace
        var result = await CallToolAsync(
            "azmcp_eventhubs_namespace_update",
            new()
            {
                { "subscription", Settings.SubscriptionId },
                { "resource-group", Settings.ResourceGroupName },
                { "namespace", testNamespaceName },
                { "location", "East US" },
                { "sku-name", "Standard" }
            });

        // Verify creation response
        var operation = result.AssertProperty("operation");
        Assert.Equal(JsonValueKind.Object, operation.ValueKind);

        var operationStatus = operation.GetProperty("status").GetString();
        Assert.True(operationStatus == "Succeeded" || operationStatus == "InProgress", 
            $"Operation status should be Succeeded or InProgress, but was {operationStatus}");

        // Cleanup - delete the test namespace
        try
        {
            await CallToolAsync(
                "azmcp_eventhubs_namespace_delete",
                new()
                {
                    { "subscription", Settings.SubscriptionId },
                    { "resource-group", Settings.ResourceGroupName },
                    { "namespace", testNamespaceName }
                });
        }
        catch
        {
            // Ignore cleanup errors in tests
        }
    }

    [Fact]
    public async Task Should_UpdateNamespace_Successfully()
    {
        // Update the existing test namespace
        var result = await CallToolAsync(
            "azmcp_eventhubs_namespace_update",
            new()
            {
                { "subscription", Settings.SubscriptionId },
                { "resource-group", Settings.ResourceGroupName },
                { "namespace", Settings.ResourceBaseName },
                { "location", "East US" },
                { "sku-name", "Standard" }
            });

        // Verify update response
        var operation = result.AssertProperty("operation");
        Assert.Equal(JsonValueKind.Object, operation.ValueKind);

        var operationStatus = operation.GetProperty("status").GetString();
        Assert.True(operationStatus == "Succeeded" || operationStatus == "InProgress", 
            $"Operation status should be Succeeded or InProgress, but was {operationStatus}");
    }

    [Fact]
    public async Task Should_DeleteNamespace_HandleNonExistentNamespace()
    {
        var nonExistentNamespace = $"{Settings.ResourceBaseName}-nonexistent-{Guid.NewGuid():N}";
        
        // Try to delete a non-existent namespace - should succeed gracefully
        var result = await CallToolAsync(
            "azmcp_eventhubs_namespace_delete",
            new()
            {
                { "subscription", Settings.SubscriptionId },
                { "resource-group", Settings.ResourceGroupName },
                { "namespace", nonExistentNamespace }
            });

        // Verify deletion response for non-existent resource
        var operation = result.AssertProperty("operation");
        Assert.Equal(JsonValueKind.Object, operation.ValueKind);

        var operationStatus = operation.GetProperty("status").GetString();
        Assert.True(operationStatus == "Succeeded" || operationStatus == "NotFound", 
            $"Operation status should be Succeeded or NotFound for non-existent resource, but was {operationStatus}");
    }

    [Fact]
    public async Task Should_ListEventHubs_Successfully()
    {
        var result = await CallToolAsync(
            "azmcp_eventhubs_eventhub_get",
            new()
            {
                { "subscription", Settings.SubscriptionId },
                { "resource-group", Settings.ResourceGroupName },
                { "namespace", Settings.ResourceBaseName }
            });

        // Should successfully retrieve the list of event hubs
        var eventHubs = result.AssertProperty("eventHubs");
        Assert.Equal(JsonValueKind.Array, eventHubs.ValueKind);

        // Event hubs array might be empty in a new namespace, which is valid
        var eventHubArray = eventHubs.EnumerateArray().ToList();
        
        // If event hubs exist, verify their structure
        if (eventHubArray.Count > 0)
        {
            var firstEventHub = eventHubArray[0];
            
            // Verify basic properties
            var name = firstEventHub.AssertProperty("name");
            Assert.False(string.IsNullOrEmpty(name.GetString()));

            var id = firstEventHub.AssertProperty("id");
            Assert.Contains($"/subscriptions/{Settings.SubscriptionId}", id.GetString());
            Assert.Contains($"/resourceGroups/{Settings.ResourceGroupName}", id.GetString());
            Assert.Contains("/providers/Microsoft.EventHub/namespaces/", id.GetString());
            Assert.Contains(Settings.ResourceBaseName, id.GetString());
            Assert.Contains("/eventhubs/", id.GetString());

            var resourceGroup = firstEventHub.AssertProperty("resourceGroup");
            Assert.Equal(Settings.ResourceGroupName, resourceGroup.GetString());
        }
    }

    [Fact]
    public async Task Should_CreateEventHub_Successfully()
    {
        var testEventHubName = $"test-eventhub-{Guid.NewGuid():N}";
        
        // First, ensure the event hub doesn't exist by trying to delete it (ignore failures)
        try
        {
            await CallToolAsync(
                "azmcp_eventhubs_eventhub_delete",
                new()
                {
                    { "subscription", Settings.SubscriptionId },
                    { "resource-group", Settings.ResourceGroupName },
                    { "namespace", Settings.ResourceBaseName },
                    { "eventhub", testEventHubName }
                });
        }
        catch
        {
            // Ignore deletion errors - event hub might not exist
        }

        // Create the event hub
        var result = await CallToolAsync(
            "azmcp_eventhubs_eventhub_update",
            new()
            {
                { "subscription", Settings.SubscriptionId },
                { "resource-group", Settings.ResourceGroupName },
                { "namespace", Settings.ResourceBaseName },
                { "eventhub", testEventHubName },
                { "partition-count", "2" },
                { "message-retention", "1" }
            });

        // Verify creation response
        var eventHub = result.AssertProperty("eventHub");
        Assert.Equal(JsonValueKind.Object, eventHub.ValueKind);

        var name = eventHub.GetProperty("name").GetString();
        Assert.Equal(testEventHubName, name);

        var partitionCount = eventHub.GetProperty("partitionCount").GetInt32();
        Assert.Equal(2, partitionCount);

        // Cleanup - delete the test event hub
        try
        {
            await CallToolAsync(
                "azmcp_eventhubs_eventhub_delete",
                new()
                {
                    { "subscription", Settings.SubscriptionId },
                    { "resource-group", Settings.ResourceGroupName },
                    { "namespace", Settings.ResourceBaseName },
                    { "eventhub", testEventHubName }
                });
        }
        catch
        {
            // Ignore cleanup errors in tests
        }
    }

    [Fact]
    public async Task Should_GetSingleEventHub_Successfully()
    {
        var testEventHubName = $"test-get-eventhub-{Guid.NewGuid():N}";
        
        // First create an event hub to get
        await CallToolAsync(
            "azmcp_eventhubs_eventhub_update",
            new()
            {
                { "subscription", Settings.SubscriptionId },
                { "resource-group", Settings.ResourceGroupName },
                { "namespace", Settings.ResourceBaseName },
                { "eventhub", testEventHubName },
                { "partition-count", "4" },
                { "message-retention", "2" }
            });

        try
        {
            // Get the specific event hub
            var result = await CallToolAsync(
                "azmcp_eventhubs_eventhub_get",
                new()
                {
                    { "subscription", Settings.SubscriptionId },
                    { "resource-group", Settings.ResourceGroupName },
                    { "namespace", Settings.ResourceBaseName },
                    { "eventhub", testEventHubName }
                });

            // Verify single event hub response
            var eventHub = result.AssertProperty("eventHub");
            Assert.Equal(JsonValueKind.Object, eventHub.ValueKind);

            var name = eventHub.GetProperty("name").GetString();
            Assert.Equal(testEventHubName, name);

            var id = eventHub.GetProperty("id").GetString();
            Assert.Contains($"/subscriptions/{Settings.SubscriptionId}", id);
            Assert.Contains($"/resourceGroups/{Settings.ResourceGroupName}", id);
            Assert.Contains("/providers/Microsoft.EventHub/namespaces/", id);
            Assert.Contains(Settings.ResourceBaseName, id);
            Assert.Contains($"/eventhubs/{testEventHubName}", id);

            var partitionCount = eventHub.GetProperty("partitionCount").GetInt32();
            Assert.Equal(4, partitionCount);

            var messageRetention = eventHub.GetProperty("messageRetentionInDays").GetInt64();
            Assert.Equal(2, messageRetention);

            // Verify partition IDs array
            var partitionIds = eventHub.AssertProperty("partitionIds");
            Assert.Equal(JsonValueKind.Array, partitionIds.ValueKind);
            Assert.Equal(4, partitionIds.GetArrayLength());
        }
        finally
        {
            // Cleanup - delete the test event hub
            try
            {
                await CallToolAsync(
                    "azmcp_eventhubs_eventhub_delete",
                    new()
                    {
                        { "subscription", Settings.SubscriptionId },
                        { "resource-group", Settings.ResourceGroupName },
                        { "namespace", Settings.ResourceBaseName },
                        { "eventhub", testEventHubName }
                    });
            }
            catch
            {
                // Ignore cleanup errors in tests
            }
        }
    }

    [Fact]
    public async Task Should_DeleteEventHub_HandleNonExistentEventHub()
    {
        var nonExistentEventHub = $"nonexistent-eventhub-{Guid.NewGuid():N}";
        
        // Try to delete a non-existent event hub - should succeed gracefully
        var result = await CallToolAsync(
            "azmcp_eventhubs_eventhub_delete",
            new()
            {
                { "subscription", Settings.SubscriptionId },
                { "resource-group", Settings.ResourceGroupName },
                { "namespace", Settings.ResourceBaseName },
                { "eventhub", nonExistentEventHub }
            });

        // Verify deletion response for non-existent resource
        var operation = result.AssertProperty("operation");
        Assert.Equal(JsonValueKind.Object, operation.ValueKind);

        var operationStatus = operation.GetProperty("status").GetString();
        Assert.True(operationStatus == "Succeeded" || operationStatus == "NotFound", 
            $"Operation status should be Succeeded or NotFound for non-existent resource, but was {operationStatus}");
    }

    [Fact]
    public async Task Should_ListConsumerGroups_Successfully()
    {
        var testEventHubName = $"test-cg-list-eventhub-{Guid.NewGuid():N}";
        
        // First create an event hub for consumer groups
        await CallToolAsync(
            "azmcp_eventhubs_eventhub_update",
            new()
            {
                { "subscription", Settings.SubscriptionId },
                { "resource-group", Settings.ResourceGroupName },
                { "namespace", Settings.ResourceBaseName },
                { "eventhub", testEventHubName },
                { "partition-count", "2" },
                { "message-retention", "1" }
            });

        try
        {
            var result = await CallToolAsync(
                "azmcp_eventhubs_consumergroup_get",
                new()
                {
                    { "subscription", Settings.SubscriptionId },
                    { "resource-group", Settings.ResourceGroupName },
                    { "namespace", Settings.ResourceBaseName },
                    { "eventhub", testEventHubName }
                });

            // Should successfully retrieve the list of consumer groups
            var consumerGroups = result.AssertProperty("consumerGroups");
            Assert.Equal(JsonValueKind.Array, consumerGroups.ValueKind);

            // Should contain at least the default $Default consumer group
            var consumerGroupArray = consumerGroups.EnumerateArray().ToList();
            Assert.True(consumerGroupArray.Count >= 1, "Should contain at least the default consumer group");

            // Verify that the $Default consumer group exists
            var defaultConsumerGroup = consumerGroupArray.FirstOrDefault(cg =>
                cg.GetProperty("name").GetString() == "$Default");
            Assert.NotEqual(default, defaultConsumerGroup);

            // Verify consumer group properties
            if (defaultConsumerGroup.ValueKind != JsonValueKind.Undefined)
            {
                var cgName = defaultConsumerGroup.GetProperty("name").GetString();
                Assert.Equal("$Default", cgName);

                var cgId = defaultConsumerGroup.GetProperty("id").GetString();
                Assert.Contains($"/subscriptions/{Settings.SubscriptionId}", cgId);
                Assert.Contains($"/resourceGroups/{Settings.ResourceGroupName}", cgId);
                Assert.Contains("/providers/Microsoft.EventHub/namespaces/", cgId);
                Assert.Contains(Settings.ResourceBaseName, cgId);
                Assert.Contains($"/eventhubs/{testEventHubName}", cgId);
                Assert.Contains("/consumergroups/$Default", cgId);

                var resourceGroup = defaultConsumerGroup.GetProperty("resourceGroup").GetString();
                Assert.Equal(Settings.ResourceGroupName, resourceGroup);
            }
        }
        finally
        {
            // Cleanup - delete the test event hub
            try
            {
                await CallToolAsync(
                    "azmcp_eventhubs_eventhub_delete",
                    new()
                    {
                        { "subscription", Settings.SubscriptionId },
                        { "resource-group", Settings.ResourceGroupName },
                        { "namespace", Settings.ResourceBaseName },
                        { "eventhub", testEventHubName }
                    });
            }
            catch
            {
                // Ignore cleanup errors in tests
            }
        }
    }

    [Fact]
    public async Task Should_CreateConsumerGroup_Successfully()
    {
        var testEventHubName = $"test-cg-create-eventhub-{Guid.NewGuid():N}";
        var testConsumerGroupName = $"test-cg-{Guid.NewGuid():N}";
        
        // First create an event hub for the consumer group
        await CallToolAsync(
            "azmcp_eventhubs_eventhub_update",
            new()
            {
                { "subscription", Settings.SubscriptionId },
                { "resource-group", Settings.ResourceGroupName },
                { "namespace", Settings.ResourceBaseName },
                { "eventhub", testEventHubName },
                { "partition-count", "2" },
                { "message-retention", "1" }
            });

        try
        {
            // First, ensure the consumer group doesn't exist by trying to delete it (ignore failures)
            try
            {
                await CallToolAsync(
                    "azmcp_eventhubs_consumergroup_delete",
                    new()
                    {
                        { "subscription", Settings.SubscriptionId },
                        { "resource-group", Settings.ResourceGroupName },
                        { "namespace", Settings.ResourceBaseName },
                        { "eventhub", testEventHubName },
                        { "consumer-group", testConsumerGroupName }
                    });
            }
            catch
            {
                // Ignore deletion errors - consumer group might not exist
            }

            // Create the consumer group
            var result = await CallToolAsync(
                "azmcp_eventhubs_consumergroup_update",
                new()
                {
                    { "subscription", Settings.SubscriptionId },
                    { "resource-group", Settings.ResourceGroupName },
                    { "namespace", Settings.ResourceBaseName },
                    { "eventhub", testEventHubName },
                    { "consumer-group", testConsumerGroupName },
                    { "user-metadata", "Test consumer group for live tests" }
                });

            // Verify creation response
            var consumerGroup = result.AssertProperty("consumerGroup");
            Assert.Equal(JsonValueKind.Object, consumerGroup.ValueKind);

            var name = consumerGroup.GetProperty("name").GetString();
            Assert.Equal(testConsumerGroupName, name);

            var userMetadata = consumerGroup.GetProperty("userMetadata").GetString();
            Assert.Equal("Test consumer group for live tests", userMetadata);

            // Cleanup - delete the test consumer group
            try
            {
                await CallToolAsync(
                    "azmcp_eventhubs_consumergroup_delete",
                    new()
                    {
                        { "subscription", Settings.SubscriptionId },
                        { "resource-group", Settings.ResourceGroupName },
                        { "namespace", Settings.ResourceBaseName },
                        { "eventhub", testEventHubName },
                        { "consumer-group", testConsumerGroupName }
                    });
            }
            catch
            {
                // Ignore cleanup errors in tests
            }
        }
        finally
        {
            // Cleanup - delete the test event hub
            try
            {
                await CallToolAsync(
                    "azmcp_eventhubs_eventhub_delete",
                    new()
                    {
                        { "subscription", Settings.SubscriptionId },
                        { "resource-group", Settings.ResourceGroupName },
                        { "namespace", Settings.ResourceBaseName },
                        { "eventhub", testEventHubName }
                    });
            }
            catch
            {
                // Ignore cleanup errors in tests
            }
        }
    }

    [Fact]
    public async Task Should_GetSingleConsumerGroup_Successfully()
    {
        var testEventHubName = $"test-cg-get-eventhub-{Guid.NewGuid():N}";
        var testConsumerGroupName = $"test-get-cg-{Guid.NewGuid():N}";
        
        // First create an event hub for the consumer group
        await CallToolAsync(
            "azmcp_eventhubs_eventhub_update",
            new()
            {
                { "subscription", Settings.SubscriptionId },
                { "resource-group", Settings.ResourceGroupName },
                { "namespace", Settings.ResourceBaseName },
                { "eventhub", testEventHubName },
                { "partition-count", "2" },
                { "message-retention", "1" }
            });

        try
        {
            // Create a consumer group to get
            await CallToolAsync(
                "azmcp_eventhubs_consumergroup_update",
                new()
                {
                    { "subscription", Settings.SubscriptionId },
                    { "resource-group", Settings.ResourceGroupName },
                    { "namespace", Settings.ResourceBaseName },
                    { "eventhub", testEventHubName },
                    { "consumer-group", testConsumerGroupName },
                    { "user-metadata", "Test consumer group for get operation" }
                });

            // Get the specific consumer group
            var result = await CallToolAsync(
                "azmcp_eventhubs_consumergroup_get",
                new()
                {
                    { "subscription", Settings.SubscriptionId },
                    { "resource-group", Settings.ResourceGroupName },
                    { "namespace", Settings.ResourceBaseName },
                    { "eventhub", testEventHubName },
                    { "consumer-group", testConsumerGroupName }
                });

            // Verify single consumer group response
            var consumerGroup = result.AssertProperty("consumerGroup");
            Assert.Equal(JsonValueKind.Object, consumerGroup.ValueKind);

            var name = consumerGroup.GetProperty("name").GetString();
            Assert.Equal(testConsumerGroupName, name);

            var id = consumerGroup.GetProperty("id").GetString();
            Assert.Contains($"/subscriptions/{Settings.SubscriptionId}", id);
            Assert.Contains($"/resourceGroups/{Settings.ResourceGroupName}", id);
            Assert.Contains("/providers/Microsoft.EventHub/namespaces/", id);
            Assert.Contains(Settings.ResourceBaseName, id);
            Assert.Contains($"/eventhubs/{testEventHubName}", id);
            Assert.Contains($"/consumergroups/{testConsumerGroupName}", id);

            var userMetadata = consumerGroup.GetProperty("userMetadata").GetString();
            Assert.Equal("Test consumer group for get operation", userMetadata);

            var resourceGroup = consumerGroup.GetProperty("resourceGroup").GetString();
            Assert.Equal(Settings.ResourceGroupName, resourceGroup);

            // Verify timestamps are present
            var creationTime = consumerGroup.AssertProperty("creationTime");
            Assert.NotEqual(JsonValueKind.Null, creationTime.ValueKind);

            var updatedAt = consumerGroup.AssertProperty("updatedAt");
            Assert.NotEqual(JsonValueKind.Null, updatedAt.ValueKind);

            // Cleanup - delete the test consumer group
            try
            {
                await CallToolAsync(
                    "azmcp_eventhubs_consumergroup_delete",
                    new()
                    {
                        { "subscription", Settings.SubscriptionId },
                        { "resource-group", Settings.ResourceGroupName },
                        { "namespace", Settings.ResourceBaseName },
                        { "eventhub", testEventHubName },
                        { "consumer-group", testConsumerGroupName }
                    });
            }
            catch
            {
                // Ignore cleanup errors in tests
            }
        }
        finally
        {
            // Cleanup - delete the test event hub
            try
            {
                await CallToolAsync(
                    "azmcp_eventhubs_eventhub_delete",
                    new()
                    {
                        { "subscription", Settings.SubscriptionId },
                        { "resource-group", Settings.ResourceGroupName },
                        { "namespace", Settings.ResourceBaseName },
                        { "eventhub", testEventHubName }
                    });
            }
            catch
            {
                // Ignore cleanup errors in tests
            }
        }
    }

    [Fact]
    public async Task Should_DeleteConsumerGroup_HandleNonExistentConsumerGroup()
    {
        var testEventHubName = $"test-cg-delete-eventhub-{Guid.NewGuid():N}";
        var nonExistentConsumerGroup = $"nonexistent-cg-{Guid.NewGuid():N}";
        
        // First create an event hub for the consumer group test
        await CallToolAsync(
            "azmcp_eventhubs_eventhub_update",
            new()
            {
                { "subscription", Settings.SubscriptionId },
                { "resource-group", Settings.ResourceGroupName },
                { "namespace", Settings.ResourceBaseName },
                { "eventhub", testEventHubName },
                { "partition-count", "2" },
                { "message-retention", "1" }
            });

        try
        {
            // Try to delete a non-existent consumer group - should succeed gracefully
            var result = await CallToolAsync(
                "azmcp_eventhubs_consumergroup_delete",
                new()
                {
                    { "subscription", Settings.SubscriptionId },
                    { "resource-group", Settings.ResourceGroupName },
                    { "namespace", Settings.ResourceBaseName },
                    { "eventhub", testEventHubName },
                    { "consumer-group", nonExistentConsumerGroup }
                });

            // Verify deletion response for non-existent resource
            var operation = result.AssertProperty("operation");
            Assert.Equal(JsonValueKind.Object, operation.ValueKind);

            var operationStatus = operation.GetProperty("status").GetString();
            Assert.True(operationStatus == "Succeeded" || operationStatus == "NotFound", 
                $"Operation status should be Succeeded or NotFound for non-existent resource, but was {operationStatus}");
        }
        finally
        {
            // Cleanup - delete the test event hub
            try
            {
                await CallToolAsync(
                    "azmcp_eventhubs_eventhub_delete",
                    new()
                    {
                        { "subscription", Settings.SubscriptionId },
                        { "resource-group", Settings.ResourceGroupName },
                        { "namespace", Settings.ResourceBaseName },
                        { "eventhub", testEventHubName }
                    });
            }
            catch
            {
                // Ignore cleanup errors in tests
            }
        }
    }
}
