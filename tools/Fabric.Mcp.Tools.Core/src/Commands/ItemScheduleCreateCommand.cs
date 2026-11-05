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
    Id = "c76bff83-45b8-4838-8aac-e0cd01c5b07a",
    Name = "create-item-schedule",
    Title = "Create Fabric Item Schedule",
    Description = """
        Creates one Microsoft Fabric item schedule for explicit workspace-id and item-id UUIDs and job-type.
        Requires explicit enabled (true or false) and configuration JSON with type, UTC start/end, and localTimeZoneId.
        Before requesting consent, confirm the timing and enabled intent: a past start time may trigger a job immediately,
        consuming compute or changing data. No timezone, time, cadence, or enabled defaults are inferred or rewritten.
        Cron interval is in minutes, not a Unix cron expression; Daily, Weekly, and Monthly use their documented fields.
        Optional execution-data is fixed static JSON defined by the item/job type; extensible settings remain intact.
        Requires Fabric permissions and, for delegated callers, both Item.Execute.All and Item.ReadWrite.All.
        Makes exactly one POST, without discovery, schedule-count preflight, automatic retry, parameter stripping,
        polling, result retrieval, or a follow-up run. Fabric enforces its documented 20-schedule limit.
        HTTP 201 returns the created schedule, including available configuration, owner, and execution data, which may be sensitive.
        Creation is not proof of job execution or success. Schedule IDs are not job-instance or operation IDs.
        After an uncertain failure, verify existing schedules separately before any new creation; never resubmit blindly.
        """,
    OperationPlane = ToolOperationPlane.Control,
    Destructive = true,
    Idempotent = false,
    LocalRequired = false,
    OpenWorld = false,
    ReadOnly = false,
    Secret = true)]
public sealed class ItemScheduleCreateCommand(
    ILogger<ItemScheduleCreateCommand> logger,
    IFabricCoreService fabricCoreService) : FabricCoreCommand<ItemScheduleCreateOptions, ItemScheduleCreateCommandResult>
{
    private readonly ILogger<ItemScheduleCreateCommand> _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    private readonly IFabricCoreService _fabricCoreService = fabricCoreService ?? throw new ArgumentNullException(nameof(fabricCoreService));

    public override JsonTypeInfo<ItemScheduleCreateCommandResult> ResultTypeInfo => CoreJsonContext.Default.ItemScheduleCreateCommandResult;

    public override void ValidateOptions(ItemScheduleCreateOptions options, ValidationResult validationResult)
    {
        base.ValidateOptions(options, validationResult);
        ItemJobRunInput.ValidateContext(options.WorkspaceId, options.ItemId, options.JobType, validationResult);
    }

    public override async Task<CommandResponse> ExecuteAsync(
        CommandContext context, ItemScheduleCreateOptions options, CancellationToken cancellationToken)
    {
        try
        {
            var result = await _fabricCoreService.CreateItemScheduleAsync(
                options.WorkspaceId, options.ItemId, options.JobType, options.Enabled, options.Configuration, options.ExecutionData, cancellationToken);
            SetResult(context, result);
            context.Response.Status = HttpStatusCode.Created;
            context.Response.Message = result.Guidance;
        }
        catch (Exception ex)
        {
            _logger.LogError("Error creating Fabric item schedule ({ExceptionType}).", ex.GetType().Name);
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
                "Invalid Fabric schedule creation. Provide workspace/item UUIDs, job-type, explicit enabled true or false, configuration JSON, and optional execution-data.");
        }
    }

    protected override string GetErrorMessage(Exception ex)
    {
        if (ex is FabricItemInputException)
        {
            return ex.Message;
        }
        var message = ex is ItemScheduleCreateRequestException { CreationConfirmed: true }
            ? "Fabric confirmed schedule creation (HTTP 201), but the returned schedule metadata could not be read or validated. Do not resubmit this creation; inspect existing schedules separately"
            : GetStatusCode(ex) switch
            {
                HttpStatusCode.BadRequest => "Fabric rejected the schedule request. Check the job type, configuration, and execution-data support",
                HttpStatusCode.Unauthorized => "Authentication failed. Check the configured Fabric identity",
                HttpStatusCode.Forbidden => "Access denied. Check Fabric permissions and delegated Item.Execute.All and Item.ReadWrite.All scopes",
                HttpStatusCode.NotFound => "The Fabric workspace or item was not found, or the caller does not have access",
                HttpStatusCode.TooManyRequests => "Fabric throttled this schedule creation",
                HttpStatusCode.RequestTimeout or HttpStatusCode.GatewayTimeout => "The schedule request was canceled or timed out",
                _ => "Fabric schedule creation was not confirmed"
            };
        if (ex is not ItemScheduleCreateRequestException { CreationConfirmed: true })
        {
            message += ". A schedule may have been created; verify existing schedules separately before any new creation. Do not retry blindly";
        }
        message += ". No automatic retry or parameter removal was attempted";
        if (ex is ItemScheduleCreateRequestException error)
        {
            if (error.ErrorCode is { } code)
            {
                message += $". Fabric error code: {code}";
                if (code == "ScheduleExceedsLimit")
                {
                    message += ". Fabric enforces the schedule limit; no schedule was automatically deleted";
                }
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
