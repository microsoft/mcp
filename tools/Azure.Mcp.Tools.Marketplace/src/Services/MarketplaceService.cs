// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json.Serialization.Metadata;
using Azure.Core;
using Azure.Core.Pipeline;
using Azure.Mcp.Core.Options;
using Azure.Mcp.Core.Services.Azure;
using Azure.Mcp.Core.Services.Azure.Tenant;
using Azure.Mcp.Tools.Marketplace.Commands;
using Azure.Mcp.Tools.Marketplace.Models;

namespace Azure.Mcp.Tools.Marketplace.Services;

public class MarketplaceService(ITenantService tenantService)
    : BaseAzureService(tenantService), IMarketplaceService
{
    private const int TokenExpirationBuffer = 300;
    private const string ManagementApiBaseUrl = "https://management.azure.com";
    private const string ApiVersion = "2023-01-01-preview";

    private string? _cachedAccessToken;
    private DateTimeOffset _tokenExpiryTime;

    /// <summary>
    /// Retrieves a single private product (offer) for a given subscription.
    /// </summary>
    /// <param name="productId">The ID of the product to retrieve.</param>
    /// <param name="subscription">The Azure subscription ID.</param>
    /// <param name="includeStopSoldPlans">Include stop-sold or hidden plans.</param>
    /// <param name="language">Product language (default: en).</param>
    /// <param name="market">Product market (default: US).</param>
    /// <param name="lookupOfferInTenantLevel">Check against tenant private audience.</param>
    /// <param name="planId">Filter by plan ID.</param>
    /// <param name="skuId">Filter by SKU ID.</param>
    /// <param name="includeServiceInstructionTemplates">Include service instruction templates.</param>
    /// <param name="pricingAudience">Pricing audience.</param>
    /// <param name="tenant">Optional. The Azure tenant ID for authentication.</param>
    /// <param name="retryPolicy">Optional. Policy parameters for retrying failed requests.</param>
    /// <returns>A JSON node containing the product information.</returns>
    /// <exception cref="ArgumentException">Thrown when required parameters are missing or invalid.</exception>
    /// <exception cref="Exception">Thrown when parsing the product response fails.</exception>
    public async Task<ProductDetails> GetProduct(
        string productId,
        string subscription,
        bool? includeStopSoldPlans = null,
        string? language = null,
        string? market = null,
        bool? lookupOfferInTenantLevel = null,
        string? planId = null,
        string? skuId = null,
        bool? includeServiceInstructionTemplates = null,
        string? pricingAudience = null,
        string? tenant = null,
        RetryPolicyOptions? retryPolicy = null)
    {
        ValidateRequiredParameters(
            (nameof(productId), productId),
            (nameof(subscription), subscription));

        string productUrl = BuildProductUrl(subscription, productId, includeStopSoldPlans, language, market,
            lookupOfferInTenantLevel, planId, skuId, includeServiceInstructionTemplates);

        return await GetMarketplaceSingleProductResponseAsync(productUrl, tenant, retryPolicy);
    }

    /// <summary>
    /// Retrieves private products (offers) that a subscription has access to in the Azure Marketplace.
    /// </summary>
    /// <param name="subscription">The Azure subscription ID.</param>
    /// <param name="language">Product language (default: en).</param>
    /// <param name="search">Search by display name, publisher name, or keywords.</param>
    /// <param name="filter">OData filter expression.</param>
    /// <param name="orderBy">OData orderby expression.</param>
    /// <param name="select">OData select expression. Renamed from 'select' to avoid reserved word.</param>
    /// <param name="nextCursor">Pagination cursor.</param>
    /// <param name="expand">OData expand expression to include related data.</param>
    /// <param name="tenant">Optional. The Azure tenant ID for authentication.</param>
    /// <param name="retryPolicy">Optional. Policy parameters for retrying failed requests.</param>
    /// <returns>A list of ProductSummary objects containing the marketplace products.</returns>
    /// <exception cref="ArgumentException">Thrown when required parameters are missing or invalid.</exception>
    /// <exception cref="Exception">Thrown when parsing the products response fails.</exception>
    public async Task<ProductListResponseWithNextCursor> ListProducts(
        string subscription,
        string? language = null,
        string? search = null,
        string? filter = null,
        string? orderBy = null,
        string? select = null,
        string? nextCursor = null,
        string? expand = null,
        string? tenant = null,
        RetryPolicyOptions? retryPolicy = null)
    {
        ValidateRequiredParameters((nameof(subscription), subscription));

        string productsUrl = BuildProductsListUrl(subscription, language, search, filter, orderBy, select, nextCursor, expand);

        return await GetMarketplaceListProductsResponseAsync(productsUrl, tenant, retryPolicy);
    }

    private static string BuildProductsListUrl(
        string subscription,
        string? language,
        string? search,
        string? filter,
        string? orderBy,
        string? select,
        string? nextCursor,
        string? expand)
    {
        var queryParams = new List<string>
        {
            $"api-version={ApiVersion}"
        };

        if (!string.IsNullOrEmpty(language))
            queryParams.Add($"language={Uri.EscapeDataString(language)}");

        if (!string.IsNullOrEmpty(search))
            queryParams.Add($"$search={Uri.EscapeDataString(search)}");

        // Add OData query parameters
        if (!string.IsNullOrEmpty(filter))
            queryParams.Add($"$filter={Uri.EscapeDataString(filter)}");

        if (!string.IsNullOrEmpty(orderBy))
            queryParams.Add($"$orderby={Uri.EscapeDataString(orderBy)}");

        if (!string.IsNullOrEmpty(select))
            queryParams.Add($"$select={Uri.EscapeDataString(select)}");

        if (!string.IsNullOrEmpty(expand))
            queryParams.Add($"$expand={Uri.EscapeDataString(expand)}");

        if (!string.IsNullOrEmpty(nextCursor))
            queryParams.Add($"$skiptoken={Uri.EscapeDataString(nextCursor)}");

        queryParams.Add("storefront=any"); // include all storefronts
        string queryString = string.Join("&", queryParams);
        return $"{ManagementApiBaseUrl}/subscriptions/{subscription}/providers/Microsoft.Marketplace/products?{queryString}";
    }

    private async Task<ProductListResponseWithNextCursor> GetMarketplaceListProductsResponseAsync(string url, string? tenant, RetryPolicyOptions? retryPolicy = null)
    {
        var productsListResponse = await ExecuteMarketplaceRequestAsync(
            url, MarketplaceJsonContext.Default.ProductsListResponse, retryPolicy, tenant);

        var result = new ProductListResponseWithNextCursor
        {
            Items = productsListResponse?.Items ?? [],
            NextCursor = ExtractSkipTokenFromUrl(productsListResponse?.NextPageLink)
        };
        return result;
    }


    private static string BuildProductUrl(
        string subscription,
        string productId,
        bool? includeStopSoldPlans,
        string? language,
        string? market,
        bool? lookupOfferInTenantLevel,
        string? planId,
        string? skuId,
        bool? includeServiceInstructionTemplates)
    {
        var queryParams = new List<string>
        {
            $"api-version={ApiVersion}"
        };

        if (includeStopSoldPlans.HasValue)
            queryParams.Add($"includeStopSoldPlans={includeStopSoldPlans.Value.ToString().ToLower()}");

        if (!string.IsNullOrEmpty(language))
            queryParams.Add($"language={Uri.EscapeDataString(language)}");

        if (!string.IsNullOrEmpty(market))
            queryParams.Add($"market={Uri.EscapeDataString(market)}");

        if (lookupOfferInTenantLevel.HasValue)
            queryParams.Add($"lookupOfferInTenantLevel={lookupOfferInTenantLevel.Value.ToString().ToLower()}");

        if (!string.IsNullOrEmpty(planId))
            queryParams.Add($"planId={Uri.EscapeDataString(planId)}");

        if (!string.IsNullOrEmpty(skuId))
            queryParams.Add($"skuId={Uri.EscapeDataString(skuId)}");

        if (includeServiceInstructionTemplates.HasValue)
            queryParams.Add($"includeServiceInstructionTemplates={includeServiceInstructionTemplates.Value.ToString().ToLower()}");

        string queryString = string.Join("&", queryParams);
        return $"{ManagementApiBaseUrl}/subscriptions/{subscription}/providers/Microsoft.Marketplace/products/{productId}?{queryString}";
    }

    private async Task<ProductDetails> GetMarketplaceSingleProductResponseAsync(string url, string? tenant, RetryPolicyOptions? retryPolicy = null)
    {
        var productDetails = await ExecuteMarketplaceRequestAsync(
            url,
            MarketplaceJsonContext.Default.ProductDetails,
            retryPolicy,
            tenant
        );
        return productDetails ?? throw new JsonException("Failed to deserialize marketplace response to ProductDetails.");
    }

    private async Task<string> GetAccessTokenAsync(string? tenant = null)
    {
        if (_cachedAccessToken != null && DateTimeOffset.UtcNow < _tokenExpiryTime)
        {
            return _cachedAccessToken;
        }

        AccessToken accessToken = await GetEntraIdAccessTokenAsync(ManagementApiBaseUrl, tenant);
        _cachedAccessToken = accessToken.Token;
        _tokenExpiryTime = accessToken.ExpiresOn.AddSeconds(-TokenExpirationBuffer);

        return _cachedAccessToken;
    }

    private async Task<AccessToken> GetEntraIdAccessTokenAsync(string resource, string? tenant = null)
    {
        var tokenRequestContext = new TokenRequestContext([$"{resource}/.default"]);
        var tokenCredential = await GetCredential(tenant);
        return await tokenCredential
            .GetTokenAsync(tokenRequestContext, CancellationToken.None);
    }

    private async Task<T> ExecuteMarketplaceRequestAsync<T>(
        string url,
        JsonTypeInfo<T> jsonTypeInfo,
        RetryPolicyOptions? retryPolicy = null,
        string? tenant = null
    )
    {
        // Use Azure Core pipeline approach consistently
        var clientOptions = AddDefaultPolicies(new MarketplaceClientOptions());

        // Configure retry policy if provided
        if (retryPolicy != null)
        {
            clientOptions.Retry.MaxRetries = retryPolicy.MaxRetries;
            clientOptions.Retry.Mode = retryPolicy.Mode;
            clientOptions.Retry.Delay = TimeSpan.FromSeconds(retryPolicy.DelaySeconds);
            clientOptions.Retry.MaxDelay = TimeSpan.FromSeconds(retryPolicy.MaxDelaySeconds);
            clientOptions.Retry.NetworkTimeout = TimeSpan.FromSeconds(retryPolicy.NetworkTimeoutSeconds);
        }

        // Create pipeline
        var pipeline = HttpPipelineBuilder.Build(clientOptions);

        string accessToken = await GetAccessTokenAsync(tenant);
        ValidateRequiredParameters((nameof(accessToken), accessToken));

        var request = pipeline.CreateRequest();
        request.Method = RequestMethod.Get;
        request.Uri.Reset(new Uri(url));
        request.Headers.Add("Authorization", $"Bearer {accessToken}");

        var response = await pipeline.SendRequestAsync(request, CancellationToken.None);

        if (!response.IsError)
        {
            var result = JsonSerializer.Deserialize(response.Content.ToStream(), jsonTypeInfo);
            return result ?? throw new JsonException("Marketplace response deserialized to null.");
        }

        throw new HttpRequestException($"Request failed with status {response.Status}: {response.ReasonPhrase}");
    }

    private static string? ExtractSkipTokenFromUrl(string? url)
    {
        if (url == null)
        {
            return null;
        }

        try
        {
            var uri = new Uri(url);
            var query = System.Web.HttpUtility.ParseQueryString(uri.Query);
            return query["$skiptoken"];
        }
        catch
        {
            // If we can't parse the URL or extract the token, return null
            return null;
        }
    }
}
