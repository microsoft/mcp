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
        command.Options.Add(FabricOptionDefinitions.ItemId.AsOptional());
        command.Options.Add(FabricOptionDefinitions.Item.AsOptional());
        command.Options.Add(FabricOptionDefinitions.RoleName.AsRequired());
        command.Validators.Add(result =>
        {
            var workspaceId = result.GetValueOrDefault<string>(FabricOptionDefinitions.WorkspaceId.Name);
            var workspace = result.GetValueOrDefault<string>(FabricOptionDefinitions.Workspace.Name);
            var itemId = result.GetValueOrDefault<string>(FabricOptionDefinitions.ItemId.Name);
            var item = result.GetValueOrDefault<string>(FabricOptionDefinitions.Item.Name);

            if (string.IsNullOrWhiteSpace(workspaceId) && string.IsNullOrWhiteSpace(workspace))
            {
                result.AddError("Workspace identifier is required. Provide --workspace or --workspace-id.");
            }

            if (string.IsNullOrWhiteSpace(item) && string.IsNullOrWhiteSpace(itemId))
            {
                result.AddError("Item identifier is required. Provide --item or --item-id.");
            }
        });
    }

    protected override DataAccessRoleDeleteOptions BindOptions(ParseResult parseResult)
    {
        var options = base.BindOptions(parseResult);
        options.WorkspaceId = parseResult.GetValueOrDefault<string>(FabricOptionDefinitions.WorkspaceId.Name);
        options.Workspace = parseResult.GetValueOrDefault<string>(FabricOptionDefinitions.Workspace.Name);
        options.ItemId = parseResult.GetValueOrDefault<string>(FabricOptionDefinitions.ItemId.Name);
        options.Item = parseResult.GetValueOrDefault<string>(FabricOptionDefinitions.Item.Name);
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
            var workspaceIdentifier = !string.IsNullOrWhiteSpace(options.WorkspaceId)
                ? options.WorkspaceId
                : options.Workspace;

            var itemIdentifier = !string.IsNullOrWhiteSpace(options.ItemId)
                ? options.ItemId
                : options.Item;

            await _oneLakeService.DeleteDataAccessRoleAsync(workspaceIdentifier!, itemIdentifier!, options.RoleName!, cancellationToken);
            var result = new DataAccessRoleDeleteCommandResult(options.RoleName!, "Data access role deleted successfully.");
            context.Response.Results = ResponseResult.Create(result, OneLakeJsonContext.Default.DataAccessRoleDeleteCommandResult);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting data access role. Workspace: {Workspace}, Item: {Item}, Role: {Role}.",
                options.WorkspaceId ?? options.Workspace, options.ItemId ?? options.Item, options.RoleName);
            HandleException(context, ex);
        }

        return context.Response;
    }

    public sealed class DataAccessRoleDeleteCommandResult
    {
        public string RoleName { get; init; } = string.Empty;
        public string Message { get; init; } = string.Empty;

        public DataAccessRoleDeleteCommandResult()
        {
        }

        public DataAccessRoleDeleteCommandResult(string roleName, string message)
        {
            RoleName = roleName;
            Message = message;
        }
    }
}
