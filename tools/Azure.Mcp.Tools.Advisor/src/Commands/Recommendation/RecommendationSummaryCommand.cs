// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net;
using Azure.Mcp.Tools.Advisor.Options;
using Azure.Mcp.Tools.Advisor.Options.Recommendation;
using Azure.Mcp.Tools.Advisor.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Mcp.Core.Commands;
using Microsoft.Mcp.Core.Extensions;
using Microsoft.Mcp.Core.Models.Command;
using Microsoft.Mcp.Core.Models.Option;

namespace Azure.Mcp.Tools.Advisor.Commands.Recommendation;

[CommandMetadata(
    Id = "9f6a9d4e-6e8a-4d1c-9a7a-7e1f3b2d4a55",
    Name = "summary",
    Title = "Summarize Advisor Recommendations",
    Description = "Group Azure Advisor recommendations by a chosen field and return the top N buckets by count. " +
        "Required: --group-by (one of 'recommendation', 'category', 'impact', 'resource-type', 'resource'). " +
        "Optional: --top (default 5, clamped to 1-50), plus the same filters as 'list' (--category, --impact, --resource-type, --resource, --search). " +
        "Filters are applied first, then aggregation runs over the filtered set.",
    Destructive = false,
    Idempotent = true,
    OpenWorld = false,
    ReadOnly = true,
    Secret = false,
    LocalRequired = false)]
public sealed class RecommendationSummaryCommand(ILogger<RecommendationSummaryCommand> logger, IAdvisorService advisorService)
    : BaseAdvisorCommand<RecommendationSummaryOptions>(logger)
{
    private const int MinTop = 1;
    private const int MaxTop = 50;
    private const int DefaultTop = 5;

    private readonly IAdvisorService _advisorService = advisorService;

    protected override void RegisterOptions(Command command)
    {
        base.RegisterOptions(command);
        command.Options.Add(AdvisorOptionDefinitions.GroupBy.AsRequired());
        command.Options.Add(AdvisorOptionDefinitions.Top.AsOptional());
        command.Options.Add(AdvisorOptionDefinitions.Category.AsOptional());
        command.Options.Add(AdvisorOptionDefinitions.Impact.AsOptional());
        command.Options.Add(AdvisorOptionDefinitions.ResourceType.AsOptional());
        command.Options.Add(AdvisorOptionDefinitions.Resource.AsOptional());
        command.Options.Add(AdvisorOptionDefinitions.Search.AsOptional());
    }

    protected override RecommendationSummaryOptions BindOptions(ParseResult parseResult)
    {
        var options = base.BindOptions(parseResult);
        options.GroupBy = parseResult.GetValueOrDefault(AdvisorOptionDefinitions.GroupBy);
        options.Top = parseResult.CommandResult.HasOptionResult(AdvisorOptionDefinitions.Top)
            ? parseResult.GetValueOrDefault(AdvisorOptionDefinitions.Top)
            : (int?)null;
        options.Category = parseResult.GetValueOrDefault(AdvisorOptionDefinitions.Category);
        options.Impact = parseResult.GetValueOrDefault(AdvisorOptionDefinitions.Impact);
        options.ResourceType = parseResult.GetValueOrDefault(AdvisorOptionDefinitions.ResourceType);
        options.Resource = parseResult.GetValueOrDefault(AdvisorOptionDefinitions.Resource);
        options.Search = parseResult.GetValueOrDefault(AdvisorOptionDefinitions.Search);
        return options;
    }

    public override async Task<CommandResponse> ExecuteAsync(CommandContext context, ParseResult parseResult, CancellationToken cancellationToken)
    {
        if (!Validate(parseResult.CommandResult, context.Response).IsValid)
        {
            return context.Response;
        }

        var options = BindOptions(parseResult);

        var groupBy = options.GroupBy?.Trim().ToLowerInvariant();
        if (string.IsNullOrEmpty(groupBy) || !RecommendationAggregator.AllowedGroupBy.Contains(groupBy))
        {
            context.Response.Status = HttpStatusCode.BadRequest;
            context.Response.Message =
                $"Invalid --group-by value '{options.GroupBy}'. Allowed values: {string.Join(", ", RecommendationAggregator.AllowedGroupBy)}.";
            return context.Response;
        }

        var top = Math.Clamp(options.Top ?? DefaultTop, MinTop, MaxTop);

        try
        {
            var filters = new Models.RecommendationFilters(
                Category: options.Category,
                Impact: options.Impact,
                ResourceType: options.ResourceType,
                Resource: options.Resource,
                Search: options.Search);

            var summary = await _advisorService.SummarizeRecommendationsAsync(
                options.Subscription!,
                options.ResourceGroup,
                options.RetryPolicy,
                groupBy,
                top,
                filters,
                cancellationToken);

            context.Response.Results = ResponseResult.Create(
                new RecommendationSummaryResult(summary),
                AdvisorJsonContext.Default.RecommendationSummaryResult);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Error summarizing Advisor recommendations. Subscription: {Subscription}, ResourceGroup: {ResourceGroup}, " +
                "GroupBy: {GroupBy}, Top: {Top}, Category: {Category}, Impact: {Impact}, ResourceType: {ResourceType}, " +
                "Resource: {Resource}, HasSearch: {HasSearch}.",
                options.Subscription,
                options.ResourceGroup,
                groupBy,
                top,
                options.Category,
                options.Impact,
                options.ResourceType,
                options.Resource,
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
            $"Authorization failed accessing the Advisor recommendations. Verify you have appropriate permissions. Details: {reqEx.Message}",
        RequestFailedException reqEx => reqEx.Message,
        _ => base.GetErrorMessage(ex)
    };

    internal record RecommendationSummaryResult(Models.RecommendationSummary Summary);
}
