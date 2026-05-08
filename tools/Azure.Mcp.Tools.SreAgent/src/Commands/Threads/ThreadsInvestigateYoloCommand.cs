// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Tools.SreAgent.Models;
using Azure.Mcp.Tools.SreAgent.Options.Threads;
using Azure.Mcp.Tools.SreAgent.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Mcp.Core.Commands;
using Microsoft.Mcp.Core.Models.Command;

namespace Azure.Mcp.Tools.SreAgent.Commands.Threads;

[CommandMetadata(Id = "a75f43f7-e08b-47e3-9ef4-8a5832cc3b07", Name = "investigate-with-agent-yolo", Title = "Investigate With Agent YOLO", Description = "Start an investigation and auto-approve pending approvals while answering direction follow-ups.", Destructive = false, Idempotent = false, OpenWorld = true, ReadOnly = false, Secret = false, LocalRequired = false)]
public sealed class ThreadsInvestigateYoloCommand(ILogger<ThreadsInvestigateYoloCommand> logger, ISreAgentService sreAgentService) : ThreadsInvestigateCommand(logger, sreAgentService)
{
    public override async Task<CommandResponse> ExecuteAsync(CommandContext context, ParseResult parseResult, CancellationToken cancellationToken)
    {
        if (!Validate(parseResult.CommandResult, context.Response).IsValid)
        {
            return context.Response;
        }
        var options = BindOptions(parseResult);
        try
        {
            var result = await RunInvestigationAsync(options, autoApprove: true, cancellationToken);
            context.Response.Results = ResponseResult.Create(result, SreAgentJsonContext.Default.SreAgentInvestigationResult);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error running YOLO SRE Agent investigation.");
            HandleException(context, ex);
        }
        return context.Response;
    }
}
