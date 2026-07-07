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
    Id = "86991cd6-75fa-4870-9d99-f986ba9f5f73",
    Name = "delete_directory",
    Title = "Delete OneLake Directory",
    Description = "Deletes a directory from OneLake storage. Use this when the user wants to remove a folder. Use recursive flag to delete non-empty directories.",
    Destructive = true,
    Idempotent = true,
    LocalRequired = false,
    OpenWorld = false,
    ReadOnly = false,
    Secret = false)]
public sealed class DirectoryDeleteCommand(
    ILogger<DirectoryDeleteCommand> logger,
    IOneLakeService oneLakeService) : AuthenticatedCommand<DirectoryDeleteOptions, DirectoryDeleteCommand.DirectoryDeleteCommandResult>
{
    private readonly ILogger<DirectoryDeleteCommand> _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    private readonly IOneLakeService _oneLakeService = oneLakeService ?? throw new ArgumentNullException(nameof(oneLakeService));

    public override void ValidateOptions(DirectoryDeleteOptions options, ValidationResult validationResult)
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

    public override async Task<CommandResponse> ExecuteAsync(CommandContext context, DirectoryDeleteOptions options, CancellationToken cancellationToken)
    {
        try
        {
            var workspaceIdentifier = !string.IsNullOrWhiteSpace(options.WorkspaceId)
                ? options.WorkspaceId
                : options.Workspace!;

            var itemIdentifier = !string.IsNullOrWhiteSpace(options.ItemId)
                ? options.ItemId
                : options.Item!;

            await _oneLakeService.DeleteDirectoryAsync(
                workspaceIdentifier,
                itemIdentifier,
                options.DirectoryPath,
                options.Recursive,
                cancellationToken);

            var message = options.Recursive
                ? "Directory and all contents deleted successfully"
                : "Directory deleted successfully";
            var result = new DirectoryDeleteCommandResult(options.DirectoryPath, message);
            context.Response.Results = ResponseResult.Create(result, OneLakeJsonContext.Default.DirectoryDeleteCommandResult);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting directory {DirectoryPath} from workspace {WorkspaceId}, item {ItemId}.",
                options.DirectoryPath, options.WorkspaceId, options.ItemId);
            HandleException(context, ex);
        }

        return context.Response;
    }

    public sealed record DirectoryDeleteCommandResult(
        string DirectoryPath,
        string Message);
}

public sealed class DirectoryDeleteOptions
{
    [Option(Description = "The ID of the Microsoft Fabric workspace.")]
    public string? WorkspaceId { get; set; }

    [Option(Description = "The name or ID of the Microsoft Fabric workspace.")]
    public string? Workspace { get; set; }

    [Option(Description = "The ID of the Fabric item.")]
    public string? ItemId { get; set; }

    [Option(Description = "The name or ID of the Fabric item. When using friendly names, MUST include the item type suffix (e.g., 'ItemName.Lakehouse', 'ItemName.Warehouse').")]
    public string? Item { get; set; }

    [Option(Description = "The path to the directory in OneLake.")]
    public required string DirectoryPath { get; set; }

    [Option(Description = "Whether to perform the operation recursively.")]
    public bool Recursive { get; set; }
}
