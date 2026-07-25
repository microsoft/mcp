// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net;
using Azure.Mcp.Core.Commands.Subscription;
using Azure.Mcp.Core.Services.Azure.Subscription;
using Azure.Mcp.Tools.Advisor.Options.Recommendation;
using Azure.Mcp.Tools.Advisor.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Mcp.Core.Commands;
using Microsoft.Mcp.Core.Models.Command;

namespace Azure.Mcp.Tools.Advisor.Commands.Recommendation;

[CommandMetadata(
    Id = "e3f09221-523a-4107-a715-823cebd97902",
    Name = "list",
    Title = "List Advisor Recommendations",
    Description = "Retrieve individual Azure Advisor recommendation records (one row per recommendation) from a subscription. " +
        "Use this ONLY when the user wants to see the actual recommendation contents/details. " +
        "Do NOT use this to answer aggregate questions like 'how many', 'top N resource types', 'breakdown by category', " +
        "or 'which impact has the most' — for those, call the 'summary' tool instead (it aggregates server-side over the " +
        "entire population, while 'list' is capped at 100 items and will silently undercount). " +
        "Only active recommendations (status 'New') are returned; dismissed and postponed ones are excluded. " +
        "Supports optional filters: --category, --impact, --resource-type, --resource, --search. " +
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

    public override async Task<CommandResponse> ExecuteAsync(CommandContext context, RecommendationListOptions options, CancellationToken cancellationToken)
    {
        var top = Math.Clamp(options.Top ?? DefaultTop, MinTop, MaxTop);

        try
        {
            var filters = new Models.RecommendationFilters(
                Category: options.Category,
                Impact: options.Impact,
                ResourceType: options.ResourceType,
                Resource: options.Resource,
                Search: options.Search);

            var recommendations = await _advisorService.ListRecommendationsAsync(
                options.Subscription!,
                options.ResourceGroup,
                options.RetryPolicy,
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
                "Category: {Category}, Impact: {Impact}, ResourceType: {ResourceType}, Resource: {Resource}, Top: {Top}, HasSearch: {HasSearch}.",
                options.Subscription,
                options.ResourceGroup,
                options.Category,
                options.Impact,
                options.ResourceType,
                options.Resource,
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

    public sealed record RecommendationListResult(List<Models.Recommendation> Recommendations, bool AreResultsTruncated);
}
