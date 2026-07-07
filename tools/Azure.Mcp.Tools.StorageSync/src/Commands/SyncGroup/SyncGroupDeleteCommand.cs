// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Core.Commands.Subscription;
using Azure.Mcp.Core.Services.Azure.Subscription;
using Azure.Mcp.Tools.StorageSync.Options.SyncGroup;
using Azure.Mcp.Tools.StorageSync.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Mcp.Core.Commands;
using Microsoft.Mcp.Core.Models.Command;

namespace Azure.Mcp.Tools.StorageSync.Commands.SyncGroup;

[CommandMetadata(
    Id = "c8f91bd7-ea1d-4af4-9703-fe83c43b34b5",
    Name = "delete",
    Title = "Delete Sync Group",
    Description = "Remove a sync group from a Storage Sync service. Deleting a sync group also removes all associated cloud endpoints and server endpoints within that group.",
    Destructive = true,
    Idempotent = false,
    OpenWorld = false,
    ReadOnly = false,
    Secret = false,
    LocalRequired = false)]
public sealed class SyncGroupDeleteCommand(ILogger<SyncGroupDeleteCommand> logger, IStorageSyncService service, ISubscriptionResolver subscriptionResolver)
    : SubscriptionCommand<SyncGroupDeleteOptions, SyncGroupDeleteCommand.SyncGroupDeleteCommandResult>(subscriptionResolver)
{
    private readonly IStorageSyncService _service = service;
    private readonly ILogger<SyncGroupDeleteCommand> _logger = logger;

    public override async Task<CommandResponse> ExecuteAsync(CommandContext context, SyncGroupDeleteOptions options, CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("Deleting sync group. Subscription: {Subscription}, ResourceGroup: {ResourceGroup}, ServiceName: {ServiceName}, GroupName: {GroupName}",
                options.Subscription, options.ResourceGroup, options.Name, options.SyncGroupName);

            await _service.DeleteSyncGroupAsync(
                options.Subscription!,
                options.ResourceGroup,
                options.Name,
                options.SyncGroupName,
                options.Tenant,
                options.RetryPolicy,
                cancellationToken);

            context.Response.Message = "Sync group deleted successfully";
            context.Response.Results = ResponseResult.Create(new("Sync group deleted successfully"), StorageSyncJsonContext.Default.SyncGroupDeleteCommandResult);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting sync group");
            HandleException(context, ex);
        }

        return context.Response;
    }

    public sealed record SyncGroupDeleteCommandResult(string Message);
}
