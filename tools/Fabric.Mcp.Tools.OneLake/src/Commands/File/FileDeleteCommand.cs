// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Fabric.Mcp.Tools.OneLake.Models;
using Fabric.Mcp.Tools.OneLake.Options;
using Fabric.Mcp.Tools.OneLake.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Mcp.Core.Commands;
using Microsoft.Mcp.Core.Models.Command;
using Microsoft.Mcp.Core.Options;

namespace Fabric.Mcp.Tools.OneLake.Commands.File;

[CommandMetadata(
    Id = "0aa3f887-0085-4141-8e34-f0cf1ed44f71",
    Name = "delete_file",
    Title = "Delete OneLake File",
    Description = "Deletes a file from OneLake storage. Use this when the user wants to remove a specific file. Permanently removes the file at the specified path.",
    Destructive = true,
    Idempotent = true,
    LocalRequired = false,
    OpenWorld = false,
    ReadOnly = false,
    Secret = false)]
public sealed class FileDeleteCommand(ILogger<FileDeleteCommand> logger, IOneLakeService oneLakeService)
    : AuthenticatedCommand<FileDeleteOptions, FileDeleteCommand.FileDeleteCommandResult>
{
    private readonly ILogger<FileDeleteCommand> _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    private readonly IOneLakeService _oneLakeService = oneLakeService ?? throw new ArgumentNullException(nameof(oneLakeService));

    public override void ValidateOptions(FileDeleteOptions options, ValidationResult validationResult)
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

    public override async Task<CommandResponse> ExecuteAsync(CommandContext context, FileDeleteOptions options, CancellationToken cancellationToken)
    {
        try
        {
            var workspaceIdentifier = !string.IsNullOrWhiteSpace(options.WorkspaceId)
                ? options.WorkspaceId
                : options.Workspace!;

            var itemIdentifier = !string.IsNullOrWhiteSpace(options.ItemId)
                ? options.ItemId
                : options.Item!;

            await _oneLakeService.DeleteFileAsync(
                workspaceIdentifier,
                itemIdentifier,
                options.FilePath,
                cancellationToken);

            var result = new FileDeleteCommandResult(options.FilePath, "File deleted successfully");
            context.Response.Results = ResponseResult.Create(result, OneLakeJsonContext.Default.FileDeleteCommandResult);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting file {FilePath} from workspace {WorkspaceId}, item {ItemId}.",
                options.FilePath, options.WorkspaceId, options.ItemId);
            HandleException(context, ex);
        }

        return context.Response;
    }

    public sealed record FileDeleteCommandResult(string FilePath, string Message);
}

public sealed class FileDeleteOptions
{
    [Option(Description = OneLakeOptionDescriptions.WorkspaceId)]
    public string? WorkspaceId { get; set; }

    [Option(Description = OneLakeOptionDescriptions.Workspace)]
    public string? Workspace { get; set; }

    [Option(Description = OneLakeOptionDescriptions.ItemId)]
    public string? ItemId { get; set; }

    [Option(Description = OneLakeOptionDescriptions.Item)]
    public string? Item { get; set; }

    [Option(Description = OneLakeOptionDescriptions.FilePath)]
    public required string FilePath { get; set; }
}
