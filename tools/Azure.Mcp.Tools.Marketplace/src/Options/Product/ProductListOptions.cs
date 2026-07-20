// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Core.Options;
using Microsoft.Mcp.Core.Options;

namespace Azure.Mcp.Tools.Marketplace.Options.Product;

public sealed class ProductListOptions : ISubscriptionOption
{
    [Option(Description = MarketplaceOptionDescriptions.Language)]
    public string? Language { get; set; }

    [Option(Description = "Search for products using a short general term (up to 25 characters)")]
    public string? Search { get; set; }

    [Option(Description = "OData filter expression to filter results based on ProductSummary properties (e.g., \"displayName eq 'Azure'\").")]
    public string? Filter { get; set; }

    [Option(Name = "orderby", Description = "OData orderby expression to sort results by ProductSummary fields (e.g., \"displayName asc\" or \"popularity desc\").")]
    public string? OrderBy { get; set; }

    [Option(Description = "OData select expression to choose specific ProductSummary fields to return (e.g., \"displayName,publisherDisplayName,uniqueProductId\").")]
    public string? Select { get; set; }

    [Option(Description = "Pagination cursor to retrieve the next page of results. Use the NextPageLink value from a previous response.")]
    public string? NextCursor { get; set; }

    [Option(Description = "OData expand expression to include related data in the response (e.g., \"plans\" to include plan details).")]
    public string? Expand { get; set; }

    [Option(Description = OptionDescriptions.Subscription)]
    public string? Subscription { get; set; }

    [Option(Description = OptionDescriptions.Tenant)]
    public string? Tenant { get; set; }

    [OptionContainer(Prefix = "retry")]
    public RetryPolicyOptions? RetryPolicy { get; set; }
}
