// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net;
using Azure.Mcp.Core.Services.Azure.Subscription;
using Azure.Mcp.Tools.Advisor.Options.Recommendation;
using Azure.Mcp.Tools.Advisor.Services;
using Azure.Mcp.Tools.Advisor.Validation;
using Microsoft.Extensions.Logging;
using Microsoft.Mcp.Core.Commands;
using Microsoft.Mcp.Core.Models.Command;

namespace Azure.Mcp.Tools.Advisor.Commands.Recommendation;

[CommandMetadata(
    Id = "9f6a9d4e-6e8a-4d1c-9a7a-7e1f3b2d4a55",
    Name = "summary",
    Title = "Summarize Advisor Recommendations",
    Description = "Summarize the key themes from Azure Advisor recommendation instances in a subscription or Azure service group using server-side counts, totals, rankings, and distributions. " +
        "This is the aggregate-only tool for questions such as how many, count, breakdown, distribution, most common, which has the most, or how many overdue service-retirement Advisor recommendations are still active. " +
        "Use it for an executive summary or main themes, counts by category or business impact, the most common recommendation types, ranking resource types by critical or High-impact recommendations, lifecycle counts for New, Completed, Dismissed, and Postponed recommendations, and metadata subcategory breakdowns such as ZoneResiliency. " +
        "This is the starting tool for top recommendations, top actions to take, most critical, most important, prioritized, highest-priority, or what-should-I-fix-first requests: set --prioritized true to rank active recommendation types by priority and explain each rank, instead of starting with recommendation list. " +
        "Explain a ranking in plain business language from the group's prioritizationSignals, without exposing field names, raw values, or scores. " +
        "After a prioritized summary, list the records or affected resources of a selected type with recommendation list --recommendation-type-id set to its key and --prioritized true, adding --category Cost for a Cost type. " +
        "Count active service-retirement Advisor recommendations with retirement dates on, before, or after a specified date, including overdue recommendations that are still active, or group them by retirement date. " +
        "Use this tool to answer how many overdue service-retirement Advisor recommendations are still active. " +
        "Retirement filters are one-sided: le:<end-date> includes every active recommendation retiring on or before the end date, including overdue retirements, and is not a bounded next-N window. " +
        "Use explicit exact, on-or-before, or on-or-after retirement-date questions; recommendation list is capped and must not be counted client-side. " +
        "Group by recommendation-type, category, impact, resource-type, status, sub-category, or retirement-date; category is the default. " +
        "All groups return canonical key, label, and count values. Recommendation-type keys are recommendation type ID GUIDs with English metadata labels. " +
        "All groupings except status include only active New recommendations; status includes every backend lifecycle state. " +
        "Use either --subscription or --service-group, not both. A configured default subscription is used when neither is supplied. --resource-group applies only with subscription scope. " +
        "Filters include category, impact, recommendation type ID, impacted resource type, resource name or ARM ID, problem-text search, subcategory, and explicit retirement-date comparisons. " +
        "Use --search with this summary tool for topical aggregate questions such as counts or impact breakdowns for recommendations mentioning encryption or right-size; do not call recommendation list and count its capped results. " +
        "Use recommendation list instead when the user wants individual recommendation records. TotalRecommendations always covers the complete filtered population, even when --top limits displayed buckets; with --prioritized true it is the sum of affected resources per type, so a resource is counted once for each recommendation type that affects it.",
    OperationPlane = ToolOperationPlane.Control,
    Destructive = false,
    Idempotent = true,
    OpenWorld = false,
    ReadOnly = true,
    Secret = false,
    LocalRequired = false)]
