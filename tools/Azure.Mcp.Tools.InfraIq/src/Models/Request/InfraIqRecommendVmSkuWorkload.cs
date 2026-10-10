// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Azure.Mcp.Tools.InfraIq.Models.Request;

public sealed class InfraIqRecommendVmSkuWorkload
{
    public int? MaxConcurrentRequestsPerReplica { get; set; }

    public InfraIqRecommendVmSkuTokenBudget? TokenBudget { get; set; }
}
