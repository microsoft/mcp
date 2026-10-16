// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Core.Commands.Subscription;
using Azure.Mcp.Core.Services.Azure.Subscription;
using Azure.Mcp.Tools.DeviceProvisioning.Models;
using Azure.Mcp.Tools.DeviceProvisioning.Options.Service;
using Azure.Mcp.Tools.DeviceProvisioning.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Mcp.Core.Commands;
using Microsoft.Mcp.Core.Models.Command;

namespace Azure.Mcp.Tools.DeviceProvisioning.Commands.Service;

[CommandMetadata(
    Id = "3bf66226-33b8-438b-9f71-3abe62a6bf6c",
    Name = "get",
    Title = "Get Device Provisioning Service",
    Description = """
        Get an Azure Device Provisioning Service instance by name in a resource group.
        Returns resource identity, SKU, service state, endpoints, ID scope, allocation policy,
        public network access, data residency, and linked IoT Hub metadata.
        Connection strings, authorization policies, and keys are not returned.
        """,
    OperationPlane = ToolOperationPlane.Control,
    Destructive = false,
    Idempotent = true,
    OpenWorld = false,
    ReadOnly = true,
    Secret = false,
    LocalRequired = false)]
public sealed class DeviceProvisioningServiceGetCommand(
    ILogger<DeviceProvisioningServiceGetCommand> logger,
    IDeviceProvisioningService service,
    ISubscriptionResolver subscriptionResolver)
    : SubscriptionCommand<DeviceProvisioningServiceGetOptions, DeviceProvisioningServiceGetCommand.DeviceProvisioningServiceGetCommandResult>(subscriptionResolver)
{
    private readonly ILogger<DeviceProvisioningServiceGetCommand> _logger = logger;
    private readonly IDeviceProvisioningService _service = service;

    public override void ValidateOptions(
        DeviceProvisioningServiceGetOptions options,
        ValidationResult validationResult)
    {
        base.ValidateOptions(options, validationResult);

        if (!IsValidServiceName(options.Service))
        {
            validationResult.Errors.Add(
                "--service must be 3-64 characters long, contain only letters, numbers, or hyphens, and start and end with a letter or number.");
        }
    }

    public override async Task<CommandResponse> ExecuteAsync(
        CommandContext context,
        DeviceProvisioningServiceGetOptions options,
        CancellationToken cancellationToken)
    {
        try
        {
            var provisioningService = await _service.GetService(
                options.Service,
                options.ResourceGroup,
                options.Subscription!,
                options.Tenant,
                cancellationToken);

            context.Response.Results = ResponseResult.Create(
                new DeviceProvisioningServiceGetCommandResult(
                    provisioningService,
                    AreResultsTruncated: false),
                DeviceProvisioningJsonContext.Default.DeviceProvisioningServiceGetCommandResult);
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Error getting Device Provisioning Service '{ServiceName}' in resource group '{ResourceGroup}' and subscription '{Subscription}'.",
                options.Service,
                options.ResourceGroup,
                options.Subscription);
            HandleException(context, ex);
        }

        return context.Response;
    }

    public record DeviceProvisioningServiceGetCommandResult(
        DeviceProvisioningServiceDescription DeviceProvisioningService,
        bool AreResultsTruncated);

    private static bool IsValidServiceName(string value)
    {
        if (value.Length is < 3 or > 64 ||
            value[0] == '-' ||
            value[^1] == '-')
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
