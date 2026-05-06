// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net;
using System.Text.Json.Serialization.Metadata;
using Azure.Mcp.Tools.VirtualDesktop.Commands.Hostpool;
using Azure.Mcp.Tools.VirtualDesktop.Options.SessionHost;
using Azure.Mcp.Tools.VirtualDesktop.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Mcp.Core.Commands;
using Microsoft.Mcp.Core.Models.Command;
using Microsoft.Mcp.Core.Models.Option;

namespace Azure.Mcp.Tools.VirtualDesktop.Commands.SessionHost;

public sealed class SessionHostListCommand(ILogger<SessionHostListCommand> logger, IVirtualDesktopService virtualDesktopService) : BaseHostPoolCommand<SessionHostListOptions, SessionHostListCommand.SessionHostListCommandResult>
{
    private const string CommandTitle = "List SessionHosts";
    private readonly ILogger<SessionHostListCommand> _logger = logger;
    private readonly IVirtualDesktopService _virtualDesktopService = virtualDesktopService;

    public override string Name => "list";

    public override string Description =>
        $"""
		List all SessionHosts in a hostpool. This command retrieves all Azure Virtual Desktop SessionHost objects available
		in the specified {OptionDefinitions.Common.Subscription.Name} and hostpool. Results include SessionHost details and are
		returned as a JSON array.
		""";

    public override string Id => "6f543101-3c70-41bd-a6ed-5cc4af716081";

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

    protected override JsonTypeInfo<SessionHostListCommandResult> ResultTypeInfo => VirtualDesktopJsonContext.Default.SessionHostListCommandResult;

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

            SetResult(context, new([.. sessionHosts ?? []]));
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

    public record SessionHostListCommandResult(List<Models.SessionHost> SessionHosts);
}
