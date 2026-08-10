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
    Id = "4d15abf5-9d9e-4898-b376-d4fffb91871c",
    Name = "approve",
    Title = "Approve PIM Role Assignment Request",
    Description = """
        Approve a pending Azure RBAC Privileged Identity Management (PIM) role assignment request.
        Use role approval list first to discover the approval and stage identifiers for requests
        assigned to the current approver.
        """,
    Destructive = true,
    Idempotent = false,
    OpenWorld = false,
    ReadOnly = false,
    Secret = false,
    LocalRequired = false)]
public sealed class RoleAssignmentApprovalApproveCommand(
    ILogger<RoleAssignmentApprovalApproveCommand> logger,
    IAuthorizationService authorizationService,
    ISubscriptionResolver subscriptionResolver)
    : SubscriptionCommand<RoleAssignmentApprovalApproveOptions, RoleAssignmentApprovalApproveCommand.RoleAssignmentApprovalApproveCommandResult>(subscriptionResolver)
{
    private readonly ILogger<RoleAssignmentApprovalApproveCommand> _logger = logger;
    private readonly IAuthorizationService _authorizationService = authorizationService;

    public override async Task<CommandResponse> ExecuteAsync(CommandContext context, RoleAssignmentApprovalApproveOptions options, CancellationToken cancellationToken)
    {
        try
        {
            var stage = await _authorizationService.ApproveRoleAssignmentApprovalAsync(
                options.Subscription!,
                options.Scope!,
                options.Approval!,
                options.Stage!,
                options.Justification!,
                options.Tenant,
                options.RetryPolicy,
                cancellationToken);

            context.Response.Results = ResponseResult.Create(
                new(stage),
                AuthorizationJsonContext.Default.RoleAssignmentApprovalApproveCommandResult);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An exception occurred approving a PIM role assignment request.");
            HandleException(context, ex);
        }

        return context.Response;
    }

    public sealed record RoleAssignmentApprovalApproveCommandResult(RoleAssignmentApprovalStage Stage);
}
