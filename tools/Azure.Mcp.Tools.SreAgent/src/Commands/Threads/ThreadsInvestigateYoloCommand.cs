// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Core.Commands.Subscription;
using Azure.Mcp.Core.Services.Azure.Subscription;
using Azure.Mcp.Tools.SreAgent.Models;
using Azure.Mcp.Tools.SreAgent.Options.Threads;
using Azure.Mcp.Tools.SreAgent.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Mcp.Core.Commands;
using Microsoft.Mcp.Core.Models.Command;

namespace Azure.Mcp.Tools.SreAgent.Commands.Threads;

[CommandMetadata(Id = "a75f43f7-e08b-47e3-9ef4-8a5832cc3b07", Name = "investigate_yolo", Title = "Investigate With Agent YOLO", Description = "Start an investigation and auto-approve pending approvals while answering direction follow-ups.", Destructive = false, Idempotent = false, OpenWorld = true, ReadOnly = false, Secret = false, LocalRequired = false)]
public sealed class ThreadsInvestigateYoloCommand(ILogger<ThreadsInvestigateYoloCommand> logger, ISreAgentService sreAgentService, ISubscriptionResolver subscriptionResolver) : ThreadsInvestigateCommand(logger, sreAgentService, subscriptionResolver)
{
    public override async Task<CommandResponse> ExecuteAsync(CommandContext context, ThreadsInvestigateOptions options, CancellationToken cancellationToken)
    {
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
