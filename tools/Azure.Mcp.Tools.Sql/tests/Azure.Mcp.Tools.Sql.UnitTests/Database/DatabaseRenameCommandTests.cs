// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.CommandLine;
using System.Net;
using Azure;
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

public class DatabaseRenameCommandTests
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ISqlService _sqlService;
    private readonly ILogger<DatabaseRenameCommand> _logger;
    private readonly DatabaseRenameCommand _command;
    private readonly CommandContext _context;
    private readonly Command _commandDefinition;

    public DatabaseRenameCommandTests()
    {
        _sqlService = Substitute.For<ISqlService>();
        _logger = Substitute.For<ILogger<DatabaseRenameCommand>>();

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
        Assert.Equal("rename", command.Name);
        Assert.NotNull(command.Description);
        Assert.Contains("Rename an existing Azure SQL Database", command.Description);
    }

    [Fact]
    public async Task ExecuteAsync_WithValidParameters_RenamesDatabase()
    {
        // This test also ensures the fix for the bug where new-database-name was not being bound correctly
        var mockDatabase = new SqlDatabase(
            Name: "newdb",
            Id: "/subscriptions/sub/resourceGroups/rg/providers/Microsoft.Sql/servers/server1/databases/newdb",
            Type: "Microsoft.Sql/servers/databases",
            Location: "East US",
            Sku: null,
            Status: "Online",
            Collation: "SQL_Latin1_General_CP1_CI_AS",
            CreationDate: DateTimeOffset.UtcNow,
            MaxSizeBytes: 2147483648,
            ServiceLevelObjective: "S0",
            Edition: "Standard",
            ElasticPoolName: null,
            EarliestRestoreDate: DateTimeOffset.UtcNow,
            ReadScale: "Disabled",
            ZoneRedundant: false
        );

        _sqlService
            .RenameDatabaseAsync(
                Arg.Is("server1"),
                Arg.Is("olddb"),
                Arg.Is("newdb"), // Verify new-database-name is correctly bound (not null)
                Arg.Is("rg"),
                Arg.Is("sub"),
                Arg.Any<RetryPolicyOptions?>(),
                Arg.Any<CancellationToken>())
            .Returns(mockDatabase);

        var args = _commandDefinition.Parse([
            "--subscription", "sub",
            "--resource-group", "rg",
            "--server", "server1",
            "--database", "olddb",
            "--new-database-name", "newdb"
        ]);

        var response = await _command.ExecuteAsync(_context, args, TestContext.Current.CancellationToken);

        Assert.NotNull(response);
        Assert.Equal(HttpStatusCode.OK, response.Status);
        Assert.NotNull(response.Results);
        Assert.Equal("Success", response.Message);

        // Verify the service was called with the correct new database name (not null)
        await _sqlService.Received(1).RenameDatabaseAsync(
            "server1",
            "olddb",
            "newdb",
            "rg",
            "sub",
            Arg.Any<RetryPolicyOptions?>(),
            Arg.Any<CancellationToken>());
    }

    [Theory]
    [InlineData("", false, "Missing required options")]
    [InlineData("--subscription sub", false, "Missing required options")]
    [InlineData("--subscription sub --resource-group rg", false, "Missing required options")]
    [InlineData("--subscription sub --resource-group rg --server server1", false, "Missing required options")]
    [InlineData("--subscription sub --resource-group rg --server server1 --database olddb", false, "Missing required options")]
    [InlineData("--subscription sub --resource-group rg --server server1 --database olddb --new-database-name newdb", true, null)]
    [InlineData("--resource-group rg --server server1 --database olddb --new-database-name newdb", false, "Missing required options")] // Missing subscription
    [InlineData("--subscription sub --server server1 --database olddb --new-database-name newdb", false, "Missing required options")] // Missing resource-group
    [InlineData("--subscription sub --resource-group rg --database olddb --new-database-name newdb", false, "Missing required options")] // Missing server
    [InlineData("--subscription sub --resource-group rg --server server1 --new-database-name newdb", false, "Missing required options")] // Missing database
    public async Task ExecuteAsync_ValidatesRequiredParameters(string commandArgs, bool shouldSucceed, string? expectedError)
    {
        // Arrange
        if (shouldSucceed)
        {
            var mockDatabase = new SqlDatabase(
                Name: "newdb",
                Id: "/subscriptions/sub/resourceGroups/rg/providers/Microsoft.Sql/servers/server1/databases/newdb",
                Type: "Microsoft.Sql/servers/databases",
                Location: "East US",
                Sku: null,
                Status: "Online",
                Collation: "SQL_Latin1_General_CP1_CI_AS",
                CreationDate: DateTimeOffset.UtcNow,
                MaxSizeBytes: 2147483648,
                ServiceLevelObjective: "S0",
                Edition: "Standard",
                ElasticPoolName: null,
                EarliestRestoreDate: DateTimeOffset.UtcNow,
                ReadScale: "Disabled",
                ZoneRedundant: false
            );

            _sqlService
                .RenameDatabaseAsync(
                    Arg.Any<string>(),
                    Arg.Any<string>(),
                    Arg.Any<string>(),
                    Arg.Any<string>(),
                    Arg.Any<string>(),
                    Arg.Any<RetryPolicyOptions?>(),
                    Arg.Any<CancellationToken>())
                .Returns(mockDatabase);
        }

        var args = _commandDefinition.Parse(commandArgs.Split(' ', StringSplitOptions.RemoveEmptyEntries));

        // Act
        var response = await _command.ExecuteAsync(_context, args, TestContext.Current.CancellationToken);

        // Assert
        if (shouldSucceed)
        {
            Assert.Equal(HttpStatusCode.OK, response.Status);
        }
        else
        {
            Assert.NotEqual(HttpStatusCode.OK, response.Status);
            if (expectedError != null)
            {
                Assert.Contains(expectedError, response.Message, StringComparison.OrdinalIgnoreCase);
            }
        }
    }

    [Fact]
    public async Task ExecuteAsync_HandlesDatabaseNotFound()
    {
        // Arrange
        var notFoundException = new RequestFailedException((int)HttpStatusCode.NotFound, "Database not found");
        _sqlService
            .RenameDatabaseAsync(
                Arg.Any<string>(),
                Arg.Any<string>(),
                Arg.Any<string>(),
                Arg.Any<string>(),
                Arg.Any<string>(),
                Arg.Any<RetryPolicyOptions?>(),
                Arg.Any<CancellationToken>())
            .ThrowsAsync(notFoundException);

        var args = _commandDefinition.Parse([
            "--subscription", "sub",
            "--resource-group", "rg",
            "--server", "server1",
            "--database", "missing",
            "--new-database-name", "newdb"
        ]);

        // Act
        var response = await _command.ExecuteAsync(_context, args, TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.Status);
        Assert.Contains("not found", response.Message);
    }

    [Fact]
    public async Task ExecuteAsync_HandlesConflictWhenNewNameExists()
    {
        // Arrange
        var conflictException = new RequestFailedException((int)HttpStatusCode.Conflict, "Database name already exists");
        _sqlService
            .RenameDatabaseAsync(
                Arg.Any<string>(),
                Arg.Any<string>(),
                Arg.Any<string>(),
                Arg.Any<string>(),
                Arg.Any<string>(),
                Arg.Any<RetryPolicyOptions?>(),
                Arg.Any<CancellationToken>())
            .ThrowsAsync(conflictException);

        var args = _commandDefinition.Parse([
            "--subscription", "sub",
            "--resource-group", "rg",
            "--server", "server1",
            "--database", "olddb",
            "--new-database-name", "existingdb"
        ]);

        // Act
        var response = await _command.ExecuteAsync(_context, args, TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(HttpStatusCode.Conflict, response.Status);
        Assert.Contains("conflict", response.Message, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("does not already exist", response.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task ExecuteAsync_HandlesBadRequest()
    {
        // Arrange
        var badRequestException = new RequestFailedException((int)HttpStatusCode.BadRequest, "Invalid rename operation");
        _sqlService
            .RenameDatabaseAsync(
                Arg.Any<string>(),
                Arg.Any<string>(),
                Arg.Any<string>(),
                Arg.Any<string>(),
                Arg.Any<string>(),
                Arg.Any<RetryPolicyOptions?>(),
                Arg.Any<CancellationToken>())
            .ThrowsAsync(badRequestException);

        var args = _commandDefinition.Parse([
            "--subscription", "sub",
            "--resource-group", "rg",
            "--server", "server1",
            "--database", "olddb",
            "--new-database-name", "invalid-name!"
        ]);

        // Act
        var response = await _command.ExecuteAsync(_context, args, TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.Status);
        Assert.Contains("Invalid database rename operation", response.Message);
    }

    [Fact]
    public async Task ExecuteAsync_HandlesGeneralException()
    {
        // Arrange
        _sqlService
            .RenameDatabaseAsync(
                Arg.Any<string>(),
                Arg.Any<string>(),
                Arg.Any<string>(),
                Arg.Any<string>(),
                Arg.Any<string>(),
                Arg.Any<RetryPolicyOptions?>(),
                Arg.Any<CancellationToken>())
            .ThrowsAsync(new Exception("Unexpected error"));

        var args = _commandDefinition.Parse([
            "--subscription", "sub",
            "--resource-group", "rg",
            "--server", "server1",
            "--database", "olddb",
            "--new-database-name", "newdb"
        ]);

        // Act
        var response = await _command.ExecuteAsync(_context, args, TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(HttpStatusCode.InternalServerError, response.Status);
        Assert.Contains("Unexpected error", response.Message);
        Assert.Contains("troubleshooting", response.Message);
    }

    [Fact]
    public async Task ExecuteAsync_WithSubscriptionFromEnvironment_Succeeds()
    {
        // Arrange - Test when subscription comes from environment variable
        Environment.SetEnvironmentVariable("AZURE_SUBSCRIPTION_ID", "env-sub-id");

        var mockDatabase = new SqlDatabase(
            Name: "newdb",
            Id: "/subscriptions/env-sub-id/resourceGroups/rg/providers/Microsoft.Sql/servers/server1/databases/newdb",
            Type: "Microsoft.Sql/servers/databases",
            Location: "East US",
            Sku: null,
            Status: "Online",
            Collation: "SQL_Latin1_General_CP1_CI_AS",
            CreationDate: DateTimeOffset.UtcNow,
            MaxSizeBytes: 2147483648,
            ServiceLevelObjective: "S0",
            Edition: "Standard",
            ElasticPoolName: null,
            EarliestRestoreDate: DateTimeOffset.UtcNow,
            ReadScale: "Disabled",
            ZoneRedundant: false
        );

        _sqlService
            .RenameDatabaseAsync(
                Arg.Is("server1"),
                Arg.Is("olddb"),
                Arg.Is("newdb"),
                Arg.Is("rg"),
                Arg.Is("env-sub-id"),
                Arg.Any<RetryPolicyOptions?>(),
                Arg.Any<CancellationToken>())
            .Returns(mockDatabase);

        try
        {
            var args = _commandDefinition.Parse([
                "--resource-group", "rg",
                "--server", "server1",
                "--database", "olddb",
                "--new-database-name", "newdb"
            ]);

            // Act
            var response = await _command.ExecuteAsync(_context, args, TestContext.Current.CancellationToken);

            // Assert
            Assert.NotNull(response);
            Assert.Equal(HttpStatusCode.OK, response.Status);
            Assert.NotNull(response.Results);
            Assert.Equal("Success", response.Message);
        }
        finally
        {
            // Clean up environment variable
            Environment.SetEnvironmentVariable("AZURE_SUBSCRIPTION_ID", null);
        }
    }

}
