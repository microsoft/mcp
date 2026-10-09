// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Tools.Advisor.Models;
using Azure.Mcp.Tools.Advisor.Options.StatusInsights;
using Azure.Mcp.Tools.Advisor.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Mcp.Core.Commands;
using Microsoft.Mcp.Core.Models.Command;

namespace Azure.Mcp.Tools.Advisor.Commands.StatusInsights;

[CommandMetadata(
    Id = "7d1c4a52-3b0e-4f6a-9a58-2c8e1f0b6d34",
    Name = "get",
    Title = "Get Service Group Status and Insights",
    Description = "Get Azure Advisor Status and Insights for Service Groups. Returns per Service Group the criticality " +
        "tier (0 Mission-critical, 1 Business-critical, 2 Business-operational, 3 Administrative), the Status " +
        "(Critical, At Risk, Attention Needed, On Track, Unavailable) with its description, and Insight KPIs " +
        "(ZonalResiliency, IdleResources, ServiceRetirement). Use for questions about Service Group Status, attention " +
        "or risk, criticality, zonal resiliency, idle resources, or service retirement, for specific Service Groups " +
        "or all visible ones. The result includes statusCalculation explaining how Status is calculated; use it when asked how Status is determined. Set include=status when the user asks only for Status, health or criticality; include=insights when they ask only for Insights such as zonal resiliency, idle resources or service retirement; include=both for a full report or when unsure. Results are bounded; when moreAvailable " +
        "is true the answer is partial and continuationToken can fetch the next page. Not for listing individual " +
        "recommendations; use advisor recommendation list for that. " +
        "Response format for one Service Group: first a Service Group details block (name, criticality tier label, Status, " +
        "Status meaning from statusDescription, and impact from description); then Insights with one Markdown table per " +
        "insight (KPI and value columns) using friendly KPI names; then a 'How is my Service Group Status calculated?' " +
        "section from statusCalculation. For several Service Groups use one compact table with one row per group. " +
        "Show only values returned and never treat a missing value as zero.",
    OperationPlane = ToolOperationPlane.Control,
    Destructive = false,
    Idempotent = true,
    OpenWorld = false,
    ReadOnly = true,
    Secret = false,
    LocalRequired = false)]
public sealed class StatusInsightsGetCommand(ILogger<StatusInsightsGetCommand> logger, IServiceGroupIntelligenceService service)
    : AuthenticatedCommand<StatusInsightsGetOptions, StatusInsightsGetCommand.StatusInsightsGetResult>()
{
    private static readonly string[] IncludeValues = ["status", "insights", "both"];

    private const string StatusCalculation =
        "Azure Advisor considers the proportion of applicable resources affected by active recommendations across Zonal Resiliency, Cost Optimization, and Service Retirement, not just the number of recommendations. " +
        "Higher-priority recommendation types have greater influence, and the results are combined into the overall Status. " +
        "Only applicable areas with available data contribute. The Description, when available, identifies where to focus. " +
        "Supporting Insight counts do not by themselves determine Status or predict how much it will improve.";

    public override void ValidateOptions(StatusInsightsGetOptions options, ValidationResult validationResult)
    {
        base.ValidateOptions(options, validationResult);

        if (!IncludeValues.Contains(options.Include, StringComparer.OrdinalIgnoreCase))
        {
            validationResult.Errors.Add("--include must be one of: status, insights, both.");
        }

        if (options.CriticalityTier?.Any(t => t.Trim() is not ("0" or "1" or "2" or "3")) == true)
        {
            validationResult.Errors.Add("--criticality-tier values must be 0, 1, 2 or 3.");
        }
    }

    public override async Task<CommandResponse> ExecuteAsync(CommandContext context, StatusInsightsGetOptions options, CancellationToken cancellationToken)
    {
        try
        {
            var include = options.Include.Trim().ToLowerInvariant();
            var page = await service.GetStatusInsightsAsync(
                options.ServiceGroup,
                options.CriticalityTier,
                options.Status,
                options.InsightName,
                include != "insights",
                include != "status",
                options.ContinuationToken,
                cancellationToken);

            context.Response.Results = ResponseResult.Create(
                new StatusInsightsGetResult(page.ServiceGroups, page.MoreAvailable, page.ContinuationToken, include != "insights" ? StatusCalculation : null),
                AdvisorJsonContext.Default.StatusInsightsGetResult);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error getting Service Group status and insights.");
            HandleException(context, ex);
        }

        return context.Response;
    }

    public sealed record StatusInsightsGetResult(List<ServiceGroupStatusInsight> ServiceGroups, bool MoreAvailable, string? ContinuationToken, string? StatusCalculation);
}
