// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net;
using Azure.Mcp.Core.Commands;
using Azure.Mcp.Tools.FunctionsTemplate.Models;
using Azure.Mcp.Tools.FunctionsTemplate.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Mcp.Core.Commands;
using Microsoft.Mcp.Core.Models.Command;

namespace Azure.Mcp.Tools.FunctionsTemplate.Commands.Language;

public sealed class LanguageListCommand(ILogger<LanguageListCommand> logger) : BaseCommand<EmptyOptions>
{
    private readonly ILogger<LanguageListCommand> _logger = logger;

    public override string Id => "f7c8d9e0-a1b2-4c3d-8e5f-6a7b8c9d0e1f";

    public override string Name => "list";

    public override string Description =>
        "Returns the list of supported programming languages for Azure Functions with their " +
        "runtime versions, prerequisites, development tools, and init/run/build commands. " +
        "Use this tool when the user wants to know which languages Azure Functions supports, " +
        "compare language options, or get started with a specific language.";

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
            var service = context.GetService<IFunctionTemplatesService>();
            var result = await service.GetLanguageListAsync(cancellationToken);

            context.Response.Status = HttpStatusCode.OK;
            context.Response.Results = ResponseResult.Create(
                [result],
                FunctionTemplatesJsonContext.Default.ListLanguageListResult);
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
