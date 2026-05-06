// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Core.Commands.Subscription;
using Azure.Mcp.Tools.Monitor.Models;
using Azure.Mcp.Tools.Monitor.Options;
using Azure.Mcp.Tools.Monitor.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Mcp.Core.Commands;
using System.Text.Json.Serialization.Metadata;
using Microsoft.Mcp.Core.Models.Command;

namespace Azure.Mcp.Tools.Monitor.Commands.Workspace;

public sealed class WorkspaceListCommand(ILogger<WorkspaceListCommand> logger, IMonitorService monitorService) : SubscriptionCommand<WorkspaceListOptions, WorkspaceListCommand.WorkspaceListCommandResult>()
{
    protected override JsonTypeInfo<WorkspaceListCommandResult> ResultTypeInfo => MonitorJsonContext.Default.WorkspaceListCommandResult;
    private const string CommandTitle = "List Log Analytics Workspaces";
    private readonly ILogger<WorkspaceListCommand> _logger = logger;
    private readonly IMonitorService _monitorService = monitorService;

    public override string Id => "0c76b74e-14bf-4e0c-ab10-4bbeeb53347b";

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
        OpenWorld = false,
        ReadOnly = true,
        LocalRequired = false,
        Secret = false
    };

    public override async Task<CommandResponse> ExecuteAsync(CommandContext context, ParseResult parseResult, CancellationToken cancellationToken)
    {
        if (!Validate(parseResult.CommandResult, context.Response).IsValid)
        {
            return context.Response;
        }

        var options = BindOptions(parseResult);

        try
        {
            var workspaces = await _monitorService.ListWorkspaces(
                options.Subscription!,
                options.Tenant,
                options.RetryPolicy,
                cancellationToken);

            SetResult(context, new(workspaces ?? []));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error listing workspaces.");
            HandleException(context, ex);
        }

        return context.Response;
    }

    public record WorkspaceListCommandResult(List<WorkspaceInfo> Workspaces);
}
