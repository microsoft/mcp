// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Core.Commands.Subscription;
using Azure.Mcp.Core.Services.Azure.Subscription;
using Azure.Mcp.Tools.FileShares.Options.FileShare;
using Azure.Mcp.Tools.FileShares.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Mcp.Core.Commands;
using Microsoft.Mcp.Core.Models.Command;

namespace Azure.Mcp.Tools.FileShares.Commands.FileShare;

/// <summary>
/// Deletes a file share.
/// </summary>
[CommandMetadata(
    Id = "e9f0a1b2-c3d4-4e5f-6a7b-8c9d0e1f2a3b",
    Name = "delete",
    Title = "Delete File Share",
    Description = "Delete a file share",
    Destructive = true,
    Idempotent = false,
    OpenWorld = false,
    ReadOnly = false,
    Secret = false,
    LocalRequired = false)]
public sealed class FileShareDeleteCommand(ILogger<FileShareDeleteCommand> logger, IFileSharesService fileSharesService, ISubscriptionResolver subscriptionResolver)
    : SubscriptionCommand<FileShareDeleteOptions, FileShareDeleteCommand.FileShareDeleteCommandResult>(subscriptionResolver)
{
    public override async Task<CommandResponse> ExecuteAsync(CommandContext context, FileShareDeleteOptions options, CancellationToken cancellationToken)
    {
        try
        {
            logger.LogInformation(
                "Deleting file share {FileShareName} in resource group {ResourceGroup}, subscription {Subscription}",
                options.Name,
                options.ResourceGroup,
                options.Subscription);

            await fileSharesService.DeleteFileShareAsync(
                options.Subscription!,
                options.ResourceGroup,
                options.Name,
                options.Tenant,
                options.RetryPolicy,
                cancellationToken);

            context.Response.Results = ResponseResult.Create(
                new(true, options.Name!),
                FileSharesJsonContext.Default.FileShareDeleteCommandResult);

            logger.LogInformation(
                "Successfully deleted file share {FileShareName}",
                options.Name);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error deleting file share. FileShareName: {FileShareName}, ResourceGroup: {ResourceGroup}.", options.Name, options.ResourceGroup);
            HandleException(context, ex);
        }

        return context.Response;
    }

    public sealed record FileShareDeleteCommandResult(bool Deleted, string FileShareName);
}
