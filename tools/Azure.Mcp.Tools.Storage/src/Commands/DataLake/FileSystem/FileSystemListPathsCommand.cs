// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Core.Commands;
using Azure.Mcp.Core.Extensions;
using Azure.Mcp.Tools.Storage.Models;
using Azure.Mcp.Tools.Storage.Options;
using Azure.Mcp.Tools.Storage.Options.DataLake.FileSystem;
using Azure.Mcp.Tools.Storage.Services;
using Microsoft.Extensions.Logging;

namespace Azure.Mcp.Tools.Storage.Commands.DataLake.FileSystem;

public sealed class FileSystemListPathsCommand(ILogger<FileSystemListPathsCommand> logger) : BaseFileSystemCommand<ListPathsOptions>
{
    private const string CommandTitle = "List Data Lake File System Paths";
    private readonly ILogger<FileSystemListPathsCommand> _logger = logger;

    public override string Name => "list-paths";

    public override string Description =>
        """
        List paths in a Data Lake file system. This command retrieves and displays paths (files and directories)
        available in the specified Data Lake file system within the storage account. Results include path names,
        types (file or directory), and metadata, returned as a JSON array.
        """;

    public override string Title => CommandTitle;

    public override ToolMetadata Metadata => new()
    {
        Destructive = false,
        Idempotent = true,
        OpenWorld = true,
        ReadOnly = true,
        LocalRequired = false,
        Secret = false
    };

    protected override void RegisterOptions(Command command)
    {
        base.RegisterOptions(command);
        command.Options.Add(StorageOptionDefinitions.FilterPath);
        command.Options.Add(StorageOptionDefinitions.Recursive);
    }

    protected override ListPathsOptions BindOptions(ParseResult parseResult)
    {
        var options = base.BindOptions(parseResult);
        options.FilterPath = parseResult.GetValueOrDefault(StorageOptionDefinitions.FilterPath);
        options.Recursive = parseResult.GetValueOrDefault(StorageOptionDefinitions.Recursive);
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
            var paths = await storageService.ListDataLakePaths(
                options.Account!,
                options.FileSystem!,
                options.Recursive,
                options.Subscription!,
                options.FilterPath,
                options.Tenant,
                options.RetryPolicy);

            context.Response.Results = ResponseResult.Create(
                new FileSystemListPathsCommandResult(paths ?? []),
                StorageJsonContext.Default.FileSystemListPathsCommandResult);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error listing Data Lake file system paths. Account: {Account}, FileSystem: {FileSystem}.", options.Account, options.FileSystem);
            HandleException(context, ex);
        }

        return context.Response;
    }

    internal record FileSystemListPathsCommandResult(List<DataLakePathInfo> Paths);
}
