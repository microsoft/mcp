// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net;
using Azure;
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
        "for a subscription or, when no subscription is given, across all accessible subscriptions in the signed-in tenant, " +
        "ranked by impact and currency-normalized annual savings, by running a curated Azure " +
        "Resource Graph (ARG) query over cost recommendations. Call this whenever the user asks about " +
        "'cost savings recommendation(s)', 'cost optimization recommendation(s)', or the 'top optimization " +
        "recommendation(s)'. --top caps the number of returned items (default 100, max 1000). Returns one row per " +
        "recommendation with normalized annual/monthly savings, impacted resource, impact, and solution. When " +
        "presenting results, refer to them as 'cost optimization recommendations' (do not mention 'Azure Advisor'), " +
        "summarize the count and use a readable table sorted by impact and savings rather than raw JSON. " +
        "Unattached-disk recommendations are not listed individually; when 'diskRecommendationSummary' is present, add one line " +
        "after the table with its message and a 'Take action in Advisor' link to its actionUrl, and never list individual disks. " +
        "To explain or go deeper on a specific listed recommendation (e.g. 'explain recommendation 1'), call the 'explain' " +
        "tool with that row's resourceId and recommendationTypeId. " +
        "Pass the user's subscription name or id straight to --subscription; a name is resolved to its id internally, so do " +
        "NOT call the 'subscription list' tool first. If the user does not name a subscription, omit --subscription and " +
        "--tenant (do not ask for either or pick a default); the signed-in session's default tenant is used, so tell the " +
        "user the results cover all accessible subscriptions in their signed-in tenant. Pass --tenant only when the user " +
        "names a tenant.",
    OperationPlane = ToolOperationPlane.Control,
    Destructive = false,
    Idempotent = true,
    OpenWorld = false,
    ReadOnly = true,
    Secret = false,
    LocalRequired = false)]
public sealed class RecommendationListCommand(
    ILogger<RecommendationListCommand> logger,
    IOptimizationService optimizationService)
    : AuthenticatedCommand<RecommendationListOptions, RecommendationListCommand.RecommendationListResult>()
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
                    results.SubscriptionOptions,
                    results.DiskRecommendationSummary),
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

    protected override HttpStatusCode GetStatusCode(Exception ex) => ex switch
    {
        KeyNotFoundException => HttpStatusCode.NotFound,
        _ => base.GetStatusCode(ex)
    };

    public sealed record RecommendationListResult(
        List<CostSavingsRecommendation> Recommendations,
        bool AreResultsTruncated,
        string? Message = null,
        IReadOnlyList<SubscriptionOption>? SubscriptionOptions = null,
        DiskRecommendationSummary? DiskRecommendationSummary = null);
}
