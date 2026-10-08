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

/// <summary>Lists one page of Fabric capacities accessible to the calling principal.</summary>
[CommandMetadata(
    Id = "4ea4af95-1947-423b-bec6-a0357380485e",
    Name = "list-capacities",
    Title = "List Fabric Capacities",
    Description = """
        Lists one page of Microsoft Fabric capacities where the calling principal is an administrator or contributor.
        Use this to discover accessible capacity IDs, display names, SKUs, regions, and states.
        Returns capacity metadata and available continuationToken and continuationUri values.
        To request another page, pass the returned continuation token unchanged; omit it for the first page.
        Supports users, service principals, and managed identities. Delegated calls require Capacity.Read.All or Capacity.ReadWrite.All.
        This is the Fabric Core API, not an Azure subscription inventory or a capacity utilization report.
        Does not automatically retrieve additional pages, follow returned URIs, or modify capacities.
        """,
    OperationPlane = ToolOperationPlane.Control,
    Destructive = false,
    Idempotent = true,
    LocalRequired = false,
    OpenWorld = false,
    ReadOnly = true,
    Secret = false)]
public sealed class CapacityListCommand(
    ILogger<CapacityListCommand> logger,
    IFabricCoreService fabricCoreService) : FabricCoreCommand<CapacityListOptions, CapacityListCommandResult>
{
    private readonly ILogger<CapacityListCommand> _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    private readonly IFabricCoreService _fabricCoreService = fabricCoreService ?? throw new ArgumentNullException(nameof(fabricCoreService));

    public override JsonTypeInfo<CapacityListCommandResult> ResultTypeInfo => CoreJsonContext.Default.CapacityListCommandResult;

    public override void ValidateOptions(CapacityListOptions options, ValidationResult validationResult)
    {
        base.ValidateOptions(options, validationResult);

        if (options.ContinuationToken is not null && string.IsNullOrWhiteSpace(options.ContinuationToken))
        {
            validationResult.Errors.Add("--continuation-token must not be empty or whitespace.");
        }
    }

    public override async Task<CommandResponse> ExecuteAsync(CommandContext context, CapacityListOptions options, CancellationToken cancellationToken)
    {
        try
        {
            var page = await _fabricCoreService.ListCapacitiesAsync(options.ContinuationToken, cancellationToken);
            SetResult(context, new(page.Value, page.ContinuationToken, page.ContinuationUri));
        }
        catch (Exception ex)
        {
            _logger.LogError("Error listing Fabric capacities ({ExceptionType}).", ex.GetType().Name);
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

    protected override string GetErrorMessage(Exception ex) => GetStatusCode(ex) switch
    {
        HttpStatusCode.BadRequest => "Check the continuation token and pass it unchanged from a previous capacity listing",
        HttpStatusCode.Unauthorized => "Authentication failed. Use the configured identity with permission to read Fabric capacities",
        HttpStatusCode.Forbidden => "Access denied. The caller needs capacity administrator or contributor access; delegated calls also require Capacity.Read.All or Capacity.ReadWrite.All",
        HttpStatusCode.NotFound => "The Fabric capacity listing was not found. Check service availability and the caller's access",
        HttpStatusCode.TooManyRequests when ex is FabricThrottledException => ex.Message,
        HttpStatusCode.TooManyRequests => "Fabric throttled the request. Wait before retrying",
        HttpStatusCode.RequestTimeout => "The Fabric capacity listing was canceled or timed out",
        HttpStatusCode.GatewayTimeout => "The Fabric capacity listing timed out",
        HttpStatusCode.BadGateway => "Fabric returned an invalid capacity metadata page",
        _ => "Unable to list Fabric capacities. Retry the request or check service availability"
    };
}
