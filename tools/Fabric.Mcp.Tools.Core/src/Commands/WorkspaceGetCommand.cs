// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net;
using System.Text.Json;
using System.Text.Json.Serialization.Metadata;
using Fabric.Mcp.Tools.Core.Models;
using Fabric.Mcp.Tools.Core.Options;
using Fabric.Mcp.Tools.Core.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Mcp.Core.Commands;
using Microsoft.Mcp.Core.Models.Command;

namespace Fabric.Mcp.Tools.Core.Commands;

/// <summary>Retrieves metadata for one Fabric workspace without reading its items or data.</summary>
/// <param name="logger">The logger for sanitized operation diagnostics.</param>
/// <param name="fabricCoreService">The service that retrieves and validates workspace metadata.</param>
[CommandMetadata(
    Id = "d331817d-fbda-49dc-97b6-c3d6bed8d125",
    Name = "get-workspace",
    Title = "Get Fabric Workspace",
    Description = """
        Gets metadata for one existing Microsoft Fabric workspace using its required workspace UUID.
        Use this to inspect a known workspace's ID, display name, type, description, capacity, domain,
        workspace identity identifiers, and applied tags when available.
        Optionally request workspace-specific API and OneLake endpoints; returned endpoints are metadata only.
        Does not list workspaces or items, read item data or definitions, modify resources, or poll capacity assignments.
        """,
    OperationPlane = ToolOperationPlane.Control,
    Destructive = false,
    Idempotent = true,
    LocalRequired = false,
    OpenWorld = false,
    ReadOnly = true,
    Secret = false)]
public sealed class WorkspaceGetCommand(
    ILogger<WorkspaceGetCommand> logger,
    IFabricCoreService fabricCoreService) : AuthenticatedCommand<WorkspaceGetOptions, WorkspaceGetCommandResult>
{
    private readonly ILogger<WorkspaceGetCommand> _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    private readonly IFabricCoreService _fabricCoreService = fabricCoreService ?? throw new ArgumentNullException(nameof(fabricCoreService));

    /// <inheritdoc />
    public override JsonTypeInfo<WorkspaceGetCommandResult> ResultTypeInfo => CoreJsonContext.Default.WorkspaceGetCommandResult;

    /// <inheritdoc />
    public override void ValidateOptions(WorkspaceGetOptions options, ValidationResult validationResult)
    {
        base.ValidateOptions(options, validationResult);

        if (!Guid.TryParse(options.WorkspaceId, out var workspaceId) || workspaceId == Guid.Empty)
        {
            validationResult.Errors.Add("--workspace-id must be a nonempty UUID.");
        }
    }

    /// <inheritdoc />
    public override async Task<CommandResponse> ExecuteAsync(CommandContext context, WorkspaceGetOptions options, CancellationToken cancellationToken)
    {
        try
        {
            var workspace = await _fabricCoreService.GetWorkspaceAsync(
                options.WorkspaceId,
                options.PreferWorkspaceSpecificEndpoints,
                cancellationToken);

            SetResult(context, new(workspace));
        }
        catch (Exception ex)
        {
            _logger.LogError("Error retrieving Fabric workspace metadata ({ExceptionType}).", ex.GetType().Name);
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
        HttpStatusCode.BadRequest => "Provide a valid workspace UUID and a Boolean endpoint preference when specified",
        HttpStatusCode.Unauthorized => "Authentication failed. Use an identity authorized to read the Fabric workspace",
        HttpStatusCode.Forbidden => "Access denied. The caller needs Viewer or higher workspace access; delegated callers also need Workspace.Read.All or Workspace.ReadWrite.All",
        HttpStatusCode.NotFound => "The Fabric workspace was not found. Check its ID and the caller's access",
        HttpStatusCode.TooManyRequests when ex is FabricWorkspaceThrottledException => ex.Message,
        HttpStatusCode.TooManyRequests => "Fabric throttled the request. Wait before retrying",
        HttpStatusCode.RequestTimeout => "The Fabric workspace request was canceled or timed out",
        HttpStatusCode.GatewayTimeout => "The Fabric workspace request timed out",
        HttpStatusCode.BadGateway => "Fabric returned an invalid workspace metadata response",
        _ => "Unable to retrieve Fabric workspace metadata. Retry the request or check service availability"
    };
}
