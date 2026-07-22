// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net;
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
    Id = "95ce2336-19e6-40fb-a3ea-e2a76772036b",
    Name = "get",
    Title = "Get Sync Group",
    Description = "Get details about a specific sync group or list all sync groups. If --sync-group-name is provided, returns a specific sync group; otherwise, lists all sync groups in the Storage Sync service.",
    Destructive = false,
    Idempotent = true,
    OpenWorld = false,
    ReadOnly = true,
    Secret = false,
    LocalRequired = false)]
public sealed class SyncGroupGetCommand(ILogger<SyncGroupGetCommand> logger, IStorageSyncService service, ISubscriptionResolver subscriptionResolver)
    : SubscriptionCommand<SyncGroupGetOptions, SyncGroupGetCommand.SyncGroupGetCommandResult>(subscriptionResolver)
{
    private readonly IStorageSyncService _service = service;
    private readonly ILogger<SyncGroupGetCommand> _logger = logger;

    public override async Task<CommandResponse> ExecuteAsync(CommandContext context, SyncGroupGetOptions options, CancellationToken cancellationToken)
    {
        try
        {
            // If sync group name is provided, get specific sync group
            if (!string.IsNullOrEmpty(options.SyncGroupName))
            {
                _logger.LogInformation("Getting sync group. Subscription: {Subscription}, ResourceGroup: {ResourceGroup}, ServiceName: {ServiceName}, GroupName: {GroupName}",
                    options.Subscription, options.ResourceGroup, options.Name, options.SyncGroupName);

                var syncGroup = await _service.GetSyncGroupAsync(
                    options.Subscription!,
                    options.ResourceGroup,
                    options.Name,
                    options.SyncGroupName,
                    options.Tenant,
                    options.RetryPolicy,
                    cancellationToken);

                if (syncGroup == null)
                {
                    context.Response.Status = HttpStatusCode.NotFound;
                    context.Response.Message = "Sync group not found";
                    return context.Response;
                }

                context.Response.Results = ResponseResult.Create(new([syncGroup]), StorageSyncJsonContext.Default.SyncGroupGetCommandResult);
            }
            else
            {
                // List all sync groups
                _logger.LogInformation("Listing sync groups. Subscription: {Subscription}, ResourceGroup: {ResourceGroup}, ServiceName: {ServiceName}",
                    options.Subscription, options.ResourceGroup, options.Name);

                var syncGroups = await _service.ListSyncGroupsAsync(
                    options.Subscription!,
                    options.ResourceGroup,
                    options.Name,
                    options.Tenant,
                    options.RetryPolicy,
                    cancellationToken);

                context.Response.Results = ResponseResult.Create(new(syncGroups ?? []), StorageSyncJsonContext.Default.SyncGroupGetCommandResult);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting sync group(s)");
            HandleException(context, ex);
        }

        return context.Response;
    }

    public sealed record SyncGroupGetCommandResult(List<SyncGroupDataSchema> Results);
}
