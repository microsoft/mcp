// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.CommandLine;
using System.CommandLine.Parsing;
using System.Text.Json;
using System.Text.Json.Serialization;
using Azure.Mcp.Core.Models.Command;
using Azure.Mcp.Tools.Postgres.Commands.Table;
using Azure.Mcp.Tools.Postgres.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Xunit;

namespace Azure.Mcp.Tools.Postgres.UnitTests.Table;

public class TableListCommandTests
{
    private readonly IServiceProvider _serviceProvider;
    private readonly IPostgresService _postgresService;
    private readonly ILogger<TableListCommand> _logger;

    public TableListCommandTests()
    {
        _postgresService = Substitute.For<IPostgresService>();
        _logger = Substitute.For<ILogger<TableListCommand>>();

        var collection = new ServiceCollection();
        collection.AddSingleton(_postgresService);

        _serviceProvider = collection.BuildServiceProvider();
    }

    [Fact]
    public async Task ExecuteAsync_ReturnsTables_WhenTablesExist()
    {
        var expectedTables = new List<string> { "table1", "table2" };
        _postgresService.ListTablesAsync("sub123", "rg1", "user1", "server1", "db123").Returns(expectedTables);

        var command = new TableListCommand(_logger);
        var args = command.GetCommand().Parse(["--subscription", "sub123", "--resource-group", "rg1", "--user", "user1", "--server", "server1", "--database", "db123"]);
        var context = new CommandContext(_serviceProvider);
        var response = await command.ExecuteAsync(context, args);

        Assert.NotNull(response);
        Assert.Equal(200, response.Status);
        Assert.Equal("Success", response.Message);
        Assert.NotNull(response.Results);

        var json = JsonSerializer.Serialize(response.Results);
        var result = JsonSerializer.Deserialize<TableListResult>(json);
        Assert.NotNull(result);
        Assert.Equal(expectedTables, result.Tables);
    }

    [Fact]
    public async Task ExecuteAsync_ReturnsEmptyList_WhenNoTablesExist()
    {
        _postgresService.ListTablesAsync("sub123", "rg1", "user1", "server1", "db123").Returns([]);

        var command = new TableListCommand(_logger);
        var args = command.GetCommand().Parse(["--subscription", "sub123", "--resource-group", "rg1", "--user", "user1", "--server", "server1", "--database", "db123"]);
        var context = new CommandContext(_serviceProvider);
        var response = await command.ExecuteAsync(context, args);

        Assert.NotNull(response);
        Assert.Equal(200, response.Status);
        Assert.Equal("Success", response.Message);
        Assert.Null(response.Results);
    }

    [Theory]
    [InlineData("--subscription")]
    [InlineData("--resource-group")]
    [InlineData("--user")]
    [InlineData("--server")]
    [InlineData("--database")]
    public async Task ExecuteAsync_ReturnsError_WhenParameterIsMissing(string missingParameter)
    {
        var command = new TableListCommand(_logger);
        var args = command.GetCommand().Parse(new string[]
        {
            missingParameter == "--subscription" ? "" : "--subscription", "sub123",
            missingParameter == "--resource-group" ? "" : "--resource-group", "rg1",
            missingParameter == "--user" ? "" : "--user", "user1",
            missingParameter == "--server" ? "" : "--server", "server123",
            missingParameter == "--database" ? "" : "--database", "db123"
        });

        var context = new CommandContext(_serviceProvider);
        var response = await command.ExecuteAsync(context, args);

        Assert.NotNull(response);
        Assert.Equal(400, response.Status);
        Assert.Equal($"Missing Required options: {missingParameter}", response.Message);
    }

    private class TableListResult
    {
        [JsonPropertyName("Tables")]
        public List<string> Tables { get; set; } = [];
    }
}
