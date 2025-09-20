// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.CommandLine;
using System.Net;
using System.Text.Json.Nodes;
using Azure.Mcp.Core.Models.Command;
using Azure.Mcp.Core.Options;
using Azure.Mcp.Tools.Monitor.Commands.Log;
using Azure.Mcp.Tools.Monitor.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Xunit;

namespace Azure.Mcp.Tools.Monitor.UnitTests.Log;

[Trait("Area", "Monitor")]
public sealed class WorkspaceLogQueryCommandTests
{
    private readonly IServiceProvider _serviceProvider;
    private readonly IMonitorService _monitorService;
    private readonly ILogger<WorkspaceLogQueryCommand> _logger;
    private readonly WorkspaceLogQueryCommand _command;
    private readonly CommandContext _context;
    private readonly Command _commandDefinition;

    private const string _knownSubscription = "knownSubscription";
    private const string _knownWorkspace = "knownWorkspace";
    private const string _knownResourceGroup = "knownResourceGroup";
    private const string _knownTable = "knownTable";
    private const string _knownTenant = "knownTenant";
    private const string _knownHours = "24";
    private const string _knownLimit = "100";
    private const string _knownQuery = "| limit 10";

    public WorkspaceLogQueryCommandTests()
    {
        _monitorService = Substitute.For<IMonitorService>();
        _logger = Substitute.For<ILogger<WorkspaceLogQueryCommand>>();

        var collection = new ServiceCollection();
        collection.AddSingleton(_monitorService);
        _serviceProvider = collection.BuildServiceProvider();

        _command = new(_logger);
        _context = new(_serviceProvider);
        _commandDefinition = _command.GetCommand();
    }

    [Theory]
    [InlineData($"--subscription {_knownSubscription} --workspace {_knownWorkspace} --resource-group {_knownResourceGroup} --table {_knownTable} --query \"{_knownQuery}\"", true)]
    [InlineData($"--subscription {_knownSubscription} --workspace {_knownWorkspace} --resource-group {_knownResourceGroup} --table {_knownTable} --query \"{_knownQuery}\" --hours {_knownHours} --limit {_knownLimit}", true)]
    [InlineData($"--subscription {_knownSubscription} --workspace {_knownWorkspace} --table {_knownTable} --query \"{_knownQuery}\"", false)] // missing resource-group
    [InlineData($"--subscription {_knownSubscription}", false)]
    [InlineData("", false)]
    public async Task ExecuteAsync_ValidatesInputCorrectly(string args, bool shouldSucceed)
    {
        // Arrange
        if (shouldSucceed)
        {
            var mockResults = new List<JsonNode>
            {
                JsonNode.Parse(@"{""TimeGenerated"": ""2023-01-01T12:00:00Z"", ""Message"": ""Test log entry""}") ?? JsonNode.Parse("{}") ?? new JsonObject(),
                JsonNode.Parse(@"{""TimeGenerated"": ""2023-01-01T12:01:00Z"", ""Message"": ""Another log entry""}") ?? JsonNode.Parse("{}") ?? new JsonObject()
            };
            _monitorService.QueryWorkspaceLogs(
                Arg.Any<string>(),
                Arg.Any<string>(),
                Arg.Any<string>(),
                Arg.Any<string>(),
                Arg.Any<int?>(),
                Arg.Any<int?>(),
                Arg.Any<string>(),
                Arg.Any<RetryPolicyOptions>())
                .Returns(mockResults);
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
    public async Task ExecuteAsync_ReturnsQueryResults()
    {
        // Arrange
        var mockResults = new List<JsonNode>
        {
            JsonNode.Parse(@"{""TimeGenerated"": ""2023-01-01T12:00:00Z"", ""Message"": ""Test log entry"", ""Level"": ""Info""}") ?? new JsonObject(),
            JsonNode.Parse(@"{""TimeGenerated"": ""2023-01-01T12:01:00Z"", ""Message"": ""Another log entry"", ""Level"": ""Warning""}") ?? new JsonObject(),
            JsonNode.Parse(@"{""TimeGenerated"": ""2023-01-01T12:02:00Z"", ""Message"": ""Error occurred"", ""Level"": ""Error""}") ?? new JsonObject()
        };
        _monitorService.QueryWorkspaceLogs(
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<int?>(),
            Arg.Any<int?>(),
            Arg.Any<string>(),
            Arg.Any<RetryPolicyOptions>())
            .Returns(mockResults);

        var args = _commandDefinition.Parse($"--subscription {_knownSubscription} --workspace {_knownWorkspace} --resource-group {_knownResourceGroup} --table {_knownTable} --query \"{_knownQuery}\"");

        // Act
        var response = await _command.ExecuteAsync(_context, args);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.Status);
        Assert.NotNull(response.Results);

        // Verify the mock was called
        await _monitorService.Received(1).QueryWorkspaceLogs(
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<int?>(),
            Arg.Any<int?>(),
            Arg.Any<string>(),
            Arg.Any<RetryPolicyOptions>());
    }

    [Fact]
    public async Task ExecuteAsync_CallsServiceWithCorrectParameters()
    {
        // Arrange
        var mockResults = new List<JsonNode> { JsonNode.Parse(@"{""result"": ""data""}") ?? new JsonObject() };
        _monitorService.QueryWorkspaceLogs(
            _knownSubscription,
            _knownWorkspace,
            _knownQuery,
            _knownTable,
            int.Parse(_knownHours),
            int.Parse(_knownLimit),
            _knownTenant,
            Arg.Any<RetryPolicyOptions>())
            .Returns(mockResults);

        var args = _commandDefinition.Parse($"--subscription {_knownSubscription} --workspace {_knownWorkspace} --resource-group {_knownResourceGroup} --table {_knownTable} --query \"{_knownQuery}\" --hours {_knownHours} --limit {_knownLimit} --tenant {_knownTenant}");

        // Act
        var response = await _command.ExecuteAsync(_context, args);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.Status);
        await _monitorService.Received(1).QueryWorkspaceLogs(
            _knownSubscription,
            _knownWorkspace,
            _knownQuery,
            _knownTable,
            int.Parse(_knownHours),
            int.Parse(_knownLimit),
            _knownTenant,
            Arg.Any<RetryPolicyOptions>());
    }

