// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net;
using System.Text.RegularExpressions;
using Azure.Mcp.Tools.Advisor.Models;
using Azure.Mcp.Tools.Advisor.Options.Insight;
using Azure.Mcp.Tools.Advisor.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Mcp.Core.Commands;
using Microsoft.Mcp.Core.Models.Command;

namespace Azure.Mcp.Tools.Advisor.Commands.Insight;

[CommandMetadata(
    Id = "21f626b9-a95f-4db0-8dcc-8959e11d8d92",
    Name = "list",
    Title = "List Advisor Insights",
    Description = "Show Azure Advisor insights for one Service Group, one subscription, or across all accessible Service Groups and subscriptions. " +
              "Use for questions on insights like idle resources and potential savings, zonal resiliency coverage, or service retirement impact. " +
              "For Service Groups, answer how many idle resources exist and their potential savings amount in USD. " +
              "Get ServiceRetirement insights for subscriptions and Service Groups, including Day 1 baseline exposure, current remaining exposure, and burndown percentage. " +
              "Compare baseline and current counts to measure retirement exposure reduction or increase, and compare scopes by remaining retirement backlog. " +
              "Identify subscriptions or Service Groups that have reached 100 percent burndown with zero remaining retirement exposure. " +
              "Show how much a subscription's service retirement exposure has reduced or increased since its Day 1 baseline count. " +
              "Get ServiceRetirement baseline and current exposure for a subscription; compare subscriptions or Service Groups by remaining retirement exposure. " +
              "Optionally filter by --insight-name (IdleResources, ZonalResiliency, ServiceRetirement) and cap results with --top (default 50, max 500). " +
              "Each insight returns name, domain, scope type and ID, last updated time, and KPIs with typed values (USD amounts include a unit). ",
    OperationPlane = ToolOperationPlane.Control,
    Destructive = false,
    Idempotent = true,
    OpenWorld = false,
    ReadOnly = true,
    Secret = false,
    LocalRequired = false)]
public sealed partial class InsightListCommand(
    ILogger<InsightListCommand> logger,
    IAdvisorInsightService insightService)
    : AuthenticatedCommand<InsightListOptions, InsightListCommand.InsightListResult>()
{
    private const int MinTop = 1;
    private const int MaxTop = 500;
    private const int DefaultTop = 50;

    private readonly ILogger<InsightListCommand> _logger = logger;
    private readonly IAdvisorInsightService _insightService = insightService;

    [GeneratedRegex(@"^[A-Za-z0-9][A-Za-z0-9_.\-]{0,99}$")]
    private static partial Regex InsightNamePattern();

    // With no scope the command lists insights across all Service Groups and subscriptions, so no default subscription is
    // resolved. An explicit (even blank) value is preserved so that it is validated rather than ignored.
    public override void PostBindOptions(InsightListOptions options)
    {
        if (options.ServiceGroup is not null)
        {
            options.ServiceGroup = options.ServiceGroup.Trim().Trim('"', '\'').Trim();
        }

        if (options.Subscription is not null)
        {
            options.Subscription = options.Subscription.Trim().Trim('"', '\'');
        }
    }

    public override void ValidateOptions(InsightListOptions options, ValidationResult validationResult)
    {
        base.ValidateOptions(options, validationResult);

        if (options.ServiceGroup is not null)
        {
            if (options.Subscription is not null)
            {
                validationResult.AddError(
                    "--service-group and --subscription are mutually exclusive. Specify only one scope.",
                    "Conflicting Advisor insight scopes.");
            }
            else if (string.IsNullOrWhiteSpace(options.ServiceGroup))
            {
                validationResult.AddError("--service-group must not be empty.", "Invalid Service Group.");
            }
            else if (options.ServiceGroup.Contains('/') || options.ServiceGroup.Length > 250)
            {
                validationResult.AddError(
                    "--service-group must be the Service Group name (for example 'commerce'), not an ARM resource ID.",
                    "Invalid Service Group.");
            }
        }
        else if (options.Subscription is not null && string.IsNullOrWhiteSpace(options.Subscription))
        {
            validationResult.AddError("--subscription must not be empty.", "Invalid subscription.");
        }

        if (options.InsightName is not null && !InsightNamePattern().IsMatch(options.InsightName.Trim()))
        {
            validationResult.AddError(
                "--insight-name must be 1-100 characters: letters, digits, '_', '-' or '.', starting with a letter or digit.",
                "Invalid Advisor insight name.");
        }

        if (options.Top is { } top && (top < MinTop || top > MaxTop))
        {
            validationResult.AddError(
                $"--top must be between {MinTop} and {MaxTop}.",
                "Invalid --top value.");
        }
    }

    public override async Task<CommandResponse> ExecuteAsync(
        CommandContext context,
        InsightListOptions options,
        CancellationToken cancellationToken)
    {
        try
        {
            var insightName = string.IsNullOrWhiteSpace(options.InsightName) ? null : options.InsightName.Trim();
            var results = await _insightService.ListInsightsAsync(
                options.ServiceGroup,
                options.ServiceGroup is null ? options.Subscription : null,
                insightName,
                options.Top ?? DefaultTop,
                options.Tenant,
                cancellationToken);

            context.Response.Results = ResponseResult.Create(
                new(results.Results, results.AreResultsTruncated),
                AdvisorJsonContext.Default.InsightListResult);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Error listing Advisor insights. ScopeType: {ScopeType}, InsightName: {InsightName}, Top: {Top}.",
                GetScopeType(options),
                options.InsightName,
                options.Top);
            HandleException(context, ex);
        }

        return context.Response;
    }

    private static string GetScopeType(InsightListOptions options) =>
        options.ServiceGroup is not null ? "ServiceGroup" :
        options.Subscription is not null ? "Subscription" :
        "All";

    protected override string GetErrorMessage(Exception ex) => ex switch
    {
        ArgumentException argEx when argEx.Message.StartsWith("Multiple tenants", StringComparison.Ordinal) =>
            argEx.Message,
        RequestFailedException reqEx when reqEx.Status == (int)HttpStatusCode.Forbidden =>
            "Authorization failed querying Advisor insights in Azure Resource Graph. Verify the signed-in identity has Reader access to the scope.",
        RequestFailedException reqEx when reqEx.Status == (int)HttpStatusCode.NotFound =>
            "The requested scope or Advisor insights were not found.",
        RequestFailedException =>
            "Failed to query Advisor insights in Azure Resource Graph.",
        _ => base.GetErrorMessage(ex)
    };

    protected override HttpStatusCode GetStatusCode(Exception ex) => ex switch
    {
        ArgumentException => HttpStatusCode.BadRequest,
        RequestFailedException reqEx => (HttpStatusCode)reqEx.Status,
        _ => base.GetStatusCode(ex)
    };

    public sealed record InsightListResult(List<AdvisorInsight> Insights, bool AreResultsTruncated);
}
