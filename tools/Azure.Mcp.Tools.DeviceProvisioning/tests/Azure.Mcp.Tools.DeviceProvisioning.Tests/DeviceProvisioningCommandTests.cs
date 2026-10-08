// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json;
using Microsoft.Mcp.Tests;
using Microsoft.Mcp.Tests.Client;
using Microsoft.Mcp.Tests.Client.Helpers;
using Xunit;

namespace Azure.Mcp.Tools.DeviceProvisioning.Tests;

public class DeviceProvisioningCommandTests(
    ITestOutputHelper output,
    TestProxyFixture fixture,
    LiveServerFixture liveServerFixture)
    : RecordedCommandTestsBase(output, fixture, liveServerFixture)
{
    [Fact]
    public async Task ShouldGetDeviceProvisioningService()
    {
        var result = await CallToolAsync(
            "deviceprovisioning_service_get",
            new()
            {
                { "service", Settings.ResourceBaseName },
                { "resource-group", Settings.ResourceGroupName },
                { "subscription", Settings.SubscriptionId },
                { "tenant", Settings.TenantId }
            });

        Assert.NotNull(result);

        var payload = result.Value;
        var provisioningService = payload.AssertProperty("deviceProvisioningService");
        Assert.Equal(JsonValueKind.Object, provisioningService.ValueKind);
        provisioningService.AssertProperty("name");
        Assert.NotEmpty(
            provisioningService.AssertProperty("idScope").GetString() ?? string.Empty);

        var linkedHubs = provisioningService.AssertProperty("iotHubs");
        Assert.Equal(JsonValueKind.Array, linkedHubs.ValueKind);

        var areResultsTruncated = payload.AssertProperty("areResultsTruncated");
        Assert.False(areResultsTruncated.GetBoolean());
    }
}
