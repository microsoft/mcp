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
public sealed class SendEnhancementSelectCommand(ILogger<SendEnhancementSelectCommand> logger, SendEnhancementSelectTool sendEnhancementSelectTool)
    : BaseCommand<SendEnhancementSelectOptions>
{
    private readonly ILogger<SendEnhancementSelectCommand> _logger = logger;
    private readonly SendEnhancementSelectTool _sendEnhancementSelectTool = sendEnhancementSelectTool;

    protected override void RegisterOptions(Command command)
    {
        command.Options.Add(MonitorInstrumentationOptionDefinitions.SessionId);
        command.Options.Add(MonitorInstrumentationOptionDefinitions.EnhancementKeys);
    }

    protected override SendEnhancementSelectOptions BindOptions(ParseResult parseResult)
    {
        return new SendEnhancementSelectOptions
        {
            SessionId = parseResult.CommandResult.GetValueOrDefault(MonitorInstrumentationOptionDefinitions.SessionId),
            EnhancementKeys = parseResult.CommandResult.GetValueOrDefault(MonitorInstrumentationOptionDefinitions.EnhancementKeys)
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
            var result = _sendEnhancementSelectTool.Send(options.SessionId!, options.EnhancementKeys!);

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
