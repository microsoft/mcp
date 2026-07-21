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
    Id = "b3c4d5e6-f7a8-4b9c-0d1e-2f3a4b5c6d7e",
    Name = "create",
    Title = "Create File Share",
    Description = "Create a new Azure managed file share resource in a resource group. This creates a high-performance, fully managed file share accessible via NFS protocol.",
    Destructive = true,
    Idempotent = false,
    OpenWorld = false,
    ReadOnly = false,
    Secret = false,
    LocalRequired = false)]
public sealed class FileShareCreateCommand(ILogger<FileShareCreateCommand> logger, IFileSharesService fileSharesService, ISubscriptionResolver subscriptionResolver)
    : SubscriptionCommand<FileShareCreateOptions, FileShareCreateCommand.FileShareCreateCommandResult>(subscriptionResolver)
{
    public override async Task<CommandResponse> ExecuteAsync(CommandContext context, FileShareCreateOptions options, CancellationToken cancellationToken)
    {
        try
        {
            logger.LogInformation("Creating file share. Subscription: {Subscription}, ResourceGroup: {ResourceGroup}, FileShareName: {FileShareName}",
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

            var fileShare = await fileSharesService.CreateOrUpdateFileShareAsync(
                options.Subscription!,
                options.ResourceGroup,
                options.Name,
                options.Location,
                options.MountName,
                options.MediaTier,
                options.Redundancy,
                options.Protocol,
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

            context.Response.Results = ResponseResult.Create(new(fileShare), FileSharesJsonContext.Default.FileShareCreateCommandResult);

            logger.LogInformation("File share created successfully. FileShare: {FileShareName}", options.Name);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to create file share");
            HandleException(context, ex);
        }

        return context.Response;
    }

    public sealed record FileShareCreateCommandResult(FileShareInfo FileShare);
}
