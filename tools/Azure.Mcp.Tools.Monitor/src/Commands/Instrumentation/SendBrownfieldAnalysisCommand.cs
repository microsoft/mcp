// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net;
using System.Text.Json;
using Azure.Mcp.Tools.Monitor.Models.Instrumentation;
using Azure.Mcp.Tools.Monitor.Options.Instrumentation;
using Azure.Mcp.Tools.Monitor.Tools.Instrumentation;
using Microsoft.Extensions.Logging;
using Microsoft.Mcp.Core.Commands;
using Microsoft.Mcp.Core.Models.Command;

namespace Azure.Mcp.Tools.Monitor.Commands.Instrumentation;

[CommandMetadata(
    Id = "8f69c45b-7e4f-4ea7-9a7d-58fa7fc0897e",
    Name = "send-brownfield-analysis",
    Title = "Send Brownfield Analysis",
    Description = """
        Send brownfield code analysis findings after orchestrator-start returned status 'analysis_needed'.
        You must have scanned the workspace source files and filled in the analysis template.
        For sections that do not exist in the codebase, pass an empty/default object (e.g. found: false, hasCustomSampling: false) rather than null.
        After this call succeeds, continue with orchestrator-next as usual.
        """,
    Destructive = false,
    Idempotent = false,
    OpenWorld = false,
    ReadOnly = false,
    Secret = false,
    LocalRequired = true)]
public sealed class SendBrownfieldAnalysisCommand(ILogger<SendBrownfieldAnalysisCommand> logger, SendBrownfieldAnalysisTool sendBrownfieldAnalysisTool)
    : BaseCommand<SendBrownfieldAnalysisOptions, string>
{
    private readonly ILogger<SendBrownfieldAnalysisCommand> _logger = logger;
    private readonly SendBrownfieldAnalysisTool _sendBrownfieldAnalysisTool = sendBrownfieldAnalysisTool;

    public override Task<CommandResponse> ExecuteAsync(CommandContext context, SendBrownfieldAnalysisOptions options, CancellationToken cancellationToken)
    {
        try
        {
            var findings = JsonSerializer.Deserialize(options.FindingsJson, OnboardingJsonContext.Default.BrownfieldFindings);
            if (findings == null)
            {
                context.Response.Status = HttpStatusCode.BadRequest;
                context.Response.Message = "Invalid findings JSON payload.";
                return Task.FromResult(context.Response);
            }

            var result = _sendBrownfieldAnalysisTool.Submit(
                options.SessionId,
                findings.ServiceOptions,
                findings.Initializers,
                findings.Processors,
                findings.ClientUsage,
                findings.Sampling,
                findings.TelemetryPipeline,
                findings.Logging);

            context.Response.Status = HttpStatusCode.OK;
            context.Response.Results = ResponseResult.Create(new SendBrownfieldAnalysisCommandResult(result), MonitorJsonContext.Default.SendBrownfieldAnalysisCommandResult);
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

    internal record SendBrownfieldAnalysisCommandResult(string Result);
}
