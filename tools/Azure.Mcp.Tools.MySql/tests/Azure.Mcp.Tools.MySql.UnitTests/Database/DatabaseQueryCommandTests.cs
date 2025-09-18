// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json;
using System.Text.Json.Serialization;
using Azure.Mcp.Core.Models.Command;
using Azure.Mcp.Tools.MySql.Commands.Database;
using Azure.Mcp.Tools.MySql.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Xunit;

namespace Azure.Mcp.Tools.MySql.UnitTests.Database;

public class DatabaseQueryCommandTests
{
    private readonly IServiceProvider _serviceProvider;
    private readonly IMySqlService _mysqlService;
    private readonly ILogger<DatabaseQueryCommand> _logger;

    public DatabaseQueryCommandTests()
    {
        _mysqlService = Substitute.For<IMySqlService>();
        _logger = Substitute.For<ILogger<DatabaseQueryCommand>>();

        var collection = new ServiceCollection();
        collection.AddSingleton(_mysqlService);

        _serviceProvider = collection.BuildServiceProvider();
    }

    [Fact]
    public async Task ExecuteAsync_ReturnsResults_WhenQuerySucceeds()
    {
        var expectedResults = new List<string> { "id, name", "1, John", "2, Jane" };
        _mysqlService.ExecuteQueryAsync("sub123", "rg1", "user1", "server1", "db1", "SELECT * FROM users").Returns(expectedResults);

        var command = new DatabaseQueryCommand(_logger);
        var args = command.GetCommand().Parse([
            "--subscription", "sub123",
            "--resource-group", "rg1",
            "--user", "user1",
            "--server", "server1",
            "--database", "db1",
            "--query", "SELECT * FROM users"
        ]);
        var context = new CommandContext(_serviceProvider);

        var response = await command.ExecuteAsync(context, args);

        Assert.NotNull(response);
        Assert.Equal(200, response.Status);
        Assert.NotNull(response.Results);

        var json = JsonSerializer.Serialize(response.Results);
        var result = JsonSerializer.Deserialize<DatabaseQueryResult>(json);
        Assert.NotNull(result);
        Assert.Equal(expectedResults, result.Results);
    }

    [Fact]
    public async Task ExecuteAsync_ReturnsError_WhenQueryFails()
    {
        _mysqlService.ExecuteQueryAsync("sub123", "rg1", "user1", "server1", "db1", "INVALID SQL").ThrowsAsync(new InvalidOperationException("Syntax error"));

        var command = new DatabaseQueryCommand(_logger);
        var args = command.GetCommand().Parse([
            "--subscription", "sub123",
            "--resource-group", "rg1",
            "--user", "user1",
            "--server", "server1",
            "--database", "db1",
            "--query", "INVALID SQL"
        ]);
        var context = new CommandContext(_serviceProvider);

        var response = await command.ExecuteAsync(context, args);

        Assert.NotNull(response);
        Assert.Equal(500, response.Status);
        Assert.Contains("Syntax error", response.Message);
    }

    [Fact]
    public void Metadata_IsConfiguredCorrectly()
    {
        var command = new DatabaseQueryCommand(_logger);

        Assert.Equal("query", command.Name);
        Assert.Equal("Executes a safe, read-only SQL SELECT query against a database on Azure Database for MySQL Flexible Server. Use this tool to explore or retrieve table data without modifying it. Rejects non-SELECT statements (INSERT/UPDATE/DELETE/REPLACE/MERGE/TRUNCATE/ALTER/CREATE/DROP), multi-statements, comments hiding writes, transaction control (BEGIN/COMMIT/ROLLBACK), INTO OUTFILE, and other destructive keywords. Only a single SELECT is executed to ensure data integrity. Best practices: List needed columns (avoid SELECT *), add WHERE filters, use LIMIT/OFFSET for paging, ORDER BY for deterministic results, and avoid unnecessary sensitive data. Example: SELECT id, name, status FROM customers WHERE status = 'Active' ORDER BY name LIMIT 50;", command.Description);
        Assert.Equal("Query MySQL Database", command.Title);
        Assert.False(command.Metadata.Destructive);
        Assert.True(command.Metadata.ReadOnly);
    }

    private class DatabaseQueryResult
    {
        [JsonPropertyName("Results")]
        public List<string> Results { get; set; } = [];
    }
}
