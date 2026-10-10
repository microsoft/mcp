// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Azure.Mcp.Tools.InfraIq.Models.Request;

public sealed class InfraIqRecommendVmSkuSubscriptionOptions
{
    public bool IncludeQuota { get; set; }

    public bool IncludePlacement { get; set; }

    public bool IncludePricing { get; set; }
}
