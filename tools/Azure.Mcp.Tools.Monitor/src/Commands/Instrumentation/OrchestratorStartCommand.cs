// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net;
using Azure.Mcp.Tools.Monitor.Options;
using Azure.Mcp.Tools.Monitor.Tools;
using Microsoft.Extensions.Logging;
using Microsoft.Mcp.Core.Commands;
using Microsoft.Mcp.Core.Extensions;
using Microsoft.Mcp.Core.Models.Command;

namespace Azure.Mcp.Tools.Monitor.Commands;

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
    : BaseCommand<OrchestratorStartOptions>
{
    private readonly ILogger<OrchestratorStartCommand> _logger = logger;
    private readonly OrchestratorTool _orchestratorTool = orchestratorTool;

    protected override void RegisterOptions(Command command)
    {
        command.Options.Add(MonitorInstrumentationOptionDefinitions.WorkspacePath);
    }

    protected override OrchestratorStartOptions BindOptions(ParseResult parseResult)
    {
        return new OrchestratorStartOptions
        {
            WorkspacePath = parseResult.CommandResult.GetValueOrDefault<string>(MonitorInstrumentationOptionDefinitions.WorkspacePath.Name)
        };
    }

    public override Task<CommandResponse> ExecuteAsync(CommandContext context, ParseResult parseResult, CancellationToken cancellationToken)
    {
        if (!Validate(parseResult.CommandResult, context.Response).IsValid)
        {
            return Task.FromResult(context.Response);
        }

        var options = BindOptions(parseResult);

        try
        {
            var result = _orchestratorTool.Start(options.WorkspacePath!);

            context.Response.Status = HttpStatusCode.OK;
            context.Response.Results = ResponseResult.Create(result, MonitorInstrumentationJsonContext.Default.String);
            context.Response.Message = string.Empty;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in {Operation}. WorkspacePath: {WorkspacePath}", Name, options.WorkspacePath);
            HandleException(context, ex);
        }

        return Task.FromResult(context.Response);
    }
}
