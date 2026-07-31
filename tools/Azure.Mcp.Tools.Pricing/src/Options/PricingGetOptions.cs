// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.Mcp.Core.Options;

namespace Azure.Mcp.Tools.Pricing.Options;

/// <summary>
/// Options for the pricing get command.
/// </summary>
public sealed class PricingGetOptions
{
    /// <summary>
    /// Currency code for pricing (e.g., "USD", "EUR"). Default is USD.
    /// </summary>
    [Option(Description = "Currency code for pricing (e.g., USD, EUR). Default is USD.")]
    public string? Currency { get; set; }

    /// <summary>
    /// ARM SKU name (e.g., Standard_D4s_v5).
    /// </summary>
    [Option(Description = "ARM SKU name (e.g., Standard_D4s_v5, Standard_E64-16ds_v4)")]
    public string? Sku { get; set; }

    /// <summary>
    /// Azure service name (e.g., Virtual Machines).
    /// </summary>
    [Option(Description = "Azure service name (e.g., Virtual Machines, Storage, SQL Database)")]
    public string? Service { get; set; }

    /// <summary>
    /// Azure region (e.g., eastus, westeurope).
    /// </summary>
    [Option(Description = "Azure region (e.g., eastus, westeurope, westus2)")]
    public string? Region { get; set; }

    /// <summary>
    /// Service family (e.g., Compute, Storage).
    /// </summary>
    [Option(Description = "Service family (e.g., Compute, Storage, Databases, Networking)")]
    public string? ServiceFamily { get; set; }

    /// <summary>
    /// Price type (Consumption, Reservation, DevTestConsumption).
    /// </summary>
    [Option(Description = "Price type filter (Consumption, Reservation, DevTestConsumption)")]
    public string? PriceType { get; set; }

    /// <summary>
    /// Include savings plan pricing (uses preview API).
    /// </summary>
    [Option(Description = "Include savings plan pricing information (uses preview API version)")]
    public bool IncludeSavingsPlan { get; set; }

    /// <summary>
    /// Raw OData filter for advanced queries.
    /// </summary>
    [Option(Description = "Raw OData filter expression for advanced queries (e.g., \"meterId eq 'abc-123'\")")]
    public string? Filter { get; set; }
}
