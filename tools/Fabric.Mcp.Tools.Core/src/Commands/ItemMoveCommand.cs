// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net;
using System.Text.Json.Serialization.Metadata;
using Azure.Identity;
using Fabric.Mcp.Tools.Core.Models;
using Fabric.Mcp.Tools.Core.Options;
using Fabric.Mcp.Tools.Core.Services;
using Fabric.Mcp.Tools.Core.Validation;
using Microsoft.Extensions.Logging;
using Microsoft.Mcp.Core.Commands;
using Microsoft.Mcp.Core.Models.Command;

namespace Fabric.Mcp.Tools.Core.Commands;

[CommandMetadata(
    Id = "693dfc6f-6a98-4a80-bb45-891526b00ee3",
    Name = "move-item",
    Title = "Move Fabric Item",
    Description = """
        Moves one existing Microsoft Fabric item within the same workspace using explicit workspace and item UUIDs.
        Supply target-folder-id for an existing destination folder, or omit it to move to the workspace root.
        Fabric moves child items with their parent; a child cannot be moved on its own (CannotMoveChildOnly).
        Makes exactly one POST with only optional targetFolderId. Does not discover or move a parent automatically,
        look up names, create folders, move across workspaces, copy data, rename, or operate on OneLake or local paths.
        Does not retry automatically, poll, or perform verification requests. This is not a long-running operation.
        Only HTTP 200 confirms movement. Returns the complete metadata collection in service order, including returned
        child items; it may be empty and optional metadata may be absent. Does not infer unrelated dependencies.
        Requires Contributor or higher workspace access; delegated callers require Workspace.ReadWrite.All.
        Users, service principals, and managed identities use the existing configured authentication.
        After an uncertain send, inspect the item separately before another move; never retry blindly.
        If movement is confirmed but metadata is unavailable, do not resubmit it.
        """,
    OperationPlane = ToolOperationPlane.Control,
    Destructive = true,
    Idempotent = true,
    LocalRequired = false,
    OpenWorld = false,
    ReadOnly = false,
    Secret = false)]
public sealed class ItemMoveCommand(
    ILogger<ItemMoveCommand> logger,
    IFabricCoreService fabricCoreService) : FabricCoreCommand<ItemMoveOptions, ItemMoveCommandResult>
{
    private readonly ILogger<ItemMoveCommand> _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    private readonly IFabricCoreService _fabricCoreService = fabricCoreService ?? throw new ArgumentNullException(nameof(fabricCoreService));

    public override JsonTypeInfo<ItemMoveCommandResult> ResultTypeInfo => CoreJsonContext.Default.ItemMoveCommandResult;

    public override void ValidateOptions(ItemMoveOptions options, ValidationResult validationResult)
    {
        base.ValidateOptions(options, validationResult);
        ItemMoveInput.Validate(options.WorkspaceId, options.ItemId, options.TargetFolderId, validationResult);
    }

    public override async Task<CommandResponse> ExecuteAsync(
        CommandContext context, ItemMoveOptions options, CancellationToken cancellationToken)
    {
        try
        {
            var result = await _fabricCoreService.MoveItemAsync(
                options.WorkspaceId, options.ItemId, options.TargetFolderId, cancellationToken);
            SetResult(context, result);
            context.Response.Message = result.Guidance;
        }
        catch (Exception ex)
        {
            _logger.LogError("Error moving a Fabric item ({ExceptionType}).", ex.GetType().Name);
            HandleException(context, ex);
        }
        return context.Response;
    }

    protected override HttpStatusCode GetStatusCode(Exception ex) => ex switch
    {
        FabricItemInputException => HttpStatusCode.BadRequest,
        OperationCanceledException => HttpStatusCode.RequestTimeout,
        _ => base.GetStatusCode(ex)
    };

    protected override void HandleException(CommandContext context, Exception ex)
    {
        base.HandleException(context, ex);
        if (ex is CommandValidationException validation)
        {
            context.Response.Message = GetValidationErrorMessage(validation,
                "Invalid Fabric item move. Provide workspace and item UUIDs, and an optional target folder UUID. Omit the target to use the workspace root. The request was not sent.");
        }
    }

    protected override string GetErrorMessage(Exception ex)
    {
        if (ex is FabricItemInputException)
        {
            return ex.Message + " The item move request was not sent.";
        }
        var message = ex is ItemMoveRequestException { MoveConfirmed: true }
            ? "Fabric confirmed item movement (HTTP 200), but the returned item collection could not be read or validated. Do not resubmit this move; inspect the item separately"
            : GetStatusCode(ex) switch
            {
                HttpStatusCode.BadRequest => "Fabric rejected the item move. Check the workspace/item UUIDs and destination; a child item cannot be moved independently",
                HttpStatusCode.Unauthorized => "Authentication failed. Check the configured Fabric identity",
                HttpStatusCode.Forbidden => "Access denied. The caller needs Contributor or higher workspace access; delegated calls require Workspace.ReadWrite.All",
                HttpStatusCode.NotFound => "The Fabric workspace, item, or destination folder was not found, or the caller does not have access",
                HttpStatusCode.Conflict => "Fabric rejected the item move because of a conflict. No alternate destination or parent item was selected",
                HttpStatusCode.TooManyRequests => "Fabric throttled this item move",
                HttpStatusCode.RequestTimeout or HttpStatusCode.GatewayTimeout => "The item move request was canceled or timed out",
                _ => "Fabric item movement was not confirmed"
            };
        if (ex is ItemMoveRequestException { RequestNotSent: true } or AuthenticationFailedException or ArgumentException)
        {
            message += ". The item move request was not sent";
        }
        else if (ex is not ItemMoveRequestException { MoveConfirmed: true } &&
            ex is not ItemMoveRequestException { ResponseStatusCode: >= HttpStatusCode.BadRequest and < HttpStatusCode.InternalServerError and not HttpStatusCode.RequestTimeout })
        {
            message += ". The item may have moved; inspect it separately before another move. Do not retry blindly";
        }
        message += ". No automatic retry was attempted";
        if (ex is ItemMoveRequestException error)
        {
            if (error.ErrorCode is { } code)
            {
                message += $". Fabric error code: {code}";
            }
            if (error.ErrorMetadataUnavailable)
            {
                message += ". Additional error metadata was unavailable";
            }
            if (error.RetryAfter is { } retryAfter)
            {
                message += retryAfter.Delta is { } delay
                    ? FormattableString.Invariant($". Wait at least {delay.TotalSeconds:0} seconds before another request")
                    : FormattableString.Invariant($". Wait until at least {retryAfter.Date?.UtcDateTime:yyyy-MM-dd HH:mm:ss} UTC before another request");
            }
        }
        return message;
    }
}
