// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.CommandLine;
using System.Net;
using System.Text.Json;
using Azure.Mcp.Core.Options;
using Azure.Mcp.Tools.NetAppFiles.Commands;
using Azure.Mcp.Tools.NetAppFiles.Commands.Snapshot;
using Azure.Mcp.Tools.NetAppFiles.Models;
using Azure.Mcp.Tools.NetAppFiles.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Mcp.Core.Models.Command;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Xunit;

namespace Azure.Mcp.Tools.NetAppFiles.UnitTests.Snapshot;

public class SnapshotCreateCommandTests
{
    private readonly IServiceProvider _serviceProvider;
    private readonly INetAppFilesService _netAppFilesService;
    private readonly ILogger<SnapshotCreateCommand> _logger;
    private readonly SnapshotCreateCommand _command;
    private readonly CommandContext _context;
    private readonly Command _commandDefinition;

    public SnapshotCreateCommandTests()
    {
        _netAppFilesService = Substitute.For<INetAppFilesService>();
        _logger = Substitute.For<ILogger<SnapshotCreateCommand>>();

        var collection = new ServiceCollection().AddSingleton(_netAppFilesService);

        _serviceProvider = collection.BuildServiceProvider();
        _command = new(_logger);
        _context = new(_serviceProvider);
        _commandDefinition = _command.GetCommand();
    }

    [Fact]
    public void Constructor_InitializesCommandCorrectly()
    {
        var command = _command.GetCommand();
        Assert.Equal("create", command.Name);
        Assert.NotNull(command.Description);
        Assert.NotEmpty(command.Description);
    }

    [Theory]
    [InlineData("--account myanfaccount --pool mypool --volume myvolume --snapshot mysnapshot --resource-group myrg --location eastus --subscription sub123", true)]
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

