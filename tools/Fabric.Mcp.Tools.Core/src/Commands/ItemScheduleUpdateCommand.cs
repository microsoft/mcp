// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net;
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
    Id = "36f3e1ce-cf49-487b-9375-8f53d12c1a46",
    Name = "update-item-schedule",
    Title = "Update Fabric Item Schedule",
    Description = """
        Updates one existing Microsoft Fabric item schedule for explicit workspace-id, item-id, and schedule-id UUIDs and job-type.
        Requires both explicit enabled (true or false) and full configuration JSON, even though the API uses PATCH.
        Supply type, UTC start/end, and localTimeZoneId. Read existing settings with List Schedules separately if needed;
        this tool never fetches or merges them. No timezone, time, cadence, or enabled defaults are inferred or rewritten.
        Before requesting consent, confirm timing, enabled intent, and the calling identity: a past start time may trigger
        a job immediately, consuming compute or changing data. The schedule owner is the identity that created or last
        modified it, separately from item ownership. Disabling a schedule does not cancel existing job instances.
        Cron interval is in minutes, not a Unix cron expression; Daily, Weekly, and Monthly use their documented fields.
        Optional execution-data is fixed static JSON defined by the item/job type; extensible settings remain intact.
        Omission only omits executionData from the request; preservation or clearing of stored data is not documented.
        Requires Fabric permissions and, for delegated callers, both Item.Execute.All and Item.ReadWrite.All.
        Makes exactly one PATCH, without discovery, preflight, Get/merge/PATCH, automatic retry, parameter stripping,
        polling, result retrieval, or job execution. HTTP 200 returns the updated schedule, not a Fabric operation LRO.
        Configuration, owner, and execution data may be sensitive. Update success is not proof a job ran or succeeded.
        Schedule IDs are not job-instance or operation IDs. After an uncertain failure, inspect the target schedule
        separately before another update; never resubmit blindly.
        """,
    OperationPlane = ToolOperationPlane.Control,
    Destructive = true,
    Idempotent = false,
    LocalRequired = false,
    OpenWorld = false,
    ReadOnly = false,
    Secret = true)]
public sealed class ItemScheduleUpdateCommand(
    ILogger<ItemScheduleUpdateCommand> logger,
    IFabricCoreService fabricCoreService) : FabricCoreCommand<ItemScheduleUpdateOptions, ItemScheduleUpdateCommandResult>
{
    private readonly ILogger<ItemScheduleUpdateCommand> _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    private readonly IFabricCoreService _fabricCoreService = fabricCoreService ?? throw new ArgumentNullException(nameof(fabricCoreService));

    public override JsonTypeInfo<ItemScheduleUpdateCommandResult> ResultTypeInfo => CoreJsonContext.Default.ItemScheduleUpdateCommandResult;

    public override void ValidateOptions(ItemScheduleUpdateOptions options, ValidationResult validationResult)
    {
        base.ValidateOptions(options, validationResult);
        ItemScheduleInput.ValidateUpdateContext(options.WorkspaceId, options.ItemId, options.JobType, options.ScheduleId, validationResult);
    }

    public override async Task<CommandResponse> ExecuteAsync(
        CommandContext context, ItemScheduleUpdateOptions options, CancellationToken cancellationToken)
    {
        try
        {
            var result = await _fabricCoreService.UpdateItemScheduleAsync(
                options.WorkspaceId, options.ItemId, options.JobType, options.ScheduleId, options.Enabled,
                options.Configuration, options.ExecutionData, cancellationToken);
            SetResult(context, result);
            context.Response.Status = HttpStatusCode.OK;
            context.Response.Message = result.Guidance;
        }
        catch (Exception ex)
        {
            _logger.LogError("Error updating Fabric item schedule ({ExceptionType}).", ex.GetType().Name);
            HandleException(context, ex);
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
                "Invalid Fabric schedule update. Provide workspace/item/schedule UUIDs, job-type, explicit enabled true or false, full configuration JSON, and optional execution-data.");
        }
    }

    protected override string GetErrorMessage(Exception ex)
    {
        if (ex is FabricItemInputException)
        {
            return ex.Message;
        }
        var message = ex is ItemScheduleUpdateRequestException { UpdateConfirmed: true }
            ? "Fabric confirmed the schedule update (HTTP 200), but the returned schedule metadata could not be read or validated. Do not resubmit this update blindly; inspect the target schedule separately"
            : GetStatusCode(ex) switch
            {
                HttpStatusCode.BadRequest => "Fabric rejected the schedule update. Check the job type, full configuration, and execution-data support",
                HttpStatusCode.Unauthorized => "Authentication failed. Check the configured Fabric identity",
                HttpStatusCode.Forbidden => "Access denied. Check Fabric permissions and delegated Item.Execute.All and Item.ReadWrite.All scopes",
                HttpStatusCode.NotFound => "The Fabric workspace, item, or schedule was not found, or the caller does not have access",
                HttpStatusCode.TooManyRequests => "Fabric throttled this schedule update",
                HttpStatusCode.RequestTimeout or HttpStatusCode.GatewayTimeout => "The schedule update was canceled or timed out",
                _ => "Fabric schedule update was not confirmed"
            };
        if (ex is not ItemScheduleUpdateRequestException { UpdateConfirmed: true })
        {
            message += ". The schedule may have been updated; inspect the target schedule separately before another update. Do not retry blindly";
        }
        message += ". No automatic retry or parameter removal was attempted. Updating is not job execution; disabling does not cancel existing jobs";
        if (ex is ItemScheduleUpdateRequestException error)
        {
            if (error.ErrorCode is { } code)
            {
                message += $". Fabric error code: {code}";
            }
            if (error.ErrorMetadataUnavailable)
            {
                message += ". Additional error metadata was unavailable";
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
