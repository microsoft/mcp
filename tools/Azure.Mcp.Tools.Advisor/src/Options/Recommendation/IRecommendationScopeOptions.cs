// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Core.Options;

namespace Azure.Mcp.Tools.Advisor.Options.Recommendation;

public interface IRecommendationScopeOptions : ISubscriptionOption
{
    string? ServiceGroup { get; set; }
}
