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

namespace Azure.Mcp.Tools.StorageSync.Commands.CloudEndpoint;

public sealed class CloudEndpointDeleteCommand(ILogger<CloudEndpointDeleteCommand> logger, IStorageSyncService service) : BaseStorageSyncCommand<CloudEndpointDeleteOptions>
{
    private const string CommandTitle = "Delete Cloud Endpoint";
    private readonly IStorageSyncService _service = service;
    private readonly ILogger<CloudEndpointDeleteCommand> _logger = logger;

    public override string Id => "f5e76906-cc2a-41a4-b4f9-498221aaaf2e";

    public override string Name => "delete";

    public override string Description => "Delete a cloud endpoint from a sync group.";

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
    }

    protected override CloudEndpointDeleteOptions BindOptions(ParseResult parseResult)
    {
        var options = base.BindOptions(parseResult);
        options.ResourceGroup ??= parseResult.GetValueOrDefault<string>(OptionDefinitions.Common.ResourceGroup.Name);
        options.StorageSyncServiceName = parseResult.GetValueOrDefault<string>(StorageSyncOptionDefinitions.StorageSyncService.Name.Name);
        options.SyncGroupName = parseResult.GetValueOrDefault<string>(StorageSyncOptionDefinitions.SyncGroup.Name.Name);
        options.CloudEndpointName = parseResult.GetValueOrDefault<string>(StorageSyncOptionDefinitions.CloudEndpoint.Name.Name);
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
            _logger.LogInformation("Deleting cloud endpoint. Subscription: {Subscription}, ResourceGroup: {ResourceGroup}, ServiceName: {ServiceName}, GroupName: {GroupName}, EndpointName: {EndpointName}",
                options.Subscription, options.ResourceGroup, options.StorageSyncServiceName, options.SyncGroupName, options.CloudEndpointName);

            await _service.DeleteCloudEndpointAsync(
                options.Subscription!,
                options.ResourceGroup!,
                options.StorageSyncServiceName!,
                options.SyncGroupName!,
                options.CloudEndpointName!,
                options.Tenant,
                options.RetryPolicy,
                cancellationToken);

            context.Response.Message = "Cloud endpoint deleted successfully";
            var results = new CloudEndpointDeleteCommandResult("Cloud endpoint deleted successfully");
            context.Response.Results = ResponseResult.Create(results, StorageSyncJsonContext.Default.CloudEndpointDeleteCommandResult);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting cloud endpoint");
            HandleException(context, ex);
        }

        return context.Response;
    }

    [JsonSerializable(typeof(CloudEndpointDeleteCommandResult))]
    internal record CloudEndpointDeleteCommandResult(string Message);
}
