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
    Id = "a1c7e2d4-9b3f-4e6a-8c2d-1f5b7a9e3c40",
    Name = "list",
    Title = "List Top Cost-Saving Recommendations",
    Description = "Get Azure cost-saving / cost-optimization recommendations (a.k.a. top optimization recommendations) " +
        "for a subscription, ranked by impact and currency-normalized annual savings, by running a curated Azure " +
        "Resource Graph (ARG) query over Azure Advisor cost recommendations. Call this whenever the user asks about " +
        "'cost savings recommendation(s)', 'cost optimization recommendation(s)', or the 'top optimization " +
        "recommendation(s)'. --top caps the number of returned items (default 100, max 1000). Returns one row per " +
        "recommendation with normalized annual/monthly savings, impacted resource, impact, and solution. When " +
        "presenting results, summarize the count and use a readable table sorted by impact and savings rather than raw JSON. " +
        "To explain or go deeper on a specific listed recommendation (e.g. 'explain recommendation 1'), call the 'explain' " +
        "tool with that row's resourceId and recommendationTypeId. " +
        "Pass the user's subscription name or id straight to --subscription; a name is resolved to its id internally, so do " +
        "NOT call the 'subscription list' tool first.",
    Destructive = false,
    Idempotent = true,
    OpenWorld = false,
    ReadOnly = true,
    Secret = false,
    LocalRequired = false)]
public sealed class RecommendationListCommand(
    ILogger<RecommendationListCommand> logger,
    IOptimizationService optimizationService,
    ISubscriptionResolver subscriptionResolver)
    : SubscriptionCommand<RecommendationListOptions, RecommendationListCommand.RecommendationListResult>(subscriptionResolver)
{
    private const int MinTop = 1;
    private const int MaxTop = 1000;
    private const int DefaultTop = 100;

    private readonly IOptimizationService _optimizationService = optimizationService;
    private readonly ILogger<RecommendationListCommand> _logger = logger;

    public override async Task<CommandResponse> ExecuteAsync(
        CommandContext context, RecommendationListOptions options, CancellationToken cancellationToken)
    {
        var top = Math.Clamp(options.Top ?? DefaultTop, MinTop, MaxTop);

        try
        {
            var results = await _optimizationService.ListCostSavingsAsync(
                options.Subscription!,
                top,
                options.Tenant,
                cancellationToken);

            var message = results.SubscriptionOptions is { Count: > 0 }
                ? $"Multiple subscriptions match '{options.Subscription}'. Please select the correct one and re-run using its exact subscription id."
                : null;

            context.Response.Results = ResponseResult.Create(
                new RecommendationListResult(
                    results.Recommendations,
                    results.AreResultsTruncated,
                    message,
                    results.SubscriptionOptions),
                OptimizationJsonContext.Default.RecommendationListResult);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error listing cost-saving recommendations. Subscription: {Subscription}, Top: {Top}.",
                options.Subscription, top);
            HandleException(context, ex);
        }

        return context.Response;
    }

    protected override string GetErrorMessage(Exception ex) => ex switch
    {
        RequestFailedException reqEx when reqEx.Status == (int)HttpStatusCode.Forbidden =>
            $"Authorization failed accessing cost-saving recommendations. Verify you have appropriate permissions. Details: {reqEx.Message}",
        RequestFailedException reqEx => reqEx.Message,
        _ => base.GetErrorMessage(ex)
    };

    public sealed record RecommendationListResult(
        List<CostSavingsRecommendation> Recommendations,
        bool AreResultsTruncated,
        string? Message = null,
        IReadOnlyList<SubscriptionOption>? SubscriptionOptions = null);
}
