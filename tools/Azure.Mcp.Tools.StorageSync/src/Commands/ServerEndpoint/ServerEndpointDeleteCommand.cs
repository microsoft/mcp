// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Core.Commands.Subscription;
using Azure.Mcp.Core.Services.Azure.Subscription;
using Azure.Mcp.Tools.StorageSync.Options.ServerEndpoint;
using Azure.Mcp.Tools.StorageSync.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Mcp.Core.Commands;
using Microsoft.Mcp.Core.Models.Command;

namespace Azure.Mcp.Tools.StorageSync.Commands.ServerEndpoint;

[CommandMetadata(
    Id = "ef6c2aa9-bb64-4f94-b18b-018e04b504c9",
    Name = "delete",
    Title = "Delete Server Endpoint",
    Description = "Delete a server endpoint from a sync group.",
    Destructive = true,
    Idempotent = false,
    OpenWorld = false,
    ReadOnly = false,
    Secret = false,
    LocalRequired = false)]
public sealed class ServerEndpointDeleteCommand(ILogger<ServerEndpointDeleteCommand> logger, IStorageSyncService service, ISubscriptionResolver subscriptionResolver)
    : SubscriptionCommand<ServerEndpointDeleteOptions, ServerEndpointDeleteCommand.ServerEndpointDeleteCommandResult>(subscriptionResolver)
{
    private readonly IStorageSyncService _service = service;
    private readonly ILogger<ServerEndpointDeleteCommand> _logger = logger;

    public override async Task<CommandResponse> ExecuteAsync(CommandContext context, ServerEndpointDeleteOptions options, CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("Deleting server endpoint. Subscription: {Subscription}, ResourceGroup: {ResourceGroup}, ServiceName: {ServiceName}, GroupName: {GroupName}, EndpointName: {EndpointName}",
                options.Subscription, options.ResourceGroup, options.Name, options.SyncGroupName, options.ServerEndpointName);

            await _service.DeleteServerEndpointAsync(
                options.Subscription!,
                options.ResourceGroup,
                options.Name,
                options.SyncGroupName,
                options.ServerEndpointName,
                options.Tenant,
                options.RetryPolicy,
                cancellationToken);

            context.Response.Message = "Server endpoint deleted successfully";
            context.Response.Results = ResponseResult.Create(new("Server endpoint deleted successfully"), StorageSyncJsonContext.Default.ServerEndpointDeleteCommandResult);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting server endpoint");
            HandleException(context, ex);
        }

        return context.Response;
    }

    public sealed record ServerEndpointDeleteCommandResult(string Message);
}
