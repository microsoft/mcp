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

[CommandMetadata(
    Id = "6a7f2abe-b9db-4c7f-8ae4-c9764b3cc591",
    Name = "get-item-job-instance",
    Title = "Get Fabric Item Job Instance",
    Description = """
        Gets one existing Microsoft Fabric item's job instance using explicit workspace, item, and job-instance UUIDs.
        Returns the job identity, type, status, invocation type, available UTC timing and trace fields,
        safe failure metadata, and Retry-After guidance when supplied by Fabric.
        HTTP success means the snapshot was read, not that the job completed successfully.
        Makes one GET only: does not poll, wait, discover jobs or job types, retrieve results, run, cancel, or schedule jobs.
        Job-instance IDs are not Fabric operation IDs; do not use the operation-state or operation-result tools for them.
        """,
    OperationPlane = ToolOperationPlane.Control,
    Destructive = false,
    Idempotent = true,
    LocalRequired = false,
    OpenWorld = false,
    ReadOnly = true,
    Secret = false)]
public sealed class ItemJobInstanceGetCommand(
    ILogger<ItemJobInstanceGetCommand> logger,
    IFabricCoreService fabricCoreService) : FabricCoreCommand<ItemJobInstanceGetOptions, ItemJobInstanceGetCommandResult>
{
    private readonly ILogger<ItemJobInstanceGetCommand> _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    private readonly IFabricCoreService _fabricCoreService = fabricCoreService ?? throw new ArgumentNullException(nameof(fabricCoreService));

    public override JsonTypeInfo<ItemJobInstanceGetCommandResult> ResultTypeInfo => CoreJsonContext.Default.ItemJobInstanceGetCommandResult;

    public override void ValidateOptions(ItemJobInstanceGetOptions options, ValidationResult validationResult)
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
        CommandContext context, ItemJobInstanceGetOptions options, CancellationToken cancellationToken)
    {
        try
        {
            var result = await _fabricCoreService.GetItemJobInstanceAsync(
                options.WorkspaceId, options.ItemId, options.JobInstanceId, cancellationToken);
            SetResult(context, result);
        }
        catch (Exception ex)
        {
            _logger.LogError("Error retrieving Fabric item job instance ({ExceptionType}).", ex.GetType().Name);
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
                "Invalid Fabric Jobs request. Provide workspace, item, and job-instance UUIDs.");
        }
    }

    protected override string GetErrorMessage(Exception ex)
    {
        var message = GetStatusCode(ex) switch
        {
            HttpStatusCode.BadRequest => "Provide valid workspace, item, and job-instance UUIDs",
            HttpStatusCode.Unauthorized => "Authentication failed. Sign in with an identity that can read the Fabric item",
            HttpStatusCode.Forbidden => "Access denied. The caller needs read permission for this Fabric item",
            HttpStatusCode.NotFound => "The Fabric item or job instance was not found. Check the workspace, item, and job-instance IDs",
            HttpStatusCode.TooManyRequests => "Fabric throttled the request. Wait before retrying",
            HttpStatusCode.RequestTimeout => "The Fabric job-instance request was canceled or timed out",
            HttpStatusCode.BadGateway => "Fabric returned an invalid job-instance response. Expected matching job metadata as JSON up to 1 MiB",
            _ => "Unable to retrieve the Fabric item job instance. Retry the request or check service availability"
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
