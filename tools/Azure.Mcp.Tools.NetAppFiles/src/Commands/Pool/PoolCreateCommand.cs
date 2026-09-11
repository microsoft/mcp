// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net;
using System.Text.Json;
using System.Text.Json.Serialization;
using Azure.Mcp.Core.Commands.Subscription;
using Azure.Mcp.Core.Services.Azure.Subscription;
using Microsoft.Mcp.Core.Extensions;
using Azure.Mcp.Tools.NetAppFiles.Models;
using Azure.Mcp.Tools.NetAppFiles.Options;
using Azure.Mcp.Tools.NetAppFiles.Options.Pool;
using Azure.Mcp.Tools.NetAppFiles.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Mcp.Core.Commands;
using Microsoft.Mcp.Core.Models.Command;
using Microsoft.Mcp.Core.Models.Option;

namespace Azure.Mcp.Tools.NetAppFiles.Commands.Pool;

[CommandMetadata(
    Id = "c4f8a2e6-7d3b-4c9e-a1f5-e8b6d3c7a2f4",
    Name = "create",
    Description =
        """
        Creates an Azure NetApp Files capacity pool in a specified account and returns the created pool details including name, location, resource group, provisioning state, service level, size, QoS type, cool access, and encryption type. Supports size or sizeInBytes, customThroughputMibps, and tags, with optional policy metadata parameters.
        """,
    Title = "Create NetApp Files Capacity Pool",
    Destructive = true,
    Idempotent = false,
    OpenWorld = false,
    ReadOnly = false,
    LocalRequired = false,
    Secret = false
)]
public sealed class PoolCreateCommand(ILogger<PoolCreateCommand> logger, INetAppFilesService netAppFilesService, ISubscriptionResolver subscriptionResolver)
    : SubscriptionCommand<PoolCreateOptions, PoolCreateCommand.PoolCreateCommandResult>(subscriptionResolver)
{
    private readonly ILogger<PoolCreateCommand> _logger = logger;

    private readonly INetAppFilesService _netAppFilesService = netAppFilesService;

    public override async Task<CommandResponse> ExecuteAsync(CommandContext context, PoolCreateOptions options, CancellationToken cancellationToken)
    {
        try
        {
            ValidateUnsupportedCreateArguments(options);

            Dictionary<string, string>? tags = null;
            if (!string.IsNullOrEmpty(options.Tags))
            {
                try
                {
                    tags = JsonSerializer.Deserialize(options.Tags, NetAppFilesJsonContext.Default.DictionaryStringString);
                }
                catch (JsonException ex)
                {
                    throw new ArgumentException($"Invalid tags JSON format: {ex.Message}", nameof(options.Tags));
                }
            }

            var resolvedSize = ResolvePoolSize(options.Size, options.SizeInBytes);

            var pool = await _netAppFilesService.CreatePool(
                options.Account!,
                options.Pool!,
                options.ResourceGroup!,
                options.Location!,
                resolvedSize,
                options.Subscription!,
                options.ServiceLevel,
                options.CustomThroughputMibps,
                options.QosType,
                options.CoolAccess,
                options.EncryptionType,
                tags,
                options.Tenant,
                options.RetryPolicy,
                cancellationToken);

            context.Response.Results = ResponseResult.Create(
                new(pool),
                NetAppFilesJsonContext.Default.PoolCreateCommandResult);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Error creating NetApp Files capacity pool. Pool: {Pool}, Account: {Account}, Options: {@Options}",
                options.Pool, options.Account, options);
            HandleException(context, ex);
        }

        return context.Response;
    }

    protected override string GetErrorMessage(Exception ex) => ex switch
    {
        ArgumentException argEx => argEx.Message,
        RequestFailedException reqEx when reqEx.Status == (int)HttpStatusCode.Conflict =>
            "A capacity pool with this name already exists. Choose a different name.",
        RequestFailedException reqEx when reqEx.Status == (int)HttpStatusCode.Forbidden =>
            $"Authorization failed creating the capacity pool. Details: {reqEx.Message}",
        RequestFailedException reqEx when reqEx.Status == (int)HttpStatusCode.NotFound =>
            "Account or resource group not found. Verify they exist and you have access.",
        RequestFailedException reqEx => reqEx.Message,
        _ => base.GetErrorMessage(ex)
    };

    private static long ResolvePoolSize(long? size, long? sizeInBytes)
    {
        var hasSize = size.HasValue && size.Value > 0;
        var hasSizeInBytes = sizeInBytes.HasValue && sizeInBytes.Value > 0;

        if (hasSize && hasSizeInBytes)
        {
            throw new ArgumentException("Use either --size or --size-in-bytes, not both.");
        }

        if (!hasSize && !hasSizeInBytes)
        {
            throw new ArgumentException("Either --size or --size-in-bytes must be provided.");
        }

        return hasSize ? size!.Value : sizeInBytes!.Value;
    }

    private static void ValidateUnsupportedCreateArguments(PoolCreateOptions options)
    {
        if (options.NoWait)
        {
            throw new ArgumentException("The --no-wait argument is not supported by this command yet.");
        }

        if (options.AcquirePolicyToken)
        {
            throw new ArgumentException("The --acquirePolicyToken argument is not supported by this command yet.");
        }

        if (!string.IsNullOrWhiteSpace(options.ChangeReference))
        {
            throw new ArgumentException("The --changeReference argument is not supported by this command yet.");
        }
    }

    public record PoolCreateCommandResult([property: JsonPropertyName("pool")] CapacityPoolCreateResult Pool);
}
