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
    Id = "a1b2c3d4-3001-4000-8000-000000000003",
    Name = "modify_immutability_policy",
    Title = "Modify OneLake Immutability Policy",
    Description = """
        Modify the workspace-level OneLake immutability policy. Once enabled,
        immutability cannot be disabled — confirm with the user before applying.
        Requires OneLake.ReadWrite.All.
        """,
    Destructive = false,
    Idempotent = true,
    LocalRequired = false,
    OpenWorld = false,
    ReadOnly = false,
    Secret = false)]
public sealed class ImmutabilityPolicyModifyCommand(
    ILogger<ImmutabilityPolicyModifyCommand> logger,
    IOneLakeService oneLakeService) : GlobalCommand<ImmutabilityPolicyModifyOptions>()
{
    private readonly ILogger<ImmutabilityPolicyModifyCommand> _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    private readonly IOneLakeService _oneLakeService = oneLakeService ?? throw new ArgumentNullException(nameof(oneLakeService));

    protected override void RegisterOptions(Command command)
    {
        base.RegisterOptions(command);
        command.Options.Add(FabricOptionDefinitions.WorkspaceId.AsOptional());
        command.Options.Add(FabricOptionDefinitions.Workspace.AsOptional());
        command.Options.Add(FabricOptionDefinitions.ImmutabilityPolicyConfig.AsRequired());
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

    protected override ImmutabilityPolicyModifyOptions BindOptions(ParseResult parseResult)
    {
        var options = base.BindOptions(parseResult);
        options.WorkspaceId = parseResult.GetValueOrDefault<string>(FabricOptionDefinitions.WorkspaceIdName);
        options.Workspace = parseResult.GetValueOrDefault<string>(FabricOptionDefinitions.WorkspaceName);
        options.ImmutabilityPolicyConfig = parseResult.GetValueOrDefault<string>(FabricOptionDefinitions.ImmutabilityPolicyConfigName);
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

            var result = await _oneLakeService.ModifyImmutabilityPolicyAsync(workspaceIdentifier!, options.ImmutabilityPolicyConfig!, cancellationToken);
            context.Response.Results = ResponseResult.Create(result, OneLakeJsonContext.Default.OneLakeSettings);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error modifying OneLake immutability policy. Workspace: {Workspace}.", options.WorkspaceId ?? options.Workspace);
            HandleException(context, ex);
        }

        return context.Response;
    }
}
