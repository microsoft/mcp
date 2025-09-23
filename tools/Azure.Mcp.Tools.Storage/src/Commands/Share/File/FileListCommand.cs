// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json.Serialization;
using Azure.Mcp.Core.Commands;
using Azure.Mcp.Core.Extensions;
using Azure.Mcp.Tools.Storage.Models;
using Azure.Mcp.Tools.Storage.Options;
using Azure.Mcp.Tools.Storage.Options.Share.File;
using Azure.Mcp.Tools.Storage.Services;
using Microsoft.Extensions.Logging;

namespace Azure.Mcp.Tools.Storage.Commands.Share.File;

public sealed class FileListCommand(ILogger<FileListCommand> logger) : BaseFileCommand<FileListOptions>()
{
    private const string CommandTitle = "List Storage Share Files and Directories";
    private readonly ILogger<FileListCommand> _logger = logger;

    public override string Name => "list";

    public override string Description =>
        $"""
        Lists files and directories within a file share directory. This tool recursively lists all items in a specified
        file share directory, including files, subdirectories, and their properties. Files and directories may be filtered
        by a prefix. Returns file listing as JSON.
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
        command.Options.Add(StorageOptionDefinitions.Prefix);
    }

    protected override FileListOptions BindOptions(ParseResult parseResult)
    {
        var options = base.BindOptions(parseResult);
        options.Prefix = parseResult.GetValueOrDefault(StorageOptionDefinitions.Prefix);
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
            var filesAndDirectories = await storageService.ListFilesAndDirectories(
                options.Account!,
                options.Share!,
                options.DirectoryPath!,
                options.Prefix,
                options.Subscription!,
                options.Tenant,
                options.RetryPolicy);

            context.Response.Results = ResponseResult.Create(new(filesAndDirectories ?? []), StorageJsonContext.Default.FileListCommandResult);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error listing storage share files and directories. Account: {Account}, Share: {Share}, Directory: {Directory}, Prefix: {Prefix}.", options.Account, options.Share, options.DirectoryPath, options.Prefix);
            HandleException(context, ex);
        }

        return context.Response;
    }

    internal record FileListCommandResult([property: JsonPropertyName("files")] List<FileShareItemInfo> Files);
}
