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
    Id = "39eda9bb-2cec-4820-87f3-fde0cbe52b7a",
    Name = "delete-workspace-role-assignment",
    Title = "Delete Fabric Workspace Role Assignment",
    Description = """
        Deletes one existing Microsoft Fabric workspace role assignment using explicit workspace-id and workspace-role-assignment-id.
        Assignment and principal IDs are independent; supply the existing assignment UUID, not a principal name or an ID to translate.
        Removes only that assignment, not the principal, group membership, workspace, or other assignments.
        Does not establish that the principal has no remaining effective access.
        Requires workspace Admin access; delegated calls require Workspace.ReadWrite.All. The last admin's assignment cannot be deleted.
        Users, service principals, and managed identities use the configured Fabric authentication; Fabric enforces permissions.
        Uses normal destructive-operation confirmation. Makes exactly one DELETE without a request body or query flags.
        No List/Get preflight, directory/Graph calls, name resolution, identity translation, membership expansion,
        cascade, bulk removal, fallback, automatic retry, or follow-up verification is performed.
        Only HTTP 200 confirms deletion and returns deletionStatus Deleted with the requested workspace and assignment IDs,
        not an assignment snapshot or an effective-access assessment. A 404 remains a failure, even after an earlier deletion.
        This is not a documented LRO: never follows Location, polls, or retrieves operation results.
        Canceling the MCP request does not undo deletion. After an uncertain send inspect the assignment separately;
        never retry blindly or resubmit confirmed deletion. Retry-After timing is not permission to replay the deletion.
        """,
    OperationPlane = ToolOperationPlane.Control,
    Destructive = true,
    Idempotent = true,
    LocalRequired = false,
    OpenWorld = false,
    ReadOnly = false,
    Secret = false)]
