// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Azure.Mcp.Tools.Pricing.Models;

/// <summary>
/// Represents an Azure retail price item.
/// </summary>
public sealed class PriceItem
{
    /// <summary>
    /// ARM SKU name registered in Azure (e.g., Standard_D4s_v5).
    /// </summary>
    public string? ArmSkuName { get; set; }

    /// <summary>
    /// SKU display name.
    /// </summary>
    public string? SkuName { get; set; }

    /// <summary>
    /// Product name.
    /// </summary>
    public string? ProductName { get; set; }

    /// <summary>
    /// Azure service name (e.g., Virtual Machines).
    /// </summary>
    public string? ServiceName { get; set; }

    /// <summary>
    /// Service family (e.g., Compute, Storage).
    /// </summary>
    public string? ServiceFamily { get; set; }

    /// <summary>
    /// Azure region (e.g., eastus, westeurope).
    /// </summary>
    public string? Region { get; set; }

    /// <summary>
    /// Display location name (e.g., East US).
    /// </summary>
    public string? Location { get; set; }

    /// <summary>
    /// Retail price without discount.
    /// </summary>
    public double RetailPrice { get; set; }

    /// <summary>
    /// Unit price (same as retail price).
    /// </summary>
    public double UnitPrice { get; set; }

    /// <summary>
    /// Currency code (e.g., USD).
    /// </summary>
    public string? CurrencyCode { get; set; }

    /// <summary>
    /// Unit of measure (e.g., "1 Hour", "1 GB").
    /// </summary>
    public string? UnitOfMeasure { get; set; }

    /// <summary>
    /// Price type (Consumption, Reservation, DevTestConsumption).
    /// </summary>
    public string? PriceType { get; set; }

    /// <summary>
    /// Reservation term (1 Year, 3 Years) - only for reservation prices.
    /// </summary>
    public string? ReservationTerm { get; set; }

    /// <summary>
    /// Effective start date for this price.
    /// </summary>
    public DateTimeOffset? EffectiveStartDate { get; set; }

    /// <summary>
    /// Meter name.
    /// </summary>
    public string? MeterName { get; set; }

    /// <summary>
    /// Unique meter identifier.
    /// </summary>
    public string? MeterId { get; set; }

    /// <summary>
    /// Whether this is the primary meter region.
    /// </summary>
    public bool IsPrimaryMeterRegion { get; set; }

    /// <summary>
    /// Savings plan pricing information (only with preview API).
    /// </summary>
    public List<SavingsPlanPrice>? SavingsPlan { get; set; }
}

/// <summary>
/// Savings plan price information.
/// </summary>
public sealed class SavingsPlanPrice
{
    /// <summary>
    /// Term length (1 Year, 3 Years).
    /// </summary>
    public string? Term { get; set; }

    /// <summary>
    /// Unit price for savings plan.
    /// </summary>
    public double UnitPrice { get; set; }

    /// <summary>
    /// Retail price for savings plan.
    /// </summary>
    public double RetailPrice { get; set; }
}
