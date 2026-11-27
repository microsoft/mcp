// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net;
using System.Text.Json.Serialization.Metadata;
using Azure.Identity;
using Fabric.Mcp.Tools.Core.Models;
using Fabric.Mcp.Tools.Core.Options;
using Fabric.Mcp.Tools.Core.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Mcp.Core.Commands;
using Microsoft.Mcp.Core.Models.Command;

namespace Fabric.Mcp.Tools.Core.Commands;

[CommandMetadata(
    Id = "db1ef2ca-f276-40ae-9b96-9ce98f8298b2",
    Name = "delete-folder",
    Title = "Delete Fabric Folder",
    Description = """
        Deletes one empty logical Microsoft Fabric workspace folder using explicit workspace and folder UUIDs.
        Requires normal destructive-operation consent. Fabric rejects folders containing any items or nested folders
        with FolderNotEmpty; soft-deleted items can also prevent deletion. Never deletes or moves folder contents.
        Makes exactly one DELETE without a request body, query flags, Get/List or emptiness preflight, name/path lookup,
        descendant enumeration, automatic emptying, retry, or follow-up verification.
        Only HTTP 200 confirms deletion and returns deletionStatus Deleted with the requested workspace and folder IDs,
        not a folder snapshot. This is not an LRO: never follows Location, polls, or retrieves operation results.
        These are workspace organizational folders, not OneLake directories or local paths.
        Requires Contributor or higher workspace access; delegated callers require Workspace.ReadWrite.All.
        Users, service principals, and managed identities use the existing configured authentication.
        No recursive, force, or hard-delete option exists. The API does not document folder recovery or a retention period.
        A 404 or FolderNotFound remains a failure, even after an earlier deletion.
        Canceling the MCP request does not undo deletion. After an uncertain send, inspect the folder separately
        with Get or List Folders before considering another deletion; never retry blindly.
        """,
    OperationPlane = ToolOperationPlane.Control,
    Destructive = true,
    Idempotent = true,
    LocalRequired = false,
    OpenWorld = false,
    ReadOnly = false,
    Secret = false)]
public sealed class FolderDeleteCommand(
    ILogger<FolderDeleteCommand> logger,
    IFabricCoreService fabricCoreService) : FabricCoreCommand<FolderDeleteOptions, FolderDeleteCommandResult>
{
    private readonly ILogger<FolderDeleteCommand> _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    private readonly IFabricCoreService _fabricCoreService = fabricCoreService ?? throw new ArgumentNullException(nameof(fabricCoreService));

    public override JsonTypeInfo<FolderDeleteCommandResult> ResultTypeInfo => CoreJsonContext.Default.FolderDeleteCommandResult;

    public override void ValidateOptions(FolderDeleteOptions options, ValidationResult validationResult)
    {
        base.ValidateOptions(options, validationResult);
        if (!Guid.TryParse(options.WorkspaceId, out var workspaceId) || workspaceId == Guid.Empty)
        {
            validationResult.Errors.Add("--workspace-id must be a nonempty UUID. No DELETE request was sent.");
        }
        if (!Guid.TryParse(options.FolderId, out var folderId) || folderId == Guid.Empty)
        {
            validationResult.Errors.Add("--folder-id must be a nonempty UUID. No DELETE request was sent.");
        }
    }

    public override async Task<CommandResponse> ExecuteAsync(
        CommandContext context, FolderDeleteOptions options, CancellationToken cancellationToken)
    {
        var deletionConfirmed = false;
        try
        {
            var result = await _fabricCoreService.DeleteFolderAsync(options.WorkspaceId, options.FolderId, cancellationToken);
            deletionConfirmed = true;
            SetResult(context, result);
            context.Response.Status = HttpStatusCode.OK;
            context.Response.Message = result.Guidance;
        }
        catch (Exception ex)
        {
            _logger.LogError("Error deleting a Fabric folder ({ExceptionType}).", ex.GetType().Name);
            HandleException(context, deletionConfirmed
                ? new FolderDeleteRequestException(HttpStatusCode.BadGateway, responseStatusCode: HttpStatusCode.OK)
                : ex);
        }
        return context.Response;
    }

    protected override HttpStatusCode GetStatusCode(Exception ex) => ex switch
    {
        OperationCanceledException => HttpStatusCode.RequestTimeout,
        _ => base.GetStatusCode(ex)
    };

    protected override void HandleException(CommandContext context, Exception ex)
    {
        base.HandleException(context, ex);
        if (ex is CommandValidationException validation)
        {
            context.Response.Message = GetValidationErrorMessage(validation,
                "Invalid Fabric folder deletion. Provide explicit workspace and folder UUIDs.") +
                " No DELETE request was sent.";
        }
    }

    protected override string GetErrorMessage(Exception ex)
    {
        if (ex is ArgumentException)
        {
            return "Invalid Fabric folder deletion. Provide explicit nonempty workspace and folder UUIDs. No DELETE request was sent.";
        }
        var message = ex is FolderDeleteRequestException { DeletionConfirmed: true }
            ? "Fabric confirmed folder deletion (HTTP 200), but the local receipt could not be completed. Do not resubmit this confirmed deletion"
            : GetStatusCode(ex) switch
            {
                HttpStatusCode.BadRequest => "Fabric rejected the folder deletion. Check the explicit workspace and folder UUIDs; only empty folders can be deleted",
                HttpStatusCode.Unauthorized => "Authentication failed. Check the configured Fabric identity",
                HttpStatusCode.Forbidden => "Access denied. The caller needs Contributor or higher workspace access; delegated calls require Workspace.ReadWrite.All",
                HttpStatusCode.NotFound => "The Fabric workspace or folder was not found or is inaccessible. This is not confirmed deletion",
                HttpStatusCode.Conflict => "Fabric rejected the folder deletion because of a conflict; only empty folders can be deleted",
                HttpStatusCode.TooManyRequests => "Fabric throttled this folder deletion",
                HttpStatusCode.RequestTimeout or HttpStatusCode.GatewayTimeout => "The folder deletion request was canceled or timed out",
                _ => "Fabric folder deletion was not confirmed"
            };
        if (ex is FolderDeleteRequestException { RequestNotSent: true } or AuthenticationFailedException)
        {
            message += ". No DELETE request was sent";
        }
        else if (ex is not FolderDeleteRequestException { DeletionConfirmed: true } &&
            ex is not FolderDeleteRequestException { ResponseStatusCode: >= HttpStatusCode.BadRequest and < HttpStatusCode.InternalServerError and not HttpStatusCode.RequestTimeout })
        {
            message += ". The folder may have been deleted; inspect it with core_get-folder or core_list-folders separately before another deletion. Do not retry blindly";
        }
        message += ". No automatic retry or change to folder contents was attempted";
        if (ex is FolderDeleteRequestException error)
        {
            if (FabricCoreHttpHelpers.IsSafeErrorCode(error.ErrorCode))
            {
                message += $". Fabric error code: {error.ErrorCode}";
            }
            if (error.ErrorCode == "FolderNotEmpty")
            {
                message += ". Fabric requires an empty folder; items, including soft-deleted items, or nested folders prevent deletion";
            }
            if (error.ErrorMetadataUnavailable)
            {
                message += ". Optional Fabric error metadata was unavailable";
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
