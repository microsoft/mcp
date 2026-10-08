// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.CommandLine;
using System.Diagnostics;
using System.Net;
using System.Text.Json;
using Microsoft.Mcp.Core.Commands;
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
        Description = "A command used only to exercise command telemetry in tests.",
        OperationPlane = ToolOperationPlane.NotApplicable,
        Destructive = true,
        Idempotent = false,
        OpenWorld = true,
        ReadOnly = false,
        Secret = false,
        LocalRequired = false)]
    private sealed class TelemetryTestCommand(HttpStatusCode status, string? telemetryFailureMessage, bool failValidation = false, bool legacyValidation = false)
        : BaseCommand<EmptyOptions, string>
    {
        public override void ValidateOptions(EmptyOptions options, ValidationResult validationResult)
        {
            base.ValidateOptions(options, validationResult);

            if (failValidation)
            {
                validationResult.AddError("Invalid option 'private value'.", "Invalid option.");
            }

            if (legacyValidation)
            {
                validationResult.Errors.Add("Invalid option 'private value'.");
            }
        }

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
    public async Task ExecuteAsync_ValidationError_PreservesUserMessageAndCapturesSafeTelemetry()
    {
        using var activity = CreateActivity();
        var response = await ExecuteAsync(
            new TelemetryTestCommand(HttpStatusCode.OK, null, failValidation: true),
            activity);

        Assert.Equal(HttpStatusCode.BadRequest, response.Status);
        Assert.Equal("Invalid option 'private value'.", response.Message);
        Assert.Equal("Invalid option.", response.TelemetryFailureMessage);
        Assert.Equal("Invalid option.", activity.GetTagItem(TagName.ToolFailureMessage));
        Assert.Null(activity.GetTagItem(TagName.ExceptionMessage));
    }

    [Fact]
    public async Task ExecuteAsync_LegacyValidationError_UsesGenericSafeTelemetry()
    {
        using var activity = CreateActivity();
        var response = await ExecuteAsync(
            new TelemetryTestCommand(HttpStatusCode.OK, null, legacyValidation: true),
            activity);

        Assert.Equal("Invalid option 'private value'.", response.Message);
        Assert.Equal("Invalid options.", response.TelemetryFailureMessage);
        Assert.Equal("Invalid options.", activity.GetTagItem(TagName.ToolFailureMessage));
    }

    [Fact]
    public void Validate_ParserError_PreservesMessageAndUsesGenericSafeTelemetry()
    {
        var command = new TelemetryTestCommand(HttpStatusCode.OK, null);
        var parseResult = command.GetCommand().Parse("--unknown private");
        var response = new CommandResponse();

        var result = ((IBaseCommand)command).Validate(parseResult.CommandResult, response);

        Assert.False(result.IsValid);
        Assert.Equal(HttpStatusCode.BadRequest, response.Status);
        Assert.Equal(string.Join('\n', result.Errors), response.Message);
        Assert.Equal("Invalid options.", response.TelemetryFailureMessage);
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
