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
#if STORAGE_GENERATED_MCP
using Azure.Mcp.Tools.Storage.GeneratedMcp;
using ContainerCreateOptions = Azure.Mcp.Tools.Storage.GeneratedMcp.GeneratedContainerCreateOptions;
#endif

namespace Azure.Mcp.Tools.Storage.Commands.Blob.Container;

[CommandMetadata(
#if STORAGE_GENERATED_MCP
    Id = ContainerCreateCatalog.Id,
    Name = ContainerCreateCatalog.Name,
    Title = ContainerCreateCatalog.Title,
    Description = ContainerCreateCatalog.Description,
    Destructive = ContainerCreateCatalog.Destructive,
    Idempotent = ContainerCreateCatalog.Idempotent,
    OpenWorld = ContainerCreateCatalog.OpenWorld,
    ReadOnly = ContainerCreateCatalog.ReadOnly,
#else
    Id = "f5088334-e630-4df0-a5be-ac87787acad0",
    Name = "create",
    Title = "Create Storage Blob Container",
    Description = ContainerCreateCommand.CustomizedDescription,
    Destructive = true,
    Idempotent = false,
    OpenWorld = false,
    ReadOnly = false,
#endif
    OperationPlane = ToolOperationPlane.Data,
    Secret = false,
    LocalRequired = false)]
public sealed class ContainerCreateCommand(ILogger<ContainerCreateCommand> logger, IStorageService storageService)
    : AuthenticatedCommand<ContainerCreateOptions, ContainerCreateCommand.ContainerCreateCommandResult>
{
    internal const string CustomizedDescription = """
        Create/provision a new Azure Storage blob container in a storage account.

        Required: --account, --container
        Optional: --tenant

        Returns: container name, lastModified, eTag, leaseStatus, publicAccessLevel, hasImmutabilityPolicy, hasLegalHold.
        Creates a logical container for organizing blobs within a storage account.
        """;

    private readonly ILogger<ContainerCreateCommand> _logger = logger;
    private readonly IStorageService _storageService = storageService;

    public override async Task<CommandResponse> ExecuteAsync(CommandContext context, ContainerCreateOptions options, CancellationToken cancellationToken)
    {
        try
        {
            var containerInfo = await _storageService.CreateContainer(
                options.Account,
                options.Container,
                options.Tenant,
                cancellationToken);

#if STORAGE_GENERATED_MCP
            context.Response.Results = ResponseResult.Create(new GeneratedContainerCreateResult { Container = containerInfo },
                GeneratedStorageJsonContext.Default.GeneratedContainerCreateResult);
#else
            context.Response.Results = ResponseResult.Create(new ContainerCreateCommandResult(containerInfo), StorageJsonContext.Default.ContainerCreateCommandResult);
#endif
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating container. Account: {Account}, Container: {Container}",
                options.Account, options.Container);
            HandleException(context, ex);
        }

        return context.Response;
    }

    protected override string GetErrorMessage(Exception ex) => ex switch
    {
        RequestFailedException reqEx when reqEx.Status == (int)HttpStatusCode.Forbidden && reqEx.ErrorCode is "AuthorizationPermissionMismatch" or "InsufficientAccountPermissions" =>
            $"Access denied creating container. This commonly happens when the caller has a management-plane role (e.g., Contributor/Owner) but lacks a data-plane role such as 'Storage Blob Data Contributor' or 'Storage Blob Data Owner' on this storage account. See https://learn.microsoft.com/rest/api/storageservices/blob-service-error-codes for error code details. Details: {reqEx.Message}",
        RequestFailedException reqEx when reqEx.Status == (int)HttpStatusCode.Forbidden =>
            $"Access denied creating container. This can result from a missing data-plane RBAC role, storage account network/firewall restrictions, or an invalid/expired credential. See https://learn.microsoft.com/rest/api/storageservices/blob-service-error-codes for error code details. Details: {reqEx.Message}",
        RequestFailedException reqEx when reqEx.ErrorCode == "ContainerAlreadyExists" =>
            $"Container already exists. This tool only creates a container when one does not already exist with that name; use 'storage blob container get' first if you only need to ensure the container exists. Details: {reqEx.Message}",
        RequestFailedException reqEx when reqEx.Status == (int)HttpStatusCode.NotFound =>
            $"Storage account not found. Verify the account name. Details: {reqEx.Message}",
#if STORAGE_GENERATED_MCP
        RequestFailedException reqEx => ContainerCreateCatalog.GetDefaultError(reqEx.Status, reqEx.Message),
#else
        RequestFailedException reqEx => reqEx.Message,
#endif
        _ => base.GetErrorMessage(ex)
    };

    public record ContainerCreateCommandResult(ContainerInfo Container);
}
