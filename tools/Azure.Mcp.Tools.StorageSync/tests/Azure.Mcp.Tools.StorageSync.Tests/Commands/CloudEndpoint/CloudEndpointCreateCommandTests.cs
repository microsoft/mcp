// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net;
using Azure.Mcp.Tests.Commands;
using Azure.Mcp.Tools.StorageSync.Commands.CloudEndpoint;
using Azure.Mcp.Tools.StorageSync.Models;
using Azure.Mcp.Tools.StorageSync.Services;
using NSubstitute;
using Xunit;

namespace Azure.Mcp.Tools.StorageSync.Tests.Commands.CloudEndpoint;

public class CloudEndpointCreateCommandTests : SubscriptionCommandUnitTestsBase<CloudEndpointCreateCommand, IStorageSyncService>
{
    [Fact]
    public void Constructor_InitializesCommandCorrectly()
    {
        Assert.Equal("create", CommandDefinition.Name);
        Assert.Equal("create", Command.Name);
    }

    [Fact]
    public async Task ExecuteAsync_WithChangeEnumerationInterval_ForwardsInterval()
    {
        Service.CreateCloudEndpointAsync(
                Arg.Any<string>(),
                Arg.Any<string>(),
                Arg.Any<string>(),
                Arg.Any<string>(),
                Arg.Any<string>(),
                Arg.Any<string>(),
                Arg.Any<string>(),
                Arg.Any<int?>(),
                Arg.Any<string?>(),
                Arg.Any<CancellationToken>())
            .Returns(new CloudEndpointDataSchema(Name: "cloud-endpoint", ChangeEnumerationIntervalDays: 7));

        var response = await ExecuteCommandAsync(
            "--subscription", "subscription",
            "--resource-group", "resource-group",
            "--name", "sync-service",
            "--sync-group-name", "sync-group",
            "--cloud-endpoint-name", "cloud-endpoint",
            "--storage-account-resource-id", "/subscriptions/subscription/resourceGroups/resource-group/providers/Microsoft.Storage/storageAccounts/account",
            "--azure-file-share-name", "share",
            "--change-enumeration-interval-days", "7");

        Assert.Equal(HttpStatusCode.OK, response.Status);
        var result = ValidateAndDeserializeResponse(
            response,
            StorageSyncJsonContext.Default.CloudEndpointCreateCommandResult);
        Assert.Equal(7, result.Result.ChangeEnumerationIntervalDays);
        await Service.Received(1).CreateCloudEndpointAsync(
            "subscription",
            "resource-group",
            "sync-service",
            "sync-group",
            "cloud-endpoint",
            "/subscriptions/subscription/resourceGroups/resource-group/providers/Microsoft.Storage/storageAccounts/account",
            "share",
            7,
            null,
            Arg.Any<CancellationToken>());
    }

    [Theory]
    [InlineData(0)]
    [InlineData(21)]
    public async Task ExecuteAsync_WithOutOfRangeChangeEnumerationInterval_ReturnsBadRequest(int interval)
    {
        var response = await ExecuteCommandAsync(
            "--subscription", "subscription",
            "--resource-group", "resource-group",
            "--name", "sync-service",
            "--sync-group-name", "sync-group",
            "--cloud-endpoint-name", "cloud-endpoint",
            "--storage-account-resource-id", "/subscriptions/subscription/resourceGroups/resource-group/providers/Microsoft.Storage/storageAccounts/account",
            "--azure-file-share-name", "share",
            "--change-enumeration-interval-days", interval.ToString());

        Assert.Equal(HttpStatusCode.BadRequest, response.Status);
        Assert.Contains("between 1 and 20", response.Message, StringComparison.OrdinalIgnoreCase);
        await Service.DidNotReceiveWithAnyArgs().CreateCloudEndpointAsync(
            default!,
            default!,
            default!,
            default!,
            default!,
            default!,
            default!,
            default,
            default,
            Arg.Any<CancellationToken>());
    }
}

