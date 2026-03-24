// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.CommandLine;
using System.Net;
using System.Text.Json;
using Azure.Mcp.Core.Options;
using Azure.Mcp.Core.Services.Azure;
using Azure.Mcp.Tools.NetAppFiles.Commands;
using Azure.Mcp.Tools.NetAppFiles.Commands.ReplicationStatus;
using Azure.Mcp.Tools.NetAppFiles.Models;
using Azure.Mcp.Tools.NetAppFiles.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Mcp.Core.Models.Command;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Xunit;

namespace Azure.Mcp.Tools.NetAppFiles.UnitTests.ReplicationStatus;

public class ReplicationStatusGetCommandTests
{
    private readonly IServiceProvider _serviceProvider;
    private readonly INetAppFilesService _netAppFilesService;
    private readonly ILogger<ReplicationStatusGetCommand> _logger;
    private readonly ReplicationStatusGetCommand _command;
    private readonly CommandContext _context;
    private readonly Command _commandDefinition;

    public ReplicationStatusGetCommandTests()
    {
        _netAppFilesService = Substitute.For<INetAppFilesService>();
        _logger = Substitute.For<ILogger<ReplicationStatusGetCommand>>();

        var collection = new ServiceCollection().AddSingleton(_netAppFilesService);

        _serviceProvider = collection.BuildServiceProvider();
        _command = new(_logger);
        _context = new(_serviceProvider);
        _commandDefinition = _command.GetCommand();
    }

    [Fact]
    public async Task ExecuteAsync_NoVolumeParameter_ReturnsAllReplicationStatuses()
    {
        // Arrange
        var subscription = "sub123";
        var expectedStatuses = new ResourceQueryResults<ReplicationStatusInfo>(
        [
            new("account1/pool1/vol1", "eastus", "rg1", "Dst", "hourly", "/subscriptions/sub/resourceGroups/rg2/providers/Microsoft.NetApp/netAppAccounts/account2/capacityPools/pool2/volumes/vol2", "westus", "repl-id-1"),
            new("account1/pool1/vol2", "eastus", "rg1", "Src", "daily", "/subscriptions/sub/resourceGroups/rg3/providers/Microsoft.NetApp/netAppAccounts/account3/capacityPools/pool3/volumes/vol3", "northeurope", "repl-id-2")
        ], false);

        _netAppFilesService.GetReplicationStatusDetails(
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Is(subscription),
            Arg.Any<string>(),
            Arg.Any<RetryPolicyOptions>(),
            Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(expectedStatuses));

        var args = _commandDefinition.Parse(["--subscription", subscription]);

        // Act
        var response = await _command.ExecuteAsync(_context, args, TestContext.Current.CancellationToken);

        // Assert
        Assert.NotNull(response);
        Assert.NotNull(response.Results);

        var json = JsonSerializer.Serialize(response.Results);
        var result = JsonSerializer.Deserialize(json, NetAppFilesJsonContext.Default.ReplicationStatusGetCommandResult);

        Assert.NotNull(result);
        Assert.NotNull(result.ReplicationStatuses);
        Assert.Equal(expectedStatuses.Results.Count, result.ReplicationStatuses.Count);
        Assert.Equal(expectedStatuses.Results.Select(r => r.Name), result.ReplicationStatuses.Select(r => r.Name));
    }

    [Fact]
    public async Task ExecuteAsync_ReturnsEmpty_WhenNoReplicationStatuses()
    {
        // Arrange
        var subscription = "sub123";

        _netAppFilesService.GetReplicationStatusDetails(
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Is(subscription),
            Arg.Any<string>(),
            Arg.Any<RetryPolicyOptions>(),
            Arg.Any<CancellationToken>())
            .Returns(new ResourceQueryResults<ReplicationStatusInfo>([], false));

        var args = _commandDefinition.Parse(["--subscription", subscription]);

        // Act
        var response = await _command.ExecuteAsync(_context, args, TestContext.Current.CancellationToken);

        // Assert
        Assert.NotNull(response);
        Assert.NotNull(response.Results);

        var json = JsonSerializer.Serialize(response.Results);
        var result = JsonSerializer.Deserialize(json, NetAppFilesJsonContext.Default.ReplicationStatusGetCommandResult);

        Assert.NotNull(result);
        Assert.Empty(result.ReplicationStatuses);
    }

