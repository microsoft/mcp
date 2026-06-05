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
        Retention days cannot be reduced below the current value. Requires
        OneLake.ReadWrite.All. Caller must be a workspace Admin.
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
        command.Options.Add(FabricOptionDefinitions.ImmutabilityScope.AsRequired());
        command.Options.Add(FabricOptionDefinitions.RetentionDays.AsRequired());
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

            var scope = result.GetValueOrDefault<string>(FabricOptionDefinitions.ImmutabilityScope.Name);
            if (!string.Equals(scope, "DiagnosticLogs", StringComparison.OrdinalIgnoreCase))
            {
                result.AddError("--scope must be 'DiagnosticLogs'. No other scopes are currently supported.");
            }

            var retentionDays = result.GetValueOrDefault<int>(FabricOptionDefinitions.RetentionDays.Name);
            if (retentionDays < 1)
            {
                result.AddError("--retention-days must be at least 1.");
            }
        });
    }

    protected override ImmutabilityPolicyModifyOptions BindOptions(ParseResult parseResult)
    {
        var options = base.BindOptions(parseResult);
        var workspaceId = parseResult.GetValueOrDefault<string>(FabricOptionDefinitions.WorkspaceId.Name);
        var workspace = parseResult.GetValueOrDefault<string>(FabricOptionDefinitions.Workspace.Name);
        options.WorkspaceId = !string.IsNullOrWhiteSpace(workspaceId) ? workspaceId! : workspace ?? string.Empty;
        options.Scope = parseResult.GetValueOrDefault<string>(FabricOptionDefinitions.ImmutabilityScope.Name);
        options.RetentionDays = parseResult.GetValueOrDefault<int>(FabricOptionDefinitions.RetentionDays.Name);
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
            var policy = new ImmutabilityPolicy
            {
                Scope = options.Scope,
                RetentionDays = options.RetentionDays
            };

            await _oneLakeService.ModifyImmutabilityPolicyAsync(options.WorkspaceId!, policy, cancellationToken);
            context.Response.Results = ResponseResult.Create(new("Immutability policy modified successfully."), OneLakeJsonContext.Default.ImmutabilityPolicyModifyCommandResult);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error modifying OneLake immutability policy. Workspace: {Workspace}.", options.WorkspaceId);
            HandleException(context, ex);
        }

        return context.Response;
    }

    public sealed record ImmutabilityPolicyModifyCommandResult(string Message);
}

