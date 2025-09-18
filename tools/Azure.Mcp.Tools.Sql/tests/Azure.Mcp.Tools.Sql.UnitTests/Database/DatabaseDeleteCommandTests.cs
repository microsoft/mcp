// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.CommandLine;
using Azure.Mcp.Core.Models.Command;
using Azure.Mcp.Tools.Sql.Commands.Database;
using Azure.Mcp.Tools.Sql.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Xunit;

namespace Azure.Mcp.Tools.Sql.UnitTests.Database;

public class DatabaseDeleteCommandTests
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ISqlService _sqlService;
    private readonly ILogger<DatabaseDeleteCommand> _logger;
    private readonly DatabaseDeleteCommand _command;
    private readonly CommandContext _context;
    private readonly Command _commandDefinition;

    public DatabaseDeleteCommandTests()
    {
        _sqlService = Substitute.For<ISqlService>();
        _logger = Substitute.For<ILogger<DatabaseDeleteCommand>>();

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
        Assert.Equal("delete", command.Name);
        Assert.NotNull(command.Description);
        Assert.NotEmpty(command.Description);
        Assert.Contains("Deletes an Azure SQL Database", command.Description);
    }

    [Fact]
    public void Command_HasCorrectMetadata()
    {
        Assert.True(_command.Metadata.Destructive);
        Assert.False(_command.Metadata.ReadOnly);
        Assert.True(_command.Metadata.Idempotent);
    }

    [Theory]
    [InlineData("--subscription sub --resource-group rg --server server1 --database db1", true)]
    [InlineData("--subscription sub --resource-group rg --server server1", false)] // Missing database
    [InlineData("--subscription sub --resource-group rg --database db1", false)] // Missing server
    [InlineData("--subscription sub --server server1 --database db1", false)] // Missing resource group
    [InlineData("--resource-group rg --server server1 --database db1", false)] // Missing subscription
    [InlineData("", false)] // Missing all required
    public async Task ExecuteAsync_ValidatesInputCorrectly(string args, bool shouldSucceed)
    {
        // Arrange
        if (shouldSucceed)
        {
            _sqlService.DeleteDatabaseAsync(
                Arg.Any<string>(),
                Arg.Any<string>(),
                Arg.Any<string>(),
                Arg.Any<string>(),
                Arg.Any<Core.Options.RetryPolicyOptions?>(),
                Arg.Any<CancellationToken>())
                .Returns(true);
        }

        var parseResult = _commandDefinition.Parse(args);

        // Act
        var response = await _command.ExecuteAsync(_context, parseResult);

        // Assert
        Assert.Equal(shouldSucceed ? 200 : 400, response.Status);
        if (shouldSucceed)
        {
            Assert.Equal("Success", response.Message);
        }
        else
        {
            Assert.Contains("required", response.Message.ToLower());
        }
    }

    [Fact]
    public async Task ExecuteAsync_DeletesDatabaseSuccessfully()
    {
        _sqlService.DeleteDatabaseAsync(
            "server1",
            "db1",
            "rg1",
            "sub1",
            Arg.Any<Core.Options.RetryPolicyOptions?>(),
            Arg.Any<CancellationToken>())
            .Returns(true);

        var parseResult = _commandDefinition.Parse("--subscription sub1 --resource-group rg1 --server server1 --database db1");

        var response = await _command.ExecuteAsync(_context, parseResult);

        Assert.Equal(200, response.Status);
        Assert.NotNull(response.Results);
        Assert.Equal("Success", response.Message);
    }

    [Fact]
    public async Task ExecuteAsync_IdempotentWhenDatabaseMissing()
    {
        _sqlService.DeleteDatabaseAsync(
            "server1",
            "missingdb",
            "rg1",
            "sub1",
            Arg.Any<Core.Options.RetryPolicyOptions?>(),
            Arg.Any<CancellationToken>())
            .Returns(false);

        var parseResult = _commandDefinition.Parse("--subscription sub1 --resource-group rg1 --server server1 --database missingdb");

        var response = await _command.ExecuteAsync(_context, parseResult);

        Assert.Equal(200, response.Status);
        Assert.NotNull(response.Results);
        Assert.Equal("Success", response.Message);
    }

    [Fact]
    public async Task ExecuteAsync_HandlesServiceErrors()
    {
        _sqlService.DeleteDatabaseAsync(
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<Core.Options.RetryPolicyOptions?>(),
            Arg.Any<CancellationToken>())
            .Returns(Task.FromException<bool>(new Exception("Test error")));

        var parseResult = _commandDefinition.Parse("--subscription sub1 --resource-group rg1 --server server1 --database db1");
        var response = await _command.ExecuteAsync(_context, parseResult);

        Assert.Equal(500, response.Status);
        Assert.Contains("Test error", response.Message);
        Assert.Contains("troubleshooting", response.Message);
    }

    [Fact]
    public async Task ExecuteAsync_Handles404Error()
    {
        var requestFailed = new RequestFailedException(404, "Not found");
        _sqlService.DeleteDatabaseAsync(
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<Core.Options.RetryPolicyOptions?>(),
            Arg.Any<CancellationToken>())
            .Returns(Task.FromException<bool>(requestFailed));

        var parseResult = _commandDefinition.Parse("--subscription sub1 --resource-group rg1 --server server1 --database db1");
        var response = await _command.ExecuteAsync(_context, parseResult);

        Assert.Equal(404, response.Status);
        Assert.Contains("SQL server or database not found", response.Message);
    }

    [Fact]
    public async Task ExecuteAsync_Handles403Error()
    {
        var requestFailed = new RequestFailedException(403, "Access denied");
        _sqlService.DeleteDatabaseAsync(
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<Core.Options.RetryPolicyOptions?>(),
            Arg.Any<CancellationToken>())
            .Returns(Task.FromException<bool>(requestFailed));

        var parseResult = _commandDefinition.Parse("--subscription sub1 --resource-group rg1 --server server1 --database db1");
        var response = await _command.ExecuteAsync(_context, parseResult);

        Assert.Equal(403, response.Status);
        Assert.Contains("Authorization failed", response.Message);
    }

    [Fact]
    public async Task ExecuteAsync_CallsServiceWithCorrectParameters()
    {
        const string subscription = "sub1";
        const string resourceGroup = "rg1";
        const string server = "server1";
        const string database = "db1";

        _sqlService.DeleteDatabaseAsync(
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<Core.Options.RetryPolicyOptions?>(),
            Arg.Any<CancellationToken>())
            .Returns(true);

        var parseResult = _commandDefinition.Parse($"--subscription {subscription} --resource-group {resourceGroup} --server {server} --database {database}");
        await _command.ExecuteAsync(_context, parseResult);

        await _sqlService.Received(1).DeleteDatabaseAsync(
            server,
            database,
            resourceGroup,
            subscription,
            Arg.Any<Core.Options.RetryPolicyOptions?>(),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteAsync_WithRetryPolicyOptions()
    {
        _sqlService.DeleteDatabaseAsync(
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<Core.Options.RetryPolicyOptions?>(),
            Arg.Any<CancellationToken>())
            .Returns(true);

        var parseResult = _commandDefinition.Parse("--subscription sub1 --resource-group rg1 --server server1 --database db1 --retry-max-retries 5");
        var response = await _command.ExecuteAsync(_context, parseResult);

        Assert.Equal(200, response.Status);
        await _sqlService.Received(1).DeleteDatabaseAsync(
            "server1",
            "db1",
            "rg1",
            "sub1",
            Arg.Is<Core.Options.RetryPolicyOptions?>(r => r != null && r.MaxRetries == 5),
            Arg.Any<CancellationToken>());
    }

    [Theory]
    [InlineData("db1")]
    [InlineData("MyDatabase")]
    [InlineData("db-with-hyphens")]
    [InlineData("db_with_underscores")]
    [InlineData("db123")]
    public async Task ExecuteAsync_HandlesVariousDatabaseNames(string dbName)
    {
        _sqlService.DeleteDatabaseAsync(
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<Core.Options.RetryPolicyOptions?>(),
            Arg.Any<CancellationToken>())
            .Returns(true);

        var parseResult = _commandDefinition.Parse($"--subscription sub1 --resource-group rg1 --server server1 --database {dbName}");
        var response = await _command.ExecuteAsync(_context, parseResult);

        Assert.Equal(200, response.Status);
        await _sqlService.Received(1).DeleteDatabaseAsync(
            "server1",
            dbName,
            "rg1",
            "sub1",
            Arg.Any<Core.Options.RetryPolicyOptions?>(),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteAsync_HandlesArgumentException()
    {
        var argumentException = new ArgumentException("Invalid database name");
        _sqlService.DeleteDatabaseAsync(
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<Core.Options.RetryPolicyOptions?>(),
            Arg.Any<CancellationToken>())
            .Returns(Task.FromException<bool>(argumentException));

        var parseResult = _commandDefinition.Parse("--subscription sub1 --resource-group rg1 --server server1 --database invalidDb");
        var response = await _command.ExecuteAsync(_context, parseResult);

        Assert.Equal(500, response.Status);
        Assert.Contains("Invalid database name", response.Message);
    }

    [Fact]
    public async Task ExecuteAsync_VerifiesResultContainsExpectedData()
    {
        _sqlService.DeleteDatabaseAsync(
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<Core.Options.RetryPolicyOptions?>(),
            Arg.Any<CancellationToken>())
            .Returns(true);

        var parseResult = _commandDefinition.Parse("--subscription sub1 --resource-group rg1 --server server1 --database db1");
        var response = await _command.ExecuteAsync(_context, parseResult);

        Assert.Equal(200, response.Status);
        Assert.NotNull(response.Results);
    }
}
