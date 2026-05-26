// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Tools.SreAgent.Models;
using Azure.Mcp.Tools.SreAgent.Options;
using Azure.Mcp.Tools.SreAgent.Options.Threads;
using Azure.Mcp.Tools.SreAgent.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Mcp.Core.Commands;
using Microsoft.Mcp.Core.Extensions;
using Microsoft.Mcp.Core.Models.Command;

namespace Azure.Mcp.Tools.SreAgent.Commands.Threads;

[CommandMetadata(Id = "7c86f73c-bd69-4bb9-908a-d4a02d9f6805", Name = "delete", Title = "Delete Thread", Description = "Delete an SRE Agent thread. Requires confirm=true.", Destructive = true, Idempotent = false, OpenWorld = false, ReadOnly = false, Secret = false, LocalRequired = false)]
public sealed class ThreadsDeleteCommand(ILogger<ThreadsDeleteCommand> logger, ISreAgentService sreAgentService) : ThreadsCommandBase<ThreadsDeleteOptions>
{
    private readonly ILogger<ThreadsDeleteCommand> _logger = logger;
    private readonly ISreAgentService _sreAgentService = sreAgentService;

    protected override void RegisterOptions(Command command)
    {
        base.RegisterOptions(command);
        command.Options.Add(SreAgentOptionDefinitions.ThreadId);
        command.Options.Add(SreAgentOptionDefinitions.Confirm);
    }

    protected override ThreadsDeleteOptions BindOptions(ParseResult parseResult)
    {
        var options = base.BindOptions(parseResult);
        options.ThreadId = parseResult.GetValueOrDefault(SreAgentOptionDefinitions.ThreadId);
        options.Confirm = parseResult.GetValueOrDefault(SreAgentOptionDefinitions.Confirm);
        return options;
    }

    public override async Task<CommandResponse> ExecuteAsync(CommandContext context, ParseResult parseResult, CancellationToken cancellationToken)
    {
        if (!Validate(parseResult.CommandResult, context.Response).IsValid)
        {
            return context.Response;
        }
        var options = BindOptions(parseResult);
        try
        {
            if (!options.Confirm)
                throw new InvalidOperationException("Deleting a thread requires --confirm true.");
            var endpoint = await ResolveEndpointAsync(_sreAgentService, options, cancellationToken);
            await _sreAgentService.DeleteThreadAsync(endpoint, options.ThreadId!, options.Tenant, cancellationToken);
            context.Response.Results = ResponseResult.Create(new SreAgentThreadOperationResult(options.ThreadId, "deleted", []), SreAgentJsonContext.Default.SreAgentThreadOperationResult);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting SRE Agent thread.");
            HandleException(context, ex);
        }
        return context.Response;
    }
}
