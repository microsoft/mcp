// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.CommandLine.Parsing;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Net;
using System.Reflection;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization.Metadata;
using Azure;
using Azure.Mcp.Core.Areas.Server;
using Microsoft.Identity.Client;
using Microsoft.Mcp.Core.Extensions;
using Microsoft.Mcp.Core.Helpers;
using Microsoft.Mcp.Core.Models.Command;
using Microsoft.Mcp.Core.Options;

namespace Microsoft.Mcp.Core.Commands;

public abstract class BaseCommand<[DynamicallyAccessedMembers(TrimAnnotations.CommandAnnotations)] TOptions, TResult> : IBaseCommand where TOptions : class
{
    private const string MissingRequiredOptionsPrefix = "Missing Required options: ";
    private const string TroubleshootingUrl = "https://aka.ms/azmcp/troubleshooting";

    private readonly Command _command;

    [UnconditionalSuppressMessage("Trimming", "IL2075:UnrecognizedReflectionPattern",
        Justification = "CommandMetadataAttribute is only applied to concrete command types that are rooted by DI service registration.")]
    protected BaseCommand()
    {
        var attr = GetType().GetCustomAttribute<CommandMetadataAttribute>() ??
            throw new InvalidOperationException($"Command type '{GetType().FullName}' is missing required [CommandMetadata] attribute.");

        if (!attr.IsValid())
        {
            throw new InvalidOperationException(
                $"Command type '{GetType().FullName}' is missing required command metadata. Apply [CommandMetadata] " +
                "to the command class with non-null values that are available during BaseCommand construction.");
        }

        Id = attr.Id;
        Name = attr.Name;
        Description = attr.Description;
        Title = attr.Title;
        Metadata = attr.ToToolMetadata();

        _command = new ExtendedCommand(this, Name, Description);
        OptionBinder.RegisterOptions<TOptions>(_command);
    }

    public string Id { get; }
    public string Name { get; }
    public string Description { get; }
    public string Title { get; }
    public ToolMetadata Metadata { get; }

    /// <summary>
    /// Gets the source-generated <see cref="JsonTypeInfo{TResult}"/> for this command's result type.
    /// Migrated commands override this so the server can advertise an MCP <c>outputSchema</c> and emit
    /// <c>structuredContent</c>. Commands that have not been migrated return <see langword="null"/> and
    /// simply advertise no output schema.
    /// </summary>
    public virtual JsonTypeInfo<TResult>? ResultTypeInfo => null;

    JsonTypeInfo? IBaseCommand.ResultTypeInfo => ResultTypeInfo;

    public Command GetCommand() => _command;

    public TOptions BindOptions(ParseResult parseResult)
    {
        var options = OptionBinder.BindOptions<TOptions>(parseResult);
        PostBindOptions(options);
        return options;
    }

    /// <summary>
    /// Performs additional processing on the bound options after they have been bound.
    /// </summary>
    /// <param name="options">The bound options to process.</param>
    public virtual void PostBindOptions(TOptions options)
    {
    }

    /// <summary>
    /// Validates the options after they have been bound.
    /// </summary>
    /// <param name="options">The options to validate.</param>
    /// <param name="validationResult">The validation result to populate.</param>
    public virtual void ValidateOptions(TOptions options, ValidationResult validationResult)
    {
    }

    public abstract Task<CommandResponse> ExecuteAsync(CommandContext context, TOptions options, CancellationToken cancellationToken);

    async Task<CommandResponse> IBaseCommand.ExecuteAsync(CommandContext context, ParseResult parseResult, CancellationToken cancellationToken)
    {
        TOptions options;
        try
        {
            options = BindOptions(parseResult);
        }
        catch (CommandValidationException ex)
        {
            HandleException(context, ex);
            return context.Response;
        }

        var validationResult = new ValidationResult();
        ValidateOptions(options, validationResult);

        if (!validationResult.IsValid)
        {
            var errorMessage = string.Join('\n', validationResult.Errors);
            SetValidationError(context.Response, errorMessage, HttpStatusCode.BadRequest);
            return context.Response!;
        }

        CommandResponse response = await ExecuteAsync(context, options, cancellationToken);
        return response;
    }

    /// <summary>
    /// Stores the command result on the response using <see cref="ResultTypeInfo"/> so it is serialized
    /// with the same source-generated <see cref="JsonTypeInfo{TResult}"/> used to advertise the
    /// <c>outputSchema</c>. Only call this from commands that override <see cref="ResultTypeInfo"/>.
    /// </summary>
    protected void SetResult(CommandContext context, TResult result)
    {
        ArgumentNullException.ThrowIfNull(context);

        var typeInfo = ResultTypeInfo
            ?? throw new InvalidOperationException(
                $"Command type '{GetType().FullName}' called {nameof(SetResult)} without overriding {nameof(ResultTypeInfo)}.");

        context.Response.Results = ResponseResult.Create(result, typeInfo);
    }

