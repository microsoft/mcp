// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net;
using System.Text.Json;
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

public class IoTHubDeviceTwinGetCommandTests : SubscriptionCommandUnitTestsBase<IoTHubDeviceTwinGetCommand, IIoTHubDeviceService>
{
    private static JsonElement Parse(string json) => JsonDocument.Parse(json).RootElement.Clone();

    private static DeviceTwin CreateDeviceTwin(TwinProperties? properties = null, JsonElement? tags = null) => new(
        DeviceId: "device1",
        Etag: "etag1",
        DeviceEtag: "detag1",
        Status: "enabled",
        StatusUpdateTime: null,
        ConnectionState: "Connected",
        LastActivityTime: null,
        CloudToDeviceMessageCount: 3,
        AuthenticationType: "sas",
        Version: 7,
        Properties: properties,
        Capabilities: null,
        Tags: tags);

    [Fact]
    public void Constructor_InitializesCommandCorrectly()
    {
        var command = Command.GetCommand();
        Assert.Equal("get", command.Name);
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
            Service.GetDeviceTwin(
                Arg.Any<string>(),
                Arg.Any<string>(),
                Arg.Any<string>(),
                Arg.Any<string>(),
                Arg.Any<string?>(),
                Arg.Any<RetryPolicyOptions?>(),
                Arg.Any<CancellationToken>())
                .Returns(CreateDeviceTwin());
        }

        var response = await ExecuteCommandAsync(args);

        Assert.Equal(shouldSucceed ? HttpStatusCode.OK : HttpStatusCode.BadRequest, response.Status);
        if (!shouldSucceed)
        {
            Assert.Contains("required", response.Message.ToLower());
        }
    }

    [Fact]
    public async Task ExecuteAsync_ReturnsDeviceTwin()
    {
        Service.GetDeviceTwin(
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string?>(),
            Arg.Any<RetryPolicyOptions?>(),
            Arg.Any<CancellationToken>())
            .Returns(CreateDeviceTwin());

        var response = await ExecuteCommandAsync("--subscription", "sub123", "--resource-group", "rg1", "--hub-name", "hub1", "--device-id", "device1");

        var result = ValidateAndDeserializeResponse(response, IoTHubJsonContext.Default.DeviceTwin);
        Assert.Equal("device1", result.DeviceId);
        Assert.Equal("etag1", result.Etag);
        Assert.Equal("detag1", result.DeviceEtag);
        Assert.Equal("enabled", result.Status);
        Assert.Equal("Connected", result.ConnectionState);
        Assert.Equal("sas", result.AuthenticationType);
        Assert.Equal(3, result.CloudToDeviceMessageCount);
        Assert.Equal(7, result.Version);
    }

    [Fact]
    public async Task ExecuteAsync_ReturnsTagsAndProperties()
    {
        var twin = CreateDeviceTwin(
            properties: new TwinProperties(
                Desired: Parse("{\"interval\":30}"),
                Reported: Parse("{\"temperature\":42}")),
            tags: Parse("{\"env\":\"prod\",\"floor\":3}"));
        Service.GetDeviceTwin(
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string?>(),
            Arg.Any<RetryPolicyOptions?>(),
            Arg.Any<CancellationToken>())
            .Returns(twin);

        var response = await ExecuteCommandAsync("--subscription", "sub123", "--resource-group", "rg1", "--hub-name", "hub1", "--device-id", "device1");

        var result = ValidateAndDeserializeResponse(response, IoTHubJsonContext.Default.DeviceTwin);
        Assert.NotNull(result.Properties);
        Assert.True(result.Properties!.Desired.HasValue);
        Assert.Equal(30, result.Properties.Desired!.Value.GetProperty("interval").GetInt32());
        Assert.True(result.Properties.Reported.HasValue);
        Assert.Equal(42, result.Properties.Reported!.Value.GetProperty("temperature").GetInt32());
        Assert.True(result.Tags.HasValue);
        Assert.Equal("prod", result.Tags!.Value.GetProperty("env").GetString());
        Assert.Equal(3, result.Tags.Value.GetProperty("floor").GetInt32());
    }

    [Fact]
    public async Task ExecuteAsync_PassesParsedArgumentsToService()
    {
        Service.GetDeviceTwin(
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string?>(),
            Arg.Any<RetryPolicyOptions?>(),
            Arg.Any<CancellationToken>())
            .Returns(CreateDeviceTwin());

        var response = await ExecuteCommandAsync("--subscription", "sub123", "--resource-group", "rg1", "--hub-name", "hub1", "--device-id", "device1");

        Assert.Equal(HttpStatusCode.OK, response.Status);
        await Service.Received(1).GetDeviceTwin(
            "device1",
            "hub1",
            "rg1",
            "sub123",
            Arg.Any<string?>(),
            Arg.Any<RetryPolicyOptions?>(),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteAsync_HandlesServiceErrors()
    {
        Service.GetDeviceTwin(
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
