// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net;
using Azure.Mcp.Core.Commands;
using Azure.Mcp.Core.Extensions;
using Azure.Mcp.Tools.Functions.Models;
using Azure.Mcp.Tools.Functions.Options;
using Azure.Mcp.Tools.Functions.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Mcp.Core.Commands;
using Microsoft.Mcp.Core.Models.Command;
using Microsoft.Mcp.Core.Models.Option;

namespace Azure.Mcp.Tools.Functions.Commands.Template;

internal record TemplateGetCommandResult(TemplateListResult? TemplateList, FunctionTemplateResult? FunctionTemplate);

public sealed class TemplateGetCommand(ILogger<TemplateGetCommand> logger) : BaseCommand<TemplateGetOptions>
{
    private readonly ILogger<TemplateGetCommand> _logger = logger;

    public override string Id => "c3d4e5f6-a7b8-9012-cdef-234567890123";

    public override string Name => "get";

    public override string Description =>
        """
        List available Azure Functions templates or generate complete function code for a specific template.

        USE FOR: Code generation for serverless functions, building HTTP APIs, scheduled jobs (cron), message processors, event-driven applications. Works for both new and existing Azure Functions projects.
        
        TWO MODES:
        1. LIST MODE (no --template): Returns all templates grouped by type (triggers, input bindings, output bindings) with descriptions.
        2. GET MODE (with --template): Generates complete, ready-to-use function code with merge instructions.

        RETURNS (LIST): Template names, descriptions, binding types, resource categories.
        RETURNS (GET): Generated function source code files, project config updates, merge instructions.

        PARAMETERS:
        --language (required): csharp, java, javascript, python, powershell, typescript
        --template (optional): Template name (e.g., HttpTrigger for REST APIs, TimerTrigger for scheduled/cron jobs, BlobTrigger for file processing, QueueTrigger for message handling, CosmosDBTrigger for database change feeds)
        --runtime-version (optional): For Java or TypeScript/JavaScript placeholder replacement. See 'functions language list' for supported versions.

        WORKFLOW: functions language list → functions project get → Start here

        USAGE (LIST MODE): Select 1 trigger (required) + 0 or more input/output bindings (optional) for your function.
        USAGE (GET MODE): Create the files in your project. If adding bindings, fetch additional templates and merge them.
        """;

    public override string Title => "Get Function Template";

    public override ToolMetadata Metadata => new()
    {
        Destructive = false,
        Idempotent = true,
        OpenWorld = false,
        ReadOnly = true,
        LocalRequired = false,
        Secret = false
    };

    protected override void RegisterOptions(Command command)
    {
        base.RegisterOptions(command);
        command.Options.Add(FunctionsOptionDefinitions.Language);
        command.Options.Add(FunctionsOptionDefinitions.Template.AsOptional());
        command.Options.Add(FunctionsOptionDefinitions.RuntimeVersion);
    }

    protected override TemplateGetOptions BindOptions(ParseResult parseResult)
    {
        return new TemplateGetOptions
        {
            Language = parseResult.GetValueOrDefault<string>(FunctionsOptionDefinitions.Language.Name),
            Template = parseResult.GetValueOrDefault<string>(FunctionsOptionDefinitions.Template.Name),
            RuntimeVersion = parseResult.GetValueOrDefault<string>(FunctionsOptionDefinitions.RuntimeVersion.Name)
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
            if (string.IsNullOrWhiteSpace(options.Language))
            {
                context.Response.Status = HttpStatusCode.BadRequest;
                context.Response.Message = "The language parameter is required.";
                return context.Response;
            }

            var service = context.GetService<IFunctionsService>();

            if (string.IsNullOrEmpty(options.Template))
            {
                // List mode: return all templates grouped by binding type
                var templateList = await service.GetTemplateListAsync(options.Language, cancellationToken);

                context.Response.Status = HttpStatusCode.OK;
                context.Response.Results = ResponseResult.Create(
                    new TemplateGetCommandResult(TemplateList: templateList, FunctionTemplate: null),
                    FunctionsJsonContext.Default.TemplateGetCommandResult);
                context.Response.Message = string.Empty;
            }
            else
            {
                // Get mode: fetch specific template files
                var functionTemplate = await service.GetFunctionTemplateAsync(
                    options.Language, options.Template, options.RuntimeVersion, cancellationToken);

                context.Response.Status = HttpStatusCode.OK;
                context.Response.Results = ResponseResult.Create(
                    new TemplateGetCommandResult(TemplateList: null, FunctionTemplate: functionTemplate),
                    FunctionsJsonContext.Default.TemplateGetCommandResult);
                context.Response.Message = string.Empty;
            }
        }
        catch (ArgumentException ex)
        {
            _logger.LogError(ex, "Invalid arguments for template get. Language: {Language}, Template: {Template}",
                options.Language, options.Template);
            context.Response.Status = HttpStatusCode.BadRequest;
            context.Response.Message = ex.Message;
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
