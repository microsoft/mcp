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

[CommandMetadata(
    Id = "8f20d389-e5e1-4dcf-84b8-6702957fc408",
    Name = "list-workspaces",
    Title = "List Fabric Workspaces",
    Description = """
        Lists one page of Microsoft Fabric workspaces accessible to the calling principal using the Core REST management API.
        Use this to discover workspace IDs and inspect workspace management metadata, optionally filtering by workspace role:
        Admin, Member, Contributor, or Viewer. Returns IDs, display names, types, available descriptions, capacity/domain details,
        tags, and continuation information. Optionally request workspace-specific API endpoints.
        Pass the continuation token with the same filters to get another page.
        Unlike onelake_list-workspaces, this is management metadata, not a OneLake storage/data-plane listing.
        It does not search catalog items, read item data or definitions, or modify resources.
        """,
    OperationPlane = ToolOperationPlane.Control,
    Destructive = false,
    Idempotent = true,
    OpenWorld = false,
    ReadOnly = true,
    Secret = false,
    LocalRequired = false)]
public sealed class WorkspaceListCommand(
    ILogger<WorkspaceListCommand> logger,
    IFabricCoreService fabricCoreService) : AuthenticatedCommand<WorkspaceListOptions, WorkspaceListCommandResult>
{
    private readonly ILogger<WorkspaceListCommand> _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    private readonly IFabricCoreService _fabricCoreService = fabricCoreService ?? throw new ArgumentNullException(nameof(fabricCoreService));

    public override JsonTypeInfo<WorkspaceListCommandResult> ResultTypeInfo => CoreJsonContext.Default.WorkspaceListCommandResult;

    public override void PostBindOptions(WorkspaceListOptions options)
    {
        base.PostBindOptions(options);
        if (WorkspaceListInputValidator.TryNormalizeRoles(options.Roles, out var normalizedRoles))
        {
            options.Roles = normalizedRoles;
        }
    }

    public override void ValidateOptions(WorkspaceListOptions options, ValidationResult validationResult)
    {
        base.ValidateOptions(options, validationResult);
        if (!WorkspaceListInputValidator.TryNormalizeRoles(options.Roles, out _))
        {
            validationResult.Errors.Add(WorkspaceListInputValidator.RolesError);
        }
        if (!WorkspaceListInputValidator.IsValidContinuationToken(options.ContinuationToken))
        {
            validationResult.Errors.Add(WorkspaceListInputValidator.ContinuationTokenError);
        }
    }

    public override async Task<CommandResponse> ExecuteAsync(CommandContext context, WorkspaceListOptions options, CancellationToken cancellationToken)
    {
        try
        {
            var page = await _fabricCoreService.ListWorkspacesAsync(
                options.Roles,
                options.ContinuationToken,
                options.PreferWorkspaceSpecificEndpoints,
                cancellationToken);

            SetResult(context, new(page.Value, page.ContinuationToken, page.ContinuationUri));
        }
        catch (Exception ex)
        {
            _logger.LogError("Error listing Fabric workspaces ({ExceptionType}).", ex.GetType().Name);
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
        HttpStatusCode.BadRequest => "Check the workspace role filter and continuation token",
        HttpStatusCode.Unauthorized => "Authentication failed. Use a configured identity that can read Fabric workspaces",
        HttpStatusCode.Forbidden => "Access denied. Check the caller's workspace access, delegated Workspace.Read.All or Workspace.ReadWrite.All scope when applicable, and Fabric tenant settings for service principals",
        HttpStatusCode.NotFound => "The Fabric workspace listing endpoint was not found",
        HttpStatusCode.TooManyRequests when ex is WorkspaceListThrottledException => ex.Message,
        HttpStatusCode.TooManyRequests => "Fabric throttled the request. Wait before retrying",
        HttpStatusCode.RequestTimeout => "The Fabric workspace request was canceled or timed out",
        HttpStatusCode.GatewayTimeout => "The Fabric workspace request timed out",
        HttpStatusCode.BadGateway => "Fabric returned an invalid workspace list response",
        _ => "Unable to list Fabric workspaces. Retry the request or check service availability"
    };
}
