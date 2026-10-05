// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Azure.Mcp.Tools.InfraIq.Models.Request;

public sealed class InfraIqRecommendVmSkuHourlyCost
{
    public double? Amount { get; set; }

    public string? CurrencyCode { get; set; }
}
