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
    Id = "e3f09221-523a-4107-a715-823cebd97902",
    Name = "list",
    Title = "List Advisor Recommendations",
    Description = "List, show, or search individual Azure Advisor recommendations and their affected-resource details in a subscription or Azure Service Group. " +
        "Supports filters for category, subcategory, impact, recommendation status such as New, Postponed, Dismissed, or Completed, recommendation type ID, affected resource, resource type, free text, tracking ID, retirement date, and scope. " +
        "This returns recommendation instances, not the global metadata catalog, recommendation types, or service-retirement metadata by Service Health tracking ID; use advisor metadata list for those catalog lookups. " +
        "For status-filtered requests, use this tool to retrieve individual recommendations; use summary only for status counts or grouped analysis. " +
        "If a request has broad or unscoped top, critical, important, prioritized, priority, or equivalent prioritization intent and does not identify a specific recommendation type, do not use this tool; route to recommendation summary with --group-by recommendation-type and --prioritized true. " +
        "If the request identifies a specific recommendation type, use this tool directly with --recommendation-type-id and --prioritized true to order its individual recommendations. " +
        "For non-prioritized requests to retrieve individual recommendations, use this tool directly with any supported filters; a prior summary call is not required. " +
        "Use summary for counts, totals, rankings, distributions, or grouped analysis rather than aggregating this tool's limited results. " +
        "After a summary, use this tool to retrieve individual recommendations or affected resources for a selected group by passing its key as --recommendation-type-id and preserving the applicable filters. " +
        "Choose either subscription scope, optionally narrowed by resource group, or Service Group scope; the configured default subscription is used when neither is supplied. " +
        "Returns recommendations in ARM resource shape and a truncation indicator. Each recommendation's name is its recommendation ID.",
    OperationPlane = ToolOperationPlane.Control,
    Destructive = false,
    Idempotent = true,
    OpenWorld = false,
    ReadOnly = true,
    Secret = false,
    LocalRequired = false)]
public sealed class RecommendationListCommand(ILogger<RecommendationListCommand> logger, IAdvisorService advisorService, ISubscriptionResolver subscriptionResolver)
    : RecommendationScopeCommand<RecommendationListOptions, RecommendationListCommand.RecommendationListResult>(subscriptionResolver)
{
    private const int MinTop = 1;
    private const int MaxTop = 100;
    private const int DefaultTop = 50;

    private readonly IAdvisorService _advisorService = advisorService;
    private readonly ILogger<RecommendationListCommand> _logger = logger;

    public override void ValidateOptions(RecommendationListOptions options, ValidationResult validationResult)
    {
        base.ValidateOptions(options, validationResult);

        if (options.ServiceGroup is not null && options.ResourceGroup is not null)
        {
            validationResult.Errors.Add(
                "--resource-group can only be used with subscription scope and cannot be combined with --service-group.");
        }

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
                RetirementDate: retirementDate,
                ServiceGroup: options.ServiceGroup?.Trim(),
                Prioritized: options.Prioritized,
                ShowPrioritizationSignals: options.ShowPrioritizationSignals);

            var recommendations = await _advisorService.ListRecommendationsAsync(
                options.Subscription,
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
                "Error listing Advisor recommendations. Subscription: {Subscription}, ResourceGroup: {ResourceGroup}, ServiceGroup: {ServiceGroup}, Prioritized: {Prioritized}, ShowPrioritizationSignals: {ShowPrioritizationSignals}, " +
                "Category: {Category}, Impact: {Impact}, Status: {Status}, RecommendationTypeId: {RecommendationTypeId}, ResourceType: {ResourceType}, Resource: {Resource}, " +
                "SubCategory: {SubCategory}, TrackingIdCount: {TrackingIdCount}, RetirementDate: {RetirementDate}, Top: {Top}, HasSearch: {HasSearch}.",
                options.Subscription,
                options.ResourceGroup,
                options.ServiceGroup,
                options.Prioritized,
                options.ShowPrioritizationSignals,
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
