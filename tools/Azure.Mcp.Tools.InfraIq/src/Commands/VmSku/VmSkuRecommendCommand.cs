// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Diagnostics;
using System.Net;
using System.Text.Json.Serialization.Metadata;
using Azure.Mcp.Core.Commands.Subscription;
using Azure.Mcp.Core.Services.Azure.Subscription;
using Azure.Mcp.Tools.InfraIq.Exceptions;
using Azure.Mcp.Tools.InfraIq.Models.VmSku;
using Azure.Mcp.Tools.InfraIq.Options.VmSku;
using Azure.Mcp.Tools.InfraIq.Services;
using Azure.Mcp.Tools.InfraIq.Validation;
using Microsoft.Extensions.Logging;
using Microsoft.Mcp.Core.Commands;
using Microsoft.Mcp.Core.Models.Command;

namespace Azure.Mcp.Tools.InfraIq.Commands.VmSku;

[CommandMetadata(
    Id = "7d3b6c1e-4a52-4f0e-9b8a-2c5e1f0a9d47",
    Name = "recommend",
    Title = "Recommend VM SKUs for Inference",
    Description = """
        Recommend and rank Azure VM SKUs for hosting an AI model inference workload in an Azure location using Azure
        InfraIQ VM SKU recommendation. Provide the model with --hugging-face-model-id, or with --parameter-count plus
        --num-layers, --num-key-value-heads, and --head-dim, and optionally the weight precision, concurrency, token
        budget, replica count, procurement option, ranking preference, maximum hourly cost, and a candidate set of VM
        sizes. Optional --include-quota, --include-placement, and --include-pricing flags add subscription quota,
        placement, and pricing evaluation. Requires subscription context from --subscription, which accepts an Azure
        subscription ID or name, or from the configured default subscription. Returns the complete recommendation
        response with ranked options, readiness, topology, cost, quota, placement, and sizing details.
        """,
    OperationPlane = ToolOperationPlane.Control,
    Destructive = false,
    Idempotent = true,
    OpenWorld = false,
    ReadOnly = true,
    Secret = false,
    LocalRequired = false)]
public sealed class VmSkuRecommendCommand(
    ILogger<VmSkuRecommendCommand> logger,
    IInfraIqService infraIqService,
    ISubscriptionResolver subscriptionResolver)
    : SubscriptionCommand<VmSkuRecommendOptions, VmSkuRecommendResult>(subscriptionResolver)
{
    private readonly ILogger<VmSkuRecommendCommand> _logger = logger;
    private readonly IInfraIqService _infraIqService = infraIqService;

    public override JsonTypeInfo<VmSkuRecommendResult>? ResultTypeInfo => InfraIqJsonContext.Default.VmSkuRecommendResult;

    public override void ValidateOptions(VmSkuRecommendOptions options, ValidationResult validationResult)
    {
        base.ValidateOptions(options, validationResult);
        VmSkuRecommendOptionsValidator.Validate(options, validationResult);
    }

    public override async Task<CommandResponse> ExecuteAsync(
        CommandContext context,
        VmSkuRecommendOptions options,
        CancellationToken cancellationToken)
    {
        try
        {
            var result = await _infraIqService.RecommendVmSkuAsync(
                options.Subscription!,
                VmSkuRecommendOptionsValidator.NormalizeLocation(options.Location),
                VmSkuRecommendOptionsValidator.BuildRequestBody(options),
                options.Tenant,
                cancellationToken);

            SetResult(context, result);
        }
        catch (Exception ex)
        {
            if (ex is InfraIqArmException armException)
            {
                _logger.LogError("InfraIQ VM SKU recommendation failed. Status: {Status}, Code: {Code}", armException.Status, armException.Code);
            }
            else
            {
                _logger.LogError(ex, "InfraIQ VM SKU recommendation failed.");
            }

            HandleException(context, ex);
        }

        return context.Response;
    }

    protected override void HandleException(CommandContext context, Exception ex)
    {
        if (ex is not InfraIqArmException armException)
        {
            base.HandleException(context, ex);
            return;
        }

        context.Activity?.SetStatus(ActivityStatusCode.Error)
            ?.SetTag(TagName.ExceptionType, ex.GetType().ToString());

        var response = context.Response;
        response.Status = armException.Status is >= 400 and <= 599 ? (HttpStatusCode)armException.Status : HttpStatusCode.BadGateway;
        response.Message = armException.Message;
        response.TelemetryFailureMessage = armException.Message;
        response.Results = ResponseResult.Create(
            new VmSkuRecommendErrorResult(
                armException.Status,
                armException.Code,
                armException.Target,
                armException.RequestId,
                armException.ClientRequestId,
                armException.RetryAfter,
                armException.Message),
            InfraIqJsonContext.Default.VmSkuRecommendErrorResult);
    }
}
