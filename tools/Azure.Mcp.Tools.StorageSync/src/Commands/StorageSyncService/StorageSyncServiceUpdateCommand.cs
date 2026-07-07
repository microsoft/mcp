// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Core.Commands.Subscription;
using Azure.Mcp.Core.Services.Azure.Subscription;
using Azure.Mcp.Tools.StorageSync.Models;
using Azure.Mcp.Tools.StorageSync.Options.StorageSyncService;
using Azure.Mcp.Tools.StorageSync.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Mcp.Core.Commands;
using Microsoft.Mcp.Core.Models.Command;

namespace Azure.Mcp.Tools.StorageSync.Commands.StorageSyncService;

[CommandMetadata(
    Id = "15db4769-1941-4b1e-9514-867b0f68eb2c",
    Name = "update",
    Title = "Update Storage Sync Service",
    Description = "Update properties of an existing Azure Storage Sync service.",
    Destructive = false,
    Idempotent = false,
    OpenWorld = false,
    ReadOnly = false,
    Secret = false,
    LocalRequired = false)]
public sealed class StorageSyncServiceUpdateCommand(ILogger<StorageSyncServiceUpdateCommand> logger, IStorageSyncService service, ISubscriptionResolver subscriptionResolver)
    : SubscriptionCommand<StorageSyncServiceUpdateOptions, StorageSyncServiceUpdateCommand.StorageSyncServiceUpdateCommandResult>(subscriptionResolver)
{
    private readonly IStorageSyncService _service = service;
    private readonly ILogger<StorageSyncServiceUpdateCommand> _logger = logger;

    public override async Task<CommandResponse> ExecuteAsync(CommandContext context, StorageSyncServiceUpdateOptions options, CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("Updating storage sync service. Subscription: {Subscription}, ResourceGroup: {ResourceGroup}, ServiceName: {ServiceName}",
                options.Subscription, options.ResourceGroup, options.Name);

            var service = await _service.UpdateStorageSyncServiceAsync(
                options.Subscription!,
                options.ResourceGroup,
                options.Name,
                options.IncomingTrafficPolicy,
                options.Tags?.Split(' ', StringSplitOptions.RemoveEmptyEntries).Select(tag => tag.Split('=', 2)).ToDictionary(kv => kv[0], kv => kv[1]),
                options.IdentityType,
                options.Tenant,
                options.RetryPolicy,
                cancellationToken);

            context.Response.Results = ResponseResult.Create(new(service), StorageSyncJsonContext.Default.StorageSyncServiceUpdateCommandResult);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating storage sync service");
            HandleException(context, ex);
        }

        return context.Response;
    }

    public sealed record StorageSyncServiceUpdateCommandResult(StorageSyncServiceDataSchema Result);
}
