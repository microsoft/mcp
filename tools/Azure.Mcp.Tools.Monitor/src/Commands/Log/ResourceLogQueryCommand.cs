// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Core.Commands;
using Azure.Mcp.Core.Commands.Subscription;
using Azure.Mcp.Tools.Monitor.Options;
using Azure.Mcp.Tools.Monitor.Services;
using Microsoft.Extensions.Logging;

namespace Azure.Mcp.Tools.Monitor.Commands.Log;

public sealed class ResourceLogQueryCommand(ILogger<ResourceLogQueryCommand> logger) : SubscriptionCommand<ResourceLogQueryOptions>()
{
    private const string CommandTitle = "Query Logs for Azure Resource";
    private readonly ILogger<ResourceLogQueryCommand> _logger = logger;

    public override string Name => "query";

    public override string Description =>
        $"""
        Executes a Kusto Query Language (KQL) query to retrieve logs for any Azure resource that emits logs to Log Analytics.

        - Use the {ResourceLogQueryOptionDefinitions.ResourceIdName} parameter to specify the full Azure Resource ID (/subscriptions/0000/resourceGroups/rg/providers/Microsoft.Storage/storageAccounts/myaccount).
        - The {MonitorOptionDefinitions.TableNameName} parameter specifies the Log Analytics table to query.
        - The {MonitorOptionDefinitions.QueryTextName} parameter accepts a KQL query or a predefined query name.
        - Optional parameters: {MonitorOptionDefinitions.HoursName} (default: {MonitorOptionDefinitions.Hours.GetDefaultValue()}) to set the time range, and {MonitorOptionDefinitions.LimitName} (default: {MonitorOptionDefinitions.Limit.GetDefaultValue()}) to limit the number of results.

        This tool is useful for:
        - Querying logs for any Azure resource by resourceId
        - Investigating diagnostics, errors, and activity logs
        """;

    public override string Title => CommandTitle;

    public override ToolMetadata Metadata => new()
    {
        Destructive = false,
        Idempotent = true,
        OpenWorld = false,
        ReadOnly = true,
        LocalRequired = false,
        Secret = false
    };

    protected override void RegisterOptions(Command command)
    {
        base.RegisterOptions(command);
        command.Options.Add(ResourceLogQueryOptionDefinitions.ResourceId);
        command.Options.Add(MonitorOptionDefinitions.TableName);
        command.Options.Add(MonitorOptionDefinitions.Query);
        command.Options.Add(MonitorOptionDefinitions.Hours);
        command.Options.Add(MonitorOptionDefinitions.Limit);
    }

    protected override ResourceLogQueryOptions BindOptions(ParseResult parseResult)
    {
        var options = base.BindOptions(parseResult);
        options.ResourceId = parseResult.GetValueOrDefault<string>(ResourceLogQueryOptionDefinitions.ResourceId.Name);
        options.TableName = parseResult.GetValueOrDefault<string>(MonitorOptionDefinitions.TableName.Name);
        options.Query = parseResult.GetValueOrDefault<string>(MonitorOptionDefinitions.Query.Name);
        options.Hours = parseResult.GetValueOrDefault<int>(MonitorOptionDefinitions.Hours.Name);
        options.Limit = parseResult.GetValueOrDefault<int>(MonitorOptionDefinitions.Limit.Name);
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
            var monitorService = context.GetService<IMonitorService>();
            var results = await monitorService.QueryResourceLogs(
                options.Subscription!,
                options.ResourceId!,
                options.Query!,
                options.TableName!,
                options.Hours,
                options.Limit,
                options.Tenant,
                options.RetryPolicy);

            context.Response.Results = ResponseResult.Create(results, MonitorJsonContext.Default.ListJsonNode);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error executing log query resource command.");
            HandleException(context, ex);
        }

        return context.Response;
    }
}
