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
    Id = "065a0b8a-4d62-49f7-9316-c993faea9c7a",
    Name = "get-item-schedule",
    Title = "Get Fabric Item Schedule",
    Description = """
        Gets one existing Microsoft Fabric item schedule for explicit workspace-id, item-id, and schedule-id UUIDs and job-type.
        Returns the matching schedule ID and enabled state, plus available creation time, configuration, owner, execution data,
        and Retry-After guidance. Configuration, owner, and execution data may be sensitive and require disclosure consent, even in read-only mode.
        Requires Fabric item read permission and delegated Item.Read.All or Item.ReadWrite.All, or the corresponding item-type scope.
        Preserves optional fields, extensible settings and owner types, and UTC timestamp text without timezone conversion.
        Cron interval is in minutes, not a cron expression. Owner identifies the creator or last modifier, not necessarily the item owner.
        Makes one GET only: no List or name lookup, job-type discovery, enrichment, pagination, polling, or automatic retries.
        This snapshot is not job execution status or proof of a previous ambiguous create/update outcome.
        Schedule IDs are not job-instance or operation IDs. Reading does not authorize updating, deleting, running, or canceling anything.
        """,
    OperationPlane = ToolOperationPlane.Control,
    Destructive = false,
    Idempotent = true,
    LocalRequired = false,
    OpenWorld = false,
    ReadOnly = true,
    Secret = true)]
public sealed class ItemScheduleGetCommand(
    ILogger<ItemScheduleGetCommand> logger,
    IFabricCoreService fabricCoreService) : FabricCoreCommand<ItemScheduleGetOptions, ItemScheduleGetCommandResult>
{
    private readonly ILogger<ItemScheduleGetCommand> _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    private readonly IFabricCoreService _fabricCoreService = fabricCoreService ?? throw new ArgumentNullException(nameof(fabricCoreService));

    public override JsonTypeInfo<ItemScheduleGetCommandResult> ResultTypeInfo => CoreJsonContext.Default.ItemScheduleGetCommandResult;

    public override void ValidateOptions(ItemScheduleGetOptions options, ValidationResult validationResult)
    {
        base.ValidateOptions(options, validationResult);
        ItemScheduleInput.ValidateScheduleContext(options.WorkspaceId, options.ItemId, options.JobType, options.ScheduleId, validationResult);
    }

    public override async Task<CommandResponse> ExecuteAsync(
        CommandContext context, ItemScheduleGetOptions options, CancellationToken cancellationToken)
    {
        try
        {
            var result = await _fabricCoreService.GetItemScheduleAsync(
                options.WorkspaceId, options.ItemId, options.JobType, options.ScheduleId, cancellationToken);
            SetResult(context, result);
        }
        catch (Exception ex)
        {
            _logger.LogError("Error retrieving Fabric item schedule ({ExceptionType}).", ex.GetType().Name);
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
                "Invalid Fabric schedule lookup request. Provide workspace, item, and schedule UUIDs and an explicit job type.");
        }
    }

    protected override string GetErrorMessage(Exception ex)
    {
        var message = GetStatusCode(ex) switch
        {
            HttpStatusCode.BadRequest => "Provide valid workspace, item, and schedule UUIDs and an explicit job type",
            HttpStatusCode.Unauthorized => "Authentication failed. Sign in with an identity that can read the Fabric item",
            HttpStatusCode.Forbidden => "Access denied. The caller needs read permission for this Fabric item",
            HttpStatusCode.NotFound => "The Fabric item, job type, or schedule was not found or is inaccessible. Check the explicit workspace, item, job type, and schedule",
            HttpStatusCode.TooManyRequests => "Fabric throttled the request. Wait before retrying",
            HttpStatusCode.RequestTimeout => "The Fabric schedule lookup was canceled or timed out",
            HttpStatusCode.BadGateway => "Fabric returned an unusable schedule snapshot. Expected matching schedule metadata as JSON up to 1 MiB, 64 raw nesting levels, and 60 retained levels to leave room for output envelopes",
            _ => "Unable to retrieve the Fabric item schedule. Retry the read or check service availability"
        };
        if (ex is ItemScheduleGetRequestException error)
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
                    ? FormattableString.Invariant($". Wait at least {delay.TotalSeconds:0} seconds before retrying")
                    : FormattableString.Invariant($". Retry after {retryAfter.Date?.UtcDateTime:yyyy-MM-dd HH:mm:ss} UTC");
            }
        }
        return message;
    }
}
