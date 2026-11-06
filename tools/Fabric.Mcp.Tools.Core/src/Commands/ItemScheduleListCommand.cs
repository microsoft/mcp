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
    Id = "e8f941db-f596-4c69-8e17-ded0c4704d8c",
    Name = "list-item-schedules",
    Title = "List Fabric Item Schedules",
    Description = """
        Lists one complete page of Microsoft Fabric schedules for explicit workspace and item UUIDs and a job type.
        Returns schedule IDs and enabled state, plus available creation time, configuration, owner, execution data,
        continuation token, and Retry-After guidance. Owner and execution data can contain sensitive information.
        Preserves extensible schedule settings and UTC timestamp text without local-time conversion.
        Cron configuration interval is measured in minutes, not a Unix cron expression.
        Call again explicitly with the same IDs, job type, and unchanged token for another page.
        Makes one GET only, without name or job-type discovery, per-schedule lookups, or automatic pagination.
        Does not create, update, delete, or execute schedules; run, cancel, or poll jobs; or predict the next run.
        Schedule IDs are not job-instance or operation IDs. A listed enabled schedule does not prove a job ran or succeeded.
        """,
    OperationPlane = ToolOperationPlane.Control,
    Destructive = false,
    Idempotent = true,
    LocalRequired = false,
    OpenWorld = false,
    ReadOnly = true,
    Secret = true)]
public sealed class ItemScheduleListCommand(
    ILogger<ItemScheduleListCommand> logger,
    IFabricCoreService fabricCoreService) : FabricCoreCommand<ItemScheduleListOptions, ItemScheduleListCommandResult>
{
    private readonly ILogger<ItemScheduleListCommand> _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    private readonly IFabricCoreService _fabricCoreService = fabricCoreService ?? throw new ArgumentNullException(nameof(fabricCoreService));

    public override JsonTypeInfo<ItemScheduleListCommandResult> ResultTypeInfo => CoreJsonContext.Default.ItemScheduleListCommandResult;

    public override void ValidateOptions(ItemScheduleListOptions options, ValidationResult validationResult)
    {
        base.ValidateOptions(options, validationResult);
        ItemJobRunInput.ValidateContext(options.WorkspaceId, options.ItemId, options.JobType, validationResult);
        if (!FabricContinuationToken.IsValidContinuationToken(options.ContinuationToken))
        {
            validationResult.Errors.Add(FabricContinuationToken.InvalidContinuationTokenMessage);
        }
    }

    public override async Task<CommandResponse> ExecuteAsync(
        CommandContext context, ItemScheduleListOptions options, CancellationToken cancellationToken)
    {
        try
        {
            var result = await _fabricCoreService.ListItemSchedulesAsync(
                options.WorkspaceId, options.ItemId, options.JobType, options.ContinuationToken, cancellationToken);
            SetResult(context, result);
        }
        catch (Exception ex)
        {
            _logger.LogError("Error listing Fabric item schedules ({ExceptionType}).", ex.GetType().Name);
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
                "Invalid Fabric schedule listing request. Provide workspace and item UUIDs, a job type, and an optional unchanged continuation token.");
        }
    }

    protected override string GetErrorMessage(Exception ex)
    {
        var message = GetStatusCode(ex) switch
        {
            HttpStatusCode.BadRequest => "Provide valid workspace and item UUIDs, an explicit job type, and an optional unchanged continuation token",
            HttpStatusCode.Unauthorized => "Authentication failed. Sign in with an identity that can read the Fabric item",
            HttpStatusCode.Forbidden => "Access denied. The caller needs read permission for this Fabric item",
            HttpStatusCode.NotFound => "The Fabric item or job type was not found. Check the workspace, item, and job type",
            HttpStatusCode.TooManyRequests => "Fabric throttled the request. Wait before retrying",
            HttpStatusCode.RequestTimeout => "The Fabric schedule listing request was canceled or timed out",
            HttpStatusCode.BadGateway => "Fabric returned an invalid schedule page. Expected schedule metadata and consistent continuation information as JSON up to 1 MiB and 64 nesting levels. No partial page was returned",
            _ => "Unable to list Fabric item schedules. Retry the request or check service availability"
        };
        if (ex is ItemScheduleRequestException { RetryAfter: { } retryAfter })
        {
            message += retryAfter.Delta is { } delay
                ? FormattableString.Invariant($". Wait at least {delay.TotalSeconds:0} seconds before retrying")
                : FormattableString.Invariant($". Retry after {retryAfter.Date?.UtcDateTime:yyyy-MM-dd HH:mm:ss} UTC");
        }
        return message;
    }
}
