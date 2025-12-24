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

public sealed class StorageSyncServiceGetCommand(ILogger<StorageSyncServiceGetCommand> logger, IStorageSyncService service) : BaseStorageSyncCommand<StorageSyncServiceGetOptions>
{
    private const string CommandTitle = "Get Storage Sync Service";
    private readonly IStorageSyncService _service = service;
    private readonly ILogger<StorageSyncServiceGetCommand> _logger = logger;

    public override string Id => "77734a55-8290-4c16-8b37-cf37277f018f";

    public override string Name => "get";

    public override string Description => "Retrieve Azure Storage Sync service details or list all Storage Sync services. Use --name to get a specific service, or omit it to list all services in the subscription or resource group. Shows service properties, location, provisioning state, and configuration.";

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
        command.Options.Add(OptionDefinitions.Common.ResourceGroup.AsOptional());
        command.Options.Add(StorageSyncOptionDefinitions.StorageSyncService.Name.AsOptional());
    }

    protected override StorageSyncServiceGetOptions BindOptions(ParseResult parseResult)
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
            // If name is provided, get specific service
            if (!string.IsNullOrEmpty(options.Name))
            {
                if (string.IsNullOrEmpty(options.ResourceGroup))
                {
                    context.Response.Status = HttpStatusCode.BadRequest;
                    context.Response.Message = "Resource group is required when getting a specific storage sync service by name";
                    return context.Response;
                }

                _logger.LogInformation("Getting storage sync service. Subscription: {Subscription}, ResourceGroup: {ResourceGroup}, ServiceName: {ServiceName}",
                    options.Subscription, options.ResourceGroup, options.Name);

                var service = await _service.GetStorageSyncServiceAsync(
                    options.Subscription!,
                    options.ResourceGroup!,
                    options.Name!,
                    options.Tenant,
                    options.RetryPolicy,
                    cancellationToken);

                if (service == null)
                {
                    context.Response.Status = HttpStatusCode.NotFound;
                    context.Response.Message = "Storage sync service not found";
                    return context.Response;
                }

                var singleResult = new StorageSyncServiceGetCommandResult([service]);
                context.Response.Results = ResponseResult.Create(singleResult, StorageSyncJsonContext.Default.StorageSyncServiceGetCommandResult);
            }
            else
            {
                // List all services
                _logger.LogInformation("Listing storage sync services. Subscription: {Subscription}, ResourceGroup: {ResourceGroup}",
                    options.Subscription, options.ResourceGroup);

                var services = await _service.ListStorageSyncServicesAsync(
                    options.Subscription!,
                    options.ResourceGroup,
                    options.Tenant,
                    options.RetryPolicy,
                    cancellationToken);

                var results = new StorageSyncServiceGetCommandResult(services ?? []);
                context.Response.Results = ResponseResult.Create(results, StorageSyncJsonContext.Default.StorageSyncServiceGetCommandResult);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting storage sync service(s)");
            HandleException(context, ex);
        }

        return context.Response;
    }

    [JsonSerializable(typeof(StorageSyncServiceGetCommandResult))]
    internal record StorageSyncServiceGetCommandResult(List<StorageSyncServiceDataSchema> Results);
}
