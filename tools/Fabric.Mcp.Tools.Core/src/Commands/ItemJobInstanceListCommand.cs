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
    Id = "db646af2-22f1-46ae-b1f6-d37f1483cb95",
    Name = "list-item-job-instances",
    Title = "List Fabric Item Job Instances",
    Description = """
        Lists one complete page of Microsoft Fabric job instances for explicit workspace and item UUIDs.
        Returns job identities, types, statuses, invocation types, available UTC timing, safe failure metadata,
        and any continuation token and Retry-After guidance. Call again explicitly with the same IDs and
        unchanged token for another page. Makes one GET only, without per-job lookups or automatic pagination.
        For most items, the API reports up to 100 recently completed instances; active instances have no corresponding limit.
        This is not a page-size cap or a full-history guarantee. HTTP success does not mean the jobs succeeded.
        Does not poll, retrieve results, run, cancel, or schedule jobs. A listed job cannot establish the
        identity of an earlier ambiguous submission or one without a tracking ID. Cancellation requires separate authorization.
        """,
    OperationPlane = ToolOperationPlane.Control,
    Destructive = false,
    Idempotent = true,
    LocalRequired = false,
    OpenWorld = false,
    ReadOnly = true,
    Secret = false)]
public sealed class ItemJobInstanceListCommand(
    ILogger<ItemJobInstanceListCommand> logger,
    IFabricCoreService fabricCoreService) : FabricCoreCommand<ItemJobInstanceListOptions, ItemJobInstanceListCommandResult>
{
    private readonly ILogger<ItemJobInstanceListCommand> _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    private readonly IFabricCoreService _fabricCoreService = fabricCoreService ?? throw new ArgumentNullException(nameof(fabricCoreService));

    public override JsonTypeInfo<ItemJobInstanceListCommandResult> ResultTypeInfo => CoreJsonContext.Default.ItemJobInstanceListCommandResult;

    public override void ValidateOptions(ItemJobInstanceListOptions options, ValidationResult validationResult)
    {
        base.ValidateOptions(options, validationResult);
        if (!Guid.TryParse(options.WorkspaceId, out var workspaceId) || workspaceId == Guid.Empty)
        {
            validationResult.Errors.Add("--workspace-id must be a nonempty UUID.");
        }
        if (!Guid.TryParse(options.ItemId, out var itemId) || itemId == Guid.Empty)
        {
            validationResult.Errors.Add("--item-id must be a nonempty UUID.");
        }
        if (!ItemJobInstanceListInput.IsValidContinuationToken(options.ContinuationToken))
        {
            validationResult.Errors.Add(ItemJobInstanceListInput.InvalidContinuationTokenMessage);
        }
    }

    public override async Task<CommandResponse> ExecuteAsync(
        CommandContext context, ItemJobInstanceListOptions options, CancellationToken cancellationToken)
    {
        try
        {
            var result = await _fabricCoreService.ListItemJobInstancesAsync(
                options.WorkspaceId, options.ItemId, options.ContinuationToken, cancellationToken);
            SetResult(context, result);
        }
        catch (Exception ex)
        {
            _logger.LogError("Error listing Fabric item job instances ({ExceptionType}).", ex.GetType().Name);
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

    protected override void HandleException(CommandContext context, Exception ex)
    {
        base.HandleException(context, ex);
        if (ex is CommandValidationException validationException)
        {
            context.Response.Message = GetValidationErrorMessage(validationException,
                "Invalid Fabric Jobs list request. Provide workspace and item UUIDs and an optional unchanged continuation token.");
        }
    }

    protected override string GetErrorMessage(Exception ex)
    {
        var message = GetStatusCode(ex) switch
        {
            HttpStatusCode.BadRequest => "Provide valid workspace and item UUIDs and an optional unchanged continuation token",
            HttpStatusCode.Unauthorized => "Authentication failed. Sign in with an identity that can read the Fabric item",
            HttpStatusCode.Forbidden => "Access denied. The caller needs read permission for this Fabric item",
            HttpStatusCode.NotFound => "The Fabric item was not found. Check the workspace and item IDs",
            HttpStatusCode.TooManyRequests => "Fabric throttled the request. Wait before retrying",
            HttpStatusCode.RequestTimeout => "The Fabric job listing request was canceled or timed out",
            HttpStatusCode.BadGateway => "Fabric returned an invalid job-instance page. Expected matching job metadata and consistent continuation information as JSON up to 1 MiB and 64 nesting levels. No partial page was returned",
            _ => "Unable to list Fabric item job instances. Retry the request or check service availability"
        };
        if (ex is ItemJobInstanceRequestException { RetryAfter: { } retryAfter })
        {
            message += retryAfter.Delta is { } delay
                ? FormattableString.Invariant($". Wait at least {delay.TotalSeconds:0} seconds before retrying")
                : FormattableString.Invariant($". Retry after {retryAfter.Date?.UtcDateTime:yyyy-MM-dd HH:mm:ss} UTC");
        }
        return message;
    }
}
