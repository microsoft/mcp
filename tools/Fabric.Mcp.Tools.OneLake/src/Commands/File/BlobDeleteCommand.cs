// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Fabric.Mcp.Tools.OneLake.Models;
using Fabric.Mcp.Tools.OneLake.Options;
using Fabric.Mcp.Tools.OneLake.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Mcp.Core.Commands;
using Microsoft.Mcp.Core.Models;
using Microsoft.Mcp.Core.Models.Command;
using Microsoft.Mcp.Core.Options;

namespace Fabric.Mcp.Tools.OneLake.Commands.File;

[HiddenCommand]
[CommandMetadata(
    Id = "48561b8d-6f19-45ae-86fa-9feeb8f75e8e",
    Name = "delete",
    Title = "Delete OneLake Blob",
    Description = "Delete a blob from OneLake using the blob endpoint while returning request metadata for auditing.",
    Destructive = true,
    Idempotent = false,
    LocalRequired = false,
    OpenWorld = false,
    ReadOnly = false,
    Secret = false)]
public sealed class BlobDeleteCommand(ILogger<BlobDeleteCommand> logger, IOneLakeService oneLakeService)
    : AuthenticatedCommand<BlobDeleteOptions, BlobDeleteCommand.BlobDeleteCommandResult>
{
    private readonly ILogger<BlobDeleteCommand> _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    private readonly IOneLakeService _oneLakeService = oneLakeService ?? throw new ArgumentNullException(nameof(oneLakeService));

    public override void ValidateOptions(BlobDeleteOptions options, ValidationResult validationResult)
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

    public override async Task<CommandResponse> ExecuteAsync(CommandContext context, BlobDeleteOptions options, CancellationToken cancellationToken)
    {
        try
        {
            var workspaceIdentifier = !string.IsNullOrWhiteSpace(options.WorkspaceId)
                ? options.WorkspaceId
                : options.Workspace!;

            var itemIdentifier = !string.IsNullOrWhiteSpace(options.ItemId)
                ? options.ItemId
                : options.Item!;

            var result = await _oneLakeService.DeleteBlobAsync(
                workspaceIdentifier,
                itemIdentifier,
                options.FilePath,
                cancellationToken);

            var commandResult = new BlobDeleteCommandResult(
                result,
                "Blob deleted successfully.");

            context.Response.Results = ResponseResult.Create(commandResult, OneLakeJsonContext.Default.BlobDeleteCommandResult);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting blob {BlobPath} in workspace {WorkspaceId}, item {ItemId}.",
                options.FilePath, options.WorkspaceId, options.ItemId);
            HandleException(context, ex);
        }

        return context.Response;
    }

    public sealed record BlobDeleteCommandResult(BlobDeleteResult Result, string Message);
}

public sealed class BlobDeleteOptions
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
