// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net;
using System.Text.Json.Serialization.Metadata;
using Fabric.Mcp.Tools.Core.Models;
using Fabric.Mcp.Tools.Core.Options;
using Fabric.Mcp.Tools.Core.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Mcp.Core.Commands;
using Microsoft.Mcp.Core.Models.Command;

namespace Fabric.Mcp.Tools.Core.Commands;

/// <summary>Deletes one explicitly identified Fabric workspace and the items under it.</summary>
/// <param name="logger">The logger for sanitized operation diagnostics.</param>
/// <param name="fabricCoreService">The service that deletes the workspace.</param>
[CommandMetadata(
    Id = "694e4ea4-b867-4fb8-a7bb-0a873d2f6179",
    Name = "delete-workspace",
    Title = "Delete Fabric Workspace",
    Description = """
        Deletes one Microsoft Fabric workspace AND the items under it. This is a destructive operation.
        Requires the explicit workspace UUID; workspace names and automatic target selection are not supported.
        The caller must have the Admin workspace role; delegated callers also need Workspace.ReadWrite.All.
        Returns the targeted workspace ID and a deletion acknowledgement only after Fabric reports successful completion.
        Does not enumerate or individually delete items, retry automatically, or poll.
        """,
    OperationPlane = ToolOperationPlane.Control,
    Destructive = true,
    Idempotent = true,
    LocalRequired = false,
    OpenWorld = false,
    ReadOnly = false,
    Secret = false)]
public sealed class WorkspaceDeleteCommand(
    ILogger<WorkspaceDeleteCommand> logger,
    IFabricCoreService fabricCoreService) : AuthenticatedCommand<WorkspaceDeleteOptions, WorkspaceDeleteCommandResult>
{
    private readonly ILogger<WorkspaceDeleteCommand> _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    private readonly IFabricCoreService _fabricCoreService = fabricCoreService ?? throw new ArgumentNullException(nameof(fabricCoreService));

    /// <inheritdoc />
    public override JsonTypeInfo<WorkspaceDeleteCommandResult> ResultTypeInfo => CoreJsonContext.Default.WorkspaceDeleteCommandResult;

    /// <inheritdoc />
    public override void ValidateOptions(WorkspaceDeleteOptions options, ValidationResult validationResult)
    {
        base.ValidateOptions(options, validationResult);

        if (!Guid.TryParse(options.WorkspaceId, out var workspaceId) || workspaceId == Guid.Empty)
        {
            validationResult.Errors.Add("--workspace-id must be a nonempty UUID.");
        }
    }

    /// <inheritdoc />
    public override async Task<CommandResponse> ExecuteAsync(CommandContext context, WorkspaceDeleteOptions options, CancellationToken cancellationToken)
    {
        try
        {
            await _fabricCoreService.DeleteWorkspaceAsync(options.WorkspaceId, cancellationToken);
            SetResult(context, new(Guid.Parse(options.WorkspaceId), true));
        }
        catch (Exception ex)
        {
            _logger.LogError("Error deleting a Fabric workspace ({ExceptionType}).", ex.GetType().Name);
            HandleException(context, ex);
            context.Response.Results = null;
        }

        return context.Response;
    }

    protected override HttpStatusCode GetStatusCode(Exception ex) => ex switch
    {
        OperationCanceledException => HttpStatusCode.RequestTimeout,
        _ => base.GetStatusCode(ex)
    };

    protected override string GetErrorMessage(Exception ex) => GetStatusCode(ex) switch
    {
        HttpStatusCode.BadRequest when ex is ArgumentException => "Provide a nonempty workspace UUID",
        HttpStatusCode.BadRequest => "Fabric rejected workspace deletion. Check the workspace state and tenant restrictions before trying again. Deletion was not confirmed",
        HttpStatusCode.Unauthorized => "Authentication failed. Use an identity authorized to delete the Fabric workspace",
        HttpStatusCode.Forbidden => "Access denied. The caller needs the Admin workspace role; delegated callers also need Workspace.ReadWrite.All",
        HttpStatusCode.NotFound => "The Fabric workspace was not found. Check its ID and the caller's access. Deletion was not confirmed",
        HttpStatusCode.TooManyRequests when ex is WorkspaceDeleteThrottledException => ex.Message,
        HttpStatusCode.TooManyRequests => "Fabric throttled the request. Wait before retrying",
        HttpStatusCode.RequestTimeout => "The Fabric workspace deletion request was canceled or timed out; its outcome is not confirmed",
        HttpStatusCode.GatewayTimeout => "The Fabric workspace deletion request timed out; its outcome is not confirmed",
        HttpStatusCode.BadGateway => "Fabric returned an unexpected response; workspace deletion was not confirmed",
        _ => "Unable to confirm Fabric workspace deletion. Check the workspace state and service availability before attempting another deletion"
    };
}
