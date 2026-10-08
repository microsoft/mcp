// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net;
using Azure;
using Azure.Mcp.Tests.Commands;
using Azure.Mcp.Tools.NetAppFiles.Commands;
using Azure.Mcp.Tools.NetAppFiles.Commands.Snapshot;
using Azure.Mcp.Tools.NetAppFiles.Models;
using Azure.Mcp.Tools.NetAppFiles.Services;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Xunit;

namespace Azure.Mcp.Tools.NetAppFiles.Tests.Snapshot;

public class SnapshotCreateCommandTests : SubscriptionCommandUnitTestsBase<SnapshotCreateCommand, INetAppFilesSnapshotService>
{
    private static readonly DateTimeOffset Created = new(2026, 9, 18, 12, 0, 0, TimeSpan.Zero);

    private static readonly NetAppFilesSnapshot CreatedSnapshot = new(
        "snapshot1",
        "/subscriptions/sub/resourceGroups/rg/providers/Microsoft.NetApp/netAppAccounts/account1/capacityPools/pool1/volumes/volume1/snapshots/snapshot1",
        "eastus",
        "Succeeded",
        "00000000-0000-0000-0000-000000000001",
        Created);

    [Fact]
    public void Constructor_InitializesCommandCorrectly()
    {
        var command = Command.GetCommand();

        Assert.Equal("create", command.Name);
        Assert.False(string.IsNullOrWhiteSpace(command.Description));
    }

    [Fact]
    public async Task ExecuteAsync_CreatesSnapshot()
    {
        Service.CreateSnapshotAsync(
            "account1",
            "pool1",
            "volume1",
            "snapshot1",
            "rg",
            "sub",
            "tenant",
            Arg.Any<CancellationToken>())
            .Returns(CreatedSnapshot);

        var response = await ExecuteCommandAsync(
            "--account", "account1",
            "--pool", "pool1",
            "--volume", "volume1",
            "--snapshot", "snapshot1",
            "--resource-group", "rg",
            "--subscription", "sub",
            "--tenant", "tenant");

        var result = ValidateAndDeserializeResponse(
            response,
            NetAppFilesJsonContext.Default.SnapshotCreateResult);
        Assert.Equal(CreatedSnapshot, result.Snapshot);
    }

    [Theory]
    [InlineData("--account account/name --pool pool1 --volume volume1 --snapshot snapshot1 --resource-group rg --subscription sub", "--account")]
    [InlineData("--account account1 --pool pool/name --volume volume1 --snapshot snapshot1 --resource-group rg --subscription sub", "--pool")]
    [InlineData("--account account1 --pool pool1 --volume volume/name --snapshot snapshot1 --resource-group rg --subscription sub", "--volume")]
    [InlineData("--account account1 --pool pool1 --volume volume1 --snapshot snapshot.name --resource-group rg --subscription sub", "--snapshot")]
    [InlineData("--account account1 --pool pool1 --volume volume1 --snapshot _snapshot --resource-group rg --subscription sub", "--snapshot")]
    public async Task ExecuteAsync_InvalidName_ReturnsBadRequest(string args, string expectedMessage)
    {
        var response = await ExecuteCommandAsync(args);

        Assert.Equal(HttpStatusCode.BadRequest, response.Status);
        Assert.Contains(expectedMessage, response.Message);
        Assert.Empty(Service.ReceivedCalls());
    }

    [Fact]
    public async Task ExecuteAsync_OversizedSnapshotName_ReturnsBadRequest()
    {
        var response = await ExecuteCommandAsync(
            "--account", "account1",
            "--pool", "pool1",
            "--volume", "volume1",
            "--snapshot", new string('a', 256),
            "--resource-group", "rg",
            "--subscription", "sub");

        Assert.Equal(HttpStatusCode.BadRequest, response.Status);
        Assert.Contains("1-255", response.Message);
        Assert.Empty(Service.ReceivedCalls());
    }

    [Theory]
    [InlineData("--pool pool1 --volume volume1 --snapshot snapshot1 --resource-group rg --subscription sub")]
    [InlineData("--account account1 --volume volume1 --snapshot snapshot1 --resource-group rg --subscription sub")]
    [InlineData("--account account1 --pool pool1 --snapshot snapshot1 --resource-group rg --subscription sub")]
    [InlineData("--account account1 --pool pool1 --volume volume1 --resource-group rg --subscription sub")]
    [InlineData("--account account1 --pool pool1 --volume volume1 --snapshot snapshot1 --subscription sub")]
    [InlineData("--account account1 --pool pool1 --volume volume1 --snapshot snapshot1 --resource-group rg")]
    public async Task ExecuteAsync_MissingRequiredOption_ReturnsBadRequest(string args)
    {
        var response = await ExecuteCommandAsync(args);

        Assert.Equal(HttpStatusCode.BadRequest, response.Status);
        Assert.Contains("required", response.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task ExecuteAsync_ServiceConflict_ReturnsSafeMessage()
    {
        Service.CreateSnapshotAsync(
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string?>(),
            Arg.Any<CancellationToken>())
            .ThrowsAsync(new RequestFailedException(
                (int)HttpStatusCode.Conflict,
                "backend payload containing internal snapshot metadata"));

        var response = await ExecuteCommandAsync(
            "--account", "account1",
            "--pool", "pool1",
            "--volume", "volume1",
            "--snapshot", "snapshot1",
            "--resource-group", "rg",
            "--subscription", "sub");

        Assert.Equal(HttpStatusCode.Conflict, response.Status);
        Assert.Contains("resource conflict", response.Message);
        Assert.DoesNotContain("backend payload", response.Message);
    }
}
