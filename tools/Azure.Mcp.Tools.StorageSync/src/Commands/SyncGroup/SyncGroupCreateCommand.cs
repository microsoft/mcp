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

namespace Azure.Mcp.Tools.StorageSync.Commands.SyncGroup;

[CommandMetadata(
    Id = "3572833c-4fc2-4bb9-9eed-52ae8b8899b8",
    Name = "create",
    Title = "Create Sync Group",
    Description = "Create a sync group within an existing Storage Sync service. Sync groups define a sync topology and contain cloud endpoints (Azure File Shares) and server endpoints (local server paths) that sync together.",
    Destructive = true,
    Idempotent = false,
    OpenWorld = false,
    ReadOnly = false,
    Secret = false,
    LocalRequired = false)]
public sealed class SyncGroupCreateCommand(ILogger<SyncGroupCreateCommand> logger, IStorageSyncService service) : BaseStorageSyncCommand<SyncGroupCreateOptions>
{
    private readonly IStorageSyncService _service = service;
    private readonly ILogger<SyncGroupCreateCommand> _logger = logger;

    protected override void RegisterOptions(Command command)
    {
        base.RegisterOptions(command);
        command.Options.Add(OptionDefinitions.Common.ResourceGroup.AsRequired());
        command.Options.Add(StorageSyncOptionDefinitions.StorageSyncService.Name.AsRequired());
        command.Options.Add(StorageSyncOptionDefinitions.SyncGroup.Name.AsRequired());
    }

    protected override SyncGroupCreateOptions BindOptions(ParseResult parseResult)
    {
        var options = base.BindOptions(parseResult);
        options.ResourceGroup ??= parseResult.GetValueOrDefault<string>(OptionDefinitions.Common.ResourceGroup.Name);
        options.StorageSyncServiceName = parseResult.GetValueOrDefault<string>(StorageSyncOptionDefinitions.StorageSyncService.Name.Name);
        options.SyncGroupName = parseResult.GetValueOrDefault<string>(StorageSyncOptionDefinitions.SyncGroup.Name.Name);
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
            _logger.LogInformation("Creating sync group. Subscription: {Subscription}, ResourceGroup: {ResourceGroup}, ServiceName: {ServiceName}, GroupName: {GroupName}",
                options.Subscription, options.ResourceGroup, options.StorageSyncServiceName, options.SyncGroupName);

            var syncGroup = await _service.CreateSyncGroupAsync(
                options.Subscription!,
                options.ResourceGroup!,
                options.StorageSyncServiceName!,
                options.SyncGroupName!,
                options.Tenant,
                options.RetryPolicy,
                cancellationToken);

            context.Response.Results = ResponseResult.Create(new(syncGroup), StorageSyncJsonContext.Default.SyncGroupCreateCommandResult);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating sync group");
            HandleException(context, ex);
        }

        return context.Response;
    }

    [JsonSerializable(typeof(SyncGroupCreateCommandResult))]
    internal record SyncGroupCreateCommandResult(SyncGroupDataSchema Result);
}
