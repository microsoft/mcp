// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net;
using Azure.Mcp.Tests.Commands;
using Azure.Mcp.Tools.IoTHub.Commands;
using Azure.Mcp.Tools.IoTHub.Commands.Device;
using Azure.Mcp.Tools.IoTHub.Models;
using Azure.Mcp.Tools.IoTHub.Services;
using Microsoft.Mcp.Core.Options;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Xunit;

namespace Azure.Mcp.Tools.IoTHub.UnitTests.Device;

public class IoTHubDeviceListCommandTests : SubscriptionCommandUnitTestsBase<IoTHubDeviceListCommand, IIoTHubDeviceService>
{
    private static DeviceIdentity CreateDevice(string id) =>
        new(id, "gen1", "aaaa==", "Connected", "Enabled", null, "2024-01-01T00:00:00Z", "2024-01-02T00:00:00Z", "2024-01-03T00:00:00Z", 0, new DeviceAuthentication("SAS"), null);

    [Fact]
    public void Constructor_InitializesCommandCorrectly()
    {
        var command = Command.GetCommand();
        Assert.Equal("list", command.Name);
        Assert.NotNull(command.Description);
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
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<int?>(),
            Arg.Any<RetryPolicyOptions?>(),
            Arg.Any<CancellationToken>())
            .Returns(new DeviceListResult(devices, false));

        var response = await ExecuteCommandAsync(
            "--subscription", "sub-id",
            "--resource-group", "test-rg",
            "--name", "test-hub");

        Assert.Equal(HttpStatusCode.OK, response.Status);
        Assert.NotNull(response.Results);
    }

    [Fact]
    public async Task ExecuteAsync_MaxCountGreaterThanOneHundred_CapsAtOneHundred()
    {
        Service.ListDevices(
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<int?>(),
            Arg.Any<RetryPolicyOptions?>(),
            Arg.Any<CancellationToken>())
            .Returns(new DeviceListResult([], false));

        var response = await ExecuteCommandAsync(
            "--subscription", "sub-id",
            "--resource-group", "test-rg",
            "--name", "test-hub",
            "--max-count", "500");

        Assert.Equal(HttpStatusCode.OK, response.Status);
        await Service.Received(1).ListDevices(
            "test-hub",
            "test-rg",
            "sub-id",
            100,
            Arg.Any<RetryPolicyOptions?>(),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteAsync_ListDevices_Truncated_SetsMessage()
    {
        Service.ListDevices(
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<int?>(),
            Arg.Any<RetryPolicyOptions?>(),
            Arg.Any<CancellationToken>())
            .Returns(new DeviceListResult(new List<DeviceIdentity> { CreateDevice("device1") }, true));

        var response = await ExecuteCommandAsync(
            "--subscription", "sub-id",
            "--resource-group", "test-rg",
            "--name", "test-hub");

        Assert.Equal(HttpStatusCode.OK, response.Status);
        Assert.NotNull(response.Results);
        Assert.NotNull(response.Message);
        Assert.Contains("truncated", response.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task ExecuteAsync_MaxCountLessThanOne_ReturnsBadRequest()
    {
        var response = await ExecuteCommandAsync(
            "--subscription", "sub-id",
            "--resource-group", "test-rg",
            "--name", "test-hub",
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
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<int?>(),
            Arg.Any<RetryPolicyOptions?>(),
            Arg.Any<CancellationToken>())
            .Returns(new DeviceListResult(devices, false));

        var response = await ExecuteCommandAsync(
            "--subscription", "sub-id",
            "--resource-group", "test-rg",
            "--name", "test-hub");

        var result = ValidateAndDeserializeResponse(response, IoTHubJsonContext.Default.DeviceListResult);
        Assert.NotNull(result);
        Assert.Single(result.Devices);
        Assert.False(result.Truncated);
    }

    [Fact]
    public async Task ExecuteAsync_HandlesServiceErrors()
    {
        Service.ListDevices(
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<int?>(),
            Arg.Any<RetryPolicyOptions?>(),
            Arg.Any<CancellationToken>())
            .ThrowsAsync(new Exception("Test error"));

        var response = await ExecuteCommandAsync(
            "--subscription", "sub-id",
            "--resource-group", "test-rg",
            "--name", "test-hub");

        Assert.Equal(HttpStatusCode.InternalServerError, response.Status);
        Assert.Contains("Test error", response.Message);
    }
}
