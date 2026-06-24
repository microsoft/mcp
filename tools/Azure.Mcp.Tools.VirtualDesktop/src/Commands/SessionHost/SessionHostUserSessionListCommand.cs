// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net;
using Azure.Mcp.Core.Services.Azure.Subscription;
using Azure.Mcp.Tools.VirtualDesktop.Commands.Hostpool;
using Azure.Mcp.Tools.VirtualDesktop.Models;
using Azure.Mcp.Tools.VirtualDesktop.Options.SessionHost;
using Azure.Mcp.Tools.VirtualDesktop.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Mcp.Core.Commands;
using Microsoft.Mcp.Core.Models.Command;

namespace Azure.Mcp.Tools.VirtualDesktop.Commands.SessionHost;

[CommandMetadata(
    Id = "1653a208-ac9f-4e51-996f-fe2d29a79b2b",
    Name = "user-list",
    Title = "List User Sessions on Session Host",
    Description = """
        List all user sessions on a specific session host in a host pool. This command retrieves all Azure Virtual Desktop
        user session objects available on the specified session host. Results include user session details such as
        user principal name, session state, application type, and creation time.
        """,
    Destructive = false,
    Idempotent = true,
    OpenWorld = false,
    ReadOnly = true,
    Secret = false,
    LocalRequired = false)]
public sealed class SessionHostUserSessionListCommand(ILogger<SessionHostUserSessionListCommand> logger, IVirtualDesktopService virtualDesktopService, ISubscriptionResolver subscriptionResolver)
    : BaseHostPoolCommand<SessionHostUserSessionListOptions, SessionHostUserSessionListCommand.SessionHostUserSessionListCommandResult>(subscriptionResolver)
{
    private readonly ILogger<SessionHostUserSessionListCommand> _logger = logger;
    private readonly IVirtualDesktopService _virtualDesktopService = virtualDesktopService;

    public override async Task<CommandResponse> ExecuteAsync(CommandContext context, SessionHostUserSessionListOptions options, CancellationToken cancellationToken)
    {
        try
        {
            IReadOnlyList<UserSession> userSessions;

            if (!string.IsNullOrEmpty(options.HostpoolResourceId))
            {
                userSessions = await _virtualDesktopService.ListUserSessionsByResourceIdAsync(
                    options.Subscription!,
                    options.HostpoolResourceId,
                    options.Sessionhost,
                    options.Tenant,
                    options.RetryPolicy,
                    cancellationToken);
            }
            else if (!string.IsNullOrEmpty(options.ResourceGroup))
            {
                userSessions = await _virtualDesktopService.ListUserSessionsByResourceGroupAsync(
                    options.Subscription!,
                    options.ResourceGroup,
                    options.Hostpool!,
                    options.Sessionhost,
                    options.Tenant,
                    options.RetryPolicy,
                    cancellationToken);
            }
            else
            {
                userSessions = await _virtualDesktopService.ListUserSessionsAsync(
                    options.Subscription!,
                    options.Hostpool!,
                    options.Sessionhost,
                    options.Tenant,
                    options.RetryPolicy,
                    cancellationToken);
            }

            context.Response.Results = ResponseResult.Create(new([.. userSessions ?? []]), VirtualDesktopJsonContext.Default.SessionHostUserSessionListCommandResult);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error listing user sessions for session host {SessionHostName} in hostpool {HostPoolName} / {HostPoolResourceId}",
                options.Sessionhost, options.Hostpool, options.HostpoolResourceId);
            HandleException(context, ex);
        }

        return context.Response;
    }

    protected override string GetErrorMessage(Exception ex) => ex switch
    {
        RequestFailedException rfEx when rfEx.Status == (int)HttpStatusCode.NotFound =>
            "Session host or hostpool not found. Verify the names and that you have access to them.",
        RequestFailedException rfEx when rfEx.Status == (int)HttpStatusCode.Forbidden =>
            "Access denied. Verify you have the necessary permissions to access the session host and hostpool.",
        RequestFailedException rfEx => rfEx.Message,
        _ => base.GetErrorMessage(ex)
    };

    public sealed record SessionHostUserSessionListCommandResult(List<UserSession> UserSessions);
}
