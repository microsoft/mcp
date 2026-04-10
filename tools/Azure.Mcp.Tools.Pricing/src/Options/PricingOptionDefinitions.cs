// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.CommandLine;

namespace Azure.Mcp.Tools.Pricing.Options;

/// <summary>
/// Option definitions for Pricing commands.
/// </summary>
public static class PricingOptionDefinitions
{
    public const string SkuName = "sku";
    public const string ServiceName = "service";
    public const string RegionName = "region";
    public const string ServiceFamilyName = "service-family";
    public const string PriceTypeName = "price-type";
    public const string CurrencyName = "currency";
    public const string IncludeSavingsPlanName = "include-savings-plan";
    public const string FilterName = "filter";

    /// <summary>
    /// ARM SKU name (e.g., Standard_D4s_v5).
    /// </summary>
    public static readonly Option<string> Sku = new($"--{SkuName}")
    {
        Description = "ARM SKU name (e.g., Standard_D4s_v5, Standard_E64-16ds_v4)",
        Required = false
    };

    /// <summary>
    /// Azure service name (e.g., Virtual Machines, Storage).
    /// </summary>
    public static readonly Option<string> Service = new($"--{ServiceName}")
    {
        Description = "Azure service name (e.g., Virtual Machines, Storage, SQL Database)",
        Required = false
    };

    /// <summary>
    /// Azure region (e.g., eastus, westeurope).
    /// </summary>
    public static readonly Option<string> Region = new($"--{RegionName}")
    {
        Description = "Azure region (e.g., eastus, westeurope, westus2)",
        Required = false
    };

    /// <summary>
    /// Service family (e.g., Compute, Storage, Databases).
    /// </summary>
    public static readonly Option<string> ServiceFamily = new($"--{ServiceFamilyName}")
    {
        Description = "Service family (e.g., Compute, Storage, Databases, Networking)",
        Required = false
    };

    /// <summary>
    /// Price type (Consumption, Reservation, DevTestConsumption).
    /// </summary>
    public static readonly Option<string> PriceType = new($"--{PriceTypeName}")
    {
        Description = "Price type filter (Consumption, Reservation, DevTestConsumption)",
        Required = false
    };

    /// <summary>
    /// Currency code for pricing.
    /// </summary>
    public static readonly Option<string> Currency = new($"--{CurrencyName}")
    {
        Description = "Currency code for pricing (e.g., USD, EUR). Default is USD.",
        Required = false
    };

    /// <summary>
    /// Include savings plan pricing (uses preview API).
    /// </summary>
    public static readonly Option<bool> IncludeSavingsPlan = new($"--{IncludeSavingsPlanName}")
    {
        Description = "Include savings plan pricing information (uses preview API version)",
        Required = false
    };

    /// <summary>
    /// Raw OData filter for advanced queries.
    /// </summary>
    public static readonly Option<string> Filter = new($"--{FilterName}")
    {
        Description = "Raw OData filter expression for advanced queries (e.g., \"meterId eq 'abc-123'\")",
        Required = false
    };
}
