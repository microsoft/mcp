// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net;
using Azure;
using Azure.Mcp.Tests.Commands;
using Azure.Mcp.Tools.StorageSync.Commands.CloudEndpoint;
using Azure.Mcp.Tools.StorageSync.Models;
using Azure.Mcp.Tools.StorageSync.Services;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Xunit;

namespace Azure.Mcp.Tools.StorageSync.Tests.Commands.CloudEndpoint;

public class CloudEndpointUpdateCommandTests : SubscriptionCommandUnitTestsBase<CloudEndpointUpdateCommand, IStorageSyncService>
{
    private static readonly string[] RequiredArguments =
    [
        "--subscription", "subscription",
        "--resource-group", "resource-group",
        "--name", "sync-service",
        "--sync-group-name", "sync-group",
        "--cloud-endpoint-name", "cloud-endpoint"
    ];

    [Fact]
    public void Constructor_InitializesCommandCorrectly()
    {
        Assert.Equal("update", CommandDefinition.Name);
        Assert.Equal("update", Command.Name);
    }

    [Theory]
    [InlineData(1)]
    [InlineData(20)]
    public async Task ExecuteAsync_WithBoundaryInterval_UpdatesAndSerializesInterval(int interval)
    {
        Service.UpdateCloudEndpointAsync(
                Arg.Any<string>(),
                Arg.Any<string>(),
                Arg.Any<string>(),
                Arg.Any<string>(),
                Arg.Any<string>(),
                Arg.Any<int>(),
                Arg.Any<string?>(),
                Arg.Any<CancellationToken>())
            .Returns(new CloudEndpointDataSchema(Name: "cloud-endpoint", ChangeEnumerationIntervalDays: interval));

        var response = await ExecuteCommandAsync(
            [.. RequiredArguments, "--change-enumeration-interval-days", interval.ToString()]);

        Assert.Equal(HttpStatusCode.OK, response.Status);
        var result = ValidateAndDeserializeResponse(
            response,
            StorageSyncJsonContext.Default.CloudEndpointUpdateCommandResult);
        Assert.Equal("cloud-endpoint", result.Result.Name);
        Assert.Equal(interval, result.Result.ChangeEnumerationIntervalDays);
        await Service.Received(1).UpdateCloudEndpointAsync(
            "subscription",
            "resource-group",
            "sync-service",
            "sync-group",
            "cloud-endpoint",
            interval,
            null,
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteAsync_WithoutUpdateProperty_ReturnsBadRequest()
    {
        var response = await ExecuteCommandAsync(RequiredArguments);

        Assert.Equal(HttpStatusCode.BadRequest, response.Status);
        Assert.Contains("At least one update property", response.Message, StringComparison.OrdinalIgnoreCase);
        await Service.DidNotReceiveWithAnyArgs().UpdateCloudEndpointAsync(
            default!,
            default!,
            default!,
            default!,
            default!,
            default,
            default,
            Arg.Any<CancellationToken>());
    }

    [Theory]
    [InlineData(0)]
    [InlineData(21)]
    public async Task ExecuteAsync_WithOutOfRangeInterval_ReturnsBadRequest(int interval)
    {
        var response = await ExecuteCommandAsync(
            [.. RequiredArguments, "--change-enumeration-interval-days", interval.ToString()]);

        Assert.Equal(HttpStatusCode.BadRequest, response.Status);
        Assert.Contains("between 1 and 20", response.Message, StringComparison.OrdinalIgnoreCase);
        await Service.DidNotReceiveWithAnyArgs().UpdateCloudEndpointAsync(
            default!,
            default!,
            default!,
            default!,
            default!,
            default,
            default,
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteAsync_WhenServiceFails_UsesServiceStatusCode()
    {
        Service.UpdateCloudEndpointAsync(
                Arg.Any<string>(),
                Arg.Any<string>(),
                Arg.Any<string>(),
                Arg.Any<string>(),
                Arg.Any<string>(),
                Arg.Any<int>(),
                Arg.Any<string?>(),
                Arg.Any<CancellationToken>())
            .ThrowsAsync(new RequestFailedException(403, "Forbidden"));

        var response = await ExecuteCommandAsync(
            [.. RequiredArguments, "--change-enumeration-interval-days", "7"]);

        Assert.Equal(HttpStatusCode.Forbidden, response.Status);
        Assert.Contains("Forbidden", response.Message, StringComparison.OrdinalIgnoreCase);
    }
}
