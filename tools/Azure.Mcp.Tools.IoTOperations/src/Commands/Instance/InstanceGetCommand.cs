// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net;
using Azure.Mcp.Core.Commands.Subscription;
using Azure.Mcp.Core.Services.Azure.Subscription;
using Azure.Mcp.Tools.IoTOperations.Models;
using Azure.Mcp.Tools.IoTOperations.Options.Instance;
using Azure.Mcp.Tools.IoTOperations.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Mcp.Core.Commands;
using Microsoft.Mcp.Core.Models.Command;

namespace Azure.Mcp.Tools.IoTOperations.Commands.Instance;

[CommandMetadata(
    Id = "c926e373-38e7-4e8d-a275-5e913653ac2d",
    Name = "get",
    Title = "Get IoT Operations Instance",
    Description = """
        Gets the details of a specific Azure IoT Operations instance. Returns instance details including name,
        location, resource group, provisioning state, Azure IoT Operations version, description, and the associated
        schema registry resource ID. Not for listing multiple instances. Required: --instance, --resource-group,
        and --subscription.
        """,
    Destructive = false,
    Idempotent = true,
    OpenWorld = false,
    ReadOnly = true,
    Secret = false,
    LocalRequired = false)]
public sealed class InstanceGetCommand(ILogger<InstanceGetCommand> logger, IIoTOperationsService iotOperationsService, ISubscriptionResolver subscriptionResolver)
    : SubscriptionCommand<InstanceGetOptions, InstanceGetCommand.InstanceGetCommandResult>(subscriptionResolver)
{
    private readonly ILogger<InstanceGetCommand> _logger = logger;
    private readonly IIoTOperationsService _iotOperationsService = iotOperationsService;

    public override void ValidateOptions(InstanceGetOptions options, ValidationResult validationResult)
    {
        base.ValidateOptions(options, validationResult);

        if (string.IsNullOrWhiteSpace(options.ResourceGroup))
        {
            validationResult.Errors.Add("--resource-group is required.");
        }

        if (string.IsNullOrWhiteSpace(options.Instance))
        {
            validationResult.Errors.Add("--instance is required.");
        }
    }

    public override async Task<CommandResponse> ExecuteAsync(
        CommandContext context,
        InstanceGetOptions options,
        CancellationToken cancellationToken)
    {
        try
        {
            var instance = await _iotOperationsService.GetInstanceAsync(
                options.Subscription!,
                options.ResourceGroup!,
                options.Instance!,
                options.Tenant,
                options.RetryPolicy,
                cancellationToken);

            context.Response.Results = ResponseResult.Create(
                new InstanceGetCommandResult(instance),
                IoTOperationsJsonContext.Default.InstanceGetCommandResult);
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Error getting IoT Operations instance. Subscription: {Subscription}, ResourceGroup: {ResourceGroup}, Instance: {Instance}.",
                options.Subscription,
                options.ResourceGroup,
                options.Instance);
            HandleException(context, ex);
        }

        return context.Response;
    }

    protected override string GetErrorMessage(Exception ex) => ex switch
    {
        KeyNotFoundException =>
            "IoT Operations instance not found. Verify the instance name, resource group, and that you have access.",
        RequestFailedException reqEx when reqEx.Status == (int)HttpStatusCode.NotFound =>
            "IoT Operations instance not found. Verify the instance name, resource group, and subscription, and ensure you have access.",
        RequestFailedException reqEx when reqEx.Status == (int)HttpStatusCode.Forbidden =>
            $"Authorization failed accessing the IoT Operations instance. Details: {reqEx.Message}",
        RequestFailedException reqEx => reqEx.Message,
        _ => base.GetErrorMessage(ex)
    };

    protected override HttpStatusCode GetStatusCode(Exception ex) => ex switch
    {
        KeyNotFoundException => HttpStatusCode.NotFound,
        RequestFailedException reqEx => (HttpStatusCode)reqEx.Status,
        _ => base.GetStatusCode(ex)
    };

    public sealed record InstanceGetCommandResult(IoTOperationsInstanceInfo Instance);
}
