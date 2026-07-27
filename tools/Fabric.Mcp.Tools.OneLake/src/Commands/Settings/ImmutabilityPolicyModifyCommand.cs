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
public sealed class ImmutabilityPolicyModifyCommand(ILogger<ImmutabilityPolicyModifyCommand> logger, IOneLakeService oneLakeService)
    : AuthenticatedCommand<ImmutabilityPolicyModifyOptions, ImmutabilityPolicyModifyCommand.ImmutabilityPolicyModifyCommandResult>()
{
    private readonly ILogger<ImmutabilityPolicyModifyCommand> _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    private readonly IOneLakeService _oneLakeService = oneLakeService ?? throw new ArgumentNullException(nameof(oneLakeService));

    public override void ValidateOptions(ImmutabilityPolicyModifyOptions options, ValidationResult validationResult)
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

        if (!string.Equals(options.Scope, "DiagnosticLogs", StringComparison.OrdinalIgnoreCase))
        {
            validationResult.Errors.Add("--scope must be 'DiagnosticLogs'. No other scopes are currently supported.");
        }

        if (options.RetentionDays < 1)
        {
            validationResult.Errors.Add("--retention-days must be at least 1.");
        }
    }

    public override async Task<CommandResponse> ExecuteAsync(CommandContext context, ImmutabilityPolicyModifyOptions options, CancellationToken cancellationToken)
    {
        var workspaceId = string.IsNullOrWhiteSpace(options.WorkspaceId) ? options.Workspace : options.WorkspaceId;
        try
        {
            var policy = new ImmutabilityPolicy
            {
                Scope = options.Scope,
                RetentionDays = options.RetentionDays
            };

            await _oneLakeService.ModifyImmutabilityPolicyAsync(workspaceId!, policy, cancellationToken);
            context.Response.Results = ResponseResult.Create(new("Immutability policy modified successfully."), OneLakeJsonContext.Default.ImmutabilityPolicyModifyCommandResult);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error modifying OneLake immutability policy. Workspace: {Workspace}.", workspaceId);
            HandleException(context, ex);
        }

        return context.Response;
    }

    public sealed record ImmutabilityPolicyModifyCommandResult(string Message);
}
