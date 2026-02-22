// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net;
using System.Text.Json;
using Azure.Mcp.TestUtilities;
using Azure.Mcp.Tools.Postgres.Commands;
using Azure.Mcp.Tools.Postgres.Options;
using Azure.Mcp.Tools.Postgres.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Mcp.Core.Models.Command;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Xunit;

namespace Azure.Mcp.Tools.Postgres.UnitTests;

public class PostgresListCommandTests
{
    private readonly IServiceProvider _serviceProvider;
    private readonly IPostgresService _postgresService;
    private readonly ILogger<PostgresListCommand> _logger;

    public PostgresListCommandTests()
    {
        _postgresService = Substitute.For<IPostgresService>();
        _logger = Substitute.For<ILogger<PostgresListCommand>>();

        var collection = new ServiceCollection();
        collection.AddSingleton(_postgresService);

        _serviceProvider = collection.BuildServiceProvider();
    }

    [Fact]
    public async Task ExecuteAsync_ListsServers_WhenNoServerOrDatabaseProvided()
    {
        var expectedServers = new List<string> { "postgres-server-1", "postgres-server-2", "postgres-server-3" };
        _postgresService.ListServersAsync("sub123", "rg1", "user1", Arg.Any<CancellationToken>()).Returns(expectedServers);

        var command = new PostgresListCommand(_logger);
        var args = command.GetCommand().Parse([
            "--subscription", "sub123",
            "--resource-group", "rg1",
            "--user", "user1"
        ]);
        var context = new CommandContext(_serviceProvider);

        var response = await command.ExecuteAsync(context, args, TestContext.Current.CancellationToken);

        Assert.NotNull(response);
        Assert.Equal(HttpStatusCode.OK, response.Status);
        Assert.Equal("Success", response.Message);
        Assert.NotNull(response.Results);

        var json = JsonSerializer.Serialize(response.Results);
        var result = JsonSerializer.Deserialize(json, PostgresJsonContext.Default.PostgresListCommandResult);
        Assert.NotNull(result);
        Assert.Equal(expectedServers, result.Servers);
        Assert.Null(result.Databases);
        Assert.Null(result.Tables);
    }

    [Fact]
    public async Task ExecuteAsync_ListsDatabases_WhenServerProvided()
    {
        var expectedDatabases = new List<string> { "db1", "db2", "db3" };
        _postgresService.ListDatabasesAsync(
            "sub123",
            "rg1",
            AuthTypes.MicrosoftEntra,
            "user1",
            null,
            "server1",
            Arg.Any<CancellationToken>()).Returns(expectedDatabases);

        var command = new PostgresListCommand(_logger);
        var args = command.GetCommand().Parse([
            "--subscription", "sub123",
            "--resource-group", "rg1",
            "--user", "user1",
            $"--{PostgresOptionDefinitions.AuthTypeText}", AuthTypes.MicrosoftEntra,
            "--server", "server1"
        ]);
        var context = new CommandContext(_serviceProvider);

        var response = await command.ExecuteAsync(context, args, TestContext.Current.CancellationToken);

        Assert.NotNull(response);
        Assert.Equal(HttpStatusCode.OK, response.Status);
        Assert.Equal("Success", response.Message);
        Assert.NotNull(response.Results);

        var json = JsonSerializer.Serialize(response.Results);
        var result = JsonSerializer.Deserialize(json, PostgresJsonContext.Default.PostgresListCommandResult);
        Assert.NotNull(result);
        Assert.Null(result.Servers);
        Assert.Equal(expectedDatabases, result.Databases);
        Assert.Null(result.Tables);
    }

    [Fact]
    public async Task ExecuteAsync_ListsTables_WhenServerAndDatabaseProvided()
    {
        var expectedTables = new List<string> { "users", "products", "orders" };
        _postgresService.ListTablesAsync(
            "sub123",
            "rg1",
            AuthTypes.MicrosoftEntra,
            "user1",
            null,
            "server1",
            "db1",
            Arg.Any<CancellationToken>()).Returns(expectedTables);

        var command = new PostgresListCommand(_logger);
        var args = command.GetCommand().Parse([
            "--subscription", "sub123",
            "--resource-group", "rg1",
            "--user", "user1",
            $"--{PostgresOptionDefinitions.AuthTypeText}", AuthTypes.MicrosoftEntra,
            "--server", "server1",
            "--database", "db1"
        ]);
        var context = new CommandContext(_serviceProvider);

        var response = await command.ExecuteAsync(context, args, TestContext.Current.CancellationToken);

        Assert.NotNull(response);
        Assert.Equal(HttpStatusCode.OK, response.Status);
        Assert.Equal("Success", response.Message);
        Assert.NotNull(response.Results);

        var json = JsonSerializer.Serialize(response.Results);
        var result = JsonSerializer.Deserialize(json, PostgresJsonContext.Default.PostgresListCommandResult);
        Assert.NotNull(result);
        Assert.Null(result.Servers);
        Assert.Null(result.Databases);
        Assert.Equal(expectedTables, result.Tables);
    }

