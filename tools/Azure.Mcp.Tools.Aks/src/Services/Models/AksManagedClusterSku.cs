// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Azure.Mcp.Tools.Aks.Services.Models;

/// <summary> The SKU of a Managed Cluster. </summary>
internal sealed class AksManagedClusterSku
{
    /// <summary> The name of a managed cluster SKU. </summary>
    public string? Name { get; set; }
    /// <summary> If not specified, the default is 'Free'. See [AKS Pricing Tier](https://learn.microsoft.com/azure/aks/free-standard-pricing-tiers) for more details. </summary>
    public string? Tier { get; set; }
}
