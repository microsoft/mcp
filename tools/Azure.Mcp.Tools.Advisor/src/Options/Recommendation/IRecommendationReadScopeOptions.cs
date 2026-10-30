// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Azure.Mcp.Tools.Advisor.Options.Recommendation;

public interface IRecommendationReadScopeOptions : IRecommendationScopeOptions
{
    string[]? Subscriptions { get; set; }
}
