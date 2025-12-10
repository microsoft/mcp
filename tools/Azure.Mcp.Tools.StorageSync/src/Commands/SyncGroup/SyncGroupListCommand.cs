// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json.Serialization;
using Azure.Mcp.Core.Commands.Subscription;
using Azure.Mcp.Core.Extensions;
using Azure.Mcp.Core.Models.Option;
using Azure.Mcp.Tools.StorageSync.Models;
using Azure.Mcp.Tools.StorageSync.Options;
using Azure.Mcp.Tools.StorageSync.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Mcp.Core.Commands;
using Microsoft.Mcp.Core.Models.Command;
using Microsoft.Mcp.Core.Models.Option;

namespace Azure.Mcp.Tools.StorageSync.Commands.SyncGroup;

public sealed class SyncGroupListCommand(ILogger<SyncGroupListCommand> logger, IStorageSyncService service) : BaseStorageSyncCommand<SyncGroupListOptions>
{
    private const string CommandTitle = "List Sync Groups";
    private readonly IStorageSyncService _service = service;
    private readonly ILogger<SyncGroupListCommand> _logger = logger;

    public override string Id => "d0g6h8f2-5e7i-7g1h-2f6i-9h2g5i8j2k3l";

    public override string Name => "list";

    public override string Description => "List all sync groups in a Storage Sync service.";

    public override string Title => CommandTitle;

    public override ToolMetadata Metadata => new()
    {
        Destructive = false,
        Idempotent = true,
        OpenWorld = false,
        ReadOnly = true,
        LocalRequired = false,
        Secret = false
    };

    protected override void RegisterOptions(Command command)
    {
        base.RegisterOptions(command);
        command.Options.Add(OptionDefinitions.Common.ResourceGroup.AsRequired());
        command.Options.Add(StorageSyncOptionDefinitions.StorageSyncService.Name.AsRequired());
    }

    protected override SyncGroupListOptions BindOptions(ParseResult parseResult)
    {
        var options = base.BindOptions(parseResult);
        options.ResourceGroup ??= parseResult.GetValueOrDefault<string>(OptionDefinitions.Common.ResourceGroup.Name);
        options.StorageSyncServiceName = parseResult.GetValueOrDefault<string>(StorageSyncOptionDefinitions.StorageSyncService.Name.Name);
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
            _logger.LogInformation("Listing sync groups. Subscription: {Subscription}, ResourceGroup: {ResourceGroup}, ServiceName: {ServiceName}",
                options.Subscription, options.ResourceGroup, options.StorageSyncServiceName);

            var syncGroups = await _service.ListSyncGroupsAsync(
                options.Subscription!,
                options.ResourceGroup!,
                options.StorageSyncServiceName!,
                options.Tenant,
                options.RetryPolicy,
                cancellationToken);

            var results = new SyncGroupListCommandResult(syncGroups ?? []);
            context.Response.Results = ResponseResult.Create(results, StorageSyncJsonContext.Default.SyncGroupListCommandResult);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error listing sync groups");
            HandleException(context, ex);
        }

        return context.Response;
    }

    [JsonSerializable(typeof(SyncGroupListCommandResult))]
    internal record SyncGroupListCommandResult(List<SyncGroupData> Results);
}
