// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.CommandLine;
using System.Net;
using System.Text.Json;
using Microsoft.Mcp.Core.Options;
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

public class SnapshotUpdateCommandTests : SubscriptionCommandUnitTestsBase<SnapshotUpdateCommand, INetAppFilesService>
{
    [Fact]
    public void Constructor_InitializesCommandCorrectly()
    {
        var command = Command.GetCommand();
        Assert.Equal("update", command.Name);
        Assert.NotNull(command.Description);
        Assert.NotEmpty(command.Description);
    }

    [Theory]
    [InlineData("--account myanfaccount --pool mypool --volume myvolume --snapshot mysnapshot --resource-group myrg --location eastus --subscription sub123", true)]
    [InlineData("--account myanfaccount --pool mypool --volume myvolume --snapshot mysnapshot --resource-group myrg --subscription sub123", true)]
    [InlineData("--pool mypool --volume myvolume --snapshot mysnapshot --resource-group myrg --location eastus --subscription sub123", false)] // Missing account
    [InlineData("--account myanfaccount --volume myvolume --snapshot mysnapshot --resource-group myrg --location eastus --subscription sub123", false)] // Missing pool
    [InlineData("--account myanfaccount --pool mypool --snapshot mysnapshot --resource-group myrg --location eastus --subscription sub123", false)] // Missing volume
    [InlineData("--account myanfaccount --pool mypool --volume myvolume --resource-group myrg --location eastus --subscription sub123", false)] // Missing snapshot
    [InlineData("--account myanfaccount --pool mypool --volume myvolume --snapshot mysnapshot --location eastus --subscription sub123", false)] // Missing resource-group
    [InlineData("", false)] // No parameters
    public async Task ExecuteAsync_ValidatesInputCorrectly(string args, bool shouldSucceed)
    {
        if (shouldSucceed)
        {
            var expectedSnapshot = new SnapshotCreateResult(
                Id: "/subscriptions/sub123/resourceGroups/myrg/providers/Microsoft.NetApp/netAppAccounts/myanfaccount/capacityPools/mypool/volumes/myvolume/snapshots/mysnapshot",
                Name: "myanfaccount/mypool/myvolume/mysnapshot",
                Type: "Microsoft.NetApp/netAppAccounts/capacityPools/volumes/snapshots",
                Location: "eastus",
                ResourceGroup: "myrg",
                ProvisioningState: "Succeeded",
                Created: "2026-01-15T10:30:00Z");

            Service.UpdateSnapshot(
                Arg.Any<string>(),
                Arg.Any<string>(),
                Arg.Any<string>(),
                Arg.Any<string>(),
                Arg.Any<string>(),
                Arg.Any<string?>(),
                Arg.Any<string>(),
                Arg.Any<string>(),
                Arg.Any<RetryPolicyOptions>(),
                Arg.Any<CancellationToken>())
                .Returns(expectedSnapshot);
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
            Assert.True(
                response.Message.Contains("required", StringComparison.OrdinalIgnoreCase) ||
                response.Message.Contains("provided", StringComparison.OrdinalIgnoreCase),
                $"Expected a validation message, got: {response.Message}");
        }
    }

