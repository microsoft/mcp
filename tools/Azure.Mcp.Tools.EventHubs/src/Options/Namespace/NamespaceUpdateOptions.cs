// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Core.Options;
using Microsoft.Mcp.Core.Options;

namespace Azure.Mcp.Tools.EventHubs.Options.Namespace;

public sealed class NamespaceUpdateOptions : ISubscriptionOption
{
    [Option(Description = EventHubsOptionDescriptions.Namespace)]
    public required string Namespace { get; set; }

    [Option(Description = "The Azure region where the namespace is located (e.g., 'eastus', 'westus2').")]
    public string? Location { get; set; }

    [Option(Description = "The SKU name for the namespace. Valid values: 'Basic', 'Standard', 'Premium'.")]
    public string? SkuName { get; set; }

    [Option(Description = "The SKU tier for the namespace. Valid values: 'Basic', 'Standard', 'Premium'.")]
    public string? SkuTier { get; set; }

    [Option(Description = "The SKU capacity (throughput units) for the namespace. Valid range depends on the SKU.")]
    public int? SkuCapacity { get; set; }

    [Option(Description = "Enable or disable auto-inflate for the namespace.")]
    public bool? IsAutoInflateEnabled { get; set; }

    [Option(Description = "The maximum throughput units when auto-inflate is enabled.")]
    public int? MaximumThroughputUnits { get; set; }

    [Option(Description = "Enable or disable Kafka for the namespace.")]
    public bool? KafkaEnabled { get; set; }

    [Option(Description = "Enable or disable zone redundancy for the namespace.")]
    public bool? ZoneRedundant { get; set; }

    [Option(Description = "Tags for the namespace in JSON format (e.g., '{\"key1\":\"value1\",\"key2\":\"value2\"}').")]
    public string? Tags { get; set; }

    [Option(Description = OptionDescriptions.ResourceGroup)]
    public required string ResourceGroup { get; set; }

    [Option(Description = OptionDescriptions.Subscription)]
    public string? Subscription { get; set; }

    [Option(Description = OptionDescriptions.Tenant)]
    public string? Tenant { get; set; }

    [OptionContainer(Prefix = "retry")]
    public RetryPolicyOptions? RetryPolicy { get; set; }
}
