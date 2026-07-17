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
using Microsoft.Mcp.Core.Models.Command;
using Microsoft.Mcp.Core.Options;

namespace Fabric.Mcp.Tools.OneLake.Commands.File;

[CommandMetadata(
    Id = "75d6cb4c-4e81-4e69-a4ec-eca53a7dacd9",
    Name = "download_file",
    Title = "Download OneLake File",
    Description = "Downloads a file from OneLake storage. Use this when the user needs to retrieve file content or metadata. Returns base64 content, metadata, and text when applicable.",
    Destructive = false,
    Idempotent = true,
    LocalRequired = false,
    OpenWorld = false,
    ReadOnly = true,
    Secret = false)]
public sealed class BlobGetCommand(
    ILogger<BlobGetCommand> logger,
    IOneLakeService oneLakeService,
    IOptions<ServiceStartOptions> serviceOptions) : AuthenticatedCommand<BlobGetOptions, BlobGetCommand.BlobGetCommandResult>
{
    private readonly ILogger<BlobGetCommand> _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    private readonly IOneLakeService _oneLakeService = oneLakeService ?? throw new ArgumentNullException(nameof(oneLakeService));
    private readonly IOptions<ServiceStartOptions> _serviceOptions = serviceOptions ?? throw new ArgumentNullException(nameof(serviceOptions));

    private const long InlineContentLimitBytes = 1 * 1024 * 1024; // 1 MiB inline payload limit

    public override void ValidateOptions(BlobGetOptions options, ValidationResult validationResult)
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

    public override async Task<CommandResponse> ExecuteAsync(CommandContext context, BlobGetOptions options, CancellationToken cancellationToken)
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

            var result = await _oneLakeService.GetBlobAsync(
                workspaceIdentifier,
                itemIdentifier,
                options.FilePath,
                downloadOptions,
                cancellationToken);

            var messageBuilder = new StringBuilder();
            if (downloadPath is not null)
            {
                var resolvedPath = result.ContentFilePath ?? downloadPath;
                messageBuilder.Append($"File downloaded to local file '{resolvedPath}'.");
            }
            else if (result.InlineContentTruncated)
            {
                messageBuilder.Append($"File metadata retrieved. Content exceeds the inline limit of {InlineContentLimitBytes:N0} bytes; provide --download-file-path when running locally to save the content.");
            }
            else
            {
                messageBuilder.Append("File retrieved successfully.");
            }

            var finalMessage = messageBuilder.ToString();

            var commandResult = new BlobGetCommandResult(
                result,
                finalMessage);

            context.Response.Status = HttpStatusCode.OK;
            context.Response.Message = finalMessage;
            context.Response.Results = ResponseResult.Create(commandResult, OneLakeJsonContext.Default.BlobGetCommandResult);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving blob {BlobPath} in workspace {WorkspaceId}, item {ItemId}.",
                options.FilePath, options.WorkspaceId, options.ItemId);
            HandleException(context, ex);
        }

        return context.Response;
    }

    public sealed record BlobGetCommandResult(BlobGetResult Blob, string Message);

    protected override HttpStatusCode GetStatusCode(Exception ex) => ex switch
    {
        ArgumentException => HttpStatusCode.BadRequest,
        _ => base.GetStatusCode(ex)
    };
}

public sealed class BlobGetOptions
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
