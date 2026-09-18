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
    Id = "ab3278eb-4048-4f65-b058-3a0c350a8236",
    Name = "create",
    Title = "Create Azure NetApp Files Capacity Pool",
    Description = "Creates a capacity pool in an Azure NetApp Files account. Requires an account, pool name, size in TiB, service level, resource group, and subscription. Returns the created pool configuration and provisioning state.",
    OperationPlane = ToolOperationPlane.Control,
    Destructive = true,
    Idempotent = true,
    OpenWorld = false,
    ReadOnly = false,
    Secret = false,
    LocalRequired = false)]
public sealed class PoolCreateCommand(
    ILogger<PoolCreateCommand> logger,
    INetAppFilesPoolService service,
    ISubscriptionResolver subscriptionResolver)
    : SubscriptionCommand<PoolCreateOptions, PoolCreateCommand.PoolCreateResult>(subscriptionResolver)
{
    private const long BytesPerTebibyte = 1_099_511_627_776;
    private static readonly string[] s_serviceLevels = ["Flexible", "Premium", "Standard", "StandardZRS", "Ultra"];
    private static readonly string[] s_qosTypes = ["Auto", "Manual"];
    private static readonly string[] s_encryptionTypes = ["Single", "Double"];

    private readonly ILogger<PoolCreateCommand> _logger = logger;
    private readonly INetAppFilesPoolService _service = service;

    public override async Task<CommandResponse> ExecuteAsync(
        CommandContext context,
        PoolCreateOptions options,
        CancellationToken cancellationToken)
    {
        try
        {
            var tags = options.Tags is null
                ? null
                : JsonSerializer.Deserialize(options.Tags, NetAppFilesJsonContext.Default.DictionaryStringString);

            var pool = await _service.CreatePoolAsync(
                options.Account,
                options.Pool,
                checked(options.Size * BytesPerTebibyte),
                options.ServiceLevel,
                options.ResourceGroup,
                options.Subscription!,
                options.Location,
                options.QosType,
                options.CoolAccess,
                options.EncryptionType,
                options.CustomThroughputMibps,
                tags,
                options.Tenant,
                cancellationToken);

            context.Response.Results = ResponseResult.Create(
                new PoolCreateResult(pool),
                NetAppFilesJsonContext.Default.PoolCreateResult);
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Error creating Azure NetApp Files capacity pool. Account: {Account}, Pool: {Pool}, ResourceGroup: {ResourceGroup}, Subscription: {Subscription}",
                options.Account,
                options.Pool,
                options.ResourceGroup,
                options.Subscription);
            HandleException(context, ex);
        }

        return context.Response;
    }

    public override void ValidateOptions(PoolCreateOptions options, ValidationResult validationResult)
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

        if (options.Size <= 0 || options.Size % 4 != 0 || options.Size > long.MaxValue / BytesPerTebibyte)
        {
            validationResult.Errors.Add("--size must be a positive multiple of 4 TiB and small enough to represent in bytes.");
        }

        if (!Contains(s_serviceLevels, options.ServiceLevel))
        {
            validationResult.Errors.Add("--service-level must be one of: Flexible, Premium, Standard, StandardZRS, Ultra.");
        }

        if (options.QosType is not null && !Contains(s_qosTypes, options.QosType))
        {
            validationResult.Errors.Add("--qos-type must be one of: Auto, Manual.");
        }

        if (options.EncryptionType is not null && !Contains(s_encryptionTypes, options.EncryptionType))
        {
            validationResult.Errors.Add("--encryption-type must be one of: Single, Double.");
        }

        if (options.CustomThroughputMibps is not null &&
            (!options.ServiceLevel.Equals("Flexible", StringComparison.OrdinalIgnoreCase) ||
             !string.Equals(options.QosType, "Manual", StringComparison.OrdinalIgnoreCase)))
        {
            validationResult.Errors.Add("--custom-throughput-mibps requires --service-level Flexible and --qos-type Manual.");
        }

        if (options.CustomThroughputMibps <= 0)
        {
            validationResult.Errors.Add("--custom-throughput-mibps must be greater than zero.");
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
            "The Azure NetApp Files capacity pool could not be created because of a resource conflict. Verify the account, pool name, and resource state.",
        RequestFailedException requestFailedException when requestFailedException.Status == (int)HttpStatusCode.Forbidden =>
            "Authorization failed creating the Azure NetApp Files capacity pool. Verify that you have the required RBAC permissions.",
        RequestFailedException requestFailedException when requestFailedException.Status == (int)HttpStatusCode.NotFound =>
            "Azure NetApp Files account not found. Verify the account and resource group names and that you have access.",
        _ => base.GetErrorMessage(ex)
    };

    private static bool Contains(string[] values, string value) =>
        values.Contains(value, StringComparer.OrdinalIgnoreCase);

    public record PoolCreateResult(NetAppFilesPool Pool);
}
