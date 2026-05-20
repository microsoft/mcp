// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Tools.Monitor.Options;
using Azure.Mcp.Tools.Monitor.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Mcp.Core.Commands;
using Microsoft.Mcp.Core.Extensions;
using Microsoft.Mcp.Core.Models.Command;

namespace Azure.Mcp.Tools.Monitor.Commands.Table;

[CommandMetadata(
    Id = "2b1ae0be-d6dd-4db9-9c58-fc4fcb3bf8e6",
    Name = "list",
    Title = "List Log Analytics Tables",
    Description = """
        List all tables in a Log Analytics workspace. Requires workspace.
        Returns table names and schemas that can be used for constructing KQL queries.
        """,
    Destructive = false,
    Idempotent = true,
    OpenWorld = false,
    ReadOnly = true,
    Secret = false,
    LocalRequired = false)]
public sealed class TableListCommand(ILogger<TableListCommand> logger, IMonitorService monitorService) : BaseWorkspaceMonitorCommand<TableListOptions>()
{
    private readonly ILogger<TableListCommand> _logger = logger;
    private readonly IMonitorService _monitorService = monitorService;

    protected override void RegisterOptions(Command command)
    {
        base.RegisterOptions(command);
        command.Options.Add(MonitorOptionDefinitions.TableType);
    }

    protected override TableListOptions BindOptions(ParseResult parseResult)
    {
        var options = base.BindOptions(parseResult);
        options.TableType = parseResult.GetValueOrDefault<string>(MonitorOptionDefinitions.TableType.Name);
        return options;
    }

    public override async Task<CommandResponse> ExecuteAsync(CommandContext context, ParseResult parseResult, CancellationToken cancellationToken)
    {
        if (!Validate(parseResult.CommandResult, context.Response).IsValid)
        {
            return context.Response;
        }

        var options = BindOptions(parseResult);

        try
        {
            var tables = await _monitorService.ListTables(
                options.Subscription!,
                options.ResourceGroup!,
                options.Workspace!,
                options.TableType,
                options.Tenant,
                options.RetryPolicy,
                cancellationToken);

            context.Response.Results = ResponseResult.Create(new(tables ?? []), MonitorJsonContext.Default.TableListCommandResult);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error listing tables.");
            HandleException(context, ex);
        }

        return context.Response;
    }

    internal record TableListCommandResult(List<string> Tables);
}
