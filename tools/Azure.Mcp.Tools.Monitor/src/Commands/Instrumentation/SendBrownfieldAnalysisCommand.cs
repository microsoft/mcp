// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net;
using System.Text.Json;
using Azure.Mcp.Tools.Monitor.Models;
using Azure.Mcp.Tools.Monitor.Options;
using Azure.Mcp.Tools.Monitor.Tools;
using Microsoft.Extensions.Logging;
using Microsoft.Mcp.Core.Commands;
using Microsoft.Mcp.Core.Extensions;
using Microsoft.Mcp.Core.Models.Command;

namespace Azure.Mcp.Tools.Monitor.Commands;

public sealed class SendBrownfieldAnalysisCommand(ILogger<SendBrownfieldAnalysisCommand> logger)
    : BaseCommand<SendBrownfieldAnalysisOptions>
{
    private readonly ILogger<SendBrownfieldAnalysisCommand> _logger = logger;

    public override string Id => "8f69c45b-7e4f-4ea7-9a7d-58fa7fc0897e";

    public override string Name => "send-brownfield-analysis";

    public override string Description => @"Send brownfield code analysis findings after orchestrator-start returned status 'analysis_needed'.
You must have scanned the workspace source files and filled in the analysis template.
For sections that do not exist in the codebase, pass an empty/default object (e.g. found: false, hasCustomSampling: false) rather than null.
After this call succeeds, continue with orchestrator-next as usual.";

    public override string Title => "Send Brownfield Analysis";

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
        command.Options.Add(MonitorInstrumentationOptionDefinitions.FindingsJson);
    }

    protected override SendBrownfieldAnalysisOptions BindOptions(ParseResult parseResult)
    {
        return new SendBrownfieldAnalysisOptions
        {
            SessionId = parseResult.CommandResult.GetValueOrDefault<string>(MonitorInstrumentationOptionDefinitions.SessionId.Name),
            FindingsJson = parseResult.CommandResult.GetValueOrDefault<string>(MonitorInstrumentationOptionDefinitions.FindingsJson.Name)
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
            var findings = JsonSerializer.Deserialize(options.FindingsJson!, OnboardingJsonContext.Default.BrownfieldFindings);
            if (findings == null)
            {
                context.Response.Status = HttpStatusCode.BadRequest;
                context.Response.Message = "Invalid findings JSON payload.";
                return Task.FromResult(context.Response);
            }

            var tool = context.GetService<SendBrownfieldAnalysisTool>();
            var result = tool.Submit(
                options.SessionId!,
                findings.ServiceOptions,
                findings.Initializers,
                findings.Processors,
                findings.ClientUsage,
                findings.Sampling,
                findings.TelemetryPipeline,
                findings.Logging);

            context.Response.Status = HttpStatusCode.OK;
            context.Response.Results = ResponseResult.Create(result, MonitorInstrumentationJsonContext.Default.String);
            context.Response.Message = string.Empty;
        }
        catch (JsonException ex)
        {
            _logger.LogWarning(ex, "Invalid findings JSON for session {SessionId}", options.SessionId);
            context.Response.Status = HttpStatusCode.BadRequest;
            context.Response.Message = "Invalid findings JSON payload.";
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in {Operation}. SessionId: {SessionId}", Name, options.SessionId);
            HandleException(context, ex);
        }

        return Task.FromResult(context.Response);
    }
}
