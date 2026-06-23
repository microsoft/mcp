// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.CommandLine;
using System.Net;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Mcp.Core.Commands;
using Microsoft.Mcp.Core.Models.Command;
using Xunit;

namespace Microsoft.Mcp.Core.Tests.Commands;

/// <summary>
/// Tests for <see cref="CommandValidationException"/> defaults and for the way
/// <see cref="BaseCommand{TOptions}.HandleException"/> maps the exception's
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
        Description = "A command used only to exercise HandleException in tests.")]
    private sealed class ValidationTestCommand : BaseCommand<EmptyOptions>
    {
        protected override EmptyOptions BindOptions(ParseResult parseResult) => new();

        public override Task<CommandResponse> ExecuteAsync(
            CommandContext context, ParseResult parseResult, CancellationToken cancellationToken)
            => Task.FromResult(context.Response);

        public void InvokeHandleException(CommandContext context, Exception ex) => HandleException(context, ex);
    }

    private static CommandContext CreateContext()
    {
        var serviceProvider = new ServiceCollection().BuildServiceProvider();
        return new CommandContext(serviceProvider);
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
        var context = CreateContext();

        command.InvokeHandleException(context, new CommandValidationException("Validation failed."));

        Assert.Equal(HttpStatusCode.BadRequest, context.Response.Status);
        Assert.Equal("Validation failed.", context.Response.Message);
        Assert.Null(context.Response.Results);
    }

    [Fact]
    public void HandleException_HonorsExplicitStatusCode()
    {
        var command = new ValidationTestCommand();
        var context = CreateContext();

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
        var context = CreateContext();

        command.InvokeHandleException(
            context,
            new CommandValidationException(
                "ignored",
                missingOptions: new[] { "--resource-group", "--account" }));

        Assert.Equal(HttpStatusCode.BadRequest, context.Response.Status);
        Assert.Equal("Missing Required options: --resource-group, --account", context.Response.Message);
        Assert.Null(context.Response.Results);
    }
}
