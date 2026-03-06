// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net;
using Azure.Mcp.Core.Commands;
using Azure.Mcp.Tools.Functions.Models;
using Azure.Mcp.Tools.Functions.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Mcp.Core.Commands;
using Microsoft.Mcp.Core.Models.Command;

namespace Azure.Mcp.Tools.Functions.Commands.Language;

public sealed class LanguageListCommand(ILogger<LanguageListCommand> logger) : BaseCommand<EmptyOptions>
{
    private readonly ILogger<LanguageListCommand> _logger = logger;

    public override string Id => "f7c8d9e0-a1b2-4c3d-8e5f-6a7b8c9d0e1f";

    public override string Name => "list";

    public override string Description =>
        """
        Get supported programming languages for Azure Functions development.

        USE FOR: Creating new Azure Functions apps, discovering available languages for serverless development, comparing runtime options, checking prerequisites before starting a project.
        RETURNS: Language details (runtime, programming model, prerequisites, dev tools, init/run/build commands), supported runtime versions per language, Functions runtime version, extension bundle version.
        DOES NOT RETURN: Project files or function templates - use 'functions project get' and 'functions template get' for those.

        RUNTIME VERSION NOTE: For Java and TypeScript/JavaScript, supply --runtime-version to later tools (project get, template get) to auto-replace template placeholders like {{javaVersion}} or {{nodeVersion}}.

        WORKFLOW: Start here → functions project get → functions template get
        NEXT STEP: After selecting a language, call 'functions project get --language <language>' to get project scaffold or initialization files.
        """;

    public override string Title => "List Supported Languages";

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
    }

    protected override EmptyOptions BindOptions(ParseResult parseResult) => new();

    public override async Task<CommandResponse> ExecuteAsync(
        CommandContext context,
        ParseResult parseResult,
        CancellationToken cancellationToken)
    {
        try
        {
            var service = context.GetService<IFunctionsService>();
            var result = await service.GetLanguageListAsync(cancellationToken);

            context.Response.Status = HttpStatusCode.OK;
            context.Response.Results = ResponseResult.Create(
                [result],
                FunctionsJsonContext.Default.ListLanguageListResult);
            context.Response.Message = string.Empty;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving supported languages list");
            HandleException(context, ex);
        }

        return context.Response;
    }
}
