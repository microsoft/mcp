// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Core.Commands.Subscription;
using Azure.Mcp.Core.Services.Azure.Subscription;
using Azure.Mcp.Tools.ManagedLustre.Options.FileSystem.SubnetSize;
using Azure.Mcp.Tools.ManagedLustre.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Mcp.Core.Commands;
using Microsoft.Mcp.Core.Models.Command;

namespace Azure.Mcp.Tools.ManagedLustre.Commands.FileSystem.SubnetSize;

[CommandMetadata(
    Id = "3d3f6f27-218b-4915-9c1e-243dd53b16da",
    Name = "ask",
    Title = "Calculate AMLFS Subnet Size required number of IP Addresses",
    Description = "Calculates the required subnet size for an Azure Managed Lustre file system given a SKU and size. Use to plan network deployment for AMLFS. Returns the number of required IPs.",
    Destructive = false,
    Idempotent = true,
    OpenWorld = false,
    ReadOnly = true,
    Secret = false,
    LocalRequired = false)]
public sealed class SubnetSizeAskCommand(IManagedLustreService service, ILogger<SubnetSizeAskCommand> logger, ISubscriptionResolver subscriptionResolver)
    : SubscriptionCommand<SubnetSizeAskOptions, SubnetSizeAskCommand.FileSystemSubnetSizeResult>(subscriptionResolver)
{
    private readonly IManagedLustreService _service = service;
    private readonly ILogger<SubnetSizeAskCommand> _logger = logger;

    private static readonly string[] s_allowedSkus = [
        "AMLFS-Durable-Premium-40",
        "AMLFS-Durable-Premium-125",
        "AMLFS-Durable-Premium-250",
        "AMLFS-Durable-Premium-500"
    ];

    public override void ValidateOptions(SubnetSizeAskOptions options, ValidationResult validationResult)
    {
        base.ValidateOptions(options, validationResult);

        if (!s_allowedSkus.Contains(options.Sku))
        {
            validationResult.Errors.Add($"Invalid SKU '{options.Sku}'. Allowed values: {string.Join(", ", s_allowedSkus)}");
        }
    }

    public override async Task<CommandResponse> ExecuteAsync(CommandContext context, SubnetSizeAskOptions options, CancellationToken cancellationToken)
    {
        try
        {
            var result = await _service.GetRequiredAmlFSSubnetsSize(
                options.Subscription!,
                options.Sku,
                options.Size,
                options.Tenant,
                options.RetryPolicy,
                cancellationToken);
            context.Response.Results = ResponseResult.Create(new(result), ManagedLustreJsonContext.Default.FileSystemSubnetSizeResult);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calculating AMLFS subnet size. Subscription: {Subscription}, Sku: {Sku}.", options.Subscription, options.Sku);
            HandleException(context, ex);
        }
        return context.Response;
    }

    public sealed record FileSystemSubnetSizeResult(int NumberOfRequiredIPs);
}
