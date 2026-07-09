// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Core.Commands.Subscription;
using Azure.Mcp.Core.Services.Azure.Subscription;
using Azure.Mcp.Tools.Monitor.Options;
using Azure.Mcp.Tools.Monitor.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Mcp.Core.Commands;
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
public sealed class TableListCommand(ILogger<TableListCommand> logger, IMonitorService monitorService, ISubscriptionResolver subscriptionResolver)
    : SubscriptionCommand<TableListOptions, TableListCommand.TableListCommandResult>(subscriptionResolver)
{
    private readonly ILogger<TableListCommand> _logger = logger;
    private readonly IMonitorService _monitorService = monitorService;

    public override async Task<CommandResponse> ExecuteAsync(CommandContext context, TableListOptions options, CancellationToken cancellationToken)
    {
        try
        {
            var tables = await _monitorService.ListTables(
                options.Subscription!,
                options.ResourceGroup,
                options.Workspace,
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

    public sealed record TableListCommandResult(List<string> Tables);
}
