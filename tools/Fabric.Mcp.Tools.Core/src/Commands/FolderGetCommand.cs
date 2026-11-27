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

[CommandMetadata(
    Id = "86cfc30e-580c-4c62-9da8-b346177bca68",
    Name = "get-folder",
    Title = "Get Fabric Folder",
    Description = """
        Gets metadata for one logical Microsoft Fabric workspace folder using explicit workspace and folder UUIDs.
        Returns its ID, display name, workspace ID, and parent folder ID when available, plus Retry-After guidance.
        Makes exactly one GET without listing folders, looking up names or ancestors, enumerating children or items,
        reconstructing paths, enriching metadata, retrying, polling, or modifying resources.
        These are logical workspace folders, not OneLake directories, local paths, or item contents.
        Requires workspace Viewer or higher access; delegated calls require Workspace.Read.All or Workspace.ReadWrite.All.
        """,
    OperationPlane = ToolOperationPlane.Control,
    Destructive = false,
    Idempotent = true,
    LocalRequired = false,
    OpenWorld = false,
    ReadOnly = true,
    Secret = false)]
public sealed class FolderGetCommand(
    ILogger<FolderGetCommand> logger,
    IFabricCoreService fabricCoreService) : FabricCoreCommand<FolderGetOptions, FolderGetCommandResult>
{
    private readonly ILogger<FolderGetCommand> _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    private readonly IFabricCoreService _fabricCoreService = fabricCoreService ?? throw new ArgumentNullException(nameof(fabricCoreService));

    public override JsonTypeInfo<FolderGetCommandResult> ResultTypeInfo => CoreJsonContext.Default.FolderGetCommandResult;

    public override void ValidateOptions(FolderGetOptions options, ValidationResult validationResult)
    {
        base.ValidateOptions(options, validationResult);
        if (!Guid.TryParse(options.WorkspaceId, out var workspaceId) || workspaceId == Guid.Empty)
        {
            validationResult.Errors.Add("--workspace-id must be a nonempty UUID.");
        }
        if (!Guid.TryParse(options.FolderId, out var folderId) || folderId == Guid.Empty)
        {
            validationResult.Errors.Add("--folder-id must be a nonempty UUID.");
        }
    }

    public override async Task<CommandResponse> ExecuteAsync(
        CommandContext context, FolderGetOptions options, CancellationToken cancellationToken)
    {
        try
        {
            var result = await _fabricCoreService.GetFolderAsync(options.WorkspaceId, options.FolderId, cancellationToken);
            SetResult(context, result);
        }
        catch (Exception ex)
        {
            _logger.LogError("Error retrieving Fabric folder metadata ({ExceptionType}).", ex.GetType().Name);
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

    protected override string GetErrorMessage(Exception ex)
    {
        var message = GetStatusCode(ex) switch
        {
            HttpStatusCode.BadRequest => "Provide valid workspace and folder UUIDs",
            HttpStatusCode.Unauthorized => "Authentication failed. Use the configured identity with permission to read the Fabric workspace",
            HttpStatusCode.Forbidden => "Access denied. The caller needs workspace Viewer or higher access; delegated calls require Workspace.Read.All or Workspace.ReadWrite.All",
            HttpStatusCode.NotFound => "The Fabric workspace or folder was not found. Check its ID and the caller's access",
            HttpStatusCode.TooManyRequests => "Fabric throttled the request. Wait before retrying",
            HttpStatusCode.RequestTimeout => "The Fabric folder request was canceled or timed out",
            HttpStatusCode.GatewayTimeout => "The Fabric folder request timed out",
            HttpStatusCode.BadGateway => "Fabric returned invalid folder metadata. Expected matching workspace and folder IDs and a display name as JSON up to 1 MiB and 64 nesting levels",
            _ => "Unable to retrieve Fabric folder metadata. Retry the request or check service availability"
        };
        if (ex is FolderGetRequestException failure)
        {
            if (FabricCoreHttpHelpers.IsSafeErrorCode(failure.ErrorCode))
            {
                message += $". Fabric error code: {failure.ErrorCode}";
            }
            if (failure.ErrorMetadataUnavailable)
            {
                message += ". Additional error metadata was unavailable";
            }
            if (failure.RetryAfter is { } retryAfter)
            {
                message += retryAfter.Delta is { } delay
                    ? FormattableString.Invariant($". Wait at least {delay.TotalSeconds:0} seconds before retrying")
                    : FormattableString.Invariant($". Retry after {retryAfter.Date?.UtcDateTime:yyyy-MM-dd HH:mm:ss} UTC");
            }
        }
        return message;
    }
}
