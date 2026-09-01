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
    Id = "b2d8f3e5-0c4a-5f7b-9d3e-2a6c8b0f4d51",
    Name = "alternatives",
    Title = "Get Alternative Compute Recommendations",
    Description = "Get additional ALTERNATIVE compute optimization recommendations - other resize/SKU options beyond the " +
        "single primary recommendation - for an Azure virtual machine (VM) or virtual machine scale set (VMSS). Call this " +
        "when the user asks for other options, alternatives, different SKUs/series/families, or wants to compare resize " +
        "choices. Only covers compute (VM / VMSS). Optional inclusion/exclusion filters narrow the proposed SKU, VM series, " +
        "and processor type. Requires the full Azure ARM --resource-id; if the user only gives a resource NAME, FIRST call " +
        "the 'list' tool to look up the exact resource id. Returns a markdown comparison table and the parsed alternatives. " +
        "Present the alternatives as an ordered comparison table and explain the tradeoffs among estimated savings, cores, " +
        "SKU, VM series, and processor. Do not show raw JSON. " +
        "Pass the user's subscription name or id straight to --subscription; a name is resolved to its id internally, so do " +
        "NOT call the 'subscription list' tool first.",
    Destructive = false,
    Idempotent = true,
    OpenWorld = false,
    ReadOnly = true,
    Secret = false,
    LocalRequired = false)]
public sealed class RecommendationAlternativesCommand(
    ILogger<RecommendationAlternativesCommand> logger,
    IOptimizationService optimizationService,
    ISubscriptionResolver subscriptionResolver)
    : SubscriptionCommand<RecommendationAlternativesOptions, RecommendationAlternativesCommand.RecommendationAlternativesResult>(subscriptionResolver)
{
    private readonly IOptimizationService _optimizationService = optimizationService;
    private readonly ILogger<RecommendationAlternativesCommand> _logger = logger;

    public override void ValidateOptions(RecommendationAlternativesOptions options, ValidationResult validationResult)
    {
        base.ValidateOptions(options, validationResult);

        if (string.IsNullOrWhiteSpace(options.ResourceId))
        {
            validationResult.Errors.Add("--resource-id is required.");
        }
        else if (!ArmResourceId.IsValid(options.ResourceId))
        {
            validationResult.Errors.Add(OptimizationStrings.AltInvalidResourceIdMessage);
        }
    }

    public override async Task<CommandResponse> ExecuteAsync(
        CommandContext context, RecommendationAlternativesOptions options, CancellationToken cancellationToken)
    {
        try
        {
            var filters = new AlternativeFilters
            {
                NewSkus = AlternativeFilters.Parse(options.NewSkus),
                NewVmSeries = AlternativeFilters.Parse(options.NewVmSeries),
                NewProcessorTypes = AlternativeFilters.Parse(options.NewProcessorTypes),
                ExcludeSkus = AlternativeFilters.Parse(options.ExcludeSkus),
                ExcludeVmSeries = AlternativeFilters.Parse(options.ExcludeVmSeries),
                ExcludeProcessorTypes = AlternativeFilters.Parse(options.ExcludeProcessorTypes),
            };

            var parsed = await _optimizationService.GetAlternativesAsync(
                options.ResourceId!,
                options.Subscription!,
                options.Tenant,
                options.RetryPolicy,
                cancellationToken);

            var recommendations = filters.Apply(parsed);
            var markdown = recommendations.Count == 0
                ? AlternativeMarkdownBuilder.BuildNoData(options.ResourceId!, filters)
                : AlternativeMarkdownBuilder.Build(options.ResourceId!, recommendations, filters);

            context.Response.Results = ResponseResult.Create(
                new RecommendationAlternativesResult(options.ResourceId!, markdown, [.. recommendations]),
                OptimizationJsonContext.Default.RecommendationAlternativesResult);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting alternative recommendations. Subscription: {Subscription}.",
                options.Subscription);
            HandleException(context, ex);
        }

        return context.Response;
    }

    protected override string GetErrorMessage(Exception ex) => ex switch
    {
        RequestFailedException reqEx when reqEx.Status == (int)HttpStatusCode.Forbidden =>
            $"Authorization failed accessing alternative recommendations. Verify you have appropriate permissions. Details: {reqEx.Message}",
        RequestFailedException reqEx => reqEx.Message,
        _ => base.GetErrorMessage(ex)
    };

    public sealed record RecommendationAlternativesResult(
        string ResourceId,
        string Markdown,
        List<AlternativeRecommendation> Alternatives);
}
