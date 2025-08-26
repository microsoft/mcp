// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Azure.Mcp.Tools.Search.Options;

public static class SearchOptionDefinitions
{
    public const string ServiceName = "service";
    public const string IndexName = "index";
    public const string QueryName = "query";

    public static readonly Option<string> Service = new(
        $"--{ServiceName}"
    )
    {
        Description = "The name of the Azure AI Search service (e.g., my-search-service).",
        Required = true
    };

    public static readonly Option<string> Index = new(
        $"--{IndexName}"
    )
    {
        Description = "The name of the search index within the Azure AI Search service.",
        Required = true
    };

    public static readonly Option<string> Query = new(
        $"--{QueryName}"
    )
    {
        Description = "The search query to execute against the Azure AI Search index.",
        Required = true
    };
}
