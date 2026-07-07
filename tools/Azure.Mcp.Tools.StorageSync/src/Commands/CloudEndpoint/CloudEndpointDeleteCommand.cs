// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Core.Commands.Subscription;
using Azure.Mcp.Core.Services.Azure.Subscription;
using Azure.Mcp.Tools.StorageSync.Options.CloudEndpoint;
using Azure.Mcp.Tools.StorageSync.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Mcp.Core.Commands;
using Microsoft.Mcp.Core.Models.Command;

namespace Azure.Mcp.Tools.StorageSync.Commands.CloudEndpoint;

[CommandMetadata(
    Id = "f5e76906-cc2a-41a4-b4f9-498221aaaf2e",
    Name = "delete",
    Title = "Delete Cloud Endpoint",
    Description = "Delete a cloud endpoint from a sync group.",
    Destructive = true,
    Idempotent = false,
    OpenWorld = false,
    ReadOnly = false,
    Secret = false,
    LocalRequired = false)]
public sealed class CloudEndpointDeleteCommand(ILogger<CloudEndpointDeleteCommand> logger, IStorageSyncService service, ISubscriptionResolver subscriptionResolver)
    : SubscriptionCommand<CloudEndpointDeleteOptions, CloudEndpointDeleteCommand.CloudEndpointDeleteCommandResult>(subscriptionResolver)
{
    private readonly IStorageSyncService _service = service;
    private readonly ILogger<CloudEndpointDeleteCommand> _logger = logger;

    public override async Task<CommandResponse> ExecuteAsync(CommandContext context, CloudEndpointDeleteOptions options, CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("Deleting cloud endpoint. Subscription: {Subscription}, ResourceGroup: {ResourceGroup}, ServiceName: {ServiceName}, GroupName: {GroupName}, EndpointName: {EndpointName}",
                options.Subscription, options.ResourceGroup, options.Name, options.SyncGroupName, options.CloudEndpointName);

            await _service.DeleteCloudEndpointAsync(
                options.Subscription!,
                options.ResourceGroup,
                options.Name,
                options.SyncGroupName,
                options.CloudEndpointName,
                options.Tenant,
                options.RetryPolicy,
                cancellationToken);

            context.Response.Message = "Cloud endpoint deleted successfully";
            context.Response.Results = ResponseResult.Create(new("Cloud endpoint deleted successfully"), StorageSyncJsonContext.Default.CloudEndpointDeleteCommandResult);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting cloud endpoint");
            HandleException(context, ex);
        }

        return context.Response;
    }

    public sealed record CloudEndpointDeleteCommandResult(string Message);
}
