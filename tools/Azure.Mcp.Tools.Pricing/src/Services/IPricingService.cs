// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Tools.Pricing.Models;

namespace Azure.Mcp.Tools.Pricing.Services;

/// <summary>
/// Service interface for Azure Retail Pricing operations.
/// </summary>
public interface IPricingService
{
    /// <summary>
    /// Gets Azure retail prices based on the specified filters.
    /// </summary>
    /// <param name="sku">ARM SKU name (e.g., Standard_D4s_v5).</param>
    /// <param name="service">Azure service name (e.g., Virtual Machines).</param>
    /// <param name="region">Azure region (e.g., eastus).</param>
    /// <param name="serviceFamily">Service family (e.g., Compute).</param>
    /// <param name="priceType">Price type (Consumption, Reservation, DevTestConsumption).</param>
    /// <param name="currency">Currency code (e.g., USD). Default is USD.</param>
    /// <param name="includeSavingsPlan">Whether to include savings plan pricing.</param>
    /// <param name="filter">Raw OData filter for advanced queries.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>List of retail price items matching the criteria.</returns>
    Task<List<PriceItem>> GetPricesAsync(
        string? sku = null,
        string? service = null,
        string? region = null,
        string? serviceFamily = null,
        string? priceType = null,
        string? currency = null,
        bool includeSavingsPlan = false,
        string? filter = null,
        CancellationToken cancellationToken = default);
}
