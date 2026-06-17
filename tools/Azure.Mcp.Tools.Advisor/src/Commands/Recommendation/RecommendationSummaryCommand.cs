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
    Title = "Summarize Advisor Recommendations in a Subscription",
    Description = "Aggregate Azure Advisor recommendations server-side and return per-bucket counts plus a true total. " +
        "This is the CORRECT tool whenever the user asks 'how many', 'top N <something>', 'which <X> has the most', " +
        "'breakdown by <field>', 'distribution of', 'count of', or any ranking/comparison question over the recommendation set. " +
        "Do not try to answer such questions by calling 'list' and counting client-side — 'list' is capped at 100 items and will undercount. " +
        "Optional: --group-by (one of 'recommendation-type', 'category', 'impact', 'resource-type'); defaults to 'category' when omitted, " +
        "which surfaces the high-level themes (Cost, Security, Reliability, etc.) so prompts like 'summarize the key themes from my Advisor recommendations' work without naming a field. " +
        "Only active recommendations (status 'New') are aggregated; dismissed and postponed ones are excluded. " +
        "Optional filters (same semantics as 'list'): --category, --impact, --resource-type, --resource, --search — applied BEFORE aggregation. " +
        "Optional --top caps how many buckets are displayed (defaults to all); the 'Unknown' bucket is always preserved at the tail so users can see uncategorized items. " +
        "'TotalRecommendations' always reflects the full filtered population regardless of --top. " +
        "Presentation guidance: when rendering Groups as a table, include every returned bucket — in particular the 'Unknown' bucket must appear as a regular row at the bottom of the table, NOT relegated to a footnote, since it represents real recommendations that lack an extractable group key.",
    Destructive = false,
    Idempotent = true,
    OpenWorld = false,
    ReadOnly = true,
    Secret = false,
    LocalRequired = false)]
public sealed class RecommendationSummaryCommand(ILogger<RecommendationSummaryCommand> logger, IAdvisorService advisorService)
    : BaseAdvisorCommand<RecommendationSummaryOptions>(logger)
{
    private readonly IAdvisorService _advisorService = advisorService;

    protected override void RegisterOptions(Command command)
    {
        base.RegisterOptions(command);
        command.Options.Add(AdvisorOptionDefinitions.GroupBy.AsOptional());
        command.Options.Add(AdvisorOptionDefinitions.Top.AsOptional());
        command.Options.Add(AdvisorOptionDefinitions.Category.AsOptional());
        command.Options.Add(AdvisorOptionDefinitions.Impact.AsOptional());
        command.Options.Add(AdvisorOptionDefinitions.ResourceType.AsOptional());
        command.Options.Add(AdvisorOptionDefinitions.Resource.AsOptional());
        command.Options.Add(AdvisorOptionDefinitions.Search.AsOptional());

        command.Validators.Add(commandResult =>
        {
            if (!commandResult.TryGetValue(AdvisorOptionDefinitions.GroupBy, out string? value))
            {
                // --group-by is optional; when omitted we default to 'category' in ExecuteAsync.
                return;
            }

            var normalized = value?.Trim();
            if (string.IsNullOrEmpty(normalized) ||
                !AdvisorService.AllowedGroupBy.Contains(normalized, StringComparer.OrdinalIgnoreCase))
            {
                commandResult.AddError(
                    $"Invalid --group-by value '{value}'. Allowed values: {string.Join(", ", AdvisorService.AllowedGroupBy)}.");
            }
        });
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

        // Validator in RegisterOptions guarantees that when --group-by is supplied it is one of
        // AllowedGroupBy (case-insensitive). When omitted, default to 'category' — the most useful
        // high-level "key themes" view. Normalize to lowercase so the service receives the canonical bucket name.
        var groupBy = string.IsNullOrWhiteSpace(options.GroupBy)
            ? AdvisorService.GroupByCategory
            : options.GroupBy.Trim().ToLowerInvariant();

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
                filters,
                cancellationToken);

            // --top is a presentation cap: slice the bucket list but keep TotalRecommendations
            // pointing at the full filtered population so callers always see true totals.
            // The 'Unknown' bucket is always preserved at the tail (even when it would fall
            // outside the top-N) so users can see how much of the data is uncategorized.
            if (options.Top is int top && top > 0 && summary.Groups.Count > top)
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
                options.Top,
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
