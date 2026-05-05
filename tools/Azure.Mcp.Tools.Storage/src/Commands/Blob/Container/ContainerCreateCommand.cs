// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json.Serialization.Metadata;
using Azure.Mcp.Tools.Storage.Models;
using Azure.Mcp.Tools.Storage.Options.Blob.Container;
using Azure.Mcp.Tools.Storage.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Mcp.Core.Commands;
using Microsoft.Mcp.Core.Models.Command;

namespace Azure.Mcp.Tools.Storage.Commands.Blob.Container;

public sealed class ContainerCreateCommand(ILogger<ContainerCreateCommand> logger, IStorageService storageService) : BaseContainerCommand<ContainerCreateOptions, ContainerCreateCommand.ContainerCreateCommandResult>()
{
    private const string CommandTitle = "Create Storage Blob Container";
    private readonly ILogger<ContainerCreateCommand> _logger = logger;
    private readonly IStorageService _storageService = storageService;

    public override string Id => "f5088334-e630-4df0-a5be-ac87787acad0";

    public override string Name => "create";

    public override string Description =>
        """
        Create/provision a new Azure Storage blob container in a storage account.
        
        Required: --account, --container, --subscription
        Optional: --tenant
        
        Returns: container name, lastModified, eTag, leaseStatus, publicAccessLevel, hasImmutabilityPolicy, hasLegalHold.
        Creates a logical container for organizing blobs within a storage account.
        """;

    public override string Title => CommandTitle;

    public override ToolMetadata Metadata => new()
    {
        Destructive = true,
        Idempotent = false,
        OpenWorld = false,
        ReadOnly = false,
        LocalRequired = false,
        Secret = false
    };

    protected override JsonTypeInfo<ContainerCreateCommandResult> ResultTypeInfo => StorageJsonContext.Default.ContainerCreateCommandResult;

    public override async Task<CommandResponse> ExecuteAsync(CommandContext context, ParseResult parseResult, CancellationToken cancellationToken)
    {
        if (!Validate(parseResult.CommandResult, context.Response).IsValid)
        {
            return context.Response;
        }

        var options = BindOptions(parseResult);

        try
        {
            var containerInfo = await _storageService.CreateContainer(
                options.Account!,
                options.Container!,
                options.Subscription!,
                options.Tenant,
                options.RetryPolicy,
                cancellationToken);

            SetResult(context, new(containerInfo));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating container. Account: {Account}, Container: {Container}",
                options.Account, options.Container);
            HandleException(context, ex);
        }

        return context.Response;
    }

    public record ContainerCreateCommandResult(ContainerInfo Container);
}
