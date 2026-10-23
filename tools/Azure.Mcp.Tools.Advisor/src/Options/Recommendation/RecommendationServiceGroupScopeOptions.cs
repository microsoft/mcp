// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Core.Options;
using Microsoft.Mcp.Core.Options;

namespace Azure.Mcp.Tools.Advisor.Options.Recommendation;

public sealed class RecommendationServiceGroupScopeOptions
{
    [Option(Description = OptionDescriptions.Tenant)]
    public string? Tenant { get; set; }
}
