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
    Id = "c61e08b5-06f7-48c6-aed8-8ada63ece7bc",
    Name = "invoke-api-request",
    Title = "Invoke Fabric or Power BI API Request",
    Description = """
        Sends one POST, PUT, PATCH, or DELETE request to a Microsoft Fabric or Power BI REST endpoint that has no dedicated tool,
        for example Query Scale-Out settings, dataset refreshes, gateway bindings, deployment pipelines, Lakehouse Load Table,
        admin APIs, or read endpoints that use POST such as getDefinition. This can create, change, or delete resources:
        confirm the method, path, and body with the user before calling. Use core_get-api-resource for GET requests.
        Use api fabric (default) with a path starting with /v1/, or api powerbi with a path starting with /v1.0/myorg/.
        Only relative paths on these hosts are allowed. body is an optional JSON object or array sent unchanged.
        Returns the status, JSON value, and request ID. Never follows redirects, retries, or resubmits the request.
        A Fabric 202 returns an operation receipt; with sync=true, waits up to max-wait-seconds (default 120, maximum 600),
        and operation-result=true also returns the operation's JSON result. Resume pending operations with
        get-operation-state and get-operation-result, never by resubmitting. A Power BI 202 returns location and
        retryAfterSeconds without polling. Requires the permissions the endpoint documents for the caller.
        """,
    OperationPlane = ToolOperationPlane.Control,
    Destructive = true,
    Idempotent = false,
    LocalRequired = false,
    OpenWorld = false,
    ReadOnly = false,
    Secret = true)]
public sealed class ApiRequestInvokeCommand(
    ILogger<ApiRequestInvokeCommand> logger,
    IFabricCoreService fabricCoreService) : FabricCoreCommand<ApiRequestInvokeOptions, ApiRequestInvokeCommandResult>
{
    private readonly ILogger<ApiRequestInvokeCommand> _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    private readonly IFabricCoreService _fabricCoreService = fabricCoreService ?? throw new ArgumentNullException(nameof(fabricCoreService));

    public override JsonTypeInfo<ApiRequestInvokeCommandResult> ResultTypeInfo => CoreJsonContext.Default.ApiRequestInvokeCommandResult;

    public override void ValidateOptions(ApiRequestInvokeOptions options, ValidationResult validationResult)
    {
        base.ValidateOptions(options, validationResult);
        ApiResourceGetCommand.ValidateTarget(options.Api, options.Path, options.MaxResponseBytes, validationResult);
        if (ApiRequestInput.FindWriteMethod(options.Method) is null)
        {
            validationResult.Errors.Add(ApiRequestInput.MethodMessage);
        }
        if (!ApiRequestInput.IsValidBody(options.Body))
        {
            validationResult.Errors.Add(ApiRequestInput.BodyMessage);
        }
        if (ApiRequestTarget.Find(options.Api) is { } target &&
            ApiRequestInput.GetWaitOptionsError(target, options.Sync, options.MaxWaitSeconds, options.OperationResult) is { } error)
        {
            validationResult.Errors.Add(error);
        }
    }

    public override async Task<CommandResponse> ExecuteAsync(
        CommandContext context, ApiRequestInvokeOptions options, CancellationToken cancellationToken)
    {
        try
        {
            var result = await _fabricCoreService.InvokeApiRequestAsync(
                options.Api, options.Path, options.Method, options.Body, options.MaxResponseBytes,
                new(options.Sync, options.MaxWaitSeconds, options.EarlyPoll), options.OperationResult, cancellationToken);

            SetResult(context, result);
            context.Response.Status = result.Operation switch
            {
                null => result.Response?.Status == (int)HttpStatusCode.Accepted ? HttpStatusCode.Accepted : HttpStatusCode.OK,
                { Status: FabricOperationStatus.Succeeded } => HttpStatusCode.OK,
                { Status: FabricOperationStatus.Failed } => HttpStatusCode.BadGateway,
                { Status: FabricOperationStatus.TrackingStopped or FabricOperationStatus.ResultUnavailable, Issue.HttpStatus: { } status } => (HttpStatusCode)status,
                _ => HttpStatusCode.Accepted
            };
            if (result.Operation?.Issue is { } issue)
            {
                context.Response.Message = issue.Message;
            }
        }
        catch (Exception ex) when (ex is FabricOperationCanceledException || ex is not OperationCanceledException || !cancellationToken.IsCancellationRequested)
        {
            _logger.LogError("Error sending a Fabric or Power BI REST write request ({ExceptionType}).", ex.GetType().Name);
            HandleException(context, ex);
        }
        return context.Response;
    }

    protected override void HandleException(CommandContext context, Exception ex)
    {
        base.HandleException(context, ex);
        if (ex is FabricOperationCanceledException canceled)
        {
            SetResult(context, new(null, canceled.Operation));
            context.Response.Message = canceled.Message;
        }
    }

    protected override HttpStatusCode GetStatusCode(Exception ex) => ex switch
    {
        ApiRequestInputException => HttpStatusCode.BadRequest,
        OperationCanceledException => HttpStatusCode.RequestTimeout,
        _ => base.GetStatusCode(ex)
    };

    protected override string GetErrorMessage(Exception ex) => ex switch
    {
        FabricOperationCanceledException => ex.Message,
        _ => ApiResourceGetCommand.GetApiErrorMessage(ex, GetStatusCode(ex), isWrite: true)
    };
}
