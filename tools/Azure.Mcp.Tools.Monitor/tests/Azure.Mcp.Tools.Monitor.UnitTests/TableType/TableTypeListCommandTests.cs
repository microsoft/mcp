// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.CommandLine;
using System.Net;
using System.Text.Json;
using Azure.Mcp.Core.Models.Command;
using Azure.Mcp.Core.Options;
using Azure.Mcp.Tools.Monitor.Commands;
using Azure.Mcp.Tools.Monitor.Commands.TableType;
using Azure.Mcp.Tools.Monitor.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Xunit;

namespace Azure.Mcp.Tools.Monitor.UnitTests.TableType;

[Trait("Area", "Monitor")]
public sealed class TableTypeListCommandTests
{
    private readonly IServiceProvider _serviceProvider;
    private readonly IMonitorService _monitorService;
    private readonly ILogger<TableTypeListCommand> _logger;
    private readonly TableTypeListCommand _command;
    private readonly CommandContext _context;
    private readonly Command _commandDefinition;

    private const string _knownSubscription = "knownSubscription";
    private const string _knownWorkspace = "knownWorkspace";
    private const string _knownResourceGroup = "knownResourceGroup";

    public TableTypeListCommandTests()
    {
        _monitorService = Substitute.For<IMonitorService>();
        _logger = Substitute.For<ILogger<TableTypeListCommand>>();

        var collection = new ServiceCollection();
        collection.AddSingleton(_monitorService);
        _serviceProvider = collection.BuildServiceProvider();

        _command = new(_logger);
        _context = new(_serviceProvider);
        _commandDefinition = _command.GetCommand();
    }

    [Theory]
    [InlineData($"--subscription {_knownSubscription} --workspace {_knownWorkspace} --resource-group {_knownResourceGroup}", true)]
    [InlineData($"--subscription {_knownSubscription}", false)]
    [InlineData("", false)]
    public async Task ExecuteAsync_ValidatesInputCorrectly(string args, bool shouldSucceed)
    {
        // Arrange
        if (shouldSucceed)
        {
            var testTableTypes = new List<string>
            {
                "CustomLog",
                "AzureMetrics",
                "SystemEvents"
            };
            _monitorService.ListTableTypes(
                Arg.Any<string>(),
                Arg.Any<string>(),
                Arg.Any<string>(),
                Arg.Any<string>(),
                Arg.Any<RetryPolicyOptions>())
                .Returns(testTableTypes);
        }

        // Act
        var response = await _command.ExecuteAsync(_context, _commandDefinition.Parse(args));

        // Assert
        Assert.Equal(shouldSucceed ? HttpStatusCode.OK : HttpStatusCode.BadRequest, response.Status);
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
    public async Task ExecuteAsync_ReturnsTableTypesList()
    {
        // Arrange
        var expectedTableTypes = new List<string>
        {
            "CustomLog",
            "AzureMetrics",
            "SystemEvents",
            "ApplicationEvents"
        };
        _monitorService.ListTableTypes(
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<RetryPolicyOptions>())
            .Returns(expectedTableTypes);

        var args = _commandDefinition.Parse([
            "--subscription", _knownSubscription,
            "--workspace", _knownWorkspace,
            "--resource-group", _knownResourceGroup
        ]);

        // Act
        var response = await _command.ExecuteAsync(_context, args);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.Status);
        Assert.NotNull(response.Results);

        // Verify the mock was called
        await _monitorService.Received(1).ListTableTypes(
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<RetryPolicyOptions>());

        var json = JsonSerializer.Serialize(response.Results);
        var result = JsonSerializer.Deserialize(json, MonitorJsonContext.Default.TableTypeListCommandResult);

        Assert.NotNull(result);
        Assert.Equal(expectedTableTypes.Count, result.TableTypes.Count);
        Assert.Equal(expectedTableTypes[0], result.TableTypes[0]);
        Assert.Equal(expectedTableTypes[1], result.TableTypes[1]);
        Assert.Equal(expectedTableTypes[2], result.TableTypes[2]);
        Assert.Equal(expectedTableTypes[3], result.TableTypes[3]);
    }

    [Fact]
    public async Task ExecuteAsync_CallsServiceWithCorrectParameters()
    {
        // Arrange
        var expectedTableTypes = new List<string> { "CustomLog", "AzureMetrics" };
        _monitorService.ListTableTypes(
            _knownSubscription,
            _knownResourceGroup,
            _knownWorkspace,
            Arg.Any<string>(),
            Arg.Any<RetryPolicyOptions>())
            .Returns(expectedTableTypes);

        var args = _commandDefinition.Parse([
            "--subscription", _knownSubscription,
            "--workspace", _knownWorkspace,
            "--resource-group", _knownResourceGroup
        ]);

        // Act
        var response = await _command.ExecuteAsync(_context, args);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.Status);
        await _monitorService.Received(1).ListTableTypes(
            _knownSubscription,
            _knownResourceGroup,
            _knownWorkspace,
            Arg.Any<string>(),
            Arg.Any<RetryPolicyOptions>());
    }

    [Fact]
    public async Task ExecuteAsync_ReturnsEmptyWhenNoTableTypes()
    {
        // Arrange
        _monitorService.ListTableTypes(
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<RetryPolicyOptions>())
            .Returns([]);

        var args = _commandDefinition.Parse([
            "--subscription", _knownSubscription,
            "--workspace", _knownWorkspace,
            "--resource-group", _knownResourceGroup
        ]);

        // Act
        var response = await _command.ExecuteAsync(_context, args);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.Status);
        Assert.NotNull(response.Results);

        var json = JsonSerializer.Serialize(response.Results);
        var result = JsonSerializer.Deserialize(json, MonitorJsonContext.Default.TableTypeListCommandResult);

        Assert.NotNull(result);
        Assert.Empty(result.TableTypes);
    }

    [Fact]
    public async Task ExecuteAsync_HandlesServiceErrors()
    {
        // Arrange
        _monitorService.ListTableTypes(
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<RetryPolicyOptions>())
            .Returns(Task.FromException<List<string>>(new Exception("Test error")));

        var args = _commandDefinition.Parse([
            "--subscription", _knownSubscription,
            "--workspace", _knownWorkspace,
            "--resource-group", _knownResourceGroup
        ]);

        // Act
        var response = await _command.ExecuteAsync(_context, args);

        // Assert
        Assert.Equal(HttpStatusCode.InternalServerError, response.Status);
        Assert.Contains("Test error", response.Message);
        Assert.Contains("troubleshooting", response.Message);
    }
}
