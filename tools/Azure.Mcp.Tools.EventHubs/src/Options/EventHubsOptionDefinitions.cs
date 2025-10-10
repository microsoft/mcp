// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Azure.Mcp.Tools.EventHubs.Options;

public static class EventHubsOptionDefinitions
{
    public const string ConsumerGroup = "consumer-group";
    public const string EventHub = "eventhub";
    public const string EventHubStatus = "status";
    public const string IsAutoInflateEnabled = "is-auto-inflate-enabled";
    public const string KafkaEnabled = "kafka-enabled";
    public const string Location = "location";
    public const string MaximumThroughputUnits = "maximum-throughput-units";
    public const string MessageRetentionInHours = "message-retention-in-hours";
    public const string Namespace = "namespace";
    public const string PartitionCount = "partition-count";
    public const string SkuCapacity = "sku-capacity";
    public const string SkuName = "sku-name";
    public const string SkuTier = "sku-tier";
    public const string Tags = "tags";
    public const string UserMetadata = "user-metadata";
    public const string ZoneRedundant = "zone-redundant";

    public static readonly Option<string> ConsumerGroupOption = new(
        $"--{ConsumerGroup}"
    )
    {
        Description = "The name of the consumer group within the Event Hub.",
        Required = false
    };

    public static readonly Option<string> EventHubOption = new(
        $"--{EventHub}"
    )
    {
        Description = "The name of the Event Hub within the namespace.",
        Required = false
    };

    public static readonly Option<bool?> IsAutoInflateEnabledOption = new(
        $"--{IsAutoInflateEnabled}"
    )
    {
        Description = "Enable or disable auto-inflate for the namespace.",
        Required = false
    };

    public static readonly Option<bool?> KafkaEnabledOption = new(
        $"--{KafkaEnabled}"
    )
    {
        Description = "Enable or disable Kafka for the namespace.",
        Required = false
    };

    public static readonly Option<string> LocationOption = new(
        $"--{Location}"
    )
    {
        Description = "The Azure region where the namespace is located (e.g., 'eastus', 'westus2').",
        Required = false
    };

    public static readonly Option<int?> MaximumThroughputUnitsOption = new(
        $"--{MaximumThroughputUnits}"
    )
    {
        Description = "The maximum throughput units when auto-inflate is enabled.",
        Required = false
    };

    public static readonly Option<long?> MessageRetentionInHoursOption = new(
        $"--{MessageRetentionInHours}"
    )
    {
        Description = "The message retention time in hours. Minimum is 1 hour, maximum depends on the namespace tier.",
        Required = false
    };

    public static readonly Option<string> NamespaceOption = new(
        $"--{Namespace}"
    )
    {
        Description = "The name of the Event Hubs namespace to retrieve. Must be used with --resource-group option.",
        Required = false
    };

    public static readonly Option<int?> PartitionCountOption = new(
        $"--{PartitionCount}"
    )
    {
        Description = "The number of partitions for the event hub. Must be between 1 and 32 (or higher based on namespace tier).",
        Required = false
    };

    public static readonly Option<int?> SkuCapacityOption = new(
        $"--{SkuCapacity}"
    )
    {
        Description = "The SKU capacity (throughput units) for the namespace. Valid range depends on the SKU.",
        Required = false
    };

    public static readonly Option<string> SkuNameOption = new(
        $"--{SkuName}"
    )
    {
        Description = "The SKU name for the namespace. Valid values: 'Basic', 'Standard', 'Premium'.",
        Required = false
    };

    public static readonly Option<string> SkuTierOption = new(
        $"--{SkuTier}"
    )
    {
        Description = "The SKU tier for the namespace. Valid values: 'Basic', 'Standard', 'Premium'.",
        Required = false
    };

    public static readonly Option<string> StatusOption = new(
        $"--{EventHubStatus}"
    )
    {
        Description = "The status of the event hub (Active, Disabled, etc.). Note: Status may be read-only in some operations.",
        Required = false
    };

    public static readonly Option<string> TagsOption = new(
        $"--{Tags}"
    )
    {
        Description = "Tags for the namespace in JSON format (e.g., '{\"key1\":\"value1\",\"key2\":\"value2\"}').",
        Required = false
    };

    public static readonly Option<string> UserMetadataOption = new(
        $"--{UserMetadata}"
    )
    {
        Description = "User metadata for the consumer group.",
        Required = false
    };

    public static readonly Option<bool?> ZoneRedundantOption = new(
        $"--{ZoneRedundant}"
    )
    {
        Description = "Enable or disable zone redundancy for the namespace.",
        Required = false
    };
}
