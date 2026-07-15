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
        Get IoT Hub details or list IoT Hubs in a subscription. Optionally filter by resource group and IoT Hub name.
        Returns each IoT Hub with id, name, location, resourceGroup, subscriptionId, sku, capacity, state, and hostName.
        """,
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

        if (options.Name is null)
        {
            return;
        }

        if (!IsValidIoTHubName(options.Name))
        {
            validationResult.Errors.Add("--name must be 3-50 characters long and contain only letters, numbers, or hyphens, and it cannot end with a hyphen.");
        }
    }

    public override async Task<CommandResponse> ExecuteAsync(
        CommandContext context,
        IoTHubGetOptions options,
        CancellationToken cancellationToken)
    {
        try
        {
            var results = await _service.GetIoTHub(
                options.Name,
                options.ResourceGroup,
                options.Subscription!,
                options.Tenant,
                options.RetryPolicy,
                cancellationToken);

            context.Response.Results = ResponseResult.Create(
                new(results?.Results ?? [], results?.AreResultsTruncated ?? false),
                IoTHubJsonContext.Default.IoTHubGetCommandResult);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Error getting IoT Hub(s). Subscription: {Subscription}, ResourceGroup: {ResourceGroup}, Name: {Name}.",
                options.Subscription,
                options.ResourceGroup,
                options.Name);
            HandleException(context, ex);
        }

        return context.Response;
    }

    public record IoTHubGetCommandResult(List<IoTHubDescription> IoTHubs, bool AreResultsTruncated);

    private static bool IsValidIoTHubName(string value)
    {
        if (value.Length is < 3 or > 50 || value[^1] == '-')
        {
            return false;
        }

        foreach (var ch in value)
        {
            var isAlphaNumeric = (ch >= 'a' && ch <= 'z') ||
                                 (ch >= 'A' && ch <= 'Z') ||
                                 (ch >= '0' && ch <= '9');
            if (!isAlphaNumeric && ch != '-')
            {
                return false;
            }
        }

        return true;
    }
}
