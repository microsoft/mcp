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
    Id = "7b35bb46-0a34-4e44-9d7c-148e9992b445",
    Name = "update",
    Title = "Update Server Endpoint",
    Description = "Update properties of a server endpoint.",
    Destructive = false,
    Idempotent = false,
    OpenWorld = false,
    ReadOnly = false,
    Secret = false,
    LocalRequired = false)]
public sealed class ServerEndpointUpdateCommand(ILogger<ServerEndpointUpdateCommand> logger, IStorageSyncService service, ISubscriptionResolver subscriptionResolver)
    : SubscriptionCommand<ServerEndpointUpdateOptions, ServerEndpointUpdateCommand.ServerEndpointUpdateCommandResult>(subscriptionResolver)
{
    private readonly IStorageSyncService _service = service;
    private readonly ILogger<ServerEndpointUpdateCommand> _logger = logger;

    public override async Task<CommandResponse> ExecuteAsync(CommandContext context, ServerEndpointUpdateOptions options, CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("Updating server endpoint. Subscription: {Subscription}, ResourceGroup: {ResourceGroup}, ServiceName: {ServiceName}, GroupName: {GroupName}, EndpointName: {EndpointName}, CloudTiering: {CloudTiering}",
                options.Subscription, options.ResourceGroup, options.Name, options.SyncGroupName, options.ServerEndpointName, options.CloudTiering);

            var endpoint = await _service.UpdateServerEndpointAsync(
                options.Subscription!,
                options.ResourceGroup,
                options.Name,
                options.SyncGroupName,
                options.ServerEndpointName,
                options.CloudTiering,
                options.VolumeFreeSpacePercent,
                options.TierFilesOlderThanDays,
                options.LocalCacheMode,
                options.Tenant,
                options.RetryPolicy,
                cancellationToken);

            context.Response.Results = ResponseResult.Create(new(endpoint), StorageSyncJsonContext.Default.ServerEndpointUpdateCommandResult);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating server endpoint");
            HandleException(context, ex);
        }

        return context.Response;
    }

    public sealed record ServerEndpointUpdateCommandResult(ServerEndpointDataSchema Result);
}
