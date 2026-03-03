// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.CommandLine;
using System.Net;
using System.Text.Json;
using Azure.Mcp.Tools.Skills.Commands;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Mcp.Core.Models.Command;
using NSubstitute;
using Xunit;

namespace Azure.Mcp.Tools.Skills.UnitTests;

public class TelemetryPublishCommandTests
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<TelemetryPublishCommand> _logger;
    private readonly CommandContext _context;
    private readonly TelemetryPublishCommand _command;
    private readonly Command _commandDefinition;

    public TelemetryPublishCommandTests()
    {
        var collection = new ServiceCollection();
        _serviceProvider = collection.BuildServiceProvider();

        _context = new(_serviceProvider);
        _logger = Substitute.For<ILogger<TelemetryPublishCommand>>();
        _command = new(_logger);
        _commandDefinition = _command.GetCommand();
    }

    [Fact]
    public async Task ExecuteAsync_ValidEvents_ReturnsOk()
    {
        var events = "[{\"timestamp\":\"2026-03-03T23:11:41.3587086Z\",\"event_type\":\"tool_invocation\",\"tool_name\":\"azure_best_practices\",\"session_id\":\"abc123\"}]";
        var args = _commandDefinition.Parse(["--events", events]);

        var response = await _command.ExecuteAsync(_context, args, TestContext.Current.CancellationToken);

        Assert.NotNull(response);
        Assert.Equal(HttpStatusCode.OK, response.Status);
        Assert.NotNull(response.Results);

        var json = JsonSerializer.Serialize(response.Results);
        var result = JsonSerializer.Deserialize<TelemetryPublishCommand.TelemetryPublishResult>(json,
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

        Assert.NotNull(result);
        Assert.Equal(1, result.EventCount);
    }

    [Fact]
    public async Task ExecuteAsync_MultipleEvents_ReturnsCorrectCount()
    {
        var events = "[{\"timestamp\":\"2026-03-03T23:00:00Z\",\"event_type\":\"tool_invocation\",\"tool_name\":\"tool_a\",\"session_id\":\"s1\"},{\"timestamp\":\"2026-03-03T23:01:00Z\",\"event_type\":\"tool_result\",\"tool_name\":\"tool_b\",\"session_id\":\"s1\"}]";
        var args = _commandDefinition.Parse(["--events", events]);

        var response = await _command.ExecuteAsync(_context, args, TestContext.Current.CancellationToken);

        Assert.NotNull(response);
        Assert.Equal(HttpStatusCode.OK, response.Status);

        var json = JsonSerializer.Serialize(response.Results);
        var result = JsonSerializer.Deserialize<TelemetryPublishCommand.TelemetryPublishResult>(json,
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

        Assert.NotNull(result);
        Assert.Equal(2, result.EventCount);
    }

    [Fact]
    public async Task ExecuteAsync_EmptyArray_ReturnsOkWithZeroCount()
    {
        var args = _commandDefinition.Parse(["--events", "[]"]);

        var response = await _command.ExecuteAsync(_context, args, TestContext.Current.CancellationToken);

        Assert.NotNull(response);
        Assert.Equal(HttpStatusCode.OK, response.Status);

        var json = JsonSerializer.Serialize(response.Results);
        var result = JsonSerializer.Deserialize<TelemetryPublishCommand.TelemetryPublishResult>(json,
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

        Assert.NotNull(result);
        Assert.Equal(0, result.EventCount);
    }

    [Fact]
    public async Task ExecuteAsync_InvalidJson_ReturnsBadRequest()
    {
        var args = _commandDefinition.Parse(["--events", "not-valid-json"]);

        var response = await _command.ExecuteAsync(_context, args, TestContext.Current.CancellationToken);

        Assert.NotNull(response);
        Assert.Equal(HttpStatusCode.BadRequest, response.Status);
        Assert.Contains("Invalid JSON", response.Message);
    }

    [Fact]
    public async Task ExecuteAsync_MissingEvents_ReturnsBadRequest()
    {
        var args = _commandDefinition.Parse([]);

        var response = await _command.ExecuteAsync(_context, args, TestContext.Current.CancellationToken);

        Assert.NotNull(response);
        Assert.Equal(HttpStatusCode.BadRequest, response.Status);
    }

    [Fact]
    public void Command_HasCorrectProperties()
    {
        Assert.Equal("publish", _command.Name);
        Assert.Equal("Publish Skills Telemetry", _command.Title);
        Assert.False(_command.Metadata.Destructive);
        Assert.False(_command.Metadata.ReadOnly);
        Assert.False(_command.Metadata.Idempotent);
    }
}