public sealed class WorkspaceRoleAssignmentDeleteCommand(
    ILogger<WorkspaceRoleAssignmentDeleteCommand> logger,
    IFabricCoreService fabricCoreService) : FabricCoreCommand<WorkspaceRoleAssignmentDeleteOptions, WorkspaceRoleAssignmentDeleteCommandResult>
{
    private readonly ILogger<WorkspaceRoleAssignmentDeleteCommand> _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    private readonly IFabricCoreService _fabricCoreService = fabricCoreService ?? throw new ArgumentNullException(nameof(fabricCoreService));

    public override JsonTypeInfo<WorkspaceRoleAssignmentDeleteCommandResult> ResultTypeInfo =>
        CoreJsonContext.Default.WorkspaceRoleAssignmentDeleteCommandResult;

    public override void ValidateOptions(WorkspaceRoleAssignmentDeleteOptions options, ValidationResult validationResult)
    {
        base.ValidateOptions(options, validationResult);
        if (!Guid.TryParse(options.WorkspaceId, out var workspace) || workspace == Guid.Empty)
        {
            validationResult.Errors.Add("--workspace-id must be a nonempty UUID. No DELETE request was sent.");
        }
        if (!Guid.TryParse(options.WorkspaceRoleAssignmentId, out var assignment) || assignment == Guid.Empty)
        {
            validationResult.Errors.Add("--workspace-role-assignment-id must be a nonempty UUID identifying the existing assignment, not its principal. No DELETE request was sent.");
        }
    }

    public override async Task<CommandResponse> ExecuteAsync(
        CommandContext context, WorkspaceRoleAssignmentDeleteOptions options, CancellationToken cancellationToken)
    {
        var deletionConfirmed = false;
        try
        {
            var result = await _fabricCoreService.DeleteWorkspaceRoleAssignmentAsync(
                options.WorkspaceId, options.WorkspaceRoleAssignmentId, cancellationToken);
            deletionConfirmed = true;
            SetResult(context, result);
            context.Response.Status = HttpStatusCode.OK;
            context.Response.Message = result.Guidance;
        }
        catch (Exception ex)
        {
            _logger.LogError("Error deleting a Fabric workspace role assignment ({ExceptionType}).", ex.GetType().Name);
            HandleException(context, deletionConfirmed
                ? new WorkspaceRoleAssignmentDeleteRequestException(HttpStatusCode.BadGateway, responseStatusCode: HttpStatusCode.OK)
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
                "Invalid Fabric role-assignment deletion. Supply explicit workspace and existing assignment UUIDs.") +
                " No DELETE request was sent.";
        }
    }

    protected override string GetErrorMessage(Exception ex)
    {
        if (ex is ArgumentException)
        {
            return "Invalid Fabric role-assignment deletion. Supply explicit nonempty workspace and existing assignment UUIDs. No DELETE request was sent.";
        }
        var message = ex is WorkspaceRoleAssignmentDeleteRequestException { DeletionConfirmed: true }
            ? "Fabric confirmed role-assignment deletion (HTTP 200), but the local receipt could not be completed. Do not resubmit this confirmed deletion"
            : GetStatusCode(ex) switch
            {
                HttpStatusCode.BadRequest => "Fabric rejected the role-assignment deletion. Check the workspace and existing assignment UUIDs. The last admin's assignment cannot be deleted",
                HttpStatusCode.Unauthorized => "Authentication failed. Check the configured Fabric identity",
                HttpStatusCode.Forbidden => "Access denied. The caller needs workspace Admin access; delegated calls require Workspace.ReadWrite.All. The last admin's assignment cannot be deleted. Service principals and managed identities also need the tenant setting permitting Fabric API access. No alternative identity was selected",
                HttpStatusCode.NotFound => "The Fabric workspace or assignment was not found or is inaccessible. This is not confirmed deletion",
                HttpStatusCode.Conflict => "Fabric rejected the role-assignment deletion because of a conflict. The last admin's assignment cannot be deleted",
                HttpStatusCode.TooManyRequests => "Fabric throttled this role-assignment deletion",
                HttpStatusCode.RequestTimeout or HttpStatusCode.GatewayTimeout => "The role-assignment deletion request was canceled or timed out",
                _ => "Fabric role-assignment deletion was not confirmed"
            };
        if (ex is WorkspaceRoleAssignmentDeleteRequestException { RequestNotSent: true } or AuthenticationFailedException)
        {
            message += ". No DELETE request was sent";
        }
        else if (ex is not WorkspaceRoleAssignmentDeleteRequestException { DeletionConfirmed: true })
        {
            message += ex is WorkspaceRoleAssignmentDeleteRequestException
            { ResponseStatusCode: >= HttpStatusCode.BadRequest and < HttpStatusCode.InternalServerError and not HttpStatusCode.RequestTimeout }
                ? ". Fabric rejected the request; no fallback permission change was attempted"
                : ". The assignment may already have been deleted; inspect it with core_get-workspace-role-assignment separately before any new deletion. Do not retry blindly";
        }
        message += ". No automatic retry or change to other assignments was attempted";
        if (ex is WorkspaceRoleAssignmentDeleteRequestException error)
        {
            if (error.ResponseStatusCode is { } responseStatus && !error.DeletionConfirmed)
            {
                message += FormattableString.Invariant($". Fabric response HTTP {(int)responseStatus}");
            }
            if (FabricCoreHttpHelpers.IsSafeErrorCode(error.ErrorCode))
            {
                message += $". Fabric error code: {error.ErrorCode}";
            }
            if (error.ErrorMetadataUnavailable)
            {
                message += ". Optional Fabric error metadata was unavailable";
            }
            if (error.RetryAfter is { } retryAfter)
            {
                message += retryAfter.Delta is { } delay
                    ? FormattableString.Invariant($". Wait at least {delay.TotalSeconds:0} seconds before another request; this is not permission to replay the deletion")
                    : FormattableString.Invariant($". Wait until at least {retryAfter.Date?.UtcDateTime:yyyy-MM-dd HH:mm:ss} UTC before another request; this is not permission to replay the deletion");
            }
        }
        return message;
    }
}
