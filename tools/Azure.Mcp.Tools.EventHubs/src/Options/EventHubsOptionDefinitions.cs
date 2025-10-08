// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Azure.Mcp.Tools.EventHubs.Options;

public static class EventHubsOptionDefinitions
{
    public const string Namespace = "namespace";
    public const string Location = "location";
    public const string SkuName = "sku-name";
    public const string SkuTier = "sku-tier";
    public const string SkuCapacity = "sku-capacity";
    public const string IsAutoInflateEnabled = "is-auto-inflate-enabled";
    public const string MaximumThroughputUnits = "maximum-throughput-units";
    public const string KafkaEnabled = "kafka-enabled";
    public const string ZoneRedundant = "zone-redundant";
    public const string Tags = "tags";

    public static readonly Option<string> NamespaceOption = new(
        $"--{Namespace}"
    )
    {
        Description = "The name of the Event Hubs namespace to retrieve. Must be used with --resource-group option.",
        Required = false
    };

    public static readonly Option<string> LocationOption = new(
        $"--{Location}"
    )
    {
        Description = "The Azure region where the namespace is located (e.g., 'eastus', 'westus2').",
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

    public static readonly Option<int?> SkuCapacityOption = new(
        $"--{SkuCapacity}"
    )
    {
        Description = "The SKU capacity (throughput units) for the namespace. Valid range depends on the SKU.",
        Required = false
    };

    public static readonly Option<bool?> IsAutoInflateEnabledOption = new(
        $"--{IsAutoInflateEnabled}"
    )
    {
        Description = "Enable or disable auto-inflate for the namespace.",
        Required = false
    };

    public static readonly Option<int?> MaximumThroughputUnitsOption = new(
        $"--{MaximumThroughputUnits}"
    )
    {
        Description = "The maximum throughput units when auto-inflate is enabled.",
        Required = false
    };

    public static readonly Option<bool?> KafkaEnabledOption = new(
        $"--{KafkaEnabled}"
    )
    {
        Description = "Enable or disable Kafka for the namespace.",
        Required = false
    };

    public static readonly Option<bool?> ZoneRedundantOption = new(
        $"--{ZoneRedundant}"
    )
    {
        Description = "Enable or disable zone redundancy for the namespace.",
        Required = false
    };

    public static readonly Option<string> TagsOption = new(
        $"--{Tags}"
    )
    {
        Description = "Tags for the namespace in JSON format (e.g., '{\"key1\":\"value1\",\"key2\":\"value2\"}').",
        Required = false
    };
}
