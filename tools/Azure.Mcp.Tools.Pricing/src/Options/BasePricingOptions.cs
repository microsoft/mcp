// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Core.Options;

namespace Azure.Mcp.Tools.Pricing.Options;

/// <summary>
/// Base options for all Pricing commands.
/// </summary>
public class BasePricingOptions : GlobalOptions
{
    /// <summary>
    /// Currency code for pricing (e.g., "USD", "EUR"). Default is USD.
    /// </summary>
    public string? Currency { get; set; }
}
