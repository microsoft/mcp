// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Core.Options;
using Microsoft.Mcp.Core.Options;

namespace Azure.Mcp.Tools.Monitor.Options.Metrics;

/// <summary>
/// Base options for all metrics commands
/// </summary>
public abstract class BaseMetricsOptions : ISubscriptionOption
{
    /// <summary>
    /// The resource type (optional, e.g., 'Microsoft.Storage/storageAccounts')
    /// </summary>
    [Option(Description = "The Azure resource type (e.g., 'Microsoft.Storage/storageAccounts', 'Microsoft.Compute/virtualMachines'). If not specified, will attempt to infer from resource name.")]
    public string? ResourceType { get; set; }

    /// <summary>
    /// The resource name (required)
    /// </summary>
    [Option(Description = "The name of the Azure resource to query metrics for.")]
    public required string Resource { get; set; }

    [Option(Description = OptionDescriptions.Tenant)]
    public string? Tenant { get; set; }

    [Option(Description = OptionDescriptions.Subscription)]
    public string? Subscription { get; set; }

    [Option(Description = OptionDescriptions.ResourceGroup)]
    public string? ResourceGroup { get; set; }

    [OptionContainer(Prefix = "retry")]
    public RetryPolicyOptions? RetryPolicy { get; set; }
}
