// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Tests.Client;
using Xunit;

namespace Azure.Mcp.Tools.IoTHub.LiveTests;

public class IoTHubCommandTests(ITestOutputHelper output) : CommandTestsBase(output)
{
    [Fact]
    public async Task IoTHub_Lifecycle()
    {
        var hubName = "iothub-" + Guid.NewGuid().ToString().Substring(0, 8);
        var location = "eastus";
        var sku = "S1";
        var capacity = 1;

        // Create
        await CallToolAsync("iothub_hub_create", new()
        {
            { "name", hubName },
            { "resourceGroup", Settings.ResourceGroupName },
            { "location", location },
            { "sku", sku },
            { "capacity", capacity },
            { "subscription", Settings.SubscriptionId }
        });

        // Get
        await CallToolAsync("iothub_hub_get", new()
        {
            { "name", hubName },
            { "resourceGroup", Settings.ResourceGroupName },
            { "subscription", Settings.SubscriptionId }
        });

        // Update
        await CallToolAsync("iothub_hub_update", new()
        {
            { "name", hubName },
            { "resourceGroup", Settings.ResourceGroupName },
            { "capacity", 2 },
            { "subscription", Settings.SubscriptionId }
        });

        // Keys - will fail with consent requirement in test harness
        try
        {
            await CallToolAsync("iothub_hub_keys", new()
            {
                { "name", hubName },
                { "resourceGroup", Settings.ResourceGroupName },
                { "subscription", Settings.SubscriptionId }
            });
        }
        catch (Exception ex)
        {
            // Expected: client doesn't support consent elicitation
            Console.WriteLine($"Keys command expected failure: {ex.Message}");
        }

        // Delete
        await CallToolAsync("iothub_hub_delete", new()
        {
            { "name", hubName },
            { "resourceGroup", Settings.ResourceGroupName },
            { "subscription", Settings.SubscriptionId }
        });
    }
}
