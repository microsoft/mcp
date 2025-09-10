// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Core.Commands;
using Azure.Mcp.Tools.Monitor.Options;
using Azure.Mcp.Tools.Monitor.Services;
using Microsoft.Extensions.Logging;

namespace Azure.Mcp.Tools.Monitor.Commands.Log;

public sealed class WorkspaceLogQueryCommand(ILogger<WorkspaceLogQueryCommand> logger) : BaseMonitorCommand<WorkspaceLogQueryOptions>()
{
    private const string CommandTitle = "Query Log Analytics Workspace";
    private readonly ILogger<WorkspaceLogQueryCommand> _logger = logger;
    private readonly Option<string> _tableNameOption = MonitorOptionDefinitions.TableName;
    private readonly Option<string> _queryOption = MonitorOptionDefinitions.Query;
    private readonly Option<int> _hoursOption = MonitorOptionDefinitions.Hours;
    private readonly Option<int> _limitOption = MonitorOptionDefinitions.Limit;

    public override string Name => "query";

    public override string Description =>
        $"""
        Execute a KQL query against a Log Analytics workspace. Requires {WorkspaceOptionDefinitions.WorkspaceIdOrName}
        and resource group. Optional {MonitorOptionDefinitions.HoursName}
        (default: {MonitorOptionDefinitions.Hours.GetDefaultValue()}) and {MonitorOptionDefinitions.LimitName}
        (default: {MonitorOptionDefinitions.Limit.GetDefaultValue()}) parameters.
        The {MonitorOptionDefinitions.QueryTextName} parameter accepts KQL syntax.
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
        command.Options.Add(_tableNameOption);
        command.Options.Add(_queryOption);
        command.Options.Add(_hoursOption);
        command.Options.Add(_limitOption);
    }

    protected override WorkspaceLogQueryOptions BindOptions(ParseResult parseResult)
    {
        var options = base.BindOptions(parseResult);
        options.TableName = parseResult.GetValueOrDefault(_tableNameOption);
        options.Query = parseResult.GetValueOrDefault(_queryOption);
        options.Hours = parseResult.GetValueOrDefault(_hoursOption);
        options.Limit = parseResult.GetValueOrDefault(_limitOption);
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
            var results = await monitorService.QueryWorkspaceLogs(
                options.Subscription!,
                options.Workspace!,
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
            _logger.LogError(ex, "Error executing log query command.");
            HandleException(context, ex);
        }

        return context.Response;
    }
}
