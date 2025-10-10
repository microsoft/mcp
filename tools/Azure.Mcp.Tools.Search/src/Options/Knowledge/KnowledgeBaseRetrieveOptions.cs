// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json.Serialization;

namespace Azure.Mcp.Tools.Search.Options.Knowledge;

public class KnowledgeBaseRetrieveOptions : BaseSearchOptions
{
    [JsonPropertyName(SearchOptionDefinitions.KnowledgeBaseName)]
    public string? KnowledgeBase { get; set; }

    [JsonPropertyName(SearchOptionDefinitions.QueryName)]
    public string? Query { get; set; }

    [JsonPropertyName(SearchOptionDefinitions.MessagesName)]
    public string[] Messages { get; set; } = [];
}
