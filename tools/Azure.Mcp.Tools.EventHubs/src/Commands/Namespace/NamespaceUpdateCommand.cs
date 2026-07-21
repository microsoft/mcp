// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json;
using Azure.Mcp.Core.Commands.Subscription;
using Azure.Mcp.Core.Services.Azure.Subscription;
using Azure.Mcp.Tools.EventHubs.Options.Namespace;
using Azure.Mcp.Tools.EventHubs.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Mcp.Core.Commands;
using Microsoft.Mcp.Core.Models.Command;

namespace Azure.Mcp.Tools.EventHubs.Commands.Namespace;

[CommandMetadata(
    Id = "225eb25d-52c5-4c3a-9eb4-066cf2b9da84",
    Name = "update",
    Title = "Create or Update Event Hubs Namespace",
    Description = """
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
        """,
    Destructive = true,
    Idempotent = true,
    OpenWorld = false,
    ReadOnly = false,
    Secret = false,
    LocalRequired = false)]
public sealed class NamespaceUpdateCommand(ILogger<NamespaceUpdateCommand> logger, IEventHubsService service, ISubscriptionResolver subscriptionResolver)
    : SubscriptionCommand<NamespaceUpdateOptions, NamespaceUpdateCommand.NamespaceUpdateCommandResult>(subscriptionResolver)
{
    private readonly IEventHubsService _service = service;
    private readonly ILogger<NamespaceUpdateCommand> _logger = logger;

    public override void ValidateOptions(NamespaceUpdateOptions options, ValidationResult validationResult)
    {
        base.ValidateOptions(options, validationResult);

        // Validate that at least one update property is provided (for update scenario)
        if (string.IsNullOrEmpty(options.Location) &&
            string.IsNullOrEmpty(options.SkuName) &&
            string.IsNullOrEmpty(options.SkuTier) &&
            !options.SkuCapacity.HasValue &&
            !options.IsAutoInflateEnabled.HasValue &&
            !options.MaximumThroughputUnits.HasValue &&
            !options.KafkaEnabled.HasValue &&
            !options.ZoneRedundant.HasValue &&
            string.IsNullOrEmpty(options.Tags))
        {
            validationResult.Errors.Add("At least one update property must be provided (location, sku-name, sku-tier, sku-capacity, is-auto-inflate-enabled, maximum-throughput-units, kafka-enabled, zone-redundant, or tags).");
        }

        // Validate auto-inflate settings
        if (options.IsAutoInflateEnabled == true && !options.MaximumThroughputUnits.HasValue)
        {
            validationResult.Errors.Add("When enabling auto-inflate, maximum-throughput-units must be specified.");
        }
    }

    public override async Task<CommandResponse> ExecuteAsync(CommandContext context, NamespaceUpdateOptions options, CancellationToken cancellationToken)
    {
        try
        {
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

            var updatedNamespace = await _service.CreateOrUpdateNamespaceAsync(
                options.Namespace,
                options.ResourceGroup,
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
                options.RetryPolicy,
                cancellationToken);

            context.Response.Results = ResponseResult.Create(
                new(updatedNamespace),
                EventHubsJsonContext.Default.NamespaceUpdateCommandResult);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating Event Hubs namespace '{NamespaceName}' in resource group '{ResourceGroup}'",
                options.Namespace, options.ResourceGroup);
            HandleException(context, ex);
        }

        return context.Response;
    }

    public sealed record NamespaceUpdateCommandResult(Models.Namespace Namespace);
}
