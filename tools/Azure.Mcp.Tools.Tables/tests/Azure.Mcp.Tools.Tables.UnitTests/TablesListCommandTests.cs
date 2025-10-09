// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.CommandLine;
using System.Net;
using System.Text.Json;
using Azure.Mcp.Core.Models.Command;
using Azure.Mcp.Core.Options;
using Azure.Mcp.Tools.Tables.Commands;
using Azure.Mcp.Tools.Tables.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Xunit;

namespace Azure.Mcp.Tools.Tables.UnitTests;

public class TablesListCommandTests
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ITablesService _tablesService;
    private readonly ILogger<TablesListCommand> _logger;
    private readonly TablesListCommand _command;
    private readonly CommandContext _context;
    private readonly Command _commandDefinition;
    private readonly string _knownAccount = "account123";
    private readonly string _knownSubscription = "sub123";

    public TablesListCommandTests()
    {
        _tablesService = Substitute.For<ITablesService>();
        _logger = Substitute.For<ILogger<TablesListCommand>>();

        var collection = new ServiceCollection().AddSingleton(_tablesService);

        _serviceProvider = collection.BuildServiceProvider();
        _command = new(_logger);
        _context = new(_serviceProvider);
        _commandDefinition = _command.GetCommand();
    }

    [Fact]
    public async Task ExecuteAsync_ReturnsTables()
    {
        // Arrange
        var expectedTables = new List<string> { "table1", "table2" };

        _tablesService.ListTables(
            Arg.Is(_knownAccount),
            Arg.Is(_knownSubscription),
            Arg.Any<string?>(),
            Arg.Any<RetryPolicyOptions>())
            .Returns(expectedTables);

        var args = _commandDefinition.Parse([
            "--account", _knownAccount,
            "--subscription", _knownSubscription
        ]);

        // Act
        var response = await _command.ExecuteAsync(_context, args);

        // Assert
        Assert.NotNull(response);
        Assert.NotNull(response.Results);

        var json = JsonSerializer.Serialize(response.Results);
        var result = JsonSerializer.Deserialize(json, TablesJsonContext.Default.TablesListCommandResult);

        Assert.NotNull(result);
        Assert.Equal(expectedTables, result.Tables);
    }

    [Fact]
    public async Task ExecuteAsync_ReturnsEmpty_WhenNoTables()
    {
        // Arrange
        _tablesService.ListTables(
            Arg.Is(_knownAccount),
            Arg.Is(_knownSubscription),
            Arg.Any<string?>(),
            Arg.Any<RetryPolicyOptions>())
            .Returns([]);

        var args = _commandDefinition.Parse([
            "--account", _knownAccount,
            "--subscription", _knownSubscription
        ]);

        // Act
        var response = await _command.ExecuteAsync(_context, args);

        // Assert
        Assert.NotNull(response);
        Assert.NotNull(response.Results);

        var json = JsonSerializer.Serialize(response.Results);
        var result = JsonSerializer.Deserialize(json, TablesJsonContext.Default.TablesListCommandResult);

        Assert.NotNull(result);
        Assert.Empty(result.Tables);
    }

    [Theory]
    [InlineData("--subscription sub123")] // Missing account
    [InlineData("--account mystorageaccount")] // Missing subscription
    public async Task ExecuteAsync_ValidatesInputCorrectly(string args)
    {
        // Arrange
        var parseResult = _commandDefinition.Parse(args);

        // Act
        var response = await _command.ExecuteAsync(_context, parseResult);

        // Assert
        Assert.Contains("required", response.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task ExecuteAsync_HandlesException()
    {
        // Arrange
        var expectedError = "Test error";

        _tablesService.ListTables(
            Arg.Is(_knownAccount),
            Arg.Is(_knownSubscription),
            Arg.Any<string?>(),
            Arg.Any<RetryPolicyOptions>())
            .ThrowsAsync(new Exception(expectedError));

        var args = _command.GetCommand().Parse([
            "--account", _knownAccount,
            "--subscription", _knownSubscription
        ]);

        // Act
        var response = await _command.ExecuteAsync(_context, args);

        // Assert
        Assert.NotNull(response);
        Assert.Equal(HttpStatusCode.InternalServerError, response.Status);
        Assert.StartsWith(expectedError, response.Message);
    }
}
