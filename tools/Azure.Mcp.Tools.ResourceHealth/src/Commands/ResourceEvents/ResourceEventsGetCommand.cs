// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Core.Commands;
using Azure.Mcp.Core.Extensions;
using Azure.Mcp.Tools.ResourceHealth.Options.ResourceEvents;
using Azure.Mcp.Tools.ResourceHealth.Services;
using Microsoft.Extensions.Logging;

namespace Azure.Mcp.Tools.ResourceHealth.Commands.ResourceEvents;

/// <summary>
/// Gets historical availability events for a specific Azure resource to analyze past health issues and patterns.
/// </summary>
public sealed class ResourceEventsGetCommand(ILogger<ResourceEventsGetCommand> logger)
    : BaseResourceHealthCommand<ResourceEventsGetOptions>()
{
    private const string CommandTitle = "Get Resource Events";
    private readonly ILogger<ResourceEventsGetCommand> _logger = logger;

    public override string Name => "get";

    public override string Description =>
        $"""
        Get historical availability events for a specific Azure resource to analyze past health issues and patterns.
        Provides detailed information about resource availability state changes, incidents, and maintenance events over time.
        Supports filtering by time range and custom OData filters for targeted analysis.
        Equivalent to Azure Resource Health historical events API.
        """;

    public override string Title => CommandTitle;

    public override ToolMetadata Metadata => new() { Destructive = false, ReadOnly = true };

    protected override void RegisterOptions(Command command)
    {
        base.RegisterOptions(command);
        command.Options.Add(ResourceHealthOptionDefinitions.ResourceId);
        command.Options.Add(ResourceHealthOptionDefinitions.QueryStartTime);
        command.Options.Add(ResourceHealthOptionDefinitions.QueryEndTime);
        command.Options.Add(ResourceHealthOptionDefinitions.Filter);
    }

    protected override ResourceEventsGetOptions BindOptions(ParseResult parseResult)
    {
        var options = base.BindOptions(parseResult);
        options.ResourceId = parseResult.GetValueOrDefault(ResourceHealthOptionDefinitions.ResourceId);
        options.QueryStartTime = parseResult.GetValueOrDefault(ResourceHealthOptionDefinitions.QueryStartTime);
        options.QueryEndTime = parseResult.GetValueOrDefault(ResourceHealthOptionDefinitions.QueryEndTime);
        options.Filter = parseResult.GetValueOrDefault(ResourceHealthOptionDefinitions.Filter);
        return options;
    }

    public override async Task<CommandResponse> ExecuteAsync(CommandContext context, ParseResult parseResult)
    {
        if (!Validate(parseResult.CommandResult, context.Response).IsValid)
        {
            return context.Response;
        }

        var options = BindOptions(parseResult);

        // Additional validation for this command
        if (string.IsNullOrWhiteSpace(options.ResourceId))
        {
            context.Response.Status = 400;
            context.Response.Message = "Resource ID is required";
            return context.Response;
        }

        // Validate date format if provided
        if (!string.IsNullOrWhiteSpace(options.QueryStartTime) && 
            !DateTimeOffset.TryParse(options.QueryStartTime, out _))
        {
            context.Response.Status = 400;
            context.Response.Message = "Invalid query start time format. Use ISO 8601 format (e.g., 2024-01-01T00:00:00Z)";
            return context.Response;
        }

        if (!string.IsNullOrWhiteSpace(options.QueryEndTime) && 
            !DateTimeOffset.TryParse(options.QueryEndTime, out _))
        {
            context.Response.Status = 400;
            context.Response.Message = "Invalid query end time format. Use ISO 8601 format (e.g., 2024-01-31T23:59:59Z)";
            return context.Response;
        }

        // Validate that start time is before end time if both are provided
        if (!string.IsNullOrWhiteSpace(options.QueryStartTime) && 
            !string.IsNullOrWhiteSpace(options.QueryEndTime) &&
            DateTimeOffset.TryParse(options.QueryStartTime, out var startTime) &&
            DateTimeOffset.TryParse(options.QueryEndTime, out var endTime) &&
            startTime >= endTime)
        {
            context.Response.Status = 400;
            context.Response.Message = "Query start time must be before query end time";
            return context.Response;
        }

        try
        {
            var resourceHealthService = context.GetService<IResourceHealthService>() ??
                throw new InvalidOperationException("Resource Health service is not available.");

            var events = await resourceHealthService.GetResourceEventsAsync(
                options.ResourceId!,
                options.QueryStartTime,
                options.QueryEndTime,
                options.Filter,
                options.RetryPolicy);

            context.Response.Results = ResponseResult.Create(
                new ResourceEventsGetCommandResult(events),
                ResourceHealthJsonContext.Default.ResourceEventsGetCommandResult);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get resource events for resource {ResourceId}", options.ResourceId);
            HandleException(context, ex);
        }

        return context.Response;
    }

    internal record ResourceEventsGetCommandResult(List<Models.ResourceEvent> Events);
}
