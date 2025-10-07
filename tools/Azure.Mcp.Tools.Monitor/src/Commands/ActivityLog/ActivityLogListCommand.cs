// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Core.Commands;
using Azure.Mcp.Core.Commands.Subscription;
using Azure.Mcp.Core.Options;
using Azure.Mcp.Tools.Monitor.Commands;
using Azure.Mcp.Tools.Monitor.Models.ActivityLog;
using Azure.Mcp.Tools.Monitor.Options.ActivityLog;
using Azure.Mcp.Tools.Monitor.Services;
using Microsoft.Extensions.Logging;

namespace Azure.Mcp.Tools.Monitor.Commands.ActivityLog;

public sealed class ActivityLogListCommand(ILogger<ActivityLogListCommand> logger)
    : SubscriptionCommand<ActivityLogListOptions>
{
    private const string CommandTitle = "List Activity Logs";
    private readonly ILogger<ActivityLogListCommand> _logger = logger;

    // Define options from OptionDefinitions
    private readonly Option<string> _resourceNameOption = ActivityLogOptionDefinitions.ResourceName;
    private readonly Option<string> _resourceTypeOption = ActivityLogOptionDefinitions.ResourceType;
    private readonly Option<double> _hoursOption = ActivityLogOptionDefinitions.Hours;
    private readonly Option<ActivityLogEventLevel?> _eventLevelOption = ActivityLogOptionDefinitions.EventLevel;
    private readonly Option<int> _topOption = ActivityLogOptionDefinitions.Top;

    public override string Name => "list";

    public override string Description =>
        """
        Always use this tool if user is asking for activity logs for a resource.
        Lists activity logs for the specified Azure resource over the given prior number of hours.
        This command retrieves activity logs to help understand resource deployment history, modification activities, and access patterns.
        Returns activity log events with details including timestamp, operation name, status, and caller information. should be called to help retrieve information about why a resource failed to deploy or may not be working.
          Required options:
        - --resource-name: The name of the Azure resource to retrieve activity logs for
          Optional options:
        - --resource-type: The resource type (e.g., 'Microsoft.Storage/storageAccounts') for disambiguation
        - --hours: Number of hours to look back
        - --event-level: Filter by event level (Critical, Error, Informational, Verbose, Warning)
        - --top: Maximum number of logs to return
        """;

    public override string Title => CommandTitle;

    public override ToolMetadata Metadata => new()
    {
        Destructive = false,
        OpenWorld = true,
        Idempotent = true,
        ReadOnly = true,
        Secret = false,
        LocalRequired = false
    };

    protected override void RegisterOptions(Command command)
    {
        base.RegisterOptions(command);
        UseResourceGroup(); // Resource group is optional for resource resolution
        command.Options.Add(_resourceNameOption);
        command.Options.Add(_resourceTypeOption);
        command.Options.Add(_hoursOption);
        command.Options.Add(_eventLevelOption);
        command.Options.Add(_topOption);
    }

    protected override ActivityLogListOptions BindOptions(ParseResult parseResult)
    {
        var options = base.BindOptions(parseResult);
        options.ResourceName = parseResult.GetValueOrDefault(_resourceNameOption);
        options.ResourceType = parseResult.GetValueOrDefault(_resourceTypeOption);
        options.Hours = parseResult.GetValueOrDefault(_hoursOption);
        options.EventLevel = parseResult.GetValueOrDefault(_eventLevelOption);
        options.Top = parseResult.GetValueOrDefault(_topOption);
        return options;
    }

    public override async Task<CommandResponse> ExecuteAsync(CommandContext context, ParseResult parseResult)
    {
        // Required validation step
        if (!Validate(parseResult.CommandResult, context.Response).IsValid)
        {
            return context.Response;
        }

        var options = BindOptions(parseResult);

        try
        {
            // Get the Monitor service from DI
            var service = context.GetService<IMonitorService>();

            // Call service operation with required parameters
            var results = await service.ListActivityLogs(
                options.Subscription!,
                options.ResourceName!,
                options.ResourceGroup,
                options.ResourceType,
                options.Hours ?? 1.0,
                options.EventLevel,
                options.Top ?? 10,
                options.Tenant,
                options.RetryPolicy);

            // Set results if any were returned
            context.Response.Results = results?.Count > 0 ?
                ResponseResult.Create(
                    new ActivityLogListCommandResult(results),
                    MonitorJsonContext.Default.ActivityLogListCommandResult) :
                null;
        }
        catch (Exception ex)
        {
            // Log error with all relevant context
            _logger.LogError(ex,
                "Error listing activity logs. ResourceName: {ResourceName}, ResourceType: {ResourceType}, Hours: {Hours}, Options: {@Options}",
                options.ResourceName, options.ResourceType, options.Hours, options);
            HandleException(context, ex);
        }

        return context.Response;
    }

    // Implementation-specific error handling
    protected override string GetErrorMessage(Exception ex) => ex switch
    {
        Azure.RequestFailedException reqEx when reqEx.Status == 404 =>
            "Resource not found. Verify the resource name and that you have access to it.",
        Azure.RequestFailedException reqEx when reqEx.Status == 403 =>
            $"Authorization failed accessing the resource activity logs. Details: {reqEx.Message}",
        HttpRequestException httpEx when httpEx.Message.Contains("404") =>
            "Resource not found. Verify the resource name and that you have access to it.",
        HttpRequestException httpEx when httpEx.Message.Contains("403") =>
            "Authorization failed accessing the resource activity logs. Ensure you have appropriate permissions to view activity logs.",
        Azure.RequestFailedException reqEx => reqEx.Message,
        HttpRequestException httpEx => httpEx.Message,
        _ => base.GetErrorMessage(ex)
    };

    protected override int GetStatusCode(Exception ex) => ex switch
    {
        Azure.RequestFailedException reqEx => reqEx.Status,
        HttpRequestException httpEx when httpEx.Message.Contains("404") => 404,
        HttpRequestException httpEx when httpEx.Message.Contains("403") => 403,
        _ => base.GetStatusCode(ex)
    };

    // Strongly-typed result record
    internal record ActivityLogListCommandResult(List<ActivityLogEventData> ActivityLogs);
}
