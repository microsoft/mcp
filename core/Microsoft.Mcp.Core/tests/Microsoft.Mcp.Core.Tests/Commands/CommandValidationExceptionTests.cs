// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Diagnostics;
using System.Net;
using Microsoft.Mcp.Core.Commands;
using Microsoft.Mcp.Core.Models.Command;
using Xunit;

namespace Microsoft.Mcp.Core.Tests.Commands;

/// <summary>
/// Tests for <see cref="CommandValidationException"/> defaults and for the way
/// <see cref="BaseCommand{TOptions, TResult}.HandleException"/> maps the exception's
/// <see cref="CommandValidationException.StatusCode"/> into the command response.
/// These lock in the BadRequest (400) default so it does not regress back to 500.
/// </summary>
public sealed class CommandValidationExceptionTests
{
    // ---------- Minimal concrete command fixture exposing HandleException ----------

    [CommandMetadata(
        Id = "00000000-0000-0000-0000-0000000000ce",
        Name = "test-validation",
        Title = "Test Validation Command",
        Description = "A command used only to exercise HandleException in tests.",
        OperationPlane = ToolOperationPlane.NotApplicable,
        Destructive = true,
        Idempotent = false,
        OpenWorld = true,
        ReadOnly = false,
        Secret = false,
        LocalRequired = false)]
    private sealed class ValidationTestCommand : BaseCommand<EmptyOptions, string>
    {
        public override Task<CommandResponse> ExecuteAsync(
            CommandContext context, EmptyOptions options, CancellationToken cancellationToken)
            => Task.FromResult(context.Response);

        public void InvokeHandleException(CommandContext context, Exception ex) => HandleException(context, ex);
    }

    // ---------- Exception default tests ----------

    [Fact]
    public void StatusCode_DefaultsToBadRequest()
    {
        var exception = new CommandValidationException("Validation failed.");
        Assert.Equal(HttpStatusCode.BadRequest, exception.StatusCode);
    }

    [Fact]
    public void Code_DefaultsToValidationError()
    {
        var exception = new CommandValidationException("Validation failed.");
        Assert.Equal("ValidationError", exception.Code);
    }

    [Fact]
    public void TelemetrySafeMessage_DefaultsToGenericMessage()
    {
        var exception = new CommandValidationException("Invalid value 'private input'.");

        Assert.Equal("Command validation failed.", exception.TelemetrySafeMessage);
    }

    [Fact]
    public void TelemetrySafeMessage_WhenBlank_UsesGenericMessage()
    {
        var exception = new CommandValidationException("Invalid value 'private input'.")
        {
            TelemetrySafeMessage = " "
        };

        Assert.Equal("Command validation failed.", exception.TelemetrySafeMessage);
    }

    [Fact]
    public void Constructor_PreservesExplicitValues()
    {
        var missingOptions = new[] { "--resource-group" };
        var exception = new CommandValidationException(
            "Validation failed.",
            HttpStatusCode.Conflict,
            "CustomCode",
            missingOptions);

        Assert.Equal(HttpStatusCode.Conflict, exception.StatusCode);
        Assert.Equal("CustomCode", exception.Code);
        Assert.Equal(missingOptions, exception.MissingOptions);
    }

    // ---------- HandleException mapping tests ----------

    [Fact]
    public void HandleException_MapsDefaultStatusCode_ToBadRequest()
    {
        var command = new ValidationTestCommand();
        var context = new CommandContext();

        command.InvokeHandleException(context, new CommandValidationException("Validation failed."));

        Assert.Equal(HttpStatusCode.BadRequest, context.Response.Status);
        Assert.Equal("Validation failed.", context.Response.Message);
        Assert.Null(context.Response.Results);
    }

    [Fact]
    public void HandleException_HonorsExplicitStatusCode()
    {
        var command = new ValidationTestCommand();
        var context = new CommandContext();

        command.InvokeHandleException(
            context,
            new CommandValidationException("Conflict occurred.", HttpStatusCode.Conflict));

        Assert.Equal(HttpStatusCode.Conflict, context.Response.Status);
        Assert.Equal("Conflict occurred.", context.Response.Message);
        Assert.Null(context.Response.Results);
    }

    [Fact]
    public void HandleException_FormatsMissingOptionsMessage()
    {
        var command = new ValidationTestCommand();
        var context = new CommandContext();

        command.InvokeHandleException(
            context,
            new CommandValidationException(
                "ignored",
                missingOptions: ["--resource-group", "--account"]));

        Assert.Equal(HttpStatusCode.BadRequest, context.Response.Status);
        Assert.Equal("Missing Required options: --resource-group, --account", context.Response.Message);
        Assert.Null(context.Response.Results);
    }

    [Fact]
    public void HandleException_PreservesUserMessageAndCapturesSafeTelemetry()
    {
        using var activity = new Activity("validation-test");
        activity.Start();
        var command = new ValidationTestCommand();
        var context = new CommandContext(activity);

        command.InvokeHandleException(context, new CommandValidationException("Invalid value 'private input'.")
        {
            TelemetrySafeMessage = "Invalid value."
        });

        Assert.Equal("Invalid value 'private input'.", context.Response.Message);
        Assert.Equal("Invalid value.", context.Response.TelemetryFailureMessage);
        Assert.Equal("Invalid value.", activity.GetTagItem(TagName.ExceptionMessage));
    }

    [Fact]
    public void HandleException_WithoutExplicitSafeMessage_UsesGenericTelemetry()
    {
        using var activity = new Activity("validation-test");
        activity.Start();
        var command = new ValidationTestCommand();
        var context = new CommandContext(activity);

        command.InvokeHandleException(context, new CommandValidationException("Invalid value 'private input'."));

        Assert.Equal("Invalid value 'private input'.", context.Response.Message);
        Assert.Equal("Command validation failed.", context.Response.TelemetryFailureMessage);
        Assert.Equal("Command validation failed.", activity.GetTagItem(TagName.ExceptionMessage));
    }
}
