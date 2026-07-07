// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net;
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
    Id = "cf197b94-6aa6-403b-8679-3a1ce5440ca3",
    Name = "get",
    Title = "Get Server Endpoint",
    Description = "List all server endpoints in a sync group or retrieve details about a specific server endpoint. Returns server endpoint properties including local path, cloud tiering status, sync health, and provisioning state. Use --server-endpoint-name for a specific endpoint.",
    Destructive = false,
    Idempotent = true,
    OpenWorld = false,
    ReadOnly = true,
    Secret = false,
    LocalRequired = false)]
public sealed class ServerEndpointGetCommand(ILogger<ServerEndpointGetCommand> logger, IStorageSyncService service, ISubscriptionResolver subscriptionResolver)
    : SubscriptionCommand<ServerEndpointGetOptions, ServerEndpointGetCommand.ServerEndpointGetCommandResult>(subscriptionResolver)
{
    private readonly IStorageSyncService _service = service;
    private readonly ILogger<ServerEndpointGetCommand> _logger = logger;

    public override async Task<CommandResponse> ExecuteAsync(CommandContext context, ServerEndpointGetOptions options, CancellationToken cancellationToken)
    {
        try
        {
            // If server endpoint name is provided, get specific endpoint
            if (!string.IsNullOrEmpty(options.ServerEndpointName))
            {
                _logger.LogInformation("Getting server endpoint. Subscription: {Subscription}, ResourceGroup: {ResourceGroup}, ServiceName: {ServiceName}, GroupName: {GroupName}, EndpointName: {EndpointName}",
                    options.Subscription, options.ResourceGroup, options.Name, options.SyncGroupName, options.ServerEndpointName);

                var endpoint = await _service.GetServerEndpointAsync(
                    options.Subscription!,
                    options.ResourceGroup,
                    options.Name,
                    options.SyncGroupName,
                    options.ServerEndpointName,
                    options.Tenant,
                    options.RetryPolicy,
                    cancellationToken);

                if (endpoint == null)
                {
                    context.Response.Status = HttpStatusCode.NotFound;
                    context.Response.Message = "Server endpoint not found";
                    return context.Response;
                }

                context.Response.Results = ResponseResult.Create(new([endpoint]), StorageSyncJsonContext.Default.ServerEndpointGetCommandResult);
            }
            else
            {
                // List all server endpoints
                _logger.LogInformation("Listing server endpoints. Subscription: {Subscription}, ResourceGroup: {ResourceGroup}, ServiceName: {ServiceName}, GroupName: {GroupName}",
                    options.Subscription, options.ResourceGroup, options.Name, options.SyncGroupName);

                var endpoints = await _service.ListServerEndpointsAsync(
                    options.Subscription!,
                    options.ResourceGroup,
                    options.Name,
                    options.SyncGroupName,
                    options.Tenant,
                    options.RetryPolicy,
                    cancellationToken);

                context.Response.Results = ResponseResult.Create(new(endpoints ?? []), StorageSyncJsonContext.Default.ServerEndpointGetCommandResult);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting server endpoint(s)");
            HandleException(context, ex);
        }

        return context.Response;
    }

    public sealed record ServerEndpointGetCommandResult(List<ServerEndpointDataSchema> Results);
}
