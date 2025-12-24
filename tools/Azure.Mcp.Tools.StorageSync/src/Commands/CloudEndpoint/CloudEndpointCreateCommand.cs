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

namespace Azure.Mcp.Tools.StorageSync.Commands.CloudEndpoint;

public sealed class CloudEndpointCreateCommand(ILogger<CloudEndpointCreateCommand> logger, IStorageSyncService service) : BaseStorageSyncCommand<CloudEndpointCreateOptions>
{
    private const string CommandTitle = "Create Cloud Endpoint";
    private readonly IStorageSyncService _service = service;
    private readonly ILogger<CloudEndpointCreateCommand> _logger = logger;

    public override string Id => "df0d4ae3-519a-44f1-ad30-d25a0985e9c2";

    public override string Name => "create";

    public override string Description => "Add a cloud endpoint to a sync group by connecting an Azure File Share. Cloud endpoints represent the Azure storage side of the sync relationship.";

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
        command.Options.Add(StorageSyncOptionDefinitions.CloudEndpoint.Name.AsRequired());
        command.Options.Add(StorageSyncOptionDefinitions.CloudEndpoint.StorageAccountResourceId.AsRequired());
        command.Options.Add(StorageSyncOptionDefinitions.CloudEndpoint.AzureFileShareName.AsRequired());
    }

    protected override CloudEndpointCreateOptions BindOptions(ParseResult parseResult)
    {
        var options = base.BindOptions(parseResult);
        options.ResourceGroup ??= parseResult.GetValueOrDefault<string>(OptionDefinitions.Common.ResourceGroup.Name);
        options.StorageSyncServiceName = parseResult.GetValueOrDefault<string>(StorageSyncOptionDefinitions.StorageSyncService.Name.Name);
        options.SyncGroupName = parseResult.GetValueOrDefault<string>(StorageSyncOptionDefinitions.SyncGroup.Name.Name);
        options.CloudEndpointName = parseResult.GetValueOrDefault<string>(StorageSyncOptionDefinitions.CloudEndpoint.Name.Name);
        options.StorageAccountResourceId = parseResult.GetValueOrDefault<string>(StorageSyncOptionDefinitions.CloudEndpoint.StorageAccountResourceId.Name);
        options.AzureFileShareName = parseResult.GetValueOrDefault<string>(StorageSyncOptionDefinitions.CloudEndpoint.AzureFileShareName.Name);
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
            _logger.LogInformation("Creating cloud endpoint. Subscription: {Subscription}, ResourceGroup: {ResourceGroup}, ServiceName: {ServiceName}, GroupName: {GroupName}, EndpointName: {EndpointName}",
                options.Subscription, options.ResourceGroup, options.StorageSyncServiceName, options.SyncGroupName, options.CloudEndpointName);

            var endpoint = await _service.CreateCloudEndpointAsync(
                options.Subscription!,
                options.ResourceGroup!,
                options.StorageSyncServiceName!,
                options.SyncGroupName!,
                options.CloudEndpointName!,
                options.StorageAccountResourceId!,
                options.AzureFileShareName!,
                options.Tenant,
                options.RetryPolicy,
                cancellationToken);

            var results = new CloudEndpointCreateCommandResult(endpoint);
            context.Response.Results = ResponseResult.Create(results, StorageSyncJsonContext.Default.CloudEndpointCreateCommandResult);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating cloud endpoint");
            HandleException(context, ex);
        }

        return context.Response;
    }

    [JsonSerializable(typeof(CloudEndpointCreateCommandResult))]
    internal record CloudEndpointCreateCommandResult(CloudEndpointDataSchema Result);
}
