// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net;
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

public sealed class SyncGroupGetCommand(ILogger<SyncGroupGetCommand> logger, IStorageSyncService service) : BaseStorageSyncCommand<SyncGroupGetOptions>
{
    private const string CommandTitle = "Get Sync Group";
    private readonly IStorageSyncService _service = service;
    private readonly ILogger<SyncGroupGetCommand> _logger = logger;

    public override string Id => "95ce2336-19e6-40fb-a3ea-e2a76772036b";

    public override string Name => "get";

    public override string Description => "Get details about a specific sync group or list all sync groups. If --sync-group-name is provided, returns a specific sync group; otherwise, lists all sync groups in the Storage Sync service.";

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
        command.Options.Add(StorageSyncOptionDefinitions.SyncGroup.Name.AsOptional());
    }

    protected override SyncGroupGetOptions BindOptions(ParseResult parseResult)
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
            // If sync group name is provided, get specific sync group
            if (!string.IsNullOrEmpty(options.SyncGroupName))
            {
                _logger.LogInformation("Getting sync group. Subscription: {Subscription}, ResourceGroup: {ResourceGroup}, ServiceName: {ServiceName}, GroupName: {GroupName}",
                    options.Subscription, options.ResourceGroup, options.StorageSyncServiceName, options.SyncGroupName);

                var syncGroup = await _service.GetSyncGroupAsync(
                    options.Subscription!,
                    options.ResourceGroup!,
                    options.StorageSyncServiceName!,
                    options.SyncGroupName!,
                    options.Tenant,
                    options.RetryPolicy,
                    cancellationToken);

                if (syncGroup == null)
                {
                    context.Response.Status = HttpStatusCode.NotFound;
                    context.Response.Message = "Sync group not found";
                    return context.Response;
                }

                var singleResult = new SyncGroupGetCommandResult([syncGroup]);
                context.Response.Results = ResponseResult.Create(singleResult, StorageSyncJsonContext.Default.SyncGroupGetCommandResult);
            }
            else
            {
                // List all sync groups
                _logger.LogInformation("Listing sync groups. Subscription: {Subscription}, ResourceGroup: {ResourceGroup}, ServiceName: {ServiceName}",
                    options.Subscription, options.ResourceGroup, options.StorageSyncServiceName);

                var syncGroups = await _service.ListSyncGroupsAsync(
                    options.Subscription!,
                    options.ResourceGroup!,
                    options.StorageSyncServiceName!,
                    options.Tenant,
                    options.RetryPolicy,
                    cancellationToken);

                var results = new SyncGroupGetCommandResult(syncGroups ?? []);
                context.Response.Results = ResponseResult.Create(results, StorageSyncJsonContext.Default.SyncGroupGetCommandResult);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting sync group(s)");
            HandleException(context, ex);
        }

        return context.Response;
    }

    [JsonSerializable(typeof(SyncGroupGetCommandResult))]
    internal record SyncGroupGetCommandResult(List<SyncGroupDataSchema> Results);
}