    [Fact]
    public async Task ExecuteAsync_ReturnsNull_WhenNoServersExist()
    {
        _postgresService.ListServersAsync("sub123", "rg1", "user1", Arg.Any<CancellationToken>()).Returns([]);

        var command = new PostgresListCommand(_logger);
        var args = command.GetCommand().Parse([
            "--subscription", "sub123",
            "--resource-group", "rg1",
            "--user", "user1"
        ]);
        var context = new CommandContext(_serviceProvider);

        var response = await command.ExecuteAsync(context, args, TestContext.Current.CancellationToken);

        Assert.NotNull(response);
        Assert.Equal(HttpStatusCode.OK, response.Status);
        Assert.NotNull(response.Results);

        var json = JsonSerializer.Serialize(response.Results);
        var result = JsonSerializer.Deserialize(json, PostgresJsonContext.Default.PostgresListCommandResult);
        Assert.NotNull(result);
        Assert.NotNull(result.Servers);
        Assert.Empty(result.Servers);
        Assert.Null(result.Databases);
        Assert.Null(result.Tables);
    }

    [Fact]
    public async Task ExecuteAsync_ReturnsNull_WhenNoDatabasesExist()
    {
        _postgresService.ListDatabasesAsync(
            "sub123",
            "rg1",
            AuthTypes.MicrosoftEntra,
            "user1",
            null,
            "server1",
            Arg.Any<CancellationToken>()).Returns([]);

        var command = new PostgresListCommand(_logger);
        var args = command.GetCommand().Parse([
            "--subscription", "sub123",
            "--resource-group", "rg1",
            "--user", "user1",
            $"--{PostgresOptionDefinitions.AuthTypeText}", AuthTypes.MicrosoftEntra,
            "--server", "server1"
        ]);
        var context = new CommandContext(_serviceProvider);

        var response = await command.ExecuteAsync(context, args, TestContext.Current.CancellationToken);

        Assert.NotNull(response);
        Assert.Equal(HttpStatusCode.OK, response.Status);
        Assert.NotNull(response.Results);

        var json = JsonSerializer.Serialize(response.Results);
        var result = JsonSerializer.Deserialize(json, PostgresJsonContext.Default.PostgresListCommandResult);
        Assert.NotNull(result);
        Assert.Null(result.Servers);
        Assert.NotNull(result.Databases);
        Assert.Empty(result.Databases);
        Assert.Null(result.Tables);
    }

    [Fact]
    public async Task ExecuteAsync_ReturnsNull_WhenNoTablesExist()
    {
        _postgresService.ListTablesAsync(
            "sub123",
            "rg1",
            AuthTypes.MicrosoftEntra,
            "user1",
            null,
            "server1",
            "db1",
            Arg.Any<CancellationToken>()).Returns([]);

        var command = new PostgresListCommand(_logger);
        var args = command.GetCommand().Parse([
            "--subscription", "sub123",
            "--resource-group", "rg1",
            "--user", "user1",
            $"--{PostgresOptionDefinitions.AuthTypeText}", AuthTypes.MicrosoftEntra,
            "--server", "server1",
            "--database", "db1"
        ]);
        var context = new CommandContext(_serviceProvider);

        var response = await command.ExecuteAsync(context, args, TestContext.Current.CancellationToken);

        Assert.NotNull(response);
        Assert.Equal(HttpStatusCode.OK, response.Status);
        Assert.NotNull(response.Results);

        var json = JsonSerializer.Serialize(response.Results);
        var result = JsonSerializer.Deserialize(json, PostgresJsonContext.Default.PostgresListCommandResult);
        Assert.NotNull(result);
        Assert.Null(result.Servers);
        Assert.Null(result.Databases);
        Assert.NotNull(result.Tables);
        Assert.Empty(result.Tables);
    }

