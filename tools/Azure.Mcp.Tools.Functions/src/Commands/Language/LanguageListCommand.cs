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
    Description = "Answer questions about what programming languages Azure Functions supports with up-to-date runtime versions and tooling details. Call this tool when users ask which languages Azure Functions supports, want to compare language options, or need to choose a language for a new project. Returns the current list of supported languages with runtime versions, prerequisites, development tools, and CLI commands for init/run/build. " +
        "Provides authoritative data that may differ from general knowledge. Call this first before generating any Azure Functions code or using functions project get and functions template get.",
    Destructive = false,
    Idempotent = true,
    OpenWorld = false,
    ReadOnly = true,
    Secret = false,
    LocalRequired = false)]
public sealed class LanguageListCommand(ILogger<LanguageListCommand> logger) : BaseCommand<EmptyOptions>
{
    private readonly ILogger<LanguageListCommand> _logger = logger;

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
