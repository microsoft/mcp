// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Core.Commands;
using Azure.Mcp.Core.Extensions;
using Azure.Mcp.Tools.Storage.Options;
using Azure.Mcp.Tools.Storage.Options.Blob;
using Azure.Mcp.Tools.Storage.Services;
using Microsoft.Extensions.Logging;

namespace Azure.Mcp.Tools.Storage.Commands.Blob;

public sealed class BlobUploadCommand(ILogger<BlobUploadCommand> logger) : BaseBlobCommand<BlobUploadOptions>
{
    private const string CommandTitle = "Upload Local File to Blob";
    private readonly ILogger<BlobUploadCommand> _logger = logger;

    public override string Name => "upload";

    public override string Description =>
        """
        Uploads a local file to an Azure Storage blob, only if the blob does not exist, returning the last modified time,
        ETag, and content hash of the uploaded blob.
        """;

    public override string Title => CommandTitle;

    public override ToolMetadata Metadata => new()
    {
        Destructive = false,
        Idempotent = false,
        OpenWorld = false,
        ReadOnly = false,
        LocalRequired = true,
        Secret = false
    };

    protected override void RegisterOptions(Command command)
    {
        base.RegisterOptions(command);
        command.Options.Add(StorageOptionDefinitions.LocalFilePath);
    }

    protected override BlobUploadOptions BindOptions(ParseResult parseResult)
    {
        var options = base.BindOptions(parseResult);
        options.LocalFilePath = parseResult.GetValueOrDefault<string>(StorageOptionDefinitions.LocalFilePath.Name);
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

            var result = await storageService.UploadBlob(
                options.Account!,
                options.Container!,
                options.Blob!,
                options.LocalFilePath!,
                options.Subscription!,
                options.Tenant,
                options.RetryPolicy);

            context.Response.Results = ResponseResult.Create(result, StorageJsonContext.Default.BlobUploadResult);

            _logger.LogInformation("Successfully uploaded file {LocalFilePath} to blob {Blob} in container {Container}.",
                options.LocalFilePath, options.Blob, options.Container);

            return context.Response;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error uploading file {LocalFilePath} to blob {Blob} in container {Container}.",
                options.LocalFilePath, options.Blob, options.Container);
            HandleException(context, ex);
            return context.Response;
        }
    }
}
