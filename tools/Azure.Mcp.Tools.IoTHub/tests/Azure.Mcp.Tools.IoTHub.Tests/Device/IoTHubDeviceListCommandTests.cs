// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net;
using Azure.Mcp.Tests.Commands;
using Azure.Mcp.Tools.IoTHub.Commands;
using Azure.Mcp.Tools.IoTHub.Commands.Device;
using Azure.Mcp.Tools.IoTHub.Models;
using Azure.Mcp.Tools.IoTHub.Services;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Xunit;

namespace Azure.Mcp.Tools.IoTHub.Tests.Device;

public class IoTHubDeviceListCommandTests : SubscriptionCommandUnitTestsBase<IoTHubDeviceListCommand, IIoTHubDeviceService>
{
    private static DeviceIdentity CreateDevice(string id) =>
        new(id, "gen1", "aaaa==", "Connected", "Enabled", "provisioned", "2024-01-01T00:00:00Z", "2024-01-02T00:00:00Z", "2024-01-03T00:00:00Z", 3, new DeviceAuthentication("SAS"), new DeviceCapabilities(true));

    [Fact]
    public void Constructor_InitializesCommandCorrectly()
    {
        var command = Command.GetCommand();
        Assert.Equal("list", command.Name);
        Assert.Contains("--max-count", command.Description, StringComparison.Ordinal);
        Assert.Contains("default 100", command.Description, StringComparison.Ordinal);
        Assert.Contains("maximum 100", command.Description, StringComparison.Ordinal);
        Assert.Contains("truncated=true", command.Description, StringComparison.Ordinal);
    }

    [Fact]
    public async Task ExecuteAsync_ListDevices_Success()
    {
        var devices = new List<DeviceIdentity> { CreateDevice("device1"), CreateDevice("device2") };

        Service.ListDevices(
            "test-hub",
            "test-rg",
            "sub-id",
            "tenant-id",
            100,
            Arg.Any<CancellationToken>())
            .Returns(new DeviceListResult(devices, false));

        var response = await ExecuteCommandAsync(
            "--subscription", "sub-id",
            "--resource-group", "test-rg",
            "--hub-name", "test-hub",
            "--tenant", "tenant-id");

        Assert.Equal(HttpStatusCode.OK, response.Status);

        var result = ValidateAndDeserializeResponse(response, IoTHubJsonContext.Default.DeviceListResult);
        Assert.False(result.Truncated);
        Assert.Collection(result.Devices,
            device => Assert.Equal("device1", device.DeviceId),
            device => Assert.Equal("device2", device.DeviceId));
    }

    [Fact]
    public async Task ExecuteAsync_MaxCountGreaterThanOneHundred_ReturnsBadRequest()
    {
        var response = await ExecuteCommandAsync(
            "--subscription", "sub-id",
            "--resource-group", "test-rg",
            "--hub-name", "test-hub",
            "--tenant", "tenant-id",
            "--max-count", "500");

        Assert.Equal(HttpStatusCode.BadRequest, response.Status);
        Assert.Null(response.Results);
        Assert.Contains("greater than", response.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task ExecuteAsync_ListDevices_Truncated_SetsMessage()
    {
        Service.ListDevices(
            "test-hub",
            "test-rg",
            "sub-id",
            "tenant-id",
            100,
            Arg.Any<CancellationToken>())
            .Returns(new DeviceListResult(new List<DeviceIdentity> { CreateDevice("device1") }, true));

        var response = await ExecuteCommandAsync(
            "--subscription", "sub-id",
            "--resource-group", "test-rg",
            "--hub-name", "test-hub",
            "--tenant", "tenant-id");

        Assert.Equal(HttpStatusCode.OK, response.Status);
        Assert.Contains("truncated", response.Message, StringComparison.OrdinalIgnoreCase);

        var result = ValidateAndDeserializeResponse(response, IoTHubJsonContext.Default.DeviceListResult);
        Assert.True(result.Truncated);
        Assert.Equal("device1", Assert.Single(result.Devices).DeviceId);
    }

    [Fact]
    public async Task ExecuteAsync_MaxCountLessThanOne_ReturnsBadRequest()
    {
        var response = await ExecuteCommandAsync(
            "--subscription", "sub-id",
            "--resource-group", "test-rg",
            "--hub-name", "test-hub",
            "--max-count", "0");

        Assert.Equal(HttpStatusCode.BadRequest, response.Status);
        Assert.Null(response.Results);
        Assert.Contains("less than 1", response.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task ExecuteAsync_DeserializationValidation()
    {
        var devices = new List<DeviceIdentity> { CreateDevice("device1") };

        Service.ListDevices(
            "test-hub",
            "test-rg",
            "sub-id",
            "tenant-id",
            100,
            Arg.Any<CancellationToken>())
            .Returns(new DeviceListResult(devices, false));

        var response = await ExecuteCommandAsync(
            "--subscription", "sub-id",
            "--resource-group", "test-rg",
            "--hub-name", "test-hub",
            "--tenant", "tenant-id");

        var result = ValidateAndDeserializeResponse(response, IoTHubJsonContext.Default.DeviceListResult);
        Assert.False(result.Truncated);

        var device = Assert.Single(result.Devices);
        Assert.Equal("device1", device.DeviceId);
        Assert.Equal("gen1", device.GenerationId);
        Assert.Equal("aaaa==", device.Etag);
        Assert.Equal("Connected", device.ConnectionState);
        Assert.Equal("Enabled", device.Status);
        Assert.Equal("provisioned", device.StatusReason);
        Assert.Equal("2024-01-01T00:00:00Z", device.ConnectionStateUpdatedTime);
        Assert.Equal("2024-01-02T00:00:00Z", device.StatusUpdatedTime);
        Assert.Equal("2024-01-03T00:00:00Z", device.LastActivityTime);
        Assert.Equal(3, device.CloudToDeviceMessageCount);
        Assert.Equal("SAS", device.Authentication?.Type);
        Assert.True(device.Capabilities?.IotEdge);
    }

    [Fact]
    public async Task ExecuteAsync_HandlesServiceErrors()
    {
        Service.ListDevices(
            "test-hub",
            "test-rg",
            "sub-id",
            "tenant-id",
            100,
            Arg.Any<CancellationToken>())
            .ThrowsAsync(new Exception("Test error"));

        var response = await ExecuteCommandAsync(
            "--subscription", "sub-id",
            "--resource-group", "test-rg",
            "--hub-name", "test-hub",
            "--tenant", "tenant-id");

        Assert.Equal(HttpStatusCode.InternalServerError, response.Status);
        Assert.Contains("Test error", response.Message);
    }
}
