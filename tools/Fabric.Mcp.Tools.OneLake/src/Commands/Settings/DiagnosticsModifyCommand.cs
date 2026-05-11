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
        Modify the diagnostic logging configuration for OneLake at the workspace
        scope. Replaces the existing diagnostics block; fetch with
        onelake_get_settings first if you want to merge. Requires
        OneLake.ReadWrite.All.
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
        command.Options.Add(FabricOptionDefinitions.DiagnosticsConfig.AsRequired());
        command.Validators.Add(result =>
        {
            var workspaceId = result.GetValueOrDefault<string>(FabricOptionDefinitions.WorkspaceIdName);
            var workspace = result.GetValueOrDefault<string>(FabricOptionDefinitions.WorkspaceName);

            if (string.IsNullOrWhiteSpace(workspaceId) && string.IsNullOrWhiteSpace(workspace))
            {
                result.AddError("Workspace identifier is required. Provide --workspace or --workspace-id.");
            }
        });
    }

    protected override DiagnosticsModifyOptions BindOptions(ParseResult parseResult)
    {
        var options = base.BindOptions(parseResult);
        options.WorkspaceId = parseResult.GetValueOrDefault<string>(FabricOptionDefinitions.WorkspaceIdName);
        options.Workspace = parseResult.GetValueOrDefault<string>(FabricOptionDefinitions.WorkspaceName);
        options.DiagnosticsConfig = parseResult.GetValueOrDefault<string>(FabricOptionDefinitions.DiagnosticsConfigName);
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

            var result = await _oneLakeService.ModifyDiagnosticsAsync(workspaceIdentifier!, options.DiagnosticsConfig!, cancellationToken);
            context.Response.Results = ResponseResult.Create(result, OneLakeJsonContext.Default.OneLakeSettings);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error modifying OneLake diagnostics. Workspace: {Workspace}.", options.WorkspaceId ?? options.Workspace);
            HandleException(context, ex);
        }

        return context.Response;
    }
}
