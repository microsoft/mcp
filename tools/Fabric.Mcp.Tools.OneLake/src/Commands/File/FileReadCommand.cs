// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net;
using System.Text;
using Azure.Mcp.Core.Extensions;
using Azure.Mcp.Core.Models;
using Fabric.Mcp.Tools.OneLake.Models;
using Fabric.Mcp.Tools.OneLake.Options;
using Fabric.Mcp.Tools.OneLake.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Mcp.Core.Commands;
using Microsoft.Mcp.Core.Extensions;
using Microsoft.Mcp.Core.Models.Option;

namespace Fabric.Mcp.Tools.OneLake.Commands.File;

[HiddenCommand]
public sealed class FileReadCommand(
    ILogger<FileReadCommand> logger,
    IOneLakeService oneLakeService) : BaseItemCommand<FileReadOptions>()
{
    private readonly ILogger<FileReadCommand> _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    private readonly IOneLakeService _oneLakeService = oneLakeService ?? throw new ArgumentNullException(nameof(oneLakeService));

    private const long InlineContentLimitBytes = 1 * 1024 * 1024; // 1 MiB inline payload limit

    public override string Id => "b70e5f70-d616-4a54-9879-6aa0a80345d9";
    public override string Name => "read";
    public override string Title => "Read OneLake File";
    public override string Description => "Read the contents of a file from OneLake storage.";

    public override ToolMetadata Metadata => new()
    {
        Destructive = false,
        Idempotent = true,
        LocalRequired = true,
        OpenWorld = false,
        ReadOnly = true,
        Secret = false
    };

    protected override void RegisterOptions(Command command)
    {
        base.RegisterOptions(command);
        command.Options.Add(FabricOptionDefinitions.FilePath);
        command.Options.Add(FabricOptionDefinitions.DownloadFilePath.AsOptional());
    }

    protected override FileReadOptions BindOptions(ParseResult parseResult)
    {
        var options = base.BindOptions(parseResult);
        options.FilePath = parseResult.GetValueOrDefault<string>(FabricOptionDefinitions.FilePath.Name) ?? string.Empty;
        options.DownloadFilePath = parseResult.GetValueOrDefault<string>(FabricOptionDefinitions.DownloadFilePath.Name);
        return options;
    }

    public override async Task<CommandResponse> ExecuteAsync(CommandContext context, ParseResult parseResult, CancellationToken cancellationToken)
    {
        if (!Validate(parseResult.CommandResult, context.Response).IsValid)
        {
            return context.Response;
        }

        var options = BindOptions(parseResult);
        try
        {
            string? downloadPath = null;
            if (!string.IsNullOrWhiteSpace(options.DownloadFilePath))
            {
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
                options.WorkspaceId!,
                options.ItemId!,
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
            _logger.LogError(ex, "Error reading file {FilePath} from workspace {WorkspaceId}, item {ItemId}. Options: {@Options}",
                options.FilePath, options.WorkspaceId, options.ItemId, options);
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

public sealed class FileReadOptions : BaseItemOptions
{
    public string FilePath { get; set; } = string.Empty;
    public string? DownloadFilePath { get; set; }
}
