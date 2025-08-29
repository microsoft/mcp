// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.


using System.CommandLine.Parsing;
using System.Diagnostics;
using Azure.Mcp.Core.Models.Option;
using static Azure.Mcp.Core.Services.Telemetry.TelemetryConstants;

namespace Azure.Mcp.Core.Commands;

public abstract class BaseCommand : IBaseCommand
{
    private const string MissingRequiredOptionsPrefix = "Missing Required options: ";
    private const int ValidationErrorStatusCode = 400;
    private const string TroubleshootingUrl = "https://aka.ms/azmcp/troubleshooting";

    private readonly Command _command;
    private bool _usesResourceGroup;
    private bool _requiresResourceGroup;

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

        // Map System.CommandLine "Option '--x' is required." exceptions to a 400 validation response
        // with a standardized missing-options message so callers don't receive a 500.
        if (ex is InvalidOperationException && ex.Message != null && ex.Message.IndexOf("is required", StringComparison.OrdinalIgnoreCase) >= 0)
        {
            try
            {
                // Extract option tokens like --cluster from the exception message
                var missing = System.Text.RegularExpressions.Regex.Matches(ex.Message, "--[A-Za-z0-9\\-]+")
                    .Select(m => m.Value)
                    .Where(s => !string.IsNullOrWhiteSpace(s))
                    .ToList();

                if (missing.Count > 0)
                {
                    response.Status = ValidationErrorStatusCode;
                    response.Message = $"{MissingRequiredOptionsPrefix}{string.Join(", ", missing)}";
                    response.Results = null;
                    return;
                }
            }
            catch
            {
                // Fall through to default behavior if parsing fails
            }
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
            .Where(o => o.Required && !HasOptionResult(commandResult, o))
            .Where(o => !o.HasDefaultValue)
            .Select(o => $"--{NormalizeName(o.Name)}")
            .ToList();

        if (missingOptions.Count > 0)
        {
            result.IsValid = false;
            result.ErrorMessage = $"{MissingRequiredOptionsPrefix}{string.Join(", ", missingOptions)}";
            SetValidationError(commandResponse, result.ErrorMessage!);
            return result;
        }

        // If no missing required options, check for other parse/validation errors produced
        // by the parser or command validators. System.CommandLine sometimes reports missing
        // required options via commandResult.Errors (e.g., "Option '--cluster' is required.")
        // — detect that case and return a standardized missing-options message instead of
        // surfacing raw errors.
        if (commandResult.Errors != null && commandResult.Errors.Any())
        {
            // Look for "is required" style messages and extract option tokens like "--cluster"
            var requiredFromErrors = new List<string>();
            foreach (var e in commandResult.Errors)
            {
                var msg = e.Message ?? string.Empty;
                if (msg.IndexOf("is required", StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    try
                    {
                        var matches = System.Text.RegularExpressions.Regex.Matches(msg, "--[A-Za-z0-9\\-]+");
                        foreach (System.Text.RegularExpressions.Match m in matches)
                        {
                            var token = m.Value;
                            if (!string.IsNullOrWhiteSpace(token) && !requiredFromErrors.Contains(token))
                                requiredFromErrors.Add(token);
                        }
                    }
                    catch
                    {
                        // ignore parse failures and continue
                    }
                }
            }

            if (requiredFromErrors.Count > 0)
            {
                result.IsValid = false;
                result.ErrorMessage = $"{MissingRequiredOptionsPrefix}{string.Join(", ", requiredFromErrors)}";
                SetValidationError(commandResponse, result.ErrorMessage);
                return result;
            }

            // Fall back to propagating parser/validator errors as before
            result.IsValid = false;
            var combined = string.Join(", ", commandResult.Errors.Select(e => e.Message));
            result.ErrorMessage = combined;
            SetValidationError(commandResponse, result.ErrorMessage);
            return result;
        }

        // Check logical requirements (e.g., resource group requirement)
        if (result.IsValid && _requiresResourceGroup)
        {
            var hasRg = HasOptionResult(commandResult, OptionDefinitions.Common.ResourceGroup);
            if (!hasRg)
            {
                result.IsValid = false;
                result.ErrorMessage = $"{MissingRequiredOptionsPrefix}--resource-group";
                SetValidationError(commandResponse, result.ErrorMessage);
            }
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

    private static string NormalizeName(string? name) => (name ?? string.Empty).TrimStart('-', '/');

    private static bool HasOptionResult(CommandResult commandResult, Option option)
    {
        var normalizedOptionName = NormalizeName(option.Name);

        return commandResult.Children
            .OfType<System.CommandLine.Parsing.OptionResult>()
            .Any(or =>
            {
                // Treat option as present only if it has tokens (a provided value)
                if (or.Tokens == null || or.Tokens.Count == 0)
                    return false;
                // If all tokens are empty/whitespace treat the option as not provided
                if (or.Tokens.All(t => string.IsNullOrWhiteSpace(t.Value)))
                    return false;
                // OptionResult.Option may have a name and aliases; compare normalized names/aliases
                var orName = NormalizeName(or.Option.Name);
                if (string.Equals(orName, normalizedOptionName, StringComparison.OrdinalIgnoreCase))
                    return true;

                if (or.Option.Aliases != null && or.Option.Aliases.Any(a => string.Equals(NormalizeName(a), normalizedOptionName, StringComparison.OrdinalIgnoreCase)))
                    return true;

                // Also compare against the target option's aliases
                if (option.Aliases != null && option.Aliases.Any(a => string.Equals(NormalizeName(a), NormalizeName(or.Option.Name), StringComparison.OrdinalIgnoreCase)))
                    return true;

                return false;
            });
    }

    private static bool IsOptionValueMissing(object? value)
    {
        return value == null || (value is string str && string.IsNullOrWhiteSpace(str));
    }

    protected void UseResourceGroup()
    {
        if (_usesResourceGroup)
            return;
        _usesResourceGroup = true;
        _command.Options.Add(OptionDefinitions.Common.ResourceGroup);
    }

    protected void RequireResourceGroup()
    {
        UseResourceGroup();
        _requiresResourceGroup = true;
    }

    protected string? GetResourceGroup(ParseResult parseResult) =>
        _usesResourceGroup ? parseResult.TryGetValue(OptionDefinitions.Common.ResourceGroup, out string? rg) ? rg : null : null;

    protected bool UsesResourceGroup => _usesResourceGroup;
}
