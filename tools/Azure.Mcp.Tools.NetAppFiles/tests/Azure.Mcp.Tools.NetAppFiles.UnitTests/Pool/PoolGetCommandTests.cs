// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.CommandLine;
using System.Net;
using System.Text.Json;
using Azure.Mcp.Core.Options;
using Azure.Mcp.Core.Services.Azure;
using Azure.Mcp.Tools.NetAppFiles.Commands;
using Azure.Mcp.Tools.NetAppFiles.Commands.Pool;
using Azure.Mcp.Tools.NetAppFiles.Models;
using Azure.Mcp.Tools.NetAppFiles.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Mcp.Core.Models.Command;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Xunit;

namespace Azure.Mcp.Tools.NetAppFiles.UnitTests.Pool;

public class PoolGetCommandTests
{
    private readonly IServiceProvider _serviceProvider;
    private readonly INetAppFilesService _netAppFilesService;
    private readonly ILogger<PoolGetCommand> _logger;
    private readonly PoolGetCommand _command;
    private readonly CommandContext _context;
    private readonly Command _commandDefinition;

    public PoolGetCommandTests()
    {
        _netAppFilesService = Substitute.For<INetAppFilesService>();
        _logger = Substitute.For<ILogger<PoolGetCommand>>();

        var collection = new ServiceCollection().AddSingleton(_netAppFilesService);

        _serviceProvider = collection.BuildServiceProvider();
        _command = new(_logger);
        _context = new(_serviceProvider);
        _commandDefinition = _command.GetCommand();
    }

    [Fact]
    public async Task ExecuteAsync_NoPoolParameter_ReturnsAllPools()
    {
        // Arrange
        var subscription = "sub123";
        var expectedPools = new ResourceQueryResults<CapacityPoolInfo>(
        [
            new("account1/pool1", "eastus", "rg1", "Succeeded", "Premium", 4398046511104, "Auto", true, "Single"),
            new("account1/pool2", "westus", "rg2", "Succeeded", "Standard", 4398046511104, "Manual", false, "Single")
        ], false);

        _netAppFilesService.GetPoolDetails(
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Is(subscription),
            Arg.Any<string>(),
            Arg.Any<RetryPolicyOptions>(),
            Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(expectedPools));

        var args = _commandDefinition.Parse(["--subscription", subscription]);

        // Act
        var response = await _command.ExecuteAsync(_context, args, TestContext.Current.CancellationToken);

        // Assert
        Assert.NotNull(response);
        Assert.NotNull(response.Results);

        var json = JsonSerializer.Serialize(response.Results);
        var result = JsonSerializer.Deserialize(json, NetAppFilesJsonContext.Default.PoolGetCommandResult);

        Assert.NotNull(result);
        Assert.NotNull(result.Pools);
        Assert.Equal(expectedPools.Results.Count, result.Pools.Count);
        Assert.Equal(expectedPools.Results.Select(p => p.Name), result.Pools.Select(p => p.Name));
    }

    [Fact]
    public async Task ExecuteAsync_ReturnsEmpty_WhenNoPools()
    {
        // Arrange
        var subscription = "sub123";

        _netAppFilesService.GetPoolDetails(
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Is(subscription),
            Arg.Any<string>(),
            Arg.Any<RetryPolicyOptions>(),
            Arg.Any<CancellationToken>())
            .Returns(new ResourceQueryResults<CapacityPoolInfo>([], false));

        var args = _commandDefinition.Parse(["--subscription", subscription]);

        // Act
        var response = await _command.ExecuteAsync(_context, args, TestContext.Current.CancellationToken);

        // Assert
        Assert.NotNull(response);
        Assert.NotNull(response.Results);

        var json = JsonSerializer.Serialize(response.Results);
        var result = JsonSerializer.Deserialize(json, NetAppFilesJsonContext.Default.PoolGetCommandResult);

        Assert.NotNull(result);
        Assert.Empty(result.Pools);
    }

