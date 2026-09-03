// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.CommandLine;
using System.Diagnostics;
using System.Net;
using System.Text.Json;
using Microsoft.Mcp.Core.Commands;
using Microsoft.Mcp.Core.Extensions;
using Microsoft.Mcp.Core.Models;
using Microsoft.Mcp.Core.Models.Command;
using Xunit;

namespace Microsoft.Mcp.Core.Tests.Commands;

public sealed class CommandTelemetryTests
{
    [CommandMetadata(
        Id = "00000000-0000-0000-0000-0000000000cf",
        Name = "test-telemetry",
        Title = "Test Telemetry Command",
        Description = "A command used only to exercise command telemetry in tests.")]
    private sealed class TelemetryTestCommand(HttpStatusCode status, string? telemetryFailureMessage)
        : BaseCommand<EmptyOptions, string>
    {
        public override Task<CommandResponse> ExecuteAsync(
            CommandContext context, EmptyOptions options, CancellationToken cancellationToken)
        {
            context.Response.Status = status;
            context.Response.TelemetryFailureMessage = telemetryFailureMessage;
            return Task.FromResult(context.Response);
        }
    }

    [Fact]
    public async Task ExecuteAsync_FailedResponse_CapturesTelemetryFailureMessage()
    {
        using var activity = CreateActivity();
        var response = await ExecuteAsync(
            new TelemetryTestCommand(HttpStatusCode.BadRequest, "Sanitized failure details."),
            activity);

        Assert.Equal(HttpStatusCode.BadRequest, response.Status);
        Assert.Equal("Sanitized failure details.", activity.GetTagItem(TagName.ToolFailureMessage));
        Assert.Null(activity.GetTagItem(TagName.ExceptionMessage));
    }

    [Fact]
    public async Task ExecuteAsync_SuccessfulResponse_DoesNotCaptureTelemetryFailureMessage()
    {
        using var activity = CreateActivity();
        await ExecuteAsync(
            new TelemetryTestCommand(HttpStatusCode.OK, "Successful operation details."),
            activity);

        Assert.Null(activity.GetTagItem(TagName.ToolFailureMessage));
        Assert.Null(activity.GetTagItem(TagName.ExceptionMessage));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public async Task ExecuteAsync_EmptyTelemetryFailureMessage_PreservesExistingTelemetry(string? telemetryFailureMessage)
    {
        using var activity = CreateActivity();
        activity.SetTag(TagName.ExceptionMessage, "Existing failure details.");

        await ExecuteAsync(
            new TelemetryTestCommand(HttpStatusCode.BadRequest, telemetryFailureMessage),
            activity);

        Assert.Null(activity.GetTagItem(TagName.ToolFailureMessage));
        Assert.Equal("Existing failure details.", activity.GetTagItem(TagName.ExceptionMessage));
    }

    [Fact]
    public async Task ExecuteAsync_ExplicitTelemetryFailureMessage_PreservesExceptionTelemetry()
    {
        using var activity = CreateActivity();
        activity.SetTag(TagName.ExceptionMessage, "Generic failure details.");

        await ExecuteAsync(
            new TelemetryTestCommand(HttpStatusCode.BadRequest, "Command-specific failure details."),
            activity);

        Assert.Equal("Command-specific failure details.", activity.GetTagItem(TagName.ToolFailureMessage));
        Assert.Equal("Generic failure details.", activity.GetTagItem(TagName.ExceptionMessage));
    }

    [Fact]
    public void Serialize_DoesNotIncludeTelemetryFailureMessage()
    {
        var response = new CommandResponse
        {
            Status = HttpStatusCode.BadRequest,
            Message = "Client-visible message.",
            TelemetryFailureMessage = "Telemetry-only message."
        };

        var json = JsonSerializer.Serialize(response, ModelsJsonContext.Default.CommandResponse);

        Assert.DoesNotContain("telemetryFailureMessage", json, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("Telemetry-only message.", json, StringComparison.Ordinal);
        Assert.Contains("Client-visible message.", json, StringComparison.Ordinal);
    }

    private static Activity CreateActivity()
    {
        var activity = new Activity("test-activity");
        activity.Start();
        return activity;
    }

    private static async Task<CommandResponse> ExecuteAsync(
        TelemetryTestCommand command,
        Activity activity)
    {
        Assert.True(command.GetCommand().TryParseFromDictionary(null, out ParseResult? parseResult, out var parseError));
        Assert.Null(parseError);
        Assert.NotNull(parseResult);

        return await ((IBaseCommand)command).ExecuteAsync(
            new CommandContext(activity),
            parseResult,
            TestContext.Current.CancellationToken);
    }
}
