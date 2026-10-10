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
    Description = "List individual Azure Advisor recommendation records and their affected-resource details in a subscription or Azure Service Group. " +
        "Use this tool to inspect or search recommendation details, or to view affected records for a selected recommendation type. " +
        "For broad requests such as 'top recommendations', 'most important recommendations', or 'what should I fix first', " +
        "start with recommendation summary --group-by recommendation-type. Use summary for counts and grouped overviews. " +
        "To drill into a selected summary group, use this tool with --recommendation-type-id set to the group key, " +
        "preserving the scope, category, and other filters. " +
        "Optionally order matching records with --prioritized and include available prioritization signals with --show-prioritization-signals. " +
        "--show-prioritization-signals true requires --prioritized true. " +
        "Choose subscription or Service Group scope; resource-group filtering applies only to subscriptions. " +
        "Returns recommendation records in ARM resource shape and a truncation indicator. Each record's name is its recommendation ID.",
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
