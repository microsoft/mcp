// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Core.Commands.Subscription;
using Azure.Mcp.Core.Services.Azure.Subscription;
using Azure.Mcp.Tools.SignalR.Options.Runtime;
using Azure.Mcp.Tools.SignalR.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Mcp.Core.Commands;
using Microsoft.Mcp.Core.Models.Command;

namespace Azure.Mcp.Tools.SignalR.Commands.Runtime;

/// <summary>
/// Shows details of an Azure SignalR Service.
/// </summary>
[CommandMetadata(
    Id = "bb9035f6-f642-4ee0-83c8-87d6da8266b1",
    Name = "get",
    Title = "Show Service Details",
    Description = """
        Gets or lists details of an Azure SignalR Runtimes. If a specific SignalR name is used, the details of that
        SignalR runtime will be retrieved. Otherwise, all SignalR Runtimes in the specified subscription or resource
        group will be retrieved. Returns runtime information including identity, network ACLs, upstream templates.
        """,
    Destructive = false,
    Idempotent = true,
    OpenWorld = false,
    ReadOnly = true,
    Secret = false,
    LocalRequired = false)]
public sealed class RuntimeGetCommand(ILogger<RuntimeGetCommand> logger, ISignalRService signalRService, ISubscriptionResolver subscriptionResolver)
    : SubscriptionCommand<RuntimeGetOptions, RuntimeGetCommand.RuntimeGetCommandResult>(subscriptionResolver)
{
    private readonly ILogger<RuntimeGetCommand> _logger = logger;
    private readonly ISignalRService _signalRService = signalRService;

    public override async Task<CommandResponse> ExecuteAsync(CommandContext context, RuntimeGetOptions options, CancellationToken cancellationToken)
    {
        try
        {
            var runtimes = await _signalRService.GetRuntimeAsync(
                options.Subscription!,
                options.ResourceGroup,
                options.SignalR,
                options.Tenant,
                options.RetryPolicy,
                cancellationToken);

            _logger.LogInformation("Found {Count} SignalR service(s) in subscription {SubscriptionId}",
                runtimes.Count(), options.Subscription);

            context.Response.Results = ResponseResult.Create(new(runtimes ?? []), SignalRJsonContext.Default.RuntimeGetCommandResult);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An exception occurred showing SignalR service");
            HandleException(context, ex);
        }

        return context.Response;
    }

    public sealed record RuntimeGetCommandResult(IEnumerable<Models.Runtime> Runtimes);
}
