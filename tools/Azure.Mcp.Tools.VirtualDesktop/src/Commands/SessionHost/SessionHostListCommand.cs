// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net;
using Azure.Mcp.Tools.VirtualDesktop.Commands.Hostpool;
using Azure.Mcp.Tools.VirtualDesktop.Options.SessionHost;
using Azure.Mcp.Tools.VirtualDesktop.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Mcp.Core.Commands;
using Microsoft.Mcp.Core.Models.Command;
using Microsoft.Mcp.Core.Models.Option;

namespace Azure.Mcp.Tools.VirtualDesktop.Commands.SessionHost;

[CommandMetadata(
    Id = "6f543101-3c70-41bd-a6ed-5cc4af716081",
    Name = "list",
    Title = "List SessionHosts",
    Description = """
        List all SessionHosts in a hostpool. This command retrieves all Azure Virtual Desktop SessionHost objects available
        in the specified --subscription and hostpool. Results include SessionHost details and are
        returned as a JSON array.
        """,
    Destructive = false,
    Idempotent = true,
    OpenWorld = false,
    ReadOnly = true,
    Secret = false,
    LocalRequired = false)]
public sealed class SessionHostListCommand(ILogger<SessionHostListCommand> logger, IVirtualDesktopService virtualDesktopService) : BaseHostPoolCommand<SessionHostListOptions>
{
    private readonly ILogger<SessionHostListCommand> _logger = logger;
    private readonly IVirtualDesktopService _virtualDesktopService = virtualDesktopService;

    public override async Task<CommandResponse> ExecuteAsync(CommandContext context, ParseResult parseResult, CancellationToken cancellationToken)
    {
        if (!Validate(parseResult.CommandResult, context.Response).IsValid)
        {
            return context.Response;
        }

        var options = BindOptions(parseResult);

        try
        {
            IReadOnlyList<Models.SessionHost> sessionHosts;

            if (!string.IsNullOrEmpty(options.HostPoolResourceId))
            {
                sessionHosts = await _virtualDesktopService.ListSessionHostsByResourceIdAsync(
                    options.Subscription!,
                    options.HostPoolResourceId,
                    options.Tenant,
                    options.RetryPolicy,
                    cancellationToken);
            }
            else if (!string.IsNullOrEmpty(options.ResourceGroup))
            {
                sessionHosts = await _virtualDesktopService.ListSessionHostsByResourceGroupAsync(
                    options.Subscription!,
                    options.ResourceGroup,
                    options.HostPoolName!,
                    options.Tenant,
                    options.RetryPolicy,
                    cancellationToken);
            }
            else
            {
                sessionHosts = await _virtualDesktopService.ListSessionHostsAsync(
                    options.Subscription!,
                    options.HostPoolName!,
                    options.Tenant,
                    options.RetryPolicy,
                    cancellationToken);
            }

            context.Response.Results = ResponseResult.Create(new([.. sessionHosts ?? []]), VirtualDesktopJsonContext.Default.SessionHostListCommandResult);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error listing session hosts for hostpool {HostPoolName} / {HostPoolResourceId}",
                options.HostPoolName, options.HostPoolResourceId);
            HandleException(context, ex);
        }

        return context.Response;
    }

    protected override string GetErrorMessage(Exception ex) => ex switch
    {
        RequestFailedException rfEx when rfEx.Status == (int)HttpStatusCode.NotFound =>
            "Hostpool not found. Verify the hostpool name and that you have access to it.",
        RequestFailedException rfEx when rfEx.Status == (int)HttpStatusCode.Forbidden =>
            "Access denied. Verify you have the necessary permissions to access the hostpool.",
        RequestFailedException rfEx => rfEx.Message,
        _ => base.GetErrorMessage(ex)
    };

    internal record SessionHostListCommandResult(List<Models.SessionHost> SessionHosts);
}
