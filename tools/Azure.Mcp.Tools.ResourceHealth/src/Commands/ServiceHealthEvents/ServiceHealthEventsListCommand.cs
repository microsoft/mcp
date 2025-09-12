// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Core.Commands;
using Azure.Mcp.Core.Extensions;
using Azure.Mcp.Tools.ResourceHealth.Options.ServiceHealthEvents;
using Azure.Mcp.Tools.ResourceHealth.Services;
using Microsoft.Extensions.Logging;

namespace Azure.Mcp.Tools.ResourceHealth.Commands.ServiceHealthEvents;

/// <summary>
/// Lists Azure service health events for a subscription, providing insights into ongoing or past service issues.
/// </summary>
public sealed class ServiceHealthEventsListCommand(ILogger<ServiceHealthEventsListCommand> logger)
    : BaseResourceHealthCommand<ServiceHealthEventsListOptions>()
{
    private const string CommandTitle = "List Service Health Events";
    private readonly ILogger<ServiceHealthEventsListCommand> _logger = logger;

    public override string Name => "list";

    public override string Description =>
        $"""
        List Azure service health events for a subscription to identify ongoing or past service issues.
        Provides comprehensive information about service incidents, planned maintenance, advisories, and security events.
        Supports filtering by event type, status, tracking ID, and custom OData filters.
        Equivalent to Azure Service Health API for service events.
        """;

    public override string Title => CommandTitle;

    public override ToolMetadata Metadata => new()
    {
        Destructive = false,
        Idempotent = true,
        OpenWorld = true,
        ReadOnly = true,
        LocalRequired = false,
        Secret = false
    };

    protected override void RegisterOptions(Command command)
    {
        base.RegisterOptions(command);
        command.Options.Add(ResourceHealthOptionDefinitions.EventType);
        command.Options.Add(ResourceHealthOptionDefinitions.Status);
        command.Options.Add(ResourceHealthOptionDefinitions.TrackingId);
        command.Options.Add(ResourceHealthOptionDefinitions.Filter);
        command.Options.Add(ResourceHealthOptionDefinitions.QueryStartTime);
        command.Options.Add(ResourceHealthOptionDefinitions.QueryEndTime);

        // Add validators for enum values
        command.Validators.Add(commandResult =>
        {
            // Validate event-type enum values
            if (commandResult.TryGetValue(ResourceHealthOptionDefinitions.EventType, out var eventType) && !string.IsNullOrEmpty(eventType))
            {
                var validEventTypes = new[] { "ServiceIssue", "PlannedMaintenance", "HealthAdvisory", "Security" };
                if (!validEventTypes.Contains(eventType, StringComparer.OrdinalIgnoreCase))
                {
                    commandResult.AddError($"Invalid event-type '{eventType}'. Valid values are: {string.Join(", ", validEventTypes)}");
                }
            }

            // Validate status enum values
            if (commandResult.TryGetValue(ResourceHealthOptionDefinitions.Status, out var status) && !string.IsNullOrEmpty(status))
            {
                var validStatuses = new[] { "Active", "Resolved" };
                if (!validStatuses.Contains(status, StringComparer.OrdinalIgnoreCase))
                {
                    commandResult.AddError($"Invalid status '{status}'. Valid values are: {string.Join(", ", validStatuses)}");
                }
            }
        });
    }

    protected override ServiceHealthEventsListOptions BindOptions(ParseResult parseResult)
    {
        var options = base.BindOptions(parseResult);
        options.EventType = parseResult.GetValueOrDefault(ResourceHealthOptionDefinitions.EventType);
        options.Status = parseResult.GetValueOrDefault(ResourceHealthOptionDefinitions.Status);
        options.TrackingId = parseResult.GetValueOrDefault(ResourceHealthOptionDefinitions.TrackingId);
        options.Filter = parseResult.GetValueOrDefault(ResourceHealthOptionDefinitions.Filter);
        options.QueryStartTime = parseResult.GetValueOrDefault(ResourceHealthOptionDefinitions.QueryStartTime);
        options.QueryEndTime = parseResult.GetValueOrDefault(ResourceHealthOptionDefinitions.QueryEndTime);
        return options;
    }

    public override async Task<CommandResponse> ExecuteAsync(CommandContext context, ParseResult parseResult)
    {
        if (!Validate(parseResult.CommandResult, context.Response).IsValid)
        {
            return context.Response;
        }

        var options = BindOptions(parseResult);

        try
        {
            var resourceHealthService = context.GetService<IResourceHealthService>() ??
                throw new InvalidOperationException("Resource Health service is not available.");

            var events = await resourceHealthService.ListServiceHealthEventsAsync(
                options.Subscription!,
                options.EventType,
                options.Status,
                options.TrackingId,
                options.Filter,
                options.QueryStartTime,
                options.QueryEndTime,
                options.Tenant,
                options.RetryPolicy);

            context.Response.Results = ResponseResult.Create(
                new ServiceHealthEventsListCommandResult(events),
                ResourceHealthJsonContext.Default.ServiceHealthEventsListCommandResult);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to list service health events for subscription {Subscription}", options.Subscription);
            HandleException(context, ex);
        }

        return context.Response;
    }

    internal record ServiceHealthEventsListCommandResult(List<Models.ServiceHealthEvent> Events);
}