    [Theory]
    [InlineData("--no-wait", "no-wait")]
    [InlineData("--acquirePolicyToken", "acquirePolicyToken")]
    [InlineData("--changeReference CR-123", "changeReference")]
    [InlineData("--add properties.foo=bar", "add")]
    [InlineData("--set properties.foo=bar", "set")]
    [InlineData("--remove properties.foo", "remove")]
    [InlineData("--force-string", "force-string")]
    public async Task ExecuteAsync_RejectsUnsupportedArguments(string extraArgs, string expectedArgument)
    {
        var response = await ExecuteCommandAsync($"--account myanfaccount --pool mypool --volume myvolume --snapshot mysnapshot --resource-group myrg --subscription sub123 {extraArgs}");

        Assert.Equal(HttpStatusCode.BadRequest, response.Status);
        Assert.Contains(expectedArgument, response.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task ExecuteAsync_UpdatesSnapshot_Successfully()
    {
        // Arrange
        var account = "myanfaccount";
        var pool = "mypool";
        var volume = "myvolume";
        var snapshot = "mysnapshot";
        var resourceGroup = "myrg";
        var location = "eastus";
        var subscription = "sub123";

        var expectedSnapshot = new SnapshotCreateResult(
            Id: $"/subscriptions/{subscription}/resourceGroups/{resourceGroup}/providers/Microsoft.NetApp/netAppAccounts/{account}/capacityPools/{pool}/volumes/{volume}/snapshots/{snapshot}",
            Name: $"{account}/{pool}/{volume}/{snapshot}",
            Type: "Microsoft.NetApp/netAppAccounts/capacityPools/volumes/snapshots",
            Location: location,
            ResourceGroup: resourceGroup,
            ProvisioningState: "Succeeded",
            Created: "2026-01-15T10:30:00Z");

        Service.UpdateSnapshot(
            Arg.Is(account), Arg.Is(pool), Arg.Is(volume), Arg.Is(snapshot), Arg.Is(resourceGroup), Arg.Is(location), Arg.Is(subscription),
            Arg.Any<string>(), Arg.Any<RetryPolicyOptions>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(expectedSnapshot));

        // Act
        var response = await ExecuteCommandAsync([
            "--account", account, "--pool", pool,
            "--volume", volume, "--snapshot", snapshot,
            "--resource-group", resourceGroup, "--location", location,
            "--subscription", subscription
        ]);

        // Assert
        Assert.NotNull(response);
        Assert.NotNull(response.Results);
        Assert.Equal(HttpStatusCode.OK, response.Status);

        var json = JsonSerializer.Serialize(response.Results);
        var result = JsonSerializer.Deserialize(json, NetAppFilesJsonContext.Default.SnapshotUpdateCommandResult);

        Assert.NotNull(result);
        Assert.NotNull(result.Snapshot);
        Assert.Equal($"{account}/{pool}/{volume}/{snapshot}", result.Snapshot.Name);
        Assert.Equal(location, result.Snapshot.Location);
        Assert.Equal(resourceGroup, result.Snapshot.ResourceGroup);
        Assert.Equal("Succeeded", result.Snapshot.ProvisioningState);
        Assert.Equal("2026-01-15T10:30:00Z", result.Snapshot.Created);
    }

    [Fact]
    public async Task ExecuteAsync_DeserializationValidation()
    {
        // Arrange
        var expectedSnapshot = new SnapshotCreateResult(
            Id: "/subscriptions/sub123/resourceGroups/myrg/providers/Microsoft.NetApp/netAppAccounts/myanfaccount/capacityPools/mypool/volumes/myvolume/snapshots/mysnapshot",
            Name: "myanfaccount/mypool/myvolume/mysnapshot",
            Type: "Microsoft.NetApp/netAppAccounts/capacityPools/volumes/snapshots",
            Location: "westus2",
            ResourceGroup: "myrg",
            ProvisioningState: "Succeeded",
            Created: "2026-01-15T10:30:00Z");

        Service.UpdateSnapshot(
            Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(),
            Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string?>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<RetryPolicyOptions>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(expectedSnapshot));

        // Act
        var response = await ExecuteCommandAsync([
            "--account", "myanfaccount", "--pool", "mypool",
            "--volume", "myvolume", "--snapshot", "mysnapshot",
            "--resource-group", "myrg", "--location", "westus2",
            "--subscription", "sub123"
        ]);

        // Assert
        Assert.NotNull(response.Results);

        var json = JsonSerializer.Serialize(response.Results);
        var result = JsonSerializer.Deserialize(json, NetAppFilesJsonContext.Default.SnapshotUpdateCommandResult);

        Assert.NotNull(result);
        Assert.NotNull(result.Snapshot);
        Assert.Equal("myanfaccount/mypool/myvolume/mysnapshot", result.Snapshot.Name);
        Assert.Equal("westus2", result.Snapshot.Location);
        Assert.Equal("myrg", result.Snapshot.ResourceGroup);
        Assert.Equal("Succeeded", result.Snapshot.ProvisioningState);
        Assert.Equal("Microsoft.NetApp/netAppAccounts/capacityPools/volumes/snapshots", result.Snapshot.Type);
    }

    [Fact]
    public async Task ExecuteAsync_HandlesException()
    {
        // Arrange
        var expectedError = "Test error";

        Service.UpdateSnapshot(
            Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(),
            Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string?>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<RetryPolicyOptions>(), Arg.Any<CancellationToken>())
            .ThrowsAsync(new Exception(expectedError));

        // Act
        var response = await ExecuteCommandAsync([
            "--account", "myanfaccount", "--pool", "mypool",
            "--volume", "myvolume", "--snapshot", "mysnapshot",
            "--resource-group", "myrg", "--location", "eastus",
            "--subscription", "sub123"
        ]);

        // Assert
        Assert.NotNull(response);
        Assert.Equal(HttpStatusCode.InternalServerError, response.Status);
        Assert.Contains(expectedError, response.Message);
        Assert.Contains("troubleshooting", response.Message);
    }

    [Fact]
    public async Task ExecuteAsync_HandlesConflict()
    {
        // Arrange
        Service.UpdateSnapshot(
            Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(),
            Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string?>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<RetryPolicyOptions>(), Arg.Any<CancellationToken>())
            .ThrowsAsync(new RequestFailedException((int)HttpStatusCode.Conflict, "Snapshot already exists"));

        // Act
        var response = await ExecuteCommandAsync([
            "--account", "myanfaccount", "--pool", "mypool",
            "--volume", "myvolume", "--snapshot", "mysnapshot",
            "--resource-group", "myrg", "--location", "eastus",
            "--subscription", "sub123"
        ]);

        // Assert
        Assert.Equal(HttpStatusCode.Conflict, response.Status);
        Assert.Contains("already exists", response.Message);
        Assert.Contains("troubleshooting", response.Message);
    }

    [Fact]
    public async Task ExecuteAsync_HandlesNotFound()
    {
        // Arrange
        Service.UpdateSnapshot(
            Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(),
            Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string?>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<RetryPolicyOptions>(), Arg.Any<CancellationToken>())
            .ThrowsAsync(new RequestFailedException((int)HttpStatusCode.NotFound, "Snapshot not found"));

        // Act
        var response = await ExecuteCommandAsync([
            "--account", "myanfaccount", "--pool", "mypool",
            "--volume", "myvolume", "--snapshot", "mysnapshot",
            "--resource-group", "nonexistentrg", "--location", "eastus",
            "--subscription", "sub123"
        ]);

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.Status);
        Assert.Contains("not found", response.Message);
        Assert.Contains("troubleshooting", response.Message);
    }

    [Fact]
    public async Task ExecuteAsync_HandlesAuthorizationFailure()
    {
        // Arrange
        Service.UpdateSnapshot(
            Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(),
            Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string?>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<RetryPolicyOptions>(), Arg.Any<CancellationToken>())
            .ThrowsAsync(new RequestFailedException((int)HttpStatusCode.Forbidden, "Authorization failed"));

        // Act
        var response = await ExecuteCommandAsync([
            "--account", "myanfaccount", "--pool", "mypool",
            "--volume", "myvolume", "--snapshot", "mysnapshot",
            "--resource-group", "myrg", "--location", "eastus",
            "--subscription", "sub123"
        ]);

        // Assert
        Assert.Equal(HttpStatusCode.Forbidden, response.Status);
        Assert.Contains("Authorization failed", response.Message);
        Assert.Contains("troubleshooting", response.Message);
    }

    [Fact]
    public async Task ExecuteAsync_HandlesServiceErrors()
    {
        // Arrange
        Service.UpdateSnapshot(
            Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(),
            Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<RetryPolicyOptions>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromException<SnapshotCreateResult>(new Exception("Test error")));

        // Act
        var response = await ExecuteCommandAsync([
            "--account", "myanfaccount", "--pool", "mypool",
            "--volume", "myvolume", "--snapshot", "mysnapshot",
            "--resource-group", "myrg", "--location", "eastus",
            "--subscription", "sub123"
        ]);

        // Assert
        Assert.Equal(HttpStatusCode.InternalServerError, response.Status);
        Assert.Contains("Test error", response.Message);
        Assert.Contains("troubleshooting", response.Message);
    }

    [Fact]
    public async Task ExecuteAsync_CallsServiceWithCorrectParameters()
    {
        // Arrange
        var account = "myanfaccount";
        var pool = "mypool";
        var volume = "myvolume";
        var snapshot = "mysnapshot";
        var resourceGroup = "myrg";
        var location = "eastus";
        var subscription = "sub123";

        var expectedSnapshot = new SnapshotCreateResult(
            Id: $"/subscriptions/{subscription}/resourceGroups/{resourceGroup}/providers/Microsoft.NetApp/netAppAccounts/{account}/capacityPools/{pool}/volumes/{volume}/snapshots/{snapshot}",
            Name: $"{account}/{pool}/{volume}/{snapshot}",
            Type: "Microsoft.NetApp/netAppAccounts/capacityPools/volumes/snapshots",
            Location: location,
            ResourceGroup: resourceGroup,
            ProvisioningState: "Succeeded",
            Created: "2026-01-15T10:30:00Z");

        Service.UpdateSnapshot(
            account, pool, volume, snapshot, resourceGroup, location, subscription,
            null, Arg.Any<RetryPolicyOptions>(), Arg.Any<CancellationToken>())
            .Returns(expectedSnapshot);

        // Act
        var response = await ExecuteCommandAsync([
            "--account", account, "--pool", pool,
            "--volume", volume, "--snapshot", snapshot,
            "--resource-group", resourceGroup, "--location", location,
            "--subscription", subscription
        ]);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.Status);
        await Service.Received(1).UpdateSnapshot(
            account, pool, volume, snapshot, resourceGroup, location, subscription,
            null, Arg.Any<RetryPolicyOptions>(), Arg.Any<CancellationToken>());
    }
}
