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

namespace Azure.Mcp.Tools.StorageSync.Commands.StorageSyncService;

public sealed class StorageSyncServiceCreateCommand(ILogger<StorageSyncServiceCreateCommand> logger, IStorageSyncService service) : BaseStorageSyncCommand<StorageSyncServiceCreateOptions>
{
    private const string CommandTitle = "Create Storage Sync Service";
    private readonly IStorageSyncService _service = service;
    private readonly ILogger<StorageSyncServiceCreateCommand> _logger = logger;

    public override string Id => "7c76387f-c62e-48d1-af3b-d444d6b3b79c";

    public override string Name => "create";

    public override string Description => "Create a new Azure Storage Sync service resource in a resource group. This is the top-level service container that manages sync groups, registered servers, and synchronization workflows.";

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
        command.Options.Add(StorageSyncOptionDefinitions.StorageSyncService.Location.AsRequired());
    }

    protected override StorageSyncServiceCreateOptions BindOptions(ParseResult parseResult)
    {
        var options = base.BindOptions(parseResult);
        options.ResourceGroup ??= parseResult.GetValueOrDefault<string>(OptionDefinitions.Common.ResourceGroup.Name);
        options.Name = parseResult.GetValueOrDefault<string>(StorageSyncOptionDefinitions.StorageSyncService.Name.Name);
        options.Location = parseResult.GetValueOrDefault<string>(StorageSyncOptionDefinitions.StorageSyncService.Location.Name);
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
            _logger.LogInformation("Creating storage sync service. Subscription: {Subscription}, ResourceGroup: {ResourceGroup}, ServiceName: {ServiceName}",
                options.Subscription, options.ResourceGroup, options.Name);

            var service = await _service.CreateStorageSyncServiceAsync(
                options.Subscription!,
                options.ResourceGroup!,
                options.Name!,
                options.Location!,
                null,
                options.Tenant,
                options.RetryPolicy,
                cancellationToken);

            var results = new StorageSyncServiceCreateCommandResult(service);
            context.Response.Results = ResponseResult.Create(results, StorageSyncJsonContext.Default.StorageSyncServiceCreateCommandResult);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating storage sync service");
            HandleException(context, ex);
        }

        return context.Response;
    }

    [JsonSerializable(typeof(StorageSyncServiceCreateCommandResult))]
    internal record StorageSyncServiceCreateCommandResult(StorageSyncServiceDataSchema Result);
}
