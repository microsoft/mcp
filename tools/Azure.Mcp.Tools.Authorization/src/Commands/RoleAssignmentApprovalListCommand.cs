// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Core.Commands.Subscription;
using Azure.Mcp.Core.Services.Azure.Subscription;
using Azure.Mcp.Tools.Authorization.Models;
using Azure.Mcp.Tools.Authorization.Options;
using Azure.Mcp.Tools.Authorization.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Mcp.Core.Models.Command;

namespace Azure.Mcp.Tools.Authorization.Commands;

[CommandMetadata(
    Id = "6c9331da-29b9-47d2-9ab8-699f9679b0fd",
    Name = "list",
    Title = "List Pending PIM Role Assignment Approvals",
    Description = """
        List pending Azure RBAC Privileged Identity Management (PIM) role assignment approval requests
        for the current approver at the specified scope. Results include approval and stage identifiers
        that can be passed to the approve command.
        """,
    Destructive = false,
    Idempotent = true,
    OpenWorld = false,
    ReadOnly = true,
    Secret = false,
    LocalRequired = false)]
public sealed class RoleAssignmentApprovalListCommand(
    ILogger<RoleAssignmentApprovalListCommand> logger,
    IAuthorizationService authorizationService,
    ISubscriptionResolver subscriptionResolver)
    : SubscriptionCommand<RoleAssignmentApprovalListOptions, RoleAssignmentApprovalListCommand.RoleAssignmentApprovalListCommandResult>(subscriptionResolver)
{
    private readonly ILogger<RoleAssignmentApprovalListCommand> _logger = logger;
    private readonly IAuthorizationService _authorizationService = authorizationService;

    public override async Task<CommandResponse> ExecuteAsync(CommandContext context, RoleAssignmentApprovalListOptions options, CancellationToken cancellationToken)
    {
        try
        {
            var approvals = await _authorizationService.ListPendingRoleAssignmentApprovalsAsync(
                options.Subscription!,
                options.Scope!,
                options.Tenant,
                options.RetryPolicy,
                cancellationToken);

            context.Response.Results = ResponseResult.Create(
                new(approvals),
                AuthorizationJsonContext.Default.RoleAssignmentApprovalListCommandResult);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An exception occurred listing pending PIM role assignment approvals.");
            HandleException(context, ex);
        }

        return context.Response;
    }

    public sealed record RoleAssignmentApprovalListCommandResult(List<RoleAssignmentApproval> Approvals);
}
