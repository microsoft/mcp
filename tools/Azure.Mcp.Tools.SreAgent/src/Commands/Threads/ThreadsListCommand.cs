// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Tools.SreAgent.Models;
using Azure.Mcp.Tools.SreAgent.Options.Threads;
using Azure.Mcp.Tools.SreAgent.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Mcp.Core.Commands;
using Microsoft.Mcp.Core.Models.Command;

namespace Azure.Mcp.Tools.SreAgent.Commands.Threads;

[CommandMetadata(Id = "23b7c0d6-29c5-4d8f-82de-1e0edc1a9b01", Name = "list", Title = "List Threads", Description = "List SRE Agent chat threads.", Destructive = false, Idempotent = true, OpenWorld = false, ReadOnly = true, Secret = false, LocalRequired = false)]
public sealed class ThreadsListCommand(ILogger<ThreadsListCommand> logger, ISreAgentService sreAgentService) : SreAgentDataPlaneCommand<ThreadsListOptions>
{
    private readonly ILogger<ThreadsListCommand> _logger = logger;
    private readonly ISreAgentService _sreAgentService = sreAgentService;

    public override async Task<CommandResponse> ExecuteAsync(CommandContext context, ParseResult parseResult, CancellationToken cancellationToken)
    {
        if (!Validate(parseResult.CommandResult, context.Response).IsValid)
        {
            return context.Response;
        }
        var options = BindOptions(parseResult);
        try
        {
            var endpoint = await ResolveEndpointAsync(_sreAgentService, options, cancellationToken);
            var threads = await _sreAgentService.ListThreadsAsync(endpoint, options.Tenant, cancellationToken);
            context.Response.Results = ResponseResult.Create(new ThreadsListCommandResult(threads), SreAgentJsonContext.Default.ThreadsListCommandResult);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error listing SRE Agent threads.");
            HandleException(context, ex);
        }
        return context.Response;
    }

    internal record ThreadsListCommandResult(List<SreAgentThread> Threads);
}
