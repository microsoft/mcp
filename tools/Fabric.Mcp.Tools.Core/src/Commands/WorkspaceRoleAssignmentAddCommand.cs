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
    Id = "db9598d5-b603-4fcb-99b5-941170cc63da",
    Name = "add-workspace-role-assignment",
    Title = "Add Fabric Workspace Role Assignment",
    Description = """
        Adds one Microsoft Fabric workspace role assignment using explicit workspace-id, principal JSON, and role.
        The principal requires an id UUID and type; no name, email, application ID, group expansion, or identity lookup is performed.
        User, Group, ServicePrincipal, ServicePrincipalProfile, and EntireTenant are documented principal types.
        EntireTenant explicitly grants access to all tenant users. Admin, Member, Contributor, and Viewer are documented roles.
        Extensible type and role strings are forwarded unchanged; no default, upgrade, broadening, or permission fallback is chosen.
        Requires workspace Member or higher access; delegated calls require Workspace.ReadWrite.All. Members can grant Member or lower;
        granting Admin requires Admin access. Fabric enforces permissions and its 1,000 users/groups per workspace limit.
        Users, service principals, and managed identities use the configured Fabric authentication.
        Makes exactly one POST, without preflight List/Get, Graph calls, get-or-create, conflict updates, retries, or polling.
        Only HTTP 201 confirms creation. Returns the new assignment and available principal metadata; assignment and principal IDs are independent.
        After an uncertain send inspect assignments separately, never retry blindly. If creation is confirmed but metadata is unavailable,
        do not resubmit the grant. This tool is an additive permission mutation, not a read or a way to update an existing assignment.
        """,
    OperationPlane = ToolOperationPlane.Control,
    Destructive = false,
    Idempotent = false,
    LocalRequired = false,
    OpenWorld = false,
    ReadOnly = false,
    Secret = false)]
public sealed class WorkspaceRoleAssignmentAddCommand(
    ILogger<WorkspaceRoleAssignmentAddCommand> logger,
    IFabricCoreService fabricCoreService) : FabricCoreCommand<WorkspaceRoleAssignmentAddOptions, WorkspaceRoleAssignmentAddCommandResult>
{
    private readonly ILogger<WorkspaceRoleAssignmentAddCommand> _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    private readonly IFabricCoreService _fabricCoreService = fabricCoreService ?? throw new ArgumentNullException(nameof(fabricCoreService));

    public override JsonTypeInfo<WorkspaceRoleAssignmentAddCommandResult> ResultTypeInfo =>
        CoreJsonContext.Default.WorkspaceRoleAssignmentAddCommandResult;

    public override void ValidateOptions(WorkspaceRoleAssignmentAddOptions options, ValidationResult validationResult)
    {
        base.ValidateOptions(options, validationResult);
        try
        {
            _ = WorkspaceRoleAssignmentAddInput.CreateRequest(options.WorkspaceId, options.Principal, options.Role);
        }
        catch (FabricItemInputException ex)
        {
            validationResult.Errors.Add(ex.Message);
        }
    }

    public override async Task<CommandResponse> ExecuteAsync(
        CommandContext context, WorkspaceRoleAssignmentAddOptions options, CancellationToken cancellationToken)
    {
        try
        {
            var result = await _fabricCoreService.AddWorkspaceRoleAssignmentAsync(
                options.WorkspaceId, options.Principal, options.Role, cancellationToken);
            SetResult(context, result);
            context.Response.Status = HttpStatusCode.Created;
            context.Response.Message = result.Guidance;
        }
        catch (Exception ex)
        {
            _logger.LogError("Error adding a Fabric workspace role assignment ({ExceptionType}).", ex.GetType().Name);
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
                "Invalid Fabric role-assignment creation. Supply a workspace UUID, valid principal JSON with explicit id and type, and an explicit role. The request was not sent.");
        }
    }

    protected override string GetErrorMessage(Exception ex)
    {
        if (ex is FabricItemInputException)
        {
            return ex.Message + " The role-assignment request was not sent.";
        }
        var message = ex is WorkspaceRoleAssignmentAddRequestException { CreationConfirmed: true }
            ? "Fabric confirmed role-assignment creation (HTTP 201), but the returned metadata could not be read, validated against the requested principal and role, or serialized safely. Do not resubmit this grant; inspect workspace assignments separately"
            : GetStatusCode(ex) switch
            {
                HttpStatusCode.BadRequest => "Fabric rejected the role-assignment request. Check the workspace, principal, and requested role",
                HttpStatusCode.Unauthorized => "Authentication failed. Check the configured Fabric identity",
                HttpStatusCode.Forbidden => "Access denied. The caller needs workspace Member or higher access; delegated calls require Workspace.ReadWrite.All. Members can grant Member or lower; granting Admin requires Admin access. Service principals and managed identities also need the tenant setting permitting Fabric API access. No alternative identity or role was selected",
                HttpStatusCode.NotFound => "The Fabric workspace or principal was not found, or the caller does not have access",
                HttpStatusCode.Conflict => "Fabric rejected the grant because of a conflict. An existing assignment is not returned as success and no update was attempted",
                HttpStatusCode.TooManyRequests => "Fabric throttled this role-assignment request",
                HttpStatusCode.RequestTimeout or HttpStatusCode.GatewayTimeout => "The role-assignment request was canceled or timed out",
                _ => "Fabric role-assignment creation was not confirmed"
            };
        if (ex is WorkspaceRoleAssignmentAddRequestException { RequestNotSent: true } or
            AuthenticationFailedException or ArgumentException or OperationCanceledException)
        {
            message += ". The role-assignment request was not sent";
        }
        else if (ex is not WorkspaceRoleAssignmentAddRequestException { CreationConfirmed: true })
        {
            if (ex is WorkspaceRoleAssignmentAddRequestException
                { ResponseStatusCode: >= HttpStatusCode.BadRequest and < HttpStatusCode.InternalServerError and not HttpStatusCode.RequestTimeout })
            {
                message += ". Fabric rejected the request; no fallback grant was attempted";
            }
            else
            {
                message += ". Access may already have been granted; inspect assignments separately before any new grant. Do not retry blindly";
            }
        }
        message += ". No automatic retry was attempted";
        if (ex is WorkspaceRoleAssignmentAddRequestException error)
        {
            if (error.ResponseStatusCode is { } responseStatus && !error.CreationConfirmed)
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
                    ? FormattableString.Invariant($". Wait at least {delay.TotalSeconds:0} seconds before another request; this is not permission to replay the grant")
                    : FormattableString.Invariant($". Wait until at least {retryAfter.Date?.UtcDateTime:yyyy-MM-dd HH:mm:ss} UTC before another request; this is not permission to replay the grant");
            }
        }
        return message;
    }
}
