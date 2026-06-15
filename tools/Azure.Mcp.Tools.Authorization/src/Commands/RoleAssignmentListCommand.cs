// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Core.Commands.Subscription;
using Azure.Mcp.Core.Services.Azure.Subscription;
using Azure.Mcp.Tools.Authorization.Models;
using Azure.Mcp.Tools.Authorization.Options;
using Azure.Mcp.Tools.Authorization.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Mcp.Core.Commands;
using Microsoft.Mcp.Core.Models.Command;

namespace Azure.Mcp.Tools.Authorization.Commands;

[CommandMetadata(
    Id = "1dfbef45-4014-4575-a9ba-2242bc792e54",
    Name = "list",
    Title = "List Role Assignments",
    Description = """
        List role assignments. This command retrieves and displays all Azure RBAC role assignments
        in the specified scope. Results include role definition IDs and principal IDs, returned as a JSON array.
        """,
    Destructive = false,
    Idempotent = true,
    OpenWorld = false,
    ReadOnly = true,
    Secret = false,
    LocalRequired = false)]
public sealed class RoleAssignmentListCommand(ILogger<RoleAssignmentListCommand> logger, IAuthorizationService authorizationService, ISubscriptionResolver subscriptionResolver)
    : SubscriptionCommand<RoleAssignmentListOptions, RoleAssignmentListCommand.RoleAssignmentListCommandResult>(subscriptionResolver)
{
    private readonly ILogger<RoleAssignmentListCommand> _logger = logger;
    private readonly IAuthorizationService _authorizationService = authorizationService;

    public override async Task<CommandResponse> ExecuteAsync(CommandContext context, RoleAssignmentListOptions options, CancellationToken cancellationToken)
    {
        try
        {
            var assignments = await _authorizationService.ListRoleAssignmentsAsync(
                options.Subscription!,
                options.Scope,
                options.Tenant,
                options.RetryPolicy,
                cancellationToken);

            context.Response.Results = ResponseResult.Create(new(assignments?.Results ?? [], assignments?.AreResultsTruncated ?? false), AuthorizationJsonContext.Default.RoleAssignmentListCommandResult);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An exception occurred listing role assignments.");
            HandleException(context, ex);
        }

        return context.Response;
    }

    public sealed record RoleAssignmentListCommandResult(List<RoleAssignment> Assignments, bool AreResultsTruncated);
}
