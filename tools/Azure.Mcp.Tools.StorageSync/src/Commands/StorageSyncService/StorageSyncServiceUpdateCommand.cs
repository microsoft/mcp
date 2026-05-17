// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json.Serialization;
using Azure.Mcp.Tools.StorageSync.Models;
using Azure.Mcp.Tools.StorageSync.Options;
using Azure.Mcp.Tools.StorageSync.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Mcp.Core.Commands;
using Microsoft.Mcp.Core.Extensions;
using Microsoft.Mcp.Core.Models.Command;
using Microsoft.Mcp.Core.Models.Option;

namespace Azure.Mcp.Tools.StorageSync.Commands.StorageSyncService;

[CommandMetadata(
    Id = "15db4769-1941-4b1e-9514-867b0f68eb2c",
    Name = "update",
    Title = "Update Storage Sync Service",
    Description = "Update properties of an existing Azure Storage Sync service.",
    Destructive = false,
    Idempotent = false,
    OpenWorld = false,
    ReadOnly = false,
    Secret = false,
    LocalRequired = false)]
public sealed class StorageSyncServiceUpdateCommand(ILogger<StorageSyncServiceUpdateCommand> logger, IStorageSyncService service) : BaseStorageSyncCommand<StorageSyncServiceUpdateOptions>
{
    private readonly IStorageSyncService _service = service;
    private readonly ILogger<StorageSyncServiceUpdateCommand> _logger = logger;

    protected override void RegisterOptions(Command command)
    {
        base.RegisterOptions(command);
        command.Options.Add(OptionDefinitions.Common.ResourceGroup.AsRequired());
        command.Options.Add(StorageSyncOptionDefinitions.StorageSyncService.Name.AsRequired());
        command.Options.Add(StorageSyncOptionDefinitions.StorageSyncService.IncomingTrafficPolicy.AsOptional());
        command.Options.Add(StorageSyncOptionDefinitions.StorageSyncService.Tags.AsOptional());
        command.Options.Add(StorageSyncOptionDefinitions.StorageSyncService.IdentityType.AsOptional());
    }

    protected override StorageSyncServiceUpdateOptions BindOptions(ParseResult parseResult)
    {
        var options = base.BindOptions(parseResult);
        options.ResourceGroup ??= parseResult.GetValueOrDefault<string>(OptionDefinitions.Common.ResourceGroup.Name);
        options.Name = parseResult.GetValueOrDefault<string>(StorageSyncOptionDefinitions.StorageSyncService.Name.Name);
        options.IncomingTrafficPolicy = parseResult.GetValueOrDefault<string>(StorageSyncOptionDefinitions.StorageSyncService.IncomingTrafficPolicy.Name);

        // Parse tags from string format "key1=value1 key2=value2"
        var tagsString = parseResult.GetValueOrDefault<string>(StorageSyncOptionDefinitions.StorageSyncService.Tags.Name);
        if (!string.IsNullOrEmpty(tagsString))
        {
            options.Tags = [];
            var tagPairs = tagsString.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            foreach (var pair in tagPairs)
            {
                var parts = pair.Split('=', 2);
                if (parts.Length == 2)
                {
                    options.Tags[parts[0]] = parts[1];
                }
            }
        }

        options.IdentityType = parseResult.GetValueOrDefault<string>(StorageSyncOptionDefinitions.StorageSyncService.IdentityType.Name);
        return options;
    }

    public override async Task<CommandResponse> ExecuteAsync(CommandContext context, ParseResult parseResult, CancellationToken cancellationToken)
    {
        if (!Validate(parseResult.CommandResult, context.Response).IsValid)
        {
            return context.Response;
        }

        var options = BindOptions(parseResult);

        try
        {
            _logger.LogInformation("Updating storage sync service. Subscription: {Subscription}, ResourceGroup: {ResourceGroup}, ServiceName: {ServiceName}",
                options.Subscription, options.ResourceGroup, options.Name);

            var service = await _service.UpdateStorageSyncServiceAsync(
                options.Subscription!,
                options.ResourceGroup!,
                options.Name!,
                options.IncomingTrafficPolicy,
                options.Tags,
                options.IdentityType,
                options.Tenant,
                options.RetryPolicy,
                cancellationToken);

            context.Response.Results = ResponseResult.Create(new(service), StorageSyncJsonContext.Default.StorageSyncServiceUpdateCommandResult);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating storage sync service");
            HandleException(context, ex);
        }

        return context.Response;
    }

    [JsonSerializable(typeof(StorageSyncServiceUpdateCommandResult))]
    internal record StorageSyncServiceUpdateCommandResult(StorageSyncServiceDataSchema Result);
}
