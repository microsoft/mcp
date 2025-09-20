// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Core.Commands;
using Azure.Mcp.Tools.VirtualDesktop.Models;
using Azure.Mcp.Tools.VirtualDesktop.Services;
using Microsoft.Extensions.Logging;

namespace Azure.Mcp.Tools.VirtualDesktop.Commands.SessionHost;

public sealed class SessionHostUserSessionListCommand(ILogger<SessionHostUserSessionListCommand> logger)
    : BaseSessionHostCommand
{
    private const string CommandTitle = "List User Sessions on Session Host";
    private readonly ILogger<SessionHostUserSessionListCommand> _logger = logger;

    public override string Name => "usersession-list";

    public override string Description =>
        """
		List all user sessions on a specific session host in a host pool. This command retrieves all Azure Virtual Desktop
		user session objects available on the specified session host. Results include user session details such as
		user principal name, session state, application type, and creation time.
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
            var virtualDesktopService = context.GetService<IVirtualDesktopService>();
            IReadOnlyList<UserSession> userSessions;

            if (!string.IsNullOrEmpty(options.HostPoolResourceId))
            {
                userSessions = await virtualDesktopService.ListUserSessionsByResourceIdAsync(
                    options.Subscription!,
                    options.HostPoolResourceId,
                    options.SessionHostName!,
                    options.Tenant,
                    options.RetryPolicy);
            }
            else if (!string.IsNullOrEmpty(options.ResourceGroup))
            {
                userSessions = await virtualDesktopService.ListUserSessionsByResourceGroupAsync(
                    options.Subscription!,
                    options.ResourceGroup,
                    options.HostPoolName!,
                    options.SessionHostName!,
                    options.Tenant,
                    options.RetryPolicy);
            }
            else
            {
                userSessions = await virtualDesktopService.ListUserSessionsAsync(
                    options.Subscription!,
                    options.HostPoolName!,
                    options.SessionHostName!,
                    options.Tenant,
                    options.RetryPolicy);
            }

            context.Response.Results = ResponseResult.Create(new([.. userSessions ?? []]), VirtualDesktopJsonContext.Default.SessionHostUserSessionListCommandResult);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error listing user sessions for session host {SessionHostName} in hostpool {HostPoolName} / {HostPoolResourceId}",
                options.SessionHostName, options.HostPoolName, options.HostPoolResourceId);
            HandleException(context, ex);
        }

        return context.Response;
    }

    protected override string GetErrorMessage(Exception ex) => ex switch
    {
        RequestFailedException rfEx when rfEx.Status == 404 =>
            "Session host or hostpool not found. Verify the names and that you have access to them.",
        RequestFailedException rfEx when rfEx.Status == 403 =>
            "Access denied. Verify you have the necessary permissions to access the session host and hostpool.",
        RequestFailedException rfEx => rfEx.Message,
        _ => base.GetErrorMessage(ex)
    };

    internal record SessionHostUserSessionListCommandResult(List<UserSession> UserSessions);
}
