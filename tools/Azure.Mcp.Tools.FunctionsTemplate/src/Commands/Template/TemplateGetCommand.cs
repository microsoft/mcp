// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net;
using Azure.Mcp.Core.Commands;
using Azure.Mcp.Core.Extensions;
using Azure.Mcp.Tools.FunctionsTemplate.Models;
using Azure.Mcp.Tools.FunctionsTemplate.Options;
using Azure.Mcp.Tools.FunctionsTemplate.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Mcp.Core.Commands;
using Microsoft.Mcp.Core.Models.Command;
using Microsoft.Mcp.Core.Models.Option;

namespace Azure.Mcp.Tools.FunctionsTemplate.Commands.Template;

internal record TemplateGetCommandResult(TemplateListResult? TemplateList, FunctionTemplateResult? FunctionTemplate);

public sealed class TemplateGetCommand(ILogger<TemplateGetCommand> logger) : BaseCommand<TemplateGetOptions>
{
    private readonly ILogger<TemplateGetCommand> _logger = logger;

    public override string Id => "c3d4e5f6-a7b8-9012-cdef-234567890123";

    public override string Name => "get";

    public override string Description =>
        "List all available function templates for a language or get the complete source code for a specific " +
        "template. Without --template, returns all templates grouped by binding type (triggers, inputs, outputs). " +
        "With --template, fetches the actual template files from GitHub including function code and project " +
        "configuration files with merge instructions. Supply an optional runtimeVersion for Java or TypeScript " +
        "to auto-replace template placeholders.";

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
        command.Options.Add(FunctionTemplatesOptionDefinitions.Language);
        command.Options.Add(FunctionTemplatesOptionDefinitions.Template.AsOptional());
        command.Options.Add(FunctionTemplatesOptionDefinitions.RuntimeVersion);
    }

    protected override TemplateGetOptions BindOptions(ParseResult parseResult)
    {
        return new TemplateGetOptions
        {
            Language = parseResult.GetValueOrDefault<string>(FunctionTemplatesOptionDefinitions.Language.Name),
            Template = parseResult.GetValueOrDefault<string>(FunctionTemplatesOptionDefinitions.Template.Name),
            RuntimeVersion = parseResult.GetValueOrDefault<string>(FunctionTemplatesOptionDefinitions.RuntimeVersion.Name)
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

            var service = context.GetService<IFunctionTemplatesService>();

            if (string.IsNullOrEmpty(options.Template))
            {
                // List mode: return all templates grouped by binding type
                var templateList = await service.GetTemplateListAsync(options.Language, cancellationToken);

                context.Response.Status = HttpStatusCode.OK;
                context.Response.Results = ResponseResult.Create(
                    new TemplateGetCommandResult(TemplateList: templateList, FunctionTemplate: null),
                    FunctionTemplatesJsonContext.Default.TemplateGetCommandResult);
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
                    FunctionTemplatesJsonContext.Default.TemplateGetCommandResult);
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
