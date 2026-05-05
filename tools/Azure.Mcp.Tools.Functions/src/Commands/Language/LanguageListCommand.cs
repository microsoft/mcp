// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net;
using Azure.Mcp.Tools.Functions.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Mcp.Core.Commands;
using Microsoft.Mcp.Core.Models.Command;

namespace Azure.Mcp.Tools.Functions.Commands.Language;

[CommandMetadata(
    Id = "f7c8d9e0-a1b2-4c3d-8e5f-6a7b8c9d0e1f",
    Name = "list",
    Title = "List Supported Languages",
    Description = "Answer questions about what programming languages Azure Functions supports with up-to-date runtime versions and tooling details. Returns the current list of supported languages with runtime versions, prerequisites, development tools, and CLI commands for init/run/build. " +
        "Provides authoritative data that may differ from general knowledge. Call this tool first when users ask about Azure Functions languages or before generating code with functions_project_get or functions_template_get.",
    Destructive = false,
    Idempotent = true,
    OpenWorld = false,
    ReadOnly = true,
    Secret = false,
    LocalRequired = false)]
public sealed class LanguageListCommand(ILogger<LanguageListCommand> logger, IFunctionsService functionsService) : BaseCommand<EmptyOptions>
{
    private readonly ILogger<LanguageListCommand> _logger = logger;
    private readonly IFunctionsService _functionsService = functionsService;

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
            var result = await _functionsService.GetLanguageListAsync(cancellationToken);

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
