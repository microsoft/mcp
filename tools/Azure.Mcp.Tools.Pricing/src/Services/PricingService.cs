// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Core.Services.Azure.Authentication;
using Azure.Mcp.Tools.Pricing.Models;
using AzureRetailPrices;

namespace Azure.Mcp.Tools.Pricing.Services;

/// <summary>
/// Service implementation for Azure Retail Pricing operations.
/// </summary>
public class PricingService(IAzureCloudConfiguration cloudConfiguration) : IPricingService
{
    private const int MaxResults = 5000;

    /// <inheritdoc/>
    public async Task<List<PriceItem>> GetPricesAsync(
        string? sku = null,
        string? service = null,
        string? region = null,
        string? serviceFamily = null,
        string? priceType = null,
        string? currency = null,
        bool includeSavingsPlan = false,
        string? filter = null,
        CancellationToken cancellationToken = default)
    {
        // Build OData filter from parameters
        var oDataFilter = BuildODataFilter(sku, service, region, serviceFamily, priceType, filter);

        // Validate at least one filter is provided
        if (string.IsNullOrEmpty(oDataFilter))
        {
            throw new ArgumentException(
                "At least one filter is required. Specify --sku, --service, --region, --service-family, --price-type, or --filter.");
        }

        // Create client - use preview API version for savings plan, otherwise default
        // The Azure Retail Prices API works best without specifying api-version for regular queries
        // Savings plan pricing data requires the 2023-01-01-preview API version
        var serviceVersion = includeSavingsPlan
            ? AzureRetailPricesClientOptions.ServiceVersion.V2023_01_01_Preview
            : AzureRetailPricesClientOptions.ServiceVersion.Default;

        var clientOptions = new AzureRetailPricesClientOptions(serviceVersion);

        var client = new AzureRetailPricesClient(
            GetPricingEndpoint(),
            clientOptions);

        var retailPrices = client.GetRetailPricesClient();

        // Get prices with auto-pagination up to MaxResults
        var results = new List<PriceItem>();
        var currencyCode = currency ?? "USD";

        await foreach (var item in retailPrices.GetPricesAsync(
            currencyCode: currencyCode,
            filter: oDataFilter,
            meterRegion: null,
            skip: null,
            cancellationToken: cancellationToken))
        {
            results.Add(MapToPriceItem(item));

            if (results.Count >= MaxResults)
            {
                break;
            }
        }

        return results;
    }

    private static string? BuildODataFilter(
        string? sku,
        string? service,
        string? region,
        string? serviceFamily,
        string? priceType,
        string? rawFilter)
    {
        var filters = new List<string>();

        if (!string.IsNullOrEmpty(sku))
        {
            filters.Add($"armSkuName eq '{EscapeODataValue(sku)}'");
        }

        if (!string.IsNullOrEmpty(service))
        {
            filters.Add($"serviceName eq '{EscapeODataValue(service)}'");
        }

        if (!string.IsNullOrEmpty(region))
        {
            filters.Add($"armRegionName eq '{EscapeODataValue(region)}'");
        }

        if (!string.IsNullOrEmpty(serviceFamily))
        {
            filters.Add($"serviceFamily eq '{EscapeODataValue(serviceFamily)}'");
        }

        if (!string.IsNullOrEmpty(priceType))
        {
            filters.Add($"priceType eq '{EscapeODataValue(priceType)}'");
        }

        // Append raw filter if provided
        if (!string.IsNullOrEmpty(rawFilter))
        {
            filters.Add(rawFilter);
        }

        return filters.Count > 0 ? string.Join(" and ", filters) : null;
    }

    private static string EscapeODataValue(string value)
    {
        // Escape single quotes in OData filter values
        return value.Replace("'", "''");
    }

    private Uri GetPricingEndpoint()
    {
        return cloudConfiguration.CloudType switch
        {
            AzureCloudConfiguration.AzureCloud.AzurePublicCloud => new Uri("https://prices.azure.com"),
            AzureCloudConfiguration.AzureCloud.AzureChinaCloud => new Uri("https://prices.azure.cn"),
            AzureCloudConfiguration.AzureCloud.AzureUSGovernmentCloud => new Uri("https://prices.azure.us"),
            _ => new Uri("https://prices.azure.com")
        };
    }

    private static PriceItem MapToPriceItem(RetailPriceItem item)
    {
        var priceItem = new PriceItem
        {
            ArmSkuName = item.ArmSkuName,
            SkuName = item.SkuName,
            ProductName = item.ProductName,
            ServiceName = item.ServiceName,
            ServiceFamily = item.ServiceFamily.ToString(),
            Region = item.ArmRegionName,
            Location = item.Location,
            RetailPrice = item.RetailPrice,
            UnitPrice = item.UnitPrice,
            CurrencyCode = item.CurrencyCode,
            UnitOfMeasure = item.UnitOfMeasure,
            PriceType = item.Type.ToString(),
            ReservationTerm = item.ReservationTerm?.ToString(),
            EffectiveStartDate = item.EffectiveStartDate,
            MeterName = item.MeterName,
            MeterId = item.MeterId,
            IsPrimaryMeterRegion = item.IsPrimaryMeterRegion
        };

        // Map savings plan if available
        if (item.SavingsPlan?.Count > 0)
        {
            priceItem.SavingsPlan = item.SavingsPlan
                .Select(sp => new SavingsPlanPrice
                {
                    Term = sp.Term,
                    UnitPrice = sp.UnitPrice,
                    RetailPrice = sp.RetailPrice
                })
                .ToList();
        }

        return priceItem;
    }
}
