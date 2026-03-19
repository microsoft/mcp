// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net;
using Azure.Mcp.Core.Extensions;
using Azure.Mcp.Tools.Functions.Options;
using Azure.Mcp.Tools.Functions.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Mcp.Core.Commands;
using Microsoft.Mcp.Core.Extensions;
using Microsoft.Mcp.Core.Models.Command;

namespace Azure.Mcp.Tools.Functions.Commands.Project;

public sealed class ProjectGetCommand(ILogger<ProjectGetCommand> logger) : BaseCommand<ProjectGetOptions>
{
    private readonly ILogger<ProjectGetCommand> _logger = logger;

    public override string Id => "b2c3d4e5-f6a7-8901-bcde-f12345678901";

    public override string Name => "get";

    public override string Description =>
        "Get project scaffolding information for a new Azure Functions app. " +
        "Use for getting project structure, setup instructions, and file list for initializing serverless projects. " +
        "Returns project structure overview and setup instructions that agents use to create files. " +
        "Use after functions language list and before functions template get.";

    public override string Title => "Get Project Template";

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

    protected override ProjectGetOptions BindOptions(ParseResult parseResult)
    {
        return new ProjectGetOptions
        {
            Language = parseResult.GetValueOrDefault<string>(FunctionsOptionDefinitions.Language.Name)
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
            var result = await service.GetProjectTemplateAsync(options.Language!, cancellationToken);

            context.Response.Status = HttpStatusCode.OK;
            context.Response.Results = ResponseResult.Create(
                [result],
                FunctionsJsonContext.Default.ListProjectTemplateResult);
            context.Response.Message = string.Empty;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving project template for Language: {Language}", options.Language);
            HandleException(context, ex);
        }

        return context.Response;
    }
}
