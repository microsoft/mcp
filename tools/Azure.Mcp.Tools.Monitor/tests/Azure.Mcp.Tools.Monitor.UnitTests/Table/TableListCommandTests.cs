// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.CommandLine;
using System.Text.Json;
using Azure.Mcp.Core.Models.Command;
using Azure.Mcp.Core.Options;
using Azure.Mcp.Tools.Monitor.Commands;
using Azure.Mcp.Tools.Monitor.Commands.Table;
using Azure.Mcp.Tools.Monitor.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Xunit;

namespace Azure.Mcp.Tools.Monitor.UnitTests.Table;

[Trait("Area", "Monitor")]
public sealed class TableListCommandTests
{
    private readonly IServiceProvider _serviceProvider;
    private readonly IMonitorService _monitorService;
    private readonly ILogger<TableListCommand> _logger;
    private readonly TableListCommand _command;
    private readonly CommandContext _context;
    private readonly Command _commandDefinition;

    private const string _knownSubscription = "knownSubscription";
    private const string _knownWorkspace = "knownWorkspace";
    private const string _knownResourceGroupName = "knownResourceGroup";
    private const string _knownTableType = "CustomLog";

    public TableListCommandTests()
    {
        _monitorService = Substitute.For<IMonitorService>();
        _logger = Substitute.For<ILogger<TableListCommand>>();

        var collection = new ServiceCollection();
        collection.AddSingleton(_monitorService);
        _serviceProvider = collection.BuildServiceProvider();

        _command = new(_logger);
        _context = new(_serviceProvider);
        _commandDefinition = _command.GetCommand();
    }

    [Theory]
    [InlineData($"--subscription {_knownSubscription} --workspace {_knownWorkspace} --table-type {_knownTableType} --resource-group {_knownResourceGroupName}", true)]
    [InlineData($"--subscription {_knownSubscription} --workspace {_knownWorkspace} --resource-group {_knownResourceGroupName}", true)]
    [InlineData($"--subscription {_knownSubscription}", false)]
    [InlineData("", false)]
    public async Task ExecuteAsync_ValidatesInputCorrectly(string args, bool shouldSucceed)
    {
        // Arrange
        if (shouldSucceed)
        {
            var testTables = new List<string>
            {
                "AppEvents",
                "AppRequests",
                "AppDependencies"
            };
            _monitorService.ListTables(
                Arg.Any<string>(),
                Arg.Any<string>(),
                Arg.Any<string>(),
                Arg.Any<string>(),
                Arg.Any<string>(),
                Arg.Any<RetryPolicyOptions>())
                .Returns(testTables);
        }

        // Act
        var response = await _command.ExecuteAsync(_context, _commandDefinition.Parse(args));

        // Assert
        Assert.Equal(shouldSucceed ? 200 : 400, response.Status);
        if (shouldSucceed)
        {
            Assert.NotNull(response.Results);
            Assert.Equal("Success", response.Message);
        }
        else
        {
            Assert.Contains("required", response.Message.ToLower());
        }
    }

    [Fact]
    public async Task ExecuteAsync_ReturnsTablesList()
    {
        // Arrange
        var expectedTables = new List<string>
        {
            "AppEvents",
            "AppRequests",
            "AppDependencies",
            "AppMetrics"
        };
        _monitorService.ListTables(
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<RetryPolicyOptions>())
            .Returns(expectedTables);

        var args = _commandDefinition.Parse([
            "--subscription", _knownSubscription,
            "--workspace", _knownWorkspace,
            "--resource-group", _knownResourceGroupName
        ]);

        // Act
        var response = await _command.ExecuteAsync(_context, args);

        // Assert
        Assert.Equal(200, response.Status);
        Assert.NotNull(response.Results);

        // Verify the mock was called
        await _monitorService.Received(1).ListTables(
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<RetryPolicyOptions>());

        var json = JsonSerializer.Serialize(response.Results);
        var result = JsonSerializer.Deserialize(json, MonitorJsonContext.Default.TableListCommandResult);

        Assert.NotNull(result);
        Assert.Equal(expectedTables.Count, result.Tables.Count);
        Assert.Equal(expectedTables[0], result.Tables[0]);
        Assert.Equal(expectedTables[1], result.Tables[1]);
        Assert.Equal(expectedTables[2], result.Tables[2]);
        Assert.Equal(expectedTables[3], result.Tables[3]);
    }

    [Fact]
    public async Task ExecuteAsync_WithTableTypeParameter_CallsServiceWithCorrectParameters()
    {
        // Arrange
        var expectedTables = new List<string> { "CustomTable1", "CustomTable2" };
        _monitorService.ListTables(
            _knownSubscription,
            _knownResourceGroupName,
            _knownWorkspace,
            _knownTableType,
            Arg.Any<string>(),
            Arg.Any<RetryPolicyOptions>())
            .Returns(expectedTables);

        var args = _commandDefinition.Parse([
            "--subscription", _knownSubscription,
            "--workspace", _knownWorkspace,
            "--resource-group", _knownResourceGroupName,
            "--table-type", _knownTableType
        ]);

        // Act
        var response = await _command.ExecuteAsync(_context, args);

        // Assert
        Assert.Equal(200, response.Status);
        await _monitorService.Received(1).ListTables(
            _knownSubscription,
            _knownResourceGroupName,
            _knownWorkspace,
            _knownTableType,
            Arg.Any<string>(),
            Arg.Any<RetryPolicyOptions>());
    }

    [Fact]
    public async Task ExecuteAsync_ReturnsEmptyWhenNoTables()
    {
        // Arrange
        _monitorService.ListTables(
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<RetryPolicyOptions>())
            .Returns([]);

        var args = _commandDefinition.Parse([
            "--subscription", _knownSubscription,
            "--workspace", _knownWorkspace,
            "--resource-group", _knownResourceGroupName
        ]);

        // Act
        var response = await _command.ExecuteAsync(_context, args);

        // Assert
        Assert.Equal(200, response.Status);
        Assert.NotNull(response.Results);

        var json = JsonSerializer.Serialize(response.Results);
        var result = JsonSerializer.Deserialize(json, MonitorJsonContext.Default.TableListCommandResult);

        Assert.NotNull(result);
        Assert.Empty(result.Tables);
    }

    [Fact]
    public async Task ExecuteAsync_HandlesServiceErrors()
    {
        // Arrange
        _monitorService.ListTables(
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<RetryPolicyOptions>())
            .Returns(Task.FromException<List<string>>(new Exception("Test error")));

        var args = _commandDefinition.Parse([
            "--subscription", _knownSubscription,
            "--workspace", _knownWorkspace,
            "--resource-group", _knownResourceGroupName
        ]);

        // Act
        var response = await _command.ExecuteAsync(_context, args);

        // Assert
        Assert.Equal(500, response.Status);
        Assert.Contains("Test error", response.Message);
        Assert.Contains("troubleshooting", response.Message);
    }
}
