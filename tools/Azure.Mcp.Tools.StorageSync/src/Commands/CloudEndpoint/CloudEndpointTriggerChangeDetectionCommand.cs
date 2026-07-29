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
    Id = "96f096a2-d36f-4361-aa74-4e393e7f48a5",
    Name = "changedetection",
    Title = "Trigger Change Detection",
    Description = "Trigger change detection on a cloud endpoint to sync file changes.",
    Destructive = false,
    Idempotent = false,
    OpenWorld = false,
    ReadOnly = false,
    Secret = false,
    LocalRequired = false)]
public sealed class CloudEndpointTriggerChangeDetectionCommand(ILogger<CloudEndpointTriggerChangeDetectionCommand> logger, IStorageSyncService service, ISubscriptionResolver subscriptionResolver)
    : SubscriptionCommand<CloudEndpointTriggerChangeDetectionOptions, CloudEndpointTriggerChangeDetectionCommand.CloudEndpointTriggerChangeDetectionCommandResult>(subscriptionResolver)
{
    private readonly IStorageSyncService _service = service;
    private readonly ILogger<CloudEndpointTriggerChangeDetectionCommand> _logger = logger;

    public override async Task<CommandResponse> ExecuteAsync(CommandContext context, CloudEndpointTriggerChangeDetectionOptions options, CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("Triggering change detection. Subscription: {Subscription}, ResourceGroup: {ResourceGroup}, ServiceName: {ServiceName}, GroupName: {GroupName}, EndpointName: {EndpointName}, DirectoryPath: {DirectoryPath}, ChangeDetectionMode: {ChangeDetectionMode}",
                options.Subscription, options.ResourceGroup, options.Name, options.SyncGroupName, options.CloudEndpointName, options.DirectoryPath, options.ChangeDetectionMode);

            await _service.TriggerChangeDetectionAsync(
                options.Subscription!,
                options.ResourceGroup,
                options.Name,
                options.SyncGroupName,
                options.CloudEndpointName,
                options.DirectoryPath,
                options.ChangeDetectionMode,
                options.Paths,
                options.Tenant,
                options.RetryPolicy,
                cancellationToken);

            context.Response.Message = "Change detection triggered successfully";
            context.Response.Results = ResponseResult.Create(new("Change detection triggered successfully"), StorageSyncJsonContext.Default.CloudEndpointTriggerChangeDetectionCommandResult);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error triggering change detection");
            HandleException(context, ex);
        }

        return context.Response;
    }

    public sealed record CloudEndpointTriggerChangeDetectionCommandResult(string Message);
}
