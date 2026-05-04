// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net;
using Azure.Mcp.Tools.Functions.Models;
using Azure.Mcp.Tools.Functions.Options;
using Azure.Mcp.Tools.Functions.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Mcp.Core.Commands;
using Microsoft.Mcp.Core.Extensions;
using Microsoft.Mcp.Core.Models.Command;
using Microsoft.Mcp.Core.Models.Option;

namespace Azure.Mcp.Tools.Functions.Commands.Template;

internal record TemplateGetCommandResult(TemplateListResult? TemplateList, FunctionTemplateResult? FunctionTemplate);

[CommandMetadata(
    Id = "c3d4e5f6-a7b8-9012-cdef-234567890123",
    Name = "get",
    Title = "Get Function Template",
    Description = "Lists available Azure Functions templates or generates function code for Timer (cron schedules), HTTP, Blob, Queue, Event Hub, Cosmos DB, Service Bus, Durable, event-driven, and MCP tool triggers with input and output bindings, orchestrations, and serverless infrastructure. " +
        "Create trigger functions, activity functions, or MCP server functions in C#, Python, JavaScript, TypeScript, Java, or PowerShell. " +
        "Without --template, lists all available triggers, bindings, and templates for the selected language. With --template, generates function code files with azd infrastructure support (Bicep, Terraform, ARM). " +
        "Select one trigger (required) and zero or more input or output bindings.",
    Destructive = false,
    Idempotent = true,
    OpenWorld = false,
    ReadOnly = true,
    Secret = false,
    LocalRequired = false)]
public sealed class TemplateGetCommand(ILogger<TemplateGetCommand> logger, IFunctionsService functionsService) : BaseCommand<TemplateGetOptions>
{
    private readonly ILogger<TemplateGetCommand> _logger = logger;
    private readonly IFunctionsService _functionsService = functionsService;

    protected override void RegisterOptions(Command command)
    {
        base.RegisterOptions(command);
        command.Options.Add(FunctionsOptionDefinitions.Language);
        command.Options.Add(FunctionsOptionDefinitions.Template.AsOptional());
        command.Options.Add(FunctionsOptionDefinitions.RuntimeVersion);
        command.Options.Add(FunctionsOptionDefinitions.Output);

        command.Validators.Add(commandResult =>
        {
            var language = commandResult.GetValueWithoutDefault(FunctionsOptionDefinitions.Language);
            if (string.IsNullOrWhiteSpace(language))
            {
                commandResult.AddError("The --language parameter is required.");
            }
            else if (!FunctionsOptionDefinitions.SupportedLanguages.Contains(language))
            {
                commandResult.AddError($"Invalid language '{language}'. Supported languages: {string.Join(", ", FunctionsOptionDefinitions.SupportedLanguages)}.");
            }
        });
    }

    protected override TemplateGetOptions BindOptions(ParseResult parseResult)
    {
        return new TemplateGetOptions
        {
            Language = parseResult.GetValueOrDefault<string>(FunctionsOptionDefinitions.Language.Name),
            Template = parseResult.GetValueOrDefault<string>(FunctionsOptionDefinitions.Template.Name),
            RuntimeVersion = parseResult.GetValueOrDefault<string>(FunctionsOptionDefinitions.RuntimeVersion.Name),
            Output = parseResult.GetValueOrDefault<TemplateOutput>(FunctionsOptionDefinitions.Output.Name)
        };
    }

    public override async Task<CommandResponse> ExecuteAsync(
        CommandContext context,
        ParseResult parseResult,
        CancellationToken cancellationToken)
    {
        if (!Validate(parseResult.CommandResult, context.Response).IsValid)
        {
            return context.Response;
        }

        var options = BindOptions(parseResult);

        try
        {
            if (string.IsNullOrEmpty(options.Template))
            {
                // List mode: return all templates grouped by binding type
                var templateList = await _functionsService.GetTemplateListAsync(options.Language!, cancellationToken);

                context.Response.Status = HttpStatusCode.OK;
                context.Response.Results = ResponseResult.Create(
                    new(TemplateList: templateList, FunctionTemplate: null),
                    FunctionsJsonContext.Default.TemplateGetCommandResult);
                context.Response.Message = string.Empty;
            }
            else
            {
                // Get mode: fetch specific template files
                var functionTemplate = await _functionsService.GetFunctionTemplateAsync(
                    options.Language!, options.Template, options.RuntimeVersion, options.Output, cancellationToken);

                context.Response.Status = HttpStatusCode.OK;
                context.Response.Results = ResponseResult.Create(
                    new(TemplateList: null, FunctionTemplate: functionTemplate),
                    FunctionsJsonContext.Default.TemplateGetCommandResult);
                context.Response.Message = string.Empty;
            }
        }
        catch (Exception ex)
        {
            if (string.IsNullOrEmpty(options.Template))
            {
                _logger.LogError(ex, "Error listing templates for Language: {Language}", options.Language);
            }
            else
            {
                _logger.LogError(ex, "Error getting template {Template} for Language: {Language}",
                    options.Template, options.Language);
            }
            HandleException(context, ex);
        }

        return context.Response;
    }
}
