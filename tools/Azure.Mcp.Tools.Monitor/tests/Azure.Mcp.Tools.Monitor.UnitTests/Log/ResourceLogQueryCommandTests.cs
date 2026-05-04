// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net;
using System.Text.Json.Nodes;
using Azure.Mcp.Tools.Monitor.Commands.Log;
using Azure.Mcp.Tools.Monitor.Services;
using Microsoft.Mcp.Core.Options;
using Microsoft.Mcp.Tests.Client;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Xunit;

namespace Azure.Mcp.Tools.Monitor.UnitTests.Log;

public sealed class ResourceLogQueryCommandTests : CommandUnitTestsBase<ResourceLogQueryCommand, IMonitorService>
{
    private const string _knownSubscription = "knownSubscription";
    private const string _knownResourceId = "/subscriptions/sub123/resourceGroups/rg1/providers/Microsoft.Storage/storageAccounts/storage1";
    private const string _knownTable = "StorageEvents";
    private const string _knownQuery = "| limit 10";
    private const string _knownTenant = "knownTenant";
    private const string _knownHours = "24";
    private const string _knownLimit = "100";

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
                new JsonObject([new("TimeGenerated", "2023-01-01T12:00:00Z"), new("Message", "Resource log entry")]),
                new JsonObject([new("TimeGenerated", "2023-01-01T12:01:00Z"), new("Message", "Another resource log entry")])
            };
            Service.QueryResourceLogs(
                _knownSubscription,
                _knownResourceId,
                _knownQuery,
                _knownTable,
                Arg.Any<int?>(),
                Arg.Any<int?>(),
                Arg.Any<string>(),
                Arg.Any<RetryPolicyOptions>(),
                Arg.Any<CancellationToken>())
                .Returns(mockResults);
        }

        // Act
        var response = await ExecuteCommandAsync(args);

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
            new JsonObject([new("TimeGenerated", "2023-01-01T12:00:00Z"), new("ResourceId", _knownResourceId), new("Level", "Info")]),
            new JsonObject([new("TimeGenerated", "2023-01-01T12:01:00Z"), new("ResourceId", _knownResourceId), new("Level", "Warning")]),
            new JsonObject([new("TimeGenerated", "2023-01-01T12:02:00Z"), new("ResourceId", _knownResourceId), new("Level", "Error")])
        };
        Service.QueryResourceLogs(
            _knownSubscription,
            _knownResourceId,
            _knownQuery,
            _knownTable,
            Arg.Any<int?>(),
            Arg.Any<int?>(),
            Arg.Any<string>(),
            Arg.Any<RetryPolicyOptions>(),
            Arg.Any<CancellationToken>())
            .Returns(mockResults);

        // Act
        var response = await ExecuteCommandAsync(
            "--subscription", _knownSubscription,
            "--resource-id", _knownResourceId,
            "--table", _knownTable,
            "--query", _knownQuery);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.Status);
        Assert.NotNull(response.Results);

        // Verify the mock was called
        await Service.Received(1).QueryResourceLogs(
            _knownSubscription,
            _knownResourceId,
            _knownQuery,
            _knownTable,
            Arg.Any<int?>(),
            Arg.Any<int?>(),
            Arg.Any<string>(),
            Arg.Any<RetryPolicyOptions>(),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteAsync_CallsServiceWithCorrectParameters()
    {
        // Arrange
        var mockResults = new List<JsonNode> { new JsonObject([new("result", "data")]) };
        Service.QueryResourceLogs(
            _knownSubscription,
            _knownResourceId,
            _knownQuery,
            _knownTable,
            int.Parse(_knownHours),
            int.Parse(_knownLimit),
            _knownTenant,
            Arg.Any<RetryPolicyOptions>(),
            Arg.Any<CancellationToken>())
            .Returns(mockResults);

        // Act
        var response = await ExecuteCommandAsync(
            "--subscription", _knownSubscription,
            "--resource-id", _knownResourceId,
            "--table", _knownTable,
            "--query", _knownQuery,
            "--hours", _knownHours,
            "--limit", _knownLimit,
            "--tenant", _knownTenant);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.Status);
        await Service.Received(1).QueryResourceLogs(
            _knownSubscription,
            _knownResourceId,
            _knownQuery,
            _knownTable,
            int.Parse(_knownHours),
            int.Parse(_knownLimit),
            _knownTenant,
            Arg.Any<RetryPolicyOptions>(),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteAsync_WithDefaultParameters_UsesExpectedDefaults()
    {
        // Arrange
        var mockResults = new List<JsonNode> { new JsonObject([new("result", "data")]) };
        Service.QueryResourceLogs(
            _knownSubscription,
            _knownResourceId,
            _knownQuery,
            _knownTable,
            Arg.Any<int?>(),
            Arg.Any<int?>(),
            Arg.Any<string>(),
            Arg.Any<RetryPolicyOptions>(),
            Arg.Any<CancellationToken>())
            .Returns(mockResults);

        // Act
        var response = await ExecuteCommandAsync(
            "--subscription", _knownSubscription,
            "--resource-id", _knownResourceId,
            "--table", _knownTable,
            "--query", _knownQuery);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.Status);
        await Service.Received(1).QueryResourceLogs(
            _knownSubscription,
            _knownResourceId,
            _knownQuery,
            _knownTable,
            Arg.Any<int?>(), // Default hours
            Arg.Any<int?>(), // Default limit
            Arg.Any<string>(),
            Arg.Any<RetryPolicyOptions>(),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteAsync_HandlesServiceErrors()
    {
        // Arrange
        Service.QueryResourceLogs(
            _knownSubscription,
            _knownResourceId,
            _knownQuery,
            _knownTable,
            Arg.Any<int?>(),
            Arg.Any<int?>(),
            Arg.Any<string>(),
            Arg.Any<RetryPolicyOptions>(),
            Arg.Any<CancellationToken>())
            .ThrowsAsync(new Exception("Test error"));

        // Act
        var response = await ExecuteCommandAsync(
            "--subscription", _knownSubscription,
            "--resource-id", _knownResourceId,
            "--table", _knownTable,
            "--query", _knownQuery);

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
        var mockResults = new List<JsonNode> { new JsonObject([new("result", "vm data")]) };
        Service.QueryResourceLogs(
            _knownSubscription,
            complexResourceId,
            query,
            table,
            Arg.Any<int?>(),
            Arg.Any<int?>(),
            Arg.Any<string>(),
            Arg.Any<RetryPolicyOptions>(),
            Arg.Any<CancellationToken>())
            .Returns(mockResults);

        // Act
        var response = await ExecuteCommandAsync(
            "--subscription", _knownSubscription,
            "--resource-id", complexResourceId,
            "--table", table,
            "--query", query);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.Status);
        await Service.Received(1).QueryResourceLogs(
            _knownSubscription,
            complexResourceId,
            query,
            table,
            Arg.Any<int?>(),
            Arg.Any<int?>(),
            Arg.Any<string>(),
            Arg.Any<RetryPolicyOptions>(),
            Arg.Any<CancellationToken>());
    }
}
