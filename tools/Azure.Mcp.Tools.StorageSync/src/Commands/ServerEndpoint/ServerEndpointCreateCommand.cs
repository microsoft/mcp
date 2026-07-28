// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Core.Commands.Subscription;
using Azure.Mcp.Core.Services.Azure.Subscription;
using Azure.Mcp.Tools.StorageSync.Models;
using Azure.Mcp.Tools.StorageSync.Options.ServerEndpoint;
using Azure.Mcp.Tools.StorageSync.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Mcp.Core.Commands;
using Microsoft.Mcp.Core.Models.Command;

namespace Azure.Mcp.Tools.StorageSync.Commands.ServerEndpoint;

[CommandMetadata(
    Id = "fcbdf461-6fde-4cfb-a944-4a56a2be90e4",
    Name = "create",
    Title = "Create Server Endpoint",
    Description = "Add a server endpoint to a sync group by specifying a local server path to sync. Server endpoints represent the on-premises side of the sync relationship and include cloud tiering configuration.",
    Destructive = true,
    Idempotent = false,
    OpenWorld = false,
    ReadOnly = false,
    Secret = false,
    LocalRequired = false)]
public sealed class ServerEndpointCreateCommand(ILogger<ServerEndpointCreateCommand> logger, IStorageSyncService service, ISubscriptionResolver subscriptionResolver)
    : SubscriptionCommand<ServerEndpointCreateOptions, ServerEndpointCreateCommand.ServerEndpointCreateCommandResult>(subscriptionResolver)
{
    private readonly IStorageSyncService _service = service;
    private readonly ILogger<ServerEndpointCreateCommand> _logger = logger;

    public override async Task<CommandResponse> ExecuteAsync(CommandContext context, ServerEndpointCreateOptions options, CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("Creating server endpoint. Subscription: {Subscription}, ResourceGroup: {ResourceGroup}, ServiceName: {ServiceName}, GroupName: {GroupName}, EndpointName: {EndpointName}",
                options.Subscription, options.ResourceGroup, options.Name, options.SyncGroupName, options.ServerEndpointName);

            var endpoint = await _service.CreateServerEndpointAsync(
                options.Subscription!,
                options.ResourceGroup,
                options.Name,
                options.SyncGroupName,
                options.ServerEndpointName,
                options.ServerResourceId,
                options.ServerLocalPath,
                options.CloudTiering,
                options.VolumeFreeSpacePercent,
                options.TierFilesOlderThanDays,
                options.LocalCacheMode,
                options.Tenant,
                options.RetryPolicy,
                cancellationToken);

            context.Response.Results = ResponseResult.Create(new(endpoint), StorageSyncJsonContext.Default.ServerEndpointCreateCommandResult);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating server endpoint");
            HandleException(context, ex);
        }

        return context.Response;
    }

    public sealed record ServerEndpointCreateCommandResult(ServerEndpointDataSchema Result);
}
