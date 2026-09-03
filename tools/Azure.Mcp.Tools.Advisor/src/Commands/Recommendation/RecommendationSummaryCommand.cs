// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net;
using Azure.Mcp.Core.Commands.Subscription;
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
    Title = "Summarize Advisor Recommendations in a Subscription",
    Description = "Summarize the key themes from Azure Advisor recommendation instances using server-side counts, totals, rankings, and distributions. " +
        "This is the aggregate-only tool for questions such as how many, count, breakdown, distribution, top, most common, or which has the most. " +
        "Use it for an executive summary or main themes, counts by category or business impact, top recommendation types, ranking resource types by critical or High-impact recommendations, lifecycle counts for New, Completed, Dismissed, and Postponed recommendations, and metadata subcategory breakdowns such as ZoneResiliency. " +
        "Count overdue service-retirement Advisor recommendations that are still active, or group active service-retirement recommendations by retirement date. " +
        "This includes services retiring on an exact date, on or before a date, on or after a specified date, in the next N days or months, or soon. " +
        "For requests to summarize, count, or group retirements in the next N days or months by retirement date, always use this summary tool; recommendation list is capped and must not be counted client-side. " +
        "Group by recommendation-type, category, impact, resource-type, status, sub-category, or retirement-date; category is the default. " +
        "All groups return canonical key, label, and count values. Recommendation-type keys are stable type ID GUIDs with English metadata labels. " +
        "All groupings except status include only active New recommendations; status includes every backend lifecycle state. " +
        "Only current-engine recommendations whose stable name is a 64-character hash and whose serviceGroupId is empty are included. " +
        "Filters include category, impact, recommendation type ID, impacted resource type, resource name or ARM ID, problem-text search, subcategory, and explicit retirement-date comparisons. " +
        "Use --search with this summary tool for topical aggregate questions such as counts or impact breakdowns for recommendations mentioning encryption or right-size; do not call recommendation list and count its capped results. " +
        "For natural-language windows such as 'retiring soon' or 'in the next two months', compute an end date and pass le:<yyyy-MM-dd>; unqualified 'soon' means 90 calendar days and does not add a lower bound. " +
        "Use recommendation list instead when the user wants individual recommendation records. TotalRecommendations always covers the complete filtered population, even when --top limits displayed buckets.",
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
    : SubscriptionCommand<RecommendationSummaryOptions, RecommendationSummaryCommand.RecommendationSummaryResult>(subscriptionResolver)
{
    private const int MinTop = 1;
    private const int MaxTop = 100;

    private readonly IRecommendationSummaryService _recommendationSummaryService = recommendationSummaryService;
    private readonly ILogger<RecommendationSummaryCommand> _logger = logger;

    public override void ValidateOptions(RecommendationSummaryOptions options, ValidationResult validationResult)
    {
        base.ValidateOptions(options, validationResult);

        var normalizedGroupBy = options.GroupBy?.Trim();
        if (options.GroupBy is not null &&
            (string.IsNullOrEmpty(normalizedGroupBy) ||
             !RecommendationSummaryService.AllowedGroupBy.Contains(
                 normalizedGroupBy,
                 StringComparer.OrdinalIgnoreCase)))
        {
            validationResult.Errors.Add(
                $"Invalid --group-by value '{options.GroupBy}'. Allowed values: {string.Join(", ", RecommendationSummaryService.AllowedGroupBy)}.");
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
        var groupBy = string.IsNullOrWhiteSpace(options.GroupBy)
            ? RecommendationSummaryService.GroupByCategory
            : options.GroupBy.Trim().ToLowerInvariant();

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
                RetirementDate: retirementDate);

            var summary = await _recommendationSummaryService.SummarizeRecommendationsAsync(
                options.Subscription!,
                options.ResourceGroup,
                groupBy,
                filters,
                options.Tenant,
                cancellationToken);

            if (options.Top is int top && summary.Groups.Count > top)
            {
                var unknown = summary.Groups.FirstOrDefault(g => string.Equals(g.Key, "Unknown", StringComparison.OrdinalIgnoreCase));
                var nonUnknown = summary.Groups.Where(g => !string.Equals(g.Key, "Unknown", StringComparison.OrdinalIgnoreCase));
                var sliced = nonUnknown.Take(top).ToList();
                if (unknown is not null)
                {
                    sliced.Add(unknown);
                }
                summary = summary with { Groups = sliced };
            }

            context.Response.Results = ResponseResult.Create(
                new(summary),
                AdvisorJsonContext.Default.RecommendationSummaryResult);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Error summarizing Advisor recommendations. Subscription: {Subscription}, ResourceGroup: {ResourceGroup}, " +
                "GroupBy: {GroupBy}, Top: {Top}, Category: {Category}, Impact: {Impact}, " +
                "RecommendationTypeId: {RecommendationTypeId}, ResourceType: {ResourceType}, Resource: {Resource}, " +
                "SubCategory: {SubCategory}, RetirementDate: {RetirementDate}, HasSearch: {HasSearch}.",
                options.Subscription,
                options.ResourceGroup,
                groupBy,
                options.Top,
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
            "Advisor recommendations not found. Verify the subscription, resource group, and that you have access.",
        RequestFailedException reqEx when reqEx.Status == (int)HttpStatusCode.Forbidden =>
            "Authorization failed accessing Advisor recommendations. Verify the signed-in identity has Reader access.",
        RequestFailedException =>
            "Failed to query Advisor recommendations in Azure Resource Graph.",
        _ => base.GetErrorMessage(ex)
    };

    private static string? NormalizeOptionalFilter(string? value) =>
        string.IsNullOrWhiteSpace(value) ? null : value.Trim();

    public sealed record RecommendationSummaryResult(Models.RecommendationSummary Summary);
}
