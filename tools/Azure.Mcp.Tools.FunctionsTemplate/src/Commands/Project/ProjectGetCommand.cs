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

namespace Azure.Mcp.Tools.FunctionsTemplate.Commands.Project;

public sealed class ProjectGetCommand(ILogger<ProjectGetCommand> logger) : BaseCommand<ProjectGetOptions>
{
    private readonly ILogger<ProjectGetCommand> _logger = logger;

    public override string Id => "b2c3d4e5-f6a7-8901-bcde-f12345678901";

    public override string Name => "get";

    public override string Description =>
        "Returns project initialization files for a new Azure Functions app, including host.json, " +
        "local.settings.json, and language-specific files (requirements.txt, package.json, pom.xml, " +
        ".csproj). Supply an optional runtimeVersion for Java or TypeScript to auto-replace template " +
        "placeholders like {{javaVersion}} or {{nodeVersion}}. Call this tool BEFORE writing function " +
        "code manually. Use the returned setup instructions and project structure to bootstrap a new " +
        "Azure Functions project.";

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
        command.Options.Add(FunctionTemplatesOptionDefinitions.Language);
        command.Options.Add(FunctionTemplatesOptionDefinitions.RuntimeVersion);
    }

    protected override ProjectGetOptions BindOptions(ParseResult parseResult)
    {
        return new ProjectGetOptions
        {
            Language = parseResult.GetValueOrDefault<string>(FunctionTemplatesOptionDefinitions.Language.Name),
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
            var result = await service.GetProjectTemplateAsync(options.Language, options.RuntimeVersion, cancellationToken);

            context.Response.Status = HttpStatusCode.OK;
            context.Response.Results = ResponseResult.Create(
                [result],
                FunctionTemplatesJsonContext.Default.ListProjectTemplateResult);
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
