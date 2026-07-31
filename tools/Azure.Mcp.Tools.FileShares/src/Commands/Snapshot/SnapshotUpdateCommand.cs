// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json;
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
    Id = "b5c6d7e8-f9a0-4b1c-2d3e-4f5a6b7c8d9e",
    Name = "update",
    Title = "Update File Share Snapshot",
    Description = "Update properties and metadata of an Azure managed file share snapshot, such as tags or retention policies.",
    Destructive = true,
    Idempotent = false,
    OpenWorld = false,
    ReadOnly = false,
    Secret = false,
    LocalRequired = false)]
public sealed class SnapshotUpdateCommand(ILogger<SnapshotUpdateCommand> logger, IFileSharesService fileSharesService, ISubscriptionResolver subscriptionResolver)
    : SubscriptionCommand<SnapshotUpdateOptions, SnapshotUpdateCommand.SnapshotUpdateCommandResult>(subscriptionResolver)
{
    public override async Task<CommandResponse> ExecuteAsync(CommandContext context, SnapshotUpdateOptions options, CancellationToken cancellationToken)
    {
        try
        {
            logger.LogInformation("Updating snapshot. Subscription: {Subscription}, ResourceGroup: {ResourceGroup}, FileShareName: {FileShareName}, SnapshotName: {SnapshotName}",
                options.Subscription, options.ResourceGroup, options.FileShareName, options.SnapshotName);

            // Parse metadata if provided
            Dictionary<string, string>? metadata = null;
            if (!string.IsNullOrEmpty(options.Metadata))
            {
                try
                {
                    metadata = JsonSerializer.Deserialize(options.Metadata, FileSharesJsonContext.Default.DictionaryStringString);
                }
                catch (Exception ex)
                {
                    logger.LogWarning(ex, "Failed to parse metadata JSON: {Metadata}", options.Metadata);
                }
            }

            var snapshot = await fileSharesService.PatchSnapshotAsync(
                options.Subscription!,
                options.ResourceGroup,
                options.FileShareName,
                options.SnapshotName,
                metadata,
                options.Tenant,
                options.RetryPolicy,
                cancellationToken);

            context.Response.Results = ResponseResult.Create(new(snapshot), FileSharesJsonContext.Default.SnapshotUpdateCommandResult);

            logger.LogInformation("Snapshot updated successfully. SnapshotName: {SnapshotName}", options.SnapshotName);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to update snapshot");
            HandleException(context, ex);
        }

        return context.Response;
    }

    public sealed record SnapshotUpdateCommandResult(FileShareSnapshotInfo Snapshot);
}
