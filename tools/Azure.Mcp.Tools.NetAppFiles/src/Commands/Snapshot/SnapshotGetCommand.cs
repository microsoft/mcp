// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Core.Commands.Subscription;
using Azure.Mcp.Core.Services.Azure.Subscription;
using Microsoft.Mcp.Core.Extensions;
using Azure.Mcp.Tools.NetAppFiles.Models;
using Azure.Mcp.Tools.NetAppFiles.Options;
using Azure.Mcp.Tools.NetAppFiles.Options.Snapshot;
using Azure.Mcp.Tools.NetAppFiles.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Mcp.Core.Commands;
using Microsoft.Mcp.Core.Models.Command;
using Microsoft.Mcp.Core.Models.Option;

namespace Azure.Mcp.Tools.NetAppFiles.Commands.Snapshot;

[CommandMetadata(
    Id = "a3c7e1d9-5f2b-4a8c-b6d0-e9f4a2c8d1b5",
    Name = "get",
    Description =
        """
        Retrieves detailed information about Azure NetApp Files snapshots, including snapshot name, location, resource group, provisioning state, and creation time. If a specific snapshot name is not provided, the command will return details for all snapshots in a subscription. Optionally filter by account, capacity pool, and volume.
        """,
    Title = "Get NetApp Files Snapshot Details",
    Destructive = false,
    Idempotent = true,
    OpenWorld = false,
    ReadOnly = true,
    LocalRequired = false,
    Secret = false
)]
public sealed class SnapshotGetCommand(ILogger<SnapshotGetCommand> logger, INetAppFilesService netAppFilesService, ISubscriptionResolver subscriptionResolver)
    : SubscriptionCommand<SnapshotGetOptions, SnapshotGetCommand.SnapshotGetCommandResult>(subscriptionResolver)
{
    private readonly ILogger<SnapshotGetCommand> _logger = logger;

    private readonly INetAppFilesService _netAppFilesService = netAppFilesService;

    public override async Task<CommandResponse> ExecuteAsync(CommandContext context, SnapshotGetOptions options, CancellationToken cancellationToken)
    {
        try
        {
            var snapshots = await _netAppFilesService.GetSnapshotDetails(
                options.Account,
                options.Pool,
                options.Volume,
                options.Snapshot,
                options.ResourceGroup,
                options.Ids,
                options.Subscription!,
                options.Tenant,
                options.RetryPolicy,
                cancellationToken);

            context.Response.Results = ResponseResult.Create(
                new(snapshots?.Results ?? [], snapshots?.AreResultsTruncated ?? false),
                NetAppFilesJsonContext.Default.SnapshotGetCommandResult);
        }
        catch (Exception ex)
        {
            if (options.Snapshot is null)
            {
                _logger.LogError(ex, "Error listing NetApp Files snapshot details. Subscription: {Subscription}, Options: {@Options}", options.Subscription, options);
            }
            else
            {
                _logger.LogError(ex, "Error getting NetApp Files snapshot details. Snapshot: {Snapshot}, Subscription: {Subscription}, Options: {@Options}",
                    options.Snapshot, options.Subscription, options);
            }
            HandleException(context, ex);
        }

        return context.Response;
    }

    public record SnapshotGetCommandResult(List<SnapshotInfo> Snapshots, bool AreResultsTruncated);
}
