// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json.Serialization;

namespace Azure.Mcp.Tools.Search.Options.Knowledge;

public class KnowledgeSourceGetOptions : BaseSearchOptions
{
    [JsonPropertyName(SearchOptionDefinitions.KnowledgeSourceName)]
    public string? KnowledgeSource { get; set; }
}