    protected virtual void HandleException(CommandContext context, Exception ex)
    {
        context.Activity?.SetStatus(ActivityStatusCode.Error)
            ?.SetTag(TagName.ExceptionType, ex.GetType().ToString())
            ?.SetTag(TagName.ExceptionStackTrace, ex.StackTrace);

        var response = context.Response;

        // Handle structured validation errors first
        if (ex is CommandValidationException cve)
        {
            response.Status = cve.StatusCode;
            response.Message = cve.Message;

            // Include the command validation exception message as it should be safe. Requires custom validators to
            // exclude any sensitive information from their error messages.
            context.Activity?.SetTag(TagName.ExceptionMessage, response.Message);
            response.Results = null;
            return;
        }

        // Start with adding the status code of the exception.
        var exceptionDetails = new JsonObject([new("StatusCode", (int)GetStatusCode(ex))]);
        if (ex is RequestFailedException failedException)
        {
            // For RequestFailedException, we can include the error code and request ID.
            exceptionDetails.Add("ErrorCode", failedException.ErrorCode);
            exceptionDetails.Add("RequestId", failedException.GetRawResponse()?.ClientRequestId);
        }
        else if (ex is MsalServiceException msalServiceException)
        {
            // For MsalServiceException, we can include the error code and correlation ID.
            exceptionDetails.Add("ErrorCode", msalServiceException.ErrorCode);
            exceptionDetails.Add("CorrelationId", msalServiceException.CorrelationId);
        }
        else if (ex is MsalClientException msalClientException)
        {
            // For MsalClientException, we can include the error code and correlation ID.
            exceptionDetails.Add("ErrorCode", msalClientException.ErrorCode);
            exceptionDetails.Add("CorrelationId", msalClientException.CorrelationId);
        }

        context.Activity?.SetTag(TagName.ExceptionMessage, exceptionDetails);

        var result = new ExceptionResult(
            Message: ex.Message ?? string.Empty,
#if DEBUG
            StackTrace: ex.StackTrace,
#else
            StackTrace: null,
#endif
            Type: ex.GetType().Name);

        response.Status = GetStatusCode(ex);
        response.Message = GetErrorMessage(ex) + $". To mitigate this issue, please refer to the troubleshooting guidelines here at {TroubleshootingUrl}.";
        response.Results = ResponseResult.Create(result, CoreJsonContext.Default.ExceptionResult);
    }

    protected virtual string GetErrorMessage(Exception ex) => ex.Message;

    protected virtual HttpStatusCode GetStatusCode(Exception ex) => ex switch
    {
        ArgumentException => HttpStatusCode.BadRequest,  // Bad Request for invalid arguments
        InvalidOperationException => HttpStatusCode.UnprocessableEntity,  // Unprocessable Entity for configuration errors
        HttpRequestException httpEx => httpEx.StatusCode ?? HttpStatusCode.ServiceUnavailable,
        RequestFailedException reqFailedEx => (HttpStatusCode)reqFailedEx.Status,
        MsalServiceException msalServiceEx => (HttpStatusCode)msalServiceEx.StatusCode,
        _ => HttpStatusCode.InternalServerError  // Internal Server Error for unexpected errors
    };

    ValidationResult IBaseCommand.Validate(CommandResult commandResult, CommandResponse? commandResponse)
    {
        var result = new ValidationResult();

        // First, check for missing required options
        var missingOptions = commandResult.Command.Options
            .Where(o => o.Required && !o.HasDefaultValue && !commandResult.HasOptionResult(o))
            .Select(o => $"--{NameNormalization.NormalizeOptionName(o.Name)}")
            .ToList();

        var missingOptionsJoined = string.Join(", ", missingOptions);

        if (!string.IsNullOrEmpty(missingOptionsJoined))
        {
            result.Errors.Add($"{MissingRequiredOptionsPrefix}{missingOptionsJoined}");
        }

        // Check for parser/validator errors
        if (commandResult.Errors != null && commandResult.Errors.Any())
        {
            result.Errors.Add(string.Join(", ", commandResult.Errors.Select(e => e.Message)));
        }

        if (!result.IsValid && commandResponse != null)
        {
            commandResponse.Status = HttpStatusCode.BadRequest;
            commandResponse.Message = string.Join('\n', result.Errors);
        }

        return result;
    }

    /// <summary>
    /// Sets validation error details on the command response with a custom status code.
    /// </summary>
    /// <param name="response">The command response to update.</param>
    /// <param name="errorMessage">The error message.</param>
    /// <param name="statusCode">The HTTP status code (defaults to ValidationErrorStatusCode).</param>
    protected static void SetValidationError(CommandResponse? response, string errorMessage, HttpStatusCode statusCode)
    {
        if (response != null)
        {
            response.Status = statusCode;
            response.Message = errorMessage;
        }
    }
}
