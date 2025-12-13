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

namespace Azure.Mcp.Tools.StorageSync.Commands.StorageSyncService;

public sealed class StorageSyncServiceUpdateCommand(ILogger<StorageSyncServiceUpdateCommand> logger, IStorageSyncService service) : BaseStorageSyncCommand<StorageSyncServiceUpdateOptions>
{
    private const string CommandTitle = "Update Storage Sync Service";
    private readonly IStorageSyncService _service = service;
    private readonly ILogger<StorageSyncServiceUpdateCommand> _logger = logger;

    public override string Id => "b8e4f6d0-3c5g-5e9f-0d4g-7f0e3g6h0i1j";

    public override string Name => "update";

    public override string Description => "Update properties of an existing Azure Storage Sync service.";

    public override string Title => CommandTitle;

    public override ToolMetadata Metadata => new()
    {
        Destructive = false,
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
    }

    protected override StorageSyncServiceUpdateOptions BindOptions(ParseResult parseResult)
    {
        var options = base.BindOptions(parseResult);
        options.ResourceGroup ??= parseResult.GetValueOrDefault<string>(OptionDefinitions.Common.ResourceGroup.Name);
        options.Name = parseResult.GetValueOrDefault<string>(StorageSyncOptionDefinitions.StorageSyncService.Name.Name);
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
                null,
                options.Tenant,
                options.RetryPolicy,
                cancellationToken);

            var results = new StorageSyncServiceUpdateCommandResult(service);
            context.Response.Results = ResponseResult.Create(results, StorageSyncJsonContext.Default.StorageSyncServiceUpdateCommandResult);
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
