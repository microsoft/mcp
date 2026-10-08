// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net;
using Azure.Mcp.Tools.Advisor.Models;
using Azure.Mcp.Tools.Advisor.Options.ServiceRetirement;
using Azure.Mcp.Tools.Advisor.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Mcp.Core.Commands;
using Microsoft.Mcp.Core.Models.Command;

namespace Azure.Mcp.Tools.Advisor.Commands.ServiceRetirement;

[CommandMetadata(
    Id = "938192ed-a9e5-4edf-b00d-cdec358e7b39",
    Name = "insights",
    Title = "Get Service Retirement Insights",
    Description = "Get Azure service-retirement burndown insight at subscription or service-group aggregation levels. " +
        "Use this to compare Day 1 and current impacted-resource exposure, identify increases or reductions for a subscription or service group, " +
    "rank subscriptions or service groups by their remaining backlog or burndown percentage using Day 1 and current impacted-resource counts, " +
        "flag subscriptions or service groups with less than 50 percent burndown, meaning more than half of their Day 1 impacted resources remain impacted, " +
        "or find subscriptions and service groups with 100 percent burndown (zero remaining exposure). " +
        "For questions about a specific subscription or service group, provide the corresponding subscription ID or name or service group ID or name. " +
        "Omit both scope filters only for fleet-wide questions such as ranking remaining backlog or finding scopes with 100 percent burndown. " +
        "Returns scope identity, refresh time, Day 1 and current impacted-resource counts, insight name, deletion state, and domain.",
    OperationPlane = ToolOperationPlane.Control,
    Destructive = false,
    Idempotent = true,
    OpenWorld = false,
    ReadOnly = true,
    Secret = false,
    LocalRequired = false)]
public sealed class ServiceRetirementInsightsCommand(
    ILogger<ServiceRetirementInsightsCommand> logger,
    IAdvisorService advisorService)
    : AuthenticatedCommand<
        ServiceRetirementInsightsOptions,
        ServiceRetirementInsightsCommand.ServiceRetirementInsightsResult>()
{
    private const int MaximumFilterLength = 256;
    private const int MinTop = 1;
    private const int MaxTop = 100;
    private readonly ILogger<ServiceRetirementInsightsCommand> _logger = logger;
    private readonly IAdvisorService _advisorService = advisorService;

    public override void ValidateOptions(ServiceRetirementInsightsOptions options, ValidationResult validationResult)
    {
        base.ValidateOptions(options, validationResult);

        ValidateFilter(options.Subscription, "--subscription", validationResult);
        ValidateFilter(options.ServiceGroup, "--service-group", validationResult);

        if (options.Subscription is not null && options.ServiceGroup is not null)
        {
            validationResult.Errors.Add(
                "Specify either --subscription or --service-group, not both. Omit both only for a fleet-wide query.");
        }

        if (options.Top is < MinTop or > MaxTop)
        {
            validationResult.Errors.Add(
                $"--top controls the maximum number of service-retirement insight rows returned and must be between {MinTop} and {MaxTop}.");
        }
    }

    public override async Task<CommandResponse> ExecuteAsync(CommandContext context, ServiceRetirementInsightsOptions options, CancellationToken cancellationToken)
    {
        try
        {
            var insights = await _advisorService.ListServiceRetirementInsightsAsync(
                Normalize(options.Subscription ?? options.ServiceGroup),
                options.Top,
                cancellationToken);

            context.Response.Results = ResponseResult.Create(
                new ServiceRetirementInsightsResult(insights),
                AdvisorJsonContext.Default.ServiceRetirementInsightsResult);
        }
        catch (Exception ex)
        {
            var scopeType = options.Subscription is not null
                ? "Subscription"
                : options.ServiceGroup is not null
                    ? "ServiceGroup"
                    : "All";
            var scope = options.Subscription ?? options.ServiceGroup ?? "All";

            _logger.LogError(
                ex,
                "Error getting service retirement insights. ScopeType: {ScopeType}, Scope: {Scope}.",
                scopeType,
                scope);
            HandleException(context, ex);
        }

        return context.Response;
    }

    private static void ValidateFilter(
        string? value,
        string optionName,
        ValidationResult validationResult)
    {
        if (value is not null && (string.IsNullOrWhiteSpace(value) || value.Length > MaximumFilterLength))
        {
            validationResult.Errors.Add($"{optionName} must be a non-empty value no longer than {MaximumFilterLength} characters.");
        }
    }

    private static string? Normalize(string? value) =>
        string.IsNullOrWhiteSpace(value) ? null : value.Trim();

    protected override string GetErrorMessage(Exception ex) => ex switch
    {
        RequestFailedException reqEx when reqEx.Status == (int)HttpStatusCode.NotFound =>
            "Service Retirement Insights data not found. Verify the subscription or service group, and that you have access.",
        RequestFailedException reqEx when reqEx.Status == (int)HttpStatusCode.Forbidden =>
            $"Authorization failed accessing the Service Retirement Insights data. Verify you have appropriate permissions. Details: {reqEx.Message}",
        RequestFailedException reqEx => reqEx.Message,
        _ => base.GetErrorMessage(ex)
    };

    public sealed record ServiceRetirementInsightsResult(List<ServiceRetirementInsight> Insights);
}
