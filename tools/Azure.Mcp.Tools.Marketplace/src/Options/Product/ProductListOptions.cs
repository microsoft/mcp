// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Core.Options;

namespace Azure.Mcp.Tools.Marketplace.Options.Product;

public class ProductListOptions : SubscriptionOptions
{
    public string? Language { get; set; }
    public string? Search { get; set; }
    public string? Filter { get; set; }
    public string? OrderBy { get; set; }
    public string? Select { get; set; }
    public string? NextCursor { get; set; }
    public string? Expand { get; set; }
}
