// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.CommandLine.Parsing;
using System.Diagnostics;
using Azure.Mcp.Core.Exceptions;
using Azure.Mcp.Core.Helpers;
using static Azure.Mcp.Core.Services.Telemetry.TelemetryConstants;

namespace Azure.Mcp.Core.Commands;

public abstract class BaseCommand : IBaseCommand
{
    private const string MissingRequiredOptionsPrefix = "Missing Required options: ";
    private const int ValidationErrorStatusCode = 400;
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

    internal record ExceptionResult(
        string Message,
        string? StackTrace,
        string Type);

    protected virtual string GetErrorMessage(Exception ex) => ex.Message;

    protected virtual int GetStatusCode(Exception ex) => 500;

    public virtual ValidationResult Validate(CommandResult commandResult, CommandResponse? commandResponse = null)
    {
        var result = new ValidationResult { IsValid = true };

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

        // If no missing required options, propagate parser/validator errors as-is.
        // Commands can throw CommandValidationException for structured handling.
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
                response.Status = ValidationErrorStatusCode;
                response.Message = errorMessage;
            }
        }
    }
}
