// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Azure.Mcp.Tools.Advisor.Options.Recommendation;

public static class RecommendationTypeListOptionDefinitions
{
    public const string FilterText = "filter";

    public static readonly Option<string> Filter = new(
        $"--{FilterText}"
    )
    {
        Description = "Optional metadata dimension to filter to. When omitted, supported values from all dimensions are returned (which can be a very large combined set). Valid values: 'recommendationType' (all recommendation type IDs), 'category' (recommendation categories like Cost, Security, Performance), 'impact' (High/Medium/Low), 'resourceType' (Azure resource types Advisor analyzes).",
        Required = false
    };
}
