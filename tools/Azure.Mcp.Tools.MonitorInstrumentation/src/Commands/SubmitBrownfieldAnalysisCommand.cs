// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net;
using System.Text.Json;
using Azure.Mcp.Tools.MonitorInstrumentation.Models;
using Azure.Mcp.Tools.MonitorInstrumentation.Options;
using Azure.Mcp.Tools.MonitorInstrumentation.Tools;
using Microsoft.Extensions.Logging;
using Microsoft.Mcp.Core.Commands;
using Microsoft.Mcp.Core.Extensions;
using Microsoft.Mcp.Core.Models.Command;
using Microsoft.Mcp.Core.Models.Option;

namespace Azure.Mcp.Tools.MonitorInstrumentation.Commands;

public sealed class SubmitBrownfieldAnalysisCommand(ILogger<SubmitBrownfieldAnalysisCommand> logger)
    : BaseCommand<SubmitBrownfieldAnalysisOptions>
{
    private readonly ILogger<SubmitBrownfieldAnalysisCommand> _logger = logger;

    public override string Id => "8f69c45b-7e4f-4ea7-9a7d-58fa7fc0897e";

    public override string Name => "submit_brownfield_review";

    public override string Description => "Submit brownfield code analysis findings to continue migration orchestration.";

    public override string Title => "Submit Brownfield Analysis";

    public override ToolMetadata Metadata => new()
    {
        Destructive = false,
        Idempotent = true,
        OpenWorld = true,
        ReadOnly = true,
        LocalRequired = true,
        Secret = false
    };

    protected override void RegisterOptions(Command command)
    {
        command.Options.Add(MonitorInstrumentationOptionDefinitions.SessionId);
        command.Options.Add(MonitorInstrumentationOptionDefinitions.FindingsJson);
    }

    protected override SubmitBrownfieldAnalysisOptions BindOptions(ParseResult parseResult)
    {
        return new SubmitBrownfieldAnalysisOptions
        {
            SessionId = parseResult.CommandResult.GetValueOrDefault(MonitorInstrumentationOptionDefinitions.SessionId),
            FindingsJson = parseResult.CommandResult.GetValueOrDefault(MonitorInstrumentationOptionDefinitions.FindingsJson)
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

            var tool = context.GetService<SubmitBrownfieldAnalysisTool>();
            var result = tool.Submit(
                options.SessionId!,
                findings.ServiceOptions,
                findings.Initializers,
                findings.Processors,
                findings.ClientUsage,
                findings.Sampling);

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
