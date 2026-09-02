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
    Id = "e3f09221-523a-4107-a715-823cebd97902",
    Name = "list",
    Title = "List Advisor Recommendations",
    Description = "List, show, search, or find individual Azure Advisor recommendation records in a subscription, including affected resource details when available. " +
        "Use this when the user wants actual recommendation contents or details in the Cost, Security, Performance, HighAvailability, or OperationalExcellence categories. " +
        "Filter by category, business impact, recommendation type ID, impacted Azure resource type (for example, Microsoft.Storage/storageAccounts), resource name or ID, recommendation text, subcategory, Service Health tracking IDs, or retirement date. " +
        "Do NOT use this to answer aggregate questions like 'how many', 'top N resource types', 'breakdown by category', " +
        "or 'which impact has the most' — for those, call the 'summary' tool instead (it aggregates server-side over the " +
        "entire population, while 'list' returns at most 100 records and reports when results are truncated). " +
        "Filter by --status to return New, Postponed, Dismissed, or Completed recommendations; status defaults to New when omitted. " +
        "--tracking-ids accepts multiple Service Health tracking IDs and returns recommendations matching any of them. " +
        "--tracking-ids and --retirement-date can be used independently or together. With either filter, --sub-category " +
        "is optional; when specified, it must be ServiceUpgradeAndRetirement. " +
        "--top caps the number of returned items (default 50, max 100).",
    Destructive = false,
    Idempotent = true,
    OpenWorld = false,
    ReadOnly = true,
    Secret = false,
    LocalRequired = false)]
public sealed class RecommendationListCommand(ILogger<RecommendationListCommand> logger, IAdvisorService advisorService, ISubscriptionResolver subscriptionResolver)
    : SubscriptionCommand<RecommendationListOptions, RecommendationListCommand.RecommendationListResult>(subscriptionResolver)
{
    private const int MinTop = 1;
    private const int MaxTop = 100;
    private const int DefaultTop = 50;

    private readonly IAdvisorService _advisorService = advisorService;
    private readonly ILogger<RecommendationListCommand> _logger = logger;

    public override void ValidateOptions(RecommendationListOptions options, ValidationResult validationResult)
    {
        base.ValidateOptions(options, validationResult);
        RecommendationFilterValidator.Validate(options, validationResult);
    }

    public override async Task<CommandResponse> ExecuteAsync(CommandContext context, RecommendationListOptions options, CancellationToken cancellationToken)
    {
        var top = Math.Clamp(options.Top ?? DefaultTop, MinTop, MaxTop);

        try
        {
            _ = ServiceRetirementFilterValidator.TryParseRetirementDate(
                options.RetirementDate,
                out var retirementDateOperator,
                out var retirementDate,
                out _);

            var filters = new Models.RecommendationFilters(
                Category: options.Category?.Trim(),
                Impact: options.Impact?.Trim(),
                Status: options.Status,
                RecommendationTypeId: RecommendationFilterValidator.NormalizeRecommendationTypeId(options.RecommendationTypeId),
                ResourceType: options.ResourceType?.Trim(),
                Resource: options.Resource?.Trim(),
                Search: options.Search?.Trim(),
                SubCategory: options.SubCategory,
                TrackingIds: options.TrackingIds,
                RetirementDateOperator: retirementDateOperator,
                RetirementDate: retirementDate);

            var recommendations = await _advisorService.ListRecommendationsAsync(
                options.Subscription!,
                options.ResourceGroup,
                filters,
                top,
                options.Tenant,
                cancellationToken);

            context.Response.Results = ResponseResult.Create(new(recommendations?.Results ?? [], recommendations?.AreResultsTruncated ?? false),
                AdvisorJsonContext.Default.RecommendationListResult);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Error listing Advisor recommendations. Subscription: {Subscription}, ResourceGroup: {ResourceGroup}, " +
                "Category: {Category}, Impact: {Impact}, Status: {Status}, RecommendationTypeId: {RecommendationTypeId}, ResourceType: {ResourceType}, Resource: {Resource}, " +
                "SubCategory: {SubCategory}, TrackingIdCount: {TrackingIdCount}, RetirementDate: {RetirementDate}, Top: {Top}, HasSearch: {HasSearch}.",
                options.Subscription,
                options.ResourceGroup,
                options.Category,
                options.Impact,
                options.Status,
                options.RecommendationTypeId,
                options.ResourceType,
                options.Resource,
                options.SubCategory,
                options.TrackingIds?.Length ?? 0,
                options.RetirementDate,
                top,
                !string.IsNullOrEmpty(options.Search));
            HandleException(context, ex);
        }

        return context.Response;
    }

    protected override string GetErrorMessage(Exception ex) => ex switch
    {
        RequestFailedException reqEx when reqEx.Status == (int)HttpStatusCode.NotFound =>
            "Advisor recommendation not found. Verify the subscription, resource group, and that you have access.",
        RequestFailedException reqEx when reqEx.Status == (int)HttpStatusCode.Forbidden =>
            $"Authorization failed accessing the Advisor recommendations. Verify you have appropriate permissions. Details: {reqEx.Message}",
        RequestFailedException reqEx => reqEx.Message,
        _ => base.GetErrorMessage(ex)
    };

    /// <summary> Response containing Advisor recommendations and the truncation indicator. </summary>
    public sealed record RecommendationListResult(List<Models.Recommendation> Recommendations, bool AreResultsTruncated);
}
