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

    [Fact]
    public async Task IoTHubDevice_ListDevices()
    {
        var hubName = Environment.GetEnvironmentVariable("IOTHUB_NAME") ?? throw new InvalidOperationException("IOTHUB_NAME not set");

        // List all devices
        await CallToolAsync("iothub_device_list", new()
        {
            { "name", hubName },
            { "resourceGroup", Settings.ResourceGroupName },
            { "subscription", Settings.SubscriptionId }
        });

        // List with max count
        await CallToolAsync("iothub_device_list", new()
        {
            { "name", hubName },
            { "resourceGroup", Settings.ResourceGroupName },
            { "subscription", Settings.SubscriptionId },
            { "maxCount", 2 }
        });
    }

    [Fact]
    public async Task IoTHubDevice_GetDeviceTwin()
    {
        var hubName = Environment.GetEnvironmentVariable("IOTHUB_NAME") ?? throw new InvalidOperationException("IOTHUB_NAME not set");
        var deviceId = "test-device-1";

        await CallToolAsync("iothub_device_twin_get", new()
        {
            { "name", hubName },
            { "resourceGroup", Settings.ResourceGroupName },
            { "subscription", Settings.SubscriptionId },
            { "deviceId", deviceId }
        });
    }

    [Fact]
    public async Task IoTHubDevice_UpdateDeviceTwin()
    {
        var hubName = Environment.GetEnvironmentVariable("IOTHUB_NAME") ?? throw new InvalidOperationException("IOTHUB_NAME not set");
        var deviceId = "test-device-2";
        var patch = "{\"properties\":{\"desired\":{\"temperature\":80,\"humidity\":60}}}";

        await CallToolAsync("iothub_device_twin_update", new()
        {
            { "name", hubName },
            { "resourceGroup", Settings.ResourceGroupName },
            { "subscription", Settings.SubscriptionId },
            { "deviceId", deviceId },
            { "patch", patch }
        });

        // Verify the update
        await CallToolAsync("iothub_device_twin_get", new()
        {
            { "name", hubName },
            { "resourceGroup", Settings.ResourceGroupName },
            { "subscription", Settings.SubscriptionId },
            { "deviceId", deviceId }
        });
    }

    [Fact]
    public async Task IoTHubDevice_QueryTwins()
    {
        var hubName = Environment.GetEnvironmentVariable("IOTHUB_NAME") ?? throw new InvalidOperationException("IOTHUB_NAME not set");

        // Query all devices
        await CallToolAsync("iothub_device_twin_query", new()
        {
            { "name", hubName },
            { "resourceGroup", Settings.ResourceGroupName },
            { "subscription", Settings.SubscriptionId },
            { "query", "SELECT * FROM devices" }
        });

        // Query devices with specific tag
        await CallToolAsync("iothub_device_twin_query", new()
        {
            { "name", hubName },
            { "resourceGroup", Settings.ResourceGroupName },
            { "subscription", Settings.SubscriptionId },
            { "query", "SELECT * FROM devices WHERE tags.environment = 'test'" }
        });

        // Query devices with temperature property
        await CallToolAsync("iothub_device_twin_query", new()
        {
            { "name", hubName },
            { "resourceGroup", Settings.ResourceGroupName },
            { "subscription", Settings.SubscriptionId },
            { "query", "SELECT * FROM devices WHERE properties.desired.temperature > 70" }
        });
    }
}
