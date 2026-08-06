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

public class IoTHubDeviceStatisticsCommandTests : SubscriptionCommandUnitTestsBase<IoTHubDeviceStatisticsCommand, IIoTHubDeviceService>
{
    [Fact]
    public void Constructor_InitializesCommandCorrectly()
    {
        var command = Command.GetCommand();
        Assert.Equal("stats", command.Name);
        Assert.NotNull(command.Description);
        Assert.NotEmpty(command.Description);
    }

    [Theory]
    [InlineData("--subscription sub123 --resource-group rg1 --hub-name hub1", true)]
    [InlineData("--subscription sub123 --hub-name hub1", false)]
    [InlineData("--subscription sub123", false)]
    public async Task ExecuteAsync_ValidatesInputCorrectly(string args, bool shouldSucceed)
    {
        if (shouldSucceed)
        {
            Service.GetDeviceStatistics(
                Arg.Any<string>(),
                Arg.Any<string>(),
                Arg.Any<string>(),
                Arg.Any<string?>(),
                Arg.Any<RetryPolicyOptions?>(),
                Arg.Any<CancellationToken>())
                .Returns(new IoTHubRegistryStatistics(1, 4, 5));
        }

        var response = await ExecuteCommandAsync(args);

        Assert.Equal(shouldSucceed ? HttpStatusCode.OK : HttpStatusCode.BadRequest, response.Status);
        if (!shouldSucceed)
        {
            Assert.Contains("required", response.Message.ToLower());
        }
    }

    [Fact]
    public async Task ExecuteAsync_ReturnsStatistics()
    {
        Service.GetDeviceStatistics(
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string?>(),
            Arg.Any<RetryPolicyOptions?>(),
            Arg.Any<CancellationToken>())
            .Returns(new IoTHubRegistryStatistics(1, 4, 5));

        var response = await ExecuteCommandAsync("--subscription", "sub123", "--resource-group", "rg1", "--hub-name", "hub1");

        var result = ValidateAndDeserializeResponse(response, IoTHubJsonContext.Default.IoTHubRegistryStatistics);
        Assert.Equal(1, result.DisabledDeviceCount);
        Assert.Equal(4, result.EnabledDeviceCount);
        Assert.Equal(5, result.TotalDeviceCount);
    }

    [Fact]
    public async Task ExecuteAsync_PassesParsedArgumentsToService()
    {
        Service.GetDeviceStatistics(
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string?>(),
            Arg.Any<RetryPolicyOptions?>(),
            Arg.Any<CancellationToken>())
            .Returns(new IoTHubRegistryStatistics(1, 4, 5));

        var response = await ExecuteCommandAsync("--subscription", "sub123", "--resource-group", "rg1", "--hub-name", "hub1");

        Assert.Equal(HttpStatusCode.OK, response.Status);
        await Service.Received(1).GetDeviceStatistics(
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
        Service.GetDeviceStatistics(
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string?>(),
            Arg.Any<RetryPolicyOptions?>(),
            Arg.Any<CancellationToken>())
            .ThrowsAsync(new Exception("Test error"));

        var response = await ExecuteCommandAsync("--subscription", "sub123", "--resource-group", "rg1", "--hub-name", "hub1");

        Assert.Equal(HttpStatusCode.InternalServerError, response.Status);
        Assert.Contains("Test error", response.Message);
    }
}
