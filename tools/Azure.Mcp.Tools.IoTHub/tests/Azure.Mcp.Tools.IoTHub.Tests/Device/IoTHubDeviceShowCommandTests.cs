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

namespace Azure.Mcp.Tools.IoTHub.Tests.Device;

public class IoTHubDeviceShowCommandTests : SubscriptionCommandUnitTestsBase<IoTHubDeviceShowCommand, IIoTHubDeviceService>
{
    private static DeviceIdentity CreateDeviceIdentity() => new(
        DeviceId: "device1",
        GenerationId: "gen1",
        Etag: "etag1",
        ConnectionState: "Connected",
        Status: "enabled",
        StatusReason: "provisioned",
        ConnectionStateUpdatedTime: "2024-01-01T00:00:00Z",
        StatusUpdatedTime: "2024-01-02T00:00:00Z",
        LastActivityTime: "2024-01-03T00:00:00Z",
        CloudToDeviceMessageCount: 5,
        Authentication: new DeviceAuthentication("sas"),
        Capabilities: new DeviceCapabilities(true));

    [Fact]
    public void Constructor_InitializesCommandCorrectly()
    {
        var command = Command.GetCommand();
        Assert.Equal("show", command.Name);
        Assert.NotNull(command.Description);
        Assert.NotEmpty(command.Description);
    }

    [Theory]
    [InlineData("--subscription sub123 --resource-group rg1 --hub-name hub1 --device-id device1", true)]
    [InlineData("--subscription sub123 --resource-group rg1 --hub-name hub1", false)]
    [InlineData("--subscription sub123", false)]
    public async Task ExecuteAsync_ValidatesInputCorrectly(string args, bool shouldSucceed)
    {
        if (shouldSucceed)
        {
            Service.GetDevice(
                Arg.Any<string>(),
                Arg.Any<string>(),
                Arg.Any<string>(),
                Arg.Any<string>(),
                Arg.Any<string?>(),
                Arg.Any<RetryPolicyOptions?>(),
                Arg.Any<CancellationToken>())
                .Returns(CreateDeviceIdentity());
        }

        var response = await ExecuteCommandAsync(args);

        Assert.Equal(shouldSucceed ? HttpStatusCode.OK : HttpStatusCode.BadRequest, response.Status);
        if (!shouldSucceed)
        {
            Assert.Contains("required", response.Message.ToLower());
        }
    }

    [Fact]
    public async Task ExecuteAsync_ReturnsDeviceIdentity()
    {
        Service.GetDevice(
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string?>(),
            Arg.Any<RetryPolicyOptions?>(),
            Arg.Any<CancellationToken>())
            .Returns(CreateDeviceIdentity());

        var response = await ExecuteCommandAsync("--subscription", "sub123", "--resource-group", "rg1", "--hub-name", "hub1", "--device-id", "device1");

        var result = ValidateAndDeserializeResponse(response, IoTHubJsonContext.Default.DeviceIdentity);
        Assert.Equal("device1", result.DeviceId);
        Assert.Equal("gen1", result.GenerationId);
        Assert.Equal("etag1", result.Etag);
        Assert.Equal("Connected", result.ConnectionState);
        Assert.Equal("enabled", result.Status);
        Assert.Equal("provisioned", result.StatusReason);
        Assert.Equal("2024-01-03T00:00:00Z", result.LastActivityTime);
        Assert.Equal(5, result.CloudToDeviceMessageCount);
        Assert.NotNull(result.Authentication);
        Assert.Equal("sas", result.Authentication!.Type);
        Assert.NotNull(result.Capabilities);
        Assert.True(result.Capabilities!.IotEdge);
    }

    [Fact]
    public async Task ExecuteAsync_PassesParsedArgumentsToService()
    {
        Service.GetDevice(
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string?>(),
            Arg.Any<RetryPolicyOptions?>(),
            Arg.Any<CancellationToken>())
            .Returns(CreateDeviceIdentity());

        var response = await ExecuteCommandAsync("--subscription", "sub123", "--resource-group", "rg1", "--hub-name", "hub1", "--device-id", "device1");

        Assert.Equal(HttpStatusCode.OK, response.Status);
        await Service.Received(1).GetDevice(
            "device1",
            "hub1",
            "rg1",
            "sub123",
            Arg.Any<string?>(),
            Arg.Any<RetryPolicyOptions?>(),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteAsync_HandlesNotFound()
    {
        Service.GetDevice(
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string?>(),
            Arg.Any<RetryPolicyOptions?>(),
            Arg.Any<CancellationToken>())
            .ThrowsAsync(new RequestFailedException((int)HttpStatusCode.NotFound, "Device not found"));

        var response = await ExecuteCommandAsync("--subscription", "sub123", "--resource-group", "rg1", "--hub-name", "hub1", "--device-id", "missing");

        Assert.Equal(HttpStatusCode.NotFound, response.Status);
        Assert.Contains("Device not found", response.Message);
    }

    [Fact]
    public async Task ExecuteAsync_HandlesServiceErrors()
    {
        Service.GetDevice(
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string?>(),
            Arg.Any<RetryPolicyOptions?>(),
            Arg.Any<CancellationToken>())
            .ThrowsAsync(new Exception("Test error"));

        var response = await ExecuteCommandAsync("--subscription", "sub123", "--resource-group", "rg1", "--hub-name", "hub1", "--device-id", "device1");

        Assert.Equal(HttpStatusCode.InternalServerError, response.Status);
        Assert.Contains("Test error", response.Message);
    }
}
