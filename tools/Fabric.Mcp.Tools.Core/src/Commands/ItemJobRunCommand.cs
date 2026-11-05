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
    Id = "460dab58-8bda-4c7a-a491-43d87208e75c",
    Name = "run-on-demand-item-job",
    Title = "Run Fabric On-Demand Item Job",
    Description = """
        Submits one on-demand Microsoft Fabric item job with explicit workspace-id, item-id UUIDs and job-type.
        May consume compute or change data. Requires item execution permission and, for delegated callers,
        Item.Execute.All or the corresponding item-specific Execute.All scope.
        Optional execution-data is a JSON object; job-parameters is a JSON parameter array, supported only by some item/job types.
        Makes exactly one POST, never discovers jobs, retries, strips parameters, polls, waits, or fetches results.
        Returns an Accepted submission receipt, not running or completed job status.
        When a trustworthy jobInstanceId is returned, wait at least the supplied Retry-After guidance before
        calling jobs_get-item-job-instance separately. Missing or invalid receipt headers leave acceptance intact,
        with explicit tracking/timing limitations. Never blindly resubmit after an uncertain failure.
        Job-instance IDs are not Fabric operation IDs; do not use operation-state or operation-result tools.
        """,
    OperationPlane = ToolOperationPlane.Control,
    Destructive = true,
    Idempotent = false,
    LocalRequired = false,
    OpenWorld = false,
    ReadOnly = false,
    Secret = false)]
public sealed class ItemJobRunCommand(
    ILogger<ItemJobRunCommand> logger,
    IFabricCoreService fabricCoreService) : FabricCoreCommand<ItemJobRunOptions, ItemJobRunCommandResult>
{
    private readonly ILogger<ItemJobRunCommand> _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    private readonly IFabricCoreService _fabricCoreService = fabricCoreService ?? throw new ArgumentNullException(nameof(fabricCoreService));

    public override JsonTypeInfo<ItemJobRunCommandResult> ResultTypeInfo => CoreJsonContext.Default.ItemJobRunCommandResult;

    public override void ValidateOptions(ItemJobRunOptions options, ValidationResult validationResult)
    {
        base.ValidateOptions(options, validationResult);
        ItemJobRunInput.ValidateContext(options.WorkspaceId, options.ItemId, options.JobType, validationResult);
    }

    public override async Task<CommandResponse> ExecuteAsync(
        CommandContext context, ItemJobRunOptions options, CancellationToken cancellationToken)
    {
        try
        {
            var result = await _fabricCoreService.RunOnDemandItemJobAsync(
                options.WorkspaceId, options.ItemId, options.JobType, options.ExecutionData, options.JobParameters, cancellationToken);
            SetResult(context, result);
            context.Response.Status = HttpStatusCode.Accepted;
            context.Response.Message = result.Guidance;
        }
        catch (Exception ex)
        {
            _logger.LogError("Error submitting Fabric item job ({ExceptionType}).", ex.GetType().Name);
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
                "Invalid Fabric job submission. Provide workspace and item UUIDs and job-type; check execution-data and job-parameters JSON options.");
        }
    }

    protected override string GetErrorMessage(Exception ex)
    {
        if (ex is FabricItemInputException)
        {
            return ex.Message;
        }
        var message = GetStatusCode(ex) switch
        {
            HttpStatusCode.BadRequest => "Fabric rejected the job request. Check the job type and this item's execution-data and parameter support",
            HttpStatusCode.Unauthorized => "Authentication failed. Check the configured Fabric identity",
            HttpStatusCode.Forbidden => "Access denied. The caller needs item execution permission and delegated Item.Execute.All or the item-specific Execute.All scope",
            HttpStatusCode.NotFound => "The Fabric workspace or item was not found, or the caller does not have access",
            HttpStatusCode.TooManyRequests => "Fabric throttled this job submission",
            HttpStatusCode.RequestTimeout or HttpStatusCode.GatewayTimeout => "The job submission was canceled or timed out",
            _ => "Fabric job submission was not confirmed"
        };
        message += ". A job may have started; verify the item's job history before any new submission. Do not retry blindly. No automatic retry or parameter removal was attempted";
        if (ex is ItemJobRunRequestException { RetryAfter: { } retryAfter })
        {
            message += retryAfter.Delta is { } delay
                ? FormattableString.Invariant($". Wait at least {delay.TotalSeconds:0} seconds before another request")
                : FormattableString.Invariant($". Wait until at least {retryAfter.Date?.UtcDateTime:yyyy-MM-dd HH:mm:ss} UTC before another request");
        }
        return message;
    }
}
