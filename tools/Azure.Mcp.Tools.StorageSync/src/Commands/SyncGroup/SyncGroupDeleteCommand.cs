// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json.Serialization;
using Azure.Mcp.Core.Commands.Subscription;
using Azure.Mcp.Core.Extensions;
using Azure.Mcp.Core.Models.Option;
using Azure.Mcp.Tools.StorageSync.Options;
using Azure.Mcp.Tools.StorageSync.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Mcp.Core.Commands;
using Microsoft.Mcp.Core.Models.Command;
using Microsoft.Mcp.Core.Models.Option;

namespace Azure.Mcp.Tools.StorageSync.Commands.SyncGroup;

public sealed class SyncGroupDeleteCommand(ILogger<SyncGroupDeleteCommand> logger, IStorageSyncService service) : BaseStorageSyncCommand<SyncGroupDeleteOptions>
{
    private const string CommandTitle = "Delete Sync Group";
    private readonly IStorageSyncService _service = service;
    private readonly ILogger<SyncGroupDeleteCommand> _logger = logger;

    public override string Id => "c8f91bd7-ea1d-4af4-9703-fe83c43b34b5";

    public override string Name => "delete";

    public override string Description => "Remove a sync group from a Storage Sync service. Deleting a sync group also removes all associated cloud endpoints and server endpoints within that group.";

    public override string Title => CommandTitle;

    public override ToolMetadata Metadata => new()
    {
        Destructive = true,
        Idempotent = false,
        OpenWorld = false,
        ReadOnly = false,
        LocalRequired = false,
        Secret = false
    };

    protected override void RegisterOptions(Command command)
    {
        base.RegisterOptions(command);
        command.Options.Add(OptionDefinitions.Common.ResourceGroup.AsRequired());
        command.Options.Add(StorageSyncOptionDefinitions.StorageSyncService.Name.AsRequired());
        command.Options.Add(StorageSyncOptionDefinitions.SyncGroup.Name.AsRequired());
    }

    protected override SyncGroupDeleteOptions BindOptions(ParseResult parseResult)
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
            _logger.LogInformation("Deleting sync group. Subscription: {Subscription}, ResourceGroup: {ResourceGroup}, ServiceName: {ServiceName}, GroupName: {GroupName}",
                options.Subscription, options.ResourceGroup, options.StorageSyncServiceName, options.SyncGroupName);

            await _service.DeleteSyncGroupAsync(
                options.Subscription!,
                options.ResourceGroup!,
                options.StorageSyncServiceName!,
                options.SyncGroupName!,
                options.Tenant,
                options.RetryPolicy,
                cancellationToken);

            context.Response.Message = "Sync group deleted successfully";
            var results = new SyncGroupDeleteCommandResult("Sync group deleted successfully");
            context.Response.Results = ResponseResult.Create(results, StorageSyncJsonContext.Default.SyncGroupDeleteCommandResult);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting sync group");
            HandleException(context, ex);
        }

        return context.Response;
    }

    [JsonSerializable(typeof(SyncGroupDeleteCommandResult))]
    internal record SyncGroupDeleteCommandResult(string Message);
}
