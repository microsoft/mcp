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
    Id = "f1a2b3c4-d5e6-4f7a-8b9c-0d1e2f3a4b5c",
    Name = "create",
    Title = "Create File Share Snapshot",
    Description = "Create a snapshot of an Azure managed file share. Snapshots are read-only point-in-time copies used for backup and recovery.",
    Destructive = true,
    Idempotent = false,
    OpenWorld = false,
    ReadOnly = false,
    Secret = false,
    LocalRequired = false)]
public sealed class SnapshotCreateCommand(ILogger<SnapshotCreateCommand> logger, IFileSharesService fileSharesService, ISubscriptionResolver subscriptionResolver)
    : SubscriptionCommand<SnapshotCreateOptions, SnapshotCreateCommand.SnapshotCreateCommandResult>(subscriptionResolver)
{
    public override async Task<CommandResponse> ExecuteAsync(CommandContext context, SnapshotCreateOptions options, CancellationToken cancellationToken)
    {
        try
        {
            logger.LogInformation("Creating snapshot. Subscription: {Subscription}, ResourceGroup: {ResourceGroup}, FileShareName: {FileShareName}, SnapshotName: {SnapshotName}",
                options.Subscription, options.ResourceGroup, options.FileShareName, options.SnapshotName);

            // Parse metadata if provided
            Dictionary<string, string>? metadata = null;
            if (!string.IsNullOrEmpty(options.Metadata))
            {
                try
                {
                    metadata = JsonSerializer.Deserialize(options.Metadata, FileSharesJsonContext.Default.DictionaryStringString);
                }
                catch (JsonException ex)
                {
                    throw new ArgumentException($"Invalid metadata JSON format: {ex.Message}", nameof(options.Metadata));
                }
            }

            var snapshot = await fileSharesService.CreateSnapshotAsync(
                options.Subscription!,
                options.ResourceGroup,
                options.FileShareName,
                options.SnapshotName,
                metadata,
                options.Tenant,
                options.RetryPolicy,
                cancellationToken);

            context.Response.Results = ResponseResult.Create(new(snapshot), FileSharesJsonContext.Default.SnapshotCreateCommandResult);

            logger.LogInformation("Snapshot created successfully. SnapshotName: {SnapshotName}", options.SnapshotName);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to create snapshot");
            HandleException(context, ex);
        }

        return context.Response;
    }

    public sealed record SnapshotCreateCommandResult(FileShareSnapshotInfo Snapshot);
}
