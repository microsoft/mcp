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

public sealed class OrchestratorNextCommand(ILogger<OrchestratorNextCommand> logger)
    : BaseCommand<OrchestratorNextOptions>
{
    private readonly ILogger<OrchestratorNextCommand> _logger = logger;

    public override string Id => "dd7d9a59-fb6d-436a-9e08-8bbdf6d5f9d5";

    public override string Name => "orchestrator-next";

    public override string Description => @"Get the next instrumentation action after completing the current one.
Call this ONLY after you have executed the EXACT instruction from the previous response.
DO NOT skip steps. DO NOT improvise. DO NOT add extra code or commands.

Expected workflow:
1. You received an action from orchestrator-start or orchestrator-next
2. You executed EXACTLY what the 'instruction' field told you to do
3. Now call this tool to get the next action

Returns: The next action to execute, or 'complete' status when all steps are done.";

    public override string Title => "Get Next Azure Monitor Instrumentation Action";

    public override ToolMetadata Metadata => new()
    {
        Destructive = false,
        Idempotent = false,
        OpenWorld = false,
        ReadOnly = false,
        LocalRequired = true,
        Secret = false
    };

    protected override void RegisterOptions(Command command)
    {
        command.Options.Add(MonitorInstrumentationOptionDefinitions.SessionId);
        command.Options.Add(MonitorInstrumentationOptionDefinitions.CompletionNote);
    }

    protected override OrchestratorNextOptions BindOptions(ParseResult parseResult)
    {
        return new OrchestratorNextOptions
        {
            SessionId = parseResult.CommandResult.GetValueOrDefault<string>(MonitorInstrumentationOptionDefinitions.SessionId.Name),
            CompletionNote = parseResult.CommandResult.GetValueOrDefault<string>(MonitorInstrumentationOptionDefinitions.CompletionNote.Name)
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
            var result = tool.Next(options.SessionId!, options.CompletionNote!);

            context.Response.Status = HttpStatusCode.OK;
            context.Response.Results = ResponseResult.Create(result, MonitorInstrumentationJsonContext.Default.String);
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
