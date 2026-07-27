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
    Id = "b6317bba-e28c-445b-9133-9cfbfe677698",
    Name = "validate",
    Title = "Validate AMLFS subnet against SKU and size",
    Description = "Validates that the provided subnet can host an Azure Managed Lustre filesystem for the given SKU and size.",
    Destructive = false,
    Idempotent = true,
    OpenWorld = false,
    ReadOnly = true,
    Secret = false,
    LocalRequired = false)]
public sealed class SubnetSizeValidateCommand(IManagedLustreService service, ILogger<SubnetSizeValidateCommand> logger, ISubscriptionResolver subscriptionResolver)
    : SubscriptionCommand<SubnetSizeValidateOptions, SubnetSizeValidateCommand.FileSystemCheckSubnetResult>(subscriptionResolver)
{
    private readonly IManagedLustreService _service = service;
    private readonly ILogger<SubnetSizeValidateCommand> _logger = logger;

    private static readonly string[] s_allowedSkus = [
        "AMLFS-Durable-Premium-40",
        "AMLFS-Durable-Premium-125",
        "AMLFS-Durable-Premium-250",
        "AMLFS-Durable-Premium-500"
    ];

    public override void ValidateOptions(SubnetSizeValidateOptions options, ValidationResult validationResult)
    {
        base.ValidateOptions(options, validationResult);

        if (!s_allowedSkus.Contains(options.Sku))
        {
            validationResult.Errors.Add($"Invalid SKU '{options.Sku}'. Allowed values: {string.Join(", ", s_allowedSkus)}");
        }
    }

    public override async Task<CommandResponse> ExecuteAsync(CommandContext context, SubnetSizeValidateOptions options, CancellationToken cancellationToken)
    {
        try
        {
            var subnetIsValid = await _service.CheckAmlFSSubnetAsync(
                options.Subscription!,
                options.Sku,
                options.Size,
                options.SubnetId,
                options.Location,
                options.Tenant,
                options.RetryPolicy,
                cancellationToken);

            context.Response.Results = ResponseResult.Create(new(subnetIsValid), ManagedLustreJsonContext.Default.FileSystemCheckSubnetResult);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error validating AMLFS subnet.");
            HandleException(context, ex);
        }
        return context.Response;
    }

    public sealed record FileSystemCheckSubnetResult(bool Valid);
}