    [Fact]
    public async Task ExecuteAsync_HandlesException()
    {
        // Arrange
        var expectedError = "Test error";
        var subscription = "sub123";

        _netAppFilesService.GetPoolDetails(
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
    [InlineData("--account myanfaccount", false)] // Missing subscription
    public async Task ExecuteAsync_ValidatesInputCorrectly(string args, bool shouldSucceed)
    {
        if (shouldSucceed)
        {
            var expectedPools = new ResourceQueryResults<CapacityPoolInfo>(
                [new("account1/pool1", "eastus", "rg1", "Succeeded", "Premium", 4398046511104, "Auto", true, "Single")],
                false);

            _netAppFilesService.GetPoolDetails(
                Arg.Any<string?>(), Arg.Any<string?>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<RetryPolicyOptions>(), Arg.Any<CancellationToken>())
                .Returns(Task.FromResult(expectedPools));
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
    public async Task ExecuteAsync_ReturnsPoolDetails_WhenPoolExists()
    {
        // Arrange
        var account = "myanfaccount";
        var pool = "mypool";
        var subscription = "sub123";
        var expectedPools = new ResourceQueryResults<CapacityPoolInfo>(
            [new($"{account}/{pool}", "eastus", "rg1", "Succeeded", "Premium", 4398046511104, "Auto", true, "Single")],
            false);

        _netAppFilesService.GetPoolDetails(
            Arg.Is(account), Arg.Is(pool), Arg.Is(subscription), Arg.Any<string>(), Arg.Any<RetryPolicyOptions>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(expectedPools));

        var args = _commandDefinition.Parse(["--account", account, "--pool", pool, "--subscription", subscription]);

        // Act
        var response = await _command.ExecuteAsync(_context, args, TestContext.Current.CancellationToken);

        // Assert
        Assert.NotNull(response);
        Assert.NotNull(response.Results);
        Assert.Equal(HttpStatusCode.OK, response.Status);

        var json = JsonSerializer.Serialize(response.Results);
        var result = JsonSerializer.Deserialize(json, NetAppFilesJsonContext.Default.PoolGetCommandResult);

        Assert.NotNull(result);
        Assert.Single(result.Pools);
        Assert.Equal($"{account}/{pool}", result.Pools[0].Name);
        Assert.Equal("eastus", result.Pools[0].Location);
        Assert.Equal("rg1", result.Pools[0].ResourceGroup);
        Assert.Equal("Premium", result.Pools[0].ServiceLevel);
    }

    [Fact]
    public async Task ExecuteAsync_HandlesServiceErrors()
    {
        // Arrange
        var subscription = "sub123";

        _netAppFilesService.GetPoolDetails(
            Arg.Any<string?>(), Arg.Any<string?>(), Arg.Is(subscription), Arg.Any<string>(), Arg.Any<RetryPolicyOptions>(), Arg.Any<CancellationToken>())
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
        var pool = "nonexistentpool";

        _netAppFilesService.GetPoolDetails(
            Arg.Any<string?>(), Arg.Is(pool), Arg.Is(subscription), Arg.Any<string>(), Arg.Any<RetryPolicyOptions>(), Arg.Any<CancellationToken>())
            .ThrowsAsync(new RequestFailedException((int)HttpStatusCode.NotFound, "Pool not found"));

        var parseResult = _commandDefinition.Parse(["--pool", pool, "--subscription", subscription]);

        // Act
        var response = await _command.ExecuteAsync(_context, parseResult, TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.Status);
        Assert.Contains("Pool not found", response.Message);
    }

    [Fact]
    public async Task ExecuteAsync_HandlesAuthorizationFailure()
    {
        // Arrange
        var subscription = "sub123";

        _netAppFilesService.GetPoolDetails(
            Arg.Any<string?>(), Arg.Any<string?>(), Arg.Is(subscription), Arg.Any<string>(), Arg.Any<RetryPolicyOptions>(), Arg.Any<CancellationToken>())
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
        var expectedPools = new ResourceQueryResults<CapacityPoolInfo>(
            [new("account1/pool1", "eastus", "rg1", "Succeeded", "Premium", 4398046511104, "Auto", true, "Single")],
            false);

        _netAppFilesService.GetPoolDetails(
            Arg.Any<string?>(), Arg.Any<string?>(), Arg.Is(subscription), Arg.Any<string>(), Arg.Any<RetryPolicyOptions>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(expectedPools));

        var args = _commandDefinition.Parse(["--subscription", subscription]);

        // Act
        var response = await _command.ExecuteAsync(_context, args, TestContext.Current.CancellationToken);

        // Assert
        Assert.NotNull(response.Results);

        var json = JsonSerializer.Serialize(response.Results);
        var result = JsonSerializer.Deserialize(json, NetAppFilesJsonContext.Default.PoolGetCommandResult);

        Assert.NotNull(result);
        Assert.Single(result.Pools);
        var poolInfo = result.Pools[0];
        Assert.Equal("account1/pool1", poolInfo.Name);
        Assert.Equal("eastus", poolInfo.Location);
        Assert.Equal("rg1", poolInfo.ResourceGroup);
        Assert.Equal("Succeeded", poolInfo.ProvisioningState);
        Assert.Equal("Premium", poolInfo.ServiceLevel);
        Assert.Equal(4398046511104, poolInfo.Size);
        Assert.Equal("Auto", poolInfo.QosType);
        Assert.True(poolInfo.CoolAccess);
        Assert.Equal("Single", poolInfo.EncryptionType);
    }
}
