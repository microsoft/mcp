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

/// <summary>Retrieves metadata for one existing Fabric capacity.</summary>
/// <param name="logger">The logger for sanitized operation diagnostics.</param>
/// <param name="fabricCoreService">The service that retrieves capacity metadata.</param>
[CommandMetadata(
    Id = "b7c574c4-4213-4dc0-95a7-435dc33ed306",
    Name = "get-capacity",
    Title = "Get Fabric Capacity",
    Description = """
        Gets metadata for one existing Microsoft Fabric capacity using its required capacity UUID.
        Use this to inspect a known capacity's ID, display name, SKU, region, and state.
        Requires Administrator or Contributor permission on the capacity.
        Does not list capacities, read billing or item data, modify resources, assign workspaces,
        or poll long-running operations.
        """,
    OperationPlane = ToolOperationPlane.Control,
    Destructive = false,
    Idempotent = true,
    LocalRequired = false,
    OpenWorld = false,
    ReadOnly = true,
    Secret = false)]
public sealed class CapacityGetCommand(
    ILogger<CapacityGetCommand> logger,
    IFabricCoreService fabricCoreService) : AuthenticatedCommand<CapacityGetOptions, CapacityGetCommandResult>
{
    private readonly ILogger<CapacityGetCommand> _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    private readonly IFabricCoreService _fabricCoreService = fabricCoreService ?? throw new ArgumentNullException(nameof(fabricCoreService));

    /// <inheritdoc />
    public override JsonTypeInfo<CapacityGetCommandResult> ResultTypeInfo => CoreJsonContext.Default.CapacityGetCommandResult;

    /// <inheritdoc />
    public override void ValidateOptions(CapacityGetOptions options, ValidationResult validationResult)
    {
        base.ValidateOptions(options, validationResult);

        if (!Guid.TryParse(options.CapacityId, out var capacityId) || capacityId == Guid.Empty)
        {
            validationResult.AddError("--capacity-id must be a nonempty UUID.", "Invalid capacity ID.");
        }
    }

    /// <inheritdoc />
    public override async Task<CommandResponse> ExecuteAsync(CommandContext context, CapacityGetOptions options, CancellationToken cancellationToken)
    {
        try
        {
            var capacity = await _fabricCoreService.GetCapacityAsync(options.CapacityId, cancellationToken);
            SetResult(context, new(capacity));
        }
        catch (Exception ex)
        {
            _logger.LogError("Error retrieving Fabric capacity metadata ({ExceptionType}).", ex.GetType().Name);
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
        HttpStatusCode.BadRequest => "Provide a valid capacity UUID",
        HttpStatusCode.Unauthorized => "Authentication failed. Use an identity authorized to read the Fabric capacity",
        HttpStatusCode.Forbidden => "Access denied. The caller needs Administrator or Contributor permission on the capacity; delegated callers also need Capacity.Read.All or Capacity.ReadWrite.All",
        HttpStatusCode.NotFound => "The Fabric capacity was not found. Check its ID and the caller's access",
        HttpStatusCode.TooManyRequests when ex is FabricCapacityGetThrottledException => ex.Message,
        HttpStatusCode.TooManyRequests => "Fabric throttled the request. Wait before retrying",
        HttpStatusCode.RequestTimeout => "The Fabric capacity request was canceled or timed out",
        HttpStatusCode.GatewayTimeout => "The Fabric capacity request timed out",
        HttpStatusCode.BadGateway => "Fabric returned an invalid capacity metadata response",
        _ => "Unable to retrieve Fabric capacity metadata. Retry the request or check service availability"
    };
}
