// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net;
using Azure.Mcp.Tests.Commands;
using Azure.Mcp.Tools.IoTHub.Commands;
using Azure.Mcp.Tools.IoTHub.Commands.Routing;
using Azure.Mcp.Tools.IoTHub.Models;
using Azure.Mcp.Tools.IoTHub.Services;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Xunit;

namespace Azure.Mcp.Tools.IoTHub.Tests.Routing;

public class IoTHubRoutingEndpointHealthCommandTests
    : SubscriptionCommandUnitTestsBase<IoTHubRoutingEndpointHealthCommand, IIoTHubRoutingService>
{
    [Fact]
    public void Constructor_InitializesCommandCorrectly()
    {
        var command = Command.GetCommand();

        Assert.Equal("endpoint-health", command.Name);
        Assert.DoesNotContain(command.Options, option => option.Name.StartsWith("retry", StringComparison.Ordinal));
        Assert.DoesNotContain(command.Options, option => option.Name is "lookback" or "start-time" or "end-time" or "interval");
    }

    [Theory]
    [InlineData("--subscription sub123 --resource-group rg1 --hub-name hub1", true)]
    [InlineData("--subscription sub123 --resource-group rg1 --hub-name hub1 --endpoint-name endpoint1", true)]
    [InlineData("--subscription sub123 --hub-name hub1", false)]
    [InlineData("--subscription sub123 --resource-group rg1", false)]
    public async Task ExecuteAsync_ValidatesInputCorrectly(string args, bool shouldSucceed)
    {
        if (shouldSucceed)
        {
            Service.GetRoutingEndpointHealth(
                Arg.Any<string>(),
                Arg.Any<string>(),
                Arg.Any<string>(),
                Arg.Any<string?>(),
                Arg.Any<string?>(),
                Arg.Any<CancellationToken>())
                .Returns([]);
        }

        var response = await ExecuteCommandAsync(args);

        Assert.Equal(shouldSucceed ? HttpStatusCode.OK : HttpStatusCode.BadRequest, response.Status);
    }

    [Fact]
    public async Task ExecuteAsync_ReturnsRawSnapshotFields()
    {
        Service.GetRoutingEndpointHealth(
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<CancellationToken>())
            .Returns(
            [
                new RoutingEndpointHealthSnapshot(
                    "endpoint-id",
                    "endpoint1",
                    "unhealthy",
                    "NotFound",
                    "Wed, 27 Aug 2026 19:42:00 GMT",
                    "Wed, 27 Aug 2026 19:40:00 GMT",
                    "Wed, 27 Aug 2026 19:59:00 GMT")
            ]);

        var response = await ExecuteCommandAsync(
            "--subscription", "sub123",
            "--resource-group", "rg1",
            "--hub-name", "hub1");

        var result = ValidateAndDeserializeResponse(
            response,
            IoTHubJsonContext.Default.RoutingEndpointHealthResult);
        var endpoint = Assert.Single(result.Endpoints);
        Assert.Equal("endpoint-id", endpoint.EndpointId);
        Assert.Equal("endpoint1", endpoint.EndpointName);
        Assert.Equal("unhealthy", endpoint.HealthStatus);
        Assert.Equal("NotFound", endpoint.LastKnownError);
        Assert.Equal("Wed, 27 Aug 2026 19:42:00 GMT", endpoint.LastKnownErrorTime);
        Assert.Equal("Wed, 27 Aug 2026 19:40:00 GMT", endpoint.LastSuccessfulSendAttemptTime);
        Assert.Equal("Wed, 27 Aug 2026 19:59:00 GMT", endpoint.LastSendAttemptTime);
    }

    [Fact]
    public async Task ExecuteAsync_PassesEndpointNameToService()
    {
        Service.GetRoutingEndpointHealth(
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<CancellationToken>())
            .Returns([]);

        await ExecuteCommandAsync(
            "--subscription", "sub123",
            "--resource-group", "rg1",
            "--hub-name", "hub1",
            "--endpoint-name", "endpoint1");

        await Service.Received(1).GetRoutingEndpointHealth(
            "hub1",
            "rg1",
            "sub123",
            "endpoint1",
            Arg.Any<string?>(),
            Arg.Any<CancellationToken>());
    }

    [Theory]
    [InlineData("ab")]
    [InlineData("hub-name-")]
    [InlineData("hub!name")]
    [InlineData("hub' OR 1=1")]
    public async Task ExecuteAsync_RejectsInvalidIoTHubName(string invalidName)
    {
        var response = await ExecuteCommandAsync(
            "--subscription", "sub123",
            "--resource-group", "rg1",
            "--hub-name", invalidName);

        Assert.Equal(HttpStatusCode.BadRequest, response.Status);
        Assert.Contains("--hub-name must be 3-50 characters long", response.Message);
    }

    [Fact]
    public async Task ExecuteAsync_ReportsOperationTimeout()
    {
        Service.GetRoutingEndpointHealth(
            Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(),
            Arg.Any<string?>(), Arg.Any<string?>(), Arg.Any<CancellationToken>())
            .ThrowsAsync(new TimeoutException("Routing health operation timed out."));

        var response = await ExecuteCommandAsync(
            "--subscription sub123 --resource-group rg1 --hub-name hub1");

        Assert.Equal(HttpStatusCode.RequestTimeout, response.Status);
        Assert.Contains("timed out", response.Message);
    }

    [Fact]
    public async Task ExecuteAsync_HandlesServiceErrors()
    {
        Service.GetRoutingEndpointHealth(
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<CancellationToken>())
            .ThrowsAsync(new Exception("Test error"));

        var response = await ExecuteCommandAsync(
            "--subscription", "sub123",
            "--resource-group", "rg1",
            "--hub-name", "hub1");

        Assert.Equal(HttpStatusCode.InternalServerError, response.Status);
        Assert.Contains("Test error", response.Message);
    }
}
