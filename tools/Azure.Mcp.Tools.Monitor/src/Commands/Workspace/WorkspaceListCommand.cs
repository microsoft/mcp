// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Core.Commands.Subscription;
using Azure.Mcp.Core.Services.Azure.Subscription;
using Azure.Mcp.Tools.Monitor.Models;
using Azure.Mcp.Tools.Monitor.Options;
using Azure.Mcp.Tools.Monitor.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Mcp.Core.Commands;
using Microsoft.Mcp.Core.Models.Command;

namespace Azure.Mcp.Tools.Monitor.Commands.Workspace;

[CommandMetadata(
    Id = "0c76b74e-14bf-4e0c-ab10-4bbeeb53347b",
    Name = "list",
    Title = "List Log Analytics Workspaces",
    Description = """
        List Log Analytics workspaces in a subscription. This command retrieves all Log Analytics workspaces
        available in the specified Azure subscription, displaying their names, IDs, and other key properties.
        Use this command to identify workspaces before querying their logs or tables.
        """,
    Destructive = false,
    Idempotent = true,
    OpenWorld = false,
    ReadOnly = true,
    Secret = false,
    LocalRequired = false)]
public sealed class WorkspaceListCommand(ILogger<WorkspaceListCommand> logger, IMonitorService monitorService, ISubscriptionResolver subscriptionResolver)
    : SubscriptionCommand<WorkspaceListOptions, WorkspaceListCommand.WorkspaceListCommandResult>(subscriptionResolver)
{
    private readonly ILogger<WorkspaceListCommand> _logger = logger;
    private readonly IMonitorService _monitorService = monitorService;

    public override async Task<CommandResponse> ExecuteAsync(CommandContext context, WorkspaceListOptions options, CancellationToken cancellationToken)
    {
        try
        {
            var workspaces = await _monitorService.ListWorkspaces(
                options.Subscription!,
                options.ResourceGroup,
                options.Tenant,
                options.RetryPolicy,
                cancellationToken);

            context.Response.Results = ResponseResult.Create(new(workspaces ?? []), MonitorJsonContext.Default.WorkspaceListCommandResult);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error listing workspaces.");
            HandleException(context, ex);
        }

        return context.Response;
    }

    public sealed record WorkspaceListCommandResult(List<WorkspaceInfo> Workspaces);
}
