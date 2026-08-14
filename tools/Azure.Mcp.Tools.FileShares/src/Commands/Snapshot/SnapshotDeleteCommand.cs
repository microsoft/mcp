// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Core.Commands.Subscription;
using Azure.Mcp.Core.Services.Azure.Subscription;
using Azure.Mcp.Tools.FileShares.Options.Snapshot;
using Azure.Mcp.Tools.FileShares.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Mcp.Core.Commands;
using Microsoft.Mcp.Core.Models.Command;

namespace Azure.Mcp.Tools.FileShares.Commands.Snapshot;

/// <summary>
/// Deletes a file share snapshot.
/// </summary>
[CommandMetadata(
    Id = "c7d8e9f0-a1b2-4c3d-4e5f-6a7b8c9d0e1f",
    Name = "delete",
    Title = "Delete File Share Snapshot",
    Description = "Delete a file share snapshot permanently. This operation cannot be undone.",
    Destructive = true,
    Idempotent = false,
    OpenWorld = false,
    ReadOnly = false,
    Secret = false,
    LocalRequired = false)]
public sealed class SnapshotDeleteCommand(ILogger<SnapshotDeleteCommand> logger, IFileSharesService fileSharesService, ISubscriptionResolver subscriptionResolver)
    : SubscriptionCommand<SnapshotDeleteOptions, SnapshotDeleteCommand.SnapshotDeleteCommandResult>(subscriptionResolver)
{
    public override async Task<CommandResponse> ExecuteAsync(CommandContext context, SnapshotDeleteOptions options, CancellationToken cancellationToken)
    {
        try
        {
            logger.LogInformation(
                "Deleting snapshot {SnapshotName} for file share {FileShareName} in resource group {ResourceGroup}, subscription {Subscription}",
                options.SnapshotName,
                options.FileShareName,
                options.ResourceGroup,
                options.Subscription);

            await fileSharesService.DeleteSnapshotAsync(
                options.Subscription!,
                options.ResourceGroup,
                options.FileShareName,
                options.SnapshotName,
                options.Tenant,
                options.RetryPolicy,
                cancellationToken);

            context.Response.Results = ResponseResult.Create(
                new(true, options.SnapshotName),
                FileSharesJsonContext.Default.SnapshotDeleteCommandResult);

            logger.LogInformation("Successfully deleted snapshot {SnapshotName}", options.SnapshotName);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error deleting snapshot. SnapshotName: {SnapshotName}, FileShareName: {FileShareName}, ResourceGroup: {ResourceGroup}.", options.SnapshotName, options.FileShareName, options.ResourceGroup);
            HandleException(context, ex);
        }

        return context.Response;
    }

    public sealed record SnapshotDeleteCommandResult(bool Deleted, string SnapshotName);
}
