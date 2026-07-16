// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Tests.Client;
using Xunit;

namespace Azure.Mcp.Tools.IoTHub.LiveTests;

public class IoTHubCommandTests(ITestOutputHelper output) : CommandTestsBase(output)
{
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
}
