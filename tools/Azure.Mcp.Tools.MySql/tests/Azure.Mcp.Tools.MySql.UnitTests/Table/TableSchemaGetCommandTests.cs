// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json;
using Azure.Mcp.Core.Models.Command;
using Azure.Mcp.Tools.MySql.Commands;
using Azure.Mcp.Tools.MySql.Commands.Table;
using Azure.Mcp.Tools.MySql.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Xunit;

namespace Azure.Mcp.Tools.MySql.UnitTests.Table;

public class TableSchemaGetCommandTests
{
    private readonly IServiceProvider _serviceProvider;
    private readonly IMySqlService _mysqlService;
    private readonly ILogger<TableSchemaGetCommand> _logger;

    public TableSchemaGetCommandTests()
    {
        _mysqlService = Substitute.For<IMySqlService>();
        _logger = Substitute.For<ILogger<TableSchemaGetCommand>>();

        var collection = new ServiceCollection();
        collection.AddSingleton(_mysqlService);

        _serviceProvider = collection.BuildServiceProvider();
    }

    [Fact]
    public async Task ExecuteAsync_ReturnsSchema_WhenSuccessful()
    {
        var expectedSchema = new List<string> { "id INT PRIMARY KEY", "name VARCHAR(100) NOT NULL", "email VARCHAR(255)" };
        _mysqlService.GetTableSchemaAsync("sub123", "rg1", "user1", "server1", "db1", "users").Returns(expectedSchema);

        var command = new TableSchemaGetCommand(_logger);
        var args = command.GetCommand().Parse([
            "--subscription", "sub123",
            "--resource-group", "rg1",
            "--user", "user1",
            "--server", "server1",
            "--database", "db1",
            "--table", "users"
        ]);
        var context = new CommandContext(_serviceProvider);

        var response = await command.ExecuteAsync(context, args);

        Assert.NotNull(response);
        Assert.Equal(200, response.Status);
        Assert.NotNull(response.Results);

        var json = JsonSerializer.Serialize(response.Results);
        var result = JsonSerializer.Deserialize(json, MySqlJsonContext.Default.TableSchemaGetCommandResult);
        Assert.NotNull(result);
        Assert.Equal(expectedSchema, result.Schema);
    }

    [Fact]
    public async Task ExecuteAsync_ReturnsError_WhenTableNotFound()
    {
        _mysqlService.GetTableSchemaAsync("sub123", "rg1", "user1", "server1", "db1", "nonexistent").ThrowsAsync(new ArgumentException("Table not found"));

        var command = new TableSchemaGetCommand(_logger);
        var args = command.GetCommand().Parse([
            "--subscription", "sub123",
            "--resource-group", "rg1",
            "--user", "user1",
            "--server", "server1",
            "--database", "db1",
            "--table", "nonexistent"
        ]);
        var context = new CommandContext(_serviceProvider);

        var response = await command.ExecuteAsync(context, args);

        Assert.NotNull(response);
        Assert.Equal(500, response.Status);
        Assert.Contains("Table not found", response.Message);
    }

    [Fact]
    public void Metadata_IsConfiguredCorrectly()
    {
        var command = new TableSchemaGetCommand(_logger);

        Assert.Equal("schema", command.Name);
        Assert.Equal("Retrieves detailed schema information for a specific table within an Azure Database for MySQL Flexible Server database. This command provides comprehensive metadata including column definitions, data types, constraints, indexes, and relationships, essential for understanding table structure and supporting application development.", command.Description);
        Assert.Equal("Get MySQL Table Schema", command.Title);
        Assert.False(command.Metadata.Destructive);
        Assert.True(command.Metadata.ReadOnly);
    }
}
