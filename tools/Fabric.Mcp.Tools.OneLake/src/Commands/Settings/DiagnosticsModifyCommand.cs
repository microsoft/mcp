// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Fabric.Mcp.Tools.OneLake.Models;
using Fabric.Mcp.Tools.OneLake.Options;
using Fabric.Mcp.Tools.OneLake.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Mcp.Core.Commands;
using Microsoft.Mcp.Core.Extensions;
using Microsoft.Mcp.Core.Models.Option;

namespace Fabric.Mcp.Tools.OneLake.Commands.Settings;

[CommandMetadata(
    Id = "a1b2c3d4-3001-4000-8000-000000000002",
    Name = "modify_diagnostics",
    Title = "Modify OneLake Diagnostics",
    Description = """
        Enable or disable workspace-level OneLake diagnostic logging. When enabling,
        specify the destination lakehouse where logs will be stored. When disabling,
        destination options must be omitted. This is an LRO — the server may return
        202 Accepted. Requires OneLake.ReadWrite.All. Caller must be a workspace Admin
        on the source workspace and Contributor+ on the destination workspace.
        """,
    Destructive = false,
    Idempotent = true,
    LocalRequired = false,
    OpenWorld = false,
    ReadOnly = false,
    Secret = false)]
public sealed class DiagnosticsModifyCommand(
    ILogger<DiagnosticsModifyCommand> logger,
    IOneLakeService oneLakeService) : GlobalCommand<DiagnosticsModifyOptions>()
{
    private readonly ILogger<DiagnosticsModifyCommand> _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    private readonly IOneLakeService _oneLakeService = oneLakeService ?? throw new ArgumentNullException(nameof(oneLakeService));

    protected override void RegisterOptions(Command command)
    {
        base.RegisterOptions(command);
        command.Options.Add(FabricOptionDefinitions.WorkspaceId.AsOptional());
        command.Options.Add(FabricOptionDefinitions.Workspace.AsOptional());
        command.Options.Add(FabricOptionDefinitions.DiagnosticsStatus.AsRequired());
        command.Options.Add(FabricOptionDefinitions.DestinationLakehouseWorkspaceId.AsOptional());
        command.Options.Add(FabricOptionDefinitions.DestinationLakehouseItemId.AsOptional());
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

            var status = result.GetValueOrDefault<string>(FabricOptionDefinitions.DiagnosticsStatus.Name);
            if (!string.Equals(status, "Enabled", StringComparison.OrdinalIgnoreCase) &&
                !string.Equals(status, "Disabled", StringComparison.OrdinalIgnoreCase))
            {
                result.AddError("--status must be 'Enabled' or 'Disabled'.");
            }

            var destWorkspaceId = result.GetValueOrDefault<string>(FabricOptionDefinitions.DestinationLakehouseWorkspaceId.Name);
            var destItemId = result.GetValueOrDefault<string>(FabricOptionDefinitions.DestinationLakehouseItemId.Name);

            if (string.Equals(status, "Enabled", StringComparison.OrdinalIgnoreCase))
            {
                if (string.IsNullOrWhiteSpace(destWorkspaceId))
                    result.AddError("--destination-lakehouse-workspace-id is required when --status is Enabled.");
                else if (!Guid.TryParse(destWorkspaceId, out _))
                    result.AddError("--destination-lakehouse-workspace-id must be a valid GUID.");

                if (string.IsNullOrWhiteSpace(destItemId))
                    result.AddError("--destination-lakehouse-item-id is required when --status is Enabled.");
                else if (!Guid.TryParse(destItemId, out _))
                    result.AddError("--destination-lakehouse-item-id must be a valid GUID.");
            }
            else if (string.Equals(status, "Disabled", StringComparison.OrdinalIgnoreCase))
            {
                if (!string.IsNullOrWhiteSpace(destWorkspaceId) || !string.IsNullOrWhiteSpace(destItemId))
                    result.AddError("Destination options must be omitted when --status is Disabled.");
            }
        });
    }

    protected override DiagnosticsModifyOptions BindOptions(ParseResult parseResult)
    {
        var options = base.BindOptions(parseResult);
        var workspaceId = parseResult.GetValueOrDefault<string>(FabricOptionDefinitions.WorkspaceId.Name);
        var workspace = parseResult.GetValueOrDefault<string>(FabricOptionDefinitions.Workspace.Name);
        options.WorkspaceId = !string.IsNullOrWhiteSpace(workspaceId) ? workspaceId! : workspace ?? string.Empty;
        options.Status = parseResult.GetValueOrDefault<string>(FabricOptionDefinitions.DiagnosticsStatus.Name);
        options.DestinationLakehouseWorkspaceId = parseResult.GetValueOrDefault<string>(FabricOptionDefinitions.DestinationLakehouseWorkspaceId.Name);
        options.DestinationLakehouseItemId = parseResult.GetValueOrDefault<string>(FabricOptionDefinitions.DestinationLakehouseItemId.Name);
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
            var settings = new OneLakeDiagnosticSettings { Status = options.Status };
            if (string.Equals(options.Status, "Enabled", StringComparison.OrdinalIgnoreCase))
            {
                settings.Destination = new LakehouseDiagnosticDestination
                {
                    Lakehouse = new ItemReferenceById
                    {
                        ItemId = options.DestinationLakehouseItemId,
                        WorkspaceId = options.DestinationLakehouseWorkspaceId
                    }
                };
            }

            await _oneLakeService.ModifyDiagnosticsAsync(options.WorkspaceId!, settings, cancellationToken);
            context.Response.Results = ResponseResult.Create(new("Diagnostics settings modified successfully."), OneLakeJsonContext.Default.DiagnosticsModifyCommandResult);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error modifying OneLake diagnostics. Workspace: {Workspace}.", options.WorkspaceId);
            HandleException(context, ex);
        }

        return context.Response;
    }

    public sealed record DiagnosticsModifyCommandResult(string Message);
}

