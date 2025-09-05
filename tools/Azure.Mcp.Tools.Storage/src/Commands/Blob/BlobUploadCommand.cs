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

    // Define options from OptionDefinitions
    private readonly Option<string> _localFilePathOption = StorageOptionDefinitions.LocalFilePath;

    public override string Name => "upload";

    public override string Description =>
        """
        Uploads a local file to a blob in Azure Storage only if the blob does not exist.
        Returns details about the uploaded blob including last modified time, ETag, and content hash.
        """;

    public override string Title => CommandTitle;

    public override ToolMetadata Metadata => new()
    {
        Destructive = true,
        ReadOnly = false
    };

    protected override void RegisterOptions(Command command)
    {
        base.RegisterOptions(command);
        command.Options.Add(_localFilePathOption);
    }

    protected override BlobUploadOptions BindOptions(ParseResult parseResult)
    {
        var options = base.BindOptions(parseResult);
        options.LocalFilePath = parseResult.GetValueOrDefault(_localFilePathOption);
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
