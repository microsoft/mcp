// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net;
using System.Text;
using Fabric.Mcp.Tools.OneLake.Models;
using Fabric.Mcp.Tools.OneLake.Options;
using Fabric.Mcp.Tools.OneLake.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Mcp.Core.Areas.Server.Options;
using Microsoft.Mcp.Core.Commands;
using Microsoft.Mcp.Core.Models;
using Microsoft.Mcp.Core.Models.Command;
using Microsoft.Mcp.Core.Options;

namespace Fabric.Mcp.Tools.OneLake.Commands.File;

[HiddenCommand]
[CommandMetadata(
    Id = "b70e5f70-d616-4a54-9879-6aa0a80345d9",
    Name = "read",
    Title = "Read OneLake File",
    Description = "Read the contents of a file from OneLake storage.",
    Destructive = false,
    Idempotent = true,
    LocalRequired = false,
    OpenWorld = false,
    ReadOnly = true,
    Secret = false)]
public sealed class FileReadCommand(
    ILogger<FileReadCommand> logger,
    IOneLakeService oneLakeService,
    IOptions<ServiceStartOptions> serviceOptions) : AuthenticatedCommand<FileReadOptions, FileReadCommand.FileReadCommandResult>
{
    private readonly ILogger<FileReadCommand> _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    private readonly IOneLakeService _oneLakeService = oneLakeService ?? throw new ArgumentNullException(nameof(oneLakeService));
    private readonly IOptions<ServiceStartOptions> _serviceOptions = serviceOptions ?? throw new ArgumentNullException(nameof(serviceOptions));

    private const long InlineContentLimitBytes = 1 * 1024 * 1024; // 1 MiB inline payload limit

    public override void ValidateOptions(FileReadOptions options, ValidationResult validationResult)
    {
        base.ValidateOptions(options, validationResult);
        if (string.IsNullOrWhiteSpace(options.WorkspaceId) && string.IsNullOrWhiteSpace(options.Workspace))
        {
            validationResult.Errors.Add("Workspace identifier is required. Provide --workspace or --workspace-id.");
        }

        if (string.IsNullOrWhiteSpace(options.ItemId) && string.IsNullOrWhiteSpace(options.Item))
        {
            validationResult.Errors.Add("Item identifier is required. Provide --item or --item-id.");
        }
    }

    public override async Task<CommandResponse> ExecuteAsync(CommandContext context, FileReadOptions options, CancellationToken cancellationToken)
    {
        try
        {
            var workspaceIdentifier = !string.IsNullOrWhiteSpace(options.WorkspaceId)
                ? options.WorkspaceId
                : options.Workspace!;

            var itemIdentifier = !string.IsNullOrWhiteSpace(options.ItemId)
                ? options.ItemId
                : options.Item!;

            var transport = _serviceOptions.Value.Transport ?? "stdio";
            var isLocalTransport = string.Equals(transport, "stdio", StringComparison.OrdinalIgnoreCase);

            string? downloadPath = null;
            if (!string.IsNullOrWhiteSpace(options.DownloadFilePath))
            {
                if (!isLocalTransport)
                {
                    throw new ArgumentException("The --download-file-path option is only supported when the server runs with stdio transport.", nameof(options.DownloadFilePath));
                }

                var candidatePath = options.DownloadFilePath!;
                downloadPath = Path.IsPathRooted(candidatePath)
                    ? candidatePath
                    : Path.GetFullPath(candidatePath);

                var directory = Path.GetDirectoryName(downloadPath);
                if (!string.IsNullOrWhiteSpace(directory) && !Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                }
            }

            await using var fileStream = downloadPath is not null
                ? new FileStream(downloadPath, FileMode.Create, FileAccess.Write, FileShare.None)
                : null;

            var downloadOptions = new BlobDownloadOptions
            {
                DestinationStream = fileStream,
                LocalFilePath = downloadPath,
                IncludeInlineContent = downloadPath is null,
                InlineContentLimit = InlineContentLimitBytes
            };

            var blobResult = await _oneLakeService.ReadFileAsync(
                workspaceIdentifier,
                itemIdentifier,
                options.FilePath,
                downloadOptions,
                cancellationToken);

            var messageBuilder = new StringBuilder();

            var resolvedPath = blobResult.ContentFilePath ?? downloadPath;
            if (resolvedPath is not null)
            {
                messageBuilder.Append($"File downloaded to local file '{resolvedPath}'.");
            }
            else if (blobResult.InlineContentTruncated)
            {
                messageBuilder.Append($"File metadata retrieved. Content exceeds the inline limit of {InlineContentLimitBytes:N0} bytes; provide --download-file-path when running locally to save the content.");
            }
            else
            {
                messageBuilder.Append("File content retrieved successfully.");
            }

            var finalMessage = messageBuilder.ToString();

            string? content = blobResult.ContentText;
            if (content is null && blobResult.ContentBase64 is { Length: > 0 })
            {
                try
                {
                    var bytes = Convert.FromBase64String(blobResult.ContentBase64);
                    content = Encoding.UTF8.GetString(bytes);
                }
                catch (FormatException)
                {
                    // Ignore invalid base64 content; leave content null
                }
            }

            var result = new FileReadCommandResult(
                options.FilePath,
                content,
                finalMessage,
                resolvedPath,
                blobResult.InlineContentTruncated,
                blobResult.ContentLength,
                blobResult.ContentType,
                blobResult.Charset);

            context.Response.Message = finalMessage;
            context.Response.Results = ResponseResult.Create(result, OneLakeJsonContext.Default.FileReadCommandResult);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error reading file {FilePath} from workspace {WorkspaceId}, item {ItemId}.",
                options.FilePath, options.WorkspaceId, options.ItemId);
            HandleException(context, ex);
        }

        return context.Response;
    }

    public sealed record FileReadCommandResult(
        string FilePath,
        string? Content,
        string Message,
        string? ContentFilePath,
        bool InlineContentTruncated,
        long? ContentLength,
        string? ContentType,
        string? Charset);

    protected override HttpStatusCode GetStatusCode(Exception ex) => ex switch
    {
        ArgumentException => HttpStatusCode.BadRequest,
        _ => base.GetStatusCode(ex)
    };
}

public sealed class FileReadOptions
{
    [Option(Description = "The ID of the Microsoft Fabric workspace.")]
    public string? WorkspaceId { get; set; }

    [Option(Description = "The name or ID of the Microsoft Fabric workspace.")]
    public string? Workspace { get; set; }

    [Option(Description = "The ID of the Fabric item.")]
    public string? ItemId { get; set; }

    [Option(Description = "The name or ID of the Fabric item. When using friendly names, MUST include the item type suffix (e.g., 'ItemName.Lakehouse', 'ItemName.Warehouse').")]
    public string? Item { get; set; }

    [Option(Description = "The path to the file in OneLake.")]
    public required string FilePath { get; set; }

    [Option(Description = "Local path to save the downloaded content when running locally.")]
    public string? DownloadFilePath { get; set; }
}
