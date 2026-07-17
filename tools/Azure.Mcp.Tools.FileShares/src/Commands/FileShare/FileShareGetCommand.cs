// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Core.Commands.Subscription;
using Azure.Mcp.Core.Services.Azure.Subscription;
using Azure.Mcp.Tools.FileShares.Models;
using Azure.Mcp.Tools.FileShares.Options;
using Azure.Mcp.Tools.FileShares.Options.FileShare;
using Azure.Mcp.Tools.FileShares.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Mcp.Core.Commands;
using Microsoft.Mcp.Core.Extensions;
using Microsoft.Mcp.Core.Models.Command;
using Microsoft.Mcp.Core.Models.Option;

namespace Azure.Mcp.Tools.FileShares.Commands.FileShare;

[CommandMetadata(
    Id = "c5d6e7f8-a9b0-4c1d-2e3f-4a5b6c7d8e9f",
    Name = "get",
    Title = "Get File Share",
    Description = "Get details of a specific file share or list all file shares. If --name is provided, returns a specific file share; otherwise, lists all file shares in the subscription or resource group.",
    Destructive = false,
    Idempotent = true,
    OpenWorld = false,
    ReadOnly = true,
    Secret = false,
    LocalRequired = false)]
public sealed class FileShareGetCommand(ILogger<FileShareGetCommand> logger, IFileSharesService fileSharesService, ISubscriptionResolver subscriptionResolver)
    : SubscriptionCommand<FileShareGetOptions, FileShareGetCommand.FileShareGetCommandResult>(subscriptionResolver)
{
    public override void ValidateOptions(FileShareGetOptions options, ValidationResult validationResult)
    {
        base.ValidateOptions(options, validationResult);

        if (!string.IsNullOrWhiteSpace(options.Name) && string.IsNullOrWhiteSpace(options.ResourceGroup))
        {
            validationResult.Errors.Add("--resource-group is required when --name is provided.");
        }
    }

    public override async Task<CommandResponse> ExecuteAsync(CommandContext context, FileShareGetOptions options, CancellationToken cancellationToken)
    {
        try
        {
            // If file share name is provided, get specific file share
            if (!string.IsNullOrEmpty(options.Name))
            {
                logger.LogInformation("Getting file share. Subscription: {Subscription}, ResourceGroup: {ResourceGroup}, FileShareName: {FileShareName}",
                    options.Subscription, options.ResourceGroup, options.Name);

                var fileShare = await fileSharesService.GetFileShareAsync(
                    options.Subscription!,
                    options.ResourceGroup!,
                    options.Name,
                    options.Tenant,
                    options.RetryPolicy,
                    cancellationToken);

                context.Response.Results = ResponseResult.Create(new([fileShare]), FileSharesJsonContext.Default.FileShareGetCommandResult);

                logger.LogInformation("Successfully retrieved file share. FileShareName: {FileShareName}", options.Name);
            }
            else
            {
                // List all file shares
                logger.LogInformation("Listing file shares. Subscription: {Subscription}, ResourceGroup: {ResourceGroup}",
                    options.Subscription, options.ResourceGroup ?? "(all)");

                var fileShares = await fileSharesService.ListFileSharesAsync(
                    options.Subscription!,
                    options.ResourceGroup,
                    options.Tenant,
                    options.RetryPolicy,
                    cancellationToken);

                context.Response.Results = ResponseResult.Create(new(fileShares ?? []), FileSharesJsonContext.Default.FileShareGetCommandResult);

                logger.LogInformation("Successfully listed {Count} file shares", fileShares?.Count ?? 0);
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to get file share(s)");
            HandleException(context, ex);
        }

        return context.Response;
    }

    public sealed record FileShareGetCommandResult(List<FileShareInfo> FileShares);
}
