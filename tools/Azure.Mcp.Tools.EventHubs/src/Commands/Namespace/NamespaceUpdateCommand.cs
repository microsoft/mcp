// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net;
using Azure.Mcp.Core.Commands;
using Azure.Mcp.Core.Extensions;
using Azure.Mcp.Core.Models.Option;
using Azure.Mcp.Tools.EventHubs.Options;
using Azure.Mcp.Tools.EventHubs.Options.Namespace;
using Azure.Mcp.Tools.EventHubs.Services;
using Microsoft.Extensions.Logging;

namespace Azure.Mcp.Tools.EventHubs.Commands.Namespace;

public sealed class NamespaceUpdateCommand(ILogger<NamespaceUpdateCommand> logger)
    : BaseEventHubsCommand<NamespaceUpdateOptions>
{
    private const string CommandTitle = "Create or Update Event Hubs Namespace";

    private readonly ILogger<NamespaceUpdateCommand> _logger = logger;

    public override string Name => "update";

    public override string Description =>
        """
        Create or Update a Namespace. This tool will either create a Namespace resource or 
        update a pre-existing Namespace resource within the specified resource group, depending on 
        whether or not the specified Namespace already exists. This tool may modify existing 
        configurations, and is considered to be destructive. This is a potentially long-running operation.
        
        When updating an existing namespace, you only need to provide the properties you want to change.
        Unspecified properties will retain their existing values. At least one update property must be provided.
        
        Common update scenarios:
        - Scale up/down by changing SKU tier or capacity
        - Enable/disable auto-inflate and set maximum throughput units
        - Enable/disable Kafka support
        - Modify tags for resource management
        - Enable/disable zone redundancy (Premium SKU only)
        """;

    public override string Title => CommandTitle;

    public override ToolMetadata Metadata => new()
    {
        OpenWorld = false,
        Destructive = true,    // Modifies existing resources
        Idempotent = true,     // Same parameters produce same results
        ReadOnly = false,      // Modifies data
        Secret = false,        // Returns non-sensitive information
        LocalRequired = false  // Pure cloud API calls
    };

    protected override void RegisterOptions(Command command)
    {
        base.RegisterOptions(command);
        command.Options.Add(OptionDefinitions.Common.ResourceGroup.AsRequired());
        command.Options.Add(EventHubsOptionDefinitions.NamespaceOption.AsRequired());
        command.Options.Add(EventHubsOptionDefinitions.LocationOption);
        command.Options.Add(EventHubsOptionDefinitions.SkuNameOption);
        command.Options.Add(EventHubsOptionDefinitions.SkuTierOption);
        command.Options.Add(EventHubsOptionDefinitions.SkuCapacityOption);
        command.Options.Add(EventHubsOptionDefinitions.IsAutoInflateEnabledOption);
        command.Options.Add(EventHubsOptionDefinitions.MaximumThroughputUnitsOption);
        command.Options.Add(EventHubsOptionDefinitions.KafkaEnabledOption);
        command.Options.Add(EventHubsOptionDefinitions.ZoneRedundantOption);
        command.Options.Add(EventHubsOptionDefinitions.TagsOption);

        command.Validators.Add(commandResult =>
        {
            // Validate that at least one update property is provided (for update scenario)
            var location = commandResult.GetValueOrDefault<string>(EventHubsOptionDefinitions.LocationOption.Name);
            var skuName = commandResult.GetValueOrDefault<string>(EventHubsOptionDefinitions.SkuNameOption.Name);
            var skuTier = commandResult.GetValueOrDefault<string>(EventHubsOptionDefinitions.SkuTierOption.Name);
            var skuCapacity = commandResult.GetValueOrDefault<int?>(EventHubsOptionDefinitions.SkuCapacityOption.Name);
            var isAutoInflateEnabled = commandResult.GetValueOrDefault<bool?>(EventHubsOptionDefinitions.IsAutoInflateEnabledOption.Name);
            var maximumThroughputUnits = commandResult.GetValueOrDefault<int?>(EventHubsOptionDefinitions.MaximumThroughputUnitsOption.Name);
            var kafkaEnabled = commandResult.GetValueOrDefault<bool?>(EventHubsOptionDefinitions.KafkaEnabledOption.Name);
            var zoneRedundant = commandResult.GetValueOrDefault<bool?>(EventHubsOptionDefinitions.ZoneRedundantOption.Name);
            var tags = commandResult.GetValueOrDefault<string>(EventHubsOptionDefinitions.TagsOption.Name);

            if (string.IsNullOrEmpty(location) &&
                string.IsNullOrEmpty(skuName) &&
                string.IsNullOrEmpty(skuTier) &&
                !skuCapacity.HasValue &&
                !isAutoInflateEnabled.HasValue &&
                !maximumThroughputUnits.HasValue &&
                !kafkaEnabled.HasValue &&
                !zoneRedundant.HasValue &&
                string.IsNullOrEmpty(tags))
            {
                commandResult.AddError("At least one update property must be provided (location, sku-name, sku-tier, sku-capacity, is-auto-inflate-enabled, maximum-throughput-units, kafka-enabled, zone-redundant, or tags).");
            }

            // Validate auto-inflate settings
            if (isAutoInflateEnabled == true && !maximumThroughputUnits.HasValue)
            {
                commandResult.AddError("When enabling auto-inflate, maximum-throughput-units must be specified.");
            }
        });
    }

    protected override NamespaceUpdateOptions BindOptions(ParseResult parseResult)
    {
        var options = base.BindOptions(parseResult);
        options.ResourceGroup ??= parseResult.GetValueOrDefault<string>(OptionDefinitions.Common.ResourceGroup.Name);
        options.Namespace = parseResult.GetValueOrDefault<string>(EventHubsOptionDefinitions.NamespaceOption.Name);
        options.Location = parseResult.GetValueOrDefault<string>(EventHubsOptionDefinitions.LocationOption.Name);
        options.SkuName = parseResult.GetValueOrDefault<string>(EventHubsOptionDefinitions.SkuNameOption.Name);
        options.SkuTier = parseResult.GetValueOrDefault<string>(EventHubsOptionDefinitions.SkuTierOption.Name);
        options.SkuCapacity = parseResult.GetValueOrDefault<int?>(EventHubsOptionDefinitions.SkuCapacityOption.Name);
        options.IsAutoInflateEnabled = parseResult.GetValueOrDefault<bool?>(EventHubsOptionDefinitions.IsAutoInflateEnabledOption.Name);
        options.MaximumThroughputUnits = parseResult.GetValueOrDefault<int?>(EventHubsOptionDefinitions.MaximumThroughputUnitsOption.Name);
        options.KafkaEnabled = parseResult.GetValueOrDefault<bool?>(EventHubsOptionDefinitions.KafkaEnabledOption.Name);
        options.ZoneRedundant = parseResult.GetValueOrDefault<bool?>(EventHubsOptionDefinitions.ZoneRedundantOption.Name);
        options.Tags = parseResult.GetValueOrDefault<string>(EventHubsOptionDefinitions.TagsOption.Name);
        return options;
    }

    public override async Task<CommandResponse> ExecuteAsync(CommandContext context, ParseResult parseResult)
    {
        if (!Validate(parseResult.CommandResult, context.Response).IsValid)
        {
            return context.Response;
        }

        var options = BindOptions(parseResult);

        try
        {
            var eventHubsService = context.GetService<IEventHubsService>();

            // Parse tags if provided
            Dictionary<string, string>? tags = null;
            if (!string.IsNullOrEmpty(options.Tags))
            {
                try
                {
                    tags = JsonSerializer.Deserialize(options.Tags, EventHubsJsonContext.Default.DictionaryStringString);
                }
                catch (JsonException ex)
                {
                    throw new ArgumentException($"Invalid tags JSON format: {ex.Message}", nameof(options.Tags));
                }
            }

            var updatedNamespace = await eventHubsService.CreateOrUpdateNamespaceAsync(
                options.Namespace!,
                options.ResourceGroup!,
                options.Subscription!,
                options.Location,
                options.SkuName,
                options.SkuTier,
                options.SkuCapacity,
                options.IsAutoInflateEnabled,
                options.MaximumThroughputUnits,
                options.KafkaEnabled,
                options.ZoneRedundant,
                tags,
                options.Tenant,
                options.RetryPolicy);

            context.Response.Results = ResponseResult.Create(
                new NamespaceUpdateCommandResult(updatedNamespace),
                EventHubsJsonContext.Default.NamespaceUpdateCommandResult);
            context.Response.Status = HttpStatusCode.OK;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating Event Hubs namespace '{NamespaceName}' in resource group '{ResourceGroup}'",
                options.Namespace, options.ResourceGroup);
            HandleException(context, ex);
        }

        return context.Response;
    }

    internal record NamespaceUpdateCommandResult(Models.Namespace Namespace);
}
