// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json.Serialization;
using Azure.Mcp.Core.Commands;
using Azure.Mcp.Core.Extensions;
using Azure.Mcp.Core.Models.Option;
using Azure.Mcp.Tools.Storage.Commands.Blob.Container;
using Azure.Mcp.Tools.Storage.Options;
using Azure.Mcp.Tools.Storage.Options.Blob;
using Azure.Mcp.Tools.Storage.Services;
using Microsoft.Extensions.Logging;

namespace Azure.Mcp.Tools.Storage.Commands.Blob;

public sealed class BlobGetCommand(ILogger<BlobGetCommand> logger) : BaseContainerCommand<BlobGetOptions>()
{
    private const string CommandTitle = "Get Storage Blob Details";
    private readonly ILogger<BlobGetCommand> _logger = logger;

    public override string Name => "get";

    public override string Description =>
        $"""
        Gets the details of Azure Storage blobs, including metadata properties, approximate size, last modification time, and more.
        If a specific blob name is not provided, the command will return details for all blobs within the specified container.
        """;

    public override string Title => CommandTitle;

    public override ToolMetadata Metadata => new()
    {
        Destructive = false,
        Idempotent = true,
        OpenWorld = false,
        ReadOnly = true,
        LocalRequired = false,
        Secret = false
    };

    protected override void RegisterOptions(Command command)
    {
        base.RegisterOptions(command);
        command.Options.Add(StorageOptionDefinitions.Blob.AsOptional());
    }

    protected override BlobGetOptions BindOptions(ParseResult parseResult)
    {
        var options = base.BindOptions(parseResult);
        options.Blob = parseResult.GetValueOrDefault<string>(StorageOptionDefinitions.Blob.Name);
        return options;
    }

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
            var details = await storageService.GetBlobDetails(
                options.Account!,
                options.Container!,
                options.Blob,
                options.Subscription!,
                options.Tenant,
                options.RetryPolicy
            );

            context.Response.Results = ResponseResult.Create(new(details ?? []), StorageJsonContext.Default.BlobGetCommandResult);
            return context.Response;
        }
        catch (Exception ex)
        {
            if (options.Blob is null)
            {
                _logger.LogError(ex, "Error listing blob details. Account: {Account}, Container: {Container}.", options.Account, options.Container);
            }
            else
            {
                _logger.LogError(ex, "Error getting blob details. Account: {Account}, Container: {Container}, Blob: {Blob}.", options.Account, options.Container, options.Blob);
            }
            HandleException(context, ex);
            return context.Response;
        }
    }

    internal record BlobGetCommandResult([property: JsonPropertyName("blobs")] List<BlobInfo> Blobs);
}
