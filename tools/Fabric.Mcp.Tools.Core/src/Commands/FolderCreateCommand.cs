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
    Id = "d6cd4bca-ef5f-45bf-91e7-9e06868362af",
    Name = "create-folder",
    Title = "Create Fabric Folder",
    Description = """
        Creates one logical Microsoft Fabric workspace folder for an explicit workspace UUID and display name.
        Optionally supply an existing parent-folder-id UUID; omission or null creates at the workspace root.
        Names must meet Fabric folder naming rules and are never trimmed or sanitized.
        This is workspace organization, not a OneLake directory or local filesystem path.
        Makes exactly one POST, without parent/name lookup, duplicate checks, recursive creation, get-or-create,
        automatic retry, follow-up reads, or operation polling. A duplicate-name error remains a failure.
        HTTP 201 confirms creation and returns folder ID, display name, workspace ID, and parent ID when available.
        Requires Contributor or higher workspace access; delegated callers require Workspace.ReadWrite.All.
        Users, service principals, and managed identities use the existing configured authentication.
        After an uncertain send, verify the workspace separately before any new creation; never resubmit blindly.
        If creation is confirmed but metadata is unavailable, do not repeat the creation request.
        """,
    OperationPlane = ToolOperationPlane.Control,
    Destructive = false,
    Idempotent = false,
    LocalRequired = false,
    OpenWorld = false,
    ReadOnly = false,
    Secret = false)]
public sealed class FolderCreateCommand(
    ILogger<FolderCreateCommand> logger,
    IFabricCoreService fabricCoreService) : FabricCoreCommand<FolderCreateOptions, FolderCreateCommandResult>
{
    private readonly ILogger<FolderCreateCommand> _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    private readonly IFabricCoreService _fabricCoreService = fabricCoreService ?? throw new ArgumentNullException(nameof(fabricCoreService));

    public override JsonTypeInfo<FolderCreateCommandResult> ResultTypeInfo => CoreJsonContext.Default.FolderCreateCommandResult;

    public override void ValidateOptions(FolderCreateOptions options, ValidationResult validationResult)
    {
        base.ValidateOptions(options, validationResult);
        FolderCreateInput.Validate(options.WorkspaceId, options.DisplayName, options.ParentFolderId, validationResult);
    }

    public override async Task<CommandResponse> ExecuteAsync(
        CommandContext context, FolderCreateOptions options, CancellationToken cancellationToken)
    {
        try
        {
            var result = await _fabricCoreService.CreateFolderAsync(
                options.WorkspaceId, options.DisplayName, options.ParentFolderId, cancellationToken);
            SetResult(context, result);
            context.Response.Status = HttpStatusCode.Created;
            context.Response.Message = result.Guidance;
        }
        catch (Exception ex)
        {
            _logger.LogError("Error creating a Fabric folder ({ExceptionType}).", ex.GetType().Name);
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
                "Invalid Fabric folder creation. Provide a workspace UUID, a valid folder display name, and optionally a parent folder UUID. The request was not sent.");
        }
    }

    protected override string GetErrorMessage(Exception ex)
    {
        if (ex is FabricItemInputException)
        {
            return ex.Message + " The folder creation request was not sent.";
        }
        var message = ex is FolderCreateRequestException { CreationConfirmed: true }
            ? "Fabric confirmed folder creation (HTTP 201), but the returned folder metadata could not be read or validated. Do not resubmit this creation; inspect the workspace separately"
            : GetStatusCode(ex) switch
            {
                HttpStatusCode.BadRequest => "Check the workspace/parent UUIDs and Fabric folder naming rules",
                HttpStatusCode.Unauthorized => "Authentication failed. Check the configured Fabric identity",
                HttpStatusCode.Forbidden => "Access denied. The caller needs Contributor or higher workspace access; delegated calls require Workspace.ReadWrite.All",
                HttpStatusCode.NotFound => "The Fabric workspace or parent folder was not found, or the caller does not have access",
                HttpStatusCode.Conflict => "Fabric rejected folder creation because of a conflict. An existing folder is not returned as a successful creation",
                HttpStatusCode.TooManyRequests => "Fabric throttled this folder creation",
                HttpStatusCode.RequestTimeout or HttpStatusCode.GatewayTimeout => "The folder creation request was canceled or timed out",
                _ => "Fabric folder creation was not confirmed"
            };
        if (ex is FolderCreateRequestException { RequestNotSent: true } or AuthenticationFailedException or ArgumentException)
        {
            message += ". The folder creation request was not sent";
        }
        else if (ex is not FolderCreateRequestException { CreationConfirmed: true })
        {
            message += ". A folder may already have been created; verify the workspace separately before any new creation. Do not retry blindly";
        }
        message += ". No automatic retry was attempted";
        if (ex is FolderCreateRequestException error)
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
