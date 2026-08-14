// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json;
using Azure.Mcp.Core.Commands.Subscription;
using Azure.Mcp.Core.Services.Azure.Subscription;
using Azure.Mcp.Tools.FileShares.Models;
using Azure.Mcp.Tools.FileShares.Options.FileShare;
using Azure.Mcp.Tools.FileShares.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Mcp.Core.Commands;
using Microsoft.Mcp.Core.Models.Command;

namespace Azure.Mcp.Tools.FileShares.Commands.FileShare;

[CommandMetadata(
    Id = "d7e8f9a0-b1c2-4d3e-4f5a-6b7c8d9e0f1a",
    Name = "update",
    Title = "Update File Share",
    Description = "Update an existing Azure managed file share resource. Allows updating mutable properties like provisioned storage, IOPS, throughput, and network access settings.",
    Destructive = true,
    Idempotent = false,
    OpenWorld = false,
    ReadOnly = false,
    Secret = false,
    LocalRequired = false)]
public sealed class FileShareUpdateCommand(ILogger<FileShareUpdateCommand> logger, IFileSharesService fileSharesService, ISubscriptionResolver subscriptionResolver)
    : SubscriptionCommand<FileShareUpdateOptions, FileShareUpdateCommand.FileShareUpdateCommandResult>(subscriptionResolver)
{
    public override async Task<CommandResponse> ExecuteAsync(CommandContext context, FileShareUpdateOptions options, CancellationToken cancellationToken)
    {
        try
        {
            logger.LogInformation("Updating file share. Subscription: {Subscription}, ResourceGroup: {ResourceGroup}, FileShareName: {FileShareName}",
                options.Subscription, options.ResourceGroup, options.Name);

            // Parse tags if provided
            Dictionary<string, string>? tags = null;
            if (!string.IsNullOrEmpty(options.Tags))
            {
                try
                {
                    tags = JsonSerializer.Deserialize(options.Tags, FileSharesJsonContext.Default.DictionaryStringString);
                }
                catch (Exception ex)
                {
                    logger.LogWarning(ex, "Failed to parse tags JSON: {Tags}", options.Tags);
                }
            }

            // Parse allowed subnets if provided
            string[]? allowedSubnets = null;
            if (!string.IsNullOrEmpty(options.AllowedSubnets))
            {
                allowedSubnets = options.AllowedSubnets.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
            }

            var fileShare = await fileSharesService.PatchFileShareAsync(
                options.Subscription!,
                options.ResourceGroup,
                options.Name,
                options.ProvisionedStorageInGiB,
                options.ProvisionedIoPerSec,
                options.ProvisionedThroughputMiBPerSec,
                options.PublicNetworkAccess,
                options.NfsRootSquash,
                options.NfsEncryptionInTransit,
                allowedSubnets,
                tags,
                options.Tenant,
                options.RetryPolicy,
                cancellationToken);

            context.Response.Results = ResponseResult.Create(new(fileShare), FileSharesJsonContext.Default.FileShareUpdateCommandResult);

            logger.LogInformation("File share updated successfully. FileShare: {FileShareName}", options.Name);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to update file share");
            HandleException(context, ex);
        }

        return context.Response;
    }

    public sealed record FileShareUpdateCommandResult(FileShareInfo FileShare);
}
