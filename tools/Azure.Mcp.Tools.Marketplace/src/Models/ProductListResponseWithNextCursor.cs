// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Azure.Mcp.Tools.Marketplace.Models;

public class ProductListResponseWithNextCursor
{
    public List<ProductSummary> Items { get; set; } = new();
    public string? NextCursor { get; set; }
}
