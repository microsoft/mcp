// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Azure.Mcp.Tools.Pricing.Options;

/// <summary>
/// Options for the pricing get command.
/// </summary>
public class PricingGetOptions : BasePricingOptions
{
    /// <summary>
    /// ARM SKU name (e.g., Standard_D4s_v5).
    /// </summary>
    public string? Sku { get; set; }

    /// <summary>
    /// Azure service name (e.g., Virtual Machines).
    /// </summary>
    public string? Service { get; set; }

    /// <summary>
    /// Azure region (e.g., eastus, westeurope).
    /// </summary>
    public string? Region { get; set; }

    /// <summary>
    /// Service family (e.g., Compute, Storage).
    /// </summary>
    public string? ServiceFamily { get; set; }

    /// <summary>
    /// Price type (Consumption, Reservation, DevTestConsumption).
    /// </summary>
    public string? PriceType { get; set; }

    /// <summary>
    /// Include savings plan pricing (uses preview API).
    /// </summary>
    public bool IncludeSavingsPlan { get; set; }

    /// <summary>
    /// Raw OData filter for advanced queries.
    /// </summary>
    public string? Filter { get; set; }
}
