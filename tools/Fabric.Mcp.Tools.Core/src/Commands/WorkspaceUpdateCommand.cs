// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net;
using System.Text.Json;
using System.Text.Json.Serialization.Metadata;
using Fabric.Mcp.Tools.Core.Models;
using Fabric.Mcp.Tools.Core.Options;
using Fabric.Mcp.Tools.Core.Services;
using Fabric.Mcp.Tools.Core.Validation;
using Microsoft.Extensions.Logging;
using Microsoft.Mcp.Core.Commands;
using Microsoft.Mcp.Core.Models.Command;

namespace Fabric.Mcp.Tools.Core.Commands;

/// <summary>Updates only a Fabric workspace's display name and description.</summary>
/// <param name="logger">The logger for sanitized operation diagnostics.</param>
/// <param name="fabricCoreService">The service that updates workspace metadata.</param>
[CommandMetadata(
    Id = "aaed882f-746f-472c-b50f-b401f8994cb7",
    Name = "update-workspace",
    Title = "Update Fabric Workspace",
    Description = """
        Updates or renames an existing Microsoft Fabric workspace using its required workspace UUID.
        Supply a new display name, a new description, or both. Omitted properties remain unchanged;
        an empty description clears it. Display names are limited to 256 characters and descriptions
        to 4000 characters. Workspace names must be unique within the tenant; Admin monitoring is reserved.
        Returns the workspace ID, display name, type, and description when available.
        Requires the workspace Admin role and, for delegated callers, Workspace.ReadWrite.All.
        Does not change capacity, domain, identity, permissions, tags, endpoints, items, or data.
        Does not resolve names, fetch additional metadata, poll, or retry automatically.
        """,
    OperationPlane = ToolOperationPlane.Control,
    Destructive = true,
    Idempotent = true,
    LocalRequired = false,
    OpenWorld = false,
    ReadOnly = false,
    Secret = false)]
public sealed class WorkspaceUpdateCommand(
    ILogger<WorkspaceUpdateCommand> logger,
    IFabricCoreService fabricCoreService) : AuthenticatedCommand<WorkspaceUpdateOptions, WorkspaceUpdateCommandResult>
{
    private readonly ILogger<WorkspaceUpdateCommand> _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    private readonly IFabricCoreService _fabricCoreService = fabricCoreService ?? throw new ArgumentNullException(nameof(fabricCoreService));

    /// <inheritdoc />
    public override JsonTypeInfo<WorkspaceUpdateCommandResult> ResultTypeInfo => CoreJsonContext.Default.WorkspaceUpdateCommandResult;

    /// <inheritdoc />
    public override void ValidateOptions(WorkspaceUpdateOptions options, ValidationResult validationResult)
    {
        base.ValidateOptions(options, validationResult);

        foreach (var error in WorkspaceUpdateInputValidator.GetErrors(options.WorkspaceId, options.DisplayName, options.Description))
        {
            validationResult.AddError(error, error);
        }
    }

    /// <inheritdoc />
    public override async Task<CommandResponse> ExecuteAsync(CommandContext context, WorkspaceUpdateOptions options, CancellationToken cancellationToken)
    {
        try
        {
            var workspace = await _fabricCoreService.UpdateWorkspaceAsync(
                options.WorkspaceId,
                new UpdateWorkspaceRequest { DisplayName = options.DisplayName, Description = options.Description },
                cancellationToken);

            SetResult(context, new(workspace));
        }
        catch (Exception ex)
        {
            _logger.LogError("Error updating Fabric workspace metadata ({ExceptionType}).", ex.GetType().Name);
            HandleException(context, ex);
            context.Response.Results = null;
        }

        return context.Response;
    }

    protected override HttpStatusCode GetStatusCode(Exception ex) => ex switch
    {
        OperationCanceledException => HttpStatusCode.RequestTimeout,
        JsonException => HttpStatusCode.BadGateway,
        _ => base.GetStatusCode(ex)
    };

    protected override string GetErrorMessage(Exception ex) => GetStatusCode(ex) switch
    {
        HttpStatusCode.BadRequest => "The update was rejected. Check the workspace UUID, supplied properties, and workspace naming rules",
        HttpStatusCode.Unauthorized => "Authentication failed. Use an identity authorized to update the Fabric workspace",
        HttpStatusCode.Forbidden => "Access denied. The caller needs the workspace Admin role; delegated callers also need Workspace.ReadWrite.All",
        HttpStatusCode.NotFound => "The Fabric workspace was not found. Check its ID and the caller's access",
        HttpStatusCode.Conflict => "The update conflicts with the workspace state or an existing workspace name. Workspace names must be unique within the tenant",
        HttpStatusCode.TooManyRequests when ex is WorkspaceUpdateThrottledException => ex.Message,
        HttpStatusCode.TooManyRequests => "Fabric throttled the update. Wait before retrying",
        HttpStatusCode.RequestTimeout or HttpStatusCode.GatewayTimeout => "The workspace update was canceled or timed out. It may have been applied; verify the workspace before retrying",
        HttpStatusCode.BadGateway => "Fabric returned an invalid workspace update response. The update may have been applied",
        _ => "Unable to complete the workspace update. Check service availability and verify the workspace before retrying"
    };
}
