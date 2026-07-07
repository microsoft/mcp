// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Core.Services.Azure.Subscription;
using Azure.Mcp.Tools.ResourceHealth.Options.ServiceHealthEvents;
using Azure.Mcp.Tools.ResourceHealth.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Mcp.Core.Commands;
using Microsoft.Mcp.Core.Models.Command;

namespace Azure.Mcp.Tools.ResourceHealth.Commands.ServiceHealthEvents;

/// <summary>
/// Lists Azure service health events for a subscription, providing insights into ongoing or past service issues.
/// </summary>
[CommandMetadata(
    Id = "c3211c73-af20-4d8d-bed2-4f181e0e4c92",
    Name = "list",
    Title = "List Service Health Events",
    Description = "List Azure service health events to track service issues that occurred in recent timeframes (last 30 days, weeks, months). Query subscription for planned maintenance, past or ongoing service incidents, advisories, and security events. Provides detailed information about resource availability state, potential issues, and timestamps. Returns: trackingId, title, summary, eventType, status, startTime, endTime, impactedServices. Access Azure Service Health portal data programmatically.",
    Destructive = false,
    Idempotent = true,
    OpenWorld = false,
    ReadOnly = true,
    Secret = false,
    LocalRequired = false)]
public sealed class ServiceHealthEventsListCommand(ILogger<ServiceHealthEventsListCommand> logger, IResourceHealthService resourceHealthService, ISubscriptionResolver subscriptionResolver)
    : BaseResourceHealthCommand<ServiceHealthEventsListOptions, ServiceHealthEventsListCommand.ServiceHealthEventsListCommandResult>(subscriptionResolver)
{
    private readonly ILogger<ServiceHealthEventsListCommand> _logger = logger;
    private readonly IResourceHealthService _resourceHealthService = resourceHealthService;

    private static readonly HashSet<string> s_validEventTypes = new(StringComparer.OrdinalIgnoreCase) { "ServiceIssue", "PlannedMaintenance", "HealthAdvisory", "Security" };
    private static readonly HashSet<string> s_validStatuses = new(StringComparer.OrdinalIgnoreCase) { "Active", "Resolved" };

    public override void ValidateOptions(ServiceHealthEventsListOptions options, ValidationResult validationResult)
    {
        base.ValidateOptions(options, validationResult);

        // Validate event-type enum values
        if (!string.IsNullOrEmpty(options.EventType) && !s_validEventTypes.Contains(options.EventType))
        {
            validationResult.Errors.Add($"Invalid event-type '{options.EventType}'. Valid values are: {string.Join(", ", s_validEventTypes)}");
        }

        // Validate status enum values
        if (!string.IsNullOrEmpty(options.Status) && !s_validStatuses.Contains(options.Status))
        {
            validationResult.Errors.Add($"Invalid status '{options.Status}'. Valid values are: {string.Join(", ", s_validStatuses)}");
        }
    }

    public override async Task<CommandResponse> ExecuteAsync(CommandContext context, ServiceHealthEventsListOptions options, CancellationToken cancellationToken)
    {
        try
        {
            var events = await _resourceHealthService.ListServiceHealthEventsAsync(
                options.Subscription!,
                options.EventType,
                options.Status,
                options.TrackingId,
                options.Filter,
                options.QueryStartTime,
                options.QueryEndTime,
                options.Tenant,
                options.RetryPolicy,
                cancellationToken);

            context.Response.Results = ResponseResult.Create(new(events ?? []), ResourceHealthJsonContext.Default.ServiceHealthEventsListCommandResult);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to list service health events for subscription {Subscription}", options.Subscription);
            HandleException(context, ex);
        }

        return context.Response;
    }

    public sealed record ServiceHealthEventsListCommandResult(List<Models.ServiceHealthEvent> Events);
}
