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
    Id = "a1b2c3d4-1001-4000-8000-000000000004",
    Name = "delete_data_access_role",
    Title = "Delete OneLake Data Access Role",
    Description = """
        Delete a single data access role from a single item. Scoped to one role
        on one item per call. Destructive — principals that gained access only
        via this role lose it on this item. Does not affect roles on other items.
        Caller must be a workspace Admin or Member on the item's workspace.
        Requires OneLake.ReadWrite.All.
        """,
    Destructive = true,
    Idempotent = true,
    LocalRequired = false,
    OpenWorld = false,
    ReadOnly = false,
    Secret = false)]
public sealed class DataAccessRoleDeleteCommand(ILogger<DataAccessRoleDeleteCommand> logger, IOneLakeService oneLakeService)
    : AuthenticatedCommand<DataAccessRoleDeleteOptions, DataAccessRoleDeleteCommand.DataAccessRoleDeleteCommandResult>()
{
    private readonly ILogger<DataAccessRoleDeleteCommand> _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    private readonly IOneLakeService _oneLakeService = oneLakeService ?? throw new ArgumentNullException(nameof(oneLakeService));

    public override void ValidateOptions(DataAccessRoleDeleteOptions options, ValidationResult validationResult)
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

    public override async Task<CommandResponse> ExecuteAsync(CommandContext context, DataAccessRoleDeleteOptions options, CancellationToken cancellationToken)
    {
        var workspaceId = string.IsNullOrWhiteSpace(options.WorkspaceId) ? options.Workspace : options.WorkspaceId;
        try
        {
            await _oneLakeService.DeleteDataAccessRoleAsync(workspaceId!, options.ItemId, options.RoleName, cancellationToken);
            context.Response.Results = ResponseResult.Create(new(options.RoleName, "Data access role deleted successfully."), OneLakeJsonContext.Default.DataAccessRoleDeleteCommandResult);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting data access role. Workspace: {Workspace}, Item: {Item}, Role: {Role}.",
                workspaceId, options.ItemId, options.RoleName);
            HandleException(context, ex);
        }

        return context.Response;
    }

    public sealed record DataAccessRoleDeleteCommandResult(string RoleName, string Message);
}
