// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Azure.Mcp.Tools.Insights.Options;

public static class InsightsOptionDefinitions
{
    public const string QueryName = "query";

    public static readonly Option<string> Query = new($"--{QueryName}")
    {
        Description = "Optional free-form description of what the insights will be used for " +
                      "(e.g. 'To build an internal finance web app with a relational database backend'). " +
                      "When provided, insights are tailored toward this scenario; when omitted, generic " +
                      "patterns are returned.",
        Required = false,
    };
}