public sealed class RecommendationSummaryCommand(
    ILogger<RecommendationSummaryCommand> logger,
    IRecommendationSummaryService recommendationSummaryService,
    ISubscriptionResolver subscriptionResolver)
    : RecommendationScopeCommand<RecommendationSummaryOptions, RecommendationSummaryCommand.RecommendationSummaryResult>(
        subscriptionResolver)
{
    private const int MinTop = 1;
    private const int MaxTop = 100;

    // Prioritized summaries show a short ranked list unless the caller asks for a different number.
    private const int DefaultPrioritizedTop = 10;

    private readonly IRecommendationSummaryService _recommendationSummaryService = recommendationSummaryService;
    private readonly ILogger<RecommendationSummaryCommand> _logger = logger;

    public override void ValidateOptions(RecommendationSummaryOptions options, ValidationResult validationResult)
    {
        base.ValidateOptions(options, validationResult);

        if (options.ServiceGroup is not null && options.ResourceGroup is not null)
        {
            validationResult.Errors.Add(
                "--resource-group can only be used with subscription scope and cannot be combined with --service-group.");
        }

        var normalizedGroupBy = options.GroupBy?.Trim();
        if (options.GroupBy is not null &&
            (string.IsNullOrEmpty(normalizedGroupBy) ||
             !RecommendationSummaryService.AllowedGroupBy.Contains(
                 normalizedGroupBy,
                 StringComparer.OrdinalIgnoreCase)))
        {
            validationResult.AddError(
                $"Invalid --group-by value '{options.GroupBy}'. Allowed values: {string.Join(", ", RecommendationSummaryService.AllowedGroupBy)}.",
                "Invalid Advisor summary grouping.");
        }

        if (options.Prioritized == true &&
            !string.IsNullOrEmpty(normalizedGroupBy) &&
            !normalizedGroupBy.Equals(
                RecommendationSummaryService.GroupByRecommendationType,
                StringComparison.OrdinalIgnoreCase))
        {
            validationResult.Errors.Add(
                $"--prioritized true always groups by '{RecommendationSummaryService.GroupByRecommendationType}'. Omit --group-by or set it to '{RecommendationSummaryService.GroupByRecommendationType}'.");
        }

        if (options.Top is < MinTop or > MaxTop)
        {
            validationResult.Errors.Add($"--top must be between {MinTop} and {MaxTop}.");
        }

        RecommendationFilterValidator.ValidateCommon(
            validationResult,
            options.Category,
            options.Impact,
            options.RecommendationTypeId,
            options.ResourceType,
            options.Resource,
            options.Search,
            options.SubCategory,
            options.RetirementDate,
            serviceRetirementOnly: normalizedGroupBy?.Equals(
                RecommendationSummaryService.GroupByRetirementDate,
                StringComparison.OrdinalIgnoreCase) == true);
    }

    public override async Task<CommandResponse> ExecuteAsync(CommandContext context, RecommendationSummaryOptions options, CancellationToken cancellationToken)
    {
        var prioritized = options.Prioritized == true;
        var groupBy = prioritized
            ? RecommendationSummaryService.GroupByRecommendationType
            : string.IsNullOrWhiteSpace(options.GroupBy)
                ? RecommendationSummaryService.GroupByCategory
                : options.GroupBy.Trim().ToLowerInvariant();
        var top = options.Top ?? (prioritized ? DefaultPrioritizedTop : null);

        try
        {
            _ = ServiceRetirementFilterValidator.TryParseRetirementDate(
                options.RetirementDate,
                out var retirementDateOperator,
                out var retirementDate,
                out _);

            var filters = new Models.RecommendationFilters(
                Category: RecommendationFilterValidator.NormalizeAllowedValue(
                    options.Category,
                    RecommendationFilterValidator.AllowedCategories),
                Impact: RecommendationFilterValidator.NormalizeAllowedValue(
                    options.Impact,
                    RecommendationFilterValidator.AllowedImpacts),
                RecommendationTypeId: RecommendationFilterValidator.NormalizeRecommendationTypeId(
                    options.RecommendationTypeId),
                ResourceType: NormalizeOptionalFilter(options.ResourceType),
                Resource: NormalizeOptionalFilter(options.Resource),
                Search: NormalizeOptionalFilter(options.Search),
                SubCategory: NormalizeOptionalFilter(options.SubCategory),
                RetirementDateOperator: retirementDateOperator,
                RetirementDate: retirementDate,
                Prioritized: prioritized ? true : null);

            var summary = !string.IsNullOrEmpty(options.ServiceGroup)
                ? await _recommendationSummaryService.SummarizeServiceGroupRecommendationsAsync(
                    options.ServiceGroup,
                    groupBy,
                    filters,
                    options.Tenant,
                    cancellationToken)
                : await _recommendationSummaryService.SummarizeRecommendationsAsync(
                    options.Subscription!,
                    options.ResourceGroup,
                    groupBy,
                    filters,
                    options.Tenant,
                    cancellationToken);

            if (top is int limit && summary.Groups.Count > limit)
            {
                summary = summary with { Groups = SliceGroups(summary.Groups, limit, prioritized) };
            }

            context.Response.Results = ResponseResult.Create(
                new(summary),
                AdvisorJsonContext.Default.RecommendationSummaryResult);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Error summarizing Advisor recommendations. Subscription: {Subscription}, ServiceGroup: {ServiceGroup}, ResourceGroup: {ResourceGroup}, " +
                "GroupBy: {GroupBy}, Prioritized: {Prioritized}, Top: {Top}, Category: {Category}, Impact: {Impact}, " +
                "RecommendationTypeId: {RecommendationTypeId}, ResourceType: {ResourceType}, Resource: {Resource}, " +
                "SubCategory: {SubCategory}, RetirementDate: {RetirementDate}, HasSearch: {HasSearch}.",
                options.Subscription,
                options.ServiceGroup,
                options.ResourceGroup,
                groupBy,
                prioritized,
                top,
                options.Category,
                options.Impact,
                options.RecommendationTypeId,
                options.ResourceType,
                options.Resource,
                options.SubCategory,
                options.RetirementDate,
                !string.IsNullOrEmpty(options.Search));
            HandleException(context, ex);
        }

        return context.Response;
    }

    protected override string GetErrorMessage(Exception ex) => ex switch
    {
        RequestFailedException reqEx when reqEx.Status == (int)HttpStatusCode.NotFound =>
            "Advisor recommendations not found. Verify the subscription or service group, resource group when applicable, and that you have access.",
        RequestFailedException reqEx when reqEx.Status == (int)HttpStatusCode.Forbidden =>
            "Authorization failed accessing Advisor recommendations. Verify the signed-in identity has Reader access.",
        RequestFailedException =>
            "Failed to query Advisor recommendations in Azure Resource Graph.",
        _ => base.GetErrorMessage(ex)
    };

    private static string? NormalizeOptionalFilter(string? value) =>
        string.IsNullOrWhiteSpace(value) ? null : value.Trim();

    /// <summary>
    /// Applies --top. Prioritized groups keep their ranked order and are cut at the limit. In other groupings an
    /// Unknown bucket is not ranked, so it is preserved after the selected buckets.
    /// </summary>
    private static List<Models.RecommendationGroup> SliceGroups(
        List<Models.RecommendationGroup> groups,
        int top,
        bool prioritized)
    {
        if (prioritized)
        {
            return [.. groups.Take(top)];
        }

        var unknown = groups.FirstOrDefault(g => string.Equals(g.Key, "Unknown", StringComparison.OrdinalIgnoreCase));
        var sliced = groups
            .Where(g => !string.Equals(g.Key, "Unknown", StringComparison.OrdinalIgnoreCase))
            .Take(top)
            .ToList();
        if (unknown is not null)
        {
            sliced.Add(unknown);
        }

        return sliced;
    }

    public sealed record RecommendationSummaryResult(Models.RecommendationSummary Summary);
}
