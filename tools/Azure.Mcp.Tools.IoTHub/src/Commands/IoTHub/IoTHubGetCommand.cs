// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Core.Commands.Subscription;
using Azure.Mcp.Core.Services.Azure.Subscription;
using Azure.Mcp.Tools.IoTHub.Models;
using Azure.Mcp.Tools.IoTHub.Options.IoTHub;
using Azure.Mcp.Tools.IoTHub.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Mcp.Core.Commands;
using Microsoft.Mcp.Core.Models.Command;

namespace Azure.Mcp.Tools.IoTHub.Commands.IoTHub;

[CommandMetadata(
    Id = "108daa19-5c1b-4633-9a56-d58f4da020e1",
    Name = "get",
    Title = "Get IoT Hub",
    Description = """
        Get IoT Hub details by name in a resource group of a subscription.
        Returns the IoT Hub with id, name, location, resourceGroup, subscriptionId, sku, capacity, state, hostName,
        and the configured message-routing custom endpoints (routingEndpoints) with their target resource details.
        """,
    OperationPlane = ToolOperationPlane.Control,
    Destructive = false,
    Idempotent = true,
    OpenWorld = false,
    ReadOnly = true,
    Secret = false,
    LocalRequired = false)]
public sealed class IoTHubGetCommand(
    ILogger<IoTHubGetCommand> logger,
    IIoTHubService service,
    ISubscriptionResolver subscriptionResolver)
    : SubscriptionCommand<IoTHubGetOptions, IoTHubGetCommand.IoTHubGetCommandResult>(subscriptionResolver)
{
    private readonly ILogger<IoTHubGetCommand> _logger = logger;
    private readonly IIoTHubService _service = service;

    public override void ValidateOptions(IoTHubGetOptions options, ValidationResult validationResult)
    {
        base.ValidateOptions(options, validationResult);
        IoTHubValidation.ValidateHubName(options.HubName, validationResult);
    }

    public override async Task<CommandResponse> ExecuteAsync(
        CommandContext context,
        IoTHubGetOptions options,
        CancellationToken cancellationToken)
    {
        try
        {
            var iotHub = await _service.GetIoTHub(
                options.HubName,
                options.ResourceGroup,
                options.Subscription!,
                options.Tenant,
                cancellationToken: cancellationToken);

            context.Response.Results = ResponseResult.Create(
                new IoTHubGetCommandResult(iotHub, AreResultsTruncated: false),
                IoTHubJsonContext.Default.IoTHubGetCommandResult);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Error getting IoT Hub '{HubName}' in resource group '{ResourceGroup}' and subscription '{Subscription}'.",
                options.HubName,
                options.ResourceGroup,
                options.Subscription);
            HandleException(context, ex);
        }

        return context.Response;
    }

    public record IoTHubGetCommandResult(IoTHubDescription IoTHub, bool AreResultsTruncated);
}
