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
    Id = "a1b2c3d4-3001-4000-8000-000000000001",
    Name = "get_settings",
    Title = "Get OneLake Settings",
    Description = """
        Get the OneLake settings for a workspace — diagnostics configuration and
        immutability policy. Requires OneLake.Read.All.
        """,
    Destructive = false,
    Idempotent = true,
    LocalRequired = false,
    OpenWorld = false,
    ReadOnly = true,
    Secret = false)]
public sealed class SettingsGetCommand(ILogger<SettingsGetCommand> logger, IOneLakeService oneLakeService)
    : AuthenticatedCommand<SettingsGetOptions, OneLakeSettings>()
{
    private readonly ILogger<SettingsGetCommand> _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    private readonly IOneLakeService _oneLakeService = oneLakeService ?? throw new ArgumentNullException(nameof(oneLakeService));

    public override void ValidateOptions(SettingsGetOptions options, ValidationResult validationResult)
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

    public override async Task<CommandResponse> ExecuteAsync(CommandContext context, SettingsGetOptions options, CancellationToken cancellationToken)
    {
        var workspaceId = string.IsNullOrWhiteSpace(options.WorkspaceId) ? options.Workspace : options.WorkspaceId;
        try
        {
            var result = await _oneLakeService.GetSettingsAsync(workspaceId!, cancellationToken);
            context.Response.Results = ResponseResult.Create(result, OneLakeJsonContext.Default.OneLakeSettings);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting OneLake settings. Workspace: {Workspace}.", workspaceId);
            HandleException(context, ex);
        }

        return context.Response;
    }
}
