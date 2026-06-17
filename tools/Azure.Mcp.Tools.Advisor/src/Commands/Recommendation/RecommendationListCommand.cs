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
public sealed class RecommendationListCommand(ILogger<RecommendationListCommand> logger, IAdvisorService advisorService)
    : BaseAdvisorCommand<RecommendationListOptions>(logger)
{
    private const int MinTop = 1;
    private const int MaxTop = 100;
    private const int DefaultTop = 50;

    private readonly IAdvisorService _advisorService = advisorService;

    protected override void RegisterOptions(Command command)
    {
        base.RegisterOptions(command);
        command.Options.Add(AdvisorOptionDefinitions.Category.AsOptional());
        command.Options.Add(AdvisorOptionDefinitions.Impact.AsOptional());
        command.Options.Add(AdvisorOptionDefinitions.ResourceType.AsOptional());
        command.Options.Add(AdvisorOptionDefinitions.Resource.AsOptional());
        command.Options.Add(AdvisorOptionDefinitions.Search.AsOptional());
        command.Options.Add(AdvisorOptionDefinitions.Top.AsOptional());
    }

    protected override RecommendationListOptions BindOptions(ParseResult parseResult)
    {
        var options = base.BindOptions(parseResult);
        options.Category = parseResult.GetValueOrDefault(AdvisorOptionDefinitions.Category);
        options.Impact = parseResult.GetValueOrDefault(AdvisorOptionDefinitions.Impact);
        options.ResourceType = parseResult.GetValueOrDefault(AdvisorOptionDefinitions.ResourceType);
        options.Resource = parseResult.GetValueOrDefault(AdvisorOptionDefinitions.Resource);
        options.Search = parseResult.GetValueOrDefault(AdvisorOptionDefinitions.Search);
        options.Top = parseResult.CommandResult.HasOptionResult(AdvisorOptionDefinitions.Top)
            ? parseResult.GetValueOrDefault(AdvisorOptionDefinitions.Top)
            : (int?)null;
        return options;
    }

    public override async Task<CommandResponse> ExecuteAsync(CommandContext context, ParseResult parseResult, CancellationToken cancellationToken)
    {
        if (!Validate(parseResult.CommandResult, context.Response).IsValid)
        {
            return context.Response;
        }

        var options = BindOptions(parseResult);

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

    internal record RecommendationListResult(List<Models.Recommendation> Recommendations, bool AreResultsTruncated);
}
