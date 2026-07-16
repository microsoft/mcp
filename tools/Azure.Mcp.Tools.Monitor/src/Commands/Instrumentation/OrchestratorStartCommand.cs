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
    Id = "35f577d9-6378-4d34-b822-111ff6e8957c",
    Name = "orchestrator-start",
    Title = "Start Azure Monitor Instrumentation",
    Description = "START HERE for Azure Monitor instrumentation. Analyzes workspace and returns the first action to execute. After executing the action, call orchestrator-next to continue. DO NOT improvise. Execute EXACTLY what the 'instruction' field tells you.",
    Destructive = false,
    Idempotent = false,
    OpenWorld = false,
    ReadOnly = false,
    Secret = false,
    LocalRequired = true)]
public sealed class OrchestratorStartCommand(ILogger<OrchestratorStartCommand> logger, OrchestratorTool orchestratorTool)
    : BaseCommand<OrchestratorStartOptions, string>
{
    private readonly ILogger<OrchestratorStartCommand> _logger = logger;
    private readonly OrchestratorTool _orchestratorTool = orchestratorTool;

    public override Task<CommandResponse> ExecuteAsync(CommandContext context, OrchestratorStartOptions options, CancellationToken cancellationToken)
    {
        try
        {
            var result = _orchestratorTool.Start(options.WorkspacePath);

            context.Response.Status = HttpStatusCode.OK;
            context.Response.Results = ResponseResult.Create(new OrchestratorStartCommandResult(result), MonitorJsonContext.Default.OrchestratorStartCommandResult);
            context.Response.Message = string.Empty;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in {Operation}. WorkspacePath: {WorkspacePath}", Name, options.WorkspacePath);
            HandleException(context, ex);
        }

        return Task.FromResult(context.Response);
    }

    internal record OrchestratorStartCommandResult(string Result);
}
