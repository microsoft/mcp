// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json.Serialization;

namespace Azure.Mcp.Tools.Kusto.Options;

public class QueryOptions : BaseDatabaseOptions
{
    [JsonPropertyName(KustoOptionDefinitions.QueryText)]
    public string? Query { get; set; }
}
