// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.Mcp.Core.Options;

namespace Azure.Mcp.Tools.Advisor.Options.ServiceRetirement;

public sealed class ServiceRetirementInsightsOptions
{
    [Option(Description = "Azure subscription ID or name to use for subscription-scoped questions. Matches properties.insightResourceId or properties.insightResourceName case-insensitively. Omit for a service-group-scoped or fleet-wide question.")]
    public string? Subscription { get; set; }

    [Option(Description = "Service group ID or name to use for service-group-scoped questions. Matches properties.insightResourceId or properties.insightResourceName case-insensitively. Omit for a subscription-scoped or fleet-wide question.")]
    public string? ServiceGroup { get; set; }

    [Option(Description = "Maximum number of insight rows to return, from 1 through 100.", DefaultValue = 100)]
    public int Top { get; set; } = 100;
}
