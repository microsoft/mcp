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
    Id = "d2720924-3347-44fd-944b-71c2c933a233",
    Name = "list-workspace-role-assignments",
    Title = "List Fabric Workspace Role Assignments",
    Description = """
        Lists one complete page of Microsoft Fabric workspace role assignments for an explicit workspace UUID.
        Returns assignment IDs, roles, principal IDs and types, available display names and principal details,
        and any continuation token and Retry-After guidance. Preserves future role and principal type strings.
        Call again with the unchanged token and the same workspace for another page, even after an empty page.
        Makes exactly one GET, without directory or Graph lookups, group-membership expansion, identity enrichment,
        per-row requests, automatic pagination, retries, or polling. Does not calculate effective permissions or change roles.
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
public sealed class WorkspaceRoleAssignmentListCommand(
    ILogger<WorkspaceRoleAssignmentListCommand> logger,
    IFabricCoreService fabricCoreService) : FabricCoreCommand<WorkspaceRoleAssignmentListOptions, WorkspaceRoleAssignmentListCommandResult>
{
    private readonly ILogger<WorkspaceRoleAssignmentListCommand> _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    private readonly IFabricCoreService _fabricCoreService = fabricCoreService ?? throw new ArgumentNullException(nameof(fabricCoreService));

    public override JsonTypeInfo<WorkspaceRoleAssignmentListCommandResult> ResultTypeInfo =>
        CoreJsonContext.Default.WorkspaceRoleAssignmentListCommandResult;

    public override void ValidateOptions(WorkspaceRoleAssignmentListOptions options, ValidationResult validationResult)
    {
        base.ValidateOptions(options, validationResult);
        if (!Guid.TryParse(options.WorkspaceId, out var workspaceId) || workspaceId == Guid.Empty)
        {
            validationResult.Errors.Add("--workspace-id must be a nonempty UUID.");
        }
        if (!FabricContinuationToken.IsValidContinuationToken(options.ContinuationToken))
        {
            validationResult.Errors.Add(FabricContinuationToken.InvalidContinuationTokenMessage);
        }
    }

    public override async Task<CommandResponse> ExecuteAsync(
        CommandContext context, WorkspaceRoleAssignmentListOptions options, CancellationToken cancellationToken)
    {
        try
        {
            var result = await _fabricCoreService.ListWorkspaceRoleAssignmentsAsync(
                options.WorkspaceId, options.ContinuationToken, cancellationToken);
            SetResult(context, result);
        }
        catch (Exception ex)
        {
            _logger.LogError("Error listing Fabric workspace role assignments ({ExceptionType}).", ex.GetType().Name);
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
            HttpStatusCode.BadRequest => "Check the workspace UUID and unchanged continuation token",
            HttpStatusCode.Unauthorized => "Authentication failed. Use the configured identity with permission to read workspace role assignments",
            HttpStatusCode.Forbidden => "Access denied. The caller needs workspace Member or higher access; delegated calls require Workspace.Read.All or Workspace.ReadWrite.All. Service principals and managed identities also require the tenant setting permitting service principals to use Fabric APIs",
            HttpStatusCode.NotFound => "The Fabric workspace was not found. Check its ID and the caller's access",
            HttpStatusCode.TooManyRequests => "Fabric throttled the request. Wait before retrying",
            HttpStatusCode.RequestTimeout => "The Fabric workspace role-assignment listing was canceled or timed out",
            HttpStatusCode.GatewayTimeout => "The Fabric workspace role-assignment listing timed out",
            HttpStatusCode.BadGateway => "Fabric returned an invalid workspace role-assignment page. Expected required assignment and principal metadata with consistent continuation information, up to 1 MiB and 64 JSON levels, with at most 61 retained output levels. No partial page was returned",
            _ => "Unable to list Fabric workspace role assignments. Retry the request or check service availability"
        };
        if (ex is WorkspaceRoleAssignmentListRequestException { RetryAfter: { } retryAfter })
        {
            message += retryAfter.Delta is { } delay
                ? FormattableString.Invariant($". Wait at least {delay.TotalSeconds:0} seconds before retrying")
                : FormattableString.Invariant($". Retry after {retryAfter.Date?.UtcDateTime:yyyy-MM-dd HH:mm:ss} UTC");
        }
        return message;
    }
}
