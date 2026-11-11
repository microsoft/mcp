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
    Id = "4f7c2b8e-9d1a-4e6b-a3c5-0b8d2e7f1a94",
    Name = "get-api-resource",
    Title = "Get Fabric or Power BI API Resource",
    Description = """
        Sends one GET request to a Microsoft Fabric or Power BI REST endpoint that has no dedicated tool and returns its JSON response,
        for example gateways, connections, dataflows, deployment pipelines, Query Scale-Out settings, refresh history, or admin APIs.
        Use api fabric (default) with a path starting with /v1/, or api powerbi with a path starting with /v1.0/myorg/.
        Only relative paths on these hosts are allowed. Returns the status, JSON value, request ID, and a relative nextPath for the next page.
        Never sends a body, follows redirects, retries, polls, or modifies resources. Prefer a dedicated tool when one exists.
        Requires the permissions the endpoint documents for the caller.
        """,
    OperationPlane = ToolOperationPlane.Control,
    Destructive = false,
    Idempotent = true,
    LocalRequired = false,
    OpenWorld = false,
    ReadOnly = true,
    Secret = false)]
public sealed class ApiResourceGetCommand(
    ILogger<ApiResourceGetCommand> logger,
    IFabricCoreService fabricCoreService) : FabricCoreCommand<ApiResourceGetOptions, ApiResourceGetCommandResult>
{
    private readonly ILogger<ApiResourceGetCommand> _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    private readonly IFabricCoreService _fabricCoreService = fabricCoreService ?? throw new ArgumentNullException(nameof(fabricCoreService));

    public override JsonTypeInfo<ApiResourceGetCommandResult> ResultTypeInfo => CoreJsonContext.Default.ApiResourceGetCommandResult;

    public override void ValidateOptions(ApiResourceGetOptions options, ValidationResult validationResult)
    {
        base.ValidateOptions(options, validationResult);
        ValidateTarget(options.Api, options.Path, options.MaxResponseBytes, validationResult);
    }

    public override async Task<CommandResponse> ExecuteAsync(
        CommandContext context, ApiResourceGetOptions options, CancellationToken cancellationToken)
    {
        try
        {
            var result = await _fabricCoreService.GetApiResourceAsync(
                options.Api, options.Path, options.MaxResponseBytes, cancellationToken);
            SetResult(context, result);
        }
        catch (Exception ex)
        {
            _logger.LogError("Error sending a Fabric or Power BI REST GET request ({ExceptionType}).", ex.GetType().Name);
            HandleException(context, ex);
        }
        return context.Response;
    }

    internal static void ValidateTarget(string? api, string? path, int? maxResponseBytes, ValidationResult validationResult)
    {
        var target = ApiRequestTarget.Find(api);
        if (target is null)
        {
            validationResult.Errors.Add(ApiRequestInput.ApiMessage);
        }
        else if (!ApiRequestInput.IsValidPath(target, path))
        {
            validationResult.Errors.Add(ApiRequestInput.PathMessage);
        }
        if (maxResponseBytes is <= 0 or > ApiRequestInput.MaxResponseBytesLimit)
        {
            validationResult.Errors.Add(ApiRequestInput.MaxResponseBytesMessage);
        }
    }

    protected override HttpStatusCode GetStatusCode(Exception ex) => ex switch
    {
        ApiRequestInputException => HttpStatusCode.BadRequest,
        OperationCanceledException => HttpStatusCode.RequestTimeout,
        _ => base.GetStatusCode(ex)
    };

    protected override string GetErrorMessage(Exception ex) => GetApiErrorMessage(ex, GetStatusCode(ex));

    internal static string GetApiErrorMessage(Exception ex, HttpStatusCode statusCode)
    {
        if (ex is ApiRequestInputException)
        {
            // Fixed validator guidance; it never echoes the path or body.
            return ex.Message.TrimEnd('.');
        }
        var message = ex is ApiRequestException { Failure: not ApiRequestFailure.ServiceError } failure
            ? failure.Failure switch
            {
                ApiRequestFailure.Redirect => "The REST endpoint returned a redirect, which is not followed. Check the path",
                ApiRequestFailure.UnsupportedMediaType => "The REST endpoint returned a non-JSON response, which was not downloaded",
                ApiRequestFailure.ResponseTooLarge => "The JSON response exceeds --max-response-bytes and was not truncated. Narrow the request with query options such as $top, or raise the limit up to 16777216",
                _ => "The REST endpoint returned invalid JSON. Expected JSON up to 64 nesting levels"
            }
            : statusCode switch
            {
                HttpStatusCode.BadRequest => "The REST endpoint rejected the request. Check the path, query, and the endpoint's documentation",
                HttpStatusCode.Unauthorized => "Authentication failed. Use an identity that can obtain a token for the selected API",
                HttpStatusCode.Forbidden => "Access denied. The caller lacks the permissions, roles, or delegated scopes that the endpoint requires",
                HttpStatusCode.NotFound => "The REST resource was not found. Check the path, IDs, and the caller's access",
                HttpStatusCode.MethodNotAllowed => "The REST endpoint does not support this method for the path",
                HttpStatusCode.TooManyRequests => "The service throttled the request. Wait before retrying",
                HttpStatusCode.RequestTimeout => "The REST request was canceled or timed out",
                HttpStatusCode.GatewayTimeout => "The REST request timed out",
                _ => "The REST request failed. Retry the request or check service availability"
            };

        if (ex is ApiRequestException apiFailure)
        {
            if (apiFailure.ResponseStatus is { } status)
            {
                message += FormattableString.Invariant($". HTTP status: {status}");
            }
            if (FabricCoreHttpHelpers.IsSafeErrorCode(apiFailure.ErrorCode))
            {
                message += $". Error code: {apiFailure.ErrorCode}";
            }
            if (apiFailure.ErrorMetadataUnavailable)
            {
                message += ". Additional error metadata was unavailable";
            }
            if (FabricCoreHttpHelpers.IsSafeErrorCode(apiFailure.RequestId))
            {
                message += $". Request ID: {apiFailure.RequestId}";
            }
            if (apiFailure.RetryAfter is { } retryAfter)
            {
                message += retryAfter.Delta is { } delay
                    ? FormattableString.Invariant($". Wait at least {delay.TotalSeconds:0} seconds before retrying")
                    : FormattableString.Invariant($". Retry after {retryAfter.Date?.UtcDateTime:yyyy-MM-dd HH:mm:ss} UTC");
            }
        }
        return message;
    }
}