    [Fact]
    public async Task ExecuteAsync_WithDefaultParameters_UsesExpectedDefaults()
    {
        // Arrange
        var mockResults = new List<JsonNode> { JsonNode.Parse(@"{""result"": ""data""}") ?? new JsonObject() };
        _monitorService.QueryWorkspaceLogs(
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<int?>(),
            Arg.Any<int?>(),
            Arg.Any<string>(),
            Arg.Any<RetryPolicyOptions>())
            .Returns(mockResults);

        var args = _commandDefinition.Parse($"--subscription {_knownSubscription} --workspace {_knownWorkspace} --resource-group {_knownResourceGroup} --table {_knownTable} --query \"{_knownQuery}\"");

        // Act
        var response = await _command.ExecuteAsync(_context, args);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.Status);
        await _monitorService.Received(1).QueryWorkspaceLogs(
            _knownSubscription,
            _knownWorkspace,
            _knownQuery,
            _knownTable,
            Arg.Any<int?>(), // Default hours
            Arg.Any<int?>(), // Default limit
            Arg.Any<string>(),
            Arg.Any<RetryPolicyOptions>());
    }

    [Fact]
    public async Task ExecuteAsync_HandlesServiceErrors()
    {
        // Arrange
        _monitorService.QueryWorkspaceLogs(
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<int?>(),
            Arg.Any<int?>(),
            Arg.Any<string>(),
            Arg.Any<RetryPolicyOptions>())
            .Returns(Task.FromException<List<JsonNode>>(new Exception("Test error")));

        var args = _commandDefinition.Parse($"--subscription {_knownSubscription} --workspace {_knownWorkspace} --resource-group {_knownResourceGroup} --table {_knownTable} --query \"{_knownQuery}\"");

        // Act
        var response = await _command.ExecuteAsync(_context, args);

        // Assert
        Assert.Equal(HttpStatusCode.InternalServerError, response.Status);
        Assert.Contains("Test error", response.Message);
        Assert.Contains("troubleshooting", response.Message);
    }
}
