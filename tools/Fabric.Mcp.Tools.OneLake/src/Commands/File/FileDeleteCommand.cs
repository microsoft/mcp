// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Core.Commands;
using Azure.Mcp.Core.Extensions;
using Azure.Mcp.Core.Models.Option;
using Azure.Mcp.Core.Options;
using Fabric.Mcp.Tools.OneLake.Models;
using Fabric.Mcp.Tools.OneLake.Options;
using Fabric.Mcp.Tools.OneLake.Services;
using Microsoft.Extensions.Logging;
using System.CommandLine;
using System.CommandLine.Parsing;

namespace Fabric.Mcp.Tools.OneLake.Commands.File;

public sealed class FileDeleteCommand(
    ILogger<FileDeleteCommand> logger,
    IOneLakeService oneLakeService) : GlobalCommand<FileDeleteOptions>()
{
    private readonly ILogger<FileDeleteCommand> _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    private readonly IOneLakeService _oneLakeService = oneLakeService ?? throw new ArgumentNullException(nameof(oneLakeService));

    public override string Name => "file-delete";
    public override string Title => "Delete OneLake File";
    public override string Description => "Delete a file from OneLake storage.";

    public override ToolMetadata Metadata => new()
    {
        Destructive = true,
        Idempotent = true,
        LocalRequired = false,
        OpenWorld = false,
        ReadOnly = false,
        Secret = false
    };

    protected override void RegisterOptions(Command command)
    {
        base.RegisterOptions(command);
        command.Options.Add(FabricOptionDefinitions.WorkspaceId);
        command.Options.Add(FabricOptionDefinitions.ItemId);
        command.Options.Add(FabricOptionDefinitions.FilePath);
    }

    protected override FileDeleteOptions BindOptions(ParseResult parseResult)
    {
        var options = base.BindOptions(parseResult);
        options.WorkspaceId = parseResult.GetValueOrDefault<string>(FabricOptionDefinitions.WorkspaceId.Name) ?? string.Empty;
        options.ItemId = parseResult.GetValueOrDefault<string>(FabricOptionDefinitions.ItemId.Name) ?? string.Empty;
        options.FilePath = parseResult.GetValueOrDefault<string>(FabricOptionDefinitions.FilePath.Name) ?? string.Empty;
        return options;
    }

    public override async Task<CommandResponse> ExecuteAsync(CommandContext context, ParseResult parseResult)
    {
        var options = BindOptions(parseResult);
        try
        {
            await _oneLakeService.DeleteFileAsync(
                options.WorkspaceId,
                options.ItemId,
                options.FilePath,
                CancellationToken.None);

            var result = new FileDeleteCommandResult(options.FilePath, "File deleted successfully");
            context.Response.Results = ResponseResult.Create(result, OneLakeJsonContext.Default.FileDeleteCommandResult);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting file {FilePath} from workspace {WorkspaceId}, item {ItemId}. Options: {@Options}", 
                options.FilePath, options.WorkspaceId, options.ItemId, options);
            HandleException(context, ex);
        }
        
        return context.Response;
    }

    public sealed record FileDeleteCommandResult(
        string FilePath,
        string Message);
}

public sealed class FileDeleteOptions : GlobalOptions
{
    public string WorkspaceId { get; set; } = string.Empty;
    public string ItemId { get; set; } = string.Empty;
    public string FilePath { get; set; } = string.Empty;
}