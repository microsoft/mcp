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
    Id = "d5a9b3e7-6c4f-4d8a-b2e1-f7c8a0d3e5b9",
    Name = "update",
    Description =
        """
        Updates an existing Azure NetApp Files capacity pool in a specified account and resource group, and returns the updated pool details including name, location, resource group, provisioning state, service level, size, QoS type, cool access, and encryption type. Supports updating size (in TiB or bytes), service level, QoS type, custom throughput (MiB/s), cool access, and tags. Requires account name, pool name, resource group, location, and subscription.
        """,
    Title = "Update NetApp Files Capacity Pool",
    Destructive = true,
    Idempotent = true,
    OpenWorld = false,
    ReadOnly = false,
    LocalRequired = false,
    Secret = false
)]
public sealed class PoolUpdateCommand(ILogger<PoolUpdateCommand> logger, INetAppFilesService netAppFilesService, ISubscriptionResolver subscriptionResolver)
    : SubscriptionCommand<PoolUpdateOptions, PoolUpdateCommand.PoolUpdateCommandResult>(subscriptionResolver)
{
    private readonly ILogger<PoolUpdateCommand> _logger = logger;

    private readonly INetAppFilesService _netAppFilesService = netAppFilesService;

    public override async Task<CommandResponse> ExecuteAsync(CommandContext context, PoolUpdateOptions options, CancellationToken cancellationToken)
    {
        try
        {
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

            var pool = await _netAppFilesService.UpdatePool(
                options.Account!,
                options.Pool!,
                options.ResourceGroup!,
                options.Location!,
                options.Subscription!,
                options.Size,
                options.SizeInBytes,
                options.ServiceLevel,
                options.QosType,
                options.CustomThroughputMibps,
                options.CoolAccess,
                tags,
                options.Tenant,
                options.RetryPolicy,
                cancellationToken);

            context.Response.Results = ResponseResult.Create(
                new PoolUpdateCommandResult(pool),
                NetAppFilesJsonContext.Default.PoolUpdateCommandResult);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Error updating NetApp Files capacity pool. Pool: {Pool}, Account: {Account}, ResourceGroup: {ResourceGroup}",
                options.Pool, options.Account, options.ResourceGroup);
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
            $"Authorization failed updating the capacity pool. Details: {reqEx.Message}",
        RequestFailedException reqEx when reqEx.Status == (int)HttpStatusCode.NotFound =>
            "Capacity pool, account, or resource group not found. Verify they exist and you have access.",
        RequestFailedException reqEx => reqEx.Message,
        _ => base.GetErrorMessage(ex)
    };

    public record PoolUpdateCommandResult([property: JsonPropertyName("pool")] CapacityPoolCreateResult Pool);
}
