// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json.Serialization;
using Azure.Mcp.Core.Commands;
using Azure.Mcp.Tools.Storage.Options.Blob.Container;
using Azure.Mcp.Tools.Storage.Services;
using Microsoft.Extensions.Logging;

namespace Azure.Mcp.Tools.Storage.Commands.Blob.Container;

public sealed class ContainerCreateCommand(ILogger<ContainerCreateCommand> logger) : BaseContainerCommand<ContainerCreateOptions>()
{
    private const string CommandTitle = "Create Storage Blob Container";
    private readonly ILogger<ContainerCreateCommand> _logger = logger;

    public override string Name => "create";

    public override string Description =>
        """
        Create/provision a new Azure Storage blob container in a storage account. Required: --account <account>, --container <container>, --subscription <subscription>. Optional: --public-access-level, --tenant <tenant>. Returns: container name, lastModified, eTag, leaseStatus, publicAccessLevel, hasImmutabilityPolicy, hasLegalHold. Creates a logical container for organizing blobs within a storage account.
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

    public override async Task<CommandResponse> ExecuteAsync(CommandContext context, ParseResult parseResult)
    {
        if (!Validate(parseResult.CommandResult, context.Response).IsValid)
        {
            return context.Response;
        }

        var options = BindOptions(parseResult);

        try
        {
            var storageService = context.GetService<IStorageService>();
            var containerInfo = await storageService.CreateContainer(
                options.Account!,
                options.Container!,
                options.Subscription!,
                options.Tenant,
                options.RetryPolicy);

            context.Response.Results = ResponseResult.Create(new(containerInfo), StorageJsonContext.Default.ContainerCreateCommandResult);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating container. Account: {Account}, Container: {Container}",
                options.Account, options.Container);
            HandleException(context, ex);
        }

        return context.Response;
    }

    internal record ContainerCreateCommandResult([property: JsonPropertyName("container")] ContainerInfo Container);
}
