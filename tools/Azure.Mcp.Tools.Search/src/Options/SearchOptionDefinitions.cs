// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Azure.Mcp.Tools.Search.Options;

public static class SearchOptionDefinitions
{
    public const string ServiceName = "service";
    public const string IndexName = "index";
    public const string QueryName = "query";

    public static readonly Option<string> Service = new(
        $"--{ServiceName}",
        "The name of the Azure AI Search service (e.g., my-search-service)."
    )
    {
        IsRequired = true
    };

    public static readonly Option<string> Index = new(
        $"--{IndexName}",
        "The name of the search index within the Azure AI Search service."
    )
    {
        IsRequired = true
    };

    public static readonly Option<string> Query = new(
        $"--{QueryName}",
        "The search query to execute against the Azure AI Search index."
    )
    {
        IsRequired = true
    };
}
