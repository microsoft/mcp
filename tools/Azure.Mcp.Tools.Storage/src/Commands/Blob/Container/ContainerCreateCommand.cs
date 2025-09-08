// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Core.Commands;
using Azure.Mcp.Core.Extensions;
using Azure.Mcp.Tools.Storage.Options;
using Azure.Mcp.Tools.Storage.Options.Blob.Container;
using Azure.Mcp.Tools.Storage.Services;
using Azure.Storage.Blobs.Models;
using Microsoft.Extensions.Logging;

namespace Azure.Mcp.Tools.Storage.Commands.Blob.Container;

public sealed class ContainerCreateCommand(ILogger<ContainerCreateCommand> logger) : BaseContainerCommand<ContainerCreateOptions>()
{
    private const string CommandTitle = "Create Storage Blob Container";
    private readonly ILogger<ContainerCreateCommand> _logger = logger;

    public override string Name => "create";

    public override string Description =>
        """
        Creates an Azure Storage container, returning the last modified time and the ETag of the created container.
        """;

    public override string Title => CommandTitle;

    public override ToolMetadata Metadata => new() { Destructive = false, ReadOnly = false };

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
            var containerProperties = await storageService.CreateContainer(
                options.Account!,
                options.Container!,
                options.Subscription!,
                options.Tenant,
                options.RetryPolicy);

            var result = new ContainerCreateResult(
                containerProperties.LastModified,
                containerProperties.ETag.ToString(),
                containerProperties.PublicAccess?.ToString());

            context.Response.Results = ResponseResult.Create(
                new ContainerCreateCommandResult(result),
                StorageJsonContext.Default.ContainerCreateCommandResult);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Error creating container. Account: {Account}, Container: {Container}, Options: {@Options}",
                options.Account, options.Container, options);
            HandleException(context, ex);
        }

        return context.Response;
    }

    internal record ContainerCreateResult(
        DateTimeOffset LastModified,
        string ETag,
        string? PublicAccess);

    internal record ContainerCreateCommandResult(ContainerCreateResult Container);
}
