// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Core.Commands.Subscription;
using Azure.Mcp.Core.Services.Azure.Subscription;
using Azure.Mcp.Tools.StorageSync.Models;
using Azure.Mcp.Tools.StorageSync.Options.SyncGroup;
using Azure.Mcp.Tools.StorageSync.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Mcp.Core.Commands;
using Microsoft.Mcp.Core.Models.Command;

namespace Azure.Mcp.Tools.StorageSync.Commands.SyncGroup;

[CommandMetadata(
    Id = "3572833c-4fc2-4bb9-9eed-52ae8b8899b8",
    Name = "create",
    Title = "Create Sync Group",
    Description = "Create a sync group within an existing Storage Sync service. Sync groups define a sync topology and contain cloud endpoints (Azure File Shares) and server endpoints (local server paths) that sync together.",
    Destructive = true,
    Idempotent = false,
    OpenWorld = false,
    ReadOnly = false,
    Secret = false,
    LocalRequired = false)]
public sealed class SyncGroupCreateCommand(ILogger<SyncGroupCreateCommand> logger, IStorageSyncService service, ISubscriptionResolver subscriptionResolver)
    : SubscriptionCommand<SyncGroupCreateOptions, SyncGroupCreateCommand.SyncGroupCreateCommandResult>(subscriptionResolver)
{
    private readonly IStorageSyncService _service = service;
    private readonly ILogger<SyncGroupCreateCommand> _logger = logger;

    public override async Task<CommandResponse> ExecuteAsync(CommandContext context, SyncGroupCreateOptions options, CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("Creating sync group. Subscription: {Subscription}, ResourceGroup: {ResourceGroup}, ServiceName: {ServiceName}, GroupName: {GroupName}",
                options.Subscription, options.ResourceGroup, options.Name, options.SyncGroupName);

            var syncGroup = await _service.CreateSyncGroupAsync(
                options.Subscription!,
                options.ResourceGroup,
                options.Name,
                options.SyncGroupName,
                options.Tenant,
                options.RetryPolicy,
                cancellationToken);

            context.Response.Results = ResponseResult.Create(new(syncGroup), StorageSyncJsonContext.Default.SyncGroupCreateCommandResult);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating sync group");
            HandleException(context, ex);
        }

        return context.Response;
    }

    public sealed record SyncGroupCreateCommandResult(SyncGroupDataSchema Result);
}
