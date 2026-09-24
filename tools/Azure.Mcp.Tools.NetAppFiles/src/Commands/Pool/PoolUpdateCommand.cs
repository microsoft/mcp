// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net;
using System.Text.Json;
using Azure.Mcp.Core.Commands.Subscription;
using Azure.Mcp.Core.Services.Azure.Subscription;
using Azure.Mcp.Tools.NetAppFiles.Models;
using Azure.Mcp.Tools.NetAppFiles.Options.Pool;
using Azure.Mcp.Tools.NetAppFiles.Services;
using Azure.Mcp.Tools.NetAppFiles.Validation;
using Microsoft.Extensions.Logging;
using Microsoft.Mcp.Core.Commands;
using Microsoft.Mcp.Core.Models.Command;
using Microsoft.Mcp.Core.Options;

namespace Azure.Mcp.Tools.NetAppFiles.Commands.Pool;

[CommandMetadata(
    Id = "158311f0-8304-43c0-a413-80c854643eaf",
    Name = "update",
    Title = "Update Azure NetApp Files Capacity Pool",
    Description = "Updates the size, QoS type, cool-access setting, custom throughput, or tags of an Azure NetApp Files capacity pool. Returns the updated pool configuration and provisioning state.",
    OperationPlane = ToolOperationPlane.Control,
    Destructive = true,
    Idempotent = true,
    OpenWorld = false,
    ReadOnly = false,
    Secret = false,
    LocalRequired = false)]
public sealed class PoolUpdateCommand(
    ILogger<PoolUpdateCommand> logger,
    INetAppFilesPoolService service,
    ISubscriptionResolver subscriptionResolver)
    : SubscriptionCommand<PoolUpdateOptions, PoolUpdateCommand.PoolUpdateResult>(subscriptionResolver)
{
    private const long BytesPerTebibyte = 1_099_511_627_776;
    private static readonly string[] s_qosTypes = ["Auto", "Manual"];

    private readonly ILogger<PoolUpdateCommand> _logger = logger;
    private readonly INetAppFilesPoolService _service = service;

    public override async Task<CommandResponse> ExecuteAsync(
        CommandContext context,
        PoolUpdateOptions options,
        CancellationToken cancellationToken)
    {
        try
        {
            var tags = options.Tags is null
                ? null
                : JsonSerializer.Deserialize(options.Tags, NetAppFilesJsonContext.Default.DictionaryStringString);

            var pool = await _service.UpdatePoolAsync(
                options.Account,
                options.Pool,
                options.ResourceGroup,
                options.Subscription!,
                options.Size is null ? null : checked(options.Size.Value * BytesPerTebibyte),
                options.QosType,
                options.CoolAccess,
                options.CustomThroughputMibps,
                tags,
                options.Tenant,
                cancellationToken);

            context.Response.Results = ResponseResult.Create(
                new PoolUpdateResult(pool),
                NetAppFilesJsonContext.Default.PoolUpdateResult);
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Error updating Azure NetApp Files capacity pool. Account: {Account}, Pool: {Pool}, ResourceGroup: {ResourceGroup}, Subscription: {Subscription}",
                options.Account,
                options.Pool,
                options.ResourceGroup,
                options.Subscription);
            HandleException(context, ex);
        }

        return context.Response;
    }

    public override void ValidateOptions(PoolUpdateOptions options, ValidationResult validationResult)
    {
        base.ValidateOptions(options, validationResult);

        if (!AccountNameValidator.IsValid(options.Account))
        {
            validationResult.Errors.Add(AccountNameValidator.ErrorMessage);
        }

        if (!Validation.PoolNameValidator.IsValid(options.Pool))
        {
            validationResult.Errors.Add(Validation.PoolNameValidator.ErrorMessage);
        }

        if (options.Size is not null &&
            (options.Size <= 0 || options.Size % 4 != 0 || options.Size > long.MaxValue / BytesPerTebibyte))
        {
            validationResult.Errors.Add("--size must be a positive multiple of 4 TiB and small enough to represent in bytes.");
        }

        if (options.QosType is not null && !s_qosTypes.Contains(options.QosType, StringComparer.OrdinalIgnoreCase))
        {
            validationResult.Errors.Add("--qos-type must be one of: Auto, Manual.");
        }

        if (options.CustomThroughputMibps <= 0)
        {
            validationResult.Errors.Add("--custom-throughput-mibps must be greater than zero.");
        }

        if (options.CustomThroughputMibps is not null &&
            string.Equals(options.QosType, "Auto", StringComparison.OrdinalIgnoreCase))
        {
            validationResult.Errors.Add("--custom-throughput-mibps cannot be used with --qos-type Auto.");
        }

        if (options.Size is null &&
            options.QosType is null &&
            options.CoolAccess is null &&
            options.CustomThroughputMibps is null &&
            options.Tags is null)
        {
            validationResult.Errors.Add("At least one update property must be provided: --size, --qos-type, --cool-access, --custom-throughput-mibps, or --tags.");
        }

        if (options.Tags is not null)
        {
            try
            {
                if (JsonSerializer.Deserialize(options.Tags, NetAppFilesJsonContext.Default.DictionaryStringString) is null)
                {
                    validationResult.Errors.Add("--tags must be a JSON key-value object.");
                }
            }
            catch (JsonException)
            {
                validationResult.Errors.Add("--tags must be a JSON key-value object with string values.");
            }
        }
    }

    protected override string GetErrorMessage(Exception ex) => ex switch
    {
        KeyNotFoundException => "Resource group not found. Verify the resource group exists and you have access.",
        RequestFailedException requestFailedException when requestFailedException.Status == (int)HttpStatusCode.Conflict =>
            "The Azure NetApp Files capacity pool could not be updated because of a resource conflict. Verify the pool state and update values.",
        RequestFailedException requestFailedException when requestFailedException.Status == (int)HttpStatusCode.Forbidden =>
            "Authorization failed updating the Azure NetApp Files capacity pool. Verify that you have the required RBAC permissions.",
        RequestFailedException requestFailedException when requestFailedException.Status == (int)HttpStatusCode.NotFound =>
            "Azure NetApp Files capacity pool not found. Verify the account, pool, and resource group names.",
        _ => base.GetErrorMessage(ex)
    };

    public record PoolUpdateResult(NetAppFilesPool Pool);
}
