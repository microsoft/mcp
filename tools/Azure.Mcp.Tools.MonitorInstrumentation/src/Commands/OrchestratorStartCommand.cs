// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net;
using Azure.Mcp.Tools.MonitorInstrumentation.Options;
using Azure.Mcp.Tools.MonitorInstrumentation.Tools;
using Microsoft.Extensions.Logging;
using Microsoft.Mcp.Core.Commands;
using Microsoft.Mcp.Core.Extensions;
using Microsoft.Mcp.Core.Models.Command;
using Microsoft.Mcp.Core.Models.Option;

namespace Azure.Mcp.Tools.MonitorInstrumentation.Commands;

public sealed class OrchestratorStartCommand(ILogger<OrchestratorStartCommand> logger)
    : BaseCommand<OrchestratorStartOptions>
{
    private readonly ILogger<OrchestratorStartCommand> _logger = logger;

    public override string Id => "35f577d9-6378-4d34-b822-111ff6e8957c";

    public override string Name => "orchestrator_start";

    public override string Description =>
        "START HERE for Azure Monitor instrumentation. Analyzes workspace and returns the first action to execute. After executing the action, call orchestrator_next to continue. DO NOT improvise. Execute EXACTLY what the 'instruction' field tells you.";

    public override string Title => "Start Azure Monitor Instrumentation";

    public override ToolMetadata Metadata => new()
    {
        Destructive = false,
        Idempotent = false,
        OpenWorld = true,
        ReadOnly = true,
        LocalRequired = true,
        Secret = false
    };

    protected override void RegisterOptions(Command command)
    {
        command.Options.Add(MonitorInstrumentationOptionDefinitions.WorkspacePath);
    }

    protected override OrchestratorStartOptions BindOptions(ParseResult parseResult)
    {
        return new OrchestratorStartOptions
        {
            WorkspacePath = parseResult.CommandResult.GetValueOrDefault(MonitorInstrumentationOptionDefinitions.WorkspacePath)
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
            var tool = context.GetService<OrchestratorTool>();
            var result = tool.Start(options.WorkspacePath!);

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
