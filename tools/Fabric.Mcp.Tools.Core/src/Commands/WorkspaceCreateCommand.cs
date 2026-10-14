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

/// <summary>Creates a Fabric workspace without provisioning or looking up related resources.</summary>
/// <param name="logger">The logger for sanitized operation diagnostics.</param>
/// <param name="fabricCoreService">The service that creates the workspace.</param>
[CommandMetadata(
    Id = "26e554fc-d05b-4e71-af64-12186c262080",
    Name = "create-workspace",
    Title = "Create Fabric Workspace",
    Description = """
        Creates a new Microsoft Fabric workspace with a required display name and optional description.
        Optionally assign an existing capacity and domain by UUID in the same creation request.
        Returns the created workspace's management metadata and the Location header when supplied.
        Requires Fabric workspace-creation permission, capacity contributor/admin access when assigning a capacity,
        and domain assignment permission when assigning a domain; delegated callers need Workspace.ReadWrite.All.
        Does not provision capacities or domains, resolve names, create items, or poll capacity assignment.
        Creation is not idempotent and is never automatically retried. If the outcome is uncertain,
        check whether the workspace was created before making another creation request.
        """,
    OperationPlane = ToolOperationPlane.Control,
    Destructive = false,
    Idempotent = false,
    LocalRequired = false,
    OpenWorld = false,
    ReadOnly = false,
    Secret = false)]
public sealed class WorkspaceCreateCommand(
    ILogger<WorkspaceCreateCommand> logger,
    IFabricCoreService fabricCoreService) : FabricCoreCommand<WorkspaceCreateOptions, WorkspaceCreateResult>
{
    private readonly ILogger<WorkspaceCreateCommand> _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    private readonly IFabricCoreService _fabricCoreService = fabricCoreService ?? throw new ArgumentNullException(nameof(fabricCoreService));
    private const string UncertainOutcomeMessage = "The workspace may already have been created. Check before making another creation request";

    /// <inheritdoc />
    public override JsonTypeInfo<WorkspaceCreateResult> ResultTypeInfo => CoreJsonContext.Default.WorkspaceCreateResult;

    /// <inheritdoc />
    public override void ValidateOptions(WorkspaceCreateOptions options, ValidationResult validationResult)
    {
        base.ValidateOptions(options, validationResult);
        foreach (var error in WorkspaceCreateInputValidator.GetErrors(
            options.DisplayName, options.Description, options.CapacityId, options.DomainId))
        {
            validationResult.Errors.Add(error);
        }
    }

    /// <inheritdoc />
    public override async Task<CommandResponse> ExecuteAsync(CommandContext context, WorkspaceCreateOptions options, CancellationToken cancellationToken)
    {
        try
        {
            var result = await _fabricCoreService.CreateWorkspaceAsync(
                new CreateWorkspaceRequest
                {
                    DisplayName = options.DisplayName,
                    Description = options.Description,
                    CapacityId = options.CapacityId,
                    DomainId = options.DomainId
                },
                cancellationToken);

            SetResult(context, result);
            context.Response.Status = HttpStatusCode.Created;
        }
        catch (Exception ex)
        {
            _logger.LogError("Error creating a Fabric workspace ({ExceptionType}).", ex.GetType().Name);
            HandleException(context, ex);
        }

        return context.Response;
    }

    protected override HttpStatusCode GetStatusCode(Exception ex) => ex switch
    {
        OperationCanceledException => HttpStatusCode.RequestTimeout,
        JsonException => HttpStatusCode.BadGateway,
        _ => base.GetStatusCode(ex)
    };

    protected override string GetErrorMessage(Exception ex)
    {
        var message = GetStatusCode(ex) switch
        {
            HttpStatusCode.BadRequest => "Check the display name and any supplied description, capacity UUID, or domain UUID. Verify the name is unused",
            HttpStatusCode.Unauthorized => "Authentication failed. Use an identity authorized to create Fabric workspaces; delegated callers need Workspace.ReadWrite.All",
            HttpStatusCode.Forbidden => "Access denied. Check Fabric workspace-creation permission, the service-principal workspace-creation tenant setting when applicable, capacity contributor/admin access, and domain assignment permission",
            HttpStatusCode.NotFound => "Fabric could not find a requested resource. Check the supplied capacity/domain IDs and the caller's access",
            HttpStatusCode.Conflict => "Fabric rejected workspace creation because of a conflict. Check whether the workspace name is already in use",
            HttpStatusCode.TooManyRequests => "Fabric throttled workspace creation. Wait before making further requests; creation is not automatically retried",
            HttpStatusCode.RequestTimeout or HttpStatusCode.GatewayTimeout => $"Workspace creation was canceled or timed out. {UncertainOutcomeMessage}",
            HttpStatusCode.BadGateway => $"Fabric returned an invalid or unexpected workspace creation response. {UncertainOutcomeMessage}",
            _ => $"Unable to confirm Fabric workspace creation. {UncertainOutcomeMessage}"
        };

        if (ex is WorkspaceCreateRequestException { RetryAfter: { } retryAfter })
        {
            message += $". Retry-After: {retryAfter}";
        }

        return message;
    }
}
