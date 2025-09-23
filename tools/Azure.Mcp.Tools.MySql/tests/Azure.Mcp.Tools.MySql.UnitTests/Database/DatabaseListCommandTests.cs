// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json;
using Azure.Mcp.Core.Models.Command;
using Azure.Mcp.Tools.MySql.Commands;
using Azure.Mcp.Tools.MySql.Commands.Database;
using Azure.Mcp.Tools.MySql.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Xunit;

namespace Azure.Mcp.Tools.MySql.UnitTests.Database;

public class DatabaseListCommandTests
{
    private readonly IServiceProvider _serviceProvider;
    private readonly IMySqlService _mysqlService;
    private readonly ILogger<DatabaseListCommand> _logger;

    public DatabaseListCommandTests()
    {
        _mysqlService = Substitute.For<IMySqlService>();
        _logger = Substitute.For<ILogger<DatabaseListCommand>>();

        var collection = new ServiceCollection();
        collection.AddSingleton(_mysqlService);

        _serviceProvider = collection.BuildServiceProvider();
    }

    [Fact]
    public async Task ExecuteAsync_ReturnsDatabases_WhenSuccessful()
    {
        var expectedDatabases = new List<string> { "db1", "db2", "db3" };
        _mysqlService.ListDatabasesAsync("sub123", "rg1", "user1", "server1").Returns(expectedDatabases);

        var command = new DatabaseListCommand(_logger);
        var args = command.GetCommand().Parse([
            "--subscription", "sub123",
            "--resource-group", "rg1",
            "--user", "user1",
            "--server", "server1"
        ]);
        var context = new CommandContext(_serviceProvider);

        var response = await command.ExecuteAsync(context, args);

        Assert.NotNull(response);
        Assert.Equal(200, response.Status);
        Assert.Equal("Success", response.Message);
        Assert.NotNull(response.Results);

        var json = JsonSerializer.Serialize(response.Results);
        var result = JsonSerializer.Deserialize(json, MySqlJsonContext.Default.DatabaseListCommandResult);
        Assert.NotNull(result);
        Assert.Equal(expectedDatabases, result.Databases);
    }

    [Fact]
    public async Task ExecuteAsync_ReturnsEmpty_WhenNoDatabasesExist()
    {
        _mysqlService.ListDatabasesAsync("sub123", "rg1", "user1", "server1").Returns([]);

        var command = new DatabaseListCommand(_logger);
        var args = command.GetCommand().Parse([
            "--subscription", "sub123",
            "--resource-group", "rg1",
            "--user", "user1",
            "--server", "server1"
        ]);
        var context = new CommandContext(_serviceProvider);

        var response = await command.ExecuteAsync(context, args);

        Assert.NotNull(response);
        Assert.Equal(200, response.Status);
        Assert.NotNull(response.Results);

        var json = JsonSerializer.Serialize(response.Results);
        var result = JsonSerializer.Deserialize(json, MySqlJsonContext.Default.DatabaseListCommandResult);
        Assert.NotNull(result);
        Assert.Empty(result.Databases);
    }

    [Fact]
    public async Task ExecuteAsync_ReturnsError_WhenServiceThrows()
    {
        _mysqlService.ListDatabasesAsync("sub123", "rg1", "user1", "server1")
            .ThrowsAsync(new UnauthorizedAccessException("Access denied"));

        var command = new DatabaseListCommand(_logger);
        var args = command.GetCommand().Parse([
            "--subscription", "sub123",
            "--resource-group", "rg1",
            "--user", "user1",
            "--server", "server1"
        ]);
        var context = new CommandContext(_serviceProvider);

        var response = await command.ExecuteAsync(context, args);

        Assert.NotNull(response);
        Assert.Equal(500, response.Status);
        Assert.Contains("Access denied", response.Message);
    }

    [Fact]
    public void Metadata_IsConfiguredCorrectly()
    {
        var command = new DatabaseListCommand(_logger);

        Assert.Equal("list", command.Name);
        Assert.Equal("Retrieves a comprehensive list of all databases available on the specified Azure Database for MySQL Flexible Server instance. This command provides visibility into the database structure and helps identify available databases for connection and querying operations.", command.Description);
        Assert.Equal("List MySQL Databases", command.Title);
        Assert.False(command.Metadata.Destructive);
        Assert.True(command.Metadata.ReadOnly);
    }
}
