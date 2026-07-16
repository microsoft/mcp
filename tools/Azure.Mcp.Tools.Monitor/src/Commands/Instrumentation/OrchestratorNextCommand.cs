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
    Id = "dd7d9a59-fb6d-436a-9e08-8bbdf6d5f9d5",
    Name = "orchestrator-next",
    Title = "Get Next Azure Monitor Instrumentation Action",
    Description = """
        Get the next instrumentation action after completing the current one.
        Call this ONLY after you have executed the EXACT instruction from the previous response.
        DO NOT skip steps. DO NOT improvise. DO NOT add extra code or commands.

        Expected workflow:
        1. You received an action from orchestrator-start or orchestrator-next
        2. You executed EXACTLY what the 'instruction' field told you to do
        3. Now call this tool to get the next action

        Returns: The next action to execute, or 'complete' status when all steps are done.
        """,
    Destructive = false,
    Idempotent = false,
    OpenWorld = false,
    ReadOnly = false,
    Secret = false,
    LocalRequired = true)]
public sealed class OrchestratorNextCommand(ILogger<OrchestratorNextCommand> logger, OrchestratorTool orchestratorTool)
    : BaseCommand<OrchestratorNextOptions, string>
{
    private readonly ILogger<OrchestratorNextCommand> _logger = logger;
    private readonly OrchestratorTool _orchestratorTool = orchestratorTool;

    public override Task<CommandResponse> ExecuteAsync(CommandContext context, OrchestratorNextOptions options, CancellationToken cancellationToken)
    {
        try
        {
            var result = _orchestratorTool.Next(options.SessionId, options.CompletionNote);

            context.Response.Status = HttpStatusCode.OK;
            context.Response.Results = ResponseResult.Create(new OrchestratorNextCommandResult(result), MonitorJsonContext.Default.OrchestratorNextCommandResult);
            context.Response.Message = string.Empty;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in {Operation}. SessionId: {SessionId}", Name, options.SessionId);
            HandleException(context, ex);
        }

        return Task.FromResult(context.Response);
    }

    internal record OrchestratorNextCommandResult(string Result);
}
