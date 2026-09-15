// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net;
using System.Text.Json;
using System.Text.Json.Serialization.Metadata;
using Azure.Mcp.Tools.Monitor.Commands;
using Azure.Mcp.Tools.Monitor.Commands.Instrumentation;
using Azure.Mcp.Tools.Monitor.Instrumentation.Pipeline;
using Azure.Mcp.Tools.Monitor.Options.Instrumentation;
using Azure.Mcp.Tools.Monitor.Tools.Instrumentation;
using Microsoft.Extensions.Logging;
using Microsoft.Mcp.Core.Commands;
using Microsoft.Mcp.Core.Models.Command;
using NSubstitute;
using Xunit;

namespace Azure.Mcp.Tools.Monitor.Tests.Instrumentation.Commands;

public sealed class InstrumentationCommandResultTests
{
    [Fact]
    public async Task OrchestratorStartCommand_ReturnsObjectRoot()
    {
        var tool = new OrchestratorTool(new WorkspaceAnalyzer([], [], [], []));
        var command = new OrchestratorStartCommand(
            Substitute.For<ILogger<OrchestratorStartCommand>>(),
            tool);
        var options = new OrchestratorStartOptions
        {
            WorkspacePath = Path.Combine(Path.GetTempPath(), $"missing-{Guid.NewGuid():N}")
        };

        var response = await command.ExecuteAsync(
            new CommandContext(null!),
            options,
            TestContext.Current.CancellationToken);

        var result = DeserializeObjectResult(
            response,
            MonitorJsonContext.Default.OrchestratorStartCommandResult);
        AssertJsonResult(result.Result);
    }

    [Fact]
    public async Task OrchestratorNextCommand_ReturnsObjectRoot()
    {
        var tool = new OrchestratorTool(new WorkspaceAnalyzer([], [], [], []));
        var command = new OrchestratorNextCommand(
            Substitute.For<ILogger<OrchestratorNextCommand>>(),
            tool);
        var options = new OrchestratorNextOptions
        {
            SessionId = $"missing-{Guid.NewGuid():N}",
            CompletionNote = "done"
        };

        var response = await command.ExecuteAsync(
            new CommandContext(null!),
            options,
            TestContext.Current.CancellationToken);

        var result = DeserializeObjectResult(
            response,
            MonitorJsonContext.Default.OrchestratorNextCommandResult);
        AssertJsonResult(result.Result);
    }

    [Fact]
    public async Task SendBrownfieldAnalysisCommand_ReturnsObjectRoot()
    {
        var command = new SendBrownfieldAnalysisCommand(
            Substitute.For<ILogger<SendBrownfieldAnalysisCommand>>(),
            new SendBrownfieldAnalysisTool([]));
        var options = new SendBrownfieldAnalysisOptions
        {
            SessionId = $"missing-{Guid.NewGuid():N}",
            FindingsJson = "{}"
        };

        var response = await command.ExecuteAsync(
            new CommandContext(null!),
            options,
            TestContext.Current.CancellationToken);

        var result = DeserializeObjectResult(
            response,
            MonitorJsonContext.Default.SendBrownfieldAnalysisCommandResult);
        AssertJsonResult(result.Result);
    }

    [Fact]
    public async Task SendEnhancementSelectCommand_ReturnsObjectRoot()
    {
        var command = new SendEnhancementSelectCommand(
            Substitute.For<ILogger<SendEnhancementSelectCommand>>());
        var options = new SendEnhancementSelectOptions
        {
            SessionId = $"missing-{Guid.NewGuid():N}",
            EnhancementKeys = "redis"
        };

        var response = await command.ExecuteAsync(
            new CommandContext(null!),
            options,
            TestContext.Current.CancellationToken);

        var result = DeserializeObjectResult(
            response,
            MonitorJsonContext.Default.SendEnhancementSelectCommandResult);
        AssertJsonResult(result.Result);
    }

    private static T DeserializeObjectResult<T>(
        CommandResponse response,
        JsonTypeInfo<T> typeInfo)
    {
        Assert.Equal(HttpStatusCode.OK, response.Status);
        Assert.NotNull(response.Results);

        var json = JsonSerializer.Serialize(response.Results);
        using var document = JsonDocument.Parse(json);
        Assert.Equal(JsonValueKind.Object, document.RootElement.ValueKind);

        var result = JsonSerializer.Deserialize(json, typeInfo);
        Assert.NotNull(result);
        return result;
    }

    private static void AssertJsonResult(string result)
    {
        using var document = JsonDocument.Parse(result);
        Assert.Equal("error", document.RootElement.GetProperty("status").GetString());
    }
}
