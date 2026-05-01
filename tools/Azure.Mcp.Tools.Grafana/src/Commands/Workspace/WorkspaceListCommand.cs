// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Core.Commands.Subscription;
using Azure.Mcp.Tools.Grafana.Models;
using Azure.Mcp.Tools.Grafana.Options.Workspace;
using Azure.Mcp.Tools.Grafana.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Mcp.Core.Commands;
using Microsoft.Mcp.Core.Extensions;
using Microsoft.Mcp.Core.Models.Command;
using Microsoft.Mcp.Core.Models.Option;

namespace Azure.Mcp.Tools.Grafana.Commands.Workspace;

/// <summary>
/// Lists Azure Managed Grafana workspaces in the specified subscription.
/// </summary>
[CommandMetadata(
    Id = "7a47b562-f219-47de-80f6-12e19367b61d",
    Name = "list",
    Title = "List Grafana Workspaces",
    Description = """
        List all Grafana workspace resources in a specified subscription. Returns an array of Grafana workspace details.
        Use this command to explore which Grafana workspace resources are available in your subscription.
        """,
    Destructive = false,
    Idempotent = true,
    OpenWorld = false,
    ReadOnly = true,
    Secret = false,
    LocalRequired = false)]
public sealed class WorkspaceListCommand(IGrafanaService grafanaService, ILogger<WorkspaceListCommand> logger) : SubscriptionCommand<WorkspaceListOptions>()
{
    private readonly IGrafanaService _grafanaService = grafanaService;
    private readonly ILogger<WorkspaceListCommand> _logger = logger;

    protected override void RegisterOptions(Command command)
    {
        base.RegisterOptions(command);
        command.Options.Add(OptionDefinitions.Common.ResourceGroup.AsOptional());
    }

    protected override WorkspaceListOptions BindOptions(ParseResult parseResult)
    {
        var options = base.BindOptions(parseResult);
        options.ResourceGroup = parseResult.GetValueOrDefault<string>(OptionDefinitions.Common.ResourceGroup.Name);
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
            var workspaces = await _grafanaService.ListWorkspacesAsync(
                options.Subscription!,
                options.ResourceGroup,
                options.Tenant,
                options.RetryPolicy,
                cancellationToken);

            context.Response.Results = ResponseResult.Create(new(workspaces?.Results ?? [], workspaces?.AreResultsTruncated ?? false), GrafanaJsonContext.Default.WorkspaceListCommandResult);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to list Grafana workspaces");

            HandleException(context, ex);
        }

        return context.Response;
    }

    internal record WorkspaceListCommandResult(IEnumerable<GrafanaWorkspace> Workspaces, bool AreResultsTruncated);
}
