// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.


using System.Diagnostics;
using static Microsoft.Mcp.Core.Services.Telemetry.TelemetryConstants;

namespace Microsoft.Mcp.Core.Commands;

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
        var result = new ExceptionResult(
            Message: ex.Message,
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

    public record ExceptionResult(
        string Message,
        string? StackTrace,
        string Type);

    protected virtual string GetErrorMessage(Exception ex) => ex.Message;

    protected virtual int GetStatusCode(Exception ex) => 500;

    public virtual ValidationResult Validate(CommandResult commandResult, CommandResponse? commandResponse = null)
    {
        var result = new ValidationResult { IsValid = true };

        var missingOptions = commandResult.Command.Options
            .Where(o => o.IsRequired && IsOptionValueMissing(commandResult.GetValueForOption(o)))
            .Select(o => $"--{o.Name}")
            .ToList();

        if (missingOptions.Count > 0 || !string.IsNullOrEmpty(commandResult.ErrorMessage))
        {
            result.IsValid = false;
            result.ErrorMessage = missingOptions.Count > 0
                ? $"{MissingRequiredOptionsPrefix}{string.Join(", ", missingOptions)}"
                : commandResult.ErrorMessage;

            SetValidationError(commandResponse, result.ErrorMessage!);
        }

        return result;
    }

    protected void SetValidationError(CommandResponse? response, string errorMessage)
    {
        if (response != null)
        {
            response.Status = ValidationErrorStatusCode;
            response.Message = errorMessage;
        }
    }

    private static bool IsOptionValueMissing(object? value)
    {
        return value == null || (value is string str && string.IsNullOrWhiteSpace(str));
    }
}
