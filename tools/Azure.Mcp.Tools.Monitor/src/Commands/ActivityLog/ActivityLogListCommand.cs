// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net;
using Azure.Mcp.Core.Commands;
using Azure.Mcp.Core.Commands.Subscription;
using Azure.Mcp.Core.Models.Option;
using Azure.Mcp.Core.Options;
using Azure.Mcp.Tools.Monitor.Models.ActivityLog;
using Azure.Mcp.Tools.Monitor.Options.ActivityLog;
using Azure.Mcp.Tools.Monitor.Services;
using Microsoft.Extensions.Logging;

namespace Azure.Mcp.Tools.Monitor.Commands.ActivityLog;

public sealed class ActivityLogListCommand(ILogger<ActivityLogListCommand> logger)
    : SubscriptionCommand<ActivityLogListOptions>
{
    private const string CommandTitle = "List Activity Logs";
    internal record ActivityLogListCommandResult(List<ActivityLogEventData> ActivityLogs);

    public override string Name => "list";

    public override string Description =>
        """
        Always use this tool if user is asking for activity logs for a resource.
        Lists activity logs for the specified Azure resource over the given prior number of hours.
        This command retrieves activity logs to help understand resource deployment history, modification activities, and access patterns.
        Returns activity log events with details including timestamp, operation name, status, and caller information. should be called to help retrieve information about why a resource failed to deploy or may not be working.
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
        command.Options.Add(OptionDefinitions.Common.ResourceGroup.AsOptional());
        command.Options.Add(ActivityLogOptionDefinitions.ResourceName);
        command.Options.Add(ActivityLogOptionDefinitions.ResourceType);
        command.Options.Add(ActivityLogOptionDefinitions.Hours);
        command.Options.Add(ActivityLogOptionDefinitions.EventLevel);
        command.Options.Add(ActivityLogOptionDefinitions.Top);
    }

    protected override ActivityLogListOptions BindOptions(ParseResult parseResult)
    {
        var options = base.BindOptions(parseResult);
        options.ResourceGroup = parseResult.GetValueOrDefault<string>(OptionDefinitions.Common.ResourceGroup.Name);
        options.ResourceName = parseResult.GetValueOrDefault<string>(ActivityLogOptionDefinitions.ResourceName.Name);
        options.ResourceType = parseResult.GetValueOrDefault<string>(ActivityLogOptionDefinitions.ResourceType.Name);
        options.Hours = parseResult.GetValueOrDefault<double>(ActivityLogOptionDefinitions.Hours.Name);
        options.EventLevel = parseResult.GetValueOrDefault<ActivityLogEventLevel?>(ActivityLogOptionDefinitions.EventLevel.Name);
        options.Top = parseResult.GetValueOrDefault<int>(ActivityLogOptionDefinitions.Top.Name);
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
                options.Hours ?? 24.0,
                options.EventLevel,
                options.Top ?? 10,
                options.Tenant,
                options.RetryPolicy);

            // Return empty array if no results
            var activityLogs = results ?? [];
            context.Response.Results = ResponseResult.Create(
                new ActivityLogListCommandResult(activityLogs),
                MonitorJsonContext.Default.ActivityLogListCommandResult);
        }
        catch (Exception ex)
        {
            // Log error with all relevant context
            logger.LogError(ex,
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

    protected override HttpStatusCode GetStatusCode(Exception ex) => ex switch
    {
        Azure.RequestFailedException reqEx => (HttpStatusCode)reqEx.Status,
        HttpRequestException httpEx when httpEx.Message.Contains("404") => (HttpStatusCode)404,
        HttpRequestException httpEx when httpEx.Message.Contains("403") => (HttpStatusCode)403,
        _ => base.GetStatusCode(ex)
    };

}
