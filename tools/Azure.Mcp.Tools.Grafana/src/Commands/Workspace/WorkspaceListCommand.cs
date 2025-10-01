// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Core.Commands;
using Azure.Mcp.Core.Commands.Subscription;
using Azure.Mcp.Tools.Grafana.Models;
using Azure.Mcp.Tools.Grafana.Options.Workspace;
using Azure.Mcp.Tools.Grafana.Services;
using Microsoft.Extensions.Logging;

namespace Azure.Mcp.Tools.Grafana.Commands.Workspace;

/// <summary>
/// Lists Azure Managed Grafana workspaces in the specified subscription.
/// </summary>
public sealed class WorkspaceListCommand(ILogger<WorkspaceListCommand> logger) : SubscriptionCommand<WorkspaceListOptions>()
{
    private const string CommandTitle = "List Grafana Workspaces";
    private readonly ILogger<WorkspaceListCommand> _logger = logger;

    public override string Name => "list";

    public override string Description =>
        $"""
        List all Grafana workspace resources in a specified subscription. Returns an array of Grafana workspace details.
        Use this command to explore which Grafana workspace resources are available in your subscription.
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

    public override async Task<CommandResponse> ExecuteAsync(CommandContext context, ParseResult parseResult)
    {
        if (!Validate(parseResult.CommandResult, context.Response).IsValid)
        {
            return context.Response;
        }

        var options = BindOptions(parseResult);

        try
        {
            var grafanaService = context.GetService<IGrafanaService>() ?? throw new InvalidOperationException("Grafana service is not available.");
            var workspaces = await grafanaService.ListWorkspacesAsync(
                options.Subscription!,
                options.Tenant,
                options.RetryPolicy);

            context.Response.Results = ResponseResult.Create(new(workspaces ?? []), GrafanaJsonContext.Default.WorkspaceListCommandResult);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to list Grafana workspaces");

            HandleException(context, ex);
        }

        return context.Response;
    }

    internal record WorkspaceListCommandResult(IEnumerable<GrafanaWorkspace> Workspaces);
}
