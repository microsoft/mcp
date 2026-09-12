// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

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
        List role assignments. This command retrieves and displays the Azure RBAC role assignments
        at the specified scope and at any scope nested beneath it. Assignments inherited from a parent
        scope are not included. The scope may be a subscription, resource group, resource, or management
        group; a subscription is not required when the scope is a management group. Results include role
        definition IDs and principal IDs.
        """,
    OperationPlane = ToolOperationPlane.Control,
    Destructive = false,
    Idempotent = true,
    OpenWorld = false,
    ReadOnly = true,
    Secret = false,
    LocalRequired = false)]
public sealed class RoleAssignmentListCommand(ILogger<RoleAssignmentListCommand> logger, IAuthorizationService authorizationService, ISubscriptionResolver subscriptionResolver)
    : AuthenticatedCommand<RoleAssignmentListOptions, RoleAssignmentListCommand.RoleAssignmentListCommandResult>
{
    private readonly ILogger<RoleAssignmentListCommand> _logger = logger;
    private readonly IAuthorizationService _authorizationService = authorizationService;
    private readonly ISubscriptionResolver _subscriptionResolver = subscriptionResolver;

    // Management groups are outside every subscription, so this command binds and validates
    // --subscription itself rather than inheriting SubscriptionCommand's unconditional handling.
    private static bool IsManagementGroupScope(RoleAssignmentListOptions options) =>
        ManagementGroupScope.TryParse(options.Scope, out _);

    public override void PostBindOptions(RoleAssignmentListOptions options)
    {
        base.PostBindOptions(options);

        if (IsManagementGroupScope(options))
        {
            return;
        }

        // Always post-process subscription via resolver (env var / CLI profile fallback)
        options.Subscription = _subscriptionResolver.ResolveSubscription(options.Subscription);
        if (!string.IsNullOrEmpty(options.Subscription))
        {
            // Trim any surrounding quotes that may have been included in the input
            options.Subscription = options.Subscription.Trim('"', '\'');
        }
    }

    public override void ValidateOptions(RoleAssignmentListOptions options, ValidationResult validationResult)
    {
        base.ValidateOptions(options, validationResult);

        if (IsManagementGroupScope(options))
        {
            if (!string.IsNullOrEmpty(options.Subscription))
            {
                validationResult.Errors.Add(
                    "Omit --subscription when --scope is a management group because management groups are outside subscriptions.");
            }
        }
        else if (string.IsNullOrEmpty(options.Subscription))
        {
            validationResult.Errors.Add("Missing Required options: --subscription");
        }
    }

    public override async Task<CommandResponse> ExecuteAsync(CommandContext context, RoleAssignmentListOptions options, CancellationToken cancellationToken)
    {
        try
        {
            var assignments = await _authorizationService.ListRoleAssignmentsAsync(
                options.Subscription,
                options.Scope,
                options.Tenant,
                cancellationToken);

            context.Response.Results = ResponseResult.Create(new(options.Scope, assignments?.Results ?? [], assignments?.AreResultsTruncated ?? false), AuthorizationJsonContext.Default.RoleAssignmentListCommandResult);
        }
        catch (Exception ex)
        {
            if (IsManagementGroupScope(options))
            {
                _logger.LogError(
                    ex,
                    "An exception occurred listing role assignments for scope '{Scope}'.",
                    FormatForLogging(options.Scope));
            }
            else
            {
                _logger.LogError(
                    ex,
                    "An exception occurred listing role assignments for scope '{Scope}' in subscription '{Subscription}'.",
                    FormatForLogging(options.Scope),
                    FormatForLogging(options.Subscription));
            }

            HandleException(context, ex);
        }

        return context.Response;
    }

    private static string FormatForLogging(string? value) => value switch
    {
        null => "<null>",
        "" => "<empty>",
        _ => value
    };

    public sealed record RoleAssignmentListCommandResult(string Scope, List<RoleAssignment> Assignments, bool AreResultsTruncated);
}
