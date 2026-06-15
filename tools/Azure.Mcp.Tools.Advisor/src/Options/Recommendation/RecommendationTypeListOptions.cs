// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.Mcp.Core.Options;

namespace Azure.Mcp.Tools.Advisor.Options.Recommendation;

public class RecommendationTypeListOptions : GlobalOptions
{
    public string? ResourceType { get; set; }
    public string? Impact { get; set; }
    public string? Category { get; set; }
}
