// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Core.Options;
using Azure.Mcp.Tools.Marketplace.Models;

namespace Azure.Mcp.Tools.Marketplace.Services;

public interface IMarketplaceService
{
    Task<ProductDetails> GetProduct(
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
        RetryPolicyOptions? retryPolicy = null);

    Task<ProductListResponseWithNextCursor> ListProducts(
        string subscription,
        string? language = null,
        string? search = null,
        string? filter = null,
        string? orderBy = null,
        string? select = null,
        string? nextCursor = null,
        string? expand = null,
        string? tenant = null,
        RetryPolicyOptions? retryPolicy = null);
}
