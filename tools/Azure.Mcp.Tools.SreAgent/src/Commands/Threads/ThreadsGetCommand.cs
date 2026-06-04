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

[CommandMetadata(
    Id = "efab1704-5543-496a-830d-19ddb816a102",
    Name = "get",
    Title = "Get Thread",
    Description = "Get messages for an SRE Agent thread.",
    Destructive = false,
    Idempotent = true,
    OpenWorld = false,
    ReadOnly = true,
    Secret = false,
    LocalRequired = false)]
public sealed class ThreadsGetCommand(ILogger<ThreadsGetCommand> logger, ISreAgentService sreAgentService) : SreAgentDataPlaneCommand<ThreadsGetOptions>
{
    private readonly ILogger<ThreadsGetCommand> _logger = logger;
    private readonly ISreAgentService _sreAgentService = sreAgentService;

    protected override void RegisterOptions(Command command)
    {
        base.RegisterOptions(command);
        command.Options.Add(SreAgentOptionDefinitions.ThreadId);
    }

    protected override ThreadsGetOptions BindOptions(ParseResult parseResult)
    {
        var options = base.BindOptions(parseResult);
        options.ThreadId = parseResult.GetValueOrDefault(SreAgentOptionDefinitions.ThreadId);
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
            var endpoint = await ResolveEndpointAsync(_sreAgentService, options, cancellationToken);
            var messages = await _sreAgentService.GetThreadMessagesAsync(endpoint, options.ThreadId!, options.Tenant, cancellationToken);
            context.Response.Results = ResponseResult.Create(new ThreadsGetCommandResult(options.ThreadId, messages), SreAgentJsonContext.Default.ThreadsGetCommandResult);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting SRE Agent thread.");
            HandleException(context, ex);
        }
        return context.Response;
    }

    internal record ThreadsGetCommandResult(string? ThreadId, List<SreAgentThreadMessage> Messages);
}
