// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Core.Options;

namespace Azure.Mcp.Tools.Advisor.Options.Recommendation;

public interface IRecommendationScopeOptions : ISubscriptionOption
{
    string[]? Subscriptions { get; set; }

    string? ServiceGroup { get; set; }

    string[]? ServiceGroups { get; set; }
}