    [Fact]
    public async Task ExecuteAsync_ReturnsError_WhenListServersThrows()
    {
        var expectedError = "Test error. To mitigate this issue, please refer to the troubleshooting guidelines here at https://aka.ms/azmcp/troubleshooting.";
        _postgresService.ListServersAsync("sub123", "rg1", "user1", Arg.Any<CancellationToken>())
            .ThrowsAsync(new Exception("Test error"));

        var command = new PostgresListCommand(_logger);
        var args = command.GetCommand().Parse([
            "--subscription", "sub123",
            "--resource-group", "rg1",
            "--user", "user1"
        ]);
        var context = new CommandContext(_serviceProvider);

        var response = await command.ExecuteAsync(context, args, TestContext.Current.CancellationToken);

        Assert.NotNull(response);
        Assert.Equal(HttpStatusCode.InternalServerError, response.Status);
        Assert.Equal(expectedError, response.Message);
    }

    [Fact]
    public async Task ExecuteAsync_ReturnsError_WhenListDatabasesThrows()
    {
        var expectedError = "Test error. To mitigate this issue, please refer to the troubleshooting guidelines here at https://aka.ms/azmcp/troubleshooting.";
        _postgresService.ListDatabasesAsync(
            "sub123",
            "rg1",
            AuthTypes.MicrosoftEntra,
            "user1",
            null,
            "server1",
            Arg.Any<CancellationToken>())
            .ThrowsAsync(new Exception("Test error"));

        var command = new PostgresListCommand(_logger);
        var args = command.GetCommand().Parse([
            "--subscription", "sub123",
            "--resource-group", "rg1",
            "--user", "user1",
            $"--{PostgresOptionDefinitions.AuthTypeText}", AuthTypes.MicrosoftEntra,
            "--server", "server1"
        ]);
        var context = new CommandContext(_serviceProvider);

        var response = await command.ExecuteAsync(context, args, TestContext.Current.CancellationToken);

        Assert.NotNull(response);
        Assert.Equal(HttpStatusCode.InternalServerError, response.Status);
        Assert.Equal(expectedError, response.Message);
    }

    [Fact]
    public async Task ExecuteAsync_ReturnsError_WhenListTablesThrows()
    {
        var expectedError = "Test error. To mitigate this issue, please refer to the troubleshooting guidelines here at https://aka.ms/azmcp/troubleshooting.";
        _postgresService.ListTablesAsync(
            "sub123",
            "rg1",
            AuthTypes.MicrosoftEntra,
            "user1",
            null,
            "server1",
            "db1",
            Arg.Any<CancellationToken>())
            .ThrowsAsync(new Exception("Test error"));

        var command = new PostgresListCommand(_logger);
        var args = command.GetCommand().Parse([
            "--subscription", "sub123",
            "--resource-group", "rg1",
            "--user", "user1",
            $"--{PostgresOptionDefinitions.AuthTypeText}", AuthTypes.MicrosoftEntra,
            "--server", "server1",
            "--database", "db1"
        ]);
        var context = new CommandContext(_serviceProvider);

        var response = await command.ExecuteAsync(context, args, TestContext.Current.CancellationToken);

        Assert.NotNull(response);
        Assert.Equal(HttpStatusCode.InternalServerError, response.Status);
        Assert.Equal(expectedError, response.Message);
    }

    [Theory]
    [InlineData("--subscription")]
    [InlineData("--resource-group")]
    [InlineData("--user")]
    public async Task ExecuteAsync_ReturnsError_WhenRequiredParameterIsMissing(string missingParameter)
    {
        var command = new PostgresListCommand(_logger);
        var args = command.GetCommand().Parse(ArgBuilder.BuildArgs(missingParameter,
            ("--subscription", "sub123"),
            ("--resource-group", "rg1"),
            ("--user", "user1")
        ));

        var context = new CommandContext(_serviceProvider);
        var response = await command.ExecuteAsync(context, args, TestContext.Current.CancellationToken);

        Assert.NotNull(response);
        Assert.Equal(HttpStatusCode.BadRequest, response.Status);
        Assert.Equal($"Missing Required options: {missingParameter}", response.Message);
    }

    [Fact]
    public void Metadata_IsConfiguredCorrectly()
    {
        var command = new PostgresListCommand(_logger);

        Assert.False(command.Metadata.Destructive);
        Assert.True(command.Metadata.ReadOnly);
    }

    [Fact]
    public void Name_IsCorrect()
    {
        var command = new PostgresListCommand(_logger);
        Assert.Equal("list", command.Name);
    }

    [Fact]
    public void Description_IsCorrect()
    {
        var command = new PostgresListCommand(_logger);
        Assert.Contains("List PostgreSQL servers", command.Description);
        Assert.Contains("databases, or tables", command.Description);
    }
}
