// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.CommandLine;
using System.CommandLine.Parsing;
using System.IO;
using System.Net;
using System.Text;
using Azure.Mcp.Core.Areas.Server.Options;
using Azure.Mcp.Core.Commands;
using Azure.Mcp.Core.Extensions;
using Azure.Mcp.Core.Models;
using Azure.Mcp.Core.Options;
using Fabric.Mcp.Tools.OneLake.Models;
using Fabric.Mcp.Tools.OneLake.Options;
using Fabric.Mcp.Tools.OneLake.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Mcp.Core.Commands;
using Microsoft.Mcp.Core.Models.Command;
using Microsoft.Mcp.Core.Models.Option;

namespace Fabric.Mcp.Tools.OneLake.Commands.File;

[HiddenCommand]
public sealed class FileReadCommand(
    ILogger<FileReadCommand> logger,
    IOneLakeService oneLakeService) : GlobalCommand<FileReadOptions>()
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
        LocalRequired = false,
        OpenWorld = false,
        ReadOnly = true,
        Secret = false
    };

    protected override void RegisterOptions(Command command)
    {
        base.RegisterOptions(command);
        command.Options.Add(FabricOptionDefinitions.WorkspaceId.AsOptional());
        command.Options.Add(FabricOptionDefinitions.Workspace.AsOptional());
        command.Options.Add(FabricOptionDefinitions.ItemId.AsOptional());
        command.Options.Add(FabricOptionDefinitions.Item.AsOptional());
        command.Options.Add(FabricOptionDefinitions.FilePath);
        command.Options.Add(FabricOptionDefinitions.DownloadFilePath.AsOptional());
    }

    protected override FileReadOptions BindOptions(ParseResult parseResult)
    {
        var options = base.BindOptions(parseResult);
        var workspaceId = parseResult.GetValueOrDefault<string>(FabricOptionDefinitions.WorkspaceId.Name);
        var workspaceName = parseResult.GetValueOrDefault<string>(FabricOptionDefinitions.Workspace.Name);
        options.WorkspaceId = !string.IsNullOrWhiteSpace(workspaceId)
            ? workspaceId!
            : workspaceName ?? string.Empty;

        var itemId = parseResult.GetValueOrDefault<string>(FabricOptionDefinitions.ItemId.Name);
        var itemName = parseResult.GetValueOrDefault<string>(FabricOptionDefinitions.Item.Name);
        options.ItemId = !string.IsNullOrWhiteSpace(itemId)
            ? itemId!
            : itemName ?? string.Empty;

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
            if (string.IsNullOrWhiteSpace(options.WorkspaceId))
            {
                throw new ArgumentException("Workspace identifier is required. Provide --workspace or --workspace-id.", nameof(options.WorkspaceId));
            }

            if (string.IsNullOrWhiteSpace(options.ItemId))
            {
                throw new ArgumentException("Item identifier is required. Provide --item or --item-id.", nameof(options.ItemId));
            }

            var serviceStartOptions = context.GetService<IOptions<ServiceStartOptions>>();
            var transport = serviceStartOptions.Value.Transport ?? "stdio";
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
                options.WorkspaceId,
                options.ItemId,
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

            context.Response.Status = HttpStatusCode.OK;
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

public sealed class FileReadOptions : GlobalOptions
{
    public string WorkspaceId { get; set; } = string.Empty;
    public string ItemId { get; set; } = string.Empty;
    public string FilePath { get; set; } = string.Empty;
    public string? DownloadFilePath { get; set; }
}
