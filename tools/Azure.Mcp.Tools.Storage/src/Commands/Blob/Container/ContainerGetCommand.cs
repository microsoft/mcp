// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net;
using Azure;
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

        Required: --account
        Optional: --container, --tenant, --prefix

        Returns: container name, lastModified, leaseStatus, publicAccess, metadata, and container properties.
        Do not use this tool to list blobs in a container.
        """,
    OperationPlane = ToolOperationPlane.Data,
    Destructive = false,
    Idempotent = true,
    OpenWorld = false,
    ReadOnly = true,
    Secret = false,
    LocalRequired = false)]
public sealed class ContainerGetCommand(ILogger<ContainerGetCommand> logger, IStorageService storageService)
    : AuthenticatedCommand<ContainerGetOptions, ContainerGetCommand.ContainerGetCommandResult>
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
                options.Prefix,
                options.Tenant,
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

    protected override string GetErrorMessage(Exception ex) => ex switch
    {
        RequestFailedException reqEx when reqEx.Status == (int)HttpStatusCode.Forbidden && reqEx.ErrorCode is "AuthorizationPermissionMismatch" or "InsufficientAccountPermissions" =>
            $"Access denied reading container details. This commonly happens when the caller has a management-plane role (e.g., Contributor/Owner) but lacks a data-plane role such as 'Storage Blob Data Reader' or 'Storage Blob Data Contributor' on this storage account. See https://learn.microsoft.com/rest/api/storageservices/blob-service-error-codes for error code details. Details: {reqEx.Message}",
        RequestFailedException reqEx when reqEx.Status == (int)HttpStatusCode.Forbidden =>
            $"Access denied reading container details. This can result from a missing data-plane RBAC role, storage account network/firewall restrictions, or an invalid/expired credential. See https://learn.microsoft.com/rest/api/storageservices/blob-service-error-codes for error code details. Details: {reqEx.Message}",
        RequestFailedException reqEx when reqEx.ErrorCode == "ContainerNotFound" =>
            $"Container not found. Verify the container exists in the specified storage account. Details: {reqEx.Message}",
        RequestFailedException reqEx when reqEx.Status == (int)HttpStatusCode.NotFound =>
            $"Storage account or container not found. Verify the account and container names. See https://learn.microsoft.com/rest/api/storageservices/blob-service-error-codes for error code details. Details: {reqEx.Message}",
        RequestFailedException reqEx => reqEx.Message,
        _ => base.GetErrorMessage(ex)
    };

    public record ContainerGetCommandResult(List<ContainerInfo> Containers);
}
