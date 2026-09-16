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
    Id = "f5088334-e630-4df0-a5be-ac87787acad0",
    Name = "create",
    Title = "Create Storage Blob Container",
    Description = """
        Create/provision a new Azure Storage blob container in a storage account.

        Required: --account, --container
        Optional: --tenant

        Returns: container name, lastModified, eTag, leaseStatus, publicAccessLevel, hasImmutabilityPolicy, hasLegalHold.
        Creates a logical container for organizing blobs within a storage account.
        """,
    OperationPlane = ToolOperationPlane.Data,
    Destructive = true,
    Idempotent = false,
    OpenWorld = false,
    ReadOnly = false,
    Secret = false,
    LocalRequired = false)]
public sealed class ContainerCreateCommand(ILogger<ContainerCreateCommand> logger, IStorageService storageService)
    : AuthenticatedCommand<ContainerCreateOptions, ContainerCreateCommand.ContainerCreateCommandResult>
{
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

            context.Response.Results = ResponseResult.Create(new ContainerCreateCommandResult(containerInfo), StorageJsonContext.Default.ContainerCreateCommandResult);
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
        RequestFailedException reqEx when reqEx.Status == (int)HttpStatusCode.Forbidden =>
            $"Access denied creating container. Verify the caller has a data-plane role (e.g., 'Storage Blob Data Contributor' or 'Storage Blob Data Owner') on this storage account; management-plane roles like Contributor/Owner do not grant blob data access. Details: {reqEx.Message}",
        RequestFailedException reqEx when reqEx.Status == (int)HttpStatusCode.Conflict =>
            "Container already exists. This tool only creates a container when one does not already exist with that name.",
        RequestFailedException reqEx when reqEx.Status == (int)HttpStatusCode.NotFound =>
            "Storage account not found. Verify the account name.",
        RequestFailedException reqEx => reqEx.Message,
        _ => base.GetErrorMessage(ex)
    };

    public record ContainerCreateCommandResult(ContainerInfo Container);
}
