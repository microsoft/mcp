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
    Id = "b9e01f3d-d6e4-4b8a-9c31-5f29bce16b7d",
    Name = "move-folder",
    Title = "Move Fabric Folder",
    Description = """
        Moves one existing logical Microsoft Fabric folder within the same workspace using explicit workspace and folder UUIDs.
        Supply target-folder-id for an existing destination folder, or omit it to move to the workspace root.
        Fabric moves the folder hierarchy, including its contained folders and items, server-side.
        Makes exactly one POST with only optional targetFolderId, without enumerating descendants or moving items individually.
        Does not rename, move across workspaces, create destinations, resolve names or paths, inspect ancestors, check conflicts,
        modify OneLake or local directories, retry automatically, or poll. Fabric enforces hierarchy, depth, and name constraints.
        Only HTTP 200 confirms movement and returns the folder's metadata snapshot, including its parent ID when available.
        Requires Contributor or higher workspace access; delegated callers require Workspace.ReadWrite.All.
        Users, service principals, and managed identities use the existing configured authentication.
        After an uncertain send, inspect the folder separately before another move; never retry blindly.
        If movement is confirmed but metadata is unavailable, do not resubmit it.
        """,
    OperationPlane = ToolOperationPlane.Control,
    Destructive = true,
    Idempotent = true,
    LocalRequired = false,
    OpenWorld = false,
    ReadOnly = false,
    Secret = false)]
public sealed class FolderMoveCommand(
    ILogger<FolderMoveCommand> logger,
    IFabricCoreService fabricCoreService) : FabricCoreCommand<FolderMoveOptions, FolderMoveCommandResult>
{
    private readonly ILogger<FolderMoveCommand> _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    private readonly IFabricCoreService _fabricCoreService = fabricCoreService ?? throw new ArgumentNullException(nameof(fabricCoreService));

    public override JsonTypeInfo<FolderMoveCommandResult> ResultTypeInfo => CoreJsonContext.Default.FolderMoveCommandResult;

    public override void ValidateOptions(FolderMoveOptions options, ValidationResult validationResult)
    {
        base.ValidateOptions(options, validationResult);
        FolderMoveInput.Validate(options.WorkspaceId, options.FolderId, options.TargetFolderId, validationResult);
    }

    public override async Task<CommandResponse> ExecuteAsync(
        CommandContext context, FolderMoveOptions options, CancellationToken cancellationToken)
    {
        try
        {
            var result = await _fabricCoreService.MoveFolderAsync(
                options.WorkspaceId, options.FolderId, options.TargetFolderId, cancellationToken);
            SetResult(context, result);
            context.Response.Message = result.Guidance;
        }
        catch (Exception ex)
        {
            _logger.LogError("Error moving a Fabric folder ({ExceptionType}).", ex.GetType().Name);
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
                "Invalid Fabric folder move. Provide workspace and folder UUIDs, and an optional different target folder UUID. Omit the target to use the workspace root. The request was not sent.");
        }
    }

    protected override string GetErrorMessage(Exception ex)
    {
        if (ex is FabricItemInputException)
        {
            return ex.Message + " The folder move request was not sent.";
        }
        var message = ex is FolderMoveRequestException { MoveConfirmed: true }
            ? "Fabric confirmed folder movement (HTTP 200), but the returned folder metadata could not be read or validated. Do not resubmit this move; inspect the folder separately"
            : GetStatusCode(ex) switch
            {
                HttpStatusCode.BadRequest => "Fabric rejected the folder move. Check the workspace/folder UUIDs, destination, and hierarchy constraints",
                HttpStatusCode.Unauthorized => "Authentication failed. Check the configured Fabric identity",
                HttpStatusCode.Forbidden => "Access denied. The caller needs Contributor or higher workspace access; delegated calls require Workspace.ReadWrite.All",
                HttpStatusCode.NotFound => "The Fabric workspace, folder, or destination was not found, or the caller does not have access",
                HttpStatusCode.Conflict => "Fabric rejected the folder move because of a conflict. No alternate destination or name was selected",
                HttpStatusCode.TooManyRequests => "Fabric throttled this folder move",
                HttpStatusCode.RequestTimeout or HttpStatusCode.GatewayTimeout => "The folder move request was canceled or timed out",
                _ => "Fabric folder movement was not confirmed"
            };
        if (ex is FolderMoveRequestException { RequestNotSent: true } or AuthenticationFailedException or ArgumentException)
        {
            message += ". The folder move request was not sent";
        }
        else if (ex is not FolderMoveRequestException { MoveConfirmed: true } &&
            ex is not FolderMoveRequestException { ResponseStatusCode: >= HttpStatusCode.BadRequest and < HttpStatusCode.InternalServerError and not HttpStatusCode.RequestTimeout })
        {
            message += ". The folder may have moved; inspect it separately before another move. Do not retry blindly";
        }
        message += ". No automatic retry was attempted";
        if (ex is FolderMoveRequestException error)
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
