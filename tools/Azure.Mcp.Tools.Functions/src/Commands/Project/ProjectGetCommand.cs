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

namespace Azure.Mcp.Tools.Functions.Commands.Project;

public sealed class ProjectGetCommand(ILogger<ProjectGetCommand> logger) : BaseCommand<ProjectGetOptions>
{
    private readonly ILogger<ProjectGetCommand> _logger = logger;

    public override string Id => "b2c3d4e5-f6a7-8901-bcde-f12345678901";

    public override string Name => "get";

    public override string Description =>
        """
        Get project scaffold files for code generation of a new Azure Functions app.

        USE FOR: Scaffolding a new serverless project, code generation for Azure Functions apps, getting host.json and local.settings.json, getting language-specific config files (requirements.txt, package.json, pom.xml, .csproj).
        RETURNS: Project template files with content (host.json, local.settings.json, language-specific files), setup instructions, project structure description.
        DOES NOT RETURN: Function code templates - use 'functions template get' for trigger/binding code generation.

        PARAMETERS:
        --language (required): csharp, java, javascript, python, powershell, typescript
        --runtime-version (optional): For Java or TypeScript/JavaScript to replace {{javaVersion}} or {{nodeVersion}} placeholders. See 'functions language list' for supported versions.

        WORKFLOW: functions language list → Start here → functions template get
        NEXT STEP: After creating project files, call 'functions template get --language <language>' to see available triggers and bindings.
        """;

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
        command.Options.Add(FunctionsOptionDefinitions.RuntimeVersion);
    }

    protected override ProjectGetOptions BindOptions(ParseResult parseResult)
    {
        return new ProjectGetOptions
        {
            Language = parseResult.GetValueOrDefault<string>(FunctionsOptionDefinitions.Language.Name),
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
            var result = await service.GetProjectTemplateAsync(options.Language, options.RuntimeVersion, cancellationToken);

            context.Response.Status = HttpStatusCode.OK;
            context.Response.Results = ResponseResult.Create(
                [result],
                FunctionsJsonContext.Default.ListProjectTemplateResult);
            context.Response.Message = string.Empty;
        }
        catch (ArgumentException ex)
        {
            _logger.LogError(ex, "Invalid arguments for project template. Language: {Language}, RuntimeVersion: {RuntimeVersion}",
                options.Language, options.RuntimeVersion);
            context.Response.Status = HttpStatusCode.BadRequest;
            context.Response.Message = ex.Message;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving project template for Language: {Language}", options.Language);
            HandleException(context, ex);
        }

        return context.Response;
    }
}
