// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.CommandLine.Parsing;
using System.Diagnostics;
using System.Net;
using Azure.Mcp.Core.Exceptions;
using Azure.Mcp.Core.Helpers;
using static Azure.Mcp.Core.Services.Telemetry.TelemetryConstants;

namespace Azure.Mcp.Core.Commands;

public abstract class BaseCommand<TOptions> : IBaseCommand where TOptions : class, new()
{
    private const string MissingRequiredOptionsPrefix = "Missing Required options: ";
    private const string TroubleshootingUrl = "https://aka.ms/azmcp/troubleshooting";

    private readonly Command _command;

    protected BaseCommand()
    {
        _command = new Command(Name, Description);
        RegisterOptions(_command);
    }

    public Command GetCommand() => _command;

    public abstract string Name { get; }
    public abstract string Description { get; }
    public abstract string Title { get; }
    public abstract ToolMetadata Metadata { get; }

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

    public abstract Task<CommandResponse> ExecuteAsync(CommandContext context, ParseResult parseResult);

    protected virtual void HandleException(CommandContext context, Exception ex)
    {
        context.Activity?.SetStatus(ActivityStatusCode.Error)?.AddTag(TagName.ErrorDetails, ex.Message);

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
            response.Results = null;
            return;
        }

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
        response.Results = ResponseResult.Create(result, JsonSourceGenerationContext.Default.ExceptionResult);
    }

    protected virtual string GetErrorMessage(Exception ex) => ex.Message;

    protected virtual HttpStatusCode GetStatusCode(Exception ex) => ex switch
    {
        ArgumentException => HttpStatusCode.BadRequest,  // Bad Request for invalid arguments
        InvalidOperationException => HttpStatusCode.UnprocessableEntity,  // Unprocessable Entity for configuration errors
        _ => HttpStatusCode.InternalServerError  // Internal Server Error for unexpected errors
    };

    public virtual ValidationResult Validate(CommandResult commandResult, CommandResponse? commandResponse = null)
    {
        var result = new ValidationResult { IsValid = true };

        // First, check for missing required options
        var missingOptions = commandResult.Command.Options
            .Where(o => o.Required && !o.HasDefaultValue && !commandResult.HasOptionResult(o))
            .Select(o => $"--{NameNormalization.NormalizeOptionName(o.Name)}")
            .ToList();

        var missingOptionsJoined = string.Join(", ", missingOptions);

        if (!string.IsNullOrEmpty(missingOptionsJoined))
        {
            result.IsValid = false;
            result.ErrorMessage = $"{MissingRequiredOptionsPrefix}{missingOptionsJoined}";
            SetValidationError(commandResponse, result.ErrorMessage!);
            return result;
        }

        // Check for parser/validator errors
        if (commandResult.Errors != null && commandResult.Errors.Any())
        {
            result.IsValid = false;
            var combined = string.Join(", ", commandResult.Errors.Select(e => e.Message));
            result.ErrorMessage = combined;
            SetValidationError(commandResponse, result.ErrorMessage);
            return result;
        }

        return result;

        static void SetValidationError(CommandResponse? response, string errorMessage)
        {
            if (response != null)
            {
                response.Status = HttpStatusCode.BadRequest;
                response.Message = errorMessage;
            }
        }
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

internal record ExceptionResult(
    string Message,
    string? StackTrace,
    string Type);
