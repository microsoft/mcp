// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.CommandLine.Parsing;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Net;
using System.Reflection;
using System.Text.Json.Nodes;
using Azure;
using Azure.Mcp.Core.Areas.Server;
using Microsoft.Identity.Client;
using Microsoft.Mcp.Core.Extensions;
using Microsoft.Mcp.Core.Helpers;
using Microsoft.Mcp.Core.Models.Command;

namespace Microsoft.Mcp.Core.Commands;

public abstract class BaseCommand<TOptions> : IBaseCommand where TOptions : class, new()
{
    private const string MissingRequiredOptionsPrefix = "Missing Required options: ";
    private const string TroubleshootingUrl = "https://aka.ms/azmcp/troubleshooting";

    private readonly Command _command;

    [UnconditionalSuppressMessage("Trimming", "IL2075:UnrecognizedReflectionPattern",
        Justification = "CommandMetadataAttribute is only applied to concrete command types that are rooted by DI service registration.")]
    protected BaseCommand()
    {
        var attr = GetType().GetCustomAttribute<CommandMetadataAttribute>();
        if (attr is not null)
        {
            Id = attr.Id;
            Name = attr.Name;
            Description = attr.Description;
            Title = attr.Title;
            Metadata = attr.ToToolMetadata();
        }

        ValidateMetadataConfiguration();

        _command = new ExtendedCommand(this, Name, Description);
        RegisterOptions(_command);
    }

    public virtual string Id { get; protected set; } = null!;
    public virtual string Name { get; protected set; } = null!;
    public virtual string Description { get; protected set; } = null!;
    public virtual string Title { get; protected set; } = null!;
    public virtual ToolMetadata Metadata { get; protected set; } = null!;

    public Command GetCommand() => _command;

    protected virtual void RegisterOptions(Command command)
    {
    }

    /// <summary>
    /// Binds the parsed command line arguments to a strongly-typed options object.
    /// Implement this method in derived classes to provide option binding logic.
    /// </summary>
    /// <param name="parseResult">The parsed command line arguments.</param>
    /// <returns>An options object containing the bound options.</returns>
    protected abstract TOptions BindOptions(ParseResult parseResult);

    public abstract Task<CommandResponse> ExecuteAsync(CommandContext context, ParseResult parseResult, CancellationToken cancellationToken);

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
            // If specific missing options are provided, format a consistent message
            if (cve.MissingOptions is { Count: > 0 })
            {
                response.Message = $"{MissingRequiredOptionsPrefix}{string.Join(", ", cve.MissingOptions)}";
            }
            else
            {
                response.Message = cve.Message;
            }
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

    public ValidationResult Validate(CommandResult commandResult, CommandResponse? commandResponse = null)
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
            Activity.Current?.SetStatus(ActivityStatusCode.Error)
                ?.SetTag(TagName.ExceptionType, "ValidationError")
                ?.SetTag(TagName.ExceptionMessage, string.Join("; ", result.Errors));

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

    private void ValidateMetadataConfiguration()
    {
        if (!string.IsNullOrWhiteSpace(Id) &&
            !string.IsNullOrWhiteSpace(Name) &&
            !string.IsNullOrWhiteSpace(Description) &&
            !string.IsNullOrWhiteSpace(Title) &&
            Metadata is not null)
        {
            return;
        }

        throw new InvalidOperationException(
            $"Command type '{GetType().FullName}' is missing required command metadata. " +
            "Apply [CommandMetadata] to the command class or override Id, Name, Description, Title, and Metadata " +
            "with non-null values that are available during BaseCommand construction.");
    }
}

public record ExceptionResult(
    string Message,
    string? StackTrace,
    string Type);
