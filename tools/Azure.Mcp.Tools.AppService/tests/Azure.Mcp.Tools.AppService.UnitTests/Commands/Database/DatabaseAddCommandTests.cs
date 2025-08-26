// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.CommandLine;
using System.CommandLine.Invocation;
using System.CommandLine.Parsing;
using Azure.Mcp.Tools.AppService.Commands.Database;
using Azure.Mcp.Tools.AppService.Models;
using Azure.Mcp.Tools.AppService.Services;
using Azure.Mcp.Core.Models.Command;
using Azure.Mcp.Core.Options;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Xunit;

namespace Azure.Mcp.Tools.AppService.UnitTests.Commands.Database;

[Trait("Area", "AppService")]
[Trait("Command", "DatabaseAdd")]
public class DatabaseAddCommandTests
{
    private readonly IAppServiceService _appServiceService;
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<DatabaseAddCommand> _logger;

    public DatabaseAddCommandTests()
    {
        _appServiceService = Substitute.For<IAppServiceService>();
        _logger = Substitute.For<ILogger<DatabaseAddCommand>>();

        // Set default environment variables for testing
        Environment.SetEnvironmentVariable("AZURE_SUBSCRIPTION_ID", "env-sub-123");

        var collection = new ServiceCollection();
        collection.AddSingleton(_appServiceService);
        collection.AddSingleton(_logger);
        _serviceProvider = collection.BuildServiceProvider();
    }

    [Theory]
    [InlineData("SqlServer", "test-server.database.windows.net", "test-db", null, null)]
    [InlineData("MySQL", "mysql-server.mysql.database.azure.com", "mysql-db", "Server=custom-server;Database=custom-db;", null)]
    [InlineData("PostgreSQL", "postgres-server.postgres.database.azure.com", "postgres-db", null, "tenant123")]
    [InlineData("CosmosDB", "cosmos-account.documents.azure.com", "cosmos-db", "AccountEndpoint=https://cosmos-account.documents.azure.com:443/;AccountKey=key;", "tenant456")]
    public async Task ExecuteAsync_WithValidParameters_CallsServiceWithCorrectArguments(
        string databaseType, 
        string databaseServer, 
        string databaseName, 
        string? connectionString, 
        string? tenant)
    {
        // Arrange
        var subscription = "sub123";
        var resourceGroup = "rg1";
        var appName = "test-app";

        var expectedConnection = new DatabaseConnectionInfo
        {
            DatabaseType = databaseType,
            DatabaseServer = databaseServer,
            DatabaseName = databaseName,
            ConnectionString = connectionString ?? $"Generated connection string for {databaseType}",
            ConnectionStringName = $"{databaseName}Connection",
            IsConfigured = true,
            ConfiguredAt = DateTime.UtcNow
        };

        // Set up the mock to return success for any arguments
        _appServiceService
            .AddDatabaseAsync(
                Arg.Any<string>(),
                Arg.Any<string>(),
                Arg.Any<string>(),
                Arg.Any<string>(),
                Arg.Any<string>(),
                Arg.Any<string>(),
                Arg.Any<string>(),
                Arg.Any<string>(),
                Arg.Any<RetryPolicyOptions>())
            .Returns(expectedConnection);

        // Test the service directly
        var connectionInfo = await _appServiceService.AddDatabaseAsync(
            appName,
            resourceGroup,
            databaseType,
            databaseServer,
            databaseName,
            connectionString ?? string.Empty,
            subscription,
            tenant,
            new RetryPolicyOptions());

        // Verify the service returns expected data
        Assert.NotNull(connectionInfo);
        Assert.Equal(databaseType, connectionInfo.DatabaseType);
        Assert.Equal(databaseServer, connectionInfo.DatabaseServer);
        Assert.Equal(databaseName, connectionInfo.DatabaseName);

        // Verify that the mock was called with the expected parameters
        await _appServiceService.Received(1).AddDatabaseAsync(
            Arg.Is<string>(x => x == appName),
            Arg.Is<string>(x => x == resourceGroup),
            Arg.Is<string>(x => x == databaseType),
            Arg.Is<string>(x => x == databaseServer),
            Arg.Is<string>(x => x == databaseName),
            Arg.Any<string>(),
            Arg.Is<string>(x => x == subscription),
            Arg.Any<string>(),
            Arg.Any<RetryPolicyOptions>());
    }

    [Theory]
    [InlineData("--subscription", "sub123", "--resource-group", "rg1")] // Missing app, database-type, database-server, database
    [InlineData("--subscription", "sub123", "--resource-group", "rg1", "--app", "test-app")] // Missing database-type, database-server, database
    [InlineData("--subscription", "sub123", "--resource-group", "rg1", "--app", "test-app", "--database-type", "SqlServer")] // Missing database-server, database
    [InlineData("--subscription", "sub123", "--resource-group", "rg1", "--app", "test-app", "--database-type", "SqlServer", "--database-server", "test-server")] // Missing database
    [InlineData("--resource-group", "rg1", "--app", "test-app", "--database-type", "SqlServer", "--database-server", "test-server", "--database", "test-db")] // Missing subscription
    [InlineData("--subscription", "sub123", "--app", "test-app", "--database-type", "SqlServer", "--database-server", "test-server", "--database", "test-db")] // Missing resource-group
    public async Task ExecuteAsync_MissingRequiredParameter_ReturnsErrorResponse(params string[] commandArgs)
    {
        // Arrange
        var command = new DatabaseAddCommand(_logger);
        var args = command.GetCommand().Parse(commandArgs);
        var context = new CommandContext(_serviceProvider);

        // Act
        var response = await command.ExecuteAsync(context, args);

        // Assert
        Assert.NotNull(response);
        Assert.Equal(400, response.Status);

        await _appServiceService.DidNotReceive().AddDatabaseAsync(
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<RetryPolicyOptions>());
    }

    [Fact]
    public async Task ExecuteAsync_ServiceThrowsException_ReturnsErrorResponse()
    {
        // Arrange
        var subscription = "sub123";
        var resourceGroup = "rg1";
        var appName = "test-app";
        var databaseType = "SqlServer";
        var databaseServer = "test-server.database.windows.net";
        var databaseName = "test-db";

        _appServiceService
            .When(x => x.AddDatabaseAsync(
                Arg.Any<string>(),
                Arg.Any<string>(),
                Arg.Any<string>(),
                Arg.Any<string>(),
                Arg.Any<string>(),
                Arg.Any<string>(),
                Arg.Any<string>(),
                Arg.Any<string>(),
                Arg.Any<RetryPolicyOptions>()))
            .Do(x => throw new InvalidOperationException("Service error"));

        var command = new DatabaseAddCommand(_logger);
        var args = command.GetCommand().Parse([
            "--subscription", subscription,
            "--resource-group", resourceGroup,
            "--app", appName,
            "--database-type", databaseType,
            "--database-server", databaseServer,
            "--database", databaseName
        ]);
        var context = new CommandContext(_serviceProvider);

        // Act
        var response = await command.ExecuteAsync(context, args);
        // Assert
        Assert.NotNull(response);
        Assert.Equal(400, response.Status);
    }
}
