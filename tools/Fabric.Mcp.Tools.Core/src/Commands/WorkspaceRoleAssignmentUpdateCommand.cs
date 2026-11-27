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
    Id = "b27b343a-9972-4f69-867e-7dc7d7a27811",
    Name = "update-workspace-role-assignment",
    Title = "Update Fabric Workspace Role Assignment",
    Description = """
        Updates one existing Microsoft Fabric workspace role assignment using explicit workspace-id, workspace-role-assignment-id, and role.
        Assignment and principal IDs are independent; supply the existing assignment UUID, not a principal name or an ID to translate.
        Admin, Member, Contributor, and Viewer are documented roles. Future nonempty role strings are forwarded unchanged.
        No default, trimming, upgrade, broadening, or permission fallback is selected. This can increase or reduce existing workspace access.
        Requires workspace Admin access; delegated calls require Workspace.ReadWrite.All. The last admin's assignment cannot be changed.
        Users, service principals, and managed identities use the configured Fabric authentication; Fabric enforces permissions.
        Makes exactly one PATCH with only role. No principal payload, preflight List/Get, directory/Graph calls, membership expansion,
        enrichment, create/upsert fallback, automatic conflict update, retry, polling, or follow-up verification is performed.
        Only HTTP 200 confirms update, returning matching assignment and role metadata with available principal details.
        After an uncertain send inspect the assignment separately; never retry blindly. If update is confirmed but metadata is unavailable,
        do not resubmit it. This is a permission-changing mutation, not a read or a way to create an assignment.
        """,
    OperationPlane = ToolOperationPlane.Control,
    Destructive = true,
    Idempotent = true,
    LocalRequired = false,
    OpenWorld = false,
    ReadOnly = false,
    Secret = false)]
public sealed class WorkspaceRoleAssignmentUpdateCommand(
    ILogger<WorkspaceRoleAssignmentUpdateCommand> logger,
    IFabricCoreService fabricCoreService) : FabricCoreCommand<WorkspaceRoleAssignmentUpdateOptions, WorkspaceRoleAssignmentUpdateCommandResult>
{
    private readonly ILogger<WorkspaceRoleAssignmentUpdateCommand> _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    private readonly IFabricCoreService _fabricCoreService = fabricCoreService ?? throw new ArgumentNullException(nameof(fabricCoreService));

    public override JsonTypeInfo<WorkspaceRoleAssignmentUpdateCommandResult> ResultTypeInfo =>
        CoreJsonContext.Default.WorkspaceRoleAssignmentUpdateCommandResult;

    public override void ValidateOptions(WorkspaceRoleAssignmentUpdateOptions options, ValidationResult validationResult)
    {
        base.ValidateOptions(options, validationResult);
        try
        {
            _ = WorkspaceRoleAssignmentUpdateInput.CreateRequest(options.WorkspaceId, options.WorkspaceRoleAssignmentId, options.Role);
        }
        catch (FabricItemInputException ex)
        {
            validationResult.Errors.Add(ex.Message);
        }
    }

    public override async Task<CommandResponse> ExecuteAsync(
        CommandContext context, WorkspaceRoleAssignmentUpdateOptions options, CancellationToken cancellationToken)
    {
        try
        {
            var result = await _fabricCoreService.UpdateWorkspaceRoleAssignmentAsync(
                options.WorkspaceId, options.WorkspaceRoleAssignmentId, options.Role, cancellationToken);
            SetResult(context, result);
            context.Response.Message = result.Guidance;
        }
        catch (Exception ex)
        {
            _logger.LogError("Error updating a Fabric workspace role assignment ({ExceptionType}).", ex.GetType().Name);
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
                "Invalid Fabric role-assignment update. Supply workspace and existing assignment UUIDs and an explicit replacement role. The request was not sent.");
        }
    }

    protected override string GetErrorMessage(Exception ex)
    {
        if (ex is FabricItemInputException)
        {
            return ex.Message + " The role-assignment update request was not sent.";
        }
        var message = ex is WorkspaceRoleAssignmentUpdateRequestException { UpdateConfirmed: true }
            ? "Fabric confirmed role-assignment update (HTTP 200), but the returned metadata could not be read, validated against the requested assignment and role, or serialized safely. Do not resubmit this update; inspect the assignment separately"
            : GetStatusCode(ex) switch
            {
                HttpStatusCode.BadRequest => "Fabric rejected the role-assignment update. Check the workspace, existing assignment, and requested role. The last admin's assignment cannot be changed",
                HttpStatusCode.Unauthorized => "Authentication failed. Check the configured Fabric identity",
                HttpStatusCode.Forbidden => "Access denied. The caller needs workspace Admin access; delegated calls require Workspace.ReadWrite.All. The last admin's assignment cannot be changed. Service principals and managed identities also need the tenant setting permitting Fabric API access. No alternative identity or role was selected",
                HttpStatusCode.NotFound => "The Fabric workspace or assignment was not found, or the caller does not have access. No assignment was created",
                HttpStatusCode.Conflict => "Fabric rejected the role-assignment update because of a conflict. No fallback or automatic conflict update was attempted",
                HttpStatusCode.TooManyRequests => "Fabric throttled this role-assignment update",
                HttpStatusCode.RequestTimeout or HttpStatusCode.GatewayTimeout => "The role-assignment update request was canceled or timed out",
                _ => "Fabric role-assignment update was not confirmed"
            };
        if (ex is WorkspaceRoleAssignmentUpdateRequestException { RequestNotSent: true } or
            AuthenticationFailedException or ArgumentException or OperationCanceledException)
        {
            message += ". The role-assignment update request was not sent";
        }
        else if (ex is not WorkspaceRoleAssignmentUpdateRequestException { UpdateConfirmed: true })
        {
            if (ex is WorkspaceRoleAssignmentUpdateRequestException
                { ResponseStatusCode: >= HttpStatusCode.BadRequest and < HttpStatusCode.InternalServerError and not HttpStatusCode.RequestTimeout })
            {
                message += ". Fabric rejected the request; no fallback permission change was attempted";
            }
            else
            {
                message += ". Access may already have changed; inspect the assignment separately before any new update. Do not retry blindly";
            }
        }
        message += ". No automatic retry was attempted";
        if (ex is WorkspaceRoleAssignmentUpdateRequestException error)
        {
            if (error.ResponseStatusCode is { } responseStatus && !error.UpdateConfirmed)
            {
                message += FormattableString.Invariant($". Fabric response HTTP {(int)responseStatus}");
            }
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
                    ? FormattableString.Invariant($". Wait at least {delay.TotalSeconds:0} seconds before another request; this is not permission to replay the update")
                    : FormattableString.Invariant($". Wait until at least {retryAfter.Date?.UtcDateTime:yyyy-MM-dd HH:mm:ss} UTC before another request; this is not permission to replay the update");
            }
        }
        return message;
    }
}
