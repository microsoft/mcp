// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

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
    Id = "df0d4ae3-519a-44f1-ad30-d25a0985e9c2",
    Name = "create",
    Title = "Create Cloud Endpoint",
    Description = "Add a cloud endpoint to a sync group by connecting an Azure File Share. Cloud endpoints represent the Azure storage side of the sync relationship.",
    Destructive = true,
    Idempotent = false,
    OpenWorld = false,
    ReadOnly = false,
    Secret = false,
    LocalRequired = false)]
public sealed class CloudEndpointCreateCommand(ILogger<CloudEndpointCreateCommand> logger, IStorageSyncService service, ISubscriptionResolver subscriptionResolver)
    : SubscriptionCommand<CloudEndpointCreateOptions, CloudEndpointCreateCommand.CloudEndpointCreateCommandResult>(subscriptionResolver)
{
    private readonly IStorageSyncService _service = service;
    private readonly ILogger<CloudEndpointCreateCommand> _logger = logger;

    public override async Task<CommandResponse> ExecuteAsync(CommandContext context, CloudEndpointCreateOptions options, CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("Creating cloud endpoint. Subscription: {Subscription}, ResourceGroup: {ResourceGroup}, ServiceName: {ServiceName}, GroupName: {GroupName}, EndpointName: {EndpointName}",
                options.Subscription, options.ResourceGroup, options.Name, options.SyncGroupName, options.CloudEndpointName);

            var endpoint = await _service.CreateCloudEndpointAsync(
                options.Subscription!,
                options.ResourceGroup,
                options.Name,
                options.SyncGroupName,
                options.CloudEndpointName,
                options.StorageAccountResourceId,
                options.AzureFileShareName,
                options.Tenant,
                options.RetryPolicy,
                cancellationToken);

            context.Response.Results = ResponseResult.Create(new(endpoint), StorageSyncJsonContext.Default.CloudEndpointCreateCommandResult);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating cloud endpoint");
            HandleException(context, ex);
        }

        return context.Response;
    }

    public sealed record CloudEndpointCreateCommandResult(CloudEndpointDataSchema Result);
}
