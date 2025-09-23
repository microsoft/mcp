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
public sealed class ResourceLogQueryCommandTests
{
    private readonly IServiceProvider _serviceProvider;
    private readonly IMonitorService _monitorService;
    private readonly ILogger<ResourceLogQueryCommand> _logger;
    private readonly ResourceLogQueryCommand _command;
    private readonly CommandContext _context;
    private readonly Command _commandDefinition;

    private const string _knownSubscription = "knownSubscription";
    private const string _knownResourceId = "/subscriptions/sub123/resourceGroups/rg1/providers/Microsoft.Storage/storageAccounts/storage1";
    private const string _knownTable = "StorageEvents";
    private const string _knownQuery = "| limit 10";
    private const string _knownTenant = "knownTenant";
    private const string _knownHours = "24";
    private const string _knownLimit = "100";

    public ResourceLogQueryCommandTests()
    {
        _monitorService = Substitute.For<IMonitorService>();
        _logger = Substitute.For<ILogger<ResourceLogQueryCommand>>();

        var collection = new ServiceCollection();
        collection.AddSingleton(_monitorService);
        _serviceProvider = collection.BuildServiceProvider();

        _command = new(_logger);
        _context = new(_serviceProvider);
        _commandDefinition = _command.GetCommand();
    }

    [Theory]
    [InlineData($"--subscription {_knownSubscription} --resource-id {_knownResourceId} --table {_knownTable} --query \"{_knownQuery}\"", true)]
    [InlineData($"--subscription {_knownSubscription} --resource-id {_knownResourceId} --table {_knownTable} --query \"{_knownQuery}\" --hours {_knownHours} --limit {_knownLimit}", true)]
    [InlineData($"--subscription {_knownSubscription} --table {_knownTable} --query \"{_knownQuery}\"", false)] // missing resource-id
    [InlineData($"--subscription {_knownSubscription}", false)]
    [InlineData("", false)]
    public async Task ExecuteAsync_ValidatesInputCorrectly(string args, bool shouldSucceed)
    {
        // Arrange
        if (shouldSucceed)
        {
            var mockResults = new List<JsonNode>
            {
                JsonNode.Parse(@"{""TimeGenerated"": ""2023-01-01T12:00:00Z"", ""Message"": ""Resource log entry""}") ?? JsonNode.Parse("{}") ?? new JsonObject(),
                JsonNode.Parse(@"{""TimeGenerated"": ""2023-01-01T12:01:00Z"", ""Message"": ""Another resource log entry""}") ?? JsonNode.Parse("{}") ?? new JsonObject()
            };
            _monitorService.QueryResourceLogs(
                _knownSubscription,
                _knownResourceId,
                _knownQuery,
                _knownTable,
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
            JsonNode.Parse($@"{{""TimeGenerated"": ""2023-01-01T12:00:00Z"", ""ResourceId"": ""{_knownResourceId}"", ""Level"": ""Info""}}") ?? new JsonObject(),
            JsonNode.Parse($@"{{""TimeGenerated"": ""2023-01-01T12:01:00Z"", ""ResourceId"": ""{_knownResourceId}"", ""Level"": ""Warning""}}") ?? new JsonObject(),
            JsonNode.Parse($@"{{""TimeGenerated"": ""2023-01-01T12:02:00Z"", ""ResourceId"": ""{_knownResourceId}"", ""Level"": ""Error""}}") ?? new JsonObject()
        };
        _monitorService.QueryResourceLogs(
            _knownSubscription,
            _knownResourceId,
            _knownQuery,
            _knownTable,
            Arg.Any<int?>(),
            Arg.Any<int?>(),
            Arg.Any<string>(),
            Arg.Any<RetryPolicyOptions>())
            .Returns(mockResults);

        var args = _commandDefinition.Parse($"--subscription {_knownSubscription} --resource-id {_knownResourceId} --table {_knownTable} --query \"{_knownQuery}\"");

        // Act
        var response = await _command.ExecuteAsync(_context, args);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.Status);
        Assert.NotNull(response.Results);

        // Verify the mock was called
        await _monitorService.Received(1).QueryResourceLogs(
            _knownSubscription,
            _knownResourceId,
            _knownQuery,
            _knownTable,
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
        _monitorService.QueryResourceLogs(
            _knownSubscription,
            _knownResourceId,
            _knownQuery,
            _knownTable,
            int.Parse(_knownHours),
            int.Parse(_knownLimit),
            _knownTenant,
            Arg.Any<RetryPolicyOptions>())
            .Returns(mockResults);

        var args = _commandDefinition.Parse($"--subscription {_knownSubscription} --resource-id {_knownResourceId} --table {_knownTable} --query \"{_knownQuery}\" --hours {_knownHours} --limit {_knownLimit} --tenant {_knownTenant}");

        // Act
        var response = await _command.ExecuteAsync(_context, args);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.Status);
        await _monitorService.Received(1).QueryResourceLogs(
            _knownSubscription,
            _knownResourceId,
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
        _monitorService.QueryResourceLogs(
            _knownSubscription,
            _knownResourceId,
            _knownQuery,
            _knownTable,
            Arg.Any<int?>(),
            Arg.Any<int?>(),
            Arg.Any<string>(),
            Arg.Any<RetryPolicyOptions>())
            .Returns(mockResults);

        var args = _commandDefinition.Parse($"--subscription {_knownSubscription} --resource-id {_knownResourceId} --table {_knownTable} --query \"{_knownQuery}\"");

        // Act
        var response = await _command.ExecuteAsync(_context, args);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.Status);
        await _monitorService.Received(1).QueryResourceLogs(
            _knownSubscription,
            _knownResourceId,
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
        _monitorService.QueryResourceLogs(
            _knownSubscription,
            _knownResourceId,
            _knownQuery,
            _knownTable,
            Arg.Any<int?>(),
            Arg.Any<int?>(),
            Arg.Any<string>(),
            Arg.Any<RetryPolicyOptions>())
            .Returns(Task.FromException<List<JsonNode>>(new Exception("Test error")));

        var args = _commandDefinition.Parse($"--subscription {_knownSubscription} --resource-id {_knownResourceId} --table {_knownTable} --query \"{_knownQuery}\"");

        // Act
        var response = await _command.ExecuteAsync(_context, args);

        // Assert
        Assert.Equal(HttpStatusCode.InternalServerError, response.Status);
        Assert.Contains("Test error", response.Message);
        Assert.Contains("troubleshooting", response.Message);
    }

    [Fact]
    public async Task ExecuteAsync_WithComplexResourceId_HandlesCorrectly()
    {
        // Arrange
        var complexResourceId = "/subscriptions/12345678-1234-1234-1234-123456789012/resourceGroups/my-rg/providers/Microsoft.Compute/virtualMachines/my-vm";
        var query = "| where Level == 'Error'";
        var table = "VMEvents";
        var mockResults = new List<JsonNode> { JsonNode.Parse(@"{""result"": ""vm data""}") ?? new JsonObject() };
        _monitorService.QueryResourceLogs(
            _knownSubscription,
            complexResourceId,
            query,
            table,
            Arg.Any<int?>(),
            Arg.Any<int?>(),
            Arg.Any<string>(),
            Arg.Any<RetryPolicyOptions>())
            .Returns(mockResults);

        var args = _commandDefinition.Parse($"--subscription {_knownSubscription} --resource-id \"{complexResourceId}\" --table {table} --query \"{query}\"");

        // Act
        var response = await _command.ExecuteAsync(_context, args);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.Status);
        await _monitorService.Received(1).QueryResourceLogs(
            _knownSubscription,
            complexResourceId,
            query,
            table,
            Arg.Any<int?>(),
            Arg.Any<int?>(),
            Arg.Any<string>(),
            Arg.Any<RetryPolicyOptions>());
    }
}
