// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net;
using System.Text.Json.Serialization.Metadata;
using Azure.Identity;
using Fabric.Mcp.Tools.Core.Models;
using Fabric.Mcp.Tools.Core.Options;
using Fabric.Mcp.Tools.Core.Services;
using Fabric.Mcp.Tools.Core.Validation;
using Microsoft.Extensions.Logging;
using Microsoft.Mcp.Core.Commands;
using Microsoft.Mcp.Core.Models.Command;

namespace Fabric.Mcp.Tools.Core.Commands;

[CommandMetadata(
    Id = "7213dd35-7e89-4c33-9ef8-83138034aefd",
    Name = "delete-item-schedule",
    Title = "Delete Fabric Item Schedule",
    Description = """
        Deletes one existing Microsoft Fabric item schedule for explicit workspace-id, item-id, and schedule-id UUIDs and job-type.
        This destructive action requires consent, Fabric permissions, and delegated Item.ReadWrite.All or the corresponding item-type ReadWrite.All scope.
        Makes exactly one DELETE with no request body, schedule configuration, discovery, preflight, automatic retry, or follow-up verification.
        Only HTTP 200 confirms deletion and returns deletionStatus Deleted with the requested context; no schedule snapshot is returned.
        This is not a Fabric operation LRO. Never follows Location, polls, waits, retrieves results, executes a job, or cancels a job.
        Deleting a schedule does not cancel already-running jobs or promise rollback. Cancellation requires a separate tool and authorization.
        Repeated deletion targets the same schedule but may fail, including with 404; failures are not converted to successful deletion.
        Canceling the MCP request does not undo a deletion. After an uncertain failure, inspect the target with Get or List Schedules
        separately before considering another deletion; never retry blindly. A later read is not proof of which request changed it.
        Schedule IDs are not job-instance or operation IDs.
        """,
    OperationPlane = ToolOperationPlane.Control,
    Destructive = true,
    Idempotent = true,
    LocalRequired = false,
    OpenWorld = false,
    ReadOnly = false,
    Secret = false)]
public sealed class ItemScheduleDeleteCommand(
    ILogger<ItemScheduleDeleteCommand> logger,
    IFabricCoreService fabricCoreService) : FabricCoreCommand<ItemScheduleDeleteOptions, ItemScheduleDeleteCommandResult>
{
    private readonly ILogger<ItemScheduleDeleteCommand> _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    private readonly IFabricCoreService _fabricCoreService = fabricCoreService ?? throw new ArgumentNullException(nameof(fabricCoreService));

    public override JsonTypeInfo<ItemScheduleDeleteCommandResult> ResultTypeInfo => CoreJsonContext.Default.ItemScheduleDeleteCommandResult;

    public override void ValidateOptions(ItemScheduleDeleteOptions options, ValidationResult validationResult)
    {
        base.ValidateOptions(options, validationResult);
        ItemScheduleInput.ValidateScheduleContext(options.WorkspaceId, options.ItemId, options.JobType, options.ScheduleId, validationResult);
    }

    public override async Task<CommandResponse> ExecuteAsync(
        CommandContext context, ItemScheduleDeleteOptions options, CancellationToken cancellationToken)
    {
        var deletionConfirmed = false;
        try
        {
            var result = await _fabricCoreService.DeleteItemScheduleAsync(
                options.WorkspaceId, options.ItemId, options.JobType, options.ScheduleId, cancellationToken);
            deletionConfirmed = true;
            SetResult(context, result);
            context.Response.Status = HttpStatusCode.OK;
            context.Response.Message = result.Guidance;
        }
        catch (Exception ex)
        {
            _logger.LogError("Error deleting Fabric item schedule ({ExceptionType}).", ex.GetType().Name);
            HandleException(context, deletionConfirmed
                ? new ItemScheduleDeleteRequestException(HttpStatusCode.BadGateway, deletionConfirmed: true)
                : ex);
        }
        return context.Response;
    }

    protected override HttpStatusCode GetStatusCode(Exception ex) => ex switch
    {
        FabricItemInputException => HttpStatusCode.BadRequest,
        OperationCanceledException => HttpStatusCode.RequestTimeout,
        _ => base.GetStatusCode(ex)
    };

    protected override void HandleException(CommandContext context, Exception ex)
    {
        base.HandleException(context, ex);
        if (ex is CommandValidationException validation)
        {
            context.Response.Message = GetValidationErrorMessage(validation,
                "Invalid Fabric schedule deletion. Provide workspace/item/schedule UUIDs and an explicit job type.") +
                " No DELETE request was sent.";
        }
    }

    protected override string GetErrorMessage(Exception ex)
    {
        if (ex is FabricItemInputException)
        {
            return ex.Message + " No DELETE request was sent.";
        }
        if (ex is AuthenticationFailedException)
        {
            return "Authentication failed. Check the configured Fabric identity. No DELETE request was sent.";
        }

        var message = ex is ItemScheduleDeleteRequestException { DeletionConfirmed: true }
            ? "Fabric confirmed schedule deletion (HTTP 200), but the local receipt could not be completed. Do not retry this confirmed deletion"
            : ex is ItemScheduleDeleteRequestException { RequestNotSent: true }
                ? "The schedule deletion request failed or was canceled before sending. No DELETE request was sent"
                : GetStatusCode(ex) switch
                {
                    HttpStatusCode.BadRequest => "Fabric rejected the schedule deletion. Check the explicit workspace, item, job type, and schedule",
                    HttpStatusCode.Unauthorized => "Authentication failed. Check the configured Fabric identity",
                    HttpStatusCode.Forbidden => "Access denied. Check Fabric permissions and delegated Item.ReadWrite.All or the corresponding item-type ReadWrite.All scope",
                    HttpStatusCode.NotFound => "The Fabric workspace, item, job type, or schedule was not found or is inaccessible. This is not confirmed deletion",
                    HttpStatusCode.TooManyRequests => "Fabric throttled this schedule deletion",
                    _ => "Schedule deletion was not confirmed; the outcome may be uncertain. Inspect the target with jobs_get-item-schedule or jobs_list-item-schedules separately before considering another deletion. Do not retry blindly"
                };
        message += ". No automatic retry was attempted. Deleting a schedule does not cancel existing jobs or roll back completed work";
        if (ex is ItemScheduleDeleteRequestException error)
        {
            if (FabricCoreHttpHelpers.IsSafeErrorCode(error.ErrorCode))
            {
                message += $". Fabric error code: {error.ErrorCode}";
            }
            if (error.ErrorMetadataUnavailable)
            {
                message += ". Optional Fabric error metadata was unavailable";
            }
            if (error.RetryAfter is { } retryAfter)
            {
                message += retryAfter.Delta is { } delay
                    ? FormattableString.Invariant($". Wait at least {delay.TotalSeconds:0} seconds before another request")
                    : FormattableString.Invariant($". Wait until at least {retryAfter.Date?.UtcDateTime:yyyy-MM-dd HH:mm:ss} UTC before another request");
            }
        }
        return message;
    }
}
