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
    Id = "178a1c7d-7799-4ade-b5cd-f386cdb9aa64",
    Name = "update-folder",
    Title = "Update Fabric Folder",
    Description = """
        Renames one existing logical Microsoft Fabric workspace folder using explicit workspace and folder UUIDs.
        Requires display-name, following Fabric folder naming rules; the value is never trimmed or sanitized.
        Makes exactly one PATCH with only displayName. Does not move the folder, modify items or permissions,
        create OneLake or local directories, resolve names, check duplicates, fetch or merge existing metadata,
        retry automatically, or poll. A duplicate-name error remains a failure.
        HTTP 200 confirms the update and returns the folder's metadata snapshot, including its parent ID when available.
        Requires Contributor or higher workspace access; delegated callers require Workspace.ReadWrite.All.
        Users, service principals, and managed identities use the existing configured authentication.
        After an uncertain send, inspect the folder separately before another update; never retry blindly.
        If the update is confirmed but metadata is unavailable, do not resubmit it.
        """,
    OperationPlane = ToolOperationPlane.Control,
    Destructive = true,
    Idempotent = true,
    LocalRequired = false,
    OpenWorld = false,
    ReadOnly = false,
    Secret = false)]
public sealed class FolderUpdateCommand(
    ILogger<FolderUpdateCommand> logger,
    IFabricCoreService fabricCoreService) : FabricCoreCommand<FolderUpdateOptions, FolderUpdateCommandResult>
{
    private readonly ILogger<FolderUpdateCommand> _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    private readonly IFabricCoreService _fabricCoreService = fabricCoreService ?? throw new ArgumentNullException(nameof(fabricCoreService));

    public override JsonTypeInfo<FolderUpdateCommandResult> ResultTypeInfo => CoreJsonContext.Default.FolderUpdateCommandResult;

    public override void ValidateOptions(FolderUpdateOptions options, ValidationResult validationResult)
    {
        base.ValidateOptions(options, validationResult);
        FolderUpdateInput.Validate(options.WorkspaceId, options.FolderId, options.DisplayName, validationResult);
    }

    public override async Task<CommandResponse> ExecuteAsync(
        CommandContext context, FolderUpdateOptions options, CancellationToken cancellationToken)
    {
        try
        {
            var result = await _fabricCoreService.UpdateFolderAsync(
                options.WorkspaceId, options.FolderId, options.DisplayName, cancellationToken);
            SetResult(context, result);
            context.Response.Message = result.Guidance;
        }
        catch (Exception ex)
        {
            _logger.LogError("Error updating a Fabric folder ({ExceptionType}).", ex.GetType().Name);
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
                "Invalid Fabric folder update. Provide workspace and folder UUIDs and a valid replacement display name. The request was not sent.");
        }
    }

    protected override string GetErrorMessage(Exception ex)
    {
        if (ex is FabricItemInputException)
        {
            return ex.Message + " The folder update request was not sent.";
        }
        var message = ex is FolderUpdateRequestException { UpdateConfirmed: true }
            ? "Fabric confirmed folder update (HTTP 200), but the returned folder metadata could not be read or validated. Do not resubmit this update; inspect the folder separately"
            : GetStatusCode(ex) switch
            {
                HttpStatusCode.BadRequest => "Fabric rejected the folder update. Check the workspace/folder UUIDs and Fabric folder naming rules",
                HttpStatusCode.Unauthorized => "Authentication failed. Check the configured Fabric identity",
                HttpStatusCode.Forbidden => "Access denied. The caller needs Contributor or higher workspace access; delegated calls require Workspace.ReadWrite.All",
                HttpStatusCode.NotFound => "The Fabric workspace or folder was not found, or the caller does not have access",
                HttpStatusCode.Conflict => "Fabric rejected the folder update because of a conflict. A duplicate name remains a failure; no replacement name was generated",
                HttpStatusCode.TooManyRequests => "Fabric throttled this folder update",
                HttpStatusCode.RequestTimeout or HttpStatusCode.GatewayTimeout => "The folder update request was canceled or timed out",
                _ => "Fabric folder update was not confirmed"
            };
        if (ex is FolderUpdateRequestException { RequestNotSent: true } or AuthenticationFailedException or ArgumentException)
        {
            message += ". The folder update request was not sent";
        }
        else if (ex is not FolderUpdateRequestException { UpdateConfirmed: true } &&
            ex is not FolderUpdateRequestException { ResponseStatusCode: >= HttpStatusCode.BadRequest and < HttpStatusCode.InternalServerError and not HttpStatusCode.RequestTimeout })
        {
            message += ". The update may have been applied; inspect the folder separately before another update. Do not retry blindly";
        }
        message += ". No automatic retry was attempted";
        if (ex is FolderUpdateRequestException error)
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