            _netAppFilesService.CreateSnapshot(
                Arg.Any<string>(),
                Arg.Any<string>(),
                Arg.Any<string>(),
                Arg.Any<string>(),
                Arg.Any<string>(),
                Arg.Any<string>(),
                Arg.Any<string>(),
                Arg.Any<string>(),
                Arg.Any<RetryPolicyOptions>(),
                Arg.Any<CancellationToken>())
                .Returns(expectedSnapshot);
        }

        var parseResult = _commandDefinition.Parse(args);

        // Act
        var response = await _command.ExecuteAsync(_context, parseResult, TestContext.Current.CancellationToken);

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
    public async Task ExecuteAsync_CreatesSnapshot_Successfully()
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

        _netAppFilesService.CreateSnapshot(
            Arg.Is(account), Arg.Is(pool), Arg.Is(volume), Arg.Is(snapshot), Arg.Is(resourceGroup), Arg.Is(location), Arg.Is(subscription),
            Arg.Any<string>(), Arg.Any<RetryPolicyOptions>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(expectedSnapshot));

        var args = _commandDefinition.Parse([
            "--account", account, "--pool", pool,
            "--volume", volume, "--snapshot", snapshot,
            "--resource-group", resourceGroup, "--location", location,
            "--subscription", subscription
        ]);

        // Act
        var response = await _command.ExecuteAsync(_context, args, TestContext.Current.CancellationToken);

        // Assert
        Assert.NotNull(response);
        Assert.NotNull(response.Results);
        Assert.Equal(HttpStatusCode.OK, response.Status);

        var json = JsonSerializer.Serialize(response.Results);
        var result = JsonSerializer.Deserialize(json, NetAppFilesJsonContext.Default.SnapshotCreateCommandResult);

        Assert.NotNull(result);
        Assert.NotNull(result.Snapshot);
        Assert.Equal($"{account}/{pool}/{volume}/{snapshot}", result.Snapshot.Name);
        Assert.Equal(location, result.Snapshot.Location);
        Assert.Equal(resourceGroup, result.Snapshot.ResourceGroup);
        Assert.Equal("Succeeded", result.Snapshot.ProvisioningState);
        Assert.Equal("2026-01-15T10:30:00Z", result.Snapshot.Created);
    }

    [Fact]
    public async Task ExecuteAsync_HandlesException()
    {
        // Arrange
        var expectedError = "Test error";

        _netAppFilesService.CreateSnapshot(
            Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(),
            Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<RetryPolicyOptions>(), Arg.Any<CancellationToken>())
            .ThrowsAsync(new Exception(expectedError));

        var args = _commandDefinition.Parse([
            "--account", "myanfaccount", "--pool", "mypool",
            "--volume", "myvolume", "--snapshot", "mysnapshot",
            "--resource-group", "myrg", "--location", "eastus",
            "--subscription", "sub123"
        ]);

        // Act
        var response = await _command.ExecuteAsync(_context, args, TestContext.Current.CancellationToken);

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
        _netAppFilesService.CreateSnapshot(
            Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(),
            Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<RetryPolicyOptions>(), Arg.Any<CancellationToken>())
            .ThrowsAsync(new RequestFailedException((int)HttpStatusCode.Conflict, "Snapshot already exists"));

        var args = _commandDefinition.Parse([
            "--account", "myanfaccount", "--pool", "mypool",
            "--volume", "myvolume", "--snapshot", "mysnapshot",
            "--resource-group", "myrg", "--location", "eastus",
            "--subscription", "sub123"
        ]);

        // Act
        var response = await _command.ExecuteAsync(_context, args, TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(HttpStatusCode.Conflict, response.Status);
        Assert.Contains("already exists", response.Message);
        Assert.Contains("troubleshooting", response.Message);
    }

    [Fact]
    public async Task ExecuteAsync_HandlesNotFound()
    {
        // Arrange
        _netAppFilesService.CreateSnapshot(
            Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(),
            Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<RetryPolicyOptions>(), Arg.Any<CancellationToken>())
            .ThrowsAsync(new RequestFailedException((int)HttpStatusCode.NotFound, "Volume not found"));

        var args = _commandDefinition.Parse([
            "--account", "myanfaccount", "--pool", "mypool",
            "--volume", "myvolume", "--snapshot", "mysnapshot",
            "--resource-group", "nonexistentrg", "--location", "eastus",
            "--subscription", "sub123"
        ]);

        // Act
        var response = await _command.ExecuteAsync(_context, args, TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.Status);
        Assert.Contains("not found", response.Message);
        Assert.Contains("troubleshooting", response.Message);
    }

    [Fact]
    public async Task ExecuteAsync_HandlesAuthorizationFailure()
    {
        // Arrange
        _netAppFilesService.CreateSnapshot(
            Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(),
            Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<RetryPolicyOptions>(), Arg.Any<CancellationToken>())
            .ThrowsAsync(new RequestFailedException((int)HttpStatusCode.Forbidden, "Authorization failed"));

        var args = _commandDefinition.Parse([
            "--account", "myanfaccount", "--pool", "mypool",
            "--volume", "myvolume", "--snapshot", "mysnapshot",
            "--resource-group", "myrg", "--location", "eastus",
            "--subscription", "sub123"
        ]);

        // Act
        var response = await _command.ExecuteAsync(_context, args, TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(HttpStatusCode.Forbidden, response.Status);
        Assert.Contains("Authorization failed", response.Message);
        Assert.Contains("troubleshooting", response.Message);
    }

    [Fact]
    public async Task ExecuteAsync_HandlesServiceErrors()
    {
        // Arrange
        _netAppFilesService.CreateSnapshot(
            Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(),
            Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<RetryPolicyOptions>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromException<SnapshotCreateResult>(new Exception("Test error")));

        var args = _commandDefinition.Parse([
            "--account", "myanfaccount", "--pool", "mypool",
            "--volume", "myvolume", "--snapshot", "mysnapshot",
            "--resource-group", "myrg", "--location", "eastus",
            "--subscription", "sub123"
        ]);

        // Act
        var response = await _command.ExecuteAsync(_context, args, TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(HttpStatusCode.InternalServerError, response.Status);
        Assert.Contains("Test error", response.Message);
        Assert.Contains("troubleshooting", response.Message);
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

        _netAppFilesService.CreateSnapshot(
            Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(),
            Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<RetryPolicyOptions>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(expectedSnapshot));

        var args = _commandDefinition.Parse([
            "--account", "myanfaccount", "--pool", "mypool",
            "--volume", "myvolume", "--snapshot", "mysnapshot",
            "--resource-group", "myrg", "--location", "westus2",
            "--subscription", "sub123"
        ]);

        // Act
        var response = await _command.ExecuteAsync(_context, args, TestContext.Current.CancellationToken);

        // Assert
        Assert.NotNull(response.Results);

        var json = JsonSerializer.Serialize(response.Results);
        var result = JsonSerializer.Deserialize(json, NetAppFilesJsonContext.Default.SnapshotCreateCommandResult);

        Assert.NotNull(result);
        Assert.NotNull(result.Snapshot);
        Assert.Equal("myanfaccount/mypool/myvolume/mysnapshot", result.Snapshot.Name);
        Assert.Equal("westus2", result.Snapshot.Location);
        Assert.Equal("myrg", result.Snapshot.ResourceGroup);
        Assert.Equal("Succeeded", result.Snapshot.ProvisioningState);
        Assert.Equal("Microsoft.NetApp/netAppAccounts/capacityPools/volumes/snapshots", result.Snapshot.Type);
        Assert.Equal("2026-01-15T10:30:00Z", result.Snapshot.Created);
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

        _netAppFilesService.CreateSnapshot(
            account, pool, volume, snapshot, resourceGroup, location, subscription,
            null, Arg.Any<RetryPolicyOptions>(), Arg.Any<CancellationToken>())
            .Returns(expectedSnapshot);

        var args = _commandDefinition.Parse([
            "--account", account, "--pool", pool,
            "--volume", volume, "--snapshot", snapshot,
            "--resource-group", resourceGroup, "--location", location,
            "--subscription", subscription
        ]);

        // Act
        var response = await _command.ExecuteAsync(_context, args, TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.Status);
        await _netAppFilesService.Received(1).CreateSnapshot(
            account, pool, volume, snapshot, resourceGroup, location, subscription,
            null, Arg.Any<RetryPolicyOptions>(), Arg.Any<CancellationToken>());
    }
}
