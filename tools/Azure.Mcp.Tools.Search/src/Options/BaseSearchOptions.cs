// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json.Serialization;
using Azure.Mcp.Core.Options;

namespace Azure.Mcp.Tools.Search.Options;

public class BaseSearchOptions : GlobalOptions
{
    [JsonPropertyName(SearchOptionDefinitions.ServiceName)]
    public string? Service { get; set; }
}
