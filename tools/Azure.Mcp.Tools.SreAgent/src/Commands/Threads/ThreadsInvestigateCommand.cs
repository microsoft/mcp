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

[CommandMetadata(
    Id = "ab73d6fa-d53e-446c-9d4c-9d8cf41a3106",
    Name = "investigate",
    Title = "Investigate With Agent",
    Description = "Investigate an issue or incident using an SRE Agent. Sends your investigation message and automatically follows up on agent questions until the investigation is complete.",
    Destructive = false,
    Idempotent = false,
    OpenWorld = true,
    ReadOnly = false,
    Secret = false,
    LocalRequired = false)]
public sealed class ThreadsInvestigateCommand(ILogger<ThreadsInvestigateCommand> logger, ISreAgentService sreAgentService, ISubscriptionResolver subscriptionResolver)
    : SubscriptionCommand<ThreadsInvestigateOptions, SreAgentInvestigationResult>(subscriptionResolver)
{
    private readonly ILogger<ThreadsInvestigateCommand> _logger = logger;
    private readonly ISreAgentService _sreAgentService = sreAgentService;

    public override async Task<CommandResponse> ExecuteAsync(CommandContext context, ThreadsInvestigateOptions options, CancellationToken cancellationToken)
    {
        try
        {
            var result = await SreAgentService.RunInvestigationAsync(_sreAgentService, options, autoApprove: false, cancellationToken);
            context.Response.Results = ResponseResult.Create(result, SreAgentJsonContext.Default.SreAgentInvestigationResult);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error running SRE Agent investigation.");
            HandleException(context, ex);
        }
        return context.Response;
    }
}
