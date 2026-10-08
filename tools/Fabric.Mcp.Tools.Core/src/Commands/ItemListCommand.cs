// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net;
using System.Text.Json;
using System.Text.Json.Serialization.Metadata;
using Fabric.Mcp.Tools.Core.Models;
using Fabric.Mcp.Tools.Core.Options;
using Fabric.Mcp.Tools.Core.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Mcp.Core.Commands;
using Microsoft.Mcp.Core.Models.Command;

namespace Fabric.Mcp.Tools.Core.Commands;

/// <summary>Lists one page of item metadata within a known Fabric workspace or folder.</summary>
/// <param name="logger">The logger for sanitized diagnostics.</param>
/// <param name="fabricCoreService">The Fabric Core service.</param>
[CommandMetadata(
    Id = "a5c36b36-be01-4f78-8c4e-f714e8d87883",
    Name = "list-items",
    Title = "List Fabric Items",
    Description = """
        Lists one page of Microsoft Fabric item metadata in a known workspace, optionally filtered by item type or root folder.
        Requires a workspace UUID. Use this for a workspace or folder inventory of Lakehouses, Notebooks, Reports, and other Fabric items.
        Includes nested folders by default; set recursive to false for direct items only.
        Returns item IDs, names, types, workspace and folder IDs, other inventory metadata, and available continuation information.
        To retrieve another page, pass the returned continuation token with the same filters.
        This is the Fabric Core metadata API, not OneLake file/blob listing or cross-workspace catalog search.
        Does not read item data, definitions, or default identities, and does not modify resources.
        """,
    OperationPlane = ToolOperationPlane.Control,
    Destructive = false,
    Idempotent = true,
    LocalRequired = false,
    OpenWorld = false,
    ReadOnly = true,
    Secret = false)]
public sealed class ItemListCommand(
    ILogger<ItemListCommand> logger,
    IFabricCoreService fabricCoreService) : FabricCoreCommand<ItemListOptions, ItemListCommandResult>
{
    private readonly ILogger<ItemListCommand> _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    private readonly IFabricCoreService _fabricCoreService = fabricCoreService ?? throw new ArgumentNullException(nameof(fabricCoreService));

    /// <inheritdoc />
    public override JsonTypeInfo<ItemListCommandResult> ResultTypeInfo => CoreJsonContext.Default.ItemListCommandResult;

    /// <inheritdoc />
    public override void ValidateOptions(ItemListOptions options, ValidationResult validationResult)
    {
        base.ValidateOptions(options, validationResult);

        if (!Guid.TryParse(options.WorkspaceId, out var workspaceId) || workspaceId == Guid.Empty)
        {
            validationResult.Errors.Add("--workspace-id must be a nonempty UUID.");
        }

        if (options.RootFolderId is not null &&
            (!Guid.TryParse(options.RootFolderId, out var folderId) || folderId == Guid.Empty))
        {
            validationResult.Errors.Add("--root-folder-id must be a nonempty UUID.");
        }

        if (options.Type is not null && string.IsNullOrWhiteSpace(options.Type))
        {
            validationResult.Errors.Add("--type must not be empty or whitespace.");
        }

        if (options.ContinuationToken is not null && string.IsNullOrWhiteSpace(options.ContinuationToken))
        {
            validationResult.Errors.Add("--continuation-token must not be empty or whitespace.");
        }
    }

    /// <inheritdoc />
    public override async Task<CommandResponse> ExecuteAsync(CommandContext context, ItemListOptions options, CancellationToken cancellationToken)
    {
        try
        {
            var page = await _fabricCoreService.ListItemsAsync(
                options.WorkspaceId,
                options.Type,
                options.Recursive ?? true,
                options.RootFolderId,
                options.ContinuationToken,
                cancellationToken);

            SetResult(context, new(page.Value, page.ContinuationToken, page.ContinuationUri));
        }
        catch (Exception ex)
        {
            _logger.LogError("Error listing Fabric item metadata ({ExceptionType}).", ex.GetType().Name);
            HandleException(context, ex);
        }

        return context.Response;
    }

    protected override HttpStatusCode GetStatusCode(Exception ex) => ex switch
    {
        OperationCanceledException => HttpStatusCode.RequestTimeout,
        JsonException => HttpStatusCode.BadGateway,
        _ => base.GetStatusCode(ex)
    };

    protected override string GetErrorMessage(Exception ex) => GetStatusCode(ex) switch
    {
        HttpStatusCode.BadRequest => "Check the workspace and root folder UUIDs, item type, and continuation token",
        HttpStatusCode.Unauthorized => "Authentication failed. Use the configured identity with permission to read the Fabric workspace",
        HttpStatusCode.Forbidden => "Access denied. The caller needs workspace Viewer access; delegated calls require Workspace.Read.All or Workspace.ReadWrite.All",
        HttpStatusCode.NotFound => "The Fabric workspace or root folder was not found. Check its ID and the caller's access",
        HttpStatusCode.TooManyRequests when ex is FabricItemListThrottledException => ex.Message,
        HttpStatusCode.TooManyRequests => "Fabric throttled the request. Wait before retrying",
        HttpStatusCode.RequestTimeout => "The Fabric item listing was canceled or timed out",
        HttpStatusCode.GatewayTimeout => "The Fabric item listing timed out",
        HttpStatusCode.BadGateway => "Fabric returned an invalid item metadata page",
        _ => "Unable to list Fabric item metadata. Retry the request or check service availability"
    };
}
