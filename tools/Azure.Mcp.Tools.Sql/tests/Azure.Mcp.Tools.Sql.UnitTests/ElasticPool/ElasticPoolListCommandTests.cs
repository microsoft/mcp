// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.CommandLine.Parsing;
using Azure.Mcp.Core.Models.Command;
using Azure.Mcp.Core.Options;
using Azure.Mcp.Tools.Sql.Commands.ElasticPool;
using Azure.Mcp.Tools.Sql.Models;
using Azure.Mcp.Tools.Sql.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Xunit;

namespace Azure.Mcp.Tools.Sql.UnitTests.ElasticPool;

public class ElasticPoolListCommandTests
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ISqlService _sqlService;
    private readonly ILogger<ElasticPoolListCommand> _logger;
    private readonly ElasticPoolListCommand _command;
    private readonly CommandContext _context;
    private readonly Parser _parser;

    public ElasticPoolListCommandTests()
    {
        _sqlService = Substitute.For<ISqlService>();
        _logger = Substitute.For<ILogger<ElasticPoolListCommand>>();

        var collection = new ServiceCollection();
        collection.AddSingleton(_sqlService);
        _serviceProvider = collection.BuildServiceProvider();

        _command = new(_logger);
        _context = new(_serviceProvider);
        _parser = new(_command.GetCommand());
    }

    [Fact]
    public void Constructor_InitializesCommandCorrectly()
    {
        var command = _command.GetCommand();
        Assert.Equal("list", command.Name);
        Assert.NotNull(command.Description);
        Assert.NotEmpty(command.Description);
        Assert.Contains("elastic pools", command.Description);
    }

    [Fact]
    public async Task ExecuteAsync_WithValidParameters_ReturnsElasticPools()
    {
        // Arrange
        var mockElasticPools = new List<SqlElasticPool>
        {
            new(
                Name: "pool1",
                Id: "/subscriptions/sub/resourceGroups/rg/providers/Microsoft.Sql/servers/server1/elasticPools/pool1",
                Type: "Microsoft.Sql/servers/elasticPools",
                Location: "East US",
                Sku: new ElasticPoolSku("StandardPool", "Standard", 100, null, null),
                State: "Ready",
                CreationDate: DateTimeOffset.UtcNow,
                MaxSizeBytes: 5368709120,
                PerDatabaseSettings: new ElasticPoolPerDatabaseSettings(0, 25),
                ZoneRedundant: false,
                LicenseType: "LicenseIncluded",
                DatabaseDtuMin: 0,
                DatabaseDtuMax: 25,
                Dtu: 100,
                StorageMB: 5120
            )
        };

        _sqlService.GetElasticPoolsAsync(
            Arg.Is("server1"),
            Arg.Is("rg"),
            Arg.Is("sub"),
            Arg.Any<RetryPolicyOptions>(),
            Arg.Any<CancellationToken>())
            .Returns(mockElasticPools);

        var args = _parser.Parse(["--subscription", "sub", "--resource-group", "rg", "--server", "server1"]);

        // Act
        var response = await _command.ExecuteAsync(_context, args);

        // Assert
        Assert.NotNull(response);
        Assert.Equal(200, response.Status);
        Assert.NotNull(response.Results);
        Assert.Equal("Success", response.Message);
    }

    [Fact]
    public async Task ExecuteAsync_WithEmptyList_ReturnsEmptyResults()
    {
        // Arrange
        var mockElasticPools = new List<SqlElasticPool>();

        _sqlService.GetElasticPoolsAsync(
            Arg.Is("server1"),
            Arg.Is("rg"),
            Arg.Is("sub"),
            Arg.Any<RetryPolicyOptions>(),
            Arg.Any<CancellationToken>())
            .Returns(mockElasticPools);

        var args = _parser.Parse(["--subscription", "sub", "--resource-group", "rg", "--server", "server1"]);

        // Act
        var response = await _command.ExecuteAsync(_context, args);

        // Assert
        Assert.NotNull(response);
        Assert.Equal(200, response.Status);
        Assert.NotNull(response.Results);
        Assert.Equal("Success", response.Message);
    }

    [Fact]
    public async Task ExecuteAsync_HandlesServiceErrors()
    {
        // Arrange
        _sqlService.GetElasticPoolsAsync(
                Arg.Any<string>(),
                Arg.Any<string>(),
                Arg.Any<string>(),
                Arg.Any<RetryPolicyOptions>(),
                Arg.Any<CancellationToken>())
            .ThrowsAsync(new Exception("Test error"));

        var args = _parser.Parse(["--subscription", "sub", "--resource-group", "rg", "--server", "server1"]);

        // Act
        var response = await _command.ExecuteAsync(_context, args);

        // Assert
        Assert.Equal(500, response.Status);
        Assert.Contains("Test error", response.Message);
        Assert.Contains("troubleshooting", response.Message);
    }

    [Fact]
    public async Task ExecuteAsync_HandlesRequestFailedException_NotFound()
    {
        // Arrange
        var requestException = new Azure.RequestFailedException(404, "Server not found");
        _sqlService.GetElasticPoolsAsync(
                Arg.Any<string>(),
                Arg.Any<string>(),
                Arg.Any<string>(),
                Arg.Any<RetryPolicyOptions>(),
                Arg.Any<CancellationToken>())
            .ThrowsAsync(requestException);

        var args = _parser.Parse(["--subscription", "sub", "--resource-group", "rg", "--server", "server1"]);

        // Act
        var response = await _command.ExecuteAsync(_context, args);

        // Assert
        Assert.Equal(404, response.Status);
        Assert.Contains("SQL server not found", response.Message);
    }

    [Fact]
    public async Task ExecuteAsync_HandlesRequestFailedException_Forbidden()
    {
        // Arrange
        var requestException = new Azure.RequestFailedException(403, "Forbidden");
        _sqlService.GetElasticPoolsAsync(
                Arg.Any<string>(),
                Arg.Any<string>(),
                Arg.Any<string>(),
                Arg.Any<RetryPolicyOptions>(),
                Arg.Any<CancellationToken>())
            .ThrowsAsync(requestException);

        var args = _parser.Parse(["--subscription", "sub", "--resource-group", "rg", "--server", "server1"]);

        // Act
        var response = await _command.ExecuteAsync(_context, args);

        // Assert
        Assert.Equal(403, response.Status);
        Assert.Contains("Authorization failed", response.Message);
    }

    [Theory]
    [InlineData("--subscription sub --resource-group rg --server server1", true)]
    [InlineData("--resource-group rg --server server1", false)]  // Missing subscription
    [InlineData("--subscription sub --server server1", false)]   // Missing resource group
    [InlineData("--subscription sub --resource-group rg", false)] // Missing server
    [InlineData("", false)]  // Missing all required options
    public async Task ExecuteAsync_ValidatesRequiredParameters(string args, bool shouldSucceed)
    {
        // Arrange
        if (shouldSucceed)
        {
            _sqlService.GetElasticPoolsAsync(
                Arg.Any<string>(),
                Arg.Any<string>(),
                Arg.Any<string>(),
                Arg.Any<RetryPolicyOptions>(),
                Arg.Any<CancellationToken>())
                .Returns(new List<SqlElasticPool>());
        }

        var parseResult = _parser.Parse(args.Split(' ', StringSplitOptions.RemoveEmptyEntries));

        // Act
        var response = await _command.ExecuteAsync(_context, parseResult);

        // Assert
        if (shouldSucceed)
        {
            Assert.Equal(200, response.Status);
            Assert.Equal("Success", response.Message);
        }
        else
        {
            Assert.Equal(400, response.Status);
            Assert.Contains("required", response.Message.ToLower());
        }
    }
}
