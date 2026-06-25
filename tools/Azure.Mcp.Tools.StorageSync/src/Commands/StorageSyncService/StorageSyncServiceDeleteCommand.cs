// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Core.Commands.Subscription;
using Azure.Mcp.Core.Services.Azure.Subscription;
using Azure.Mcp.Tools.StorageSync.Options.StorageSyncService;
using Azure.Mcp.Tools.StorageSync.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Mcp.Core.Commands;
using Microsoft.Mcp.Core.Models.Command;

namespace Azure.Mcp.Tools.StorageSync.Commands.StorageSyncService;

[CommandMetadata(
    Id = "a7dcf4e2-fd1d-4d0a-acd3-f56ea5eceef6",
    Name = "delete",
    Title = "Delete Storage Sync Service",
    Description = "Delete an Azure Storage Sync service and all its associated resources.",
    Destructive = true,
    Idempotent = false,
    OpenWorld = false,
    ReadOnly = false,
    Secret = false,
    LocalRequired = false)]
public sealed class StorageSyncServiceDeleteCommand(ILogger<StorageSyncServiceDeleteCommand> logger, IStorageSyncService service, ISubscriptionResolver subscriptionResolver)
    : SubscriptionCommand<StorageSyncServiceDeleteOptions, StorageSyncServiceDeleteCommand.StorageSyncServiceDeleteCommandResult>(subscriptionResolver)
{
    private readonly IStorageSyncService _service = service;
    private readonly ILogger<StorageSyncServiceDeleteCommand> _logger = logger;

    public override async Task<CommandResponse> ExecuteAsync(CommandContext context, StorageSyncServiceDeleteOptions options, CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("Deleting storage sync service. Subscription: {Subscription}, ResourceGroup: {ResourceGroup}, ServiceName: {ServiceName}",
                options.Subscription, options.ResourceGroup, options.Name);

            await _service.DeleteStorageSyncServiceAsync(
                options.Subscription!,
                options.ResourceGroup,
                options.Name,
                options.Tenant,
                options.RetryPolicy,
                cancellationToken);

            context.Response.Message = "Storage sync service deleted successfully";
            context.Response.Results = ResponseResult.Create(new("Storage sync service deleted successfully"), StorageSyncJsonContext.Default.StorageSyncServiceDeleteCommandResult);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting storage sync service");
            HandleException(context, ex);
        }

        return context.Response;
    }

    public sealed record StorageSyncServiceDeleteCommandResult(string Message);
}
