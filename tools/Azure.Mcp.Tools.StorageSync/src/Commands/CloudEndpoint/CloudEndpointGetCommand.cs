// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net;
using Azure.Mcp.Core.Commands.Subscription;
using Azure.Mcp.Core.Services.Azure.Subscription;
using Azure.Mcp.Tools.StorageSync.Models;
using Azure.Mcp.Tools.StorageSync.Options.CloudEndpoint;
using Azure.Mcp.Tools.StorageSync.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Mcp.Core.Commands;
using Microsoft.Mcp.Core.Models.Command;

namespace Azure.Mcp.Tools.StorageSync.Commands.CloudEndpoint;

[CommandMetadata(
    Id = "25dd8bb3-5ba3-4c0d-993d-54917f63d52e",
    Name = "get",
    Title = "Get Cloud Endpoint",
    Description = "List all cloud endpoints in a sync group or retrieve details about a specific cloud endpoint. Returns cloud endpoint properties including Azure File Share configuration, storage account details, and provisioning state. Use --cloud-endpoint-name for a specific endpoint.",
    Destructive = false,
    Idempotent = true,
    OpenWorld = false,
    ReadOnly = true,
    Secret = false,
    LocalRequired = false)]
public sealed class CloudEndpointGetCommand(ILogger<CloudEndpointGetCommand> logger, IStorageSyncService service, ISubscriptionResolver subscriptionResolver)
    : SubscriptionCommand<CloudEndpointGetOptions, CloudEndpointGetCommand.CloudEndpointGetCommandResult>(subscriptionResolver)
{
    private readonly IStorageSyncService _service = service;
    private readonly ILogger<CloudEndpointGetCommand> _logger = logger;

    public override async Task<CommandResponse> ExecuteAsync(CommandContext context, CloudEndpointGetOptions options, CancellationToken cancellationToken)
    {
        try
        {
            // If cloud endpoint name is provided, get specific endpoint
            if (!string.IsNullOrEmpty(options.CloudEndpointName))
            {
                _logger.LogInformation("Getting cloud endpoint. Subscription: {Subscription}, ResourceGroup: {ResourceGroup}, ServiceName: {ServiceName}, GroupName: {GroupName}, EndpointName: {EndpointName}",
                    options.Subscription, options.ResourceGroup, options.Name, options.SyncGroupName, options.CloudEndpointName);

                var endpoint = await _service.GetCloudEndpointAsync(
                    options.Subscription!,
                    options.ResourceGroup,
                    options.Name,
                    options.SyncGroupName,
                    options.CloudEndpointName,
                    options.Tenant,
                    options.RetryPolicy,
                    cancellationToken);

                if (endpoint == null)
                {
                    context.Response.Status = HttpStatusCode.NotFound;
                    context.Response.Message = "Cloud endpoint not found";
                    return context.Response;
                }

                context.Response.Results = ResponseResult.Create(new([endpoint]), StorageSyncJsonContext.Default.CloudEndpointGetCommandResult);
            }
            else
            {
                // List all cloud endpoints
                _logger.LogInformation("Listing cloud endpoints. Subscription: {Subscription}, ResourceGroup: {ResourceGroup}, ServiceName: {ServiceName}, GroupName: {GroupName}",
                    options.Subscription, options.ResourceGroup, options.Name, options.SyncGroupName);

                var endpoints = await _service.ListCloudEndpointsAsync(
                    options.Subscription!,
                    options.ResourceGroup,
                    options.Name,
                    options.SyncGroupName,
                    options.Tenant,
                    options.RetryPolicy,
                    cancellationToken);

                context.Response.Results = ResponseResult.Create(new(endpoints ?? []), StorageSyncJsonContext.Default.CloudEndpointGetCommandResult);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting cloud endpoint(s)");
            HandleException(context, ex);
        }

        return context.Response;
    }

    public sealed record CloudEndpointGetCommandResult(List<CloudEndpointDataSchema> Results);
}
