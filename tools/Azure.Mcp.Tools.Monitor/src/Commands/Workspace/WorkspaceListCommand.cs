// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Core.Commands;
using Azure.Mcp.Core.Commands.Subscription;
using Azure.Mcp.Tools.Monitor.Models;
using Azure.Mcp.Tools.Monitor.Options;
using Azure.Mcp.Tools.Monitor.Services;
using Microsoft.Extensions.Logging;

namespace Azure.Mcp.Tools.Monitor.Commands.Workspace;

public sealed class WorkspaceListCommand(ILogger<WorkspaceListCommand> logger) : SubscriptionCommand<WorkspaceListOptions>()
{
    private const string CommandTitle = "List Log Analytics Workspaces";
    private readonly ILogger<WorkspaceListCommand> _logger = logger;

    public override string Name => "list";

    public override string Description =>
        """
        List Log Analytics workspaces in a subscription. This command retrieves all Log Analytics workspaces
        available in the specified Azure subscription, displaying their names, IDs, and other key properties.
        Use this command to identify workspaces before querying their logs or tables.
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
            var workspaces = await monitorService.ListWorkspaces(
                options.Subscription!,
                options.Tenant,
                options.RetryPolicy);

            context.Response.Results = workspaces?.Count > 0 ?
                ResponseResult.Create(
                    new WorkspaceListCommandResult(workspaces),
                    MonitorJsonContext.Default.WorkspaceListCommandResult) :
                null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error listing workspaces.");
            HandleException(context, ex);
        }

        return context.Response;
    }

    internal record WorkspaceListCommandResult(List<WorkspaceInfo> Workspaces);
}
