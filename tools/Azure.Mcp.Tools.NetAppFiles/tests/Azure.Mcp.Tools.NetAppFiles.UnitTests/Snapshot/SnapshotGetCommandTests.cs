// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.CommandLine;
using System.Net;
using System.Text.Json;
using Microsoft.Mcp.Core.Options;
using Azure.Mcp.Core.Services.Azure;
using Azure.Mcp.Tools.NetAppFiles.Commands;
using Azure.Mcp.Tools.NetAppFiles.Commands.Snapshot;
using Azure.Mcp.Tools.NetAppFiles.Models;
using Azure.Mcp.Tools.NetAppFiles.Services;
using Azure.Mcp.Tests.Commands;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Mcp.Core.Models.Command;
using Microsoft.Mcp.Tests.Helpers;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Xunit;

namespace Azure.Mcp.Tools.NetAppFiles.UnitTests.Snapshot;

public class SnapshotGetCommandTests : SubscriptionCommandUnitTestsBase<SnapshotGetCommand, INetAppFilesService>
{
    [Fact]
    public async Task ExecuteAsync_NoSnapshotParameter_ReturnsAllSnapshots()
    {
        // Arrange
        var subscription = "sub123";
        var expectedSnapshots = new ResourceQueryResults<SnapshotInfo>(
        [
            new("account1/pool1/vol1/snap1", "eastus", "rg1", "Succeeded", "2025-01-15T10:30:00Z"),
            new("account1/pool1/vol1/snap2", "eastus", "rg1", "Succeeded", "2025-01-16T12:00:00Z")
        ], false);

        Service.GetSnapshotDetails(
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<IReadOnlyList<string>?>(),
            Arg.Is(subscription),
            Arg.Any<string?>(),
            Arg.Any<RetryPolicyOptions>(),
            Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(expectedSnapshots));

        // Act
        var response = await ExecuteCommandAsync(["--subscription", subscription]);

        // Assert
        Assert.NotNull(response);
        Assert.NotNull(response.Results);

        var json = JsonSerializer.Serialize(response.Results);
        var result = JsonSerializer.Deserialize(json, NetAppFilesJsonContext.Default.SnapshotGetCommandResult);

        Assert.NotNull(result);
        Assert.NotNull(result.Snapshots);
        Assert.Equal(expectedSnapshots.Results.Count, result.Snapshots.Count);
        Assert.Equal(expectedSnapshots.Results.Select(s => s.Name), result.Snapshots.Select(s => s.Name));
    }

    [Fact]
    public async Task ExecuteAsync_ReturnsEmpty_WhenNoSnapshots()
    {
        // Arrange
        var subscription = "sub123";

        Service.GetSnapshotDetails(
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<IReadOnlyList<string>?>(),
            Arg.Is(subscription),
            Arg.Any<string>(),
            Arg.Any<RetryPolicyOptions>(),
            Arg.Any<CancellationToken>())
            .Returns(new ResourceQueryResults<SnapshotInfo>([], false));

        // Act
        var response = await ExecuteCommandAsync(["--subscription", subscription]);

        // Assert
        Assert.NotNull(response);
        Assert.NotNull(response.Results);

        var json = JsonSerializer.Serialize(response.Results);
        var result = JsonSerializer.Deserialize(json, NetAppFilesJsonContext.Default.SnapshotGetCommandResult);

        Assert.NotNull(result);
        Assert.Empty(result.Snapshots);
    }

    [Fact]
    public async Task ExecuteAsync_HandlesException()
    {
        // Arrange
        var expectedError = "Test error";
        var subscription = "sub123";

        Service.GetSnapshotDetails(
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<IReadOnlyList<string>?>(),
            Arg.Is(subscription),
            null,
            Arg.Any<RetryPolicyOptions>(),
            Arg.Any<CancellationToken>())
            .ThrowsAsync(new Exception(expectedError));

        // Act
        var response = await ExecuteCommandAsync(["--subscription", subscription]);

        // Assert
        Assert.NotNull(response);
        Assert.Equal(HttpStatusCode.InternalServerError, response.Status);
        Assert.StartsWith(expectedError, response.Message);
    }

    [Fact]
    public void Constructor_InitializesCommandCorrectly()
    {
        var command = Command.GetCommand();
        Assert.Equal("get", command.Name);
        Assert.NotNull(command.Description);
        Assert.NotEmpty(command.Description);
    }

    [Theory]
    [InlineData("--subscription sub123", true)]
    [InlineData("--subscription sub123 --account myanfaccount", true)]
    [InlineData("--subscription sub123 --account myanfaccount --pool mypool", true)]
    [InlineData("--subscription sub123 --account myanfaccount --pool mypool --volume myvol", true)]
    [InlineData("--subscription sub123 --account myanfaccount --pool mypool --volume myvol --snapshot mysnap", true)]
    [InlineData("--subscription sub123 --resource-group myrg", true)]
    [InlineData("--subscription sub123 --ids /subscriptions/sub123/resourceGroups/myrg/providers/Microsoft.NetApp/netAppAccounts/myanfaccount/capacityPools/mypool/volumes/myvol/snapshots/mysnap", true)]
    [InlineData("--subscription sub123 --snapshot mysnap", true)] // Snapshot without account/pool/volume is valid
    [InlineData("--account myanfaccount", false)] // Missing subscription
    public async Task ExecuteAsync_ValidatesInputCorrectly(string args, bool shouldSucceed)
    {
        if (shouldSucceed)
        {
            var expectedSnapshots = new ResourceQueryResults<SnapshotInfo>(
                [new("account1/pool1/vol1/snap1", "eastus", "rg1", "Succeeded", "2025-01-15T10:30:00Z")],
                false);

            Service.GetSnapshotDetails(
                Arg.Any<string?>(), Arg.Any<string?>(), Arg.Any<string?>(), Arg.Any<string?>(), Arg.Any<string?>(), Arg.Any<IReadOnlyList<string>?>(), Arg.Any<string>(), Arg.Any<string?>(), Arg.Any<RetryPolicyOptions>(), Arg.Any<CancellationToken>())
                .Returns(Task.FromResult(expectedSnapshots));
        }

        // Act
        var response = await ExecuteCommandAsync(args);

        // Assert
        Assert.Equal(shouldSucceed ? HttpStatusCode.OK : HttpStatusCode.BadRequest, response.Status);
        if (shouldSucceed)
        {
            Assert.NotNull(response.Results);
            Assert.Equal("Success", response.Message);
        }
        else
        {
            Assert.Contains("required", response.Message.ToLower());
        }
    }

