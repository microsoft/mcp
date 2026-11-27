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

[CommandMetadata(
    Id = "b4b202a4-5a61-4a4c-833e-e9707d604ebe",
    Name = "assign-workspace-to-capacity",
    Title = "Assign Fabric Workspace to Capacity",
    Description = """
        Submit a request to assign an existing Microsoft Fabric workspace to a target capacity.
        Requires workspace-id and capacity-id as nonempty GUIDs, workspace Admin and capacity
        Contributor or Admin permissions, and Capacity.ReadWrite.All and Workspace.ReadWrite.All scopes.
        Returns 202 Accepted with the requested workspace and capacity IDs, accepted=true, and
        state=Pending. Acceptance does not confirm completion. Does not poll, retry, create workspaces,
        or provision or resize capacities. Inspect the workspace separately to check assignment progress.
        """,
    OperationPlane = ToolOperationPlane.Control,
    Destructive = true,
    Idempotent = false,
    LocalRequired = false,
    OpenWorld = false,
    ReadOnly = false,
    Secret = false)]
public sealed class WorkspaceAssignToCapacityCommand(
    ILogger<WorkspaceAssignToCapacityCommand> logger,
    IFabricCoreService fabricCoreService)
    : AuthenticatedCommand<WorkspaceAssignToCapacityOptions, WorkspaceAssignToCapacityCommandResult>
{
    private readonly ILogger<WorkspaceAssignToCapacityCommand> _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    private readonly IFabricCoreService _fabricCoreService = fabricCoreService ?? throw new ArgumentNullException(nameof(fabricCoreService));

    public override JsonTypeInfo<WorkspaceAssignToCapacityCommandResult> ResultTypeInfo =>
        CoreJsonContext.Default.WorkspaceAssignToCapacityCommandResult;

    public override void ValidateOptions(WorkspaceAssignToCapacityOptions options, ValidationResult validationResult)
    {
        base.ValidateOptions(options, validationResult);

        if (!Guid.TryParse(options.WorkspaceId, out var workspaceId) || workspaceId == Guid.Empty)
        {
            validationResult.Errors.Add("--workspace-id must be a nonempty GUID.");
        }

        if (!Guid.TryParse(options.CapacityId, out var capacityId) || capacityId == Guid.Empty)
        {
            validationResult.Errors.Add("--capacity-id must be a nonempty GUID.");
        }
    }

    public override async Task<CommandResponse> ExecuteAsync(
        CommandContext context, WorkspaceAssignToCapacityOptions options, CancellationToken cancellationToken)
    {
        try
        {
            var workspaceId = Guid.Parse(options.WorkspaceId);
            var capacityId = Guid.Parse(options.CapacityId);
            await _fabricCoreService.AssignWorkspaceToCapacityAsync(workspaceId, capacityId, cancellationToken);

            SetResult(context, new(workspaceId, capacityId));
            context.Response.Status = HttpStatusCode.Accepted;
            context.Response.Message = "Request accepted. Capacity assignment is pending; completion has not been verified.";
        }
        catch (Exception ex) when (ex is not OperationCanceledException || !cancellationToken.IsCancellationRequested)
        {
            _logger.LogError("Error submitting Fabric capacity assignment. Status: {Status}.", (int)GetStatusCode(ex));
            HandleException(context, ex);
        }

        return context.Response;
    }

    protected override void HandleException(CommandContext context, Exception ex)
    {
        if (ex is CommandValidationException)
        {
            base.HandleException(context, ex);
            return;
        }

        // The base handler also serializes exception messages and, in debug builds, stack traces.
        var sanitized = new WorkspaceCapacityAssignmentException(
            GetStatusCode(ex), (ex as WorkspaceCapacityAssignmentException)?.RetryAfterSeconds);
        base.HandleException(context, sanitized);
    }

    protected override string GetErrorMessage(Exception ex) =>
        ex is WorkspaceCapacityAssignmentException ? ex.Message : "Unable to submit the capacity assignment request.";

    protected override HttpStatusCode GetStatusCode(Exception ex) => ex switch
    {
        FormatException => HttpStatusCode.BadRequest,
        OperationCanceledException => HttpStatusCode.GatewayTimeout,
        _ => base.GetStatusCode(ex)
    };
}
