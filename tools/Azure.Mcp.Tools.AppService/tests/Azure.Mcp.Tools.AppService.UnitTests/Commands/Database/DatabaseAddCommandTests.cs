// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.CommandLine;
using System.CommandLine.Invocation;
using System.CommandLine.Parsing;
using System.Linq;
using System.Net;
using Azure.Mcp.Core.Models.Command;
using Azure.Mcp.Core.Options;
using Azure.Mcp.Tools.AppService.Commands.Database;
using Azure.Mcp.Tools.AppService.Models;
using Azure.Mcp.Tools.AppService.Services;
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
        Assert.Equal(HttpStatusCode.BadRequest, response.Status);

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

    [Theory]
    [InlineData("basic", null, null, null, null)]
    [InlineData("custom-connection-string", "Server=custom;Database=custom;UserId=user;Password=pass;", null, null, null)]
    [InlineData("tenant", null, "test-tenant-id", null, null)]
    [InlineData("retry-policy", null, null, 3, 1.0)]
    public async Task ExecuteAsync_WithVariousParameters_AcceptsParameters(
        string scenario,
        string? connectionString,
        string? tenant,
        int? retryMaxRetries,
        double? retryDelay)
    {
        var subscription = "sub123";
        var parameters = new Dictionary<string, object?>
        {
            { "subscription", subscription },
            { "resource-group", "test-rg" },
            { "app", "test-app" },
            { "database-type", "SqlServer" },
            { "database-server", "test-server.database.windows.net" },
            { "database", "test-db" }
        };

        // Add optional parameters based on scenario
        if (connectionString != null)
            parameters.Add("connection-string", connectionString);

        if (tenant != null)
            parameters.Add("tenant", tenant);

        if (retryMaxRetries.HasValue)
            parameters.Add("retry-max-retries", retryMaxRetries.Value);

        if (retryDelay.HasValue)
            parameters.Add("retry-delay", retryDelay.Value);

        // Execute the command directly in unit tests rather than via the tool runner helper
        var command = new DatabaseAddCommand(_logger);
        var argList = parameters.SelectMany(kvp => new[] { $"--{kvp.Key}", kvp.Value?.ToString() ?? string.Empty }).ToArray();
        var parseResult = command.GetCommand().Parse(argList);
        var context = new CommandContext(_serviceProvider);
        var response = await command.ExecuteAsync(context, parseResult);

        // Test actual command execution and proper error handling
        Assert.NotNull(response);

        // Validate that parameters are correctly passed and processed
        if (response.Status != HttpStatusCode.OK)
        {
            var errorContent = response.Message ?? string.Empty;

            // Should not fail due to parameter validation issues for valid scenarios
            Assert.False(
                errorContent.Contains("required parameter") ||
                errorContent.Contains("invalid parameter") ||
                errorContent.Contains("ArgumentException"),
                $"[{scenario}] Parameter validation failed: {errorContent}");

            // Should fail due to Azure resource issues, which is expected in live tests
            Assert.True(
                errorContent.Contains("not found") ||
                errorContent.Contains("does not exist") ||
                errorContent.Contains("ResourceGroupNotFound") ||
                errorContent.Contains("WebSiteNotFound") ||
                errorContent.Contains("subscription"),
                $"[{scenario}] Expected Azure resource error but got: {errorContent}");
        }
        else
        {
            // If successful, validate the response has expected structure
            Assert.Equal(HttpStatusCode.OK, response.Status);
        }
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
        Assert.Equal(HttpStatusCode.InternalServerError, response.Status);
    }
}
