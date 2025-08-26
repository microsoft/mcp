// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.CommandLine;
using System.CommandLine.Invocation;
using System.CommandLine.Parsing;
using AzureMcp.AppService.Commands.Database;
using AzureMcp.AppService.Models;
using AzureMcp.AppService.Services;
using AzureMcp.Core.Models.Command;
using AzureMcp.Core.Options;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Xunit;

namespace AzureMcp.AppService.UnitTests.Commands.Database;

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

        var collection = new ServiceCollection();
        collection.AddSingleton(_appServiceService);
        collection.AddSingleton(_logger);
        _serviceProvider = collection.BuildServiceProvider();
    }

    [Theory]
    [InlineData("SqlServer", "test-server.database.windows.net", "test-db", null, null, true)]
    [InlineData("MySQL", "mysql-server.mysql.database.azure.com", "mysql-db", "Server=custom-server;Database=custom-db;", null, true)]
    [InlineData("PostgreSQL", "postgres-server.postgres.database.azure.com", "postgres-db", null, "tenant123", true)]
    [InlineData("CosmosDB", "cosmos-account.documents.azure.com", "cosmos-db", "AccountEndpoint=https://cosmos-account.documents.azure.com:443/;AccountKey=key;", "tenant456", true)]
    public async Task ExecuteAsync_WithValidParameters_CallsServiceWithCorrectArguments(
        string databaseType, 
        string databaseServer, 
        string databaseName, 
        string? connectionString, 
        string? tenant, 
        bool expectedSuccess)
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

        _appServiceService.AddDatabaseAsync(
            Arg.Is(appName),
            Arg.Is(resourceGroup),
            Arg.Is(databaseType),
            Arg.Is(databaseServer),
            Arg.Is(databaseName),
            Arg.Is(connectionString ?? Arg.Any<string>()),
            Arg.Is(subscription),
            Arg.Is(tenant ?? Arg.Any<string>()),
            Arg.Any<RetryPolicyOptions>())
            .Returns(expectedConnection);

        var command = new DatabaseAddCommand(_logger);
        var commandArgs = new List<string>
        {
            "--subscription", subscription,
            "--resource-group", resourceGroup,
            "--app", appName,
            "--database-type", databaseType,
            "--database-server", databaseServer,
            "--database", databaseName
        };

        if (!string.IsNullOrEmpty(connectionString))
        {
            commandArgs.AddRange(["--connection-string", connectionString]);
        }

        if (!string.IsNullOrEmpty(tenant))
        {
            commandArgs.AddRange(["--tenant", tenant]);
        }

        var args = command.GetCommand().Parse(commandArgs.ToArray());
        var context = new CommandContext(_serviceProvider);

        // Act
        var response = await command.ExecuteAsync(context, args);

        // Assert
        Assert.NotNull(response);
        if (expectedSuccess)
        {
            Assert.Equal(200, response.Status);
            await _appServiceService.Received(1).AddDatabaseAsync(
                appName,
                resourceGroup,
                databaseType,
                databaseServer,
                databaseName,
                connectionString ?? Arg.Any<string>(),
                subscription,
                tenant ?? Arg.Any<string>(),
                Arg.Any<RetryPolicyOptions>());
        }
        else
        {
            Assert.NotEqual(200, response.Status);
        }
    }

    [Theory]
    [InlineData(new string[] { "--subscription", "sub123", "--resource-group", "rg1" })] // Missing app, database-type, database-server, database
    [InlineData(new string[] { "--subscription", "sub123", "--resource-group", "rg1", "--app", "test-app" })] // Missing database-type, database-server, database
    [InlineData(new string[] { "--subscription", "sub123", "--resource-group", "rg1", "--app", "test-app", "--database-type", "SqlServer" })] // Missing database-server, database
    [InlineData(new string[] { "--subscription", "sub123", "--resource-group", "rg1", "--app", "test-app", "--database-type", "SqlServer", "--database-server", "test-server" })] // Missing database
    [InlineData(new string[] { "--resource-group", "rg1", "--app", "test-app", "--database-type", "SqlServer", "--database-server", "test-server", "--database", "test-db" })] // Missing subscription
    [InlineData(new string[] { "--subscription", "sub123", "--app", "test-app", "--database-type", "SqlServer", "--database-server", "test-server", "--database", "test-db" })] // Missing resource-group
    public async Task ExecuteAsync_MissingRequiredParameter_ReturnsErrorResponse(string[] commandArgs)
    {
        // Arrange
        var command = new DatabaseAddCommand(_logger);
        var args = command.GetCommand().Parse(commandArgs);
        var context = new CommandContext(_serviceProvider);

        // Act
        var response = await command.ExecuteAsync(context, args);

        // Assert
        Assert.NotNull(response);
        Assert.NotEqual(200, response.Status);

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
        Assert.NotEqual(200, response.Status);
    }
}
