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

public sealed class FileReadCommand(
    ILogger<FileReadCommand> logger,
    IOneLakeService oneLakeService) : GlobalCommand<FileReadOptions>()
{
    private readonly ILogger<FileReadCommand> _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    private readonly IOneLakeService _oneLakeService = oneLakeService ?? throw new ArgumentNullException(nameof(oneLakeService));

    public override string Name => "file-read";
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
        command.Options.Add(FabricOptionDefinitions.WorkspaceId);
        command.Options.Add(FabricOptionDefinitions.ItemId);
        command.Options.Add(FabricOptionDefinitions.FilePath);
    }

    protected override FileReadOptions BindOptions(ParseResult parseResult)
    {
        var options = base.BindOptions(parseResult);
        options.WorkspaceId = parseResult.GetValueOrDefault<string>(FabricOptionDefinitions.WorkspaceIdName) ?? string.Empty;
        options.ItemId = parseResult.GetValueOrDefault<string>(FabricOptionDefinitions.ItemIdName) ?? string.Empty;
        options.FilePath = parseResult.GetValueOrDefault<string>(FabricOptionDefinitions.FilePathName) ?? string.Empty;
        return options;
    }

    public override async Task<CommandResponse> ExecuteAsync(CommandContext context, ParseResult parseResult)
    {
        var options = BindOptions(parseResult);
        try
        {
            using var stream = await _oneLakeService.ReadFileAsync(
                options.WorkspaceId,
                options.ItemId,
                options.FilePath,
                CancellationToken.None);

            using var reader = new StreamReader(stream);
            var content = await reader.ReadToEndAsync(CancellationToken.None);

            var result = new FileReadCommandResult(options.FilePath, content);
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
        string Content);
}

public sealed class FileReadOptions : GlobalOptions
{
    public string WorkspaceId { get; set; } = string.Empty;
    public string ItemId { get; set; } = string.Empty;
    public string FilePath { get; set; } = string.Empty;
}