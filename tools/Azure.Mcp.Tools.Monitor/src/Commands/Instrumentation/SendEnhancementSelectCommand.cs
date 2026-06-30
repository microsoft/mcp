// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net;
using Azure.Mcp.Tools.Monitor.Options.Instrumentation;
using Azure.Mcp.Tools.Monitor.Tools.Instrumentation;
using Microsoft.Extensions.Logging;
using Microsoft.Mcp.Core.Commands;
using Microsoft.Mcp.Core.Models.Command;

namespace Azure.Mcp.Tools.Monitor.Commands.Instrumentation;

[CommandMetadata(
    Id = "8fd4eb5f-14d1-450f-982c-82d761f0f7d6",
    Name = "send-enhancement-select",
    Title = "Send Enhancement Selection",
    Description = """
        Submit the user's enhancement selection after orchestrator-start returned status 'enhancement_available'.
        Present the enhancement options to the user first, then call this tool with their chosen option key(s).
        Multiple enhancements can be selected by passing a comma-separated list (e.g. 'redis,processors').
        After this call succeeds, continue with orchestrator-next as usual.
        """,
    Destructive = false,
    Idempotent = false,
    OpenWorld = false,
    ReadOnly = false,
    Secret = false,
    LocalRequired = true)]
public sealed class SendEnhancementSelectCommand(ILogger<SendEnhancementSelectCommand> logger)
    : BaseCommand<SendEnhancementSelectOptions, string>
{
    private readonly ILogger<SendEnhancementSelectCommand> _logger = logger;

    public override Task<CommandResponse> ExecuteAsync(CommandContext context, SendEnhancementSelectOptions options, CancellationToken cancellationToken)
    {
        try
        {
            var result = SendEnhancementSelectTool.Send(options.SessionId, options.EnhancementKeys);

            context.Response.Status = HttpStatusCode.OK;
            context.Response.Results = ResponseResult.Create(result, MonitorJsonContext.Default.String);
            context.Response.Message = string.Empty;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in {Operation}. SessionId: {SessionId}", Name, options.SessionId);
            HandleException(context, ex);
        }

        return Task.FromResult(context.Response);
    }
}
