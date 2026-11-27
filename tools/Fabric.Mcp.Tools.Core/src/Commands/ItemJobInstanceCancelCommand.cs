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
    Id = "29cb789b-53a0-4f69-8cde-7c23206457ed",
    Name = "cancel-item-job-instance",
    Title = "Cancel Fabric Item Job Instance",
    Description = """
        Requests cancellation of one existing Microsoft Fabric item job using explicit workspace-id, item-id,
        and job-instance-id nonempty UUIDs. This destructive action requires consent and item execution
        permission; delegated callers need Item.Execute.All or the corresponding item-specific Execute.All scope.
        Makes exactly one POST with no body or job-type. Never discovers jobs, retries, polls, waits, or fetches results.
        Returns cancellationStatus Requested and the requested IDs when Fabric accepts cancellation, not a Cancelled job state.
        Wait at least the supplied Retry-After guidance before calling jobs_get-item-job-instance separately.
        Missing or invalid response metadata does not undo acceptance. JobAlreadyCompleted is a failure, not successful cancellation.
        Repeated cancellation targets the same job, but may fail; no automatic retry is performed.
        After an uncertain failure, check this known job with Get before considering another cancellation request.
        Canceling the MCP request itself does not cancel the Fabric job. No rollback of completed work is promised.
        Job-instance IDs are not Fabric operation IDs; do not use operation-state or operation-result tools.
        """,
    OperationPlane = ToolOperationPlane.Control,
    Destructive = true,
    Idempotent = true,
    LocalRequired = false,
    OpenWorld = false,
    ReadOnly = false,
    Secret = false)]
public sealed class ItemJobInstanceCancelCommand(
    ILogger<ItemJobInstanceCancelCommand> logger,
    IFabricCoreService fabricCoreService) : FabricCoreCommand<ItemJobInstanceCancelOptions, ItemJobInstanceCancelCommandResult>
{
    private const string PermissionMessage = "Access denied. The caller needs item execution permission and delegated Item.Execute.All or the item-specific Execute.All scope";

    private readonly ILogger<ItemJobInstanceCancelCommand> _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    private readonly IFabricCoreService _fabricCoreService = fabricCoreService ?? throw new ArgumentNullException(nameof(fabricCoreService));

    public override JsonTypeInfo<ItemJobInstanceCancelCommandResult> ResultTypeInfo => CoreJsonContext.Default.ItemJobInstanceCancelCommandResult;

    public override void ValidateOptions(ItemJobInstanceCancelOptions options, ValidationResult validationResult)
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
        if (!Guid.TryParse(options.JobInstanceId, out var jobInstanceId) || jobInstanceId == Guid.Empty)
        {
            validationResult.Errors.Add("--job-instance-id must be a nonempty UUID.");
        }
    }

    public override async Task<CommandResponse> ExecuteAsync(
        CommandContext context, ItemJobInstanceCancelOptions options, CancellationToken cancellationToken)
    {
        try
        {
            var result = await _fabricCoreService.CancelItemJobInstanceAsync(
                options.WorkspaceId, options.ItemId, options.JobInstanceId, cancellationToken);
            SetResult(context, result);
            context.Response.Status = HttpStatusCode.Accepted;
            context.Response.Message = result.Guidance;
        }
        catch (Exception ex)
        {
            _logger.LogError("Error requesting Fabric job cancellation ({ExceptionType}).", ex.GetType().Name);
            HandleException(context, ex);
        }
        return context.Response;
    }

    protected override HttpStatusCode GetStatusCode(Exception ex) => ex switch
    {
        OperationCanceledException => HttpStatusCode.RequestTimeout,
        _ => base.GetStatusCode(ex)
    };

    protected override void HandleException(CommandContext context, Exception ex)
    {
        base.HandleException(context, ex);
        if (ex is CommandValidationException validation)
        {
            context.Response.Message = GetValidationErrorMessage(validation,
                "Invalid Fabric job cancellation request. Provide workspace, item, and job-instance UUIDs.");
        }
    }

    protected override string GetErrorMessage(Exception ex)
    {
        var message = ex switch
        {
            ItemJobInstanceCancelRequestException { ErrorCode: "JobAlreadyCompleted" } =>
                "Fabric returned JobAlreadyCompleted. Cancellation was not accepted because the job has already completed; this does not establish its final success or failure status",
            ItemJobInstanceCancelRequestException { ErrorCode: "InsufficientPrivileges" } => PermissionMessage,
            ItemJobInstanceCancelRequestException { ErrorCode: "ItemNotFound" } =>
                "The Fabric item was not found or is not accessible to the caller",
            _ => GetStatusCode(ex) switch
            {
                HttpStatusCode.BadRequest => "Fabric rejected the cancellation request. Check the workspace, item, and job-instance UUIDs",
                HttpStatusCode.Unauthorized => "Authentication failed. Check the configured Fabric identity",
                HttpStatusCode.Forbidden => PermissionMessage,
                HttpStatusCode.NotFound => "The Fabric item or job instance was not found or is not accessible to the caller",
                HttpStatusCode.TooManyRequests => "Fabric throttled this cancellation request",
                HttpStatusCode.RequestTimeout or HttpStatusCode.GatewayTimeout => "The cancellation request was interrupted or timed out; its outcome may be uncertain",
                _ => "Fabric job cancellation was not confirmed; its outcome may be uncertain"
            }
        };
        message += ". Check the known job with jobs_get-item-job-instance in a separate call before considering another cancellation request. No automatic retry was attempted. Canceling the MCP request itself does not cancel the Fabric job";
        if (ex is ItemJobInstanceCancelRequestException { ErrorMetadataUnavailable: true })
        {
            message += ". Additional Fabric error metadata was unavailable";
        }
        if (ex is ItemJobInstanceCancelRequestException { RetryAfter: { } retryAfter })
        {
            message += retryAfter.Delta is { } delay
                ? FormattableString.Invariant($". Wait at least {delay.TotalSeconds:0} seconds before another request")
                : FormattableString.Invariant($". Wait until at least {retryAfter.Date?.UtcDateTime:yyyy-MM-dd HH:mm:ss} UTC before another request");
        }
        return message;
    }

}
