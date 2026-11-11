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
    Id = "8e4764dd-7631-432b-9b94-6e2a97f08a81",
    Name = "get-workspace-role-assignment",
    Title = "Get Fabric Workspace Role Assignment",
    Description = """
        Gets one Microsoft Fabric workspace role assignment using explicit workspace and assignment UUIDs.
        Returns the assignment ID, role, principal ID and type, available display name and principal details,
        and Retry-After guidance. Preserves future role and principal type strings. Assignment and principal IDs are independent.
        Makes exactly one GET, without listing assignments, translating principal IDs, directory or Graph lookups,
        group-membership expansion, identity enrichment, retries, or polling. Does not calculate effective permissions or change roles.
        Requires workspace Member or higher access; delegated calls require Workspace.Read.All or Workspace.ReadWrite.All.
        Supports users, service principals, and managed identities through the configured Fabric authentication.
        """,
    OperationPlane = ToolOperationPlane.Control,
    Destructive = false,
    Idempotent = true,
    LocalRequired = false,
    OpenWorld = false,
    ReadOnly = true,
    Secret = false)]
public sealed class WorkspaceRoleAssignmentGetCommand(
    ILogger<WorkspaceRoleAssignmentGetCommand> logger,
    IFabricCoreService fabricCoreService) : FabricCoreCommand<WorkspaceRoleAssignmentGetOptions, WorkspaceRoleAssignmentGetCommandResult>
{
    private readonly ILogger<WorkspaceRoleAssignmentGetCommand> _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    private readonly IFabricCoreService _fabricCoreService = fabricCoreService ?? throw new ArgumentNullException(nameof(fabricCoreService));

    public override JsonTypeInfo<WorkspaceRoleAssignmentGetCommandResult> ResultTypeInfo =>
        CoreJsonContext.Default.WorkspaceRoleAssignmentGetCommandResult;

    public override void ValidateOptions(WorkspaceRoleAssignmentGetOptions options, ValidationResult validationResult)
    {
        base.ValidateOptions(options, validationResult);
        if (!Guid.TryParse(options.WorkspaceId, out var workspaceId) || workspaceId == Guid.Empty)
        {
            validationResult.Errors.Add("--workspace-id must be a nonempty UUID.");
        }
        if (!Guid.TryParse(options.WorkspaceRoleAssignmentId, out var assignmentId) || assignmentId == Guid.Empty)
        {
            validationResult.Errors.Add("--workspace-role-assignment-id must be a nonempty UUID.");
        }
    }

    public override async Task<CommandResponse> ExecuteAsync(
        CommandContext context, WorkspaceRoleAssignmentGetOptions options, CancellationToken cancellationToken)
    {
        try
        {
            var result = await _fabricCoreService.GetWorkspaceRoleAssignmentAsync(
                options.WorkspaceId, options.WorkspaceRoleAssignmentId, cancellationToken);
            SetResult(context, result);
        }
        catch (Exception ex)
        {
            _logger.LogError("Error retrieving Fabric workspace role-assignment metadata ({ExceptionType}).", ex.GetType().Name);
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
            HttpStatusCode.BadRequest => "Provide valid workspace and workspace role assignment UUIDs; the assignment ID is not a principal-ID lookup",
            HttpStatusCode.Unauthorized => "Authentication failed. Use the configured identity with permission to read workspace role assignments",
            HttpStatusCode.Forbidden => "Access denied. The caller needs workspace Member or higher access; delegated calls require Workspace.Read.All or Workspace.ReadWrite.All. Service principals and managed identities also require the tenant setting permitting service principals to use Fabric APIs",
            HttpStatusCode.NotFound => "The Fabric workspace or role assignment was not found. Check the supplied IDs and the caller's access",
            HttpStatusCode.TooManyRequests => "Fabric throttled the request. Wait before retrying",
            HttpStatusCode.RequestTimeout => "The Fabric workspace role-assignment request was canceled or timed out",
            HttpStatusCode.GatewayTimeout => "The Fabric workspace role-assignment request timed out",
            HttpStatusCode.BadGateway => "Fabric returned invalid workspace role-assignment metadata. Expected a matching assignment ID and required principal metadata, up to 1 MiB and 64 JSON levels, with at most 61 retained output levels. No partial result was returned",
            _ => "Unable to retrieve Fabric workspace role-assignment metadata. Retry the request or check service availability"
        };
        if (ex is WorkspaceRoleAssignmentGetRequestException { RetryAfter: { } retryAfter })
        {
            message += retryAfter.Delta is { } delay
                ? FormattableString.Invariant($". Wait at least {delay.TotalSeconds:0} seconds before retrying")
                : FormattableString.Invariant($". Retry after {retryAfter.Date?.UtcDateTime:yyyy-MM-dd HH:mm:ss} UTC");
        }
        return message;
    }
}
