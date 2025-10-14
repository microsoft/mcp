// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json.Serialization;

namespace Azure.Mcp.Tools.Search.Options.Knowledge;

public class KnowledgeBaseGetOptions : BaseSearchOptions
{
    [JsonPropertyName(SearchOptionDefinitions.KnowledgeBaseName)]
    public string? KnowledgeBase { get; set; }
}
