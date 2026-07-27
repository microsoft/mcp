// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Core.Commands.Subscription;
using Azure.Mcp.Core.Services.Azure.Subscription;
using Azure.Mcp.Tools.FileShares.Models;
using Azure.Mcp.Tools.FileShares.Options.Snapshot;
using Azure.Mcp.Tools.FileShares.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Mcp.Core.Commands;
using Microsoft.Mcp.Core.Models.Command;

namespace Azure.Mcp.Tools.FileShares.Commands.Snapshot;

[CommandMetadata(
    Id = "a3b4c5d6-e7f8-4a9b-0c1d-2e3f4a5b6c7d",
    Name = "get",
    Title = "Get File Share Snapshot",
    Description = "Get details of a specific file share snapshot or list all snapshots. If --snapshot-name is provided, returns a specific snapshot; otherwise, lists all snapshots for the file share.",
    Destructive = false,
    Idempotent = true,
    OpenWorld = false,
    ReadOnly = true,
    Secret = false,
    LocalRequired = false)]
public sealed class SnapshotGetCommand(ILogger<SnapshotGetCommand> logger, IFileSharesService fileSharesService, ISubscriptionResolver subscriptionResolver)
    : SubscriptionCommand<SnapshotGetOptions, SnapshotGetCommand.SnapshotGetCommandResult>(subscriptionResolver)
{
    public override async Task<CommandResponse> ExecuteAsync(CommandContext context, SnapshotGetOptions options, CancellationToken cancellationToken)
    {
        try
        {
            // If snapshot name is provided, get specific snapshot
            if (!string.IsNullOrEmpty(options.SnapshotName))
            {
                logger.LogInformation("Getting snapshot. Subscription: {Subscription}, ResourceGroup: {ResourceGroup}, FileShareName: {FileShareName}, SnapshotName: {SnapshotName}",
                    options.Subscription, options.ResourceGroup, options.FileShareName, options.SnapshotName);

                var snapshot = await fileSharesService.GetSnapshotAsync(
                    options.Subscription!,
                    options.ResourceGroup,
                    options.FileShareName,
                    options.SnapshotName,
                    options.Tenant,
                    options.RetryPolicy,
                    cancellationToken);

                context.Response.Results = ResponseResult.Create(new([snapshot]), FileSharesJsonContext.Default.SnapshotGetCommandResult);

                logger.LogInformation("Successfully retrieved snapshot. SnapshotName: {SnapshotName}", options.SnapshotName);
            }
            else
            {
                // List all snapshots
                logger.LogInformation("Listing snapshots. Subscription: {Subscription}, ResourceGroup: {ResourceGroup}, FileShareName: {FileShareName}",
                    options.Subscription, options.ResourceGroup, options.FileShareName);

                var snapshots = await fileSharesService.ListSnapshotsAsync(
                    options.Subscription!,
                    options.ResourceGroup,
                    options.FileShareName,
                    options.Tenant,
                    options.RetryPolicy,
                    cancellationToken);

                context.Response.Results = ResponseResult.Create(new(snapshots ?? []), FileSharesJsonContext.Default.SnapshotGetCommandResult);

                logger.LogInformation("Successfully listed {Count} snapshots for file share {FileShareName}", snapshots?.Count ?? 0, options.FileShareName);
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to get snapshot(s)");
            HandleException(context, ex);
        }

        return context.Response;
    }

    public sealed record SnapshotGetCommandResult(List<FileShareSnapshotInfo> Snapshots);
}
