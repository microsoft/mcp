// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Fabric.Mcp.Tools.OneLake.Models;
using Fabric.Mcp.Tools.OneLake.Options;
using Fabric.Mcp.Tools.OneLake.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Mcp.Core.Commands;
using Microsoft.Mcp.Core.Models.Command;

namespace Fabric.Mcp.Tools.OneLake.Commands.Security;

[CommandMetadata(
    Id = "a1b2c3d4-1001-4000-8000-000000000002",
    Name = "get_data_access_role",
    Title = "Get OneLake Data Access Role",
    Description = """
        Get the full definition of a single data access role on a single item —
        members, permissions, decision rules. Scoped to one role on one item per
        call. Use after onelake_list_data_access_roles once you know which role
        you need on which item. Distinct from onelake_get_principal_access,
        which returns the effective (resolved) access for a given principal
        across all roles on an item. Caller must be a workspace Admin or Member
        on the item's workspace. Requires OneLake.Read.All.
        """,
    Destructive = false,
    Idempotent = true,
    LocalRequired = false,
    OpenWorld = false,
    ReadOnly = true,
    Secret = false)]
public sealed class DataAccessRoleGetCommand(ILogger<DataAccessRoleGetCommand> logger, IOneLakeService oneLakeService)
    : AuthenticatedCommand<DataAccessRoleGetOptions, DataAccessRole>()
{
    private readonly ILogger<DataAccessRoleGetCommand> _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    private readonly IOneLakeService _oneLakeService = oneLakeService ?? throw new ArgumentNullException(nameof(oneLakeService));

    public override void ValidateOptions(DataAccessRoleGetOptions options, ValidationResult validationResult)
    {
        base.ValidateOptions(options, validationResult);

        if (string.IsNullOrWhiteSpace(options.WorkspaceId) && string.IsNullOrWhiteSpace(options.Workspace))
        {
            validationResult.Errors.Add("Workspace identifier is required. Provide --workspace or --workspace-id.");
        }

        var effectiveValue = !string.IsNullOrWhiteSpace(options.WorkspaceId) ? options.WorkspaceId : options.Workspace;
        if (!string.IsNullOrWhiteSpace(effectiveValue) && !Guid.TryParse(effectiveValue, out _))
        {
            validationResult.Errors.Add("Workspace must be a valid GUID. Name-based resolution is not supported for this command.");
        }
    }

    public override async Task<CommandResponse> ExecuteAsync(CommandContext context, DataAccessRoleGetOptions options, CancellationToken cancellationToken)
    {
        var workspaceId = string.IsNullOrWhiteSpace(options.WorkspaceId) ? options.Workspace : options.WorkspaceId;
        try
        {
            var result = await _oneLakeService.GetDataAccessRoleAsync(workspaceId!, options.ItemId, options.RoleName, cancellationToken);
            context.Response.Results = ResponseResult.Create(result, OneLakeJsonContext.Default.DataAccessRole);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting data access role. Workspace: {Workspace}, Item: {Item}, Role: {Role}.",
                workspaceId, options.ItemId, options.RoleName);
            HandleException(context, ex);
        }

        return context.Response;
    }
}

