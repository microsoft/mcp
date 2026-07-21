// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Core.Options;
using Microsoft.Mcp.Core.Options;

namespace Azure.Mcp.Tools.Marketplace.Options.Product;

public sealed class ProductGetOptions : ISubscriptionOption
{
    [Option(Description = "The ID of the marketplace product to retrieve. This is the unique identifier for the product in the Azure Marketplace.")]
    public required string ProductId { get; set; }

    [Option(Description = "Include stop-sold or hidden plans in the response.")]
    public bool IncludeStopSoldPlans { get; set; }

    [Option(Description = MarketplaceOptionDescriptions.Language)]
    public string? Language { get; set; }

    [Option(Description = "Check against tenant private audience when retrieving the product.")]
    public bool LookupOfferInTenantLevel { get; set; }

    [Option(Description = "Filter results by a specific plan ID.")]
    public string? PlanId { get; set; }

    [Option(Description = "Filter results by a specific SKU ID.")]
    public string? SkuId { get; set; }

    [Option(Description = "Include service instruction templates in the response.")]
    public bool IncludeServiceInstructionTemplates { get; set; }

    [Option(Description = OptionDescriptions.Subscription)]
    public string? Subscription { get; set; }

    [Option(Description = OptionDescriptions.Tenant)]
    public string? Tenant { get; set; }

    [OptionContainer(Prefix = "retry")]
    public RetryPolicyOptions? RetryPolicy { get; set; }
}
