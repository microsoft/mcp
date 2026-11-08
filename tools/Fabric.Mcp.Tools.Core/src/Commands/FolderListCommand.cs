// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net;
using System.Text.Json;
using System.Text.Json.Serialization.Metadata;
using Fabric.Mcp.Tools.Core.Models;
using Fabric.Mcp.Tools.Core.Options;
using Fabric.Mcp.Tools.Core.Services;
using Fabric.Mcp.Tools.Core.Validation;
using Microsoft.Extensions.Logging;
using Microsoft.Mcp.Core.Commands;
using Microsoft.Mcp.Core.Models.Command;

namespace Fabric.Mcp.Tools.Core.Commands;

[CommandMetadata(
    Id = "9b609c7c-5f5e-46da-bcaa-94dceca44c93",
    Name = "list-folders",
    Title = "List Fabric Folders",
    Description = """
        Lists one complete page of logical Microsoft Fabric workspace folders for an explicit workspace UUID.
        Optionally select a root folder UUID and recursive true or false. The API includes nested folders by default.
        Returns folder IDs, display names, workspace IDs, parent folder IDs when available, and any continuation token
        and Retry-After guidance. Parents may be outside the current page; no paths are reconstructed.
        Call again with the unchanged token and the same workspace, root folder, and recursive options for another page.
        Makes exactly one GET, without folder walking, lookups, enrichment, automatic pagination, retries, or polling.
        This is workspace organization metadata, not OneLake directories, local files, or item contents.
        Requires workspace Viewer or higher access; delegated calls require Workspace.Read.All or Workspace.ReadWrite.All.
        """,
    OperationPlane = ToolOperationPlane.Control,
    Destructive = false,
    Idempotent = true,
    LocalRequired = false,
    OpenWorld = false,
    ReadOnly = true,
    Secret = false)]
public sealed class FolderListCommand(
    ILogger<FolderListCommand> logger,
    IFabricCoreService fabricCoreService) : FabricCoreCommand<FolderListOptions, FolderListCommandResult>
{
    private readonly ILogger<FolderListCommand> _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    private readonly IFabricCoreService _fabricCoreService = fabricCoreService ?? throw new ArgumentNullException(nameof(fabricCoreService));

    public override JsonTypeInfo<FolderListCommandResult> ResultTypeInfo => CoreJsonContext.Default.FolderListCommandResult;

    public override void ValidateOptions(FolderListOptions options, ValidationResult validationResult)
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
        if (!FabricContinuationToken.IsValidContinuationToken(options.ContinuationToken))
        {
            validationResult.Errors.Add(FabricContinuationToken.InvalidContinuationTokenMessage);
        }
    }

    public override async Task<CommandResponse> ExecuteAsync(
        CommandContext context, FolderListOptions options, CancellationToken cancellationToken)
    {
        try
        {
            var result = await _fabricCoreService.ListFoldersAsync(
                options.WorkspaceId, options.RootFolderId, options.Recursive, options.ContinuationToken, cancellationToken);
            SetResult(context, result);
        }
        catch (Exception ex)
        {
            _logger.LogError("Error listing Fabric folders ({ExceptionType}).", ex.GetType().Name);
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
            HttpStatusCode.BadRequest => "Check the workspace and root folder UUIDs, recursive Boolean, and unchanged continuation token",
            HttpStatusCode.Unauthorized => "Authentication failed. Use the configured identity with permission to read the Fabric workspace",
            HttpStatusCode.Forbidden => "Access denied. The caller needs workspace Viewer or higher access; delegated calls require Workspace.Read.All or Workspace.ReadWrite.All",
            HttpStatusCode.NotFound => "The Fabric workspace or root folder was not found. Check its ID and the caller's access",
            HttpStatusCode.TooManyRequests => "Fabric throttled the request. Wait before retrying",
            HttpStatusCode.RequestTimeout => "The Fabric folder listing was canceled or timed out",
            HttpStatusCode.GatewayTimeout => "The Fabric folder listing timed out",
            HttpStatusCode.BadGateway => "Fabric returned an invalid folder page. Expected matching workspace metadata and consistent continuation information as JSON up to 1 MiB and 64 nesting levels. No partial page was returned",
            _ => "Unable to list Fabric folders. Retry the request or check service availability"
        };
        if (ex is FolderListRequestException { RetryAfter: { } retryAfter })
        {
            message += retryAfter.Delta is { } delay
                ? FormattableString.Invariant($". Wait at least {delay.TotalSeconds:0} seconds before retrying")
                : FormattableString.Invariant($". Retry after {retryAfter.Date?.UtcDateTime:yyyy-MM-dd HH:mm:ss} UTC");
        }
        return message;
    }
}
