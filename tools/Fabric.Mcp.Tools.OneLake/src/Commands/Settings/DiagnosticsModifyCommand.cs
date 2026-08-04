// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Fabric.Mcp.Tools.OneLake.Models;
using Fabric.Mcp.Tools.OneLake.Options;
using Fabric.Mcp.Tools.OneLake.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Mcp.Core.Commands;
using Microsoft.Mcp.Core.Models.Command;

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
public sealed class DiagnosticsModifyCommand(ILogger<DiagnosticsModifyCommand> logger, IOneLakeService oneLakeService)
    : AuthenticatedCommand<DiagnosticsModifyOptions, DiagnosticsModifyCommand.DiagnosticsModifyCommandResult>()
{
    private readonly ILogger<DiagnosticsModifyCommand> _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    private readonly IOneLakeService _oneLakeService = oneLakeService ?? throw new ArgumentNullException(nameof(oneLakeService));

    public override void ValidateOptions(DiagnosticsModifyOptions options, ValidationResult validationResult)
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

        if (!string.Equals(options.Status, "Enabled", StringComparison.OrdinalIgnoreCase) &&
            !string.Equals(options.Status, "Disabled", StringComparison.OrdinalIgnoreCase))
        {
            validationResult.Errors.Add("--status must be 'Enabled' or 'Disabled'.");
        }

        if (string.Equals(options.Status, "Enabled", StringComparison.OrdinalIgnoreCase))
        {
            if (string.IsNullOrWhiteSpace(options.DestinationLakehouseWorkspaceId))
            {
                validationResult.Errors.Add("--destination-lakehouse-workspace-id is required when --status is Enabled.");
            }
            else if (!Guid.TryParse(options.DestinationLakehouseWorkspaceId, out _))
            {
                validationResult.Errors.Add("--destination-lakehouse-workspace-id must be a valid GUID.");
            }

            if (string.IsNullOrWhiteSpace(options.DestinationLakehouseItemId))
            {
                validationResult.Errors.Add("--destination-lakehouse-item-id is required when --status is Enabled.");
            }
            else if (!Guid.TryParse(options.DestinationLakehouseItemId, out _))
            {
                validationResult.Errors.Add("--destination-lakehouse-item-id must be a valid GUID.");
            }
        }
        else if (string.Equals(options.Status, "Disabled", StringComparison.OrdinalIgnoreCase))
        {
            if (!string.IsNullOrWhiteSpace(options.DestinationLakehouseWorkspaceId) ||
                !string.IsNullOrWhiteSpace(options.DestinationLakehouseItemId))
            {
                validationResult.Errors.Add("Destination options must be omitted when --status is Disabled.");
            }
        }
    }

    public override async Task<CommandResponse> ExecuteAsync(CommandContext context, DiagnosticsModifyOptions options, CancellationToken cancellationToken)
    {
        var workspaceId = string.IsNullOrWhiteSpace(options.WorkspaceId) ? options.Workspace : options.WorkspaceId;
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

            await _oneLakeService.ModifyDiagnosticsAsync(workspaceId!, settings, cancellationToken);
            context.Response.Results = ResponseResult.Create(new("Diagnostics settings modified successfully."), OneLakeJsonContext.Default.DiagnosticsModifyCommandResult);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error modifying OneLake diagnostics. Workspace: {Workspace}.", workspaceId);
            HandleException(context, ex);
        }

        return context.Response;
    }

    public sealed record DiagnosticsModifyCommandResult(string Message);
}
