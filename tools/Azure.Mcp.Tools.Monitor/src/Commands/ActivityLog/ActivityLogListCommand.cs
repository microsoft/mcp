// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net;
using Azure.Mcp.Core.Commands.Subscription;
using Azure.Mcp.Core.Services.Azure.Subscription;
using Azure.Mcp.Tools.Monitor.Models.ActivityLog;
using Azure.Mcp.Tools.Monitor.Options.ActivityLog;
using Azure.Mcp.Tools.Monitor.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Mcp.Core.Commands;
using Microsoft.Mcp.Core.Models.Command;

namespace Azure.Mcp.Tools.Monitor.Commands.ActivityLog;

[CommandMetadata(
    Id = "ffc0ed72-0622-4a27-bfd8-6df9b83adce8",
    Name = "list",
    Title = "List Activity Logs",
    Description = """
        Always use this tool if user is asking for activity logs for a resource.
        Lists activity logs for the specified Azure resource over the given prior number of hours.
        This command retrieves activity logs to help understand resource deployment history, modification activities, and access patterns.
        Returns activity log events with details including timestamp, operation name, status, and caller information. should be called to help retrieve information about why a resource failed to deploy or may not be working.
        """,
    Destructive = false,
    Idempotent = true,
    OpenWorld = false,
    ReadOnly = true,
    Secret = false,
    LocalRequired = false)]
public sealed class ActivityLogListCommand(ILogger<ActivityLogListCommand> logger, IMonitorService monitorService, ISubscriptionResolver subscriptionResolver)
    : SubscriptionCommand<ActivityLogListOptions, ActivityLogListCommand.ActivityLogListCommandResult>(subscriptionResolver)
{
    private readonly ILogger<ActivityLogListCommand> _logger = logger;
    private readonly IMonitorService _monitorService = monitorService;
    public sealed record ActivityLogListCommandResult(List<ActivityLogEventData> ActivityLogs);

    public override async Task<CommandResponse> ExecuteAsync(CommandContext context, ActivityLogListOptions options, CancellationToken cancellationToken)
    {
        try
        {
            // Call service operation with required parameters
            var results = await _monitorService.ListActivityLogs(
                options.Subscription!,
                options.ResourceName,
                options.ResourceGroup,
                options.ResourceType,
                options.Hours ?? 24.0,
                options.EventLevel,
                options.Top ?? 10,
                options.Tenant,
                options.RetryPolicy,
                cancellationToken);

            // Return empty array if no results
            context.Response.Results = ResponseResult.Create(new(results ?? []), MonitorJsonContext.Default.ActivityLogListCommandResult);
        }
        catch (Exception ex)
        {
            // Log error with all relevant context
            _logger.LogError(ex,
                "Error listing activity logs. ResourceName: {ResourceName}, ResourceType: {ResourceType}, Hours: {Hours}.",
                options.ResourceName, options.ResourceType, options.Hours);
            HandleException(context, ex);
        }

        return context.Response;
    }

    // Implementation-specific error handling
    protected override string GetErrorMessage(Exception ex) => ex switch
    {
        RequestFailedException reqEx when reqEx.Status == 404 =>
            "Resource not found. Verify the resource name and that you have access to it.",
        RequestFailedException reqEx when reqEx.Status == 403 =>
            $"Authorization failed accessing the resource activity logs. Details: {reqEx.Message}",
        HttpRequestException httpEx when httpEx.Message.Contains("404") =>
            "Resource not found. Verify the resource name and that you have access to it.",
        HttpRequestException httpEx when httpEx.Message.Contains("403") =>
            "Authorization failed accessing the resource activity logs. Ensure you have appropriate permissions to view activity logs.",
        RequestFailedException reqEx => reqEx.Message,
        HttpRequestException httpEx => httpEx.Message,
        _ => base.GetErrorMessage(ex)
    };

    protected override HttpStatusCode GetStatusCode(Exception ex) => ex switch
    {
        RequestFailedException reqEx => (HttpStatusCode)reqEx.Status,
        HttpRequestException httpEx when httpEx.Message.Contains("404") => (HttpStatusCode)404,
        HttpRequestException httpEx when httpEx.Message.Contains("403") => (HttpStatusCode)403,
        _ => base.GetStatusCode(ex)
    };

}
