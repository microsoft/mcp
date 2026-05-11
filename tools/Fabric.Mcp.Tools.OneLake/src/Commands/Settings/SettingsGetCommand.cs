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
    Id = "a1b2c3d4-3001-4000-8000-000000000001",
    Name = "get_settings",
    Title = "Get OneLake Settings",
    Description = """
        Get the OneLake settings for a workspace — diagnostics configuration and
        immutability policy. Read-only; for changes use onelake_modify_diagnostics
        or onelake_modify_immutability_policy. Requires OneLake.Read.All.
        """,
    Destructive = false,
    Idempotent = true,
    LocalRequired = false,
    OpenWorld = false,
    ReadOnly = true,
    Secret = false)]
public sealed class SettingsGetCommand(
    ILogger<SettingsGetCommand> logger,
    IOneLakeService oneLakeService) : GlobalCommand<SettingsGetOptions>()
{
    private readonly ILogger<SettingsGetCommand> _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    private readonly IOneLakeService _oneLakeService = oneLakeService ?? throw new ArgumentNullException(nameof(oneLakeService));

    protected override void RegisterOptions(Command command)
    {
        base.RegisterOptions(command);
        command.Options.Add(FabricOptionDefinitions.WorkspaceId.AsOptional());
        command.Options.Add(FabricOptionDefinitions.Workspace.AsOptional());
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

    protected override SettingsGetOptions BindOptions(ParseResult parseResult)
    {
        var options = base.BindOptions(parseResult);
        options.WorkspaceId = parseResult.GetValueOrDefault<string>(FabricOptionDefinitions.WorkspaceIdName);
        options.Workspace = parseResult.GetValueOrDefault<string>(FabricOptionDefinitions.WorkspaceName);
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

            var result = await _oneLakeService.GetSettingsAsync(workspaceIdentifier!, cancellationToken);
            context.Response.Results = ResponseResult.Create(result, OneLakeJsonContext.Default.OneLakeSettings);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting OneLake settings. Workspace: {Workspace}.", options.WorkspaceId ?? options.Workspace);
            HandleException(context, ex);
        }

        return context.Response;
    }
}
