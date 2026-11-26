// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net;
using System.Text.Json.Serialization;
using Azure.Mcp.Core.Services.Azure.Subscription;
using Azure.Mcp.Tools.ResourceHealth.Models;
using Azure.Mcp.Tools.ResourceHealth.Options.AvailabilityStatus;
using Azure.Mcp.Tools.ResourceHealth.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Mcp.Core.Commands;
using Microsoft.Mcp.Core.Models.Command;

namespace Azure.Mcp.Tools.ResourceHealth.Commands.AvailabilityStatus;

/// <summary>
/// Gets or lists availability status information for Azure resources.
/// </summary>
[CommandMetadata(
    Id = "3b388cc7-4b16-4919-9e90-f592247d9891",
    Name = "get",
    Title = "Get/List Resource Availability Status",
    Description = "Get the Azure Resource Health availability status for a specific resource or all resources in a subscription or resource group. Use this tool when asked about the availability status, health status, or Resource Health of an Azure resource (e.g. virtual machine, storage account). Reports whether a resource is Available, Unavailable, Degraded, or Unknown, including the reason and details. This is the correct tool for questions like 'What is the availability status of VM X?' or 'Is resource Y healthy?'.",
    OperationPlane = ToolOperationPlane.Control,
    Destructive = false,
    Idempotent = true,
    OpenWorld = false,
    ReadOnly = true,
    Secret = false,
    LocalRequired = false)]
public sealed class AvailabilityStatusGetCommand(ILogger<AvailabilityStatusGetCommand> logger, IResourceHealthService resourceHealthService, ISubscriptionResolver subscriptionResolver)
    : BaseResourceHealthCommand<AvailabilityStatusGetOptions, AvailabilityStatusGetCommand.AvailabilityStatusGetCommandResult>(subscriptionResolver)
{
    private readonly ILogger<AvailabilityStatusGetCommand> _logger = logger;
    private readonly IResourceHealthService _resourceHealthService = resourceHealthService;

    public override async Task<CommandResponse> ExecuteAsync(CommandContext context, AvailabilityStatusGetOptions options, CancellationToken cancellationToken)
    {
        try
        {
            List<Models.AvailabilityStatus> statuses;

            // If resourceId is provided, get single resource status
            if (!string.IsNullOrEmpty(options.ResourceId))
            {
                // Tag before the call so unsupported types are captured too.
                ResourceHealthTelemetryTags.AddResourceTypeTag(context.Activity, options.ResourceId);

                var status = await _resourceHealthService.GetAvailabilityStatusAsync(
                    options.ResourceId,
                    cancellationToken);

                statuses = [status];
            }
            // Otherwise, list all resources
            else
            {
                statuses = await _resourceHealthService.ListAvailabilityStatusesAsync(
                    options.Subscription!,
                    options.ResourceGroup,
                    options.Tenant,
                    cancellationToken) ?? [];
            }

            context.Response.Results = ResponseResult.Create(
                new(statuses),
                ResourceHealthJsonContext.Default.AvailabilityStatusGetCommandResult);
        }
        catch (ResourceHealthUnprocessableEntityException ex) when (
            string.Equals(ex.ErrorCode, "UnsupportedResourceType", StringComparison.OrdinalIgnoreCase))
        {
            var message = $"Resource type '{ex.ResourceType}' is not supported by Azure Resource Health. Supported resource types and health checks: https://learn.microsoft.com/azure/service-health/resource-health-checks-resource-types";
            _logger.LogInformation("Resource type {ResourceType} is not supported by Azure Resource Health", ex.ResourceType);
            context.Response.Status = HttpStatusCode.OK;
            context.Response.Message = message;
            context.Response.Results = ResponseResult.Create(
                new([], message),
                ResourceHealthJsonContext.Default.AvailabilityStatusGetCommandResult);
        }
        catch (Exception ex)
        {
            if (!string.IsNullOrEmpty(options.ResourceId))
            {
                _logger.LogError(ex, "Failed to get availability status for resource {ResourceId}", options.ResourceId);
            }
            else
            {
                _logger.LogError(ex, "Failed to list availability statuses for subscription {Subscription}{ResourceGroupInfo}",
                    options.Subscription,
                    options.ResourceGroup != null ? $" and resource group {options.ResourceGroup}" : "");
            }
            HandleException(context, ex);
        }

        return context.Response;
    }

    protected override string GetErrorMessage(Exception ex) => ex switch
    {
        ResourceHealthUnprocessableEntityException unprocessableEx =>
            $"Azure Resource Health could not retrieve availability status for resource type '{unprocessableEx.ResourceType}'. Azure error code: {unprocessableEx.ErrorCode ?? "UnprocessableEntity"}. Details: {unprocessableEx.ErrorDetails ?? unprocessableEx.Message} Supported resource types and health checks: https://learn.microsoft.com/azure/service-health/resource-health-checks-resource-types",
        FormatException =>
            "Invalid Azure resource ID. Provide a resource ID in the format /subscriptions/<subscription>/resourceGroups/<resource-group>/providers/<provider>/<type>/<name>",
        _ => base.GetErrorMessage(ex)
    };

    protected override HttpStatusCode GetStatusCode(Exception ex) => ex switch
    {
        ResourceHealthUnprocessableEntityException unprocessableEx => unprocessableEx.StatusCode,
        FormatException => HttpStatusCode.BadRequest,
        _ => base.GetStatusCode(ex)
    };

    public sealed record AvailabilityStatusGetCommandResult(
        List<Models.AvailabilityStatus> Statuses,
        [property: JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] string? Message = null);
}
