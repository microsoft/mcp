// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Fabric.Mcp.Tools.OneLake.Models;
using Fabric.Mcp.Tools.OneLake.Options;
using Fabric.Mcp.Tools.OneLake.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Mcp.Core.Commands;
using Microsoft.Mcp.Core.Extensions;
using Microsoft.Mcp.Core.Models.Option;

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
public sealed class DataAccessRoleDeleteCommand(
    ILogger<DataAccessRoleDeleteCommand> logger,
    IOneLakeService oneLakeService) : GlobalCommand<DataAccessRoleDeleteOptions>()
{
    private readonly ILogger<DataAccessRoleDeleteCommand> _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    private readonly IOneLakeService _oneLakeService = oneLakeService ?? throw new ArgumentNullException(nameof(oneLakeService));

    protected override void RegisterOptions(Command command)
    {
        base.RegisterOptions(command);
        command.Options.Add(FabricOptionDefinitions.WorkspaceId.AsOptional());
        command.Options.Add(FabricOptionDefinitions.Workspace.AsOptional());
        command.Options.Add(FabricOptionDefinitions.ItemId.AsRequired());
        command.Options.Add(FabricOptionDefinitions.RoleName.AsRequired());
        command.Validators.Add(result =>
        {
            var workspaceId = result.GetValueOrDefault<string>(FabricOptionDefinitions.WorkspaceId.Name);
            var workspace = result.GetValueOrDefault<string>(FabricOptionDefinitions.Workspace.Name);
            if (string.IsNullOrWhiteSpace(workspaceId) && string.IsNullOrWhiteSpace(workspace))
            {
                result.AddError("Workspace identifier is required. Provide --workspace or --workspace-id.");
            }

            var effectiveValue = !string.IsNullOrWhiteSpace(workspaceId) ? workspaceId : workspace;
            if (!string.IsNullOrWhiteSpace(effectiveValue) && !Guid.TryParse(effectiveValue, out _))
            {
                result.AddError("Workspace must be a valid GUID. Name-based resolution is not supported for this command.");
            }
        });
    }

    protected override DataAccessRoleDeleteOptions BindOptions(ParseResult parseResult)
    {
        var options = base.BindOptions(parseResult);
        var workspaceId = parseResult.GetValueOrDefault<string>(FabricOptionDefinitions.WorkspaceId.Name);
        var workspace = parseResult.GetValueOrDefault<string>(FabricOptionDefinitions.Workspace.Name);
        options.WorkspaceId = !string.IsNullOrWhiteSpace(workspaceId) ? workspaceId! : workspace ?? string.Empty;
        options.ItemId = parseResult.GetValueOrDefault<string>(FabricOptionDefinitions.ItemId.Name);
        options.RoleName = parseResult.GetValueOrDefault<string>(FabricOptionDefinitions.RoleName.Name);
        return options;
    }

    public override async Task<CommandResponse> ExecuteAsync(CommandContext context, ParseResult parseResult, CancellationToken cancellationToken)
    {
        if (!Validate(parseResult.CommandResult, context.Response).IsValid)
        {
            return context.Response;
        }

        var options = BindOptions(parseResult);
        try
        {
            await _oneLakeService.DeleteDataAccessRoleAsync(options.WorkspaceId!, options.ItemId!, options.RoleName!, cancellationToken);
            var result = new DataAccessRoleDeleteCommandResult(options.RoleName!, "Data access role deleted successfully.");
            context.Response.Results = ResponseResult.Create(result, OneLakeJsonContext.Default.DataAccessRoleDeleteCommandResult);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting data access role. Workspace: {Workspace}, Item: {Item}, Role: {Role}.",
                options.WorkspaceId, options.ItemId, options.RoleName);
            HandleException(context, ex);
        }

        return context.Response;
    }

    public sealed record DataAccessRoleDeleteCommandResult(string RoleName, string Message);
}

