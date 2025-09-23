// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.CommandLine;
using System.Net;
using Azure.Mcp.Core.Models.Command;
using Azure.Mcp.Core.Options;
using Azure.Mcp.Tools.Sql.Commands.Database;
using Azure.Mcp.Tools.Sql.Models;
using Azure.Mcp.Tools.Sql.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Xunit;

namespace Azure.Mcp.Tools.Sql.UnitTests.Database;

public class DatabaseShowCommandTests
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ISqlService _sqlService;
    private readonly ILogger<DatabaseShowCommand> _logger;
    private readonly DatabaseShowCommand _command;
    private readonly CommandContext _context;
    private readonly Command _commandDefinition;

    public DatabaseShowCommandTests()
    {
        _sqlService = Substitute.For<ISqlService>();
        _logger = Substitute.For<ILogger<DatabaseShowCommand>>();

        var collection = new ServiceCollection();
        collection.AddSingleton(_sqlService);
        _serviceProvider = collection.BuildServiceProvider();

        _command = new(_logger);
        _context = new(_serviceProvider);
        _commandDefinition = _command.GetCommand();
    }

    [Fact]
    public void Constructor_InitializesCommandCorrectly()
    {
        var command = _command.GetCommand();
        Assert.Equal("show", command.Name);
        Assert.NotNull(command.Description);
        Assert.NotEmpty(command.Description);
        Assert.Contains("Azure SQL Database", command.Description);
    }

    [Fact]
    public async Task ExecuteAsync_WithValidParameters_ReturnsDatabase()
    {
        // Arrange
        var mockDatabase = new SqlDatabase(
            Name: "testdb",
            Id: "/subscriptions/sub/resourceGroups/rg/providers/Microsoft.Sql/servers/server1/databases/testdb",
            Type: "Microsoft.Sql/servers/databases",
            Location: "East US",
            Sku: null,
            Status: "Online",
            Collation: "SQL_Latin1_General_CP1_CI_AS",
            CreationDate: DateTimeOffset.UtcNow,
            MaxSizeBytes: 1073741824,
            ServiceLevelObjective: "S0",
            Edition: "Standard",
            ElasticPoolName: null,
            EarliestRestoreDate: DateTimeOffset.UtcNow,
            ReadScale: "Disabled",
            ZoneRedundant: false
        );

        _sqlService.GetDatabaseAsync(
            Arg.Is("server1"),
            Arg.Is("testdb"),
            Arg.Is("rg"),
            Arg.Is("sub"),
            Arg.Any<RetryPolicyOptions>(),
            Arg.Any<CancellationToken>())
            .Returns(mockDatabase);

        var args = _commandDefinition.Parse(["--subscription", "sub", "--resource-group", "rg", "--server", "server1", "--database", "testdb"]);

        // Act
        var response = await _command.ExecuteAsync(_context, args);

        // Assert
        Assert.NotNull(response);
        Assert.Equal(HttpStatusCode.OK, response.Status);
        Assert.NotNull(response.Results);
        Assert.Equal("Success", response.Message);
    }

    [Fact]
    public async Task ExecuteAsync_HandlesServiceErrors()
    {
        // Arrange
        _sqlService.GetDatabaseAsync(
                Arg.Any<string>(),
                Arg.Any<string>(),
                Arg.Any<string>(),
                Arg.Any<string>(),
                Arg.Any<RetryPolicyOptions>(),
                Arg.Any<CancellationToken>())
            .ThrowsAsync(new Exception("Test error"));

        var args = _commandDefinition.Parse(["--subscription", "sub", "--resource-group", "rg", "--server", "server1", "--database", "testdb"]);

        // Act
        var response = await _command.ExecuteAsync(_context, args);

        // Assert
        Assert.Equal(HttpStatusCode.InternalServerError, response.Status);
        Assert.Contains("Test error", response.Message);
        Assert.Contains("troubleshooting", response.Message);
    }

    [Fact]
    public async Task ExecuteAsync_HandlesNotFoundDatabase()
    {
        // Arrange
        _sqlService.GetDatabaseAsync(
                Arg.Any<string>(),
                Arg.Any<string>(),
                Arg.Any<string>(),
                Arg.Any<string>(),
                Arg.Any<RetryPolicyOptions>(),
                Arg.Any<CancellationToken>())
            .Returns(Task.FromException<SqlDatabase>(new KeyNotFoundException("Database not found")));

        var args = _commandDefinition.Parse(["--subscription", "sub", "--resource-group", "rg", "--server", "server1", "--database", "notfound"]);

        // Act
        var response = await _command.ExecuteAsync(_context, args);

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.Status);
        Assert.Contains("not found", response.Message);
    }

    [Fact]
    public async Task ExecuteAsync_HandlesRequestFailedException()
    {
        // Arrange
        var requestException = new RequestFailedException((int)HttpStatusCode.NotFound, "Database not found");
        _sqlService.GetDatabaseAsync(
                Arg.Any<string>(),
                Arg.Any<string>(),
                Arg.Any<string>(),
                Arg.Any<string>(),
                Arg.Any<RetryPolicyOptions>(),
                Arg.Any<CancellationToken>())
            .ThrowsAsync(requestException);

        var args = _commandDefinition.Parse(["--subscription", "sub", "--resource-group", "rg", "--server", "server1", "--database", "testdb"]);

        // Act
        var response = await _command.ExecuteAsync(_context, args);

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.Status);
        Assert.Contains("Database or server not found", response.Message);
    }
}