    [Fact]
    public async Task ExecuteAsync_ReturnsSnapshotDetails_WhenSnapshotExists()
    {
        // Arrange
        var account = "myanfaccount";
        var pool = "mypool";
        var volume = "myvol";
        var snapshot = "mysnap";
        var subscription = "sub123";
        var expectedSnapshots = new ResourceQueryResults<SnapshotInfo>(
            [new($"{account}/{pool}/{volume}/{snapshot}", "eastus", "rg1", "Succeeded", "2025-01-15T10:30:00Z")],
            false);

        Service.GetSnapshotDetails(
            Arg.Is(account),
            Arg.Is(pool),
            Arg.Is(volume),
            Arg.Is(snapshot),
            Arg.Any<string?>(),
            Arg.Any<IReadOnlyList<string>?>(),
            Arg.Is(subscription),
            Arg.Any<string>(),
            Arg.Any<RetryPolicyOptions>(),
            Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(expectedSnapshots));

        // Act
        var response = await ExecuteCommandAsync(["--subscription", subscription, "--account", account, "--pool", pool, "--volume", volume, "--snapshot", snapshot]);

        // Assert
        Assert.NotNull(response);
        Assert.NotNull(response.Results);

        var json = JsonSerializer.Serialize(response.Results);
        var result = JsonSerializer.Deserialize(json, NetAppFilesJsonContext.Default.SnapshotGetCommandResult);

        Assert.NotNull(result);
        Assert.Single(result.Snapshots);
        Assert.Equal($"{account}/{pool}/{volume}/{snapshot}", result.Snapshots[0].Name);
    }

    [Fact]
    public async Task ExecuteAsync_DeserializationValidation()
    {
        // Arrange
        var subscription = "sub123";
        var expectedSnapshots = new ResourceQueryResults<SnapshotInfo>(
        [
            new("account1/pool1/vol1/snap1", "eastus", "rg1", "Succeeded", "2025-01-15T10:30:00Z")
        ], false);

        Service.GetSnapshotDetails(
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<IReadOnlyList<string>?>(),
            Arg.Is(subscription),
            Arg.Any<string>(),
            Arg.Any<RetryPolicyOptions>(),
            Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(expectedSnapshots));

        // Act
        var response = await ExecuteCommandAsync(["--subscription", subscription]);

        // Assert
        var json = JsonSerializer.Serialize(response.Results);
        var result = JsonSerializer.Deserialize(json, NetAppFilesJsonContext.Default.SnapshotGetCommandResult);

        Assert.NotNull(result);
        Assert.Single(result.Snapshots);

        var snapshotInfo = result.Snapshots[0];
        Assert.Equal("account1/pool1/vol1/snap1", snapshotInfo.Name);
        Assert.Equal("eastus", snapshotInfo.Location);
        Assert.Equal("rg1", snapshotInfo.ResourceGroup);
        Assert.Equal("Succeeded", snapshotInfo.ProvisioningState);
        Assert.Equal("2025-01-15T10:30:00Z", snapshotInfo.Created);
    }

    [Fact]
    public async Task ExecuteAsync_PassesResourceGroupAndIds()
    {
        var subscription = "sub123";
        var resourceGroup = "myrg";
        var snapshotId = "/subscriptions/sub123/resourceGroups/myrg/providers/Microsoft.NetApp/netAppAccounts/myanfaccount/capacityPools/mypool/volumes/myvol/snapshots/mysnap";
        var expectedSnapshots = new ResourceQueryResults<SnapshotInfo>(
            [new("myanfaccount/mypool/myvol/mysnap", "eastus", resourceGroup, "Succeeded", "2025-01-15T10:30:00Z")],
            false);

        Service.GetSnapshotDetails(
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Is(resourceGroup),
            Arg.Is<IReadOnlyList<string>?>(ids => ids != null && ids.Count == 1 && ids[0] == snapshotId),
            Arg.Is(subscription),
            Arg.Any<string?>(),
            Arg.Any<RetryPolicyOptions>(),
            Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(expectedSnapshots));

        // Act
        var response = await ExecuteCommandAsync([
            "--subscription", subscription,
            "--resource-group", resourceGroup,
            "--ids", snapshotId
        ]);

        Assert.Equal(HttpStatusCode.OK, response.Status);
        await Service.Received(1).GetSnapshotDetails(
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            resourceGroup,
            Arg.Is<IReadOnlyList<string>?>(ids => ids != null && ids.Count == 1 && ids[0] == snapshotId),
            subscription,
            null,
            Arg.Any<RetryPolicyOptions>(),
            Arg.Any<CancellationToken>());
    }
}
