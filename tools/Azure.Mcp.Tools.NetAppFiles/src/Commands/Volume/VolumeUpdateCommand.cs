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
using Azure.Mcp.Tools.NetAppFiles.Options.Volume;
using Azure.Mcp.Tools.NetAppFiles.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Mcp.Core.Commands;
using Microsoft.Mcp.Core.Models.Command;
using Microsoft.Mcp.Core.Models.Option;

namespace Azure.Mcp.Tools.NetAppFiles.Commands.Volume;

[CommandMetadata(
    Id = "f1a3b5c7-9d2e-4f8a-b6c0-e4d7a2f9c3b5",
    Name = "update",
    Description =
        """
        Updates an existing Azure NetApp Files volume in a specified capacity pool and returns the updated volume details including name, location, resource group, provisioning state, service level, quota, creation token, subnet, and protocol types. Supports updating usage threshold (quota), service level, and tags. Requires account name, pool name, volume name, resource group, location, and subscription.
        """,
    Title = "Update NetApp Files Volume",
    Destructive = true,
    Idempotent = true,
    OpenWorld = false,
    ReadOnly = false,
    LocalRequired = false,
    Secret = false
)]
public sealed class VolumeUpdateCommand(ILogger<VolumeUpdateCommand> logger, INetAppFilesService netAppFilesService, ISubscriptionResolver subscriptionResolver)
    : SubscriptionCommand<VolumeUpdateOptions, VolumeUpdateCommand.VolumeUpdateCommandResult>(subscriptionResolver)
{
    private readonly ILogger<VolumeUpdateCommand> _logger = logger;

    private readonly INetAppFilesService _netAppFilesService = netAppFilesService;

    public override async Task<CommandResponse> ExecuteAsync(CommandContext context, VolumeUpdateOptions options, CancellationToken cancellationToken)
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

            var volume = await _netAppFilesService.UpdateVolume(
                options.Account!,
                options.Pool!,
                options.Volume!,
                options.ResourceGroup!,
                options.Location!,
                options.Subscription!,
                options.UsageThreshold != 0 ? options.UsageThreshold : null,
                options.ServiceLevel,
                tags,
                options.Tenant,
                options.RetryPolicy,
                cancellationToken);

            context.Response.Results = ResponseResult.Create(
                new VolumeUpdateCommandResult(volume),
                NetAppFilesJsonContext.Default.VolumeUpdateCommandResult);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Error updating NetApp Files volume. Volume: {Volume}, Account: {Account}, Pool: {Pool}, Options: {@Options}",
                options.Volume, options.Account, options.Pool, options);
            HandleException(context, ex);
        }

        return context.Response;
    }

    protected override string GetErrorMessage(Exception ex) => ex switch
    {
        ArgumentException argEx => argEx.Message,
        RequestFailedException reqEx when reqEx.Status == (int)HttpStatusCode.Conflict =>
            "A volume with this name already exists. Choose a different name.",
        RequestFailedException reqEx when reqEx.Status == (int)HttpStatusCode.Forbidden =>
            $"Authorization failed updating the volume. Details: {reqEx.Message}",
        RequestFailedException reqEx when reqEx.Status == (int)HttpStatusCode.NotFound =>
            "Volume, account, pool, or resource group not found. Verify they exist and you have access.",
        RequestFailedException reqEx => reqEx.Message,
        _ => base.GetErrorMessage(ex)
    };

    public record VolumeUpdateCommandResult([property: JsonPropertyName("volume")] NetAppVolumeCreateResult Volume);
}
