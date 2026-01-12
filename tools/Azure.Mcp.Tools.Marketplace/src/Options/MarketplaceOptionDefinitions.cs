// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Azure.Mcp.Tools.Marketplace.Options;

public static class MarketplaceOptionDefinitions
{
    public const string ProductIdName = "product-id";
    public const string IncludeStopSoldPlansName = "include-stop-sold-plans";
    public const string LanguageName = "language";
    public const string MarketName = "market";
    public const string LookupOfferInTenantLevelName = "lookup-offer-in-tenant-level";
    public const string PlanIdName = "plan-id";
    public const string SkuIdName = "sku-id";
    public const string IncludeServiceInstructionTemplatesName = "include-service-instruction-templates";
    public const string PricingAudienceName = "pricing-audience";
    public const string SearchName = "search";
    public const string FilterName = "filter";
    public const string OrderByName = "orderby";
    public const string SelectName = "select";
    public const string ExpandName = "expand";
    public const string NextCursorName = "next-cursor";

    public static readonly Option<string> ProductId = new($"--{ProductIdName}")
    {
        Description = "The ID of the marketplace product to retrieve. This is the unique identifier for the product in the Azure Marketplace.",
        Required = true
    };

    public static readonly Option<bool> IncludeStopSoldPlans = new($"--{IncludeStopSoldPlansName}")
    {
        Description = "Include stop-sold or hidden plans in the response.",
        Required = false
    };

    public static readonly Option<string> Language = new($"--{LanguageName}")
    {
        Description = "Product language code (e.g., 'en' for English, 'fr' for French).",
        Required = false
    };

    public static readonly Option<string> Market = new($"--{MarketName}")
    {
        Description = "Product market code (e.g., 'US' for United States, 'UK' for United Kingdom).",
        Required = false
    };

    public static readonly Option<bool> LookupOfferInTenantLevel = new($"--{LookupOfferInTenantLevelName}")
    {
        Description = "Check against tenant private audience when retrieving the product.",
        Required = false
    };

    public static readonly Option<string> PlanId = new($"--{PlanIdName}")
    {
        Description = "Filter results by a specific plan ID.",
        Required = false
    };

    public static readonly Option<string> SkuId = new($"--{SkuIdName}")
    {
        Description = "Filter results by a specific SKU ID.",
        Required = false
    };

    public static readonly Option<bool> IncludeServiceInstructionTemplates = new($"--{IncludeServiceInstructionTemplatesName}")
    {
        Description = "Include service instruction templates in the response.",
        Required = false
    };

    public static readonly Option<string> PricingAudience = new($"--{PricingAudienceName}")
    {
        Description = "Pricing audience for the request header.",
        Required = false
    };

    public static readonly Option<string> Search = new($"--{SearchName}")
    {
        Description = "Search for products using a short general term (up to 25 characters)",
        Required = false
    };

    public static readonly Option<string> Filter = new($"--{FilterName}")
    {
        Description = "OData filter expression to filter results based on ProductSummary properties (e.g., \"displayName eq 'Azure'\").",
        Required = false
    };

    public static readonly Option<string> OrderBy = new($"--{OrderByName}")
    {
        Description = "OData orderby expression to sort results by ProductSummary fields (e.g., \"displayName asc\" or \"popularity desc\").",
        Required = false
    };

    public static readonly Option<string> Select = new($"--{SelectName}")
    {
        Description = "OData select expression to choose specific ProductSummary fields to return (e.g., \"displayName,publisherDisplayName,uniqueProductId\").",
        Required = false
    };

    public static readonly Option<string> NextCursor = new($"--{NextCursorName}")
    {
        Description = "Pagination cursor to retrieve the next page of results. Use the NextPageLink value from a previous response.",
        Required = false
    };

    public static readonly Option<string> Expand = new($"--{ExpandName}")
    {
        Description = "OData expand expression to include related data in the response (e.g., \"plans\" to include plan details).",
        Required = false
    };
}
