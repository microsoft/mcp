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
using Microsoft.Mcp.Core.Extensions;
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
        "Generate Azure Functions code from templates including triggers, bindings, AI agents, Durable Functions, and MCP servers or list available templates. " +
        "Use for code generation for serverless functions with triggers and bindings. " +
        "Without --template, lists available templates. " +
        "With --template, generates function code with the specified trigger and optional input/output bindings. " +
        "Select one trigger (required) and zero or more bindings. " +
        "Use after functions language list and functions project get.";

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

        command.Validators.Add(commandResult =>
        {
            var language = commandResult.GetValueWithoutDefault<string>(FunctionsOptionDefinitions.Language.Name);
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
            var service = context.GetService<IFunctionsService>();

            if (string.IsNullOrEmpty(options.Template))
            {
                // List mode: return all templates grouped by binding type
                var templateList = await service.GetTemplateListAsync(options.Language!, cancellationToken);

                context.Response.Status = HttpStatusCode.OK;
                context.Response.Results = ResponseResult.Create(
                    new(TemplateList: templateList, FunctionTemplate: null),
                    FunctionsJsonContext.Default.TemplateGetCommandResult);
                context.Response.Message = string.Empty;
            }
            else
            {
                // Get mode: fetch specific template files
                var functionTemplate = await service.GetFunctionTemplateAsync(
                    options.Language!, options.Template, options.RuntimeVersion, cancellationToken);

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
