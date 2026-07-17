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
    Id = "7c76387f-c62e-48d1-af3b-d444d6b3b79c",
    Name = "create",
    Title = "Create Storage Sync Service",
    Description = "Create a new Azure Storage Sync service resource in a resource group. This is the top-level service container that manages sync groups, registered servers, and synchronization workflows.",
    Destructive = true,
    Idempotent = false,
    OpenWorld = false,
    ReadOnly = false,
    Secret = false,
    LocalRequired = false)]
public sealed class StorageSyncServiceCreateCommand(ILogger<StorageSyncServiceCreateCommand> logger, IStorageSyncService service, ISubscriptionResolver subscriptionResolver)
    : SubscriptionCommand<StorageSyncServiceCreateOptions, StorageSyncServiceCreateCommand.StorageSyncServiceCreateCommandResult>(subscriptionResolver)
{
    private readonly IStorageSyncService _service = service;
    private readonly ILogger<StorageSyncServiceCreateCommand> _logger = logger;

    public override async Task<CommandResponse> ExecuteAsync(CommandContext context, StorageSyncServiceCreateOptions options, CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("Creating storage sync service. Subscription: {Subscription}, ResourceGroup: {ResourceGroup}, ServiceName: {ServiceName}",
                options.Subscription, options.ResourceGroup, options.Name);

            var service = await _service.CreateStorageSyncServiceAsync(
                options.Subscription!,
                options.ResourceGroup,
                options.Name,
                options.Location,
                options.Tags?.Split(' ', StringSplitOptions.RemoveEmptyEntries).Select(tag => tag.Split('=', 2)).ToDictionary(kv => kv[0], kv => kv[1]),
                options.Tenant,
                options.RetryPolicy,
                cancellationToken);

            context.Response.Results = ResponseResult.Create(new(service), StorageSyncJsonContext.Default.StorageSyncServiceCreateCommandResult);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating storage sync service");
            HandleException(context, ex);
        }

        return context.Response;
    }

    public sealed record StorageSyncServiceCreateCommandResult(StorageSyncServiceDataSchema Result);
}
