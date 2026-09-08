// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net;
using Azure;
using Azure.Mcp.Core.Commands.Subscription;
using Azure.Mcp.Core.Services.Azure.Subscription;
using Azure.Mcp.Tools.Optimization.Models;
using Azure.Mcp.Tools.Optimization.Options.Recommendation;
using Azure.Mcp.Tools.Optimization.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Mcp.Core.Commands;
using Microsoft.Mcp.Core.Models.Command;

namespace Azure.Mcp.Tools.Optimization.Commands.Recommendation;

[CommandMetadata(
    Id = "c3e9a4f6-1d5b-6a8c-0e4f-3b7d9c1a5e62",
    Name = "explain",
    Title = "Explain Recommendation with Utilization Projection",
    Description = "Explain a specific Azure Advisor cost/right-size recommendation for a resource and return its " +
        "utilization time-series, which can be rendered as an inline chart. Call this whenever the user asks to " +
        "'explain recommendation N', 'explain this recommendation', 'tell me more about recommendation N', 'why is this " +
        "recommended', or 'go deeper on' a recommendation after listing recommendations with the 'list' tool. Pass the " +
        "--resource-id from the corresponding row returned by the 'list' tool. --target-sku is OPTIONAL: pass the target " +
        "VM/VMSS SKU to project against only if the user names one. When --target-sku is omitted, the response contains " +
        "ONLY the current utilization (no target configuration and no projected target series); provide a target SKU to " +
        "get a current-versus-target comparison. Returns the matching recommendation count, the current (and, when a " +
        "target SKU is given, target) configuration (SKU, instance count, vCPUs, memory), the utilization thresholds, and " +
        "the CPU / used-memory / total-network utilization time-series as structured JSON over a seven-day window in " +
        "30-minute maximum buckets (detail view) by default. Network utilization may be absent and should be treated as " +
        "optional. Set --view to 'Trend' (six-hour) or 'Both' only when the user explicitly asks for longer-term trend " +
        "data. Utilization is read from Azure Monitor; vCPU and memory come from the Microsoft.Compute Resource SKUs API. " +
        "The response contains structured JSON only \u2014 there is no markdown summary. When possible, render the " +
        "recentUtilization (and longTermUtilization when present) series as an inline line/time-series chart with the " +
        "timestamp on the x-axis and percentage on the y-axis, drawing lines for current (and target when present) CPU " +
        "and used-memory utilization, including network only when network values are present, and marking the threshold " +
        "levels from thresholds when available. Then briefly summarize the recommendation, the configuration, the maximum " +
        "utilization, and any threshold risks. If inline chart rendering is not available, summarize the data in text " +
        "instead. " +
        "Pass the user's subscription name or id straight to --subscription; a name is resolved to its id internally, so do " +
        "NOT call the 'subscription list' tool first.",
    Destructive = false,
    Idempotent = true,
    OpenWorld = false,
    ReadOnly = true,
    Secret = false,
    LocalRequired = false)]
public sealed class RecommendationExplainCommand(
    ILogger<RecommendationExplainCommand> logger,
    IOptimizationService optimizationService,
    ISubscriptionResolver subscriptionResolver)
    : SubscriptionCommand<RecommendationExplainOptions, RecommendationExplanationResult>(subscriptionResolver)
{
    private readonly IOptimizationService _optimizationService = optimizationService;
    private readonly ILogger<RecommendationExplainCommand> _logger = logger;

    public override void ValidateOptions(RecommendationExplainOptions options, ValidationResult validationResult)
    {
        base.ValidateOptions(options, validationResult);

        if (string.IsNullOrWhiteSpace(options.ResourceId))
        {
            validationResult.Errors.Add("--resource-id is required.");
        }
        else if (!ArmResourceId.IsValid(ArmResourceId.StripAdvisorRecommendationSuffix(options.ResourceId)))
        {
            validationResult.Errors.Add(OptimizationStrings.ErrorInvalidResourceId);
        }

        if (!string.IsNullOrWhiteSpace(options.View) &&
            !Enum.TryParse<UtilizationView>(options.View, ignoreCase: true, out _))
        {
            validationResult.Errors.Add("--view must be one of 'Detail', 'Trend', or 'Both'.");
        }
    }

    public override async Task<CommandResponse> ExecuteAsync(
        CommandContext context, RecommendationExplainOptions options, CancellationToken cancellationToken)
    {
        var view = Enum.TryParse<UtilizationView>(options.View, ignoreCase: true, out var parsed)
            ? parsed
            : UtilizationView.Detail;

        try
        {
            var result = await _optimizationService.GetRecommendationExplanationAsync(
                options.ResourceId!,
                options.TargetSku,
                view,
                options.Subscription!,
                options.Tenant,
                cancellationToken);

            context.Response.Results = ResponseResult.Create(
                result, OptimizationJsonContext.Default.RecommendationExplanationResult);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error explaining recommendation. Subscription: {Subscription}.",
                options.Subscription);
            HandleException(context, ex);
        }

        return context.Response;
    }

    protected override string GetErrorMessage(Exception ex) => ex switch
    {
        RequestFailedException reqEx when reqEx.Status == (int)HttpStatusCode.Forbidden =>
            $"Authorization failed retrieving the recommendation explanation. Verify you have appropriate permissions. Details: {reqEx.Message}",
        RequestFailedException reqEx => reqEx.Message,
        _ => base.GetErrorMessage(ex)
    };
}
