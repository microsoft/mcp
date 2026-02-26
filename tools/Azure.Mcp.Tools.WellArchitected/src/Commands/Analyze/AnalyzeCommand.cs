// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.CommandLine.Parsing;
using System.Net;
using Azure.Mcp.Core.Commands;
using Azure.Mcp.Core.Extensions;
using Azure.Mcp.Tools.WellArchitected.Models;
using Azure.Mcp.Tools.WellArchitected.Options;
using Azure.Mcp.Tools.WellArchitected.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Mcp.Core.Commands;
using Microsoft.Mcp.Core.Models.Command;
using Microsoft.Mcp.Core.Models.Option;

namespace Azure.Mcp.Tools.WellArchitected.Commands.Analyze;

public sealed class AnalyzeCommand(ILogger<AnalyzeCommand> logger)
    : GlobalCommand<AnalyzeOptions>
{
    public override string Id => "f7c8b3e4-2d91-4a5f-9b8e-3c7d1a4f6e9b";
    public override string Name => "analyze";
    public override string Description => "Analyze infrastructure configuration against Azure Well-Architected Framework. " +
        "Returns curated WAF guidance including agent instructions, property signals, matched service guides, " +
        "and relevant recommendations with relevance reasons. Use as follows: " +
        "1. Read the agentInstructions in wafGuidance for analysis framework. " +
        "2. Compare propertySignals against relevantRecommendations for each pillar. " +
        "3. For each pillar, identify strengths (signals match recommendations), gaps (recommendations without matching signals), and specific suggestions. " +
        "4. Prioritize by user intent. Reference WAF recommendation IDs (e.g., RE:01, SE:05) in all findings. " +
        "5. Use matchedServiceGuides for service-specific context. Be concrete and actionable.";
    public override string Title => "Analyze Infrastructure";
    public override ToolMetadata Metadata => new()
    {
        Destructive = false,
        Idempotent = true,
        OpenWorld = false,
        ReadOnly = true,
        LocalRequired = false,
        Secret = false
    };

    protected override void RegisterOptions(Command command)
    {
        base.RegisterOptions(command);
        command.Options.Add(WellArchitectedOptionDefinitions.InfrastructureConfig.AsRequired());
        command.Options.Add(WellArchitectedOptionDefinitions.Intent.AsRequired());
    }

    protected override AnalyzeOptions BindOptions(ParseResult parseResult)
    {
        var options = base.BindOptions(parseResult);
        options.InfrastructureConfig = parseResult.GetValueOrDefault<string>(WellArchitectedOptionDefinitions.InfrastructureConfig.Name);
        options.Intent = parseResult.GetValueOrDefault<string>(WellArchitectedOptionDefinitions.Intent.Name);
        return options;
    }

    public override async Task<CommandResponse> ExecuteAsync(CommandContext context, ParseResult parseResult, CancellationToken cancellationToken)
    {
        if (!Validate(parseResult.CommandResult, context.Response).IsValid)
        {
            return context.Response;
        }

        var options = BindOptions(parseResult);

        try
        {
            context.Activity?.AddTag(WellArchitectedTelemetryTags.Intent, options.Intent?.Length > 200 ? options.Intent[..200] : options.Intent);

            if (string.IsNullOrEmpty(options.InfrastructureConfig))
            {
                throw new ArgumentException("Infrastructure configuration is required", nameof(options.InfrastructureConfig));
            }

            if (string.IsNullOrEmpty(options.Intent))
            {
                throw new ArgumentException("Intent is required", nameof(options.Intent));
            }

            var service = context.GetService<IWellArchitectedService>();
            var response = await service.AnalyzeAsync(options.InfrastructureConfig, options.Intent, cancellationToken);

            var commandResult = new AnalyzeCommandResult(response);
            context.Response.Status = HttpStatusCode.OK;
            context.Response.Results = ResponseResult.Create(commandResult, WellArchitectedJsonContext.Default.AnalyzeCommandResult);
            context.Response.Message = string.Empty;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error analyzing infrastructure. Options: {@Options}", options);
            HandleException(context, ex);
        }

        return context.Response;
    }

    internal record AnalyzeCommandResult(WafAnalyzeResponse Response);
}
