// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Core.Commands.Subscription;
using Azure.Mcp.Core.Services.Azure.Subscription;
using Azure.Mcp.Tools.Storage.Models;
using Azure.Mcp.Tools.Storage.Options.Blob.Container;
using Azure.Mcp.Tools.Storage.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Mcp.Core.Commands;
using Microsoft.Mcp.Core.Models.Command;

namespace Azure.Mcp.Tools.Storage.Commands.Blob.Container;

[CommandMetadata(
    Id = "e96eb850-abb8-431d-bdc6-7ccd0a24838e",
    Name = "get",
    Title = "Get Storage Container Details",
    Description = """
        Show/list containers in a storage account. Use this tool to list all blob containers in the storage account or
        show details for a specific Storage container. If no container specified, shows all containers in the storage
        account, optionally filtering on a prefix. The prefix is ignored if a container is specified.

        Required: --account, --subscription
        Optional: --container, --tenant, --prefix

        Returns: container name, lastModified, leaseStatus, publicAccess, metadata, and container properties.
        Do not use this tool to list blobs in a container.
        """,
    Destructive = false,
    Idempotent = true,
    OpenWorld = false,
    ReadOnly = true,
    Secret = false,
    LocalRequired = false)]
public sealed class ContainerGetCommand(ILogger<ContainerGetCommand> logger, IStorageService storageService, ISubscriptionResolver subscriptionResolver)
    : SubscriptionCommand<ContainerGetOptions, ContainerGetCommand.ContainerGetCommandResult>(subscriptionResolver)
{
    private readonly ILogger<ContainerGetCommand> _logger = logger;
    private readonly IStorageService _storageService = storageService;

    public override async Task<CommandResponse> ExecuteAsync(CommandContext context, ContainerGetOptions options, CancellationToken cancellationToken)
    {
        try
        {
            var containers = await _storageService.GetContainerDetails(
                options.Account,
                options.Container,
                options.Subscription!,
                options.Prefix,
                options.Tenant,
                options.RetryPolicy,
                cancellationToken
            );

            context.Response.Results = ResponseResult.Create(new ContainerGetCommandResult(containers ?? []), StorageJsonContext.Default.ContainerGetCommandResult);
            return context.Response;
        }
        catch (Exception ex)
        {
            if (options.Container is null)
            {
                _logger.LogError(ex, "Error listing container details. Account: {Account}.", options.Account);
            }
            else
            {
                _logger.LogError(ex, "Error getting container details. Account: {Account}, Container: {Container}.", options.Account, options.Container);
            }
            HandleException(context, ex);
            return context.Response;
        }
    }

    public record ContainerGetCommandResult(List<ContainerInfo> Containers);
}