    [Fact]
    public async Task ExecuteAsync_HandlesException()
    {
        // Arrange
        var expectedError = "Test error";
        var subscription = "sub123";

        _netAppFilesService.GetReplicationStatusDetails(
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Is(subscription),
            null,
            Arg.Any<RetryPolicyOptions>(),
            Arg.Any<CancellationToken>())
            .ThrowsAsync(new Exception(expectedError));

        var args = _commandDefinition.Parse(["--subscription", subscription]);

        // Act
        var response = await _command.ExecuteAsync(_context, args, TestContext.Current.CancellationToken);

        // Assert
        Assert.NotNull(response);
        Assert.Equal(HttpStatusCode.InternalServerError, response.Status);
        Assert.StartsWith(expectedError, response.Message);
    }

    [Fact]
    public void Constructor_InitializesCommandCorrectly()
    {
        var command = _command.GetCommand();
        Assert.Equal("get", command.Name);
        Assert.NotNull(command.Description);
        Assert.NotEmpty(command.Description);
    }

    [Theory]
    [InlineData("--subscription sub123", true)]
    [InlineData("--subscription sub123 --account myanfaccount", true)]
    [InlineData("--subscription sub123 --account myanfaccount --pool mypool", true)]
    [InlineData("--subscription sub123 --account myanfaccount --pool mypool --volume myvol", true)]
    [InlineData("--subscription sub123 --volume myvol", true)]
    [InlineData("--account myanfaccount", false)]
    public async Task ExecuteAsync_ValidatesInputCorrectly(string args, bool shouldSucceed)
    {
        if (shouldSucceed)
        {
            var expectedStatuses = new ResourceQueryResults<ReplicationStatusInfo>(
                [new("account1/pool1/vol1", "eastus", "rg1", "Dst", "hourly", "/subscriptions/sub/resourceGroups/rg2/providers/Microsoft.NetApp/netAppAccounts/account2/capacityPools/pool2/volumes/vol2", "westus", "repl-id-1")],
                false);

            _netAppFilesService.GetReplicationStatusDetails(
                Arg.Any<string?>(), Arg.Any<string?>(), Arg.Any<string?>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<RetryPolicyOptions>(), Arg.Any<CancellationToken>())
                .Returns(Task.FromResult(expectedStatuses));
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
    public async Task ExecuteAsync_ReturnsReplicationStatus_WhenVolumeExists()
    {
        // Arrange
        var account = "myanfaccount";
        var pool = "mypool";
        var volume = "myvol";
        var subscription = "sub123";
        var expectedStatuses = new ResourceQueryResults<ReplicationStatusInfo>(
            [new($"{account}/{pool}/{volume}", "eastus", "rg1", "Dst", "hourly", "/subscriptions/sub/resourceGroups/rg2/providers/Microsoft.NetApp/netAppAccounts/account2/capacityPools/pool2/volumes/vol2", "westus", "repl-id-1")],
            false);

        _netAppFilesService.GetReplicationStatusDetails(
            Arg.Is(account), Arg.Is(pool), Arg.Is(volume), Arg.Is(subscription), Arg.Any<string>(), Arg.Any<RetryPolicyOptions>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(expectedStatuses));

        var args = _commandDefinition.Parse(["--account", account, "--pool", pool, "--volume", volume, "--subscription", subscription]);

        // Act
        var response = await _command.ExecuteAsync(_context, args, TestContext.Current.CancellationToken);

        // Assert
        Assert.NotNull(response);
        Assert.NotNull(response.Results);
        Assert.Equal(HttpStatusCode.OK, response.Status);

        var json = JsonSerializer.Serialize(response.Results);
        var result = JsonSerializer.Deserialize(json, NetAppFilesJsonContext.Default.ReplicationStatusGetCommandResult);

        Assert.NotNull(result);
        Assert.Single(result.ReplicationStatuses);
        Assert.Equal($"{account}/{pool}/{volume}", result.ReplicationStatuses[0].Name);
        Assert.Equal("eastus", result.ReplicationStatuses[0].Location);
        Assert.Equal("rg1", result.ReplicationStatuses[0].ResourceGroup);
        Assert.Equal("Dst", result.ReplicationStatuses[0].EndpointType);
        Assert.Equal("hourly", result.ReplicationStatuses[0].ReplicationSchedule);
        Assert.Equal("westus", result.ReplicationStatuses[0].RemoteVolumeRegion);
    }

    [Fact]
    public async Task ExecuteAsync_HandlesServiceErrors()
    {
        // Arrange
        var subscription = "sub123";

        _netAppFilesService.GetReplicationStatusDetails(
            Arg.Any<string?>(), Arg.Any<string?>(), Arg.Any<string?>(), Arg.Is(subscription), Arg.Any<string>(), Arg.Any<RetryPolicyOptions>(), Arg.Any<CancellationToken>())
            .ThrowsAsync(new Exception("Test error"));

        var parseResult = _commandDefinition.Parse(["--subscription", subscription]);

        // Act
        var response = await _command.ExecuteAsync(_context, parseResult, TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(HttpStatusCode.InternalServerError, response.Status);
        Assert.Contains("Test error", response.Message);
        Assert.Contains("troubleshooting", response.Message);
    }

    [Fact]
    public async Task ExecuteAsync_HandlesNotFound()
    {
        // Arrange
        var subscription = "sub123";
        var volume = "nonexistentvol";

        _netAppFilesService.GetReplicationStatusDetails(
            Arg.Any<string?>(), Arg.Any<string?>(), Arg.Is(volume), Arg.Is(subscription), Arg.Any<string>(), Arg.Any<RetryPolicyOptions>(), Arg.Any<CancellationToken>())
            .ThrowsAsync(new RequestFailedException((int)HttpStatusCode.NotFound, "Volume not found"));

        var parseResult = _commandDefinition.Parse(["--volume", volume, "--subscription", subscription]);

        // Act
        var response = await _command.ExecuteAsync(_context, parseResult, TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.Status);
        Assert.Contains("Volume not found", response.Message);
    }

    [Fact]
    public async Task ExecuteAsync_HandlesAuthorizationFailure()
    {
        // Arrange
        var subscription = "sub123";

        _netAppFilesService.GetReplicationStatusDetails(
            Arg.Any<string?>(), Arg.Any<string?>(), Arg.Any<string?>(), Arg.Is(subscription), Arg.Any<string>(), Arg.Any<RetryPolicyOptions>(), Arg.Any<CancellationToken>())
            .ThrowsAsync(new RequestFailedException((int)HttpStatusCode.Forbidden, "Authorization failed"));

        var parseResult = _commandDefinition.Parse(["--subscription", subscription]);

        // Act
        var response = await _command.ExecuteAsync(_context, parseResult, TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(HttpStatusCode.Forbidden, response.Status);
        Assert.Contains("Authorization failed", response.Message);
    }

    [Fact]
    public async Task ExecuteAsync_DeserializationValidation()
    {
        // Arrange
        var subscription = "sub123";
        var expectedStatuses = new ResourceQueryResults<ReplicationStatusInfo>(
            [new("account1/pool1/vol1", "eastus", "rg1", "Dst", "hourly", "/subscriptions/sub/resourceGroups/rg2/providers/Microsoft.NetApp/netAppAccounts/account2/capacityPools/pool2/volumes/vol2", "westus", "repl-id-1")],
            false);

        _netAppFilesService.GetReplicationStatusDetails(
            Arg.Any<string?>(), Arg.Any<string?>(), Arg.Any<string?>(), Arg.Is(subscription), Arg.Any<string>(), Arg.Any<RetryPolicyOptions>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(expectedStatuses));

        var args = _commandDefinition.Parse(["--subscription", subscription]);

        // Act
        var response = await _command.ExecuteAsync(_context, args, TestContext.Current.CancellationToken);

        // Assert
        Assert.NotNull(response.Results);

        var json = JsonSerializer.Serialize(response.Results);
        var result = JsonSerializer.Deserialize(json, NetAppFilesJsonContext.Default.ReplicationStatusGetCommandResult);

        Assert.NotNull(result);
        Assert.Single(result.ReplicationStatuses);
        var status = result.ReplicationStatuses[0];
        Assert.Equal("account1/pool1/vol1", status.Name);
        Assert.Equal("eastus", status.Location);
        Assert.Equal("rg1", status.ResourceGroup);
        Assert.Equal("Dst", status.EndpointType);
        Assert.Equal("hourly", status.ReplicationSchedule);
        Assert.Equal("/subscriptions/sub/resourceGroups/rg2/providers/Microsoft.NetApp/netAppAccounts/account2/capacityPools/pool2/volumes/vol2", status.RemoteVolumeResourceId);
        Assert.Equal("westus", status.RemoteVolumeRegion);
        Assert.Equal("repl-id-1", status.ReplicationId);
    }
}
