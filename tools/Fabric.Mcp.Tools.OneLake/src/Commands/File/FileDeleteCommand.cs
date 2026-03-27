// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net;
using Azure.Mcp.Core.Extensions;
using Fabric.Mcp.Tools.OneLake.Models;
using Fabric.Mcp.Tools.OneLake.Options;
using Fabric.Mcp.Tools.OneLake.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Mcp.Core.Commands;
using Microsoft.Mcp.Core.Extensions;

namespace Fabric.Mcp.Tools.OneLake.Commands.File;

public sealed class FileDeleteCommand(
    ILogger<FileDeleteCommand> logger,
    IOneLakeService oneLakeService) : BaseItemCommand<FileDeleteOptions>()
{
    private readonly ILogger<FileDeleteCommand> _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    private readonly IOneLakeService _oneLakeService = oneLakeService ?? throw new ArgumentNullException(nameof(oneLakeService));

    public override string Id => "0aa3f887-0085-4141-8e34-f0cf1ed44f71";
    public override string Name => "delete-file";
    public override string Title => "Delete OneLake File";
    public override string Description => "Deletes a file from OneLake storage. Use this when the user wants to remove a specific file. Permanently removes the file at the specified path.";

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
        command.Options.Add(FabricOptionDefinitions.FilePath);
    }

    protected override FileDeleteOptions BindOptions(ParseResult parseResult)
    {
        var options = base.BindOptions(parseResult);
        options.FilePath = parseResult.GetValueOrDefault<string>(FabricOptionDefinitions.FilePath.Name) ?? string.Empty;
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
            await _oneLakeService.DeleteFileAsync(
                options.WorkspaceId!,
                options.ItemId!,
                options.FilePath,
                cancellationToken);

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

    protected override HttpStatusCode GetStatusCode(Exception ex) => ex switch
    {
        ArgumentException => HttpStatusCode.BadRequest,
        _ => base.GetStatusCode(ex)
    };
}

public sealed class FileDeleteOptions : BaseItemOptions
{
    public string FilePath { get; set; } = string.Empty;
}
